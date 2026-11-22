using System.Collections.ObjectModel;
using LottieAgent.Enums;
using LottieAgent.Models.Abstract;
using LottieAgent.Models.Modifier.Values;

namespace LottieAgent.Models;

/// <summary>
/// Observable collection of typed Lottie modifiers.
/// It supports both the fluent C# API and child elements declared in XAML.
/// </summary>
[ContentProperty(nameof(Items))]
public sealed class LottieModifierCollection : ObservableCollection<LottieModifier>
{
    /// <summary>
    /// XAML content property. The collection itself is exposed as its item list.
    /// </summary>
    public new IList<LottieModifier> Items => this;

    /// <summary>
    /// Adds or replaces the color of a named Lottie element.
    /// </summary>
    /// <param name="targetName">The target element name from the source JSON.</param>
    /// <param name="color">The replacement color.</param>
    /// <returns>This collection for fluent configuration.</returns>
    public LottieModifierCollection SetColor(string targetName, Color color) => AddModifier(targetName, new ValueModifier<Color>(color));

    /// <summary>
    /// Replaces the text in a named Lottie text element.
    /// </summary>
    /// <param name="targetName">The target element name from the source JSON.</param>
    /// <param name="text">The replacement text.</param>
    /// <returns>This collection for fluent configuration.</returns>
    public LottieModifierCollection SetText(string targetName, string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return AddModifier(targetName, new ValueModifier<string>(text));
    }

    /// <summary>
    /// Sets the opacity of a named element to an absolute value.
    /// </summary>
    /// <param name="targetName">The target element name.</param>
    /// <param name="opacity">The opacity in the inclusive range from zero to one.</param>
    /// <returns>This collection for fluent configuration.</returns>
    public LottieModifierCollection SetOpacity(string targetName, double opacity)
    {
        ValidateRange(opacity, 0, 1, nameof(opacity));

        return AddModifier(targetName, new ValueModifier<OpacityModifierValue>(new(opacity, LottieModifierOperation.Set)));
    }

    /// <summary>
    /// Multiplies the current opacity of a named element.
    /// </summary>
    /// <param name="targetName">The target element name.</param>
    /// <param name="factor">The non-negative opacity multiplier.</param>
    /// <returns>This collection for fluent configuration.</returns>
    public LottieModifierCollection MultiplyOpacity(string targetName, double factor)
    {
        ValidateNonNegative(factor, nameof(factor));

        return AddModifier(targetName, new ValueModifier<OpacityModifierValue>(new(factor, LottieModifierOperation.Multiply)));
    }

    /// <summary>
    /// Sets the X/Y position of a named element.
    /// </summary>
    /// <param name="targetName">The target element name.</param>
    /// <param name="position">The absolute position.</param>
    /// <returns>This collection for fluent configuration.</returns>
    public LottieModifierCollection SetPosition(string targetName, Point position)
    {
        ValidatePoint(position, nameof(position));

        return AddTransform(targetName, LottieTransformProperty.Position, LottieModifierOperation.Set, position);
    }

    /// <summary>
    /// Offsets the X/Y position of a named element.
    /// </summary>
    /// <param name="targetName">The target element name.</param>
    /// <param name="offset">The position offset.</param>
    /// <returns>This collection for fluent configuration.</returns>
    public LottieModifierCollection OffsetPosition(string targetName, Point offset)
    {
        ValidatePoint(offset, nameof(offset));

        return AddTransform(targetName, LottieTransformProperty.Position, LottieModifierOperation.Offset, offset);
    }

    /// <summary>
    /// Uniform scale. 1 means 100%, 1.2 means 120%; negative values mirror the target.
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

    /// <summary>
    /// Sets the absolute rotation of a named element.
    /// </summary>
    /// <param name="targetName">The target element name.</param>
    /// <param name="degrees">The replacement angle in degrees.</param>
    /// <returns>This collection for fluent configuration.</returns>
    public LottieModifierCollection SetRotation(string targetName, double degrees)
    {
        ValidateFinite(degrees, nameof(degrees));

        return AddModifier(targetName, new ValueModifier<RotationModifierValue>(new(degrees, LottieModifierOperation.Set)));
    }

    /// <summary>
    /// Adds a rotation offset to a named element.
    /// </summary>
    /// <param name="targetName">The target element name.</param>
    /// <param name="degrees">The rotation offset in degrees.</param>
    /// <returns>This collection for fluent configuration.</returns>
    public LottieModifierCollection OffsetRotation(string targetName, double degrees)
    {
        ValidateFinite(degrees, nameof(degrees));

        return AddModifier(targetName, new ValueModifier<RotationModifierValue>(new(degrees, LottieModifierOperation.Offset)));
    }

    private LottieModifierCollection AddTransform(string targetName, LottieTransformProperty property, LottieModifierOperation operation, Point value)
        => AddModifier(targetName, new ValueModifier<TransformModifierValue>(new(property, operation, value)));

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

        if (point.X == 0 || point.Y == 0) throw new ArgumentOutOfRangeException(parameterName, "Scale coordinates must be non-zero. Negative values mirror the target.");
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
