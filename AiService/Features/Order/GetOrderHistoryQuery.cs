using AiService.Contracts;
using MediatR;

namespace AiService.Features.Order;

public record GetOrderHistoryQuery : IRequest<OrderHistoryResponse>;
