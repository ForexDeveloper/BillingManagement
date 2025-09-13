using FluentValidation;

namespace Application.Command.TransactionCommands.Validators;

public class RefundTransactionCommandValidator : AbstractValidator<RefundTransactionCommand>
{
    public RefundTransactionCommandValidator()
    {
        RuleFor(x => x.FinancialDocumentId)
          .NotNull().WithMessage("شناسه پرداخت الزامی می باشد.");

        RuleFor(x => x.Amount)
          .GreaterThan(0).WithMessage("مبلغ عودت الزامی می باشد.");
    }
}
