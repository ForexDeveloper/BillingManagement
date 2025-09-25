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
        List<DateTime> installmentDates = [];

        PersianCalendar pc = new();
        var persianYear = pc.GetYear(depositDate);
        var persianMonth = pc.GetMonth(depositDate);
        var persianDay = pc.GetDayOfMonth(depositDate);

        var installmentDate = depositDate;

        if (installmentBreakType is not null && installmentBreak is not null && installmentBreakType is > 0 && installmentBreak is > 0)
        {
            installmentDate = installmentBreakType switch
            {
                TimeInterval.Day => pc.AddDays(new DateTime(persianYear, persianMonth, persianDay, pc),
                    installmentBreak.Value),

                TimeInterval.Week => pc.AddWeeks(new DateTime(persianYear, persianMonth, persianDay, pc),
                    installmentBreak.Value),

                TimeInterval.Month => pc.AddMonths(new DateTime(persianYear, persianMonth, persianDay, pc),
                    installmentBreak.Value),

                _ => depositDate
            };
        }

        for (var i = 0; i < numberOfInstallments; i++)
        {
            var year = pc.GetYear(installmentDate);
            var month = pc.GetMonth(installmentDate);
            var day = pc.GetDayOfMonth(installmentDate);

            switch (periodType)
            {
                case TimeInterval.Day:

                    installmentDate = pc.AddDays(new DateTime(year, month, day, pc), billingPeriod);

                    break;

                case TimeInterval.Week:

                    installmentDate = pc.AddWeeks(new DateTime(year, month, day, pc), 1);

                    break;

                case TimeInterval.Month:

                    installmentDate = pc.AddMonths(new DateTime(year, month, day, pc), 1);

                    var regulatedDateString = GetRegulatedDateString(installmentDate, billingPeriod);

                    installmentDate = ConvertPersianToGregorian(regulatedDateString);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            installmentDates.Add(installmentDate);
        }

        return installmentDates;
    }

    public static string GetRegulatedDateString(DateTime date, int periodOfMonth)
    {
        var pc = new PersianCalendar();

        var year = pc.GetYear(date);
        var month = pc.GetMonth(date);

        periodOfMonth = RegulateBillingPeriod(year, month, periodOfMonth);

        return $"{year}/{month:00}/{periodOfMonth:00}";
    }

    public static int RegulateBillingPeriod(int year, int month, int billingPeriod)
    {
        PersianCalendar pc = new();

        var daysInMonth = pc.GetDaysInMonth(year, month);

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