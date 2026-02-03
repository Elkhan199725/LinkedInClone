using Application.Auth.Dtos;
using MediatR;

namespace Application.Auth.Commands;

public sealed record LoginCommand(LoginRequest Request) : IRequest<AuthResponse>;
