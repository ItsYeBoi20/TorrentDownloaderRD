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
            this.lstDownloadLinks = new CustomListBox();
            this.customListBox1 = new CustomListBox();
            this.SuspendLayout();
            // 
            // lstDownloadLinks
            // 
            this.lstDownloadLinks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstDownloadLinks.FormattingEnabled = true;
            this.lstDownloadLinks.ItemHeight = 15;
            this.lstDownloadLinks.Location = new System.Drawing.Point(12, 401);
            this.lstDownloadLinks.Name = "lstDownloadLinks";
            this.lstDownloadLinks.Size = new System.Drawing.Size(687, 139);
            this.lstDownloadLinks.Sorted = true;
            this.lstDownloadLinks.TabIndex = 5;
            this.lstDownloadLinks.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstDownloadLinks_MouseDoubleClick_1);
            // 
            // customListBox1
            // 
            this.customListBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.ClientSize = new System.Drawing.Size(711, 552);
            this.Controls.Add(this.lstDownloadLinks);
            this.Controls.Add(this.customListBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinimumSize = new System.Drawing.Size(582, 351);
            this.Name = "SavedTorrents";
            this.Text = "Saved Torrents";
            this.Load += new System.EventHandler(this.SavedTorrents_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private CustomListBox customListBox1;
        private CustomListBox lstDownloadLinks;
    }
}