using System.Text.Json.Nodes;

namespace LottieAgent.Interfaces.Base
{
    /// <summary>
    /// Non-generic bridge used by the processor to dispatch a heterogeneous collection of handlers.
    /// </summary>
    public interface ILottieModifierHandler
    {
        /// <summary>
        /// Gets the runtime type of modifier payload handled by this handler.
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        /// Applies a non-generic modifier payload to a JSON element.
        /// </summary>
        /// <param name="target">The target Lottie JSON element.</param>
        /// <param name="modifier">The typed or untyped modifier payload.</param>
        /// <returns><see langword="true"/> if the modifier was applied.</returns>
        bool Apply(JsonObject target, ILottieModifierValue modifier);
    }
}
