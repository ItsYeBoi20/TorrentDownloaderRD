namespace MediaDownloader
{
    partial class Settings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            this.textBox_Key = new System.Windows.Forms.TextBox();
            this.button_Save = new System.Windows.Forms.Button();
            this.label_Key = new System.Windows.Forms.Label();
            this.button_Edit = new System.Windows.Forms.Button();
            this.numericUpDown_Pages = new System.Windows.Forms.NumericUpDown();
            this.label_Pages = new System.Windows.Forms.Label();
            this.label_Providers = new System.Windows.Forms.Label();
            this.checkBox_Remove = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.checkedListBox_Providers = new CustomCheckedListBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Pages)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox_Key
            // 
            this.textBox_Key.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Key.Enabled = false;
            this.textBox_Key.Location = new System.Drawing.Point(12, 27);
            this.textBox_Key.Name = "textBox_Key";
            this.textBox_Key.Size = new System.Drawing.Size(512, 23);
            this.textBox_Key.TabIndex = 1;
            // 
            // button_Save
            // 
            this.button_Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Save.Location = new System.Drawing.Point(451, 227);
            this.button_Save.Name = "button_Save";
            this.button_Save.Size = new System.Drawing.Size(73, 44);
            this.button_Save.TabIndex = 4;
            this.button_Save.Text = "Save";
            this.button_Save.UseVisualStyleBackColor = true;
            this.button_Save.Click += new System.EventHandler(this.button_Save_Click);
            // 
            // label_Key
            // 
            this.label_Key.AutoSize = true;
            this.label_Key.Location = new System.Drawing.Point(12, 9);
            this.label_Key.Name = "label_Key";
            this.label_Key.Size = new System.Drawing.Size(115, 15);
            this.label_Key.TabIndex = 0;
            this.label_Key.Text = "Real-Debrid API Key:";
            // 
            // button_Edit
            // 
            this.button_Edit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Edit.Location = new System.Drawing.Point(372, 227);
            this.button_Edit.Name = "button_Edit";
            this.button_Edit.Size = new System.Drawing.Size(73, 44);
            this.button_Edit.TabIndex = 3;
            this.button_Edit.Text = "Edit";
            this.button_Edit.UseVisualStyleBackColor = true;
            this.button_Edit.Click += new System.EventHandler(this.button_Edit_Click);
            // 
            // numericUpDown_Pages
            // 
            this.numericUpDown_Pages.Enabled = false;
            this.numericUpDown_Pages.Location = new System.Drawing.Point(12, 84);
            this.numericUpDown_Pages.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDown_Pages.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_Pages.Name = "numericUpDown_Pages";
            this.numericUpDown_Pages.Size = new System.Drawing.Size(151, 23);
            this.numericUpDown_Pages.TabIndex = 2;
            this.numericUpDown_Pages.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label_Pages
            // 
            this.label_Pages.AutoSize = true;
            this.label_Pages.Location = new System.Drawing.Point(12, 66);
            this.label_Pages.Name = "label_Pages";
            this.label_Pages.Size = new System.Drawing.Size(154, 15);
            this.label_Pages.TabIndex = 5;
            this.label_Pages.Text = "Amount of Pages to Search:";
            // 
            // label_Providers
            // 
            this.label_Providers.AutoSize = true;
            this.label_Providers.Location = new System.Drawing.Point(12, 122);
            this.label_Providers.Name = "label_Providers";
            this.label_Providers.Size = new System.Drawing.Size(95, 15);
            this.label_Providers.TabIndex = 7;
            this.label_Providers.Text = "Providers to Use:";
            // 
            // checkBox_Remove
            // 
            this.checkBox_Remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox_Remove.AutoSize = true;
            this.checkBox_Remove.Enabled = false;
            this.checkBox_Remove.Location = new System.Drawing.Point(352, 77);
            this.checkBox_Remove.Name = "checkBox_Remove";
            this.checkBox_Remove.Size = new System.Drawing.Size(172, 34);
            this.checkBox_Remove.TabIndex = 8;
            this.checkBox_Remove.Text = "Remove Torrent From\r\nReal-Debrid after Download";
            this.checkBox_Remove.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(449, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 15);
            this.label1.TabIndex = 11;
            this.label1.Text = "Check Status";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(372, 209);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 15);
            this.label2.TabIndex = 12;
            this.label2.Text = "Current Version: ";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // checkedListBox_Providers
            // 
            this.checkedListBox_Providers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBox_Providers.CheckOnClick = true;
            this.checkedListBox_Providers.Enabled = false;
            this.checkedListBox_Providers.FormattingEnabled = true;
            this.checkedListBox_Providers.Items.AddRange(new object[] {
            "1337x",
            "LimeTorrents",
            "Nyaa",
            "Piratebay",
            "Torlock2",
            "TorrentProject",
            "Torrents-CSV",
            "TorrentDownload",
            "YourBittorrent",
            "TorrentGalaxy",
            "BitSearch",
            "TheRarbg",
            "FitGirl"});
            this.checkedListBox_Providers.Location = new System.Drawing.Point(12, 140);
            this.checkedListBox_Providers.Name = "checkedListBox_Providers";
            this.checkedListBox_Providers.Size = new System.Drawing.Size(354, 130);
            this.checkedListBox_Providers.TabIndex = 9;
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(536, 282);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkedListBox_Providers);
            this.Controls.Add(this.checkBox_Remove);
            this.Controls.Add(this.label_Providers);
            this.Controls.Add(this.label_Pages);
            this.Controls.Add(this.numericUpDown_Pages);
            this.Controls.Add(this.button_Edit);
            this.Controls.Add(this.label_Key);
            this.Controls.Add(this.button_Save);
            this.Controls.Add(this.textBox_Key);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinimumSize = new System.Drawing.Size(552, 321);
            this.Name = "Settings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.Settings_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Pages)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_Key;
        private System.Windows.Forms.Button button_Save;
        private System.Windows.Forms.Label label_Key;
        private System.Windows.Forms.Button button_Edit;
        private System.Windows.Forms.NumericUpDown numericUpDown_Pages;
        private System.Windows.Forms.Label label_Pages;
        private System.Windows.Forms.Label label_Providers;
        private System.Windows.Forms.CheckBox checkBox_Remove;
        private CustomCheckedListBox checkedListBox_Providers;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}