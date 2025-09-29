using System.ComponentModel;

namespace Domain.Core.Enums;

public enum PaymentMethodType : byte
{
    [Description("چک")]
    Cheque = 1,

    [Description("واریز به حساب")]
    BankAccountDeposit = 2
}