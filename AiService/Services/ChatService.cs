using AiService.Contracts;
using AiService.Models;
using AiService.Providers;
using AiService.Repositories;

namespace AiService.Services;

public class ChatService : IChatService
{
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IChatProvider _chatProvider;
    private readonly ISearchService _searchService;
    private readonly IConversationRepository _conversationRepository;
    private readonly IWebSearchProvider _webSearchProvider;

    private const string SystemPrompt = """
        You are a helpful AI shopping assistant for an e-commerce store. Your role is to help customers find products, answer questions about products, and provide recommendations based on available product data.

        Guidelines:
        - Answer based only on the product data provided below
        - Be concise, friendly, and helpful
        - If no relevant products are found, say so honestly
        - When recommending products, format them clearly with name, price, and brief description
        - Do not make up product information that is not in the provided data
        """;

    private static readonly HashSet<string> FollowUpIndicators =
    [
        "more", "another", "others", "similar", "cheaper", "expensive", "different",
        "yes", "no", "thanks", "ok", "sure", "tell", "show", "what", "which"
    ];

    public ChatService(
        IEmbeddingProvider embeddingProvider,
        IChatProvider chatProvider,
        ISearchService searchService,
        IWebSearchProvider webSearchProvider,
        IConversationRepository conversationRepository)
    {
        _embeddingProvider = embeddingProvider;
        _chatProvider = chatProvider;
        _searchService = searchService;
        _webSearchProvider = webSearchProvider;
        _conversationRepository = conversationRepository;
    }

    public async Task<ChatResponse> AskAsync(string message)
    {
        var products = await GetRelevantProductsAsync(message, 5);
        var ragContext = BuildRagContext(products);

        string finalMessage;
        if (products.Any())
        {
            finalMessage = $"""
                User question: {message}

                Available products:
                {ragContext}

                Please answer the user's question based on the products listed above.
                """;
        }
        else
        {
            var webFallback = await _webSearchProvider.SearchAsync(message);
            finalMessage = $"""
                User question: {message}

                {webFallback}

                Please respond letting the user know no specific products were found.
                """;
        }

        var reply = await _chatProvider.GenerateChatResponseAsync(SystemPrompt, finalMessage);

        var conversation = await _conversationRepository.CreateConversationAsync(userId: null, title: "Chat");

        await _conversationRepository.AddMessageAsync(conversation.Id, "user", message);
        await _conversationRepository.AddMessageAsync(conversation.Id, "assistant", reply);

        return new ChatResponse(reply, products.ToList(), conversation.Id);
    }

    public async Task<ChatResponse> AskWithContextAsync(string message, Guid conversationId)
    {
        var conversation = await _conversationRepository.GetConversationAsync(conversationId);
        if (conversation == null)
            return await AskAsync(message);

        var recentMessages = (await _conversationRepository.GetMessagesAsync(conversationId, 6)).ToList();

        var products = await GetRelevantProductsAsync(message, 5);
        var ragContext = BuildRagContext(products);

        var isFollowUp = IsFollowUpMessage(message);
        var historyMessages = recentMessages.ToList();

        string userPrompt;
        if (products.Any())
        {
            userPrompt = $"""
                User question: {message}

                Available products:
                {ragContext}

                Please answer based on the products listed above.
                """;
        }
        else
        {
            var webFallback = await _webSearchProvider.SearchAsync(message);
            userPrompt = $"""
                User question: {message}

                {webFallback}

                Please respond letting the user know no specific products were found.
                """;
        }

        if (isFollowUp && historyMessages.Count > 0)
        {
            var lastAssistantMsg = historyMessages.LastOrDefault(m => m.Role == "assistant");
            if (lastAssistantMsg != null)
            {
                userPrompt = $"""
                    Previous context: {lastAssistantMsg.Content}

                    Follow-up question: {message}
                    """;
            }
        }

        string conversationContext;
        if (historyMessages.Count > 0)
        {
            conversationContext = string.Join("\n", historyMessages.Select(m =>
                $"{(m.Role == "user" ? "User" : "Assistant")}: {m.Content}"));
            userPrompt = $"""
                Conversation history:
                {conversationContext}

                {userPrompt}
                """;
        }

        await _conversationRepository.AddMessageAsync(conversationId, "user", message);

        var reply = await _chatProvider.GenerateChatResponseAsync(SystemPrompt, userPrompt, historyMessages);

        await _conversationRepository.AddMessageAsync(conversationId, "assistant", reply);

        return new ChatResponse(reply, products.ToList(), conversationId);
    }

    private async Task<List<Product>> GetRelevantProductsAsync(string message, int limit)
    {
        var (results, _) = await _searchService.SmartSearchAsync(message, limit);
        return results.ToList();
    }

    private static string BuildRagContext(List<Product> products)
    {
        if (products.Count == 0)
            return string.Empty;

        return string.Join("\n\n", products.Select((p, i) =>
            $"Product {i + 1}: {p.Name}\n" +
            $"Price: ${p.Price:F2}\n" +
            $"Brand: {p.Brand ?? "N/A"}\n" +
            $"Type: {p.Type ?? "N/A"}\n" +
            $"Rating: {p.Rating}/5\n" +
            $"Description: {p.Description ?? "No description available"}\n" +
            $"In Stock: {(p.Stock > 0 ? "Yes" : "No")}"
        ));
    }

    private static bool IsFollowUpMessage(string message)
    {
        var words = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 6)
            return false;

        return words.All(w => FollowUpIndicators.Contains(w.ToLowerInvariant()));
    }
}
