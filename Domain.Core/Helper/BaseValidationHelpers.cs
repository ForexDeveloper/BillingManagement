using Domain.Core.Entities.Shared.Exceptions;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

public static class BaseValidationHelpers
{
    public static bool IsValidNationalId(string nationalId)
    {
        if (string.IsNullOrEmpty(nationalId))
            throw new ArgumentValidationException(nameof(nationalId), $"{nameof(nationalId)} is required");

        if (!Regex.IsMatch(nationalId, @"^\d{10}$"))
            return false;

        if (nationalId == "1111111111" ||
            nationalId == "0000000000" ||
            nationalId == "2222222222" ||
            nationalId == "3333333333" ||
            nationalId == "4444444444" ||
            nationalId == "5555555555" ||
            nationalId == "6666666666" ||
            nationalId == "7777777777" ||
            nationalId == "8888888888" ||
            nationalId == "9999999999")
            return false;

        var check = Convert.ToInt32(nationalId.Substring(9, 1));
        var sum = Enumerable.Range(0, 9)
            .Select(x => Convert.ToInt32(nationalId.Substring(x, 1)) * (10 - x))
            .Sum() % 11;

        return (sum < 2 && check == sum) || (sum >= 2 && check + sum == 11);
    }

    public static bool IsValidMobileNumber(string mobileNumber)
    {
        Regex regex = new Regex(@"^(?:0|98|\+98|\+980|0098|098|00980)?(9\d{9})$");
        if (!regex.IsMatch(mobileNumber))
            return false;

        return true;
    }

    public static bool IsValidUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }


    public static bool IsSafeData(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return true;
        }

        var regex = new Regex(@"^(?!.*(<[^>]+>|SELECT|INSERT|UPDATE|DELETE|DROP|--|#|\bEXEC\b|\bUNION\b|\bOR\b|\bAND\b)).*$");
        return regex.IsMatch(input);
    }

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        Regex regex = new Regex(emailPattern, RegexOptions.IgnoreCase);

        return regex.IsMatch(email);
    }

    public static bool IsImageFile(string filePath)
    {
        string pattern = @"^.*\.(jpg|jpeg|png|gif|bmp|tiff)$";
        return Regex.IsMatch(filePath, pattern, RegexOptions.IgnoreCase);
    }
    public static bool IsGuid(string guid)
    {
        return (Guid.TryParse(guid, out Guid _));
    }

    public static bool IsValidIban(this string iban)
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

    public static bool IsValidContractNumber(string contractNumber)
    {
        if (string.IsNullOrEmpty(contractNumber.Trim()) || contractNumber.Length > 30)
        {
            return false;
        }

        string pattern = @"^[\u0600-\u06FFa-zA-Z0-9\s/\-\\._|(),]+$";
        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

        return regex.IsMatch(contractNumber);
    }
}