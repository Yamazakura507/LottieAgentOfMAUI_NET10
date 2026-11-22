using LottieAgent.Enums;
using LottieAgent.Models.Abstract;
using LottieAgent.Models.Modifier.Values;

namespace LottieAgent.Models.Modifier;

/// <summary>
/// XAML-friendly opacity replacement or multiplier.
/// </summary>
public sealed record LottieOpacityModifier : LottieModifier
{
    private double opacity = 1d;
    private LottieModifierOperation operation = LottieModifierOperation.Set;

    /// <summary>
    /// Initializes an opacity modifier with a value of one.
    /// </summary>
    public LottieOpacityModifier() : base(string.Empty, new ValueModifier<OpacityModifierValue>(new(1d, LottieModifierOperation.Set))) { }

    /// <summary>
    /// Gets or sets the opacity value or multiplier.
    /// </summary>
    public double Opacity
    {
        get => opacity;
        set
        {
            if (!double.IsFinite(value) || value < 0 || (operation == LottieModifierOperation.Set && value > 1))
                throw new ArgumentOutOfRangeException(nameof(value), "Set opacity must be in [0, 1]; a multiplier must be non-negative.");

            opacity = value;

            UpdateValue();
        }
    }

    /// <summary>
    /// Gets or sets whether opacity is replaced or multiplied.
    /// </summary>
    public LottieModifierOperation Operation
    {
        get => operation;
        set
        {
            operation = value;
            if (operation == LottieModifierOperation.Set && opacity > 1)
                throw new ArgumentOutOfRangeException(nameof(value), "Set opacity must be in [0, 1].");

            UpdateValue();
        }
    }

    private void UpdateValue() => SetModifierValue(new ValueModifier<OpacityModifierValue>(new(opacity, operation)));
}
