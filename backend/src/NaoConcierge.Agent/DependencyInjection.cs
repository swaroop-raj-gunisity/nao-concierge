using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using NaoConcierge.Agent.Orchestration;
using NaoConcierge.Agent.Plugins;
using NaoConcierge.Application.Interfaces;

namespace NaoConcierge.Agent;

public static class DependencyInjection
{
    public static IServiceCollection AddAgentServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<Kernel>(sp =>
        {
            var builder = Kernel.CreateBuilder();

            var embeddingService = sp.GetRequiredService<IEmbeddingService>();
            var vectorStore = sp.GetRequiredService<IVectorStore>();
            var redactionService = sp.GetRequiredService<IRedactionService>();

            var intakePlugin = new IntakePlugin(embeddingService, vectorStore, redactionService);
            builder.Plugins.AddFromObject(intakePlugin, "IntakePlugin");

            return builder.Build();
        });

        services.AddScoped<IChatOrchestrator, ConciergeChatOrchestrator>();

        return services;
    }
}
