
using Microsoft.EntityFrameworkCore;
using OrderInventory.Api.Models;
using System.Security.Cryptography;

namespace OrderInventory.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!await db.Users.AnyAsync())
        {
            db.Users.AddRange(
                CreateUser("Admin User", "admin@example.com", "Admin"),
                CreateUser("Customer One", "customer1@example.com", "Customer"),
                CreateUser("Customer Two", "customer2@example.com", "Customer"),
                CreateUser("Customer Three", "customer3@example.com", "Customer")
            );
        }

        if (!await db.Products.AnyAsync())
        {
            var products = Enumerable.Range(1, 15).Select(i => new Product
            {
                Sku = $"SKU-{i:000}",
                Name = $"Product {i}",
                Price = 100m * i,
                StockQuantity = 10,
                IsActive = true
            });

            db.Products.AddRange(products);
        }

        await db.SaveChangesAsync();
    }

    private static User CreateUser(string name, string email, string role)
    {
         var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            "ChangeMe123!",
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            32);

        return new User
        {
            FullName = name,
            Email = email,
            Role = role,
            PasswordHash = $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}"
        };
    }
}
