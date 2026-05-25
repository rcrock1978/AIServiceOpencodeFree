namespace AiService.Services;

public class WebSearchProvider : IWebSearchProvider
{
    public Task<string> SearchAsync(string query)
    {
        return Task.FromResult("Web search is not yet configured. Please check back later.");
    }
}
