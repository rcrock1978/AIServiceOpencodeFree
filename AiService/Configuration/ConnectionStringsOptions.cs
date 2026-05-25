namespace AiService.Configuration;

public class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";
    public string PgVector { get; set; } = string.Empty;
}
