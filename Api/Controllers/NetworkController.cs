using Application.Network.Commands;
using Application.Network.Dtos;
using Application.Network.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class NetworkController : ControllerBase
{
    private readonly IMediator _mediator;

    public NetworkController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Follow Endpoints

    [HttpPost("follows/{userId:guid}")]
    public async Task<ActionResult<FollowResponse>> FollowUser(Guid userId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _mediator.Send(new FollowUserCommand(currentUserId, userId), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("follows/{userId:guid}")]
    public async Task<ActionResult<FollowResponse>> UnfollowUser(Guid userId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _mediator.Send(new UnfollowUserCommand(currentUserId, userId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("follows/me")]
    public async Task<ActionResult<FollowListResponse>> GetMyFollowing(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _mediator.Send(new GetFollowingQuery(currentUserId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("followers/me")]
    public async Task<ActionResult<FollowListResponse>> GetMyFollowers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _mediator.Send(new GetFollowersQuery(currentUserId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    #endregion

    #region Connection Request Endpoints

    [HttpPost("connections/requests/{userId:guid}")]
    public async Task<ActionResult<ConnectionRequestDto>> SendConnectionRequest(Guid userId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _mediator.Send(new SendConnectionRequestCommand(currentUserId, userId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("connections/requests/{requestId:guid}/accept")]
    public async Task<IActionResult> AcceptConnectionRequest(Guid requestId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        await _mediator.Send(new AcceptConnectionRequestCommand(currentUserId, requestId), cancellationToken);
        return NoContent();
    }

    [HttpPost("connections/requests/{requestId:guid}/reject")]
    public async Task<IActionResult> RejectConnectionRequest(Guid requestId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        await _mediator.Send(new RejectConnectionRequestCommand(currentUserId, requestId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("connections/requests/{requestId:guid}")]
    public async Task<IActionResult> CancelConnectionRequest(Guid requestId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        await _mediator.Send(new CancelConnectionRequestCommand(currentUserId, requestId), cancellationToken);
        return NoContent();
    }

    [HttpGet("connections/requests/incoming")]
    public async Task<ActionResult<ConnectionRequestListResponse>> GetIncomingRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _mediator.Send(new GetIncomingRequestsQuery(currentUserId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("connections/requests/outgoing")]
    public async Task<ActionResult<ConnectionRequestListResponse>> GetOutgoingRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _mediator.Send(new GetOutgoingRequestsQuery(currentUserId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    #endregion

    #region Connection Endpoints

    [HttpGet("connections")]
    public async Task<ActionResult<ConnectionListResponse>> GetConnections(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _mediator.Send(new GetConnectionsQuery(currentUserId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("connections/{userId:guid}")]
    public async Task<IActionResult> RemoveConnection(Guid userId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        await _mediator.Send(new RemoveConnectionCommand(currentUserId, userId), cancellationToken);
        return NoContent();
    }

    #endregion

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid or missing user identifier.");
        return userId;
    }
}
