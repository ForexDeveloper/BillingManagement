using FluentValidation;

namespace Application.Command.WalletCommands.Validators;

public class ActivateWalletCommandValidator : AbstractValidator<ActivateWalletCommand>
{
    public ActivateWalletCommandValidator()
    {
        RuleFor(x => x.WalletId)
            .GreaterThan(0).WithMessage("شناسه کیف پول صحیح نمی باشد.");

        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("شناسه مالک زیرساخت صحیح نمی باشد.");
    }
}
