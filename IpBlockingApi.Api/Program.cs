using IpBlockingApi.Middleware;
using IpBlockingApi.Repositories.Implementations;
using IpBlockingApi.Repositories.Interfaces;
using IpBlockingApi.Services.Implementations;
using IpBlockingApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = ctx =>
        {
            var errors = ctx.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(x => x.ErrorMessage))
                .FirstOrDefault() ?? "Validation failed.";

            var result = IpBlockingApi.Common.ApiResponse<object>.Fail(errors);
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(result);
        };
    });

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "IP Blocking API",
        Version = "v1",
        Description = """
                      REST API for managing blocked countries and validating IP addresses
                      using a third-party geolocation provider (ipapi.co).
                      All data is stored in-memory — no database is used.
                      """,
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "GitHub Repository",
            Url = new Uri("https://github.com/kareem-sabry/IpBlockingApi")
        }
    });

    // Include XML documentation comments from the compiled assembly.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    // Show enums as strings and keep schema definitions inline for readability.
    options.UseInlineDefinitionsForEnums();
    options.DescribeAllParametersInCamelCase();
});

// ── HTTP Client ───────────────────────────────────────────────────────────────
builder.Services.AddHttpClient();

// ── Repositories (singleton — shared in-memory state) ────────────────────────
builder.Services.AddSingleton<ICountryRepository, CountryRepository>();
builder.Services.AddSingleton<ILogRepository, LogRepository>();
builder.Services.AddSingleton<IpBlockingApi.Common.GeoLocationRateLimiter>();

var geoBaseUrl = builder.Configuration["GeoLocation:BaseUrl"]!.TrimEnd('/') + "/";
builder.Services.AddHttpClient<IGeoLocationService, GeoLocationService>(client =>
{
    client.BaseAddress = new Uri(geoBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("IpBlockingApi/1.0");
});
// ── GeoLocation: settings + typed HttpClient ──────────────────────────────────
builder.Services.AddOptions<IpBlockingApi.Settings.GeoLocationSettings>()
    .Bind(builder.Configuration.GetSection("GeoLocation"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ── Application services ──────────────────────────────────────────────────────
builder.Services.AddScoped<ICountryService,
    CountryService>();
builder.Services.AddScoped<IIpService, IpService>();
builder.Services.AddScoped<ILogService, LogService>();

// ── Background services ───────────────────────────────────────────────────────
builder.Services.AddHostedService<IpBlockingApi.BackgroundServices.TemporalBlockCleanupService>();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────────────────────────
// Exception handler must be first so it wraps the entire pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Security headers on every response.
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();