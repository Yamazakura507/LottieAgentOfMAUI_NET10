using LottieAgent.Interfaces;
using LottieAgent.Models.Abstract;

namespace LottieAgent.Models;

/// <summary>
/// Modifier that keeps its payload strongly typed.
/// </summary>
public sealed record LottieModifier<TValue> : LottieModifier
{
    /// <summary>
    /// Initializes the modifier with a target name and typed payload.
    /// </summary>
    /// <param name="targetName">The target Lottie element name.</param>
    /// <param name="value">The typed modifier payload.</param>
    public LottieModifier(string targetName, ILottieModifierValue<TValue> value) : base(targetName, value)
    {

    }

    /// <summary>
    /// Gets the modifier payload in its strongly typed form.
    /// </summary>
    public new ILottieModifierValue<TValue> Value => (ILottieModifierValue<TValue>)base.Value;
}
