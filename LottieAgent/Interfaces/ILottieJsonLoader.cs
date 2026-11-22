namespace LottieAgent.Interfaces;

/// <summary>
/// Loads a Lottie JSON asset from the MAUI application package.
/// </summary>
public interface ILottieJsonLoader
{
    /// <summary>
    /// Loads a Lottie JSON asset.
    /// </summary>
    /// <param name="assetPath">The application asset path.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>The animation JSON document.</returns>
    ValueTask<string> LoadAsync(string assetPath, CancellationToken cancellationToken = default);
}
