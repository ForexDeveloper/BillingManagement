using FluentValidation;

namespace Application.Command.TransactionCommands.Validators;

public class ReverseTransactionCommandValidator : AbstractValidator<ReverseTransactionCommand>
{
    public ReverseTransactionCommandValidator()
    {
        RuleFor(x => x.PaymentIds)
          .NotNull().WithMessage("شناسه پرداخت الزامی می باشد")
          .Must(x => x.Count != 0).WithMessage("شناسه پرداخت الزامی می باشد");
    }
}
