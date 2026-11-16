using LottieAgent.Interfaces.Base;
using System.Text.Json.Nodes;

namespace LottieAgent.Interfaces;

/// <summary>
/// Strongly typed handler for a specific modifier value type.
/// </summary>
public interface ILottieModifierHandler<TValue> : ILottieModifierHandler
{
    bool Apply(JsonObject target, ILottieModifierValue<TValue> modifier);
}
