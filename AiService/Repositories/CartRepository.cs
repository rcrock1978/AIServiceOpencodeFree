using AiService.Models;
using Dapper;
using Npgsql;

namespace AiService.Repositories;

public class CartRepository : ICartRepository
{
    private readonly string _connectionString;

    public CartRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Cart?> GetCartAsync(Guid userId)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var cart = await connection.QueryFirstOrDefaultAsync<Cart>(
            "SELECT id, user_id AS UserId, created_at AS CreatedAt, updated_at AS UpdatedAt FROM cart WHERE user_id = @userId",
            new { userId });

        if (cart == null) return null;

        var items = await connection.QueryAsync<CartItem>(
            @"SELECT id, cart_id AS CartId, product_id AS ProductId, quantity, created_at AS CreatedAt
              FROM cart_items WHERE cart_id = @cartId",
            new { cartId = cart.Id });

        cart.Items = items.ToList();

        var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToArray();
        if (productIds.Length != 0)
        {
            var products = await connection.QueryAsync<Product>(
                "SELECT id, name, description, price, brand, type, image_url AS ImageUrl, rating, stock, created_at AS CreatedAt, updated_at AS UpdatedAt FROM products WHERE id = ANY(@ids)",
                new { ids = productIds });

            var productDict = products.ToDictionary(p => p.Id);
            foreach (var item in cart.Items)
            {
                if (productDict.TryGetValue(item.ProductId, out var product))
                {
                    item.Product = product;
                }
            }
        }

        return cart;
    }

    public async Task<Cart> CreateCartAsync(Guid userId)
    {
        const string sql = """
            INSERT INTO cart (id, user_id, created_at, updated_at)
            VALUES (gen_random_uuid(), @userId, NOW(), NOW())
            RETURNING id, user_id AS UserId, created_at AS CreatedAt, updated_at AS UpdatedAt
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstAsync<Cart>(sql, new { userId });
    }

    public async Task AddItemAsync(Guid cartId, Guid productId, int quantity)
    {
        const string sql = """
            WITH upsert AS (
                UPDATE cart_items
                SET quantity = quantity + @quantity
                WHERE cart_id = @cartId AND product_id = @productId
                RETURNING *
            )
            INSERT INTO cart_items (id, cart_id, product_id, quantity, created_at)
            SELECT gen_random_uuid(), @cartId, @productId, @quantity, NOW()
            WHERE NOT EXISTS (SELECT 1 FROM upsert)
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new { cartId, productId, quantity });
    }

    public async Task RemoveItemAsync(Guid cartId, Guid productId)
    {
        const string sql = "DELETE FROM cart_items WHERE cart_id = @cartId AND product_id = @productId";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new { cartId, productId });
    }

    public async Task UpdateItemQuantityAsync(Guid cartId, Guid productId, int quantity)
    {
        const string sql = """
            UPDATE cart_items
            SET quantity = @quantity
            WHERE cart_id = @cartId AND product_id = @productId
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new { cartId, productId, quantity });
    }

    public async Task ClearCartAsync(Guid cartId)
    {
        const string sql = "DELETE FROM cart_items WHERE cart_id = @cartId";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new { cartId });
    }
}
