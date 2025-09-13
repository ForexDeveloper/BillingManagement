using Application.Command.TransactionCommands.Validators;
using FluentValidation;

namespace Application.Command.BillingCommands.Validators;

public class CreateBillingPaymentCommandValidator : AbstractValidator<CreateBillingPaymentCommand>
{
    public CreateBillingPaymentCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("انتخاب مالک زیر ساخت الزامی می باشد.");

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("شناسه مشتری الزامی می باشد.");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(20000).WithMessage("حداقل مبلغ پرداختی 2000 تومان می باشد.");

        RuleFor(x => x.PaymentId)
            .GreaterThan(0).WithMessage("شناسه پرداخت الزامی می باشد.");

        RuleFor(x => x.BillingId)
            .GreaterThan(0).WithMessage("شناسه صورت حساب الزامی می باشد.");

        RuleFor(x => x.PaymentDetails)
            .NotNull().WithMessage("جزییات پرداخت الزامی می باشد")
            .Must(x => x.Count != 0).WithMessage("جزییات پرداخت الزامی می باشد")
            .ForEach(item => item.SetValidator(new PaymentDetailValidator()));

        //RuleFor(x => x.PaymentDetails)
        //    .NotNull().WithMessage("جزییات پرداخت ضروری است.")
        //    .SetValidator(new PaymentDetailValidator());
    }
}
