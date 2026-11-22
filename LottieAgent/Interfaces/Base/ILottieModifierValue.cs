namespace LottieAgent.Interfaces.Base
{
    /// <summary>
    /// Non-generic contract used to store values of different types in one modifier collection.
    /// </summary>
    public interface ILottieModifierValue
    {
        /// <summary>
        /// Gets the runtime type represented by this modifier payload.
        /// </summary>
        Type ValueType { get; }
    }
}
