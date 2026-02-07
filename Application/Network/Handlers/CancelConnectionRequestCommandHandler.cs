using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Network.Commands;
using Domain.Enums;
using MediatR;

namespace Application.Network.Handlers;

public sealed class CancelConnectionRequestCommandHandler : IRequestHandler<CancelConnectionRequestCommand, bool>
{
    private readonly IConnectionRequestRepository _requestRepository;

    public CancelConnectionRequestCommandHandler(IConnectionRequestRepository requestRepository)
    {
        _requestRepository = requestRepository;
    }

    public async Task<bool> Handle(CancelConnectionRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _requestRepository.GetByIdAsync(command.RequestId, cancellationToken);

        if (request is null)
            throw new NotFoundException("ConnectionRequest", command.RequestId);

        if (request.SenderId != command.CurrentUserId)
            throw new ForbiddenException("You can only cancel requests you sent.");

        if (request.Status != ConnectionRequestStatus.Pending)
            throw new ForbiddenException("This request is no longer pending.");

        request.Status = ConnectionRequestStatus.Cancelled;
        request.UpdatedAt = DateTime.UtcNow;

        _requestRepository.Update(request);
        await _requestRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
