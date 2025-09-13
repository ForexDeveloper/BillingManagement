using Application.Command.Base;
using Domain.Core.Enums;
using FluentValidation;
using System.Linq;

namespace Application.Command.WalletConfigurationCommands.Validators
{
    public class CreateWalletConfigurationCommandValidator : BaseCommandValidator<CreateWalletConfigurationCommand>
    {
        public CreateWalletConfigurationCommandValidator()
        {
            RuleFor(x => x.TenantId).GreaterThan(0).WithMessage("انتخاب نام مالک زیر ساخت اجباریست.");

            RuleFor(x => x.Title).NotEmpty().NotNull().WithMessage("وارد نمودن نام کیف پول اجباریست.");

            RuleFor(x => x.WalletTypeId).IsInEnum().WithMessage("انتخاب مدل کیف پول اجباریست.");

            RuleFor(x => x.MaxWallet).GreaterThan(0).WithMessage("وارد نمودن سقف کیف پول اجباریست.");

            RuleFor(x => x.MaxWallet).LessThan(99999999999999).WithMessage("سقف کیف پول وارد شده نامعتبر است.");

            RuleFor(x => x.Installments).NotNull().WithMessage("بخش اقساط را تکمیل کنید.");

            RuleFor(x => x.Installments.MaxInstallments)
                .Must(c => c != null && c.Count > 0)
                .WithMessage("انتخاب تعداد اقساط اجباریست.")
                .When(x => x.Installments != null );

            RuleFor(x => x.Installments.MaxInstallments)
                .Must(c => c.GroupBy(c => c).Count() == c.Count)
                .WithMessage("تعداد اقساط تکراریست.")
                .When(x => x.Installments != null && x.Installments.MaxInstallments != null
                  && x.Installments.MaxInstallments.Count > 0);

            RuleForEach(x => x.Installments.MaxInstallments)
              .Must(c => c > 0)
              .WithMessage("مقدار صفر برای تعداد اقساط صحیح نیست.")
              .When(x => x.Installments != null && x.Installments.MaxInstallments != null
                  && x.Installments.MaxInstallments.Count > 0);


            RuleFor(c => c.Installments.PrepaymentMaxPercent).
                   Must(x => x <= 99 && x > 0).
                   WithMessage("حداکثر در صد  پیش پرداخت صحیح نمی باشد.")
                   .When(x => x.Installments != null && x.Installments.PrepaymentMaxPercent > 0);

            RuleFor(c => c.Installments.PrepaymentMinPercent).
                 Must(x => x <= 99 && x > 0).
                 WithMessage("حداقل در صد پیش پرداخت صحیح نمی باشد.")
                 .When(x => x.Installments != null && x.Installments.PrepaymentMinPercent > 0);


            RuleFor(c => c.Installments).
                  Must(x => x.PrepaymentMinPercent <= x.PrepaymentMaxPercent).
                  WithMessage("حداقل در صد پیش پرداخت صحیح نمی باشد.")
                  .When(x => x.Installments != null && x.Installments.PrepaymentMinAmount > 0
                    && x.Installments.PrepaymentMaxAmount > 0);

            RuleFor(c => c.Installments.PrepaymentMaxAmount).
                  Must(x => x <= 999999999999 && x > 0).
                  WithMessage("حداکثر مبلغ صحیح نمی باشد.")
                  .When(x => x.Installments != null && x.Installments.PrepaymentMaxAmount > 0);

            RuleFor(c => c.Installments.PrepaymentMinAmount).
                  Must(x => x <= 999999999999 && x > 0).
                  WithMessage("حداقل مبلغ صحیح نمی باشد.")
                  .When(x => x.Installments != null && x.Installments.PrepaymentMinAmount > 0);

            RuleFor(c => c.Installments).
                  Must(x => x.PrepaymentMinAmount <= x.PrepaymentMaxAmount).
                  WithMessage("حداقل مبلغ صحیح نمی باشد.")
                  .When(x => x.Installments != null && x.Installments.PrepaymentMinAmount > 0
                    && x.Installments.PrepaymentMaxAmount > 0);

            RuleFor(c => c.Financial.InterestPeriodMaxPercent).
                 Must(x => x <= 99 && x > 0).
                 WithMessage("حداکثر درصد دامنه بهره صحیح نمی باشد.")
                 .When(x => x.Financial != null && x.Financial.InterestPeriodMaxPercent > 0);

            RuleFor(c => c.Financial.InterestPeriodMinPercent).
                  Must(x => x <= 99 && x > 0).
                  WithMessage("حداقل درصد دامنه بهره صحیح نمی باشد.")
                  .When(x => x.Financial != null && x.Financial.InterestPeriodMinPercent > 0);

            RuleFor(c => c.Financial).
                  Must(x => x.InterestPeriodMinPercent <= x.InterestPeriodMaxPercent).
                  WithMessage("حداقل درصد دامنه بهره صحیح نمی باشد.")
                  .When(x => x.Financial != null && x.Financial.InterestPeriodMinPercent > 0
                    && x.Financial.InterestPeriodMaxPercent > 0);


            RuleFor(c => c.Financial.InterestPeriodMaxAmount).
                 Must(x => x <= 10000000000 && x > 0).
                 WithMessage("حداکثر مبلغ دامنه بهره صحیح نمی باشد.")
                 .When(x => x.Financial != null && x.Financial.InterestPeriodMaxAmount > 0);

            RuleFor(c => c.Financial.InterestPeriodMinAmount).
                  Must(x => x <= 10000000000 && x > 0).
                  WithMessage("حداقل درصد مبلغ بهره صحیح نمی باشد.")
                  .When(x => x.Financial != null && x.Financial.InterestPeriodMinAmount > 0);

            RuleFor(c => c.Financial).
                  Must(x => x.InterestPeriodMinAmount <= x.InterestPeriodMaxAmount).
                  WithMessage("حداقل درصد مبلغ بهره صحیح نمی باشد.")
                  .When(x => x.Financial != null && x.Financial.InterestPeriodMinAmount > 0
                    && x.Financial.InterestPeriodMaxAmount > 0);


            RuleFor(c => c.Financial.PenaltyPeriodMaxPercent).
                 Must(x => x <= 99 && x > 0).
                 WithMessage("حداکثر درصد دامنه جریمه صحیح نمی باشد.")
                 .When(x => x.Financial != null && x.Financial.PenaltyPeriodMaxPercent > 0);

            RuleFor(c => c.Financial.PenaltyPeriodMinPercent).
                  Must(x => x <= 99 && x > 0).
                  WithMessage("حداقل درصد دامنه جریمه صحیح نمی باشد.")
                  .When(x => x.Financial != null && x.Financial.PenaltyPeriodMinPercent > 0);

            RuleFor(c => c.Financial).
                  Must(x => x.PenaltyPeriodMinPercent <= x.PenaltyPeriodMaxPercent).
                  WithMessage("حداقل درصد دامنه جریمه صحیح نمی باشد.")
                  .When(x => x.Financial != null && x.Financial.PenaltyPeriodMinPercent > 0
                    && x.Financial.PenaltyPeriodMaxPercent > 0);


            RuleFor(c => c.Financial.PenaltyPeriodMaxAmount).
                 Must(x => x <= 10000000000 && x > 0).
                 WithMessage("حداکثر مبلغ دامنه جریمه صحیح نمی باشد.")
                 .When(x => x.Financial != null && x.Financial.PenaltyPeriodMaxAmount > 0);

            RuleFor(c => c.Financial.PenaltyPeriodMinAmount).
                  Must(x => x <= 10000000000 && x > 0).
                  WithMessage("حداقل درصد مبلغ جریمه صحیح نمی باشد.")
                  .When(x => x.Financial != null && x.Financial.PenaltyPeriodMinAmount > 0);

            RuleFor(c => c.Financial).
                  Must(x => x.PenaltyPeriodMinAmount <= x.PenaltyPeriodMaxAmount).
                  WithMessage("حداقل درصد مبلغ جریمه صحیح نمی باشد.")
                  .When(x => x.Financial != null && x.Financial.PenaltyPeriodMinAmount > 0
                    && x.Financial.PenaltyPeriodMaxAmount > 0);

            RuleFor(c => c.Financial.WaiverPeriodMaxPercent).
                 Must(x => x <= 99 && x > 0).
                 WithMessage("حداکثر درصد دامنه پاداش خوش حسابی صحیح نمی باشد.")
                 .When(x => x.Financial != null && x.Financial.WaiverPeriodMaxPercent > 0);

            RuleFor(c => c.Financial.WaiverPeriodMinPercent).
                  Must(x => x <= 99 && x > 0).
                  WithMessage("حداقل درصد دامنه پاداش خوش حسابی صحیح نمی باشد.")
                  .When(x => x.Financial != null && x.Financial.WaiverPeriodMinPercent > 0);

            RuleFor(c => c.Financial).
                  Must(x => x.WaiverPeriodMinPercent <= x.WaiverPeriodMaxPercent).
                  WithMessage("حداقل درصد دامنه پاداش خوش حسابی صحیح نمی باشد.")
                  .When(x => x.Financial != null && x.Financial.WaiverPeriodMinPercent > 0
                    && x.Financial.WaiverPeriodMaxPercent > 0);


            RuleFor(c => c.Financial.WaiverPeriodMaxAmount).
                 Must(x => x <= 10000000000 && x > 0).
                 WithMessage("حداکثر مبلغ دامنه پاداش خوش حسابی صحیح نمی باشد.")
                 .When(x => x.Financial != null && x.Financial.WaiverPeriodMaxAmount > 0);

            RuleFor(c => c.Financial.WaiverPeriodMinAmount).
                  Must(x => x <= 10000000000 && x > 0).
                  WithMessage("حداقل  مبلغ پاداش خوش حسابی صحیح نمی باشد.")
                  .When(x => x.Financial != null && x.Financial.WaiverPeriodMinAmount > 0);

            RuleFor(c => c.Financial).
                  Must(x => x.WaiverPeriodMinAmount <= x.WaiverPeriodMaxAmount).
                  WithMessage("حداقل  مبلغ پاداش خوش حسابی صحیح نمی باشد.")
                  .When(x => x.Financial != null && x.Financial.WaiverPeriodMinAmount > 0
                    && x.Financial.WaiverPeriodMaxAmount > 0);


        }
    }
}