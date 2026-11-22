using LottieAgent.Interfaces.Base;

namespace LottieAgent.Interfaces;

/// <summary>
/// Strongly typed modifier payload.
/// </summary>
public interface ILottieModifierValue<out TValue> : ILottieModifierValue
{
    /// <summary>
    /// Gets the strongly typed value carried by the modifier.
    /// </summary>
    TValue Value { get; }
}
