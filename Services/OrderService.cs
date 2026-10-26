
using Microsoft.EntityFrameworkCore;
using OrderInventory.Api.Data;
using OrderInventory.Api.DTOs;
using OrderInventory.Api.Models;

namespace OrderInventory.Api.Services;

public class OrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Order> CreateOrderAsync(
        int customerId,
        CreateOrderRequest request,
        string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException("Idempotency key is required.");

        if (request.Items.Count == 0)
            throw new ArgumentException("An order must contain at least one item.");

        if (request.Items.Any(i => i.Quantity <= 0))
            throw new ArgumentException("Quantity must be greater than zero.");

        if (request.Items.GroupBy(i => i.ProductId).Any(g => g.Count() > 1))
            throw new ArgumentException("Each product can appear only once.");

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        var existing = await _db.IdempotencyRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.CustomerId == customerId &&
                x.Key == idempotencyKey);

        if (existing != null)
        {
            var previousOrder = await _db.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == existing.OrderId);

            if (previousOrder == null)
                throw new InvalidOperationException(
                    "The original order for this key could not be found.");

            await transaction.CommitAsync();
            return previousOrder;
        }

        var productIds = request.Items.Select(i => i.ProductId).ToList();

        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        if (products.Count != productIds.Count)
            throw new ArgumentException(
                "One or more products were not found or are inactive.");

        foreach (var item in request.Items)
        {
            var product = products.Single(p => p.Id == item.ProductId);

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for {product.Name}.");

            product.StockQuantity -= item.Quantity;
        }

        var orderItems = request.Items.Select(item =>
        {
            var product = products.Single(p => p.Id == item.ProductId);

            return new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            };
        }).ToList();

        var subTotal = orderItems.Sum(i => i.UnitPrice * i.Quantity);

        var previousOrderCount = await _db.Orders
            .CountAsync(o => o.CustomerId == customerId);

        decimal discountRate = subTotal > 10000m ? 0.05m : 0m;

        if ((previousOrderCount + 1) % 5 == 0)
            discountRate += 0.10m;

        var discount = Math.Round(
            subTotal * discountRate, 2, MidpointRounding.AwayFromZero);

        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            SubTotal = subTotal,
            Discount = discount,
            Total = subTotal - discount,
            CreatedAt = DateTime.UtcNow,
            Items = orderItems
        };

        _db.Orders.Add(order);

        try
        {
            await _db.SaveChangesAsync();

            _db.IdempotencyRecords.Add(new IdempotencyRecord
            {
                CustomerId = customerId,
                Key = idempotencyKey,
                OrderId = order.Id
            });

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return order;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                "Stock changed while placing the order. Please retry.");
        }
    }
    
public async Task<Order> UpdateStatusAsync(int orderId, OrderStatus newStatus)
{
    await using var transaction = await _db.Database.BeginTransactionAsync();

    var order = await _db.Orders
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == orderId);

    if (order == null)
        throw new KeyNotFoundException("Order not found.");

    bool allowed =
        (order.Status == OrderStatus.Pending &&
            (newStatus == OrderStatus.Confirmed ||
             newStatus == OrderStatus.Cancelled))
        || (order.Status == OrderStatus.Confirmed &&
            (newStatus == OrderStatus.Shipped ||
             newStatus == OrderStatus.Cancelled));

    if (!allowed)
        throw new ArgumentException("Invalid order status transition.");

    if (newStatus == OrderStatus.Cancelled)
    {
        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();

        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        foreach (var item in order.Items)
        {
            var product = products.Single(p => p.Id == item.ProductId);
            product.StockQuantity += item.Quantity;
        }
    }

    order.Status = newStatus;

    await _db.SaveChangesAsync();
    await transaction.CommitAsync();

    return order;
}

}
