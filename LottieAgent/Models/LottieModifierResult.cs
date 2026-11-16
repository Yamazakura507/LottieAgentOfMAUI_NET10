namespace LottieAgent.Models;

/// <summary>
/// Result of applying a modifier collection to a Lottie JSON document.
/// </summary>
public sealed record LottieModifierResult(string Json, IReadOnlyList<string> NotApplied);
