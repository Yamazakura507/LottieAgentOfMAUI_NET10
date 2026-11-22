using LottieAgent.Enums;
using LottieAgent.Extensions;
using LottieAgent.Handlers.Abstract;
using LottieAgent.Models.Modifier.Values;
using System.Text.Json.Nodes;

namespace LottieAgent.Handlers
{
    /// <summary>
    /// Handles two-dimensional position and scale operations.
    /// </summary>
    public sealed class TransformModifierHandler : LottieModifierHandler<TransformModifierValue>
    {
        /// <inheritdoc />
        protected override bool Apply(JsonObject target, TransformModifierValue value) => (value.Property, value.Operation) switch
        {
            (LottieTransformProperty.Position, LottieModifierOperation.Set) => target.SetPosition(value.Value),
            (LottieTransformProperty.Position, LottieModifierOperation.Offset) => target.OffsetPosition(value.Value),
            (LottieTransformProperty.Scale, LottieModifierOperation.Set) => target.SetScale(value.Value),
            (LottieTransformProperty.Scale, LottieModifierOperation.Multiply) => target.MultiplyScale(value.Value),
            _ => false
        };
    }
}
