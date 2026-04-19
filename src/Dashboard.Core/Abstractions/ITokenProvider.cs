namespace Dashboard.Core.Abstractions;

public interface ITokenProvider
{
    Task<string?> GetNotionTokenAsync(CancellationToken ct = default);

    Task SetNotionTokenAsync(string token, CancellationToken ct = default);

    Task ClearAsync(CancellationToken ct = default);
}
