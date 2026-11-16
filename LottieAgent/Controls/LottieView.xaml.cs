using System.Diagnostics;
using System.Text;
using LottieAnimation = SkiaSharp.Skottie.Animation;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using LottieAgent.Models;
using LottieAgent.Interfaces;
using LottieAgent.Services;

namespace LottieAgent.Controls;

/// <summary>
/// Displays a Lottie JSON animation using SkiaSharp.Skottie.
/// Modifiers are applied to a copy of the JSON before the animation is created.
/// </summary>
public partial class LottieView : SkiaSharp.Views.Maui.Controls.SKCanvasView
{
    private readonly Stopwatch stopwatch = new();
    private ILottieJsonLoader jsonLoader;
    private ILottieModifierProcessor modifierProcessor;
    private LottieAnimation? animation;
    private string? sourceJson;
    private bool isTimerRunning;
    private int loadVersion;

    public static readonly BindableProperty SourceProperty =
        BindableProperty.Create(
            nameof(Source),
            typeof(string),
            typeof(LottieView),
            default(string),
            propertyChanged: (bindable, _, value) => ((LottieView)bindable).LoadSourceAsync((string?)value));

    public static readonly BindableProperty ModifiersProperty =
        BindableProperty.Create(
            nameof(Modifiers),
            typeof(LottieModifierCollection),
            typeof(LottieView),
            default(LottieModifierCollection),
            propertyChanged: OnModifiersChanged);

    public static readonly BindableProperty JsonLoaderProperty =
        BindableProperty.Create(
            nameof(JsonLoader),
            typeof(ILottieJsonLoader),
            typeof(LottieView),
            default(ILottieJsonLoader),
            propertyChanged: OnJsonLoaderChanged);

    public static readonly BindableProperty ModifierProcessorProperty =
        BindableProperty.Create(
            nameof(ModifierProcessor),
            typeof(ILottieModifierProcessor),
            typeof(LottieView),
            default(ILottieModifierProcessor),
            propertyChanged: OnModifierProcessorChanged);

    public static readonly BindableProperty IsPlayingProperty =
        BindableProperty.Create(
            nameof(IsPlaying),
            typeof(bool),
            typeof(LottieView),
            false,
            propertyChanged: (bindable, _, value) => ((LottieView)bindable).UpdatePlayback((bool)value));

    public static readonly BindableProperty IsLoopingProperty =
        BindableProperty.Create(nameof(IsLooping), typeof(bool), typeof(LottieView), true);

    public static readonly BindableProperty SpeedProperty =
        BindableProperty.Create(
            nameof(Speed),
            typeof(double),
            typeof(LottieView),
            1d,
            validateValue: (_, value) => value is double speed && speed > 0);

    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(
            nameof(Progress),
            typeof(double),
            typeof(LottieView),
            0d,
            BindingMode.TwoWay,
            validateValue: (_, value) => value is double progress && progress >= 0 && progress <= 1,
            propertyChanged: (bindable, _, value) => ((LottieView)bindable).Seek((double)value));

    public string? Source
    {
        get => (string?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Named overrides applied to the source JSON before Skottie loads it.
    /// Updating the collection rebuilds the animation.
    /// </summary>
    public LottieModifierCollection Modifiers
    {
        get => (LottieModifierCollection)GetValue(ModifiersProperty);
        set => SetValue(ModifiersProperty, value ?? new LottieModifierCollection());
    }

    /// <summary>
    /// Asset loader. Can be supplied from dependency injection.
    /// </summary>
    public ILottieJsonLoader JsonLoader
    {
        get => (ILottieJsonLoader)GetValue(JsonLoaderProperty);
        set => SetValue(JsonLoaderProperty, value);
    }

    /// <summary>
    /// Modifier processor. Can be supplied from dependency injection.
    /// </summary>
    public ILottieModifierProcessor ModifierProcessor
    {
        get => (ILottieModifierProcessor)GetValue(ModifierProcessorProperty);
        set => SetValue(ModifierProcessorProperty, value);
    }

    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    public bool IsLooping
    {
        get => (bool)GetValue(IsLoopingProperty);
        set => SetValue(IsLoopingProperty, value);
    }

    /// <summary>
    /// Playback multiplier. Must be greater than zero.
    /// </summary>
    public double Speed
    {
        get => (double)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    /// <summary>
    /// Normalized animation position in the range [0, 1].
    /// </summary>
    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public event EventHandler? AnimationLoaded;
    public event Action<Exception>? AnimationFailed;

    /// <summary>
    /// Raised when a modifier target is not found or its property is unsupported.
    /// </summary>
    public event Action<string>? ModifierNotApplied;

    /// <summary>
    /// Parameterless constructor used by XAML. It creates the built-in services.
    /// When created through DI, use the constructor accepting service interfaces.
    /// </summary>
    public LottieView() : this(new MauiLottieJsonLoader(), LottieModifierProcessor.CreateDefault())
    {

    }

    public LottieView(ILottieJsonLoader jsonLoader, ILottieModifierProcessor modifierProcessor)
    {
        this.jsonLoader = jsonLoader ?? throw new ArgumentNullException(nameof(jsonLoader));
        this.modifierProcessor = modifierProcessor ?? throw new ArgumentNullException(nameof(modifierProcessor));

        SetValue(ModifiersProperty, new LottieModifierCollection());
        SetValue(JsonLoaderProperty, jsonLoader);
        SetValue(ModifierProcessorProperty, modifierProcessor);

        InitializeComponent();
    }

    public void Play()
    {
        if (Progress >= 1) Progress = 0;

        IsPlaying = true;
    }

    public void Pause() => IsPlaying = false;

    public void Stop()
    {
        IsPlaying = false;
        Progress = 0;
    }

    private static void OnModifiersChanged(BindableObject bindable, object oldValue, object newValue)
    {
        LottieView view = (LottieView)bindable;

        if (oldValue is LottieModifierCollection oldCollection) oldCollection.CollectionChanged -= view.OnModifiersCollectionChanged;
        if (newValue is LottieModifierCollection newCollection) newCollection.CollectionChanged += view.OnModifiersCollectionChanged;

        view.RebuildAnimation();
    }

    private void OnModifiersCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => RebuildAnimation();

    private static void OnJsonLoaderChanged(BindableObject bindable, object oldValue, object newValue)
    {
        LottieView view = (LottieView)bindable;
        view.jsonLoader = newValue as ILottieJsonLoader ?? new MauiLottieJsonLoader();

        if (!string.IsNullOrWhiteSpace(view.Source)) view.LoadSourceAsync(view.Source);
    }

    private static void OnModifierProcessorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        LottieView view = (LottieView)bindable;
        view.modifierProcessor = newValue as ILottieModifierProcessor ?? LottieModifierProcessor.CreateDefault();

        view.RebuildAnimation();
    }

    private async void LoadSourceAsync(string? source)
    {
        int version = Interlocked.Increment(ref loadVersion);

        stopwatch.Stop();
        animation?.Dispose();

        animation = null;
        sourceJson = null;
        Progress = 0;

        InvalidateSurface();

        if (string.IsNullOrWhiteSpace(source)) return;

        try
        {
            string json = await jsonLoader.LoadAsync(source);

            if (version != loadVersion) return;

            sourceJson = json;

            RebuildAnimation();
        }
        catch (Exception exception)
        {
            if (version == loadVersion) AnimationFailed?.Invoke(exception);
        }
    }

    private void RebuildAnimation()
    {
        if (string.IsNullOrWhiteSpace(sourceJson)) return;

        try
        {
            LottieModifierResult result = modifierProcessor.Apply(sourceJson, Modifiers);

            foreach (string targetName in result.NotApplied.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                ModifierNotApplied?.Invoke(targetName);
            }

            using MemoryStream stream = new (Encoding.UTF8.GetBytes(result.Json));
            LottieAnimation? rebuiltAnimation = LottieAnimation.Create(stream);

            if (rebuiltAnimation is null) throw new InvalidDataException("Skottie could not create an animation from the modified JSON.");

            rebuiltAnimation.Seek(Progress);

            LottieAnimation? previousAnimation = animation;
            animation = rebuiltAnimation;

            previousAnimation?.Dispose();

            InvalidateSurface();

            UpdatePlayback(IsPlaying);

            AnimationLoaded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception exception)
        {
            AnimationFailed?.Invoke(exception);
        }
    }

    private void UpdatePlayback(bool isPlaying)
    {
        if (!isPlaying || animation is null)
        {
            stopwatch.Stop();

            return;
        }

        stopwatch.Restart();

        if (isTimerRunning) return;

        isTimerRunning = true;

        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), OnAnimationTick);
    }

    private bool OnAnimationTick()
    {
        if (!IsPlaying || animation is null)
        {
            isTimerRunning = false;

            return false;
        }

        double duration = animation.Duration.TotalSeconds;

        if (duration <= 0)
        {
            IsPlaying = false;
            isTimerRunning = false;

            return false;
        }

        double nextProgress = Progress + (stopwatch.Elapsed.TotalSeconds * Speed / duration);

        stopwatch.Restart();

        if (nextProgress >= 1)
        {
            if (IsLooping)
            {
                nextProgress %= 1;
            }
            else
            {
                Progress = 1;
                IsPlaying = false;
                isTimerRunning = false;

                return false;
            }
        }

        Progress = nextProgress;

        return IsPlaying;
    }

    private void Seek(double progress)
    {
        if (animation is null) return;

        animation.Seek(progress);
        InvalidateSurface();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear();

        if (animation is null) return;

        SKRect destination = new(0, 0, e.Info.Width, e.Info.Height);
        animation.Render(canvas, destination);
    }

    private void OnUnloaded(object? sender, EventArgs e) => IsPlaying = false;
}
