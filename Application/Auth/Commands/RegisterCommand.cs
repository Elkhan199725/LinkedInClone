using Application.Auth.Dtos;
using MediatR;

namespace Application.Auth.Commands;

public sealed record RegisterCommand(RegisterRequest Request) : IRequest<AuthResponse>;
