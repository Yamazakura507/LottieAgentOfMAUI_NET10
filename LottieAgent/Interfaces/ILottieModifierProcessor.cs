using LottieAgent.Models;
using LottieAgent.Models.Abstract;

namespace LottieAgent.Interfaces;

/// <summary>
/// Applies a collection of typed modifiers to an original Lottie JSON document.
/// </summary>
public interface ILottieModifierProcessor
{
    /// <summary>
    /// Applies named modifiers to a Lottie JSON document.
    /// </summary>
    /// <param name="sourceJson">The original animation JSON.</param>
    /// <param name="modifiers">The named modifiers to apply.</param>
    /// <returns>The modified JSON and any modifier targets that were not applied.</returns>
    LottieModifierResult Apply(string sourceJson, IEnumerable<LottieModifier> modifiers);
}
