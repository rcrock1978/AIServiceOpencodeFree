using AiService.Contracts;
using AiService.Models;
using AiService.Providers;
using AiService.Repositories;
using AiService.Services;
using FluentAssertions;
using Moq;

namespace AiService.Tests.Services;

public class ChatServiceTests
{
    private readonly Mock<IEmbeddingProvider> _embedder;
    private readonly Mock<IChatProvider> _chatProvider;
    private readonly Mock<ISearchService> _searchService;
    private readonly Mock<IWebSearchProvider> _webSearch;
    private readonly Mock<IConversationRepository> _conversationRepo;
    private readonly ChatService _sut;

    public ChatServiceTests()
    {
        _embedder = new Mock<IEmbeddingProvider>();
        _chatProvider = new Mock<IChatProvider>();
        _searchService = new Mock<ISearchService>();
        _webSearch = new Mock<IWebSearchProvider>();
        _conversationRepo = new Mock<IConversationRepository>();

        _chatProvider.Setup(p => p.GenerateChatResponseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<Message>?>()))
            .ReturnsAsync("Here are some products.");

        _conversationRepo.Setup(r => r.CreateConversationAsync(It.IsAny<Guid?>(), It.IsAny<string>()))
            .ReturnsAsync(new Conversation { Id = Guid.NewGuid(), Title = "Chat" });

        _conversationRepo.Setup(r => r.AddMessageAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Message());

        _sut = new ChatService(
            _embedder.Object,
            _chatProvider.Object,
            _searchService.Object,
            _webSearch.Object,
            _conversationRepo.Object);
    }

    [Fact]
    public async Task AskAsync_ReturnsReplyWithProducts_WhenProductsFound()
    {
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Test Product", Price = 29.99m }
        };
        _searchService.Setup(s => s.SmartSearchAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((products, true));

        var result = await _sut.AskAsync("show me products");

        result.Reply.Should().NotBeNullOrEmpty();
        result.Products.Should().HaveCount(1);
        result.Products![0].Name.Should().Be("Test Product");
        result.ConversationId.Should().NotBeNull();
    }

    [Fact]
    public async Task AskAsync_ReturnsWebFallback_WhenNoProductsFound()
    {
        _searchService.Setup(s => s.SmartSearchAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((Enumerable.Empty<Product>(), false));
        _webSearch.Setup(w => w.SearchAsync(It.IsAny<string>()))
            .ReturnsAsync("Web search fallback content");

        var result = await _sut.AskAsync("something obscure");

        result.Reply.Should().NotBeNullOrEmpty();
        result.Products.Should().BeEmpty();
    }

    [Fact]
    public async Task AskWithContextAsync_CreatesNewConversation_WhenNotFound()
    {
        var conversationId = Guid.NewGuid();
        _conversationRepo.Setup(r => r.GetConversationAsync(conversationId))
            .ReturnsAsync((Conversation?)null);
        _searchService.Setup(s => s.SmartSearchAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((Enumerable.Empty<Product>(), false));
        _webSearch.Setup(w => w.SearchAsync(It.IsAny<string>()))
            .ReturnsAsync("fallback");

        var result = await _sut.AskWithContextAsync("hello", conversationId);

        result.ConversationId.Should().NotBe(conversationId);
    }

    [Fact]
    public async Task AskWithContextAsync_UsesExistingConversation()
    {
        var conversationId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = conversationId,
            Title = "Test",
            Messages = new List<Message>
            {
                new() { Role = "user", Content = "previous question" },
                new() { Role = "assistant", Content = "previous answer" }
            }
        };

        _conversationRepo.Setup(r => r.GetConversationAsync(conversationId))
            .ReturnsAsync(conversation);
        _conversationRepo.Setup(r => r.GetMessagesAsync(conversationId, 6))
            .ReturnsAsync(conversation.Messages);
        _searchService.Setup(s => s.SmartSearchAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((Enumerable.Empty<Product>(), false));
        _webSearch.Setup(w => w.SearchAsync(It.IsAny<string>()))
            .ReturnsAsync("fallback");

        var result = await _sut.AskWithContextAsync("follow up", conversationId);

        _conversationRepo.Verify(r => r.AddMessageAsync(conversationId, "user", "follow up"), Times.Once);
        _conversationRepo.Verify(r => r.AddMessageAsync(conversationId, "assistant", It.IsAny<string>()), Times.Once);
    }
}
