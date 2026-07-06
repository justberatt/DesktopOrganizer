namespace DesktopOrganizer
{
    public partial class SettingsForm : Form
    {
        // Palette from the design spec (same values as Form1)
        private static readonly Color Background = Color.FromArgb(30, 30, 30);
        private static readonly Color Surface = Color.FromArgb(45, 45, 45);
        private static readonly Color SurfaceHover = Color.FromArgb(56, 56, 56);
        private static readonly Color Accent = Color.FromArgb(255, 107, 53);
        private static readonly Color TextPrimary = Color.FromArgb(224, 224, 224);
        private static readonly Color TextSecondary = Color.FromArgb(153, 153, 153);
        private static readonly Color Border = Color.FromArgb(61, 61, 61);

        public SettingsForm()
        {
            InitializeComponent();
            ApplyTheme();

            btnSave.Click += BtnSave_Click;
            btnReset.Click += BtnReset_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Load += (s, e) => PopulateGrid();
        }

        private void PopulateGrid()
        {
            gridCategories.Rows.Clear();

            foreach (var (category, extensions) in Categories.Map)
                gridCategories.Rows.Add(category, string.Join(", ", extensions));
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var map = ParseGrid(out string? error);

            if (map == null)
            {
                MessageBox.Show(error, "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Categories.Save(map);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            Categories.ResetToDefaults();
            PopulateGrid();
        }

        // Reads the grid back into a dictionary. Returns null (with an error
        // message) if the input is invalid.
        private Dictionary<string, string[]>? ParseGrid(out string? error)
        {
            var map = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            char[] invalidChars = Path.GetInvalidFileNameChars();

            foreach (DataGridViewRow row in gridCategories.Rows)
            {
                if (row.IsNewRow)
                    continue;

                string category = (row.Cells[0].Value?.ToString() ?? "").Trim();
                string extensionsText = (row.Cells[1].Value?.ToString() ?? "").Trim();

                // Fully empty rows are simply ignored.
                if (category.Length == 0 && extensionsText.Length == 0)
                    continue;

                if (category.Length == 0)
                {
                    error = "Every row needs a category name.";
                    return null;
                }

                if (category.IndexOfAny(invalidChars) >= 0)
                {
                    error = $"\"{category}\" contains characters that aren't allowed in folder names.";
                    return null;
                }

                if (map.ContainsKey(category))
                {
                    error = $"Duplicate category \"{category}\" — each category can only appear once.";
                    return null;
                }

                // Accept ".pdf", "pdf", "*.pdf" separated by commas, semicolons or spaces.
                string[] extensions = extensionsText
                    .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(ext => "." + ext.TrimStart('.', '*').ToLowerInvariant())
                    .Where(ext => ext.Length > 1)
                    .Distinct()
                    .ToArray();

                if (extensions.Length == 0)
                {
                    error = $"Category \"{category}\" has no extensions.";
                    return null;
                }

                map[category] = extensions;
            }

            if (map.Count == 0)
            {
                error = "Add at least one category.";
                return null;
            }

            error = null;
            return map;
        }

        // ----- Theming & layout (applied in code so the designer stays simple) -----

        private void ApplyTheme()
        {
            Text = "Settings";
            BackColor = Background;
            Font = new Font("Segoe UI", 9.75F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(520, 480);

            // Grid
            gridCategories.SetBounds(20, 20, 480, 384);
            gridCategories.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridCategories.BackgroundColor = Surface;
            gridCategories.BorderStyle = BorderStyle.FixedSingle;
            gridCategories.GridColor = Border;
            gridCategories.RowHeadersVisible = false;
            gridCategories.AllowUserToResizeRows = false;
            gridCategories.RowTemplate.Height = 32;
            gridCategories.ColumnHeadersHeight = 36;
            gridCategories.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            gridCategories.EnableHeadersVisualStyles = false;
            gridCategories.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            gridCategories.ColumnHeadersDefaultCellStyle.BackColor = Surface;
            gridCategories.ColumnHeadersDefaultCellStyle.ForeColor = TextSecondary;
            gridCategories.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            gridCategories.ColumnHeadersDefaultCellStyle.SelectionBackColor = Surface;

            gridCategories.DefaultCellStyle.BackColor = Surface;
            gridCategories.DefaultCellStyle.ForeColor = TextPrimary;
            gridCategories.DefaultCellStyle.SelectionBackColor = Color.FromArgb(45, 61, 69); // #2D3D45
            gridCategories.DefaultCellStyle.SelectionForeColor = TextPrimary;
            gridCategories.RowsDefaultCellStyle.BackColor = Surface;

            if (gridCategories.Columns.Count == 0)
            {
                gridCategories.Columns.Add("colCategory", "Category");
                gridCategories.Columns.Add("colExtensions", "Extensions");
            }
            gridCategories.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridCategories.Columns[0].FillWeight = 32;
            gridCategories.Columns[1].FillWeight = 68;

            // Buttons
            btnSave.Text = "Save";
            btnSave.SetBounds(20, 424, 120, 36);
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            StyleButton(btnSave, primary: true);

            btnReset.Text = "Reset to defaults";
            btnReset.SetBounds(152, 424, 150, 36);
            btnReset.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            StyleButton(btnReset, primary: false);

            btnCancel.Text = "Cancel";
            btnCancel.SetBounds(400, 424, 100, 36);
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            StyleButton(btnCancel, primary: false);
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
    }
}
