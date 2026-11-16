using LottieAgent.Enums;

namespace LottieAgent.Models.Modifier
{
    /// <summary>
    /// Typed payload for rotation changes, which use a scalar angle.
    /// </summary>
    public sealed record RotationModifierValue(double Degrees, LottieModifierOperation Operation);
}
