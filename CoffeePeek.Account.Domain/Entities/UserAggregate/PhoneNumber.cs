using System.Text.RegularExpressions;
using CoffeePeek.Shared.Extensions.Exceptions;

namespace CoffeePeek.Account.Domain.Entities.UserAggregate;

public partial record PhoneNumber
{
    private static readonly Regex PhoneRegex = PhoneNumberRegex();

    public string Value { get; init; }

    private PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Phone number cannot be empty.");

        var cleanedValue = EmptyPhoneRegex().Replace(value, "");
        if (!cleanedValue.StartsWith("+")) cleanedValue = "+" + cleanedValue;

        if (!PhoneRegex.IsMatch(cleanedValue))
            throw new DomainException("Invalid phone number format.");

        Value = cleanedValue;
    }

    public static PhoneNumber Create(string value) => new(value);
    
    public string GetBelarusianOperator()
    {
        if (!Value.StartsWith("+375")) return "International / Unknown";

        var prefix = Value.Substring(4, 2);
        
        return prefix switch
        {
            "29" => Value[6] == '2' || Value[6] == '5' || Value[6] == '7' || Value[6] == '8' ? "A1" : "MTS",
            "44" => "A1",
            "33" => "MTS",
            "25" => "Life",
            "17" => "Landline (Minsk)",
            _ => "Other Belarusian"
        };
    }

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
    public override string ToString() => Value;
    [GeneratedRegex(@"^\+?[1-9]\d{1,14}$", RegexOptions.Compiled)]
    private static partial Regex PhoneNumberRegex();
    [GeneratedRegex(@"[\s\-\(\)]")]
    private static partial Regex EmptyPhoneRegex();
}