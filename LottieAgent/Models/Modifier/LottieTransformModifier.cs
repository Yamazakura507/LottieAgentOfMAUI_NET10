using LottieAgent.Enums;
using LottieAgent.Models.Abstract;
using LottieAgent.Models.Modifier.Values;

namespace LottieAgent.Models.Modifier;

/// <summary>
/// XAML-friendly two-dimensional position or scale modifier.
/// </summary>
public sealed record LottieTransformModifier : LottieModifier
{
    private LottieTransformProperty property = LottieTransformProperty.Position;
    private LottieModifierOperation operation = LottieModifierOperation.Set;
    private Point vector = Point.Zero;

    /// <summary>
    /// Initializes a position modifier at the zero point.
    /// </summary>
    public LottieTransformModifier() : base(string.Empty, new ValueModifier<TransformModifierValue>(new(LottieTransformProperty.Position, LottieModifierOperation.Set, Point.Zero))) { }

    /// <summary>
    /// Gets or sets whether the transform targets position or scale.
    /// </summary>
    public LottieTransformProperty Property
    {
        get => property;
        set
        {
            property = value;

            UpdateValue();
        }
    }

    /// <summary>
    /// Gets or sets whether the transform replaces, multiplies or offsets a value.
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

    /// <summary>
    /// Gets or sets the two-dimensional transform vector.
    /// </summary>
    public Point Vector
    {
        get => vector;
        set
        {
            if (!double.IsFinite(value.X) || !double.IsFinite(value.Y))
                throw new ArgumentOutOfRangeException(nameof(value), "Both coordinates must be finite.");
            if (property == LottieTransformProperty.Scale && (value.X == 0 || value.Y == 0))
                throw new ArgumentOutOfRangeException(nameof(value), "Scale coordinates must be non-zero. Negative values mirror the target.");

            vector = value;

            UpdateValue();
        }
    }

    private void UpdateValue() => SetModifierValue(new ValueModifier<TransformModifierValue>(new(property, operation, vector)));
}
