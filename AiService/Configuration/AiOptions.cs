namespace AiService.Configuration;

public class AiOptions
{
    public const string SectionName = "AI";
    public string Provider { get; set; } = "Ollama";
}
