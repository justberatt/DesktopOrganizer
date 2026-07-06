namespace DesktopOrganizer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            listPreview = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            btnOrganize = new Button();
            btnUndo = new Button();
            lblStatus = new Label();
            chkAutoSort = new CheckBox();
            SuspendLayout();
            // 
            // listPreview
            // 
            listPreview.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            listPreview.Location = new Point(156, 83);
            listPreview.Name = "listPreview";
            listPreview.Size = new Size(525, 240);
            listPreview.TabIndex = 0;
            listPreview.UseCompatibleStateImageBehavior = false;
            listPreview.SelectedIndexChanged += listView1_SelectedIndexChanged;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "File";
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Will move to";
            // 
            // btnOrganize
            // 
            btnOrganize.Location = new Point(156, 341);
            btnOrganize.Name = "btnOrganize";
            btnOrganize.Size = new Size(180, 39);
            btnOrganize.TabIndex = 1;
            btnOrganize.Text = "Organize";
            btnOrganize.UseVisualStyleBackColor = true;
            // 
            // btnUndo
            // 
            btnUndo.Location = new Point(353, 342);
            btnUndo.Name = "btnUndo";
            btnUndo.Size = new Size(86, 37);
            btnUndo.TabIndex = 2;
            btnUndo.Text = "Undo";
            btnUndo.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(156, 393);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(50, 20);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "Ready";
            // 
            // chkAutoSort
            // 
            chkAutoSort.AutoSize = true;
            chkAutoSort.Location = new Point(525, 349);
            chkAutoSort.Name = "chkAutoSort";
            chkAutoSort.Size = new Size(156, 24);
            chkAutoSort.TabIndex = 4;
            chkAutoSort.Text = "Auto-sort new files";
            chkAutoSort.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(954, 504);
            Controls.Add(chkAutoSort);
            Controls.Add(lblStatus);
            Controls.Add(btnUndo);
            Controls.Add(btnOrganize);
            Controls.Add(listPreview);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load_1;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListView listPreview;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private Button btnOrganize;
        private Button btnUndo;
        private Label lblStatus;
        private CheckBox chkAutoSort;
    }
}
