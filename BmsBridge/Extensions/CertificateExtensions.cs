using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;

public static class CertificateExtensions
{
    public static void AddCertificateSource(this IServiceCollection services, IHostEnvironment env)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            services.AddSingleton<ICertificateSource>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<AzureSettings>>();
                return new StoreCertificateSource(options);
            });
        }
        else
        {
            var certPath = Path.Combine(
                env.ContentRootPath,
                "DevelopmentKeys",
                "CertificateTest.pfx"
            );

            services.AddSingleton<ICertificateSource>(
                _ => new PfxCertificateSource(certPath)
            );
        }
    }
}
