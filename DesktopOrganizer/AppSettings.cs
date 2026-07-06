using System.Text.Json;

namespace DesktopOrganizer
{
    // App state that survives restarts, stored in AppData\Local\DesktopOrganizer.
    public class AppSettings
    {
        public bool AutoSortEnabled { get; set; }

        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DesktopOrganizer", "appstate.json");

        public static AppSettings Current { get; } = Load();

        private static AppSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new();
            }
            catch (Exception)
            {
                // Corrupt or unreadable settings file — fall back to defaults.
            }

            return new();
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
        }
    }
}
