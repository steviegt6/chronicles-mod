using Terraria.Localization;

namespace Chronicles.Common.Localization;

internal readonly struct LocalizableText {
    private readonly ILocalizableTextProvider provider;

    public object[] Arguments { get; }

    private LocalizableText(ILocalizableTextProvider provider, params object[] arguments) {
        this.provider = provider;
        Arguments = arguments;
    }

    public LocalizableText WithArguments(params object[] arguments) {
        return new LocalizableText(provider, arguments);
    }

    public override string ToString() {
        return MaskWhenNotYetLoaded(provider, Arguments);
    }

    private static string MaskWhenNotYetLoaded(ILocalizableTextProvider provider, object?[] arguments) {
        const string not_yet_loaded_msg = "Localization not yet loaded...";
        return !provider.IsLoaded() ? not_yet_loaded_msg : provider.GetText(arguments);
    }

    public static LocalizableText FromLiteral(string text, params object[] args) => new(new LiteralLocalizableTextProvider(text), args);

    public static LocalizableText FromKey(string key, params object[] args) => new(new KeyLocalizableTextProvider(key), args);

    public static LocalizableText FromLocalizedText(LocalizedText text, params object[] args) => new(new LocalizedTextLocalizableTextProvider(text), args);
}
