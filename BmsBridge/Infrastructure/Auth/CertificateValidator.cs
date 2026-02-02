using System.Security.Cryptography.X509Certificates;

public static class CertificateValidator
{
    public static bool IsValid(X509Certificate2 cert)
    {
        var now = DateTime.UtcNow;
        return cert.NotBefore <= now && cert.NotAfter >= now;
    }
}
