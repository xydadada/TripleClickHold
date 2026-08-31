using System.Text.Json;
using System.Text.Json.Serialization;

namespace TripleClickHold;

internal sealed record TripleSettings(
    int ClickCount = 3,
    int MinDelayMs = 0,
    int MaxDelayMs = 0,
    bool RandomDelay = false,
    bool HoldLastDown = true,
    bool LeftEnabled = true,
    bool RightEnabled = true,
    uint ToggleModifiers = 0,
    uint ToggleKey = NativeMethods.VkF8,
    uint ExitModifiers = NativeMethods.ModControl | NativeMethods.ModAlt,
    uint ExitKey = NativeMethods.VkF11,
    bool StartEnabled = false,
    bool ShowTrayStatus = true)
{
    internal static TripleSettings Default => new();

    internal TripleSettings Normalized()
    {
        var normalized = this with
        {
            ClickCount = Math.Clamp(ClickCount, 1, 20),
            MinDelayMs = Math.Clamp(MinDelayMs, 0, 100),
            MaxDelayMs = Math.Clamp(MaxDelayMs, 0, 100),
            ToggleKey = KeyList.IsValid(ToggleKey) ? ToggleKey : NativeMethods.VkF8,
            ExitKey = KeyList.IsValid(ExitKey) ? ExitKey : NativeMethods.VkF11
        };
        return normalized with { MaxDelayMs = Math.Max(normalized.MinDelayMs, normalized.MaxDelayMs) };
    }
}

internal sealed class SettingsState
{
    private TripleSettings _current;

    internal SettingsState(TripleSettings initial) => _current = initial.Normalized();

    internal TripleSettings Current => Volatile.Read(ref _current);

    internal void Set(TripleSettings value) => Volatile.Write(ref _current, value.Normalized());
}

internal static class SettingsStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    internal static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TripleClickHold", "settings.json");

    internal static TripleSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return TripleSettings.Default;
            var value = JsonSerializer.Deserialize<TripleSettings>(File.ReadAllText(FilePath), Options);
            return (value ?? TripleSettings.Default).Normalized();
        }
        catch
        {
            return TripleSettings.Default;
        }
    }

    internal static bool Save(TripleSettings value, out string error)
    {
        try
        {
            var path = FilePath;
            var directory = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(directory);
            var temp = path + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(value.Normalized(), Options));
            File.Move(temp, path, true);
            error = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}

internal static class KeyList
{
    internal static readonly (string Name, uint Value)[] Items =
    [
        ("F1", 0x70), ("F2", 0x71), ("F3", 0x72), ("F4", 0x73),
        ("F5", 0x74), ("F6", 0x75), ("F7", 0x76), ("F8", 0x77),
        ("F9", 0x78), ("F10", 0x79), ("F11", 0x7A), ("F12", 0x7B)
    ];

    internal static bool IsValid(uint key) => Items.Any(item => item.Value == key);

    internal static int IndexOf(uint key)
    {
        var index = Array.FindIndex(Items, item => item.Value == key);
        return index < 0 ? 7 : index;
    }
}
