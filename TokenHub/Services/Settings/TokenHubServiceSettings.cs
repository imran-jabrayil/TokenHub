namespace TokenHub.Services.Settings;

public record TokenHubServiceSettings
{
    public required TimeSpan TokenExpirationPeriod { get; init; }
    public required string CacheKey { get; init; }
    public required string LockKey { get; init; }
    public required TimeSpan LockTimeout { get; init; }
}