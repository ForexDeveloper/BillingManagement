using System;
using Domain.Core.Enums;
using System.Globalization;
using System.Collections.Generic;

namespace Application.Service.Helper;

public static class DateHelper
{
    public static List<DateTime> CalculateInstallments(DateTime depositDate, int? numberOfInstallments,
       TimeInterval? installmentBreakType, int? installmentBreak, int billingPeriod, TimeInterval periodType)
    {
        PersianCalendar pc = new();

        List<DateTime> installmentDates = [];

        var installmentDate = depositDate.Date;

        if (installmentBreakType.HasValue && installmentBreak > 0)
        {
            installmentDate = installmentBreakType switch
            {
                TimeInterval.Day => pc.AddDays(installmentDate, installmentBreak.Value),

                TimeInterval.Week => pc.AddWeeks(installmentDate, installmentBreak.Value),

                TimeInterval.Month => pc.AddMonths(installmentDate, installmentBreak.Value),

                _ => depositDate
            };
        }

        for (var i = 0; i < numberOfInstallments; i++)
        {
            switch (periodType)
            {
                case TimeInterval.Day:

                    installmentDate = pc.AddDays(installmentDate, billingPeriod);

                    break;

                case TimeInterval.Week:

                    installmentDate = pc.AddWeeks(installmentDate, 1);

                    break;

                case TimeInterval.Month:

                    installmentDate = pc.AddMonths(installmentDate, 1);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            installmentDates.Add(installmentDate);
        }

        return installmentDates;
    }

    public static int RegulateBillingPeriod(int daysInMonth, int billingPeriod)
    {
        if (daysInMonth == 29 && (billingPeriod == 30 || billingPeriod == 31))
        {
            billingPeriod = 29;
        }
        else if (daysInMonth == 30 && billingPeriod == 31)
        {
            billingPeriod = 30;
        }

        return billingPeriod;
    }

    private static DateTime AdjustPeriodOnBillingPeriod(PersianCalendar pc, DateTime dateTime, int billingPeriod)
    {
        var year = pc.GetYear(dateTime);
        var month = pc.GetMonth(dateTime);
        var dayOfMonth = pc.GetDayOfMonth(dateTime);

        if (dayOfMonth is 29 or 30 && (billingPeriod is 30 or 31))
        {
            dateTime = pc.ToDateTime(year, month, billingPeriod, 0, 0, 0, 0);
        }

        return dateTime;
    }

    private static DateTime ConvertPersianToGregorian(string? persianDate)
    {
        if (string.IsNullOrEmpty(persianDate))
            throw new Exception("Invalid persianDate");

        PersianCalendar pc = new();

        var persianDateParts = persianDate.Split("/");

        int year = Convert.ToInt32(persianDateParts[0]);
        int month = Convert.ToInt32(persianDateParts[1]);
        int day = Convert.ToInt32(persianDateParts[2]);

        return pc.ToDateTime(year, month, day, 0, 0, 0, 0);
    }
}