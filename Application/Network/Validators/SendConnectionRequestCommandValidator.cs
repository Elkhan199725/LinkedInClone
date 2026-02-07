using Application.Network.Commands;
using FluentValidation;

namespace Application.Network.Validators;

public sealed class SendConnectionRequestCommandValidator : AbstractValidator<SendConnectionRequestCommand>
{
    public SendConnectionRequestCommandValidator()
    {
        RuleFor(x => x.SenderId)
            .NotEmpty().WithMessage("Sender ID is required.");

        RuleFor(x => x.ReceiverId)
            .NotEmpty().WithMessage("Receiver ID is required.");
    }
}
