using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace TokenService.Services;

public class KeyVaultService
{
    private readonly SecretClient _secretClient;

    public KeyVaultService(Uri keyVaultUrl)
    {
        _secretClient = new SecretClient(keyVaultUrl, new DefaultAzureCredential());
    }

    public async Task<(string?, string?)> GetSecretAsync(string apiKey, string providerName)
    {
        await foreach (var secret in _secretClient.GetPropertiesOfSecretsAsync())
        {
            if (!secret.Name.Contains("ApiKey")) continue;

            var keyVaultSecret = await _secretClient.GetSecretAsync(secret.Name);
            if (keyVaultSecret.Value.Value == apiKey && keyVaultSecret.Value.Name == providerName) return (keyVaultSecret.Value.Value, providerName);
        }
        return (null, null);
    }
}