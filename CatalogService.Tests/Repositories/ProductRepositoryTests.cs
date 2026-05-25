using CatalogService.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CatalogService.Tests.Repositories;

public class ProductRepositoryTests
{
    [Fact]
    public void Constructor_Throws_WhenConnectionStringMissing()
    {
        var config = new ConfigurationBuilder().Build();

        Action act = () => new ProductRepository(config);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Connection string 'PgVector' not found.");
    }

    [Fact]
    public void Constructor_Succeeds_WithValidConfig()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PgVector"] = "Host=localhost;Port=5433;Database=aivector;Username=aiuser;Password=aiPass123"
            })
            .Build();

        var repo = new ProductRepository(config);

        repo.Should().NotBeNull();
    }
}
