using AiService.Configuration;
using AiService.Providers.AzureOpenAI;
using AiService.Providers.Ollama;
using AiService.Providers.OpenAI;

namespace AiService.Providers;

public static class ProviderFactory
{
    public static void AddAIProviders(this IServiceCollection services, IConfiguration configuration)
    {
        var aiOptions = configuration.GetSection("AI").Get<AiOptions>();
        var provider = aiOptions?.Provider ?? "Ollama";

        switch (provider)
        {
            case "AzureOpenAI":
                services.AddSingleton<IEmbeddingProvider, AzureOpenAIEmbeddingProvider>();
                services.AddSingleton<IChatProvider, AzureOpenAIChatProvider>();
                break;
            case "OpenAI":
                services.AddSingleton<IEmbeddingProvider, OpenAIEmbeddingProvider>();
                services.AddSingleton<IChatProvider, OpenAIChatProvider>();
                break;
            default:
                services.AddSingleton<IEmbeddingProvider, OllamaEmbeddingProvider>();
                services.AddSingleton<IChatProvider, OllamaChatProvider>();
                break;
        }
    }
}
