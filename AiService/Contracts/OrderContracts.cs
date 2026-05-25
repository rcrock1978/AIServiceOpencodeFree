using AiService.Models;

namespace AiService.Contracts;

public record OrderHistoryResponse(List<Order> Orders);
