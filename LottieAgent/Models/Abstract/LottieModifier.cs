using LottieAgent.Interfaces.Base;

namespace LottieAgent.Models.Abstract;

/// <summary>
/// Base record for a named Lottie JSON modification.
/// TargetName and Value are mutable so concrete modifiers can be authored in XAML.
/// </summary>
public record LottieModifier
{
    /// <summary>
    /// Initializes a modifier for the named element and value payload.
    /// </summary>
    /// <param name="targetName">The Lottie JSON element name to modify.</param>
    /// <param name="value">The value payload used by the modifier.</param>
    protected LottieModifier(string targetName, ILottieModifierValue value)
    {
        TargetName = targetName ?? string.Empty;
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the exact Lottie element name targeted by this modifier.
    /// </summary>
    public string TargetName { get; set; }

    /// <summary>
    /// Gets the modifier typed payload.
    /// </summary>
    public ILottieModifierValue Value { get; protected set; }

    /// <summary>
    /// Replaces the payload when a XAML-facing property changes.
    /// </summary>
    /// <param name="value">The updated payload.</param>
    protected void SetModifierValue(ILottieModifierValue value) => Value = value ?? throw new ArgumentNullException(nameof(value));
}
