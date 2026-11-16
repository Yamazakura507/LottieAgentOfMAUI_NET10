using LottieAgent.Models;
using LottieAgent.Models.Abstract;

namespace LottieAgent.Interfaces;

/// <summary>
/// Applies a collection of typed modifiers to an original Lottie JSON document.
/// </summary>
public interface ILottieModifierProcessor
{
    LottieModifierResult Apply(string sourceJson, IEnumerable<LottieModifier> modifiers);
}
