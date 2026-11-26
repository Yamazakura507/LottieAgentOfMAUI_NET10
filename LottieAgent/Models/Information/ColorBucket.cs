namespace LottieAgent.Models.Information
{
    internal sealed class ColorBucket
    {
        private double red, green, blue;

        public double Weight { get; private set; }
        public int Count { get; private set; }

        public void Add(Color color, double weight)
        {
            red += color.Red * weight;
            green += color.Green * weight;
            blue += color.Blue * weight;
            Weight += weight;
            Count++;
        }

        public Color ToColor() => new((float)(red / Weight), (float)(green / Weight), (float)(blue / Weight), 1f);
    }
}
