using System.Text.RegularExpressions;
using CoffeePeek.Shared.Extensions.Exceptions;

namespace CoffeePeek.Account.Domain.Entities.UserAggregate;

public partial record PhoneNumber
{
    private static readonly Regex PhoneRegex = BelarusPhoneRegex();

    public string Value { get; init; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Phone number cannot be empty.");

        var digitsOnly = OnlyDigitsRegex().Replace(value, "");

        string normalized;

        if (digitsOnly.StartsWith("80") && digitsOnly.Length == 11)
        {
            normalized = "+375" + digitsOnly[2..];
        }
        else if (digitsOnly.StartsWith("375") && digitsOnly.Length == 12)
        {
            normalized = "+" + digitsOnly;
        }
        else if (digitsOnly.Length == 9)
        {
            normalized = "+375" + digitsOnly;
        }
        else
        {
            normalized = "+" + digitsOnly;
        }

        if (!PhoneRegex.IsMatch(normalized))
            throw new DomainException("Invalid Belarusian phone number format. Expected 375XXXXXXXXX or 80XXXXXXXXX");

        return new PhoneNumber(normalized);
    }

    public string GetOperator()
    {
        var code = Value.Substring(4, 2);

        return code switch
        {
            "25" => "Life:)",
            "33" => "MTS",
            "44" => "A1",
            "17" => "Beltelecom (Minsk)",
            "29" => GetOperatorFor29(),
            _ => "Other/Landline"
        };
    }

    private string GetOperatorFor29()
    {
        var seventhDigit = Value[6];
        return seventhDigit switch
        {
            '2' or '5' or '7' or '8' => "A1",
            _ => "MTS"
        };
    }

    [GeneratedRegex(@"^\+375\d{9}$", RegexOptions.Compiled)]
    private static partial Regex BelarusPhoneRegex();

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex OnlyDigitsRegex();

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
    public override string ToString() => Value;
}