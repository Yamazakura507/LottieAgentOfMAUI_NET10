using System.Text.Json.Nodes;
using LottieAgent.Interfaces;
using LottieAgent.Interfaces.Base;

namespace LottieAgent.Handlers.Abstract;

/// <summary>
/// Type-safe adapter that exposes a generic handler through the non-generic
/// contract required by the modifier processor and dependency injection.
/// </summary>
public abstract class LottieModifierHandler<TValue> : ILottieModifierHandler<TValue>
{
    /// <inheritdoc />
    public Type ValueType => typeof(TValue);

    /// <inheritdoc />
    public bool Apply(JsonObject target, ILottieModifierValue modifier)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(modifier);

        return modifier is ILottieModifierValue<TValue> typedModifier && Apply(target, typedModifier);
    }

    /// <inheritdoc />
    public bool Apply(JsonObject target, ILottieModifierValue<TValue> modifier)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(modifier);

        return Apply(target, modifier.Value);
    }

    /// <summary>
    /// Applies the typed modifier payload to a matching JSON object.
    /// </summary>
    /// <param name="target">The Lottie JSON element being modified.</param>
    /// <param name="value">The strongly typed modifier payload.</param>
    /// <returns><see langword="true"/> when the target was modified.</returns>
    protected abstract bool Apply(JsonObject target, TValue value);
}
