using Application.Service.Dtos.FinancialDocuments;
using FluentValidation;

namespace Application.Command.TransactionCommands.Validators;

public class PaymentDetailValidator : AbstractValidator<PaymentDetailDto>
{
    public PaymentDetailValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("مبلغ جزییات پرداخت الزامی می باشد.");
        RuleFor(x => x.PaymentDetailId).GreaterThan(0).WithMessage("شناسه جزییات پرداخت الزامی می باشد.");
        RuleFor(x => x.Type).NotNull().WithMessage("نوع جزییات پرداختی الزامی می باشد");
    }
}
