using Serilog.Enrichers.Sensitive;
using System.Text.RegularExpressions;

namespace Shared.Logging.Serilog.MaskOperations
{
    public class CustomIbanMaskingOperator : RegexMaskingOperator
    {
        private const string IbanReplacePattern = "[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{7}([a-zA-Z0-9]?){0,16}";

        public CustomIbanMaskingOperator() : base(IbanReplacePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)
        {
        }

        protected override string PreprocessMask(string mask, Match match)
        {
            if (match is null)
                throw new ArgumentNullException(nameof(match));

            int strLength = match.Value.Length;
            var start = match.Value.Substring(0, 4);
            var end = match.Value.Substring(strLength - 5, strLength - 1);
            return $"{start}********{end}";

        }
    }
}
