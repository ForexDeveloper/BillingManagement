using System.ComponentModel;

namespace Domain.Core.Enums;

public enum FinancialDocumentState : byte
{
    [Description("ثبت شده")]
    Registered = 1,

    [Description("تایید شده")]
    Verified = 2,

    [Description("تسویه شده")]
    Completed = 3,

    [Description("لغو شده")]
    Reverse = 4,
}
