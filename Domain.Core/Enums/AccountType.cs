using System.ComponentModel;

namespace Domain.Core.Enums;

public enum AccountType : byte
{
    [Description("کارمزد")]
    Commission = 1,
    [Description("خرید")]
    Purchase = 2,
    [Description("کیف پول نقدی")]
    CashWallet = 3,
    [Description("تسهیلات")]
    Loan = 4,
    [Description("BNPL")]
    BNPL = 5,
    [Description("بهره")]
    Interest = 6,
    [Description("جریمه")]
    Penalty = 7,
    [Description("بانک")]
    Bank = 8,
    [Description("کیف پول تسهیلاتی")]
    LoanWallet = 9
}
