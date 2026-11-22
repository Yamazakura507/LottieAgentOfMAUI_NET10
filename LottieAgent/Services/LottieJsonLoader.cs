using System.Text;
using LottieAgent.Interfaces;

namespace LottieAgent.Services;

/// <summary>
/// Loads JSON files bundled as MAUI app package assets.
/// </summary>
public sealed class MauiLottieJsonLoader : ILottieJsonLoader
{
    /// <inheritdoc />
    public async ValueTask<string> LoadAsync(string assetPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(assetPath)) throw new ArgumentException("An asset path is required.", nameof(assetPath));

        await using Stream stream = await FileSystem.OpenAppPackageFileAsync(assetPath);
        using StreamReader reader = new (stream, Encoding.UTF8);

        return await reader.ReadToEndAsync(cancellationToken);
    }
}
