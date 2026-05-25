namespace AiService.Configuration;

public class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string EmbeddingDeployment { get; set; } = "text-embedding-3-small";
    public string ChatDeployment { get; set; } = "gpt-4o-mini";
    public string ApiVersion { get; set; } = "2024-02-01";
    public int Dimensions { get; set; } = 1536;
}
