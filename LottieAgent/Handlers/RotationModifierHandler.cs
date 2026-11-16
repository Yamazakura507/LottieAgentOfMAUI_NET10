using LottieAgent.Enums;
using LottieAgent.Extensions;
using LottieAgent.Handlers.Abstract;
using LottieAgent.Models.Modifier;
using System.Text.Json.Nodes;

namespace LottieAgent.Handlers
{
    /// <summary>
    /// Handles scalar rotation values.
    /// </summary>
    public sealed class RotationModifierHandler : LottieModifierHandler<RotationModifierValue>
    {
        protected override bool Apply(JsonObject target, RotationModifierValue value) => value.Operation switch
        {
            LottieModifierOperation.Set => target.SetRotation(value.Degrees),
            LottieModifierOperation.Offset => target.OffsetRotation(value.Degrees),
            _ => false
        };
    }
}
