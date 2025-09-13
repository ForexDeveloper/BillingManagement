using System.ComponentModel;

namespace Domain.Core.Enums;

public enum SaleType : byte
{
    [Description("حضوری/آنلاین")]
    InPersonOnline = 1,
    [Description("آنلاین")]
    Online = 2,
    [Description("حضوری")]
    InPerson = 3
}
