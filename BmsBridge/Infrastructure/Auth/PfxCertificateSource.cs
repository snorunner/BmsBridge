using System.Security.Cryptography.X509Certificates;

public sealed class PfxCertificateSource : ICertificateSource
{
    private readonly string _path;
    private readonly string? _password;

    public PfxCertificateSource(string path, string? password = null)
    {
        _path = path;
        _password = password;
    }

    public X509Certificate2? Load()
    {
        if (!File.Exists(_path))
            return null;

        var cert = X509CertificateLoader.LoadPkcs12(
            File.ReadAllBytes(_path),
            _password,
            X509KeyStorageFlags.MachineKeySet |
            X509KeyStorageFlags.Exportable |
            X509KeyStorageFlags.EphemeralKeySet
        );

        return CertificateValidator.IsValid(cert) ? cert : null;
    }
}
