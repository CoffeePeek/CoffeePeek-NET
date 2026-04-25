using System.Collections;
using System.IO.Hashing;
using System.Text;

namespace CoffeePeek.Account.Application.Common;

public class EmailExistenceFilter
{
    private readonly BitArray _bits;
    private readonly int _numHashes;
    private readonly int _size;

    public EmailExistenceFilter(int expectedCount, double falsePositiveProbability)
    {
        _size = (int)-(expectedCount * Math.Log(falsePositiveProbability) / Math.Pow(Math.Log(2), 2));
        _numHashes = (int)Math.Max(1, Math.Round((double)_size / expectedCount * Math.Log(2)));
        _bits = new BitArray(_size);
    }

    private IEnumerable<int> GetIndices(string email)
    {
        var normalizedEmail = email.Trim().ToLower();
        var data = Encoding.UTF8.GetBytes(normalizedEmail);
        
        var h1 = XxHash32.HashToUInt32(data);
        var h2 = XxHash32.HashToUInt32(data, 12345);

        for (var i = 0; i < _numHashes; i++)
        {
            var combinedHash = Math.Abs(h1 + i * h2);
            yield return (int)(combinedHash % _size);
        }
    }

    public void Add(string email)
    {
        foreach (var index in GetIndices(email)) _bits.Set(index, true);
    }

    public bool MightExist(string email)
    {
        return GetIndices(email).All(index => _bits.Get(index));
    }
}