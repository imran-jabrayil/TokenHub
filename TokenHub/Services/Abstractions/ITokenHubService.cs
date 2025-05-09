namespace TokenHub.Services.Abstractions;

public interface ITokenHubService
{
    Task<string?> GetAsync();

    Task<bool> Reset();
}