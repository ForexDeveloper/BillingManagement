using System.ComponentModel;

namespace Domain.Core.Enums;

public enum PurchaseGatewayType : byte
{
    [Description("Cpg")]
    Cpg = 2,

    [Description("Opg")]
    Opg = 3
}
