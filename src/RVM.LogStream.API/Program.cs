using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using RVM.LogStream.API.Auth;
using RVM.LogStream.API.Health;
using RVM.LogStream.API.Hubs;
using RVM.LogStream.API.Middleware;
using RVM.LogStream.API.Services;
using RVM.LogStream.Infrastructure;
using RVM.LogStream.Infrastructure.Data;
using RVM.Common.Security;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(new RenderedCompactJsonFormatter());

        var seqUrl = context.Configuration["Seq:ServerUrl"];
        if (!string.IsNullOrEmpty(seqUrl))
            loggerConfiguration.WriteTo.Seq(seqUrl);
    });

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();
    builder.Services.AddSignalR();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });

    builder.Services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("database");

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 60, Window = TimeSpan.FromMinutes(1) }));
    });

    builder.Services.AddAuthentication(ApiKeyAuthOptions.Scheme)
        .AddScheme<ApiKeyAuthOptions, ApiKeyAuthHandler>(ApiKeyAuthOptions.Scheme, options =>
        {
            builder.Configuration.GetSection("ApiKeys").Bind(options);
        });
    builder.Services.AddAuthorization();

    builder.Services.AddScoped<LogIngestionService>();
    builder.Services.AddScoped<LogSearchService>();
    builder.Services.AddHostedService<RetentionWorker>();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<LogStreamDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    var pathBase = app.Configuration["App:PathBase"];
    if (!string.IsNullOrEmpty(pathBase))
        app.UsePathBase(pathBase);

    app.UseForwardedHeaders();
    app.UseSecurityHeaders();
    app.UseStaticFiles();
    app.UseAntiforgery();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapRazorComponents<RVM.LogStream.API.Components.App>()
        .AddInteractiveServerRenderMode();
    app.MapHub<LogStreamHub>("/hubs/log-stream").AllowAnonymous();
    app.MapHealthChecks("/health").AllowAnonymous();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
