using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum ContractSubjectType : byte
    {
        [Description("زیر ساخت")]
        Core = 1,
        [Description("وایت لیبل")]
        WhiteLabel = 2
    }
}
