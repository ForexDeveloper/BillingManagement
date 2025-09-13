using Application.Command.Base;
using FluentValidation;

namespace Application.Command.CustomerCommands.Validators;

public class RejectCustomerCashOutRequestCommandValidator : BaseCommandValidator<RejectCustomerCashOutRequestCommand>
{
    public RejectCustomerCashOutRequestCommandValidator()
    {
        RuleFor(x => x.CustomerId)
              .GreaterThan(0).WithMessage("شناسه مشتری نامعتبر است.");

        RuleFor(x => x.CashOutRequestId)
              .GreaterThan(0).WithMessage("شناسه درخواست دریافت وجه نقد معتبر نمی باشد.");

        RuleFor(x => x.TenantId)
                .GreaterThan(0).WithMessage("شناسه مالک زیر ساخت نامعتبر است.");

        RuleFor(x => x.RejectReason)
            .NotNull().WithMessage("نوع دلیل رد درخواست اجباریست.")
            .IsInEnum().WithMessage("نوع دلیل رد درخواست نامعتبر است.");

        RuleFor(x => x.Description)
            .MaximumLength(150).WithMessage("تعداد کارکتر های عنوان طرح بیش از حد مجاز هست.");
    }
}