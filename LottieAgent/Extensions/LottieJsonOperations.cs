using System.Text.Json.Nodes;

namespace LottieAgent.Extensions;

/// <summary>
/// Low-level JSON operations shared by modifier handlers.
/// This class contains no DI or playback logic.
/// </summary>
internal static class LottieJsonOperations
{
    /// <summary>
    /// Replaces the color of every fill and stroke in the target Lottie element.
    /// Animated color keyframes are updated as well.
    /// </summary>
    /// <param name="target">The Lottie JSON element whose fill and stroke colors are changed.</param>
    /// <param name="color">The replacement color. Its alpha value is also applied to the element opacity.</param>
    /// <returns><see langword="true"/> if at least one fill or stroke was found and updated; otherwise, <see langword="false"/>.</returns>
    public static bool ReplaceColor(this JsonObject target, Color color)
    {
        bool applied = false;

        foreach (JsonObject element in target.EnumerateObjects())
        {
            string? type = element["ty"].GetString();

            if (type is not ("fl" or "st") || element["c"] is not JsonObject colorProperty) continue;

            colorProperty.ReplaceColorProperty(color);

            if (color.Alpha < 1) element.MapProperty("o", node => node.MapNumericValues(value => value * color.Alpha));

            applied = true;
        }

        return applied;
    }

    /// <summary>
    /// Replaces the text document value in all text keyframes belonging to the target Lottie element.
    /// </summary>
    /// <param name="target">The Lottie text element whose text keyframes are updated.</param>
    /// <param name="text">The replacement text.</param>
    /// <returns><see langword="true"/> if at least one text document was updated; otherwise, <see langword="false"/>.</returns>
    public static bool ReplaceText(this JsonObject target, string text)
    {
        if (target["t"]?["d"]?["k"] is not JsonArray keyframes) return false;

        bool applied = false;

        foreach (JsonObject keyframe in keyframes.OfType<JsonObject>())
        {
            applied |= keyframe["s"].ReplaceTextValue(text);
            applied |= keyframe["e"].ReplaceTextValue(text);
        }

        return applied;
    }

    /// <summary>
    /// Sets the opacity of the target fill or stroke to a constant value.
    /// The supplied normalized opacity is converted to the percentage scale used by Lottie.
    /// </summary>
    /// <param name="target">The Lottie fill or stroke element whose opacity is changed.</param>
    /// <param name="opacity">The normalized opacity value, where zero is transparent and one is fully opaque.</param>
    /// <returns><see langword="true"/> if an opacity property was found and updated; otherwise, <see langword="false"/>.</returns>
    public static bool SetOpacity(this JsonObject target, double opacity)
    {
        JsonObject? owner = target.GetOpacityOwner();

        if (owner is null) return false;

        owner.SetConstantProperty("o", JsonValue.Create(opacity * 100));

        return true;
    }

    /// <summary>
    /// Multiplies all numeric values in the target fill or stroke opacity property by the specified factor.
    /// Animated opacity keyframes are handled individually.
    /// </summary>
    /// <param name="target">The Lottie fill or stroke element whose opacity is scaled.</param>
    /// <param name="factor">The multiplier applied to each opacity value.</param>
    /// <returns><see langword="true"/> if an opacity property was found and updated; otherwise, <see langword="false"/>.</returns>
    public static bool MultiplyOpacity(this JsonObject target, double factor) => 
        target.GetOpacityOwner()?.MapProperty("o", node => node.MapNumericValues(value => value * factor)) ?? false;

    /// <summary>
    /// Sets the target transform position to the specified constant two-dimensional position.
    /// Separated-dimension position properties are updated through their X and Y components.
    /// </summary>
    /// <param name="target">The Lottie element or group containing the transform to update.</param>
    /// <param name="positionValue">The replacement position in Lottie composition coordinates.</param>
    /// <returns><see langword="true"/> if a transform was found and updated; otherwise, <see langword="false"/>.</returns>
    public static bool SetPosition(this JsonObject target, Point positionValue)
    {
        JsonObject? transform = target.GetTransform();

        if (transform is null) return false;

        if ((transform["p"]?["s"]).IsTrue())
        {
            JsonObject position = (JsonObject)transform["p"]!;

            position.SetConstantProperty("x", JsonValue.Create(positionValue.X));
            position.SetConstantProperty("y", JsonValue.Create(positionValue.Y));
        }
        else
        {
            transform.SetConstantProperty("p", positionValue.CreateVector());
        }

        return true;
    }

    /// <summary>
    /// Adds the specified offset to the target transform position.
    /// The offset is applied to static values and to each animated position keyframe.
    /// </summary>
    /// <param name="target">The Lottie element or group containing the transform to update.</param>
    /// <param name="offset">The horizontal and vertical position offsets.</param>
    /// <returns><see langword="true"/> if a position value was found and updated; otherwise, <see langword="false"/>.</returns>
    public static bool OffsetPosition(this JsonObject target, Point offset)
    {
        JsonObject? transform = target.GetTransform();

        if (transform is null) return false;
        if (transform["p"] is JsonObject position && position["s"].IsTrue())
        {
            bool changedX = position.MapProperty("x", node => node.MapNumericValues(value => value + offset.X));
            bool changedY = position.MapProperty("y", node => node.MapNumericValues(value => value + offset.Y));

            return changedX || changedY;
        }

        return transform.MapProperty("p", node => node.MapVectorValues((value, index) => value + (index == 0 ? offset.X : offset.Y)));
    }

    /// <summary>
    /// Sets the target transform scale to the specified constant X and Y percentages.
    /// The unit scale value is converted to Lottie's percentage representation.
    /// </summary>
    /// <param name="target">The Lottie element or group containing the transform to update.</param>
    /// <param name="scale">The horizontal and vertical scale factors, where one represents 100 percent.</param>
    /// <returns><see langword="true"/> if a transform was found and updated; otherwise, <see langword="false"/>.</returns>
    public static bool SetScale(this JsonObject target, Point scale)
    {
        JsonObject? transform = target.GetTransform();

        if (transform is null) return false;

        transform.SetConstantProperty("s", new Point(scale.X * 100, scale.Y * 100).CreateVector());

        return true;
    }

    /// <summary>
    /// Multiplies the target transform scale by the specified horizontal and vertical factors.
    /// Static values and animated scale keyframes are supported.
    /// </summary>
    /// <param name="target">The Lottie element or group containing the transform to update.</param>
    /// <param name="factor">The horizontal and vertical scale multipliers.</param>
    /// <returns><see langword="true"/> if a scale property was found and updated; otherwise, <see langword="false"/>.</returns>
    public static bool MultiplyScale(this JsonObject target, Point factor) =>
        target.GetTransform()?.MapProperty("s", node => node.MapVectorValues((value, index) => value * (index == 0 ? factor.X : factor.Y))) ?? false;

    /// <summary>
    /// Sets the target transform rotation to the specified constant angle.
    /// </summary>
    /// <param name="target">The Lottie element or group containing the transform to update.</param>
    /// <param name="degrees">The replacement rotation angle in degrees.</param>
    /// <returns><see langword="true"/> if a transform was found and updated; otherwise, <see langword="false"/>.</returns>
    public static bool SetRotation(this JsonObject target, double degrees)
    {
        JsonObject? transform = target.GetTransform();

        if (transform is null) return false;

        transform.SetConstantProperty("r", JsonValue.Create(degrees));

        return true;
    }

    /// <summary>
    /// Adds the specified angle to the target transform rotation.
    /// The offset is applied to static values and to each animated rotation keyframe.
    /// </summary>
    /// <param name="target">The Lottie element or group containing the transform to update.</param>
    /// <param name="degrees">The rotation offset in degrees.</param>
    /// <returns><see langword="true"/> if a rotation property was found and updated; otherwise, <see langword="false"/>.</returns>
    public static bool OffsetRotation(this JsonObject target, double degrees) => 
        target.GetTransform()?.MapProperty("r", node => node.MapNumericValues(value => value + degrees)) ?? false;

    private static bool ReplaceTextValue(this JsonNode? node, string text)
    {
        if (node is not JsonObject textDocument ||
            textDocument["t"] is not JsonValue currentText ||
            !currentText.TryGetValue<string>(out _)) return false;

        textDocument["t"] = JsonValue.Create(text);

        return true;
    }

    private static void ReplaceColorProperty(this JsonObject property, Color color)
    {
        if (property["a"].IsAnimated() && property["k"] is JsonArray keyframes)
        {
            foreach (JsonObject keyframe in keyframes.OfType<JsonObject>())
            {
                keyframe["s"].ReplaceColorVector(color);
                keyframe["e"].ReplaceColorVector(color);
            }

            return;
        }

        property["k"].ReplaceColorVector(color);
    }

    private static void ReplaceColorVector(this JsonNode? value, Color color)
    {
        if (value is not JsonArray vector) return;
        if (vector.Count > 0 && vector[0] is JsonArray nested)
        {
            nested.ReplaceColorVector(color);

            return;
        }
        if (vector.Count < 3) return;

        vector[0] = JsonValue.Create((double)color.Red);
        vector[1] = JsonValue.Create((double)color.Green);
        vector[2] = JsonValue.Create((double)color.Blue);
    }

    private static JsonObject? TransformCheckAll(this JsonObject target, string[] isTarget) =>
        isTarget.Contains(target["ty"].GetString())
            ? target
            : target["ks"] is JsonObject layerTransform
                ? layerTransform
                : target.FindGroupTransform();

    private static JsonObject? GetOpacityOwner(this JsonObject target) => target.TransformCheckAll(["fl", "st"]);

    private static JsonObject? GetTransform(this JsonObject target) => target.TransformCheckAll(["tr"]);

    private static JsonObject? FindGroupTransform(this JsonObject target) => 
        target["it"] is not JsonArray items ? null : items.OfType<JsonObject>().LastOrDefault(item => item["ty"].GetString() == "tr");

    private static bool MapProperty(this JsonObject owner, string propertyName, Func<JsonNode, JsonNode> map)
    {
        if (owner[propertyName] is not JsonObject property || property["k"] is null) return false;
        if (property["a"].IsAnimated() && property["k"] is JsonArray keyframes)
        {
            foreach (JsonObject keyframe in keyframes.OfType<JsonObject>())
            {
                if (keyframe["s"] is JsonNode start) keyframe["s"] = map(start);
                if (keyframe["e"] is JsonNode end) keyframe["e"] = map(end);
            }
        }
        else
        {
            property["k"] = map(property["k"]!);
        }

        return true;
    }

    private static void SetConstantProperty(this JsonObject owner, string propertyName, JsonNode? value)
    {
        if (owner[propertyName] is not JsonObject property)
        {
            property = new ();
            owner[propertyName] = property;
        }

        property["a"] = JsonValue.Create(0);
        property["k"] = value;
    }

    private static JsonNode MapNumericValues(this JsonNode node, Func<double, double> map)
    {
        if (node is JsonArray array)
        {
            JsonArray result = new ();

            foreach (JsonNode? item in array) result.Add(item is null ? null : item.MapNumericValues(map));

            return result;
        }

        return node.TryGetNumber(out double number) ? JsonValue.Create(map(number))! : node.DeepClone();
    }

    private static JsonNode MapVectorValues(this JsonNode node, Func<double, int, double> map)
    {
        if (node is not JsonArray array) return node.DeepClone();

        JsonArray result = new ();

        if (array.Count > 0 && array[0] is JsonArray nested)
        {
            result.Add(nested.MapVectorValues(map));

            for (int i = 1; i < array.Count; i++) result.Add(array[i]?.DeepClone());

            return result;
        }

        for (int i = 0; i < array.Count; i++)
        {
            if (i < 2 && array[i].TryGetNumber(out double number)) result.Add(JsonValue.Create(map(number, i)));
            else result.Add(array[i]?.DeepClone());
        }

        return result;
    }

    private static JsonArray CreateVector(this Point value) => new(JsonValue.Create(value.X), JsonValue.Create(value.Y));

    private static bool TryGetNumber(this JsonNode? node, out double number)
    {
        number = 0;

        return node is JsonValue value && value.TryGetValue(out number) && double.IsFinite(number);
    }

    private static bool IsAnimated(this JsonNode? node) => node.TryGetNumber(out double value) && value == 1;

    private static bool IsTrue(this JsonNode? node) => 
        node is not JsonValue value 
            ? false 
            : value.TryGetValue(out bool boolean) 
                ? boolean 
                : node.TryGetNumber(out double number) && number != 0;

    internal static string? GetString(this JsonNode? node) => node is JsonValue value && value.TryGetValue(out string? text) ? text : null;

    internal static IEnumerable<JsonObject> EnumerateObjects(this JsonNode? node)
    {
        if (node is JsonObject obj) yield return obj;

        IEnumerable<JsonNode?> children = node switch
        {
            JsonObject jsonObject => jsonObject.Select(pair => pair.Value),
            JsonArray jsonArray => jsonArray,
            _ => Enumerable.Empty<JsonNode?>()
        };

        foreach (JsonObject descendant in children.SelectMany(EnumerateObjects)) yield return descendant;
    }
}
