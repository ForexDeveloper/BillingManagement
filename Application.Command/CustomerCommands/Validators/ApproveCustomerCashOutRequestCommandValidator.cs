
using Application.Command.Base;
using FluentValidation;

namespace Application.Command.CustomerCommands.Validators;

public class ApproveCustomerCashOutRequestCommandValidator : BaseCommandValidator<ApproveCustomerCashOutRequestCommand>
{
    public ApproveCustomerCashOutRequestCommandValidator()
    {
        RuleFor(x => x.CustomerId)
              .GreaterThan(0).WithMessage("شناسه مشتری نامعتبر است.");

        RuleFor(x => x.CashOutRequestId)
              .GreaterThan(0).WithMessage("شناسه درخواست دریافت وجه نقد معتبر نمی باشد.");

        RuleFor(x => x.TenantId)
                .GreaterThan(0).WithMessage("شناسه مالک زیر ساخت نامعتبر است.");

        RuleFor(x => x.Description)
            .MaximumLength(150).WithMessage("تعداد کارکترهای توضیحات بیش از 150 کاراکتر می‌باشد.");

        RuleFor(x => x.BankTransactionCode)
            .NotNull().NotEmpty()
            .WithMessage("شناسه تراکنش بانکی اجباری می باشد.")
            .MaximumLength(100)
            .WithMessage("طول کاراکترهای شناسه تراکنش بانکی بیش از 20 کارکتر می‌باشد.");
    }
}