namespace MediaDownloader
{
    partial class SavedTorrents
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
            this.progress_Label = new System.Windows.Forms.Label();
            this.button_Download = new System.Windows.Forms.Button();
            this.speed_Label = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lstDownloadLinks = new CustomListBox();
            this.customListBox1 = new CustomListBox();
            this.SuspendLayout();
            // 
            // progress_Label
            // 
            this.progress_Label.AutoSize = true;
            this.progress_Label.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.progress_Label.Location = new System.Drawing.Point(11, 543);
            this.progress_Label.Name = "progress_Label";
            this.progress_Label.Size = new System.Drawing.Size(89, 15);
            this.progress_Label.TabIndex = 30;
            this.progress_Label.Text = "Retrieved Links:";
            // 
            // button_Download
            // 
            this.button_Download.BackColor = System.Drawing.Color.Transparent;
            this.button_Download.Enabled = false;
            this.button_Download.Location = new System.Drawing.Point(579, 546);
            this.button_Download.Name = "button_Download";
            this.button_Download.Size = new System.Drawing.Size(120, 27);
            this.button_Download.TabIndex = 29;
            this.button_Download.Text = "Download";
            this.button_Download.UseVisualStyleBackColor = false;
            this.button_Download.Click += new System.EventHandler(this.button_Download_Click);
            // 
            // speed_Label
            // 
            this.speed_Label.AutoSize = true;
            this.speed_Label.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.speed_Label.Location = new System.Drawing.Point(11, 561);
            this.speed_Label.Name = "speed_Label";
            this.speed_Label.Size = new System.Drawing.Size(99, 15);
            this.speed_Label.TabIndex = 28;
            this.speed_Label.Text = "Download Speed:";
            this.speed_Label.Visible = false;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 579);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(687, 23);
            this.progressBar1.TabIndex = 27;
            this.progressBar1.Visible = false;
            // 
            // lstDownloadLinks
            // 
            this.lstDownloadLinks.FormattingEnabled = true;
            this.lstDownloadLinks.ItemHeight = 15;
            this.lstDownloadLinks.Location = new System.Drawing.Point(12, 401);
            this.lstDownloadLinks.Name = "lstDownloadLinks";
            this.lstDownloadLinks.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstDownloadLinks.Size = new System.Drawing.Size(687, 139);
            this.lstDownloadLinks.Sorted = true;
            this.lstDownloadLinks.TabIndex = 5;
            this.lstDownloadLinks.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstDownloadLinks_KeyDown);
            this.lstDownloadLinks.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstDownloadLinks_MouseDoubleClick_1);
            // 
            // customListBox1
            // 
            this.customListBox1.FormattingEnabled = true;
            this.customListBox1.ItemHeight = 15;
            this.customListBox1.Location = new System.Drawing.Point(12, 12);
            this.customListBox1.Name = "customListBox1";
            this.customListBox1.Size = new System.Drawing.Size(687, 379);
            this.customListBox1.TabIndex = 1;
            this.customListBox1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.customListBox1_MouseDoubleClick);
            // 
            // SavedTorrents
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(711, 583);
            this.Controls.Add(this.progress_Label);
            this.Controls.Add(this.button_Download);
            this.Controls.Add(this.speed_Label);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.lstDownloadLinks);
            this.Controls.Add(this.customListBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinimumSize = new System.Drawing.Size(582, 351);
            this.Name = "SavedTorrents";
            this.Text = "Saved Torrents";
            this.Load += new System.EventHandler(this.SavedTorrents_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CustomListBox customListBox1;
        private CustomListBox lstDownloadLinks;
        private System.Windows.Forms.Label progress_Label;
        private System.Windows.Forms.Button button_Download;
        private System.Windows.Forms.Label speed_Label;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}