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
    public Type ValueType => typeof(TValue);

    public bool Apply(JsonObject target, ILottieModifierValue modifier)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(modifier);

        return modifier is ILottieModifierValue<TValue> typedModifier &&
               Apply(target, typedModifier);
    }

    public bool Apply(JsonObject target, ILottieModifierValue<TValue> modifier)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(modifier);

        return Apply(target, modifier.Value);
    }

    protected abstract bool Apply(JsonObject target, TValue value);
}
