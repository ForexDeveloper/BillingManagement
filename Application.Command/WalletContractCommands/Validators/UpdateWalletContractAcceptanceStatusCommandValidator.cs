using FluentValidation;

namespace Application.Command.WalletContractCommands.Validators
{
    public class UpdateWalletContractAcceptanceStatusCommandValidator : AbstractValidator<UpdateWalletContractAcceptanceStatusCommand>
    {
        public UpdateWalletContractAcceptanceStatusCommandValidator()
        {
            RuleFor(x => x.Reason)
                .Must(BaseValidationHelpers.IsSafeData).WithMessage("علت درخواست حاوی کاراکترهای غیر مجاز است.")
                .When(x => !string.IsNullOrEmpty(x.Reason));

            RuleFor(x => x.Reason)
                .Must(x => x.Length < 256).WithMessage("حداکثر تعداد کاراکترهای علت رد درخواست می تواند 256 باشد.")
                .When(x => !string.IsNullOrEmpty(x.Reason));

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("شناسه قرار کیف پول صحیح نمی باشد.");

            RuleFor(x => x.TenantId)
                .GreaterThan(0).WithMessage("شناسه مالک زیر ساخت صحیح نمی باشد.");

        }
    }
}
