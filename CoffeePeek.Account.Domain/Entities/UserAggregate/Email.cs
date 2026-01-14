using System.Text.RegularExpressions;
using CoffeePeek.Shared.Extensions.Exceptions;

namespace CoffeePeek.Account.Domain.Entities.UserAggregate;

public partial record Email
{
    private static readonly Regex EmailRegex = MyRegex();

    public string Value { get; init; }

    private Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty.");

        value = value.Trim().ToLowerInvariant();

        if (value.Length > 255)
            throw new DomainException("Email is too long.");

        if (!EmailRegex.IsMatch(value))
            throw new DomainException("Invalid email format.");

        Value = value;
    }

    public static Email Create(string value) => new(value);

    public static implicit operator string(Email email) => email.Value;

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@(?:[a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex MyRegex();
}