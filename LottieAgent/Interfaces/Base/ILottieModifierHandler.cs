using System.Text.Json.Nodes;

namespace LottieAgent.Interfaces.Base
{
    /// <summary>
    /// Non-generic bridge used by the processor to dispatch a heterogeneous collection of handlers.
    /// </summary>
    public interface ILottieModifierHandler
    {
        Type ValueType { get; }

        bool Apply(JsonObject target, ILottieModifierValue modifier);
    }
}
