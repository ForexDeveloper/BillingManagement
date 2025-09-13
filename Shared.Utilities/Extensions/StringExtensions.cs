using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Shared.Utilities.Extensions
{
    public static class StringExtensions
    {
        public static List<int> ToListInt(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string pattern = @"^\d+(,\d+)*$";
            bool isValid = Regex.IsMatch(value, pattern);
            if (!isValid)
                return null;

            var cats = value?.Split(',');
            return cats?.Length > 0 ? cats.Select(int.Parse).ToList() : null;
        }
    }
}
