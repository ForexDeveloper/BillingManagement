using Application.Service.Dtos.Shared;
using FluentValidation;

namespace Application.Command.TenantMerchantContractCommands.Validators;
public class TieredCommissionsValidator : AbstractValidator<TieredCommissionDto>
{
    public TieredCommissionsValidator()
    {
        RuleFor(c => c.FromAmount)
            .GreaterThanOrEqualTo(0).WithMessage("مبلغ کارمزد پلکانی نامعتبر است.");

        RuleFor(c => c.ToAmount)
            .GreaterThan(x => x.FromAmount).WithMessage("مبلغ کارمزد پلکانی نامعتبر است.")
            .When(x => x.ToAmount != null);

        RuleFor(c => c.Percentage)
            .Must(x => x >= 0 && x <= 100).WithMessage("درصد کارمزد پلکانی نامعتبر است.");

        RuleFor(c => c.MinAmount)
            .GreaterThanOrEqualTo(0).WithMessage("حداقل کارمزد هر تراکنش نامعتبر است.")
            .When(x => x.MinAmount != null);

        RuleFor(c => c.MaxAmount)
            .GreaterThan(x => x.MinAmount).WithMessage("حداکثر کارمزد هر تراکنش نامعتبر است.")
            .When(x => x.MaxAmount != null && x.MinAmount != null);
    }
}
