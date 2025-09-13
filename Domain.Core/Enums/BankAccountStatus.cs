using System.ComponentModel;

namespace Domain.Core.Enums;

public enum BankAccountStatus
{
    [Description("Unknow")]
    None=0,

    [Description("فعال")]
    Active=1,

    [Description("غیر فعال")]
    Inactive=2,
    
}