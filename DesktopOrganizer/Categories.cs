using System.Text.Json;

namespace DesktopOrganizer
{
    public static class Categories
    {
        // Built-in defaults, used when no custom settings.json exists.
        private static readonly Dictionary<string, string[]> Defaults = new()
        {
            ["Images"] = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg", ".ico", ".tiff", ".heic"],
            ["PDFs"] = [".pdf"],
            ["Documents"] = [".doc", ".docx", ".rtf", ".odt"],
            ["Text Files"] = [".txt", ".md"],
            ["Spreadsheets"] = [".xls", ".xlsx", ".csv", ".ods"],
            ["Videos"] = [".mp4", ".mov", ".avi", ".mkv", ".wmv", ".webm", ".flv"],
            ["Audio"] = [".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a", ".wma"],
            ["Archives"] = [".zip", ".rar", ".7z", ".tar", ".gz", ".iso"],
            ["App Shortcuts"] = [".lnk", ".url", ".appref-ms"],
        };

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DesktopOrganizer", "settings.json");

        // The active mapping: custom settings if saved, otherwise the defaults.
        public static Dictionary<string, string[]> Map { get; private set; } = Load();

        public static bool HasCustomSettings => File.Exists(SettingsPath);

        public static string GetCategory(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            foreach (var (category, extensions) in Map)
            {
                if (extensions.Contains(extension))
                    return category;
            }

            return "Other";
        }

        // Persists a custom mapping to settings.json and makes it active.
        public static void Save(Dictionary<string, string[]> map)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath,
                JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true }));
            Map = map;
        }

        // Deletes settings.json and restores the built-in defaults.
        public static void ResetToDefaults()
        {
            if (File.Exists(SettingsPath))
                File.Delete(SettingsPath);

            Map = CopyOfDefaults();
        }

        private static Dictionary<string, string[]> Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var custom = JsonSerializer.Deserialize<Dictionary<string, string[]>>(
                        File.ReadAllText(SettingsPath));

                    if (custom is { Count: > 0 })
                        return custom;
                }
            }
            catch (Exception)
            {
                // Corrupt settings file — fall back to defaults.
            }

            return CopyOfDefaults();
        }

        private static Dictionary<string, string[]> CopyOfDefaults() =>
            Defaults.ToDictionary(kvp => kvp.Key, kvp => (string[])kvp.Value.Clone());
    }
}
