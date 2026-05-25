using AiService.Providers;
using AiService.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AiService.Tests.Services;

public class SearchServiceTests
{
    private readonly SearchService _sut;

    public SearchServiceTests()
    {
        var embedder = new Mock<IEmbeddingProvider>();
        embedder.Setup(e => e.GenerateEmbeddingAsync(It.IsAny<string>()))
            .ReturnsAsync(new float[] { 0.1f, 0.2f, 0.3f });
        embedder.Setup(e => e.Dimensions).Returns(768);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PgVector"] = "Host=localhost;Port=5433;Database=aivector;Username=aiuser;Password=aiPass123"
            })
            .Build();

        _sut = new SearchService(embedder.Object, config);
    }

    [Fact]
    public void Constructor_Throws_WhenConnectionStringMissing()
    {
        var embedder = new Mock<IEmbeddingProvider>();
        var config = new ConfigurationBuilder().Build();

        Action act = () => new SearchService(embedder.Object, config);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Connection string 'PgVector' not found.");
    }
}
