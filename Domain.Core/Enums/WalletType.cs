using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum WalletType : byte
    {
        [Description("تسهیلاتی")]
        Loan = 1,
        [Description("اعتباری")]
        LineOfCredit = 2,
        [Description("نقدی")]
        Cash = 3,
        [Description("بن کارت")]
        BonCard = 4, //eGift
        [Description("BNPL")]
        BNPL = 5,
        //[Description("قسط به ازای خرید")]
        //Installment = 6,
    }
}
