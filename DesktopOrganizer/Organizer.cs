namespace DesktopOrganizer
{
    public static class Organizer
    {
        // Files that couldn't be moved during the last Organize run (locked, in use, etc.)
        public static List<string> SkippedFiles { get; } = [];

        public static string GetDesktopPath() =>
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        private static bool IsHiddenOrSystem(string filePath)
        {
            var attributes = File.GetAttributes(filePath);
            return attributes.HasFlag(FileAttributes.Hidden) || attributes.HasFlag(FileAttributes.System);
        }

        // Scans the desktop and moves each file into its category folder.
        // With dryRun = true nothing is moved — the returned records are the preview.
        public static List<MoveRecord> Organize(bool dryRun)
        {
            SkippedFiles.Clear();
            var records = new List<MoveRecord>();
            string desktop = GetDesktopPath();

            foreach (string filePath in Directory.GetFiles(desktop))
            {
                string fileName = Path.GetFileName(filePath);

                // Skip hidden files (including our own undo log) and system
                // files like desktop.ini, which Explorer hides via attributes.
                if (fileName.StartsWith('.') || IsHiddenOrSystem(filePath))
                    continue;

                string category = Categories.GetCategory(filePath);
                string targetFolder = Path.Combine(desktop, category);
                string targetPath = Path.Combine(targetFolder, fileName);

                if (dryRun)
                {
                    records.Add(new MoveRecord { OriginalPath = filePath, NewPath = targetPath });
                    continue;
                }

                try
                {
                    Directory.CreateDirectory(targetFolder);

                    if (File.Exists(targetPath))
                        targetPath = GetConflictFreePath(targetPath);

                    File.Move(filePath, targetPath);
                    records.Add(new MoveRecord { OriginalPath = filePath, NewPath = targetPath });
                }
                catch (IOException)
                {
                    SkippedFiles.Add(fileName);
                }
                catch (UnauthorizedAccessException)
                {
                    SkippedFiles.Add(fileName);
                }
            }

            return records;
        }

        // Moves a single file into its category folder. Returns null if the
        // file was skipped (hidden or already gone). Throws IOException if locked.
        public static MoveRecord? OrganizeFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            if (fileName.StartsWith('.') || !File.Exists(filePath) || IsHiddenOrSystem(filePath))
                return null;

            string targetFolder = Path.Combine(GetDesktopPath(), Categories.GetCategory(filePath));
            Directory.CreateDirectory(targetFolder);

            string targetPath = Path.Combine(targetFolder, fileName);
            if (File.Exists(targetPath))
                targetPath = GetConflictFreePath(targetPath);

            File.Move(filePath, targetPath);
            return new MoveRecord { OriginalPath = filePath, NewPath = targetPath };
        }

        // ----- Auto-sort (file watcher) -----

        private static FileSystemWatcher? watcher;

        // Raised (on a background thread!) with the file name after an auto-sort.
        public static event Action<string>? FileAutoSorted;

        public static bool IsWatching => watcher != null;

        public static void StartWatching()
        {
            if (watcher != null)
                return;

            watcher = new FileSystemWatcher(GetDesktopPath())
            {
                Filter = "*.*",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            };
            watcher.Created += OnFileCreated;
            watcher.EnableRaisingEvents = true;
        }

        public static void StopWatching()
        {
            if (watcher == null)
                return;

            watcher.EnableRaisingEvents = false;
            watcher.Created -= OnFileCreated;
            watcher.Dispose();
            watcher = null;
        }

        private static async void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            // Give the file time to finish copying/downloading.
            await Task.Delay(500);

            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    var record = OrganizeFile(e.FullPath);
                    if (record != null)
                        FileAutoSorted?.Invoke(Path.GetFileName(record.OriginalPath));
                    return;
                }
                catch (IOException)
                {
                    // Still locked (e.g. download in progress) — wait and retry.
                    await Task.Delay(1000);
                }
                catch (UnauthorizedAccessException)
                {
                    return;
                }
            }
        }

        // Appends a Unix timestamp to the filename to make it unique,
        // e.g. "report.pdf" -> "report_1751790000.pdf".
        public static string GetConflictFreePath(string path)
        {
            string folder = Path.GetDirectoryName(path)!;
            string name = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            return Path.Combine(folder, $"{name}_{timestamp}{extension}");
        }
    }
}
