namespace LottieAgent.Interfaces.Base
{
    /// <summary>
    /// Non-generic contract used to store values of different types in one modifier collection.
    /// </summary>
    public interface ILottieModifierValue
    {
        Type ValueType { get; }
    }
}
