using System.Security.Cryptography.X509Certificates;

public sealed class CertificateProvider : ICertificateProvider
{
    private readonly IReadOnlyList<ICertificateSource> _sources;
    private readonly IErrorFileService _errorFileService;

    public CertificateProvider(IEnumerable<ICertificateSource> sources, IErrorFileService errorFileService)
    {
        _sources = sources.ToList();
        _errorFileService = errorFileService;
    }

    public X509Certificate2 GetCertificate()
    {
        foreach (var source in _sources)
        {
            try
            {
                var cert = source.Load();
                if (cert != null)
                    return cert;
            }
            finally
            {

            }
        }

        _errorFileService.CreateBlankAsync("CERT");
        throw new InvalidOperationException("No valid certificate found.");
    }
}
