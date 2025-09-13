using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum OffsetCommissionShareType
    {
        [Description("امکان تهاتر سهم پذیرنده با کارمزد وجود داشته باشد")]
        Allowed = 1,

        [Description("امکان تهاتر سهم پذیرنده با کارمزد وجود نداشته باشد")]
        NotAllowed = 2
    }
}
