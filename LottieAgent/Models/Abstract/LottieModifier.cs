using LottieAgent.Interfaces.Base;

namespace LottieAgent.Models.Abstract
{
    /// <summary>
    /// Non-generic base record allows a collection to contain modifiers with different TValue types.
    /// </summary>
    public abstract record LottieModifier
    {
        protected LottieModifier(string targetName, ILottieModifierValue value)
        {
            if (string.IsNullOrWhiteSpace(targetName)) throw new ArgumentException("A Lottie element name is required.", nameof(targetName));

            TargetName = targetName;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string TargetName { get; }

        public ILottieModifierValue Value { get; }
    }
}
