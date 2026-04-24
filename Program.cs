using IpBlockingApi.Repositories.Implementations;
using IpBlockingApi.Repositories.Interfaces;
using IpBlockingApi.Services.Implementations;
using IpBlockingApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "IP Blocking API",
        Version = "v1",
        Description = "Manage blocked countries and validate IP addresses via geolocation."
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ── HTTP Client ───────────────────────────────────────────────────────────────
builder.Services.AddHttpClient();

// ── Repositories (singleton — shared in-memory state) ────────────────────────
builder.Services.AddSingleton<ICountryRepository, CountryRepository>();
builder.Services.AddSingleton<ILogRepository, LogRepository>();

// ── GeoLocation: settings + typed HttpClient ──────────────────────────────────
builder.Services.AddOptions<IpBlockingApi.Settings.GeoLocationSettings>()
    .Bind(builder.Configuration.GetSection("GeoLocation"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient
    <IGeoLocationService, GeoLocationService>();

// ── Application services ──────────────────────────────────────────────────────
builder.Services.AddScoped<ICountryService,
    CountryService>();
builder.Services.AddScoped<IIpService,
    IpService>();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();