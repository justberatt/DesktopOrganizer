namespace DesktopOrganizer
{
    public partial class Form1 : Form
    {
        // Palette from the design spec
        private static readonly Color Background = Color.FromArgb(30, 30, 30);      // #1E1E1E
        private static readonly Color Surface = Color.FromArgb(45, 45, 45);         // #2D2D2D
        private static readonly Color SurfaceHover = Color.FromArgb(56, 56, 56);    // #383838
        private static readonly Color Accent = Color.FromArgb(255, 107, 53);        // #FF6B35
        private static readonly Color TextPrimary = Color.FromArgb(224, 224, 224);  // #E0E0E0
        private static readonly Color TextSecondary = Color.FromArgb(153, 153, 153); // #999999
        private static readonly Color TextDisabled = Color.FromArgb(102, 102, 102); // #666666
        private static readonly Color Border = Color.FromArgb(61, 61, 61);          // #3D3D3D

        private enum StatusKind { Info, Success, Warning, Error }

        public Form1()
        {
            InitializeComponent();
            ApplyTheme();

            btnOrganize.Click += BtnOrganize_Click;
            btnUndo.Click += BtnUndo_Click;
            chkAutoSort.CheckedChanged += ChkAutoSort_CheckedChanged;
            Organizer.FileAutoSorted += OnFileAutoSorted;
            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
            FormClosed += (s, e) =>
            {
                Organizer.FileAutoSorted -= OnFileAutoSorted;
                Organizer.StopWatching();
                trayIcon.Dispose();
                trayMenu.Dispose();
            };

            SetupTray();
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            // Restore the saved toggle state; CheckedChanged starts the watcher if needed.
            chkAutoSort.Checked = AppSettings.Current.AutoSortEnabled;
            LoadPreview();
        }

        // ----- Auto-sort -----

        private void ChkAutoSort_CheckedChanged(object? sender, EventArgs e)
        {
            if (chkAutoSort.Checked)
                Organizer.StartWatching();
            else
                Organizer.StopWatching();

            AppSettings.Current.AutoSortEnabled = chkAutoSort.Checked;
            AppSettings.Current.Save();
        }

        // Fired on a background thread by the FileSystemWatcher — hop to the UI thread.
        private void OnFileAutoSorted(string fileName)
        {
            if (IsDisposed) return;

            BeginInvoke(() =>
            {
                SetStatus($"Auto-sorted \"{fileName}\".", StatusKind.Success);
                RefreshList();
                UpdateUndoButton();
            });
        }

        // ----- Preview (dry run) -----

        private void LoadPreview()
        {
            var records = Organizer.Organize(dryRun: true);

            listPreview.BeginUpdate();
            listPreview.Items.Clear();

            foreach (var record in records)
            {
                var item = new ListViewItem(Path.GetFileName(record.OriginalPath));
                item.SubItems.Add(Path.GetFileName(Path.GetDirectoryName(record.NewPath)!));
                item.SubItems[1].ForeColor = Color.FromArgb(153, 153, 153); // #999999 secondary text
                item.UseItemStyleForSubItems = false;
                listPreview.Items.Add(item);
            }

            listPreview.EndUpdate();

            SetStatus(records.Count == 0
                ? "Desktop is already clean — nothing to organize."
                : $"{records.Count} file(s) ready to organize.", StatusKind.Info);

            UpdateUndoButton();
        }

        // ----- Button handlers -----

        private void BtnOrganize_Click(object? sender, EventArgs e)
        {
            var moved = Organizer.Organize(dryRun: false);

            if (moved.Count > 0)
                UndoLog.Save(moved);

            if (Organizer.SkippedFiles.Count > 0)
                SetStatus($"{moved.Count} file(s) organized, {Organizer.SkippedFiles.Count} skipped (locked or in use).", StatusKind.Warning);
            else if (moved.Count > 0)
                SetStatus($"{moved.Count} file(s) organized.", StatusKind.Success);
            else
                SetStatus("Nothing to organize.", StatusKind.Info);

            RefreshList();
            UpdateUndoButton();
        }

        private void BtnUndo_Click(object? sender, EventArgs e)
        {
            int restored = UndoLog.Undo();
            SetStatus($"{restored} file(s) restored to the desktop.", StatusKind.Success);
            LoadPreview();
        }

        // Refreshes the ListView without overwriting the status message.
        private void RefreshList()
        {
            var records = Organizer.Organize(dryRun: true);

            listPreview.BeginUpdate();
            listPreview.Items.Clear();

            foreach (var record in records)
            {
                var item = new ListViewItem(Path.GetFileName(record.OriginalPath));
                item.SubItems.Add(Path.GetFileName(Path.GetDirectoryName(record.NewPath)!));
                item.SubItems[1].ForeColor = Color.FromArgb(153, 153, 153);
                item.UseItemStyleForSubItems = false;
                listPreview.Items.Add(item);
            }

            listPreview.EndUpdate();
        }

        private void UpdateUndoButton()
        {
            btnUndo.Enabled = UndoLog.Exists;
            btnUndo.ForeColor = btnUndo.Enabled ? TextPrimary : TextDisabled;
        }

        // ----- System tray -----

        private NotifyIcon trayIcon = null!;
        private ContextMenuStrip trayMenu = null!;
        private ToolStripMenuItem trayUndoItem = null!;
        private bool exitRequested;

        private void SetupTray()
        {
            var appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
            Icon = appIcon;

            var organizeItem = new ToolStripMenuItem("Organize Now", null, (s, e) => OrganizeFromTray());
            trayUndoItem = new ToolStripMenuItem("Undo", null, (s, e) => UndoFromTray());
            var settingsItem = new ToolStripMenuItem("Open Settings", null, (s, e) => OpenSettings());
            var exitItem = new ToolStripMenuItem("Exit", null, (s, e) => ExitApp());

            trayMenu = new ContextMenuStrip
            {
                Renderer = new DarkMenuRenderer(),
                ShowImageMargin = false,
            };
            trayMenu.Items.AddRange(new ToolStripItem[]
            {
                organizeItem,
                trayUndoItem,
                settingsItem,
                new ToolStripSeparator(),
                exitItem,
            });
            foreach (ToolStripItem item in trayMenu.Items)
                item.ForeColor = TextPrimary;
            trayMenu.Opening += (s, e) => trayUndoItem.Enabled = UndoLog.Exists;

            trayIcon = new NotifyIcon
            {
                Text = "Desktop Organizer",
                Icon = appIcon,
                ContextMenuStrip = trayMenu,
                Visible = true,
            };
            trayIcon.DoubleClick += (s, e) => RestoreFromTray();
        }

        // Closing the window hides it to the tray instead of exiting.
        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!exitRequested && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                trayIcon.ShowBalloonTip(2000, "Desktop Organizer",
                    "Still running in the tray. Double-click the icon to reopen.", ToolTipIcon.Info);
            }
        }

        private void RestoreFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
            LoadPreview(); // desktop may have changed while hidden
        }

        private void OrganizeFromTray()
        {
            BtnOrganize_Click(null, EventArgs.Empty);
            NotifyIfHidden();
        }

        private void UndoFromTray()
        {
            BtnUndo_Click(null, EventArgs.Empty);
            NotifyIfHidden();
        }

        // When the window is hidden, surface the result as a balloon tip instead.
        private void NotifyIfHidden()
        {
            if (!Visible)
                trayIcon.ShowBalloonTip(2000, "Desktop Organizer", lblStatus.Text, ToolTipIcon.Info);
        }

        private void OpenSettings()
        {
            using var dialog = new SettingsForm();

            if (dialog.ShowDialog(Visible ? this : null) == DialogResult.OK)
            {
                SetStatus("Categories saved.", StatusKind.Success);
                RefreshList();
            }
        }

        private void ExitApp()
        {
            exitRequested = true;
            trayIcon.Visible = false;
            Close();
        }

        // Dark theme for the tray context menu (default WinForms menus are light).
        private class DarkMenuRenderer : ToolStripProfessionalRenderer
        {
            public DarkMenuRenderer() : base(new DarkColorTable()) { }

            private class DarkColorTable : ProfessionalColorTable
            {
                public override Color ToolStripDropDownBackground => Surface;
                public override Color MenuItemSelected => SurfaceHover;
                public override Color MenuItemSelectedGradientBegin => SurfaceHover;
                public override Color MenuItemSelectedGradientEnd => SurfaceHover;
                public override Color MenuItemBorder => Border;
                public override Color MenuBorder => Border;
                public override Color SeparatorDark => Border;
                public override Color SeparatorLight => Border;
                public override Color ImageMarginGradientBegin => Surface;
                public override Color ImageMarginGradientMiddle => Surface;
                public override Color ImageMarginGradientEnd => Surface;
            }
        }

        // ----- Status label -----

        private void SetStatus(string message, StatusKind kind)
        {
            lblStatus.Text = message;

            (lblStatus.BackColor, lblStatus.ForeColor) = kind switch
            {
                StatusKind.Success => (Color.FromArgb(30, 70, 32), Color.FromArgb(76, 175, 80)),   // #1E4620 / #4CAF50
                StatusKind.Warning => (Color.FromArgb(74, 53, 0), Color.FromArgb(255, 165, 0)),    // #4A3500 / #FFA500
                StatusKind.Error => (Color.FromArgb(74, 31, 31), Color.FromArgb(255, 82, 82)),     // #4A1F1F / #FF5252
                _ => (Color.FromArgb(26, 58, 82), Color.FromArgb(100, 181, 246)),                  // #1A3A52 / #64B5F6
            };
        }

        // ----- Theming (applied in code so the designer stays simple) -----

        private Label lblTitle = null!;
        private Label lblSubtitle = null!;

        private void ApplyTheme()
        {
            Text = "Desktop Organizer";
            BackColor = Background;
            Font = new Font("Segoe UI", 9.75F);
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(520, 560);
            MinimumSize = new Size(416, 500);

            // Header (created in code — layout is owned by ApplyTheme)
            lblTitle = new Label
            {
                Text = "Desktop Organizer",
                Font = new Font("Segoe UI Semibold", 13.5F),
                ForeColor = TextPrimary,
                AutoSize = true,
                Location = new Point(18, 20),
            };
            Controls.Add(lblTitle);

            lblSubtitle = new Label
            {
                Text = Organizer.GetDesktopPath(),
                Font = new Font("Segoe UI", 8.25F),
                ForeColor = TextSecondary,
                AutoSize = true,
                Location = new Point(20, 52),
            };
            Controls.Add(lblSubtitle);

            // ListView
            listPreview.SetBounds(20, 84, 480, 352);
            listPreview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listPreview.BackColor = Surface;
            listPreview.ForeColor = TextPrimary;
            listPreview.BorderStyle = BorderStyle.FixedSingle;
            listPreview.View = View.Details;
            listPreview.FullRowSelect = true;
            listPreview.MultiSelect = false;
            listPreview.HeaderStyle = ColumnHeaderStyle.Nonclickable;

            if (listPreview.Columns.Count == 0)
            {
                listPreview.Columns.Add("File", 280);
                listPreview.Columns.Add("Will move to", 178);
            }
            else
            {
                listPreview.Columns[0].Text = "File";
                listPreview.Columns[0].Width = 280;
                listPreview.Columns[1].Text = "Will move to";
                listPreview.Columns[1].Width = 178;
            }

            // Owner-draw the column header so it matches the dark theme
            // (the default header is always light and ignores BackColor).
            listPreview.OwnerDraw = true;
            listPreview.DrawColumnHeader += (s, e) =>
            {
                using var back = new SolidBrush(Surface);
                e.Graphics.FillRectangle(back, e.Bounds);
                using var line = new Pen(Border);
                e.Graphics.DrawLine(line, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                TextRenderer.DrawText(e.Graphics, e.Header?.Text, new Font("Segoe UI", 8.25F, FontStyle.Bold),
                    e.Bounds, Color.FromArgb(153, 153, 153),
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.LeftAndRightPadding);
            };
            listPreview.DrawItem += (s, e) => e.DrawDefault = true;
            listPreview.DrawSubItem += (s, e) => e.DrawDefault = true;

            // Action row: Organize + Undo on the left, auto-sort toggle on the right
            btnOrganize.Text = "Organize";
            btnOrganize.SetBounds(20, 456, 170, 36);
            btnOrganize.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            StyleButton(btnOrganize, primary: true);

            btnUndo.Text = "Undo";
            btnUndo.SetBounds(202, 456, 110, 36);
            btnUndo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            StyleButton(btnUndo, primary: false);

            chkAutoSort.Text = "Auto-sort new files";
            chkAutoSort.AutoSize = true;
            chkAutoSort.ForeColor = TextPrimary;
            chkAutoSort.BackColor = Background;
            chkAutoSort.Cursor = Cursors.Hand;
            chkAutoSort.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Size checkSize = chkAutoSort.PreferredSize;
            chkAutoSort.Location = new Point(
                ClientSize.Width - 20 - checkSize.Width,
                456 + (36 - checkSize.Height) / 2);

            // Status bar pinned to the bottom
            lblStatus.AutoSize = false;
            lblStatus.SetBounds(20, 508, 480, 32);
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.Padding = new Padding(12, 0, 12, 0);
        }

        private static void StyleButton(Button button, bool primary)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;

            if (primary)
            {
                button.BackColor = Accent;
                button.ForeColor = Color.White;
                button.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 126, 78);
                button.FlatAppearance.MouseDownBackColor = Color.FromArgb(224, 87, 38);
            }
            else
            {
                button.BackColor = Surface;
                button.ForeColor = TextPrimary;
                button.FlatAppearance.BorderSize = 1;
                button.FlatAppearance.BorderColor = Border;
                button.FlatAppearance.MouseOverBackColor = SurfaceHover;
                button.FlatAppearance.MouseDownBackColor = Color.FromArgb(35, 35, 35);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
    }
}
