using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum CommissionDeductionMethodType
    {
        [Description("مبلغ کارمزد هر دوره از قسط اول کسر شود")]
        DeductFromFirstInstallment = 1,

        [Description("مبلغ کارمزد هر دوره به صورت مساوی از اقساط کسر شود")]
        DeductEquallyFromInstallments = 2
    }
}
