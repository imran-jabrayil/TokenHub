using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TokenHub.Services.Abstractions;
using TokenHub.Services.Settings;

namespace TokenHub.Services;

public class TokenHubService(
    IOptions<TokenHubServiceSettings> settings, 
    IConnectionMultiplexer redis,
    ILogger<TokenHubService> logger) : ITokenHubService
{
    private readonly TokenHubServiceSettings _settings = Guard.Against.Null(settings.Value, nameof(settings));
    private readonly IConnectionMultiplexer _redis = Guard.Against.Null(redis, nameof(redis));
    private readonly ILogger<TokenHubService> _logger = Guard.Against.Null(logger, nameof(logger));
    private readonly string _instanceName = Guid.NewGuid().ToString();
    
    public async Task<string?> GetAsync()
    {
        IDatabaseAsync db = _redis.GetDatabase();
        
        RedisValue cachedData = await db.StringGetAsync(_settings.CacheKey);

        if (cachedData.HasValue)
        {
            _logger.LogInformation("token retrieved from cache");
            return cachedData.ToString();
        }
        
        _logger.LogInformation("token expired, updating token");

        if (!await db.LockTakeAsync(_settings.LockKey, _instanceName, _settings.LockTimeout))
        {
            _logger.LogInformation("Background refresh: Could not acquire lock (another instance likely handled it). LockKey: {LockKey}. Instance {InstanceId}", 
                _settings.LockKey, _instanceName);
            return null;
        }
        
        _logger.LogInformation("lock acquired");

        try
        {
            cachedData = await db.StringGetAsync(_settings.CacheKey);
            if (cachedData.HasValue)
            {
                _logger.LogInformation(
                    "Data found in cache after acquiring lock (another instance refreshed). Key: {CacheKey}",
                    _settings.CacheKey);
                return cachedData.ToString();
            }

            var newToken = Guid.NewGuid().ToString();
            _logger.LogWarning(
                "Failed to fetch new data from external API. Cache not updated by instance {InstanceId}.",
                _instanceName);
            await db.StringSetAsync(_settings.CacheKey, newToken, _settings.TokenExpirationPeriod);
            return newToken;
        }
        finally
        {
            await db.LockReleaseAsync(_settings.LockKey, _instanceName);
            _logger.LogInformation("Lock released by instance {InstanceId}. LockKey: {LockKey}", 
                _instanceName, _settings.LockKey);
        }
    }

    public async Task<bool> Reset()
    {
        IDatabaseAsync db = _redis.GetDatabase();

        if (!await db.LockTakeAsync(_settings.LockKey, _instanceName, _settings.LockTimeout))
        {
            _logger.LogInformation("Background refresh: Could not acquire lock (another instance likely handled it). LockKey: {LockKey}. Instance {InstanceId}", 
                _settings.LockKey, _instanceName);
            return false;
        }
        
        _logger.LogInformation("lock acquired");

        try
        {
            bool result = await db.KeyDeleteAsync(_settings.CacheKey);
            _logger.LogInformation("token removed");
            return result;
        }
        finally
        {
            await db.LockReleaseAsync(_settings.LockKey, _instanceName);
            _logger.LogInformation("Lock released by instance {InstanceId}. LockKey: {LockKey}", 
                _instanceName, _settings.LockKey);
        }
    }
}