using Application.Command.Base;
using FluentValidation;

namespace Application.Command.CustomerCommands.Validators;

public class CreateCustomerCashOutRequestCommandValidator : BaseCommandValidator<CreateCustomerCashOutRequestCommand>
{
    public CreateCustomerCashOutRequestCommandValidator()
    {

        RuleFor(x => x.TenantId)
                .GreaterThan(0).WithMessage("شناسه مالک زیر ساخت نامعتبر است.");

        RuleFor(x => x.CustomerId)
               .GreaterThan(0).WithMessage("شناسه مشتری نامعتبر است.");

        RuleFor(x => x.BankAccountId)
               .GreaterThan(0).WithMessage("شناسه حساب بانکی نامعتبر است.");

        RuleFor(x => x.Amount)
               .GreaterThan(0).WithMessage("مبلغ درخواستی نامعتبر است.");
    }
}