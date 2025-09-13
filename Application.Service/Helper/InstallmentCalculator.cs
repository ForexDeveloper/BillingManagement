using System;

public class InstallmentCalculator
{
    public static decimal CalculatePMT(decimal creditAmount, decimal annualInterestRate, int numberOfInstallments)
    {
        decimal monthlyRate = (annualInterestRate / 100m) / 12m;

        if (monthlyRate == 0)
        {
            // No interest case
            return creditAmount / numberOfInstallments;
        }

        double factor = Math.Pow((double)(1 + monthlyRate), numberOfInstallments);
        decimal factorDecimal = (decimal)factor;

        decimal pmt = creditAmount * (monthlyRate * factorDecimal) / (factorDecimal - 1m);

        return pmt;
    }

    public static decimal CalculateInstallmentAmount(decimal creditAmount, int numberOfInstallments)
    {
        return creditAmount / numberOfInstallments;
    }

    public static decimal CalculateInterestAmount(decimal creditAmount, decimal annualInterestRate, int numberOfInstallments)
    {
        return CalculatePMT(creditAmount, annualInterestRate, numberOfInstallments) - CalculateInstallmentAmount(creditAmount, numberOfInstallments);
    }
}
