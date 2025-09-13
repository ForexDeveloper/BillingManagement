using System.ComponentModel;

namespace Domain.Core.Enums;
public enum FinancierCreditFlowEnum
{
    [Description("فرآیند عمومی")]
    General = 0,

    [Description("فرآیند تسهیلات خرد بلو")]
    BluBankRetailLoan = 1
}