using FluentValidation;
using System;
using System.Linq;

namespace Application.Command.WalletContractCommands.Validators;

public class UpdateWalletContractCommandValidator : AbstractValidator<UpdateWalletContractCommand>
{
    public UpdateWalletContractCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("شناسه قرارداد اجباریست.");

        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("انتخاب مالک زیر ساخت اجباریست.");

        RuleFor(x => x.StartDate)
            .Must(x => x >= DateTime.Parse("1900/01/01"))
            .WithMessage("تاریخ شروع قرارداد صحیح نیست.");

        RuleFor(x => x.EndDate)
        .Must(x => x >= DateTime.Parse("1900/01/01"))
        .WithMessage("تاریخ پایان قرارداد صحیح نیست.")
        .GreaterThan(x => x.StartDate).WithMessage("تاریخ پایان قرارداد باید بزرگتر از تاریخ شروع قرارداد باشد.");

        RuleFor(x => x.AssignWalletToOrganizationCustomers)
            .NotNull().NotEmpty().WithMessage("اختصاص کیف پول به تمام کاربران سازمان اجباریست.");

        RuleFor(x => x.PlanIds)
            .Must(x => x.Any()).WithMessage("انتخاب پلن اجباریست.");

        RuleForEach(x => x.PlanIds)
            .GreaterThan(0).WithMessage("شناسه پلن معتبر نیست.");

        RuleFor(x => x.OrganizationId)
            .GreaterThan(0).WithMessage("انتخاب سازمان اجباریست.");

        RuleFor(x => x.Customers)
            .Must(customers => customers == null || customers.All(c => c > 0))
            .WithMessage("شناسه کاربر نمی تواند کوچتر مساوی صفر باشد.");

        RuleFor(x => x.Guarantor)
            .NotNull().WithMessage("انتخاب ضامن اجباریست.")
            .SetValidator(new WalletContractGuarantorValidator());


        RuleFor(x => x.Financier)
            .SetValidator(new WalletContractFinancierValidator())
            .When(x => x.Financier != null);

        RuleFor(x => x.Facilitators)
            .Must(list => list != null && list.All(item => item != null))
            .WithMessage("تسهیلگر حاوی مقدار نامعتبر است.")
            .When(x => x.Facilitators != null && x.Facilitators.Count > 0);

        RuleForEach(x => x.Facilitators)
            .SetValidator(new WalletContractFacilitatorValidator())
            .When(x => x.Facilitators != null && x.Facilitators.Count > 0);
    }
}
