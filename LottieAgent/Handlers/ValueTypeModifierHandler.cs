using LottieAgent.Extensions;
using LottieAgent.Handlers.Abstract;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;

namespace LottieAgent.Handlers;

/// <summary>
/// Handles scalar rotation values.
/// </summary>
public sealed class ValueTypeModifierHandler<TValue> : LottieModifierHandler<TValue>
{
    protected override bool Apply(JsonObject target, TValue value) => value switch
    {
        Color => target.ReplaceColor(Unsafe.As<TValue, Color>(ref value)),
        string => target.ReplaceText(Unsafe.As<TValue, string>(ref value)),
        _ => false
    };
}


