using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Enums
{
    public enum CashOutRequestRejectReason:byte
    {
        [Description("حساب مسدود بدون قابلیت واریز")]
        BlockedAccountWithoutDepositCapability = 1,

        [Description("حساب بسته است")]
        AccountIsClosed = 2,

        [Description("حساب راکد است")]
        AccountIsDormant = 3,

        [Description("متقاضی فاقد حساب می‌باشد")]
        ApplicantDoesNotHaveAccount = 4,
        
        [Description("شماره شبا مشتری در لیست سیاه است")]
        IbanIsBlackListed = 5,

    }
}
