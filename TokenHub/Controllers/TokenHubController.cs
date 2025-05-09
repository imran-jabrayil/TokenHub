using Ardalis.GuardClauses;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TokenHub.Services.Abstractions;

namespace TokenHub.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[action]")]
public class TokenHubController(ITokenHubService tokenHubService) : Controller
{
    private readonly ITokenHubService _tokenHubService = Guard.Against.Null(tokenHubService, nameof(tokenHubService));


    [HttpGet]
    [ProducesResponseType(typeof(string),      StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get()
    {
        string? token = await _tokenHubService.GetAsync();
        return token is not null 
            ? base.Ok(token)
            : base.Problem(
                detail: "The service could not retrieve a token at this time.",
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Token retrieval failed");
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reset()
    {
        bool result = await _tokenHubService.Reset();
        
        return result
            ? base.Ok()
            : base.NoContent();
    }
}