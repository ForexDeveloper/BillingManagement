using Domain.Core.Enums;

namespace Application.Service.Helper;

public class CreditDetailsCalculator
{
    public static CreditAmountsDto CalculateCreditAmountIncludingOperationalFee(OperationalFeeType operationalFeeType, decimal? opertationalFee, decimal requestedCreditAmount)
    {
        decimal creditAmount = 0;
        decimal operationalFeeAmount = (opertationalFee is null or 0) ? 0 : requestedCreditAmount * opertationalFee.Value / 100;

        switch (operationalFeeType)
        {
            case OperationalFeeType.DeductFromLoan:
            case OperationalFeeType.OnlinePayment:
                creditAmount = requestedCreditAmount;
                break;

            case OperationalFeeType.AddToLoanInstallment:
                creditAmount = requestedCreditAmount + operationalFeeAmount;
                break;
        }

        return new CreditAmountsDto
        {
            CreditAmount = creditAmount,
            OperationalFeeAmount = operationalFeeAmount
        };
    }

    public static CreditDetailsDto Calculate(decimal creditAmount, int numberOfInstallments, decimal? interestPercent)
    {
        interestPercent = (interestPercent is null or 0) ? 0 : interestPercent;
        decimal installmentAndInterestAmount = InstallmentCalculator.CalculatePMT(creditAmount, interestPercent.Value, numberOfInstallments);
        decimal totalRepayableAmount = RoundHelper.RoundAmount(installmentAndInterestAmount * numberOfInstallments);
        decimal installmentAmount = InstallmentCalculator.CalculateInstallmentAmount(creditAmount, numberOfInstallments);
        decimal interestAmount = installmentAndInterestAmount - installmentAmount;

        var totalInterestAmount = RoundHelper.RoundAmount(interestAmount * numberOfInstallments);

        decimal firstInstallmentAmount = RoundHelper.TruncateAmount(installmentAmount);
        decimal lastInstallmentAmount = totalRepayableAmount - totalInterestAmount - (firstInstallmentAmount * (numberOfInstallments - 1));

        decimal firstInterestAmount = RoundHelper.TruncateAmount(interestAmount);
        decimal lastInterestAmount = totalInterestAmount - (firstInterestAmount * (numberOfInstallments - 1));

        return new CreditDetailsDto()
        {
            InstallmentAmount = firstInstallmentAmount,
            InterestAmount = firstInterestAmount,
            LastInstallmentAmount = lastInstallmentAmount,
            LastInterestAmount = lastInterestAmount,
            NumberOfInstallments = numberOfInstallments,
            RepayableCreditAmount = totalRepayableAmount,
            CreditAmount = creditAmount,
        };
    }
}
