using System.Security.Cryptography;

namespace CoffeePeek.Infrastructure.Services.User;

public class HashingService : IHashingService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 10000;

    public string HashString(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize
        );

        var hashBytes = new byte[SaltSize + KeySize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyHashedStrings(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        var hashBytes = Convert.FromBase64String(hashedPassword);

        if (hashBytes.Length != SaltSize + KeySize)
            return false;

        var salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        var expectedHash = new byte[KeySize];
        Array.Copy(hashBytes, SaltSize, expectedHash, 0, KeySize);

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize
        );

        return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
    }

    public string GenerateRandomPassword(int length = 12)
    {
        if (length < 8)
            throw new ArgumentException("Password length should be at least 8 characters.", nameof(length));

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-_=+";
        var random = new char[length];
        using var rng = RandomNumberGenerator.Create();

        for (var i = 0; i < length; i++)
        {
            var byteBuffer = new byte[1];
            rng.GetBytes(byteBuffer);
            random[i] = chars[byteBuffer[0] % chars.Length];
        }

        return new string(random);
    }
}