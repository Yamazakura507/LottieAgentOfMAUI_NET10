using LottieAgent.Models.Abstract;

namespace LottieAgent.Models.Modifier;

/// <summary>
/// XAML-friendly text replacement.
/// </summary>
public sealed record LottieTextModifier : LottieModifier
{
    private string text = string.Empty;

    /// <summary>
    /// Initializes a text modifier with an empty value.
    /// </summary>
    public LottieTextModifier() : base(string.Empty, new ValueModifier<string>(string.Empty)) { }

    /// <summary>
    /// Gets or sets the replacement text.
    /// </summary>
    public string Text
    {
        get => text;
        set
        {
            text = value ?? string.Empty;

            SetModifierValue(new ValueModifier<string>(text));
        }
    }
}
