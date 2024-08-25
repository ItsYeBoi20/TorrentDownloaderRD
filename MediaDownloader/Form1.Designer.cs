namespace MediaDownloader
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.textBox_Search = new System.Windows.Forms.TextBox();
            this.label_Search = new System.Windows.Forms.Label();
            this.button_Search = new System.Windows.Forms.Button();
            this.comboBox_Content = new System.Windows.Forms.ComboBox();
            this.label_Content = new System.Windows.Forms.Label();
            this.label_SortBy = new System.Windows.Forms.Label();
            this.comboBox_SortBy = new System.Windows.Forms.ComboBox();
            this.numericUpDown_Seeders = new System.Windows.Forms.NumericUpDown();
            this.label_Seeders = new System.Windows.Forms.Label();
            this.dataGridView_Torrents = new System.Windows.Forms.DataGridView();
            this.TorrentName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TorrentSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Seeders = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Leechers = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SearchEngine = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textBox_Filter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_Settings = new System.Windows.Forms.Button();
            this.backgroundWorker_Search = new System.ComponentModel.BackgroundWorker();
            this.timer_Title = new System.Windows.Forms.Timer(this.components);
            this.label_Saved = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Seeders)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Torrents)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox_Search
            // 
            this.textBox_Search.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Search.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_Search.Location = new System.Drawing.Point(12, 29);
            this.textBox_Search.Name = "textBox_Search";
            this.textBox_Search.Size = new System.Drawing.Size(388, 23);
            this.textBox_Search.TabIndex = 0;
            this.textBox_Search.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_Search_KeyDown);
            // 
            // label_Search
            // 
            this.label_Search.AutoSize = true;
            this.label_Search.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Search.Location = new System.Drawing.Point(9, 11);
            this.label_Search.Name = "label_Search";
            this.label_Search.Size = new System.Drawing.Size(45, 15);
            this.label_Search.TabIndex = 3;
            this.label_Search.Text = "Search:";
            // 
            // button_Search
            // 
            this.button_Search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Search.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Search.Location = new System.Drawing.Point(910, 28);
            this.button_Search.Name = "button_Search";
            this.button_Search.Size = new System.Drawing.Size(100, 25);
            this.button_Search.TabIndex = 4;
            this.button_Search.Text = "Search";
            this.button_Search.UseVisualStyleBackColor = true;
            this.button_Search.Click += new System.EventHandler(this.button_Search_Click);
            // 
            // comboBox_Content
            // 
            this.comboBox_Content.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_Content.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Content.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox_Content.FormattingEnabled = true;
            this.comboBox_Content.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.comboBox_Content.Items.AddRange(new object[] {
            "All",
            "Movies",
            "TV",
            "Anime",
            "Games",
            "XXX"});
            this.comboBox_Content.Location = new System.Drawing.Point(593, 29);
            this.comboBox_Content.Name = "comboBox_Content";
            this.comboBox_Content.Size = new System.Drawing.Size(112, 23);
            this.comboBox_Content.TabIndex = 5;
            // 
            // label_Content
            // 
            this.label_Content.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label_Content.AutoSize = true;
            this.label_Content.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Content.Location = new System.Drawing.Point(590, 11);
            this.label_Content.Name = "label_Content";
            this.label_Content.Size = new System.Drawing.Size(53, 15);
            this.label_Content.TabIndex = 6;
            this.label_Content.Text = "Content:";
            // 
            // label_SortBy
            // 
            this.label_SortBy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label_SortBy.AutoSize = true;
            this.label_SortBy.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_SortBy.Location = new System.Drawing.Point(708, 11);
            this.label_SortBy.Name = "label_SortBy";
            this.label_SortBy.Size = new System.Drawing.Size(47, 15);
            this.label_SortBy.TabIndex = 8;
            this.label_SortBy.Text = "Sort By:";
            // 
            // comboBox_SortBy
            // 
            this.comboBox_SortBy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_SortBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_SortBy.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox_SortBy.FormattingEnabled = true;
            this.comboBox_SortBy.Items.AddRange(new object[] {
            "None",
            "Size Descending",
            "Size Ascending",
            "Time Descending",
            "Time Ascending",
            "Seeders Descending",
            "Seeders Ascending"});
            this.comboBox_SortBy.Location = new System.Drawing.Point(711, 29);
            this.comboBox_SortBy.Name = "comboBox_SortBy";
            this.comboBox_SortBy.Size = new System.Drawing.Size(112, 23);
            this.comboBox_SortBy.TabIndex = 7;
            // 
            // numericUpDown_Seeders
            // 
            this.numericUpDown_Seeders.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDown_Seeders.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numericUpDown_Seeders.Location = new System.Drawing.Point(829, 29);
            this.numericUpDown_Seeders.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDown_Seeders.Name = "numericUpDown_Seeders";
            this.numericUpDown_Seeders.Size = new System.Drawing.Size(75, 23);
            this.numericUpDown_Seeders.TabIndex = 9;
            this.numericUpDown_Seeders.ValueChanged += new System.EventHandler(this.numericUpDown_Seeders_ValueChanged);
            // 
            // label_Seeders
            // 
            this.label_Seeders.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label_Seeders.AutoSize = true;
            this.label_Seeders.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Seeders.Location = new System.Drawing.Point(826, 11);
            this.label_Seeders.Name = "label_Seeders";
            this.label_Seeders.Size = new System.Drawing.Size(74, 15);
            this.label_Seeders.TabIndex = 10;
            this.label_Seeders.Text = "Min Seeders:";
            // 
            // dataGridView_Torrents
            // 
            this.dataGridView_Torrents.AllowUserToAddRows = false;
            this.dataGridView_Torrents.AllowUserToDeleteRows = false;
            this.dataGridView_Torrents.AllowUserToResizeRows = false;
            this.dataGridView_Torrents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView_Torrents.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridView_Torrents.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView_Torrents.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TorrentName,
            this.TorrentSize,
            this.Seeders,
            this.Leechers,
            this.SearchEngine});
            this.dataGridView_Torrents.Location = new System.Drawing.Point(12, 58);
            this.dataGridView_Torrents.Name = "dataGridView_Torrents";
            this.dataGridView_Torrents.ReadOnly = true;
            this.dataGridView_Torrents.RowHeadersVisible = false;
            this.dataGridView_Torrents.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView_Torrents.Size = new System.Drawing.Size(1028, 395);
            this.dataGridView_Torrents.TabIndex = 11;
            this.dataGridView_Torrents.Visible = false;
            this.dataGridView_Torrents.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_Torrents_CellDoubleClick);
            this.dataGridView_Torrents.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView_Torrents_ColumnHeaderMouseClick);
            // 
            // TorrentName
            // 
            this.TorrentName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.TorrentName.HeaderText = "Name";
            this.TorrentName.MinimumWidth = 600;
            this.TorrentName.Name = "TorrentName";
            this.TorrentName.ReadOnly = true;
            // 
            // TorrentSize
            // 
            this.TorrentSize.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.TorrentSize.HeaderText = "Size";
            this.TorrentSize.MinimumWidth = 60;
            this.TorrentSize.Name = "TorrentSize";
            this.TorrentSize.ReadOnly = true;
            // 
            // Seeders
            // 
            this.Seeders.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Seeders.HeaderText = "Seeders";
            this.Seeders.MinimumWidth = 60;
            this.Seeders.Name = "Seeders";
            this.Seeders.ReadOnly = true;
            // 
            // Leechers
            // 
            this.Leechers.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Leechers.HeaderText = "Leechers";
            this.Leechers.MinimumWidth = 60;
            this.Leechers.Name = "Leechers";
            this.Leechers.ReadOnly = true;
            // 
            // SearchEngine
            // 
            this.SearchEngine.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.SearchEngine.HeaderText = "Search Engine";
            this.SearchEngine.MinimumWidth = 60;
            this.SearchEngine.Name = "SearchEngine";
            this.SearchEngine.ReadOnly = true;
            // 
            // textBox_Filter
            // 
            this.textBox_Filter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Filter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_Filter.Location = new System.Drawing.Point(406, 29);
            this.textBox_Filter.Name = "textBox_Filter";
            this.textBox_Filter.Size = new System.Drawing.Size(181, 23);
            this.textBox_Filter.TabIndex = 12;
            this.textBox_Filter.TextChanged += new System.EventHandler(this.textBox_Filter_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(406, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 15);
            this.label1.TabIndex = 13;
            this.label1.Text = "Filter:";
            // 
            // button_Settings
            // 
            this.button_Settings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Settings.Location = new System.Drawing.Point(1016, 28);
            this.button_Settings.Name = "button_Settings";
            this.button_Settings.Size = new System.Drawing.Size(25, 25);
            this.button_Settings.TabIndex = 14;
            this.button_Settings.Text = "i";
            this.button_Settings.UseVisualStyleBackColor = true;
            this.button_Settings.Click += new System.EventHandler(this.button_Settings_Click);
            // 
            // backgroundWorker_Search
            // 
            this.backgroundWorker_Search.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_Search_DoWork);
            this.backgroundWorker_Search.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_Search_RunWorkerCompleted);
            // 
            // timer_Title
            // 
            this.timer_Title.Interval = 10;
            this.timer_Title.Tick += new System.EventHandler(this.timer_Title_Tick);
            // 
            // label_Saved
            // 
            this.label_Saved.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label_Saved.AutoSize = true;
            this.label_Saved.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label_Saved.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Saved.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label_Saved.Location = new System.Drawing.Point(929, 11);
            this.label_Saved.Name = "label_Saved";
            this.label_Saved.Size = new System.Drawing.Size(111, 15);
            this.label_Saved.TabIndex = 15;
            this.label_Saved.Text = "View Saved Torrents";
            this.label_Saved.Click += new System.EventHandler(this.label_Saved_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(1052, 465);
            this.Controls.Add(this.label_Saved);
            this.Controls.Add(this.button_Settings);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_Filter);
            this.Controls.Add(this.dataGridView_Torrents);
            this.Controls.Add(this.label_Seeders);
            this.Controls.Add(this.numericUpDown_Seeders);
            this.Controls.Add(this.label_SortBy);
            this.Controls.Add(this.comboBox_SortBy);
            this.Controls.Add(this.label_Content);
            this.Controls.Add(this.comboBox_Content);
            this.Controls.Add(this.button_Search);
            this.Controls.Add(this.label_Search);
            this.Controls.Add(this.textBox_Search);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(900, 200);
            this.Name = "Main";
            this.Text = "Torrent Searcher";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Seeders)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Torrents)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_Search;
        private System.Windows.Forms.Label label_Search;
        private System.Windows.Forms.Button button_Search;
        private System.Windows.Forms.ComboBox comboBox_Content;
        private System.Windows.Forms.Label label_Content;
        private System.Windows.Forms.Label label_SortBy;
        private System.Windows.Forms.ComboBox comboBox_SortBy;
        private System.Windows.Forms.NumericUpDown numericUpDown_Seeders;
        private System.Windows.Forms.Label label_Seeders;
        private System.Windows.Forms.DataGridView dataGridView_Torrents;
        private System.Windows.Forms.TextBox textBox_Filter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_Settings;
        private System.ComponentModel.BackgroundWorker backgroundWorker_Search;
        private System.Windows.Forms.Timer timer_Title;
        private System.Windows.Forms.DataGridViewTextBoxColumn TorrentName;
        private System.Windows.Forms.DataGridViewTextBoxColumn TorrentSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn Seeders;
        private System.Windows.Forms.DataGridViewTextBoxColumn Leechers;
        private System.Windows.Forms.DataGridViewTextBoxColumn SearchEngine;
        private System.Windows.Forms.Label label_Saved;
    }
}

