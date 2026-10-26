using OrderInventory.Api.Services;
namespace OrderInventory.Api.Models;

public class Product
{
    public int Id { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
