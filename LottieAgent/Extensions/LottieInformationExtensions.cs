using System.Text.Json;
using LottieAgent.Enums;
using LottieAgent.Models;
using LottieAgent.Models.Information;

namespace LottieAgent.Extensions;

/// <summary>
/// Reads structural and visual information from Lottie JSON documents.
/// </summary>
public static class LottieInformationExtensions
{
    /// <summary>
    /// Gets the dominant visible fill or stroke color at normalized animation progress.
    /// </summary>
    public static Color? GetDominantColor(this string json, double progress = 0d) => json.TryGetDominantColor(progress, out Color color) ? color : null;

    /// <summary>
    /// Attempts to get the dominant visible fill or stroke color at normalized progress.
    /// </summary>
    public static bool TryGetDominantColor(this string json, double progress, out Color color)
    {
        color = Colors.Transparent;

        if (string.IsNullOrWhiteSpace(json) || !double.IsFinite(progress)) return false;

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;
            double frame = root.GetNumber("ip", 0d) + (root.GetNumber("op", 1d) - root.GetNumber("ip", 0d)) * Math.Clamp(progress, 0d, 1d);

            Dictionary<(int R, int G, int B), ColorBucket> buckets = new();

            foreach (JsonElement element in root.EnumerateVisibleObjects())
            {
                if (element.GetString("ty") is not ("fl" or "st")
                    || !element.TryGetProperty("c", out JsonElement colorProperty)
                    || !colorProperty.TryReadVector(frame, out double[] rgba)
                    || rgba.Length < 3) continue;

                if (rgba[0] is < 0d or > 1d || rgba[1] is < 0d or > 1d || rgba[2] is < 0d or > 1d) continue;

                double alpha = rgba.Length > 3 ? Math.Clamp(rgba[3], 0d, 1d) : 1d;

                if (element.TryGetProperty("o", out JsonElement opacity) && opacity.TryReadNumber(frame, out double value))
                    alpha *= Math.Clamp(value > 1d ? value / 100d : value, 0d, 1d);

                if (alpha <= 0d) continue;

                Color current = new((float)rgba[0], (float)rgba[1], (float)rgba[2], (float)alpha);

                (int,int,int) key = 
                    ((int)Math.Round(current.Red * 255), 
                    (int)Math.Round(current.Green * 255), 
                    (int)Math.Round(current.Blue * 255));

                if (!buckets.TryGetValue(key, out ColorBucket? bucket)) buckets[key] = bucket = new ColorBucket();

                bucket.Add(current, alpha);
            }

            ColorBucket? dominant = buckets.Values.OrderByDescending(x => x.Weight).ThenByDescending(x => x.Count).FirstOrDefault();

            if (dominant is null || dominant.Weight <= 0d) return false;

            color = dominant.ToColor();

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the Lottie composition aspect ratio, or one when dimensions are unavailable.
    /// </summary>
    public static double GetCompositionAspectRatio(this string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return 1d;

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            double width = document.RootElement.GetNumber("w", 0d);
            double height = document.RootElement.GetNumber("h", 0d);

            return width > 0d && height > 0d ? width / height : 1d;
        }
        catch (JsonException) 
        { 
            return 1d; 
        }
    }

    /// <summary>
    /// Gets all named targets that contain properties supported by current modifier handlers.
    /// </summary>
    public static IReadOnlyList<LottieModifierParameterTag> GetModifierParameterTags(this string json)
        => json.TryGetModifierParameterTags(out IReadOnlyList<LottieModifierParameterTag> tags)
            ? tags
            : Array.Empty<LottieModifierParameterTag>();

    /// <summary>
    /// Attempts to return available modifier targets and their supported modifier operations.
    /// </summary>
    public static bool TryGetModifierParameterTags(this string json, out IReadOnlyList<LottieModifierParameterTag> tags)
    {
        tags = Array.Empty<LottieModifierParameterTag>();

        if (string.IsNullOrWhiteSpace(json)) return false;

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            Dictionary<string, HashSet<LottieModifierKind>> result = new(StringComparer.OrdinalIgnoreCase);

            foreach (JsonElement target in document.RootElement.EnumerateObjects())
            {
                string? name = target.GetString("nm");

                if (string.IsNullOrWhiteSpace(name)) continue;

                HashSet<LottieModifierKind> supported = target.EnumerateObjects().ToArray().GetSupportedModifiers();

                if (supported.Count == 0) continue;
                if (!result.TryGetValue(name, out HashSet<LottieModifierKind>? existing)) result[name] = existing = new ();

                existing.UnionWith(supported);
            }

            tags = result
                    .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(x => new LottieModifierParameterTag(x.Key, x.Value.OrderBy(v => v).ToArray()))
                    .ToArray();

            return true;
        }
        catch (JsonException) 
        { 
            return false; 
        }
    }

    private static HashSet<LottieModifierKind> GetSupportedModifiers(this JsonElement[] items)
    {
        HashSet<LottieModifierKind> result = new();

        if (items.Any(x => x.GetString("ty") is "fl" or "st" && x.TryGetProperty("c", out _))) result.Add(LottieModifierKind.Color);
        if (items.Any(x => x.TryGetProperty("t", out JsonElement t)
            && t.TryGetProperty("d", out JsonElement d) 
            && d.TryGetProperty("k", out _))) result.Add(LottieModifierKind.Text);
        if (items.Any(x => (x.GetString("ty") is "fl" or "st" or "tr" && x.TryGetProperty("o", out _))
            || (x.TryGetProperty("ks", out JsonElement ks) && ks.TryGetProperty("o", out _)))) result.Add(LottieModifierKind.Opacity);
        if (items.HasTransform("p")) result.Add(LottieModifierKind.Position);
        if (items.HasTransform("s")) result.Add(LottieModifierKind.Scale);
        if (items.HasTransform("r")) result.Add(LottieModifierKind.Rotation);

        return result;
    }

    private static bool HasTransform(this IEnumerable<JsonElement> items, string property)
        => items.Any(x => (x.GetString("ty") == "tr" && x.TryGetProperty(property, out _))
            || (x.TryGetProperty("ks", out JsonElement ks) && ks.TryGetProperty(property, out _)));

    private static IEnumerable<JsonElement> EnumerateVisibleObjects(this JsonElement element, bool hiddenByParent = false)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            bool hidden = hiddenByParent || (element.TryGetProperty("hd", out JsonElement h) && h.ValueKind == JsonValueKind.True);

            if (hidden) yield break;

            yield return element;

            foreach (JsonProperty property in element.EnumerateObject())
                foreach (JsonElement child in property.Value.EnumerateVisibleObjects(hidden)) yield return child;
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in element.EnumerateArray())
                foreach (JsonElement child in item.EnumerateVisibleObjects(hiddenByParent)) yield return child;
        }
    }

    private static IEnumerable<JsonElement> EnumerateObjects(this JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            yield return element;

            foreach (JsonProperty property in element.EnumerateObject())
                foreach (JsonElement child in property.Value.EnumerateObjects()) yield return child;
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in element.EnumerateArray())
                foreach (JsonElement child in item.EnumerateObjects()) yield return child;
        }
    }

    private static bool TryReadVector(this JsonElement property, double frame, out double[] values)
    {
        values = Array.Empty<double>();

        if (!property.TryGetProperty("k", out JsonElement k)) return false;
        if (property.GetNumber("a", 0d) == 1d 
            && k.ValueKind == JsonValueKind.Array
            && k.GetArrayLength() > 0 
            && k[0].ValueKind == JsonValueKind.Object 
            && k[0].TryGetProperty("t", out _)) return k.TryInterpolateVector(frame, out values);

        return k.TryReadVectorValue(out values);
    }

    private static bool TryInterpolateVector(this JsonElement keyframes, double frame, out double[] values)
    {
        values = Array.Empty<double>();

        JsonElement[] frames = keyframes.EnumerateArray().ToArray();

        if (frames.Length == 0) return false;

        int index = 0;

        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i].GetNumber("t", double.PositiveInfinity) > frame) break;

            index = i;
        }

        JsonElement current = frames[index];

        if (!current.TryGetProperty("s", out JsonElement startNode) || !startNode.TryReadVectorValue(out double[] start)) return false;
        if (current.GetNumber("h", 0d) == 1d || index >= frames.Length - 1)
        {
            values = start;

            return true;
        }
        
        JsonElement next = frames[index + 1];
        JsonElement endNode = current.GetEndNode(next);

        if (endNode.ValueKind == JsonValueKind.Undefined || !endNode.TryReadVectorValue(out double[] end) || end.Length != start.Length)
        {
            values = start;

            return true;
        }

        double startTime = current.GetNumber("t", frame);
        double endTime = next.GetNumber("t", startTime);
        double t = endTime > startTime ? Math.Clamp((frame - startTime) / (endTime - startTime), 0d, 1d) : 0d;

        values = start.Select((v, i) => v + (end[i] - v) * t).ToArray();

        return true;
    }

    private static bool TryReadVectorValue(this JsonElement node, out double[] values)
    {
        values = Array.Empty<double>();

        if (node.ValueKind != JsonValueKind.Array) return false;
        if (node.GetArrayLength() > 0 && node[0].ValueKind == JsonValueKind.Array) return node[0].TryReadVectorValue(out values);

        List<double> numbers = new();

        foreach (JsonElement item in node.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Number || !item.TryGetDouble(out double value)) return false;

            numbers.Add(value);
        }

        values = numbers.ToArray();

        return values.Length > 0;
    }

    private static bool TryReadNumber(this JsonElement property, double frame, out double number)
    {
        number = 0d;

        if (!property.TryGetProperty("k", out JsonElement k)) return false;
        if (property.GetNumber("a", 0d) != 1d 
            || k.ValueKind != JsonValueKind.Array
            || k.GetArrayLength() == 0 
            || k[0].ValueKind != JsonValueKind.Object 
            || !k[0].TryGetProperty("t", out _)) return k.TryReadNumericValue(out number);

        JsonElement[] frames = k.EnumerateArray().ToArray();

        int index = 0;

        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i].GetNumber("t", double.PositiveInfinity) > frame) break;

            index = i;
        }

        JsonElement current = frames[index];

        if (!current.TryGetProperty("s", out JsonElement startNode) || !startNode.TryReadNumericValue(out double start)) return false;
        if (current.GetNumber("h", 0d) == 1d || index >= frames.Length - 1)
        {
            number = start;

            return true;
        }

        JsonElement next = frames[index + 1];
        JsonElement endNode = current.GetEndNode(next);

        if (!endNode.TryReadNumericValue(out double end))
        {
            number = start;

            return true;
        }

        double a = current.GetNumber("t", frame), b = next.GetNumber("t", a);
        double t = b > a ? Math.Clamp((frame - a) / (b - a), 0d, 1d) : 0d;

        number = start + (end - start) * t;

        return true;
    }

    private static bool TryReadNumericValue(this JsonElement node, out double number)
    {
        number = 0d;

        if (node.ValueKind == JsonValueKind.Array)
        {
            if (node.GetArrayLength() == 0) return false;

            node = node[0];
        }

        return node.ValueKind == JsonValueKind.Number && node.TryGetDouble(out number);
    }

    private static JsonElement GetEndNode(this JsonElement current, JsonElement next) 
        => current.TryGetProperty("e", out JsonElement e)
                ? e
                : next.TryGetProperty("s", out JsonElement n)
                    ? n
                    : default;

    private static string? GetString(this JsonElement node, string key)
        => node.ValueKind == JsonValueKind.Object && node.TryGetProperty(key, out JsonElement value)
            && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static double GetNumber(this JsonElement node, string key, double fallback)
        => node.ValueKind == JsonValueKind.Object && node.TryGetProperty(key, out JsonElement value)
            && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out double number) ? number : fallback;
}
