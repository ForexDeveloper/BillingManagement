using System;
using Domain.Core.Enums;
using System.Globalization;
using Domain.Core.Entities.Shared.Exceptions;

namespace Application.Service.Helper;

public static class ContractPeriodHelper
{
    public static (DateTime StartOfPeriod, DateTime EndOfPeriod) GetPeriodBySpecificDate(int billingPeriod,
        TimeInterval billingPeriodType, DateTime? dailyBillingOriginDate, DateTime specificDate)
    {
        int difference;
        DateTime startOfPeriod;
        DateTime endOfPeriod;

        var pc = new PersianCalendar();

        var year = pc.GetYear(specificDate);
        var month = pc.GetMonth(specificDate);
        var dayOfWeek = pc.GetDayOfWeek(specificDate);
        var dayOfMonth = pc.GetDayOfMonth(specificDate);

        switch (billingPeriodType)
        {
            case TimeInterval.Day:

                if (!dailyBillingOriginDate.HasValue)
                {
                    throw new ArgumentValidationException(nameof(dailyBillingOriginDate),
                        "تاریخ شروع صورتحساب روزانه مقدار ندارد");
                }

                var originDate = dailyBillingOriginDate.Value;

                if (specificDate.Date < originDate.Date)
                {
                    throw new ArgumentValidationException(nameof(dailyBillingOriginDate),
                        $"تاریخ شروع صورتحساب روزانه {specificDate} از بزرگتر است");
                }

                var totalDays = (specificDate.Date - originDate.Date).Days;

                difference = totalDays % billingPeriod;

                startOfPeriod = difference == 0
                    ? pc.AddDays(specificDate, -billingPeriod)
                    : pc.AddDays(specificDate, -difference);

                endOfPeriod = pc.AddDays(startOfPeriod, billingPeriod);

                break;

            case TimeInterval.Week:

                var persianDayOfWeek = DateHelper.GetPersianDayOfWeek(billingPeriod);

                if (persianDayOfWeek >= dayOfWeek)
                {
                    difference = 7 - (persianDayOfWeek - dayOfWeek);
                }
                else
                {
                    difference = dayOfWeek - persianDayOfWeek;
                }

                startOfPeriod = pc.AddDays(specificDate, -difference);

                endOfPeriod = pc.AddWeeks(startOfPeriod, 1);

                break;

            case TimeInterval.Month:

                var regulatePeriod = DateHelper.RegulateBillingPeriod(pc, year, month, billingPeriod);

                if (regulatePeriod >= dayOfMonth)
                {
                    startOfPeriod = pc.AddMonths(new DateTime(year, month, regulatePeriod, pc), -1);

                    startOfPeriod = DateHelper.RegulateDateOfPeriod(pc, startOfPeriod, billingPeriod);
                }
                else
                {
                    startOfPeriod = pc.ToDateTime(year, month, billingPeriod, 0, 0, 0, 0);
                }

                endOfPeriod = pc.AddMonths(startOfPeriod, 1);

                endOfPeriod = DateHelper.RegulateDateOfPeriod(pc, endOfPeriod, billingPeriod);

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return new ValueTuple<DateTime, DateTime>(startOfPeriod, endOfPeriod);
    }

    public static (DateTime StartOfPeriod, DateTime EndOfPeriod) GetPeriodByLastBillingDueDate(int billingPeriod,
        TimeInterval billingPeriodType, DateTime? dailyBillingOriginDate, DateTime lastBillingDueDate)
    {
        DateTime startOfPeriod;
        DateTime endOfPeriod;

        var pc = new PersianCalendar();

        switch (billingPeriodType)
        {
            case TimeInterval.Day:

                startOfPeriod = lastBillingDueDate;

                endOfPeriod = pc.AddDays(startOfPeriod, billingPeriod);

                break;

            case TimeInterval.Week:

                startOfPeriod = lastBillingDueDate;

                endOfPeriod = pc.AddWeeks(startOfPeriod, 1);

                break;

            case TimeInterval.Month:

                startOfPeriod = lastBillingDueDate;

                endOfPeriod = pc.AddMonths(startOfPeriod, 1);

                endOfPeriod = DateHelper.RegulateDateOfPeriod(pc, endOfPeriod, billingPeriod);

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return new ValueTuple<DateTime, DateTime>(startOfPeriod, endOfPeriod);
    }
}