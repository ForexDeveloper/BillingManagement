using FluentValidation;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Shared.FluentValidation
{
    public abstract class BaseValidator<T> : AbstractValidator<T>
    {
        protected bool IsValidAccountNumber(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber))
                return false;

            if (accountNumber.Length > 50)
                return false;

            return true;
        }

        protected bool IsValidPan(string pan)
        {
            if (string.IsNullOrEmpty(pan))
                return false;

            if (pan.Length != 16)
                return false;

            if (pan.Any(panDigit => !char.IsDigit(panDigit)))
                return false;

            var checkSum = 0;
            for (short i = 0; i < pan.Length; i++)
                if (i % 2 == 0)
                {
                    var sum = short.Parse(pan[i].ToString(), CultureInfo.InvariantCulture) * 2;
                    if (sum > 9)
                        sum = sum - 9;

                    checkSum += sum;
                }

                else
                {
                    checkSum += short.Parse(pan[i].ToString(), CultureInfo.InvariantCulture);
                }

            if (checkSum % 10 != 0)
                return false;

            return true;
        }

        protected bool IsValidMobileNumber(string mobileNumber)
        {
            var pattern = @"09\d{9}";
            return Regex.IsMatch(mobileNumber, pattern);
        }

        protected bool IsValidWalletId(string walletId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(walletId))
                    return false;

                if (!walletId.StartsWith("09", StringComparison.InvariantCulture))
                    return false;

                if (walletId.Length != 11)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        protected bool IsValidIban(string iban)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(iban))
                    return false;

                if (iban.Length != 26)
                    return false;

                if (iban.Substring(0, 2).ToUpper(CultureInfo.InvariantCulture) != "IR")
                    return false;

                var ibanDigits = iban.Substring(2, iban.Length - 2);

                if (ibanDigits.Any(ibanDigit => !char.IsDigit(ibanDigit)))
                    return false;

                var pureIban = string.Concat(iban.AsSpan(4, iban.Length - 4), iban.Substring(0, 4).Replace("IR", "1827", StringComparison.InvariantCulture));
                var checksum = (short)(decimal.Parse(pureIban, CultureInfo.InvariantCulture) % 97);

                if (checksum != 1)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
