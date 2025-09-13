using FluentValidation;

namespace Application.Command.TransactionCommands.Validators;

public class SetPaymentTransactionCommandValidator : AbstractValidator<SetPaymentTransactionCommand>
{
    public SetPaymentTransactionCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("انتخاب مالک زیر ساخت الزامی می باشد.");

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("شناسه مشتری الزامی می باشد.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("مبلغ پرداختی الزامی می باشد.");

        RuleFor(x => x.PaymentId)
            .GreaterThan(0).WithMessage("شناسه پرداخت الزامی می باشد.");

        RuleFor(x => x.PaymentDetails)
            .NotNull().WithMessage("جزییات پرداخت الزامی می باشد")
            .Must(x => x.Count != 0).WithMessage("جزییات پرداخت الزامی می باشد")
            .ForEach(item => item.SetValidator(new PaymentDetailValidator()));
    }
}
