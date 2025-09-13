using Application.Command.Base;
using Domain.Core.Enums;
using FluentValidation;
using System.Linq;

namespace Application.Command.PlanCommands.Validators
{
    public class UpdateCashWalletConfigurationPlanCommandvalidator : BaseCommandValidator<UpdateCashWalletConfigurationPlanCommand>
    {
        public UpdateCashWalletConfigurationPlanCommandvalidator()
        {
            RuleFor(x => x.MaxWallet).GreaterThan(0).WithMessage("مقدار کیف پول صحیح نیست.");

            RuleFor(x => x.MaxDailyWithdrawal).NotNull().WithMessage("وارد نمودن سقف برداشت روزانه اجباریست.")
                .GreaterThan(0).WithMessage("مقدار سقف برداشت روزانه صحیح نیست.");

            RuleFor(x => x.MaxDailyDeposit).NotNull().WithMessage("وارد نمودن سقف واریز روزانه اجبریست.")
                 .GreaterThan(0).WithMessage("مقدار سقف واریز روزانه صحیح نیست.");

            RuleFor(x => x.MaxDailyTransactionCount).LessThan(50).WithMessage("سقف تراکنش روزانه  از حد مجاز بیشتر است.");

            RuleFor(x => x.TermsAndConditions)
              .MaximumLength(250).WithMessage("تعداد کارکتر های عنوان طرح بیش از حد مجاز هست.")
              .When(c => !string.IsNullOrEmpty(c.TermsAndConditions));
        }
    }
}
