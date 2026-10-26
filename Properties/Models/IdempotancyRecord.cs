
namespace OrderInventory.Api.Models;

public class IdempotencyRecord
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Key { get; set; } = string.Empty;

    public int OrderId { get; set; }
}
