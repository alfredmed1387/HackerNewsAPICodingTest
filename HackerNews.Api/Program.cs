using HackerNews.Api.Middleware;
using HackerNews.Models;
using HackerNews.Services;
using HackerNews.Services.Implementation;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 20; // 20 requests
        limiterOptions.Window = TimeSpan.FromMinutes(1); // per minute
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0; // no queueing
    });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register dependencies
builder.Services.AddHttpClient(); // Registers IHttpClientFactory
builder.Services.AddMemoryCache(); // Registers IMemoryCache
builder.Services.AddScoped<IHackerNewsService, HackerNewsService>();
builder.Services.Configure<HackerNewsSettings>(builder.Configuration.GetSection("HackerNews"));

var app = builder.Build();
app.UseRateLimiter();

app.MapControllers().RequireRateLimiting("fixed");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
