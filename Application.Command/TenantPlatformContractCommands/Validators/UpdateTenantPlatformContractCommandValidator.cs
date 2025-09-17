using Application.Command.TenantMerchantContractCommands.Validators;
using Domain.Core.Enums;
using FluentValidation;
using System;
using System.Linq;

namespace Application.Command.TenantPlatformContractCommands.Validators;

public class UpdateTenantPlatformContractCommandValidator : AbstractValidator<UpdateTenantPlatformContractCommand>
{
    public UpdateTenantPlatformContractCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("شناسه قرارداد معتبر نمی باشد.");

        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("انتخاب مالک زیر ساخت اجباریست.");

        RuleFor(x => x.ContractNumber)
                .Must(BaseValidationHelpers.IsValidContractNumber)
                .WithMessage("شماره قرارداد معتبر نمی باشد.");

        RuleFor(x => x.StartDate)
            .NotNull()
            .NotEmpty()
            .WithMessage("تاریخ شروع قرارداد اجباریست.")
            .Must(x => x >= DateTime.Parse("1900/01/01"))
            .WithMessage("تاریخ شروع قرارداد صحیح نیست.");

        RuleFor(x => x.EndDate)
            .NotNull()
            .NotEmpty().WithMessage("تاریخ پایان قرارداد اجباریست.")
            .Must(x => x >= DateTime.Parse("1900/01/01")).WithMessage("تاریخ پایان قرارداد صحیح نیست.")
            .GreaterThan(x => x.StartDate).WithMessage("تاریخ پایان قرارداد باید بزرگتر از تاریخ شروع قرارداد باشد.");

        RuleFor(x => x.Description)
            .Must(BaseValidationHelpers.IsSafeData).WithMessage("توضیح قرارداد حاوی عبارات غیر مجاز است.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.FeeCalculationType)
            .NotNull().WithMessage("نوع محاسبات مالی اجباریست.")
            .IsInEnum().WithMessage("نحوه محاسبات مالی نامعتبر است.");

        RuleFor(x => x.CommissionCalculationType)
            .NotNull().WithMessage("نوع محاسبه کارمزد اجباریست.")
            .IsInEnum().WithMessage("نوع محاسبه کارمزد نامعتبر است.");

        RuleFor(x => x.FixedAmount)
            .NotNull().WithMessage("مبلغ ثابت سالانه اجباریست.")
            .When(x => x.FeeCalculationType == FeeCalculationType.AnnualFixedAmountWithCommission);

        RuleFor(x => x.FixedAmount)
            .NotNull().WithMessage("مبلغ ثابت ماهانه اجباریست.")
            .When(x => x.FeeCalculationType == FeeCalculationType.MonthlyFixedAmountWithCommission);

        RuleFor(x => x.FixedAmount)
            .Must(fixedAmount => fixedAmount == null || fixedAmount == 0)
            .WithMessage("مبلغ ثابت ماهانه یا سالانه نامعتبر است.")
            .When(x => x.FeeCalculationType == FeeCalculationType.CommissionWithoutFixedAmount);

        RuleFor(x => x.TieredCommissions)
          .NotNull().WithMessage("تعیین کارمزد پلکانی اجباریست.")
          .When(x => x.CommissionCalculationType == CommissionCalculationType.UniformTiered || x.CommissionCalculationType == CommissionCalculationType.CumulativeTiered);

        RuleFor(x => x.TieredCommissions)
           .Must(tieredCommissions => tieredCommissions == null || tieredCommissions.Count == 0)
           .WithMessage("کارمزد پلکانی نامعتبر است.")
          .When(x => x.CommissionCalculationType == CommissionCalculationType.FixedAmount || x.CommissionCalculationType == CommissionCalculationType.FixedPercentage);

        RuleFor(x => x.TieredCommissions)
           .Must(tieredCommissions => tieredCommissions != null && tieredCommissions.Count >= 2)
           .WithMessage("وارد کردن حداقل دو کارمزد پلکانی اجباریست.")
            .When(x => x.CommissionCalculationType == CommissionCalculationType.UniformTiered || x.CommissionCalculationType == CommissionCalculationType.CumulativeTiered);

        RuleFor(x => x.TieredCommissions.OrderBy(x => x.FromAmount).First().FromAmount)
           .Equal(0).WithMessage("مبلغ وارد شده در اولین بازه کارمزد پلکانی نامعتبر است.")
            .When(x => x.TieredCommissions != null && x.TieredCommissions.Count != 0 && (x.CommissionCalculationType == CommissionCalculationType.UniformTiered || x.CommissionCalculationType == CommissionCalculationType.CumulativeTiered));

        RuleFor(x => x.TieredCommissions.OrderBy(x => x.FromAmount).Last().ToAmount)
           .Null().WithMessage("مبلغ وارد شده در آخرین بازه کارمزد پلکانی نامعتبر است.")
            .When(x => x.TieredCommissions != null && x.TieredCommissions.Count != 0 && (x.CommissionCalculationType == CommissionCalculationType.UniformTiered || x.CommissionCalculationType == CommissionCalculationType.CumulativeTiered));

        RuleFor(x => x.TieredCommissions)
            .Must(x => new CreateTenantPlatformContractCommandValidator().NoOverlapInTieredCommissionsAmounts(x))
            .WithMessage("مبالغ موجود در کارمزد پلکانی همپوشانی دارند.")
            .When(x => x.TieredCommissions != null && x.TieredCommissions.Count != 0 && (x.CommissionCalculationType == CommissionCalculationType.UniformTiered || x.CommissionCalculationType == CommissionCalculationType.CumulativeTiered));

        RuleForEach(x => x.TieredCommissions)
            .SetValidator(new TieredCommissionsValidator())
            .When(x => x.TieredCommissions != null && x.TieredCommissions.Count != 0 && (x.CommissionCalculationType == CommissionCalculationType.UniformTiered || x.CommissionCalculationType == CommissionCalculationType.CumulativeTiered));

        RuleFor(x => x.FixedAmountCommission)
            .Must(x => x == null)
            .WithMessage("مبلغ کارمزد نامعتبر است.")
            .When(x => x.CommissionCalculationType != CommissionCalculationType.FixedAmount);

        RuleFor(x => x.FixedAmountCommission)
           .Must(x => x != null && x.Value > 0)
           .WithMessage("مبلغ کارمزد نامعتبر است.")
           .When(x => x.CommissionCalculationType == CommissionCalculationType.FixedAmount);

        RuleFor(x => x.FixedPercentageCommission)
            .Must(x => x == null)
            .WithMessage("درصد کارمزد نامعتبر است.")
            .When(x => x.CommissionCalculationType != CommissionCalculationType.FixedPercentage);

        RuleFor(x => x.FixedPercentageCommission)
            .Must(x => x != null && x.Value > 0)
           .WithMessage("درصد کارمزد نامعتبر است.")
           .When(x => x.CommissionCalculationType == CommissionCalculationType.FixedPercentage);

        RuleFor(x => x.TransactionMinCommissionAmount)
            .Must(x => x == null)
            .WithMessage("حداقل مبلغ کارمزد هر تراکنش در حالت کارمزد درصد ثابت نامعتبر است.")
            .When(x => x.CommissionCalculationType != CommissionCalculationType.FixedPercentage);

        RuleFor(x => x.TransactionMinCommissionAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("حداقل مبلغ کارمزد هر تراکنش در حالت کارمزد درصد ثابت نامعتبر است.")
            .When(x => x.CommissionCalculationType == CommissionCalculationType.FixedPercentage && x.TransactionMinCommissionAmount != null);

        RuleFor(x => x.TransactionMaxCommissionAmount)
            .Must(x => x == null)
            .WithMessage("حداکثر مبلغ کارمزد هر تراکنش در حالت کارمزد درصد ثابت نامعتبر است.")
            .When(x => x.CommissionCalculationType != CommissionCalculationType.FixedPercentage);

        RuleFor(x => x.TransactionMaxCommissionAmount)
           .GreaterThanOrEqualTo(0)
           .WithMessage("حداکثر مبلغ کارمزد هر تراکنش در حالت کارمزد درصد ثابت نامعتبر است.")
           .When(x => x.CommissionCalculationType == CommissionCalculationType.FixedPercentage && x.TransactionMaxCommissionAmount != null);

        RuleFor(x => x.CommissionReferenceTypes)
           .Must(x => x == null)
           .WithMessage("نحوه محاسبه کارمزد نامعتبر است.")
           .When(x => x.CommissionCalculationType == CommissionCalculationType.FixedAmount);

        RuleForEach(x => x.CommissionReferenceTypes)
            .NotNull().WithMessage("نحوه محاسبه کارمزد اجباریست.")
            .IsInEnum().WithMessage("نحوه محاسبه کارمزد نامعتبر است.")
           .When(x => x.CommissionCalculationType != CommissionCalculationType.FixedAmount);

        RuleFor(x => x.PeriodMinCommissionAmount)
           .GreaterThanOrEqualTo(0)
           .WithMessage("حداقل مبلغ کارمزد از جمع تراکنش ها نامعتبر است.")
           .When(x => x.PeriodMinCommissionAmount != null);

        RuleFor(x => x.PeriodMaxCommissionAmount)
           .GreaterThanOrEqualTo(0)
           .WithMessage("حداکثر مبلغ کارمزد از جمع تراکنش ها نامعتبر است.")
           .When(x => x.PeriodMaxCommissionAmount != null);

        RuleFor(x => x.BillingPeriodType)
            .NotNull().WithMessage("نوع بازه صورت حساب اجباریست.")
            .IsInEnum().WithMessage("نوع بازه صورت حساب نامعتبر است.");

        RuleFor(x => x.BillingPeriod)
            .GreaterThanOrEqualTo(0)
            .WithMessage("روز بازه صورت حساب نامعتبر است.");

        RuleFor(x => x.DailyBillingOriginDate)
            .Must(x => x >= DateTime.Parse("1900/01/01")).WithMessage("تاریخ شروع صدور صورت حساب نامعتبر نیست.")
            .When(x => x.BillingPeriodType == TimeInterval.Day);

        RuleFor(x => x.DailyBillingOriginDate)
            .Must(x => x == null).WithMessage("تاریخ شروع صدور صورت حساب نمی تواند مقدار داشته باشد.")
            .When(x => x.BillingPeriodType != TimeInterval.Day);

        RuleFor(x => x.PenaltyPercent)
            .GreaterThanOrEqualTo(0)
            .WithMessage("درصد جریمه دیرکرد روزانه نامعتبر است.")
           .When(x => x.PenaltyPercent != null);

        RuleFor(x => x.GracePeriod)
            .GreaterThanOrEqualTo(0)
            .WithMessage("مهلت پرداخت نامعتبر است.")
           .When(x => x.GracePeriod != null);

        RuleFor(x => x.GracePeriod)
            .Must(x => x >= 0 && x <= 28)
            .WithMessage("در حالت صورت حساب ماهانه، مهلت باز پرداخت نمی تواند بیشتر از 28 باشد.")
           .When(x => x.GracePeriod != null && x.BillingPeriodType == TimeInterval.Month);

        RuleFor(x => x.GracePeriod)
            .Must(x => x >= 0 && x <= 6)
            .WithMessage("در حالت صورت حساب هفتگی، مهلت باز پرداخت نمی تواند بیشتر از 6 باشد.")
           .When(x => x.GracePeriod != null && x.BillingPeriodType == TimeInterval.Week);

        RuleFor(x => x.GracePeriod)
            .LessThan(x => x.BillingPeriod)
            .WithMessage("در حالت صورت حساب روزانه، مهلت باز پرداخت نمی تواند بیشتر از  روز صورت حساب باشد.")
           .When(x => x.GracePeriod != null && x.BillingPeriodType == TimeInterval.Day);

        RuleFor(x => x.TenantIpgSettingId)
            .GreaterThan(0).WithMessage("تنظیمات درگاه پرداخت مالک زیرساخت اجباریست.");

        RuleFor(x => x.Providers)
            .Must(provider => provider.Any())
            .WithMessage("وارد کردن حداقل یک سرویس اجباریست.")
            .When(x => x.Providers != null);

        RuleForEach(x => x.Providers)
            .SetValidator(new TenantPlatformContractProvidersValidator())
            .When(x => x.Providers != null && x.Providers.Count != 0);

        RuleFor(x => x.Facilitators)
            .Must(x => x.Count > 0)
            .WithMessage("وارد کردن حداقل یک تسهیلگر اجباریست.")
            .When(x => x.Facilitators != null && x.Facilitators.Count > 0);

        RuleForEach(x => x.Facilitators)
           .SetValidator(new TenantPlatformContractFacilitatorsValidator())
           .When(x => x.Facilitators != null && x.Facilitators.Count != 0);
    }
}
