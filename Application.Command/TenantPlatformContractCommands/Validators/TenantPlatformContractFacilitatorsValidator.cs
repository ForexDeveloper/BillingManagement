using Application.Service.Dtos.TenantPlatformContracts;
using Domain.Core.Enums;
using FluentValidation;

namespace Application.Command.TenantPlatformContractCommands.Validators;

public class TenantPlatformContractFacilitatorsValidator : AbstractValidator<TenantPlatformContractFacilitatorDto>
{
    public TenantPlatformContractFacilitatorsValidator()
    {
        RuleFor(c => c.FacilitatorId).
            GreaterThan(0).WithMessage("شناسه تسهیلگر نامعتبر است.");

        RuleFor(c => c.FixedAmountCommissionPercentage).
            GreaterThanOrEqualTo(0).WithMessage("کارمزد از مبلغ ثابت سالانه نامعتبر است.")
            .When(x => x.FixedAmountCommissionPercentage != null);

        RuleFor(c => c.TransactionsCommissionPercentage).
            GreaterThanOrEqualTo(0).WithMessage("کارمزد از کمیسیون تراکنش ها نامعتبر است.")
            .When(x => x.TransactionsCommissionPercentage != null);

        RuleFor(c => c.PaymentMethodType)
            .IsInEnum().WithMessage("نوع پرداختی تسهیلگر نامعتبر است.")
            .When(x => x.FixedAmountCommissionPercentage != null || x.TransactionsCommissionPercentage != null);

        RuleFor(c => c.PaymentMethodType)
            .Must(x => x == BmPaymentMethodType.Cheque || x == BmPaymentMethodType.BankAccountDeposit)
            .WithMessage("نوع پرداختی تسهیلگر نامعتبر است.")
            .When(x => x.FixedAmountCommissionPercentage != null || x.TransactionsCommissionPercentage != null);
    }
}
