namespace LottieAgent.Enums
{
    /// <summary>
    /// Operation applied to a numeric Lottie value.
    /// </summary>
    public enum LottieModifierOperation
    {
        /// <summary>
        /// Replaces the current value with the supplied value.
        /// </summary>
        Set,
        /// <summary>
        /// Multiplies the current value by the supplied value.
        /// </summary>
        Multiply,
        /// <summary>
        /// Adds the supplied value to the current value.
        /// </summary>
        Offset
    }
}
