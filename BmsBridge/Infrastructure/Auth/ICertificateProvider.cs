using System.Security.Cryptography.X509Certificates;

public interface ICertificateProvider
{
    X509Certificate2 GetCertificate();
}
