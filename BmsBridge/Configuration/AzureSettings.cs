public record AzureSettings
{
    public string tenant_id { get; init; } = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";
    public string client_id { get; init; } = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";
    public string device_id { get; init; } = "my-device-name";
    public string scope_id { get; init; } = "XXXXXXXXXXX";
    public string secret_name { get; init; } = "my-vault-secret";
    public string vault_name { get; init; } = "my-vault";
    public string certificate_subject { get; init; } = "my-certificate-subject";
    public string sas_ttl_days { get; init; } = "90";
}
