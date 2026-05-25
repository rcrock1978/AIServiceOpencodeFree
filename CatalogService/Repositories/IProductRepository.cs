using CatalogService.Models;

namespace CatalogService.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync(string? brand, string? type, string? search, string? sortBy, int page, int pageSize);
    Task<Product?> GetByIdAsync(Guid id);
    Task<IEnumerable<string>> GetBrandsAsync();
    Task<IEnumerable<string>> GetTypesAsync();
    Task<int> GetTotalCountAsync(string? brand, string? type, string? search);
}
