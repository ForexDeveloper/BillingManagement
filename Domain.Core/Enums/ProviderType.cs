using System.ComponentModel;

namespace Domain.Core.Enums;

public enum ProviderType : byte
{
    [Description("شاهکار")]
    Shahkar = 1,

    [Description("ثبت احوال")]
    CivilRegistration = 2,

    [Description("کد پستی")]
    PostalCode = 3,

    [Description("رتبه بانکی")]
    BankScore = 4,

    [Description("پیامک")]
    Sms = 5,

    [Description("ایمیل")]
    Email = 6,

    [Description("استعلام شبا")]
    Iban = 7,

    [Description("امضای الکترونیک")]
    DigitalSignature = 8,

    [Description("سفته الکترونیک")]
    DigitalPromissoryNote = 9
}