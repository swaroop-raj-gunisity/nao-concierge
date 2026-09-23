using Microsoft.Extensions.Configuration;
using NaoConcierge.Application.Interfaces;

namespace NaoConcierge.Infrastructure.Secrets;

public class UserSecretsProvider : ISecretsProvider
{
    private readonly IConfiguration _configuration;

    public UserSecretsProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<string?> GetSecretAsync(string key, CancellationToken ct = default)
    {
        return Task.FromResult(_configuration[key]);
    }
}
