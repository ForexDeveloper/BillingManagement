using Application.Command.Base;
using FluentValidation;
using System.Linq;

namespace Application.Command.PlanCommands.Validators
{
    public class UpdatePlanDetailCommandvalidator : BaseCommandValidator<UpdatePlanDetailDto>
    {
        public UpdatePlanDetailCommandvalidator()
        {
            RuleFor(x => x.NumberOfInstallments)
                .Must(c => c != null && c.Count > 0)
                .WithMessage("انتخاب تعداد اقساط اجباریست.");

            RuleFor(x => x.NumberOfInstallments)
                .Must(c => c.GroupBy(c => c).Count() == c.Count)
                .WithMessage("تعداد اقساط تکراریست.")
                .When(x => x.NumberOfInstallments != null
                  && x.NumberOfInstallments.Count > 0);

            RuleForEach(x => x.NumberOfInstallments)
               .Must(c => c > 0)
               .WithMessage("مقدار صفر برای تعداد اقساط صحیح نیست.")
               .When(x => x.NumberOfInstallments != null && x.NumberOfInstallments.Count > 0);

            RuleFor(x => x.OperationFee).LessThan(100).WithMessage("مقدار هزینه عملیات نامعتبر است.");

            RuleFor(c => c.PenaltyPercent).
               Must(x => x <= 99 && x > 0).
               WithMessage("مقدار درصد جریمه نامعتبر است.")
               .When(x => x.PenaltyPercent > 0);

            RuleFor(c => c.PenaltyMaxAmount).
                Must(x => x <= 10000000000 && x > 0).
                WithMessage("مقدار حداکثر مبلغ جریمه نامعتبر است.")
                .When(x => x.PenaltyMaxAmount > 0);

            RuleFor(c => c.PenaltyMinAmount).
                  Must(x => x <= 10000000000 && x > 0).
                  WithMessage("مقدار حداقل مبلغ جریمه نامعتبر است.")
                 .When(x => x.PenaltyMinAmount > 0);

            RuleFor(planDetail => planDetail).
            Must(x => x.PenaltyMinAmount <= x.PenaltyMaxAmount).
            WithMessage(" حداقل مبلغ جریمه نامعتبر است.")
            .When(x => x.PenaltyMinAmount > 0
              && x.PenaltyMaxAmount > 0);


            RuleFor(c => c.WaiverPercent).
                  Must(x => x <= 99 && x > 0).
                  WithMessage("مقدار درصد پاداش نامعتبر است.")
                  .When(x => x.WaiverPercent > 0);

            RuleFor(c => c.WaiverMaxAmount).
                Must(x => x <= 10000000000 && x > 0).
                WithMessage("مقدار حداکثر مبلغ پاداش نامعتبر است.")
                .When(x => x.WaiverMaxAmount > 0);

            RuleFor(c => c.WaiverMinAmount).
                  Must(x => x <= 10000000000 && x > 0).
                  WithMessage("مقدار حداقل مبلغ پاداش نامعتبر است.")
                  .When(x => x.WaiverMinAmount > 0);

            RuleFor(planDetail => planDetail).
               Must(x => x.WaiverMinAmount <= x.WaiverMaxAmount).
               WithMessage(" حداقل مبلغ پاداش نامعتبر است.").
               When(x => x.WaiverMinAmount > 0
               && x.WaiverMaxAmount > 0);

            RuleFor(c => c.InterestPercent).
                Must(x => x <= 99 && x > 0).
                WithMessage("مقدار درصد بهره نامعتبر است.")
                .When(x => x.InterestPercent > 0);

            RuleFor(c => c.InterestMaxAmount).
                Must(x => x <= 10000000000 && x > 0).
                WithMessage("مقدار حداکثر مبلغ بهره نامعتبر است.")
                .When(x => x.InterestMaxAmount > 0);

            RuleFor(c => c.InterestMinAmount).
                  Must(x => x <= 10000000000 && x > 0).
                  WithMessage("مقدار حداقل مبلغ بهره نامعتبر است.")
                  .When(x => x.InterestMinAmount > 0);

            RuleFor(planDetail => planDetail).
             Must(x => x.InterestMinAmount <= x.InterestMaxAmount).
             WithMessage(" حداقل مبلغ بهره نامعتبر است.").
             When(x => x.InterestMinAmount > 0
             && x.InterestMaxAmount > 0);

            RuleFor(c => c.PrepaymentPercent).
               Must(x => x <= 99 && x > 0).
               WithMessage("مقدار درصد پیش پرداخت نامعتبر است.")
               .When(x => x.PrepaymentPercent > 0);

            RuleFor(c => c.PrepaymentMaxAmount).
               Must(x => x <= 10000000000 && x > 0).
               WithMessage("مقدار حداکثر مبلغ پیش پرداخت نامعتبر است.")
               .When(x => x.PrepaymentMaxAmount > 0);

            RuleFor(c => c.PrepaymentMinAmount).
                  Must(x => x <= 10000000000 && x > 0).
                  WithMessage("مقدار حداقل مبلغ پیش پرداخت نامعتبر است.")
                  .When(x => x.PrepaymentMinAmount > 0);

            RuleFor(planDetail => planDetail).
            Must(x => x.PrepaymentMinAmount <= x.PrepaymentMaxAmount).
            WithMessage(" حداقل مبلغ پیش پرداخت نامعتبر است.").
            When(x => x.PrepaymentMinAmount > 0
            && x.PrepaymentMaxAmount > 0);

            RuleFor(x => x.OperationalFeeType).IsInEnum().WithMessage(" نوع کسر هزینه عملیات نامعتبر است.");

        }
    }
}
