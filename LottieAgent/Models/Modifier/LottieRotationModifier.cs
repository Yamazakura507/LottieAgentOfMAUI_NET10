using LottieAgent.Enums;
using LottieAgent.Models.Abstract;
using LottieAgent.Models.Modifier.Values;

namespace LottieAgent.Models.Modifier;

/// <summary>
/// XAML-friendly rotation replacement or offset.
/// </summary>
public sealed record LottieRotationModifier : LottieModifier
{
    private double degrees;
    private LottieModifierOperation operation = LottieModifierOperation.Set;

    /// <summary>
    /// Initializes a rotation modifier with zero degrees.
    /// </summary>
    public LottieRotationModifier() : base(string.Empty, new ValueModifier<RotationModifierValue>(new(0d, LottieModifierOperation.Set))) { }

    /// <summary>
    /// Gets or sets the rotation angle in degrees.
    /// </summary>
    public double Degrees
    {
        get => degrees;
        set
        {
            if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value), "Value must be finite.");

            degrees = value;

            UpdateValue();
        }
    }

    /// <summary>
    /// Gets or sets whether rotation is replaced or offset.
    /// </summary>
    public LottieModifierOperation Operation
    {
        get => operation;
        set
        {
            operation = value;

            UpdateValue();
        }
    }

    private void UpdateValue() => SetModifierValue(new ValueModifier<RotationModifierValue>(new(degrees, operation)));
}
