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

    public static int RegulateBillingPeriod(PersianCalendar pc, int year, int month, int billingPeriod)
    {
        var daysInMonth = pc.GetDaysInMonth(year, month);

        if (daysInMonth == 29 && billingPeriod is 30 or 31)
        {
            billingPeriod = 29;
        }

        else if (daysInMonth == 30 && billingPeriod == 31)
        {
            billingPeriod = 30;
        }

        return billingPeriod;
    }

    public static DateTime RegulateDateOfPeriod(PersianCalendar pc, DateTime dateOfPeriod, int billingPeriod)
    {
        var year = pc.GetYear(dateOfPeriod);
        var month = pc.GetMonth(dateOfPeriod);
        var dayOfMonth = pc.GetDayOfMonth(dateOfPeriod);
        var daysInMonth = pc.GetDaysInMonth(year, month);

        switch (daysInMonth)
        {
            case 30:
            {
                if (dayOfMonth is 29 && billingPeriod is 30)
                { 
                    dateOfPeriod = pc.ToDateTime(year, month, 30, 0, 0, 0, 0);
                }

                break;
            }

            case 31:
            {
                if (dayOfMonth is 29 && billingPeriod is 30)
                { 
                    dateOfPeriod = pc.ToDateTime(year, month, 30, 0, 0, 0, 0);
                }

                if (dayOfMonth is 29 or 30 && billingPeriod is 31)
                { 
                    dateOfPeriod = pc.ToDateTime(year, month, 31, 0, 0, 0, 0);
                }

                break;
            }
        }

        return dateOfPeriod;
    }
}