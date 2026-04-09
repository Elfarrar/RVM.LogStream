using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RVM.LogStream.Domain.Interfaces;
using RVM.LogStream.Infrastructure.Data;
using RVM.LogStream.Infrastructure.Repositories;

namespace RVM.LogStream.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<LogStreamDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ILogEntryRepository, LogEntryRepository>();
        services.AddScoped<ILogSourceRepository, LogSourceRepository>();
        services.AddScoped<IRetentionPolicyRepository, RetentionPolicyRepository>();

        return services;
    }
}
