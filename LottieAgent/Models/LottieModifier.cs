using LottieAgent.Interfaces;
using LottieAgent.Models.Abstract;

namespace LottieAgent.Models;

/// <summary>
/// Modifier that keeps its payload strongly typed.
/// </summary>
public sealed record LottieModifier<TValue> : LottieModifier
{
    public LottieModifier(string targetName, ILottieModifierValue<TValue> value) : base(targetName, value)
    {

    }

    public new ILottieModifierValue<TValue> Value => (ILottieModifierValue<TValue>)base.Value;
}
