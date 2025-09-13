using System.ComponentModel;

namespace Domain.Core.Enums;

public enum PaymentGatewayType : byte
{
    [Description("Ipg")]
    Ipg = 1,

    [Description("Cpg")]
    Cpg = 2,

    [Description("Opg")]
    Opg = 3
}
