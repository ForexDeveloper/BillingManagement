using Serilog.Enrichers.Sensitive;
using System.Text.RegularExpressions;

namespace Shared.Logging.Serilog.MaskOperations
{
    public class GeneralRegexMaskingOperator : RegexMaskingOperator
    {
        public GeneralRegexMaskingOperator(string regex) : base(regex, RegexOptions.IgnoreCase | RegexOptions.Compiled)
        {
        }

        protected override string PreprocessMask(string mask, Match match)
        {
            if (match is null)
                throw new ArgumentNullException(nameof(match));

            return $"*";

        }
    }
}
