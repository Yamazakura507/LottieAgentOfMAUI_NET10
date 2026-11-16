using LottieAgent.Enums;
using LottieAgent.Extensions;
using LottieAgent.Handlers.Abstract;
using LottieAgent.Models.Modifier;
using System.Text.Json.Nodes;

namespace LottieAgent.Handlers
{
    /// <summary>
    /// Handles both absolute and multiplicative opacity operations.
    /// </summary>
    public sealed class OpacityModifierHandler : LottieModifierHandler<OpacityModifierValue>
    {
        protected override bool Apply(JsonObject target, OpacityModifierValue value) => value.Operation switch
        {
            LottieModifierOperation.Set => target.SetOpacity(value.Value),
            LottieModifierOperation.Multiply => target.MultiplyOpacity(value.Value),
            _ => false
        };
    }
}
