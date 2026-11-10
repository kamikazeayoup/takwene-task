using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using TakweneTask.Api.Filters;
using TakweneTask.Api.Middleware;
using TakweneTask.Api.OpenApi;
using TakweneTask.Application;
using TakweneTask.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog Structured Logging
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {TraceId} {Message:lj}{NewLine}{Exception}"));

// 2. OpenTelemetry Tracing & Metrics
var otel = builder.Services.AddOpenTelemetry();
otel.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation()
           .AddNpgsql();

    var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
        ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
    {
        tracing.AddOtlpExporter(opt => opt.Endpoint = new Uri(otlpEndpoint));
    }
});

otel.WithMetrics(metrics =>
{
    metrics.AddAspNetCoreInstrumentation()
           .AddRuntimeInstrumentation();

    var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
        ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
    {
        metrics.AddOtlpExporter(opt => opt.Endpoint = new Uri(otlpEndpoint));
    }
});

// 3. Validate JWT Key at startup
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
if (keyBytes.Length < 32)
{
    throw new InvalidOperationException("Jwt:Key must be at least 32 bytes (256 bits).");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TakweneTask";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TakweneTask";

// 4. Authentication & Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        NameClaimType = "name"
    };
});

builder.Services.AddAuthorization();

// 5. CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 6. Application & Infrastructure Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 7. Exception Handling & Controllers
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<FluentValidationActionFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// 8. OpenAPI & Swagger UI
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<OpenApiDocumentTransformer>();
});

var app = builder.Build();

app.UseExceptionHandler();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0} ms (user: {User})";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("User", httpContext.User.Identity?.Name ?? "anonymous");
    };
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Takwene Track Management API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
