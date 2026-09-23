using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NaoConcierge.Application.Interfaces;
using NaoConcierge.Infrastructure.Azure;
using NaoConcierge.Infrastructure.Embedding;
using NaoConcierge.Infrastructure.Persistence;
using NaoConcierge.Infrastructure.Redaction;
using NaoConcierge.Infrastructure.Secrets;
using NaoConcierge.Infrastructure.VectorSearch;

namespace NaoConcierge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var useAzure = configuration.GetValue<bool>("UseAzureCloud");

        if (useAzure)
        {
            services.AddScoped<ICaseRepository, AzureCosmosDbCaseRepository>();
            services.AddScoped<IEmbeddingService, AzureOpenAiEmbeddingService>();
            services.AddScoped<IVectorStore, AzureAiSearchVectorStore>();
        }
        else
        {
            var connectionString = configuration.GetConnectionString("PostgreSQL");
            services.AddDbContext<NaoConciergeDbContext>(options =>
                options.UseNpgsql(connectionString, o => o.UseVector()));

            services.AddScoped<ICaseRepository, CaseRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<IEmbeddingService, OnnxEmbeddingService>();
            services.AddScoped<IVectorStore, PgVectorStore>();
        }

        services.AddScoped<IRedactionService, PassthroughRedactionGate>();
        services.AddScoped<ISecretsProvider, UserSecretsProvider>();

        return services;
    }
}
