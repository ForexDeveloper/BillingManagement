using System;
using System.Linq;
using System.Collections.Generic;
using Domain.Core.Entities.Shared.Exceptions;

namespace Domain.Core.Entities.Shared;

/// <summary>
/// از رکورد به کلاس تبدیل نشود مپ کردن کارمزد های پلکان به مشکل می خورد
/// </summary>
public sealed record TieredCommission(
    decimal FromAmount,
    decimal? ToAmount,
    decimal Percentage,
    decimal? MinAmount,
    decimal? MaxAmount)
{
    public decimal FromAmount { get; private set; } = FromAmount;

    public decimal? ToAmount { get; private set; } = ToAmount;

    public decimal Percentage { get; private set; } = Percentage;

    public decimal? MinAmount { get; private set; } = MinAmount;

    public decimal? MaxAmount { get; private set; } = MaxAmount;

    public void Update(decimal percentage, decimal? minAmount, decimal? maxAmount)
    {
        Percentage = percentage;
        MinAmount = minAmount;
        MaxAmount = maxAmount;
    }

    public static void ValidateInputList(List<TieredCommission> tieredCommissions)
    {
        tieredCommissions = tieredCommissions.OrderBy(x => x.FromAmount).ToList();

        ValidateFirstAndLastIntervals(tieredCommissions);
        ValidateIndividualIntervals(tieredCommissions);
        ValidateIntervalsContinuity(tieredCommissions);
    }

    private static void ValidateFirstAndLastIntervals(List<TieredCommission> tieredCommissions)
    {
        if (tieredCommissions.Count < 2)
            throw new ArgumentValidationException("TieredCommissions", "انتخاب حداقل دو کارمزد اجباریست.");

        if (tieredCommissions.First().FromAmount != 0)
            throw new ArgumentValidationException("TieredCommissions", "مبلغ وارد شده در اولین بازه کارمزد پلکانی نامعتبر است.");

        if (tieredCommissions.Last().ToAmount.HasValue)
            throw new ArgumentValidationException("TieredCommissions", "مبلغ وارد شده در آخرین بازه کارمزد پلکانی نامعتبر است.");
    }

    private static void ValidateIndividualIntervals(List<TieredCommission> tieredCommissions)
    {
        foreach (var tieredCommission in tieredCommissions)
        {
            if (tieredCommission.FromAmount < 0 || tieredCommission.ToAmount.HasValue && tieredCommission.ToAmount < 0)
                throw new ArgumentValidationException("TieredCommissions", "مبلغ وارد شده در کارمزد پلکانی نامعتبر است.");

            if (tieredCommission.ToAmount.HasValue && tieredCommission.ToAmount < tieredCommission.FromAmount)
                throw new ArgumentValidationException("TieredCommissions", "مبلغ وارد شده در کارمزد پلکانی نامعتبر است.");

            if (tieredCommission.Percentage < 0)
                throw new ArgumentValidationException("TieredCommissions.Percentage", "درصد کارمزد پلکانی نامعتبر است.");

            if (tieredCommission.MinAmount.HasValue && tieredCommission.MinAmount < 0)
                throw new ArgumentValidationException("TieredCommissions.MinAmount", "حداقل کارمزد هر تراکنش نامعتبر است.");

            if (tieredCommission.MaxAmount.HasValue && tieredCommission.MaxAmount < 0)
                throw new ArgumentValidationException("TieredCommissions.MaxAmount", "حداکثر کارمزد هر تراکنش نامعتبر است.");
        }
    }

    private static void ValidateIntervalsContinuity(List<TieredCommission> tieredCommissions)
    {
        for (int i = 0; i < tieredCommissions.Count - 1; i++)
        {
            if (!tieredCommissions[i].ToAmount.HasValue)
                throw new ArgumentValidationException("TieredCommissions", "مبلغ وارد شده در بازه کارمزد پلکانی نامعتبر است.");

            if (tieredCommissions[i].ToAmount >= tieredCommissions[i + 1].FromAmount || Math.Abs(tieredCommissions[i].ToAmount.Value - tieredCommissions[i + 1].FromAmount) > 1)
                throw new ArgumentValidationException("TieredCommissions", "مبالغ موجود در کارمزد پلکانی همپوشانی دارند.");
        }
    }
}