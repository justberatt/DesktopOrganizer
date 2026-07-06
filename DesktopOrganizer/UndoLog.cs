using System.Text.Json;

namespace DesktopOrganizer
{
    public class MoveRecord
    {
        public string OriginalPath { get; set; } = "";
        public string NewPath { get; set; } = "";
    }

    public static class UndoLog
    {
        private static string LogPath =>
            Path.Combine(Organizer.GetDesktopPath(), ".organizer_log.json");

        public static bool Exists => File.Exists(LogPath);

        public static void Save(List<MoveRecord> records)
        {
            string json = JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });

            // Windows refuses to overwrite a hidden file with a normal write,
            // so un-hide the existing log before replacing it.
            if (File.Exists(LogPath))
                File.SetAttributes(LogPath, FileAttributes.Normal);

            File.WriteAllText(LogPath, json);
            File.SetAttributes(LogPath, File.GetAttributes(LogPath) | FileAttributes.Hidden);
        }

        public static List<MoveRecord> Load()
        {
            if (!Exists)
                return [];

            string json = File.ReadAllText(LogPath);
            return JsonSerializer.Deserialize<List<MoveRecord>>(json) ?? [];
        }

        // Moves every file back to where it came from. Returns how many were restored.
        public static int Undo()
        {
            var records = Load();
            int restored = 0;

            foreach (var record in records)
            {
                try
                {
                    if (File.Exists(record.NewPath) && !File.Exists(record.OriginalPath))
                    {
                        File.Move(record.NewPath, record.OriginalPath);
                        restored++;
                    }
                }
                catch (IOException)
                {
                    // File is locked or in use — leave it where it is.
                }
            }

            // Remove category folders this undo emptied out — Organize created
            // them, so a full undo shouldn't leave empty husks behind. Folders
            // that still contain anything are left alone.
            foreach (string folder in records
                .Select(r => Path.GetDirectoryName(r.NewPath)!)
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    if (Directory.Exists(folder) && !Directory.EnumerateFileSystemEntries(folder).Any())
                        Directory.Delete(folder);
                }
                catch (IOException)
                {
                    // Folder locked (e.g. open in Explorer) — leave it.
                }
            }

            File.Delete(LogPath);
            return restored;
        }
    }
}
