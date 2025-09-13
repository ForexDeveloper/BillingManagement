using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Enums
{
    public enum InstallmentPaymentMethodType : byte
    {
        [Description("مشتری")]
        Customer = 1,
        [Description("مشتری-سازمان")]
        CustomerOfOrganization = 2,
        [Description("فقط سازمان")]
        OrganizationOnly = 3,
        
    }
}
