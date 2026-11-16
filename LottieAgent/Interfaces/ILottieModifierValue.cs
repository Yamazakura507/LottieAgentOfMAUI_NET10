using LottieAgent.Interfaces.Base;

namespace LottieAgent.Interfaces;

/// <summary>
/// Strongly typed modifier payload.
/// </summary>
public interface ILottieModifierValue<out TValue> : ILottieModifierValue
{
    TValue Value { get; }
}
