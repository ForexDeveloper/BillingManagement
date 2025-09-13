using Application.Service.Dtos.FinancialDocuments;
using FluentValidation;
using System.Collections.Generic;
using System.Linq;

namespace Application.Command.TransactionCommands.Validators;

public class ValidateAndSetTransactionCommandValidator : AbstractValidator<ValidateAndSetTransactionCommand>
{
    public ValidateAndSetTransactionCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("انتخاب مالک زیر ساخت الزامی می باشد.");

        RuleFor(x => x.FromBusinessIdentityId)
            .GreaterThan(0).WithMessage("FromBusinessIdentityId is required.");

        RuleFor(x => x.ToBusinessIdentityId)
            .GreaterThan(0).WithMessage("ToBusinessIdentityId is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("مبلغ پرداختی الزامی می باشد.");

        RuleFor(x => x.PaymentId)
            .GreaterThan(0).WithMessage("شناسه پرداخت الزامی می باشد.");

        RuleFor(x => x.PaymentDetails)
            .NotNull().WithMessage("جزییات پرداخت الزامی می باشد")
            .Must(x => x.Count != 0).WithMessage("جزییات پرداخت الزامی می باشد")
            .ForEach(item => item.SetValidator(new PaymentDetailValidator()));

        RuleFor(x => x.PaymentDetails)
            .Must(HaveValidAmount)
            .WithMessage("مبلغ کل جزییات پرداخت باید با مبلغ اصلی برابر باشد.")
            .When(x => x.PaymentDetails != null && x.PaymentDetails.Count > 0);
    }

    private bool HaveValidAmount(ValidateAndSetTransactionCommand payment, List<PaymentDetailDto> PaymentDetails)
    {
        return PaymentDetails.Sum(item => item.Amount) == payment.Amount;
    }
}
