namespace LottieAgent.Interfaces;

/// <summary>
/// Loads a Lottie JSON asset from the MAUI application package.
/// </summary>
public interface ILottieJsonLoader
{
    ValueTask<string> LoadAsync(string assetPath, CancellationToken cancellationToken = default);
}
