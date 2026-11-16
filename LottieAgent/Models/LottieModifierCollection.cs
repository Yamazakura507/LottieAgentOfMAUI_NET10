using System.Collections.ObjectModel;
using LottieAgent.Enums;
using LottieAgent.Models.Abstract;
using LottieAgent.Models.Modifier;

namespace LottieAgent.Models;

/// <summary>
/// Fluent, observable collection of typed Lottie modifiers.
/// </summary>
public sealed class LottieModifierCollection : ObservableCollection<LottieModifier>
{
    public LottieModifierCollection SetColor(string targetName, Color color) => AddModifier(targetName, new ValueModifier<Color>(color));

    public LottieModifierCollection SetText(string targetName, string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return AddModifier(targetName, new ValueModifier<string>(text));
    }

    public LottieModifierCollection SetOpacity(string targetName, double opacity)
    {
        ValidateRange(opacity, 0, 1, nameof(opacity));

        return AddModifier(targetName, new ValueModifier<OpacityModifierValue>(new (opacity, LottieModifierOperation.Set)));
    }

    public LottieModifierCollection MultiplyOpacity(string targetName, double factor)
    {
        ValidateNonNegative(factor, nameof(factor));

        return AddModifier(targetName, new ValueModifier<OpacityModifierValue>(new (factor, LottieModifierOperation.Multiply)));
    }

    public LottieModifierCollection SetPosition(string targetName, Point position)
    {
        ValidatePoint(position, nameof(position));

        return AddTransform(targetName, LottieTransformProperty.Position, LottieModifierOperation.Set, position);
    }

    public LottieModifierCollection OffsetPosition(string targetName, Point offset)
    {
        ValidatePoint(offset, nameof(offset));

        return AddTransform(targetName, LottieTransformProperty.Position, LottieModifierOperation.Offset, offset);
    }

    /// <summary>
    /// Uniform scale. 1 means 100%, 1.2 means 120%.
    /// </summary>
    public LottieModifierCollection SetScale(string targetName, double scale) => SetScale(targetName, new Point(scale, scale));

    /// <summary>
    /// Non-uniform scale. 1 means 100%, 1.2 means 120%.
    /// </summary>
    public LottieModifierCollection SetScale(string targetName, Point scale)
    {
        ValidatePositivePoint(scale, nameof(scale));

        return AddTransform(targetName, LottieTransformProperty.Scale, LottieModifierOperation.Set, scale);
    }

    /// <summary>
    /// Uniform scale multiplier.
    /// </summary>
    public LottieModifierCollection MultiplyScale(string targetName, double factor) => MultiplyScale(targetName, new Point(factor, factor));

    /// <summary>
    /// Non-uniform scale multiplier.
    /// </summary>
    public LottieModifierCollection MultiplyScale(string targetName, Point factor)
    {
        ValidatePositivePoint(factor, nameof(factor));

        return AddTransform(targetName, LottieTransformProperty.Scale, LottieModifierOperation.Multiply, factor);
    }

    public LottieModifierCollection SetRotation(string targetName, double degrees)
    {
        ValidateFinite(degrees, nameof(degrees));

        return AddModifier(targetName, new ValueModifier<RotationModifierValue>(new (degrees, LottieModifierOperation.Set)));
    }

    public LottieModifierCollection OffsetRotation(string targetName, double degrees)
    {
        ValidateFinite(degrees, nameof(degrees));

        return AddModifier(targetName, new ValueModifier<RotationModifierValue>(new (degrees, LottieModifierOperation.Offset)));
    }

    private LottieModifierCollection AddTransform(string targetName, LottieTransformProperty property, LottieModifierOperation operation, Point value)
        => AddModifier(targetName, new ValueModifier<TransformModifierValue>(new (property, operation, value)));

    private LottieModifierCollection AddModifier<TValue>(string targetName, ValueModifier<TValue> value)
    {
        if (string.IsNullOrWhiteSpace(targetName)) throw new ArgumentException("A Lottie element name is required.", nameof(targetName));

        Add(new LottieModifier<TValue>(targetName, value));

        return this;
    }

    private static void ValidatePoint(Point point, string parameterName)
    {
        ValidateFinite(point.X, parameterName);
        ValidateFinite(point.Y, parameterName);
    }

    private static void ValidatePositivePoint(Point point, string parameterName)
    {
        ValidatePoint(point, parameterName);

        if (point.X <= 0 || point.Y <= 0) throw new ArgumentOutOfRangeException(parameterName, "Both point coordinates must be greater than zero.");
    }

    private static void ValidateFinite(double value, string parameterName)
    {
        if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
    }

    private static void ValidateNonNegative(double value, string parameterName)
    {
        ValidateFinite(value, parameterName);

        if (value < 0) throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative.");
    }

    private static void ValidateRange(double value, double min, double max, string parameterName)
    {
        ValidateFinite(value, parameterName);

        if (value < min || value > max) throw new ArgumentOutOfRangeException(parameterName, $"Value must be between {min} and {max}.");
    }
}
