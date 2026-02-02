using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

public static class TestCertFactory
{
    public static X509Certificate2 CreateValid(string subject)
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(
            $"CN={subject}",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return req.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(1));
    }
}
