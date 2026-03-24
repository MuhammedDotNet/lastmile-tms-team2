using Hangfire;
using Hangfire.PostgreSql;
using LastMile.TMS.Api.GraphQL;
using LastMile.TMS.Api.GraphQL.Types;
using Microsoft.AspNetCore.Authentication;

namespace LastMile.TMS.Api;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Use OpenIddict JWT validation instead of Identity cookies. Call after <c>AddPersistence</c> and <c>AddInfrastructure</c>.
    /// </summary>
    public static IServiceCollection AddOpenIddictJwtAuthenticationDefaults(this IServiceCollection services)
    {
        services.Configure<AuthenticationOptions>(options =>
        {
            options.DefaultAuthenticateScheme =
                OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme =
                OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });
        return services;
    }

    public static IServiceCollection AddLastMileApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddControllers();
        services.AddGraphQLServer()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .AddType<VehicleDtoType>()
            .AddAuthorization()
            .AddErrorFilter<DomainExceptionErrorFilter>();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? new[] { "http://localhost:3000" };

                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        services.AddStackExchangeRedisCache(options =>
            options.Configuration = configuration.GetConnectionString("Redis"));

        services.AddHangfire(config =>
            config.UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(configuration.GetConnectionString("HangfireConnection"))));
        services.AddHangfireServer();

        return services;
    }
}
