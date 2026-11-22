using LottieAgent.Interfaces.Base;
using System.Text.Json.Nodes;

namespace LottieAgent.Interfaces;

/// <summary>
/// Strongly typed handler for a specific modifier value type.
/// </summary>
public interface ILottieModifierHandler<TValue> : ILottieModifierHandler
{
    /// <summary>
    /// Applies a modifier whose payload has the handler's declared value type.
    /// </summary>
    /// <param name="target">The target Lottie JSON element.</param>
    /// <param name="modifier">The typed modifier payload.</param>
    /// <returns><see langword="true"/> if the modifier was applied.</returns>
    bool Apply(JsonObject target, ILottieModifierValue<TValue> modifier);
}
