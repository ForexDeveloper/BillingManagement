using Application.Command.Base;
using Domain.Core.Enums;
using Domain.Core.Helper;
using FluentValidation;
using System;
using System.Linq;

namespace Application.Command.TenantMerchantContractCommands.Validators
{
    public class UpdateTenantMerchantContractCommandValidator : BaseCommandValidator<UpdateTenantMerchantContractCommand>
    {
        public UpdateTenantMerchantContractCommandValidator()
        {
            RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("شناسه قرارداد معتبر نمی باشد.");

            RuleFor(x => x.MerchantId)
                .GreaterThan(0).WithMessage("انتخاب پذیرنده اجباریست.");

            RuleFor(x => x.ContractNumber)
                .Must(BaseValidationHelpers.IsValidContractNumber)
                .WithMessage("شماره قرارداد معتبر نمی باشد.");

            RuleFor(x => x.StartDate)
            .Must(x => x >= DateTime.Parse("1900/01/01"))
            .WithMessage("تاریخ شروع قرارداد صحیح نیست.");

            RuleFor(x => x.EndDate)
            .Must(x => x >= DateTime.Parse("1900/01/01"))
            .WithMessage("تاریخ پایان قرارداد صحیح نیست.")
            .GreaterThan(x => x.StartDate).WithMessage("تاریخ پایان قرارداد باید بزرگتر از تاریخ شروع قرارداد باشد.");

            RuleFor(x => x.ContractDocument)
                .Must(x => x != null)
                .WithMessage("انتخاب تصویر پذیرنده حقیقی یا حقوقی اجباریست.");

            RuleFor(x => x.ContractDocument.EnamadLink)
                .Must(BaseValidationHelpers.IsValidUrl)
                .WithMessage("لینک اینماد نامعتبر است.")
                .When(x => x.ContractDocument != null && !string.IsNullOrEmpty(x.ContractDocument.EnamadLink));

            RuleFor(x => x.ContractDocument.InternetBusinessLicenseLink)
                .Must(BaseValidationHelpers.IsValidUrl)
                .WithMessage("لینک مجوز کسب و کار اینترنتی نامعتبر است.")
               .When(x => x.ContractDocument != null && !string.IsNullOrEmpty(x.ContractDocument.InternetBusinessLicenseLink));

            RuleFor(x => x.SettlementType)
                 .NotNull().WithMessage("انتخاب نحوه تسویه‌حساب اجباریست.")
                 .IsInEnum().WithMessage("نحوه تسویه‌حساب نامعتبر است.");

            RuleFor(x => x.IsCommissionExchanged)
               .NotNull().WithMessage("انتخاب امکان تهاتر یا عدم تهاتر اجباریست.");

            RuleFor(x => x.InstallmentsCount)
              .NotNull().WithMessage("تعداد اقساط اجباریست.")
              .GreaterThanOrEqualTo(2).WithMessage("تعداد اقساط نامعتبر است.")
              .When(x => x.SettlementType == SettlementType.Installments);

            RuleFor(x => x.InstallmentsCount)
              .Null().WithMessage("تعداد اقساط نامعتبر است.")
              .When(x => x.SettlementType == SettlementType.LumpSum);

            RuleFor(x => x.CommissionDeductionMethodType)
                .NotNull().WithMessage("انتخاب نحوه کسر کارمزد از اقساط اجباریست.")
                .IsInEnum().WithMessage("نحوه کسر کارمزد از اقساط نامعتبر است.")
                .When(x => x.SettlementType == SettlementType.Installments && x.IsCommissionExchanged);

            RuleFor(x => x.CommissionDeductionMethodType)
                .Null().WithMessage("نحوه کسر کارمزد از اقساط نامعتبر است.")
                .When(x => x.SettlementType == SettlementType.LumpSum);

            RuleFor(x => x.InterestPercentage)
                .Must(x => x is >= 0 and <= 100).WithMessage("درصد بهره نامعتبر است.")
                .When(x => x.InterestPercentage != null);

            RuleForEach(x => x.InterestReferenceTypes)
                .NotNull().WithMessage("نحوه محاسبه بهره اجباریست.")
                .IsInEnum().WithMessage("نحوه محاسبه بهره نامعتبر است.")
                .When(x => x.InterestPercentage > 0);

            RuleFor(x => x.InterestReferenceTypes)
                .Null().WithMessage("نحوه محاسبه بهره نامعتبر است.")
                .When(x => x.InterestPercentage == null);

            RuleFor(x => x.BillingPeriodType)
            .NotNull().WithMessage("نوع بازه صورت حساب اجباریست.")
            .IsInEnum().WithMessage("نوع بازه صورت حساب نامعتبر است.");

            RuleFor(x => x.BillingPeriod)
            .Must((x, period) =>
                x.BillingPeriodType == TimeInterval.Day
                    ? period > 0
                    : period >= 0)
            .WithMessage("تعداد روز بازه صورت حساب نامعتبر است.");

            RuleFor(x => x.DailyBillingOriginDate)
                .Must(x => x >= DateTime.Parse("1900/01/01")).WithMessage("تاریخ شروع صدور صورت حساب نامعتبر نیست.")
                .When(x => x.BillingPeriodType == TimeInterval.Day);

            RuleFor(x => x.DailyBillingOriginDate)
                .Must(x => x == null).WithMessage("تاریخ شروع صدور صورت حساب نمی تواند مقدار داشته باشد.")
                .When(x => x.BillingPeriodType != TimeInterval.Day);

            RuleFor(x => x.BillingBreak)
            .GreaterThanOrEqualTo(0)
            .WithMessage("بازه تنفس نامعتبر است.")
           .When(x => x.BillingBreak != null);

            RuleFor(c => c.PaymentMethodType)
             .NotNull().WithMessage("روش پرداخت سهم پذیرنده اجباریست.")
            .IsInEnum().WithMessage("روش پرداخت سهم پذیرنده نامعتبر است.");

            RuleFor(x => x.GuaranteeType)
                 .IsInEnum().WithMessage("نوع ضمانت معتبر نیست.")
                 .When(x => x.GuaranteeType != null);

            RuleFor(x => x.CommissionCalculationType)
            .NotNull().WithMessage("نوع محاسبه کارمزد اجباریست.")
            .IsInEnum().WithMessage("نوع محاسبه کارمزد نامعتبر است.");

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
                .Must(x => new CreateTenantMerchantContractCommandValidator().NoOverlapInTieredCommissionsAmounts(x))
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
               .Must(x => x != null && x.Value >= 0)
               .WithMessage("مبلغ کارمزد نامعتبر است.")
               .When(x => x.CommissionCalculationType == CommissionCalculationType.FixedAmount);

            RuleFor(x => x.FixedPercentageCommission)
                .Must(x => x == null)
                .WithMessage("درصد کارمزد نامعتبر است.")
                .When(x => x.CommissionCalculationType != CommissionCalculationType.FixedPercentage);

            RuleFor(x => x.FixedPercentageCommission)
               .Must(x => x != null && x.Value >= 0)
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
               .GreaterThanOrEqualTo(x => x.PeriodMinCommissionAmount)
               .WithMessage("حداکثر مبلغ کارمزد از جمع تراکنش ها نامعتبر است.")
               .When(x => x.PeriodMaxCommissionAmount != null);
        }
    }
}