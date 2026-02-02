using System.Security.Cryptography.X509Certificates;

public sealed class FakeSource : ICertificateSource
{
    private readonly X509Certificate2? _cert;

    public FakeSource(X509Certificate2? cert)
    {
        _cert = cert;
    }

    public X509Certificate2? Load() => _cert;
}
