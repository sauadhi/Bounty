using System.Security.Cryptography;
using System.Text;

public static class OwnerKey
{
    // For the hackathon, hardcode. Later: move to appsettings.json (OwnerKey:Pepper).
    private const string Pepper = "ItsDexOWnerA1234";

    public static string DeriveManagerId(string secret)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes($"{secret}:{Pepper}"));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}