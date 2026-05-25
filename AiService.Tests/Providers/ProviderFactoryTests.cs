using AiService.Configuration;
using AiService.Providers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiService.Tests.Providers;

public class ProviderFactoryTests
{
    private static ServiceCollection BuildServices(string provider)
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{AiOptions.SectionName}:{nameof(AiOptions.Provider)}"] = provider
            })
            .Build();

        services.AddSingleton<IConfiguration>(config);
        services.AddHttpClient();
        services.AddAIProviders(config);
        return services;
    }

    [Fact]
    public void AddAIProviders_DefaultsToOllama_WhenNoProviderSpecified()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(config);
        services.AddHttpClient();
        services.AddAIProviders(config);

        var sp = services.BuildServiceProvider();
        var embedder = sp.GetRequiredService<IEmbeddingProvider>();
        var chat = sp.GetRequiredService<IChatProvider>();

        embedder.ProviderName.Should().Be("Ollama");
        chat.ProviderName.Should().Be("Ollama");
    }

    [Fact]
    public void AddAIProviders_RegistersOllama_WhenConfigured()
    {
        var services = BuildServices("Ollama");
        var sp = services.BuildServiceProvider();

        var embedder = sp.GetRequiredService<IEmbeddingProvider>();
        embedder.ProviderName.Should().Be("Ollama");
    }

    [Fact]
    public void AddAIProviders_RegistersAzureOpenAI_WhenConfigured()
    {
        var services = BuildServices("AzureOpenAI");
        var sp = services.BuildServiceProvider();

        var embedder = sp.GetRequiredService<IEmbeddingProvider>();
        embedder.ProviderName.Should().Be("AzureOpenAI");
    }

    [Fact]
    public void AddAIProviders_RegistersOpenAI_WhenConfigured()
    {
        var services = BuildServices("OpenAI");
        var sp = services.BuildServiceProvider();

        var embedder = sp.GetRequiredService<IEmbeddingProvider>();
        embedder.ProviderName.Should().Be("OpenAI");
    }
}
