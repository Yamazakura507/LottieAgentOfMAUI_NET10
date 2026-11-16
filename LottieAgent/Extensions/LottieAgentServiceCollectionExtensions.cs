using LottieAgent.Controls;
using LottieAgent.Handlers;
using LottieAgent.Interfaces;
using LottieAgent.Interfaces.Base;
using LottieAgent.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LottieAgent.Extensions;

/// <summary>
/// Registers LottieAgent services and generic modifier handlers.
/// </summary>
public static class LottieAgentServiceCollectionExtensions
{
    public static IServiceCollection AddLottieAgent(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILottieModifierHandler, ValueTypeModifierHandler<Color>>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILottieModifierHandler, ValueTypeModifierHandler<string>>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILottieModifierHandler, OpacityModifierHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILottieModifierHandler, TransformModifierHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILottieModifierHandler, RotationModifierHandler>());

        services.TryAddSingleton<ILottieJsonLoader, MauiLottieJsonLoader>();
        services.TryAddSingleton<ILottieModifierProcessor, LottieModifierProcessor>();

        services.AddTransient(provider =>
            new LottieView(provider.GetRequiredService<ILottieJsonLoader>(), provider.GetRequiredService<ILottieModifierProcessor>()));

        return services;
    }
}
