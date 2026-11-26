using LottieAgent.Enums;

namespace LottieAgent.Models;

/// <summary>
/// A named Lottie element and the modifier operations supported by its JSON subtree.
/// </summary>
public sealed record LottieModifierParameterTag(string Name, IReadOnlyList<LottieModifierKind> SupportedModifiers);


