using StackExchange.Redis;
using TokenHub.Services;
using TokenHub.Services.Abstractions;
using TokenHub.Services.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

builder.Services.AddSingleton<ITokenHubService, TokenHubService>();
builder.Services.Configure<TokenHubServiceSettings>(
    builder.Configuration.GetSection(nameof(TokenHubService)));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var redisConnectionString = configuration.GetConnectionString("RedisConnection");
    if (string.IsNullOrEmpty(redisConnectionString))
    {
        throw new InvalidOperationException("Redis connection string 'RedisConnection' not found in ConnectionStrings.");
    }
    Console.WriteLine($"Attempting to connect to Redis: {redisConnectionString.Split(',')[0]}..."); // Log only host part for security
    try
    {
        var connection = ConnectionMultiplexer.Connect(redisConnectionString);
        Console.WriteLine("Successfully connected to Redis.");
        return connection;
    }
    catch (RedisConnectionException ex)
    {
        Console.Error.WriteLine($"Failed to connect to Redis: {ex.Message}");
        // Allow app to start, but it won't work correctly. Consider a health check.
        // Or rethrow if Redis is absolutely critical for startup.
        throw;
    }
});

builder.Services.AddControllers();
builder.Services.AddApiVersioning();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

app.MapControllers();

app.UseHttpsRedirection();

app.Run();
