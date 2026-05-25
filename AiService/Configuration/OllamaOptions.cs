namespace AiService.Configuration;

public class OllamaOptions
{
    public const string SectionName = "Ollama";
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string EmbeddingModel { get; set; } = "nomic-embed-text";
    public string ChatModel { get; set; } = "tinyllama";
    public int Dimensions { get; set; } = 768;
    public int MaxTokens { get; set; } = 150;
    public double Temperature { get; set; } = 0.7;
}
