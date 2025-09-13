using Serilog.Enrichers.Sensitive;
using System.Text.RegularExpressions;

namespace Shared.Logging.Serilog.MaskOperations
{

    public class PanMaskingOperator : RegexMaskingOperator
    {
        private const string PanPattern = @"[0-9]{16,19}";
        public PanMaskingOperator() : base(PanPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)
        {
        }

        protected override string PreprocessMask(string mask, Match match)
        {
            if (match is null)
                throw new ArgumentNullException(nameof(match));

            int strLength = match.Value.Length;
            var start = match.Value.Substring(0, 4);
            var end = match.Value.Substring(strLength - 4);
            return $"{start}********{end}";

        }
    }
}
