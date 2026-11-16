using LottieAgent.Enums;

namespace LottieAgent.Models.Modifier
{
    /// <summary>
    /// Typed payload for opacity changes. Set replaces the value; Multiply
    /// scales every existing opacity keyframe.
    /// </summary>
    public sealed record OpacityModifierValue(double Value, LottieModifierOperation Operation);
}
