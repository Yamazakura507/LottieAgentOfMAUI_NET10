using LottieAgent.Interfaces;

namespace LottieAgent.Models;

/// <summary>
/// Generic wrapper for a strongly typed modifier payload.
/// </summary>
public sealed record ValueModifier<TValue>(TValue Value) : ILottieModifierValue<TValue>
{
    /// <inheritdoc />
    public Type ValueType => typeof(TValue);
}
