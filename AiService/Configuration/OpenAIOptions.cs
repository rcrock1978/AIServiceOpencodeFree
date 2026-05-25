namespace AiService.Configuration;

public class OpenAIOptions
{
    public const string SectionName = "OpenAI";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public string ApiKey { get; set; } = string.Empty;
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
    public string ChatModel { get; set; } = "gpt-4o-mini";
    public int Dimensions { get; set; } = 1536;
}
