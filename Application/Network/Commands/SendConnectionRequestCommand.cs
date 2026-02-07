using Application.Network.Dtos;
using MediatR;

namespace Application.Network.Commands;

public sealed record SendConnectionRequestCommand(Guid SenderId, Guid ReceiverId) : IRequest<ConnectionRequestDto>;
