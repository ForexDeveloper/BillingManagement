using Application.Command.Base;
using Domain.Core.Enums;
using FluentValidation;
using System.Linq;

namespace Application.Command.PlanCommands.Validators
{
    public class CreatePlanCommandvalidator : BaseCommandValidator<CreatePlanCommand>
    {
        public CreatePlanCommandvalidator()
        {
            RuleFor(x => x.WalletConfigurationId).GreaterThan(0).WithMessage("انتخاب اطلاعات پایه کیف پول اجباریست.");

            RuleFor(x => x.Title).NotEmpty().WithMessage("وارد نمودن  عنوان طرح اجباریست.")
                .MaximumLength(250).WithMessage("تعداد کارکتر های عنوان طرح بیش از حد مجاز هست.");

            RuleFor(x => x.MaxTotalCredit).NotNull().GreaterThan(0).WithMessage("وارد نمودن سقف تجمیع اعتبار اجباریست.")
                .LessThan(9999999999999999).WithMessage("سقف تجمیع اعتبار از حد مجاز بیشتر است.");

            RuleFor(x => x.MaxWallet).NotNull().GreaterThan(0).WithMessage("وارد نمودن سقف مبلغ هر کیف اجباریست.");

            RuleFor(x => x.MaxDailyWithdrawal).LessThan(9999999999999999).WithMessage(" سقف برداشت روزانه  از حد مجاز بیشتر است.");

            RuleFor(x => x.MaxDailyDeposit).LessThan(999999999999999).WithMessage(" سقف واریز روزانه  از حد مجاز بیشتر است.");

            RuleFor(x => x.MaxDailyTransactionCount).LessThan(999999999999999).WithMessage("سقف تراکنش روزانه  از حد مجاز بیشتر است.");

            RuleFor(x => x.GracePeriod).LessThan(10000).WithMessage("مقدار مهلت بازپرداخت نامعتبر است.");

            RuleFor(c => c.InstallmentBreak)
               .Must(x => x <= 31 && x > 0)
               .WithMessage("مقدار تنفس صحیح نیست.")
                .When(x => x.InstallmentBreakType == TimeInterval.Day);

            RuleFor(c => c.InstallmentBreak)
                .Must(x => x <= 7 && x > 0)
                .WithMessage("مقدار تنفس صحیح نیست.")
                 .When(x => x.InstallmentBreakType == TimeInterval.Week);

            RuleFor(c => c.InstallmentBreak)
               .Must(x => x <= 31 && x > 0)
               .WithMessage("مقدار تنفس صحیح نیست.")
                .When(x => x.InstallmentBreakType == TimeInterval.Month);


            RuleFor(x => x.BillingPeriodType).IsInEnum().WithMessage("انتخاب بازه بازه صورتحساب اجباریست.");

            RuleFor(c => c.BillingPeriod).
                Must(x => x <= 31 && x > 0).
                WithMessage("مقدار بازه صورتحساب صحیح نیست.")
                .When(x => x.BillingPeriodType == TimeInterval.Day);

            RuleFor(c => c.BillingPeriod).Must(x => x <= 7 && x > 0).WithMessage("مقدار بازه صورتحساب صحیح نیست.")
              .When(x => x.BillingPeriodType == TimeInterval.Week);

            RuleFor(c => c.BillingPeriod).Must(x => x <= 31 && x > 0).WithMessage("مقدار بازه صورتحساب صحیح نیست.")
                .When(x => x.BillingPeriodType == TimeInterval.Month);

            //RuleFor(c => c.BillingPeriodStartDate).NotEmpty().WithMessage("انتخاب تاریخ مبدا صحیح نمی باشد.")
            //    .When(x => x.BillingPeriodType == TimeInterval.Day);

            //RuleFor(c => c.GracePeriod).
            //       Must(x => x <= 31 && x > 0).
            //       WithMessage("مهلت باز پرداخت صحیح نمی باشد.")
            //      .When(x => (x.BillingPeriodType is TimeInterval.Day or TimeInterval.Month));

            //RuleFor(c => c.GracePeriod).
            //      Must(x => x <= 7 && x > 0).
            //      WithMessage("مهلت باز پرداخت صحیح نمی باشد.")
            //     .When(x => (x.BillingPeriodType is TimeInterval.Week));

            RuleForEach(x => x.PlanDetails).SetValidator(new CreatePlanDetailCommandvalidator())
               .When(x => x.PlanDetails != null && x.PlanDetails.Count > 0);

            RuleFor(x => x.PlanClosedloops)
              .Must(c => c != null && c.Count > 0)
              .WithMessage("انتخاب کلوز لوپ اجباریست.");

            RuleFor(x => x.PlanClosedloops)
                .Must(c => c.GroupBy(c => c).Count() == c.Count)
                .WithMessage("کلوز لوپ تکراریست.")
                .When(x => x.PlanClosedloops != null && x.PlanClosedloops.Count > 0);


            RuleForEach(x => x.PlanClosedloops)
             .Must(c => c > 0)
             .WithMessage("مقدار صفر برای کلوز لوپ صحیح نیست.")
             .When(x => x.PlanClosedloops != null && x.PlanClosedloops.Count > 0);

            RuleFor(x => x.TermsAndConditions).NotEmpty().WithMessage("وارد نمودن شرایط و ضوابط اجباریست.")
                .NotNull().WithMessage("وارد نمودن شرایط و ضوابط اجباریست.")
               .MaximumLength(20000).WithMessage("تعداد کارکتر های شرایط و ضوابط بیش از حد مجاز هست.");

        }
    }
}
