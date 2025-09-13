using Application.Service.Dtos.Shared;
using Application.Service.Dtos.WalletContract;
using Domain.Core.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Command.WalletContractCommands.Validators;

public class WalletContractFacilitatorValidator : AbstractValidator<WalletContractFacilitatorDto>
{
    public WalletContractFacilitatorValidator()
    {
        RuleFor(x => x.FacilitatorId)
            .GreaterThan(0).WithMessage("انتخاب تسهیلگر اجباریست.");

        RuleFor(x => x.PortionTypes)
               .Must(x => x == null)
               .WithMessage("نحوه محاسبه کارمزد نامعتبر است.")
               .When(x => x.CommissionCalculationType == CommissionCalculationType.FixedAmount);

        RuleForEach(x => x.PortionTypes)
            .NotNull().WithMessage("نحوه محاسبه کارمزد اجباریست.")
            .IsInEnum().WithMessage("نحوه محاسبه کارمزد نامعتبر است.")
           .When(x => x.CommissionCalculationType != CommissionCalculationType.FixedAmount);

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
            .Must(x => NoOverlapInTieredCommissionsAmounts(x))
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
           .Must(x => x is >= 0 and <= 100)
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

        RuleFor(x => x.PeriodMinCommissionAmount)
           .GreaterThanOrEqualTo(0)
           .WithMessage("حداقل مبلغ کارمزد از جمع تراکنش ها نامعتبر است.")
           .When(x => x.PeriodMinCommissionAmount != null);

        RuleFor(x => x.PeriodMaxCommissionAmount)
           .GreaterThanOrEqualTo(0)
           .WithMessage("حداکثر مبلغ کارمزد از جمع تراکنش ها نامعتبر است.")
           .When(x => x.PeriodMaxCommissionAmount != null);
    }

    public bool NoOverlapInTieredCommissionsAmounts(List<TieredCommissionDto> tieredCommissions)
    {
        tieredCommissions = tieredCommissions.OrderBy(x => x.FromAmount).ToList();

        for (int i = 0; i < tieredCommissions.Count - 1; i++)
        {
            if (!tieredCommissions[i].ToAmount.HasValue)
                return false;

            if (tieredCommissions[i].ToAmount >= tieredCommissions[i + 1].FromAmount || Math.Abs(tieredCommissions[i].ToAmount.Value - tieredCommissions[i + 1].FromAmount) > 1)
                return false;
        }

        return true;
    }
}