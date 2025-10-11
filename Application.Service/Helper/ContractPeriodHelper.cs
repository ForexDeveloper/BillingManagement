using System;
using Domain.Core.Enums;
using System.Globalization;

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
                    startOfPeriod = DateTime.MaxValue;
                    endOfPeriod = DateTime.MaxValue;
                    break;
                }

                var originDate = dailyBillingOriginDate.Value;

                if (specificDate.Date < originDate.Date)
                {
                    startOfPeriod = DateTime.MaxValue;
                    endOfPeriod = DateTime.MaxValue;
                    break;
                }

                var totalDays = (specificDate.Date - originDate.Date).Days;

                difference = billingPeriod - (totalDays % billingPeriod);

                startOfPeriod = pc.AddDays(specificDate, -difference);

                endOfPeriod = pc.AddDays(startOfPeriod, billingPeriod);

                break;

            case TimeInterval.Week:

                if ((DayOfWeek)billingPeriod >= dayOfWeek)
                {
                    difference = 7 - (billingPeriod - (int)dayOfWeek);
                }
                else
                {
                    difference = (int)dayOfWeek - billingPeriod;
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