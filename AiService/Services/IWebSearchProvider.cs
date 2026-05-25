namespace AiService.Services;

public interface IWebSearchProvider
{
    Task<string> SearchAsync(string query);
}
