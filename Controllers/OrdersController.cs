
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using OrderInventory.Api.DTOs;
using OrderInventory.Api.Services;
using OrderInventory.Api.Models;

namespace OrderInventory.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return BadRequest(new
            {
                message = "The Idempotency-Key header is required."
            });
        }

        try
        {
            // Temporary customer ID until JWT authentication is implemented.
            var order = await _orderService.CreateOrderAsync(
    1, request, idempotencyKey);

            return Created($"/api/orders/{order.Id}", order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
     [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromQuery] OrderStatus status)
    {
        try
        {
            var order = await _orderService.UpdateStatusAsync(id, status);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new
            {
                message = "The order changed concurrently. Retry."
            });
        }
    }
}
