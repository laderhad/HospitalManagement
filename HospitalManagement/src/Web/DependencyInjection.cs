using Azure.Identity;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Domain.Constants;
using HospitalManagement.Infrastructure.Data;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<IUser, CurrentUser>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApi(options =>
        {
            options.AddOperationTransformer<ApiExceptionOperationTransformer>();
            options.AddOperationTransformer<IdentityApiOperationTransformer>();
        });

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(Policies.IsAdministrator, policy => policy.RequireRole(Roles.Administrator))
            .AddPolicy(Policies.IsDoctor, policy => policy.RequireRole(Roles.Doctor))
            .AddPolicy(Policies.IsPatient, policy => policy.RequireRole(Roles.Patient))
            .AddPolicy(Policies.IsAdministratorOrDoctor, policy =>
                policy.RequireRole(Roles.Administrator, Roles.Doctor))
            .AddPolicy(Policies.IsAdministratorOrPatient, policy =>
                policy.RequireRole(Roles.Administrator, Roles.Patient));

        builder.Services.AddCors();
    }

    public static void AddKeyVaultIfConfigured(this IHostApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }
    }
}
