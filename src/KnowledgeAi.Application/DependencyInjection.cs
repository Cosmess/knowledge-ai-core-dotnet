using System.Reflection;
using FluentValidation;
using KnowledgeAi.Application.Common.Behaviors;
using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Application.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeAi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddScoped<IMediator, Mediator>();

        RegisterOpenGenericImplementations(services, assembly, typeof(IRequestHandler<,>));

        // Pipeline order matters and is composed explicitly here (outermost first),
        // rather than relying on assembly-scan order, which is undefined.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddValidatorsFromAssembly(assembly);

        services.AddSingleton<IQuestionClassifier, KeywordQuestionClassifier>();

        return services;
    }

    private static void RegisterOpenGenericImplementations(IServiceCollection services, Assembly assembly, Type openGenericInterface)
    {
        var registrations = assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .SelectMany(type => type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterface)
                .Select(i => new { Service = i, Implementation = type }));

        foreach (var registration in registrations)
        {
            services.AddScoped(registration.Service, registration.Implementation);
        }
    }
}
