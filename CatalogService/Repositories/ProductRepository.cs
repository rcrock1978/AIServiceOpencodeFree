using Dapper;
using CatalogService.Models;
using Npgsql;

namespace CatalogService.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PgVector")
            ?? throw new InvalidOperationException("Connection string 'PgVector' not found.");
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    private const string ProductColumns = "id, name, description, price, brand, type, image_url, rating, stock, created_at, updated_at";

    public async Task<IEnumerable<Product>> GetAllAsync(string? brand, string? type, string? search, string? sortBy, int page, int pageSize)
    {
        using var conn = CreateConnection();

        var sql = $"SELECT {ProductColumns} FROM products WHERE 1=1";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(brand))
        {
            sql += " AND brand = @Brand";
            parameters.Add("Brand", brand);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            sql += " AND type = @Type";
            parameters.Add("Type", type);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " AND (name ILIKE @Search OR description ILIKE @Search)";
            parameters.Add("Search", $"%{search}%");
        }

        sql += sortBy?.ToLower() switch
        {
            "name" => " ORDER BY name ASC",
            "price" => " ORDER BY price ASC",
            "pricedesc" => " ORDER BY price DESC",
            "rating" => " ORDER BY rating DESC",
            _ => " ORDER BY created_at DESC"
        };

        var offset = (page - 1) * pageSize;
        sql += " LIMIT @PageSize OFFSET @Offset";
        parameters.Add("PageSize", pageSize);
        parameters.Add("Offset", offset);

        return await conn.QueryAsync<Product>(sql, parameters);
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Product>(
            $"SELECT {ProductColumns} FROM products WHERE id = @Id",
            new { Id = id });
    }

    public async Task<IEnumerable<string>> GetBrandsAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<string>(
            "SELECT DISTINCT brand FROM products ORDER BY brand");
    }

    public async Task<IEnumerable<string>> GetTypesAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<string>(
            "SELECT DISTINCT type FROM products ORDER BY type");
    }

    public async Task<int> GetTotalCountAsync(string? brand, string? type, string? search)
    {
        using var conn = CreateConnection();

        var sql = "SELECT COUNT(*) FROM products WHERE 1=1";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(brand))
        {
            sql += " AND brand = @Brand";
            parameters.Add("Brand", brand);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            sql += " AND type = @Type";
            parameters.Add("Type", type);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " AND (name ILIKE @Search OR description ILIKE @Search)";
            parameters.Add("Search", $"%{search}%");
        }

        return await conn.ExecuteScalarAsync<int>(sql, parameters);
    }
}
