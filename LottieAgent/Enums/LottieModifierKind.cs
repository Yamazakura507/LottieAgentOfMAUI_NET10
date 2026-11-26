namespace LottieAgent.Enums
{
    /// <summary>
    /// Modifier operations currently implemented by LottieAgent.
    /// </summary>
    public enum LottieModifierKind
    {
        /// <summary>
        /// Replaces fill and stroke colors.
        /// </summary>
        Color,
        /// <summary>
        /// Replaces text document values.
        /// </summary>
        Text,
        /// <summary>
        /// Changes shape or transform opacity.
        /// </summary>
        Opacity,
        /// <summary>
        /// Changes transform position.
        /// </summary>
        Position,
        /// <summary>
        /// Changes transform scale.
        /// </summary>
        Scale,
        /// <summary>
        /// Changes transform rotation.
        /// </summary>
        Rotation
    }
}
