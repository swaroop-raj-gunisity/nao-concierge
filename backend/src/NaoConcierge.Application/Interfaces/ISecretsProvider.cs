namespace NaoConcierge.Application.Interfaces;

public interface ISecretsProvider
{
    Task<string?> GetSecretAsync(string key, CancellationToken ct = default);
}
