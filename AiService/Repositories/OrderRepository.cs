using AiService.Models;
using Dapper;
using Npgsql;

namespace AiService.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Order> CreateOrderAsync(
        Guid userId, decimal totalAmount, string shippingAddress, List<CartItem> items)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        var orderSql = """
            INSERT INTO orders (id, user_id, total_amount, status, shipping_address, created_at, updated_at)
            VALUES (gen_random_uuid(), @userId, @totalAmount, 'pending', @shippingAddress, NOW(), NOW())
            RETURNING id, user_id AS UserId, total_amount AS TotalAmount, status, shipping_address AS ShippingAddress, created_at AS CreatedAt, updated_at AS UpdatedAt
            """;

        var order = await connection.QueryFirstAsync<Order>(
            orderSql, new { userId, totalAmount, shippingAddress }, transaction);

        var itemSql = """
            INSERT INTO order_items (id, order_id, product_id, quantity, unit_price, created_at)
            VALUES (gen_random_uuid(), @orderId, @productId, @quantity, @unitPrice, NOW())
            """;

        foreach (var cartItem in items)
        {
            var unitPrice = cartItem.Product?.Price ?? 0m;
            await connection.ExecuteAsync(
                itemSql,
                new { orderId = order.Id, cartItem.ProductId, cartItem.Quantity, unitPrice },
                transaction);
        }

        await transaction.CommitAsync();
        return order;
    }

    public async Task<Order?> GetOrderAsync(Guid orderId)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var order = await connection.QueryFirstOrDefaultAsync<Order>(
            "SELECT id, user_id AS UserId, total_amount AS TotalAmount, status, shipping_address AS ShippingAddress, created_at AS CreatedAt, updated_at AS UpdatedAt FROM orders WHERE id = @orderId",
            new { orderId });

        if (order == null) return null;

        var items = await connection.QueryAsync<OrderItem, Product, OrderItem>(
            """
            SELECT oi.id, oi.order_id AS OrderId, oi.product_id AS ProductId, oi.quantity, oi.unit_price AS UnitPrice, oi.created_at AS CreatedAt,
                   p.id, p.name, p.description, p.price, p.brand, p.type, p.image_url AS ImageUrl, p.rating, p.stock, p.created_at AS CreatedAt, p.updated_at AS UpdatedAt
            FROM order_items oi
            INNER JOIN products p ON p.id = oi.product_id
            WHERE oi.order_id = @orderId
            """,
            (item, product) =>
            {
                item.Product = product;
                return item;
            },
            new { orderId },
            splitOn: "id");

        order.Items = items.ToList();
        return order;
    }

    public async Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var orderDict = new Dictionary<Guid, Order>();

        var orderItems = await connection.QueryAsync<Order, OrderItem, Product, Order>(
            """
            SELECT o.id, o.user_id AS UserId, o.total_amount AS TotalAmount, o.status, o.shipping_address AS ShippingAddress, o.created_at AS CreatedAt, o.updated_at AS UpdatedAt,
                   oi.id, oi.order_id AS OrderId, oi.product_id AS ProductId, oi.quantity, oi.unit_price AS UnitPrice, oi.created_at AS CreatedAt,
                   p.id, p.name, p.description, p.price, p.brand, p.type, p.image_url AS ImageUrl, p.rating, p.stock, p.created_at AS CreatedAt, p.updated_at AS UpdatedAt
            FROM orders o
            INNER JOIN order_items oi ON oi.order_id = o.id
            INNER JOIN products p ON p.id = oi.product_id
            WHERE o.user_id = @userId
            ORDER BY o.created_at DESC
            """,
            (order, orderItem, product) =>
            {
                if (!orderDict.TryGetValue(order.Id, out var existing))
                {
                    existing = order;
                    existing.Items = [];
                    orderDict.Add(order.Id, existing);
                }
                orderItem.Product = product;
                existing.Items.Add(orderItem);
                return existing;
            },
            new { userId },
            splitOn: "id,id");

        return orderDict.Values;
    }

    public async Task UpdateOrderStatusAsync(Guid orderId, string status)
    {
        const string sql = """
            UPDATE orders
            SET status = @status, updated_at = NOW()
            WHERE id = @orderId
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new { orderId, status });
    }
}
