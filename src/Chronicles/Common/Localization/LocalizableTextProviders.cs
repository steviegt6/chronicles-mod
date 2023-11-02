using Terraria.Localization;

namespace Chronicles.Common.Localization;

internal interface ILocalizableTextProvider {
    string GetText(object?[] arguments);

    bool IsLoaded();
}

internal sealed class LiteralLocalizableTextProvider : ILocalizableTextProvider {
    private readonly string text;

    public LiteralLocalizableTextProvider(string text) {
        this.text = text;
    }

    public string GetText(object?[] arguments) {
        return string.Format(text, arguments);
    }

    public bool IsLoaded() {
        return true;
    }
}

internal sealed class KeyLocalizableTextProvider : ILocalizableTextProvider {
    private readonly string key;

    public KeyLocalizableTextProvider(string key) {
        this.key = key;
    }

    public string GetText(object?[] arguments) {
        return Language.GetTextValue(key, arguments);
    }

    public bool IsLoaded() {
        return Language.Exists(key);
    }
}

internal sealed class LocalizedTextLocalizableTextProvider : ILocalizableTextProvider {
    private readonly LocalizedText text;

    public LocalizedTextLocalizableTextProvider(LocalizedText text) {
        this.text = text;
    }

    // Honestly, there should never be a case where the instance isn't already
    // formatted... but better safe than sorry.
    public string GetText(object?[] arguments) {
        return Language.GetTextValue(text.Key, arguments);
    }

    // Nothing stopping you from getting an instance that doesn't exist... so
    // take this.
    public bool IsLoaded() {
        return Language.Exists(text.Key);
    }
}
