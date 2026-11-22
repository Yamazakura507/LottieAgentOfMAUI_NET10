using LottieAgent.Enums;

namespace LottieAgent.Models.Modifier.Values
{
    /// <summary>
    /// Typed payload for position and scale changes.
    /// Point.X and Point.Y represent the two-dimensional value.
    /// For scale, 1 means 100% and 1.2 means 120%.
    /// </summary>
    public sealed record TransformModifierValue(LottieTransformProperty Property, LottieModifierOperation Operation, Point Value);
}
