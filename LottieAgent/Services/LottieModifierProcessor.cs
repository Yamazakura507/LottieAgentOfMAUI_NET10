using System.Text.Json;
using System.Text.Json.Nodes;
using LottieAgent.Handlers;
using LottieAgent.Interfaces;
using LottieAgent.Models;
using LottieAgent.Extensions;
using LottieAgent.Interfaces.Base;
using LottieAgent.Models.Abstract;

namespace LottieAgent.Services;

/// <summary>
/// Finds named JSON elements and dispatches each modifier to the handler
/// registered for the modifier value's TValue.
/// </summary>
public sealed class LottieModifierProcessor : ILottieModifierProcessor
{
    private readonly IReadOnlyDictionary<Type, ILottieModifierHandler> handlers;

    public LottieModifierProcessor(IEnumerable<ILottieModifierHandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);

        this.handlers = handlers
            .GroupBy(handler => handler.ValueType)
            .ToDictionary(group => group.Key, group => group.Last());
    }

    /// <summary>
    /// Creates a processor with the built-in handlers for controls created directly by XAML.
    /// </summary>
    public static LottieModifierProcessor CreateDefault()
    {
        ILottieModifierHandler[] handlers =
        [
            new ValueTypeModifierHandler<Color>(),
            new ValueTypeModifierHandler<string>(),
            new OpacityModifierHandler(),
            new TransformModifierHandler(),
            new RotationModifierHandler()
        ];

        return new LottieModifierProcessor(handlers);
    }

    public LottieModifierResult Apply(string sourceJson, IEnumerable<LottieModifier> modifiers)
    {
        ArgumentNullException.ThrowIfNull(sourceJson);
        ArgumentNullException.ThrowIfNull(modifiers);

        LottieModifier[] modifierList = modifiers.ToArray();

        if (modifierList.Length == 0) return new LottieModifierResult(sourceJson, Array.Empty<string>());

        JsonNode root = JsonNode.Parse(sourceJson) ?? throw new InvalidDataException("The Lottie JSON document is empty.");

        List<string> notApplied = new ();

        foreach (LottieModifier modifier in modifierList)
        {
            ILottieModifierValue modifierValue = modifier.Value;
            Type valueType = modifierValue.ValueType;

            if (!handlers.TryGetValue(valueType, out ILottieModifierHandler? handler))
            {
                notApplied.Add(modifier.TargetName);

                continue;
            }

            JsonObject[] targets = root.EnumerateObjects()
                .Where(node => string.Equals(node["nm"].GetString(), modifier.TargetName, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            bool applied = false;

            foreach (JsonObject target in targets) applied |= handler.Apply(target, modifierValue);

            if (!applied) notApplied.Add(modifier.TargetName);
        }

        return new LottieModifierResult(root.ToJsonString(new JsonSerializerOptions { WriteIndented = false }), notApplied);
    }
}
