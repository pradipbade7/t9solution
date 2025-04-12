using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using T9Backend.Data;
using T9Backend.Services;
using T9Backend.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger for API documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "T9 Word Matcher API",
        Version = "v1",
        Description = "API for matching T9 digit sequences to words",
    });
});

// Configure CORS to allow React frontend to communicate with API in development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Configure strongly-typed settings
builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.Configure<T9Settings>(builder.Configuration.GetSection("T9"));
builder.Services.Configure<RateLimitSettings>(builder.Configuration.GetSection("RateLimit"));

// Register services
builder.Services.AddSingleton<IRateLimitService, InMemoryRateLimitService>();
builder.Services.AddSingleton<IWordService, WordService>();
builder.Services.AddSingleton<WordsLoader>(serviceProvider =>
{
    var t9Settings = serviceProvider.GetRequiredService<IOptions<T9Settings>>().Value;
    return new WordsLoader(
        t9Settings.DictionaryPath,
        serviceProvider.GetService<ILogger<WordsLoader>>()
    );
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "T9 Word Matcher API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseCors("AllowReactApp");

// Serve static files and configure SPA fallback
app.UseStaticFiles(); // Serves files from wwwroot
app.UseDefaultFiles(); // Looks for default.html, index.html etc.
app.MapFallbackToFile("index.html");

// Map API controllers
app.MapControllers();

app.Run();
