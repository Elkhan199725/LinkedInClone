using Application.Auth.Dtos;
using MediatR;

namespace Application.Auth.Commands;

public sealed record ForgotPasswordCommand(ForgotPasswordRequest Request) : IRequest<ForgotPasswordResponse>;
