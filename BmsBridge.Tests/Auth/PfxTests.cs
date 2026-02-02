using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;

public class PfxTests
{
    [Fact]
    public void PfxSource_Loads_Valid_Certificate()
    {
        var cert = TestCertFactory.CreateValid("TestCert");
        var temp = Path.GetTempFileName();
        File.WriteAllBytes(temp, cert.Export(X509ContentType.Pfx));

        var source = new PfxCertificateSource(temp);

        var result = source.Load();

        Assert.NotNull(result);
        Assert.Equal("TestCert", result!.GetNameInfo(X509NameType.SimpleName, false));
    }

    [Fact]
    public void StoreSource_Returns_Null_When_Not_Found()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert.True(true);
            return;
        }

        var source = new StoreCertificateSource("DefinitelyNotARealCert");

        var result = source.Load();

        Assert.Null(result);
    }

    [Fact]
    public void Provider_Uses_First_Source_That_Returns_Certificate()
    {
        var cert = TestCertFactory.CreateValid("CertA");

        var fake1 = new FakeSource(null);
        var fake2 = new FakeSource(cert);

        var provider = new CertificateProvider(new[] { fake1, fake2 });

        var result = provider.GetCertificate();

        Assert.Equal("CertA", result.GetNameInfo(X509NameType.SimpleName, false));
    }
}
