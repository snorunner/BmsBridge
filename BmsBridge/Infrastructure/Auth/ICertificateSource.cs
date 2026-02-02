using System.Security.Cryptography.X509Certificates;

public interface ICertificateSource
{
    X509Certificate2? Load();
}
