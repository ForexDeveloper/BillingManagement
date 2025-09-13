using FluentValidation;

namespace Application.Command.WalletCommands.Validators;

public class UpdateDefaultWalletCommandValidator : AbstractValidator<UpdateDefaultWalletCommand>
{
    public UpdateDefaultWalletCommandValidator()
    {
        RuleFor(x => x.WalletId)
            .GreaterThan(0).WithMessage("شناسه کیف پول صحیح نمی باشد.");

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("شناسه مشتری صحیح نمی باشد.");
    }
}
