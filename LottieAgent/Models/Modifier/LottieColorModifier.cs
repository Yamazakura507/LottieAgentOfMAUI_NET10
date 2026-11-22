using LottieAgent.Models.Abstract;

namespace LottieAgent.Models.Modifier;

/// <summary>
/// XAML-friendly color replacement.
/// Example: &lt;models:LottieColorModifier TargetName="Shape" Color="#FF5A36" /&gt;.
/// </summary>
public sealed record LottieColorModifier : LottieModifier
{
    private Color color = Colors.Transparent;

    /// <summary>
    /// Initializes a color modifier with a transparent default.
    /// </summary>
    public LottieColorModifier() : base(string.Empty, new ValueModifier<Color>(Colors.Transparent)) { }

    /// <summary>
    /// Gets or sets the replacement color.
    /// </summary>
    public Color Color
    {
        get => color;
        set
        {
            color = value;

            SetModifierValue(new ValueModifier<Color>(value));
        }
    }
}
