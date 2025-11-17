using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rest.Integration.Tests.Base
{
    public static class UtilityExtension
    {
        public static StringContent ToStringContent(this object model)
        {
            var json = JsonSerializer.Serialize(model);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        public static string GenerateUniqueInvoiceNumber()
        {
            return $"int-test-{DateTime.UtcNow:HHmmssfff}";
        }

    }
}
