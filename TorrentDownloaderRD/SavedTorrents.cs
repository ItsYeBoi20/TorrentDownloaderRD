using RealDebridAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RealDebridAPI.RealDebridClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MediaDownloader
{
    public partial class SavedTorrents : Form
    {
        private RealDebridClient _realDebridClient;
        private bool _isDownloadRunning = false;

        public SavedTorrents()
        {
            InitializeComponent();
        }

        private void SavedTorrents_Load(object sender, EventArgs e)
        {
            getFiles();
        }

        private async void customListBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Gets all the Download Links from the selected Torrent ID

            lstDownloadLinks.Items.Clear();
            string torrentStatus = "";

            try
            {
                string selectedItem = customListBox1.SelectedItem.ToString();
                var match = Regex.Split(selectedItem, ", ID: ");

                RealDebridClient.TorrentInfo torrentInfo = await _realDebridClient.GetTorrentInfoAsync(match[1]);
                torrentStatus = torrentInfo.Status;

                Dictionary<string, string> backupDownloadLinks = null;
                backupDownloadLinks = await _realDebridClient.GetDownloadLinksFromIDAsync(match[1]);

                var usedLinks = new HashSet<string>();

                if (backupDownloadLinks != null)
                {
                    if (torrentStatus == "downloaded")
                    {
                        string downloadLinksToCopy = "";

                        foreach (var kvp in backupDownloadLinks)
                        {
                            if (!string.IsNullOrWhiteSpace(kvp.Value) && !usedLinks.Contains(kvp.Value))
                            {
                                lstDownloadLinks.Items.Add($"{kvp.Key}: {kvp.Value}");
                                usedLinks.Add(kvp.Value);

                                downloadLinksToCopy += kvp.Value + " ";

                            }
                        }

                        button_Download.Enabled = true;
                        progress_Label.Text = "Retrieved Links: " + lstDownloadLinks.Items.Count.ToString();
                        System.Windows.Forms.Clipboard.SetText(downloadLinksToCopy);
                    }
                    else
                    {
                        // If the torrent isnt downloaded, ask user to delete

                        DialogResult result = MessageBox.Show($"Torrent Status is Currently: {torrentStatus},\nDelete from Real-Debrid? ", "Confirmation", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            await _realDebridClient.DeleteTorrentAsync(match[1]);
                            customListBox1.Items.Clear();
                            getFiles();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Download Links are Empty");
                }
            }
            catch
            {
                MessageBox.Show("Error Getting Download Links");
            }
        }

        private void getFiles()
        {
            // Reads Settings.txt for the API Key
            // Gets all the Torrent IDs stored in Real Debrid

            if (File.Exists("Settings.txt"))
            {
                try
                {
                    string[] lines = File.ReadAllLines("Settings.txt");
                    string GetAPIKey = lines[0].Replace("API Key: ", "");

                    if (GetAPIKey == "")
                    {
                        MessageBox.Show("Real-Debrid API Key Not Found\nViewing Your Torrents won't be Possible");
                    }
                    else
                    {
                        _realDebridClient = new RealDebridClient(GetAPIKey);

                        Task.Run(async () =>
                        {
                            var torrents = await _realDebridClient.GetAllTorrentIdAsync();
                            if (torrents != null)
                            {
                                this.Invoke((Action)(() =>
                                {
                                    foreach (var torrent in torrents)
                                    {
                                        customListBox1.Items.Add($"{torrent.Filename}, ID: {torrent.Id}");
                                    }
                                }));
                            }
                        });
                    }
                }
                catch
                {
                    MessageBox.Show("Real-Debrid API Key Not Found\nViewing Your Torrents won't be Possible");
                }
            }
            else
            {
                MessageBox.Show("Real-Debrid API Key Not Found\nViewing Your Torrents won't be Possible");
            }
        }

        #region Controls

        private void lstDownloadLinks_MouseDoubleClick_1(object sender, MouseEventArgs e)
        {
            try
            {
                string[] parts = Regex.Split(lstDownloadLinks.SelectedItem.ToString(), ": ");
                System.Windows.Forms.Clipboard.SetText(parts[1]);
                MessageBox.Show(parts[1] + "\nCopied to Clipboard!");
            }
            catch { }
        }

        private void lstDownloadLinks_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
            {
                try
                {
                    lstDownloadLinks.BeginUpdate();

                    for (int i = lstDownloadLinks.SelectedItems.Count - 1; i >= 0; i--)
                    {
                        lstDownloadLinks.Items.Remove(lstDownloadLinks.SelectedItems[i]);
                    }

                    if (lstDownloadLinks.Items.Count > 0)
                    {
                        lstDownloadLinks.TopIndex = 0;
                    }

                    e.Handled = true;
                }
                catch (IndexOutOfRangeException ex)
                {
                    //Console.WriteLine($"Error: {ex.Message}");
                }
                finally
                {
                    lstDownloadLinks.EndUpdate();
                }
            }
        }

        #endregion Controls

        #region Download

        private async void button_Download_Click(object sender, EventArgs e)
        {
            // Check if the operation is already running
            if (_isDownloadRunning)
            {
                MessageBox.Show("Operation is already in progress.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                this.Size = new Size(727, 653);
                speed_Label.Visible = true;
                progressBar1.Visible = true;
            }
            _isDownloadRunning = true;

            // Open Folder Browser Dialog
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Select the directory to download files to";
                folderBrowserDialog.SelectedPath = AppDomain.CurrentDomain.BaseDirectory;

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Download is Starting");
                    string downloadDirectory = folderBrowserDialog.SelectedPath;
                    await DownloadFilesAsync(downloadDirectory);
                }
            }

            _isDownloadRunning = false;
        }

        public async Task DownloadFilesAsync(string downloadDirectory)
        {
            // Stores all items in the ListBox
            List<string> links = new List<string>();

            if (lstDownloadLinks.Items.Count > 0)
            {
                foreach (var item in lstDownloadLinks.Items)
                {
                    links.Add(item.ToString());
                }

                int totalFiles = links.Count;
                int downloadedFiles = 0;

                progress_Label.Text = $"Downloaded: {downloadedFiles}/{totalFiles} Files";

                foreach (string item in links)
                {
                    var splitLinks = Regex.Split(item.ToString(), ": http");
                    string downloadUrl = "http" + splitLinks[1];

                    string fullPath = splitLinks[0].Trim();

                    fullPath = SanitizePath(fullPath);

                    // Get the directory where the executable is stored.
                    string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

                    // Remove any leading directory separators from the fullPath.
                    string relativePath = fullPath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    // Combine the executable directory and the relative path to form the complete local file path.
                    string localFilePath = Path.Combine(downloadDirectory, relativePath);

                    // Assign API Key
                    string[] lines = File.ReadAllLines("Settings.txt");
                    string APIKey = lines[0].Replace("API Key: ", "");

                    if (APIKey == "")
                    {
                        MessageBox.Show("Real Debrid API Key Not Found\nUsing Its Service Will Be Disabled");
                        return;
                    }
                    else
                    {
                        _realDebridClient = new RealDebridClient(APIKey);
                    }

                    string unrestrictedLink = await _realDebridClient.UnrestrictLinkAsync(downloadUrl);

                    // Ensure the directory exists.
                    string localDirectory = Path.GetDirectoryName(localFilePath);
                    if (!Directory.Exists(localDirectory))
                    {
                        Directory.CreateDirectory(localDirectory);
                    }

                    // Download
                    using (WebClient wc = new WebClient())
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        long totalBytesReceived = 0;
                        Timer timer = new Timer();
                        timer.Interval = 1000; // Update download speed every 1000ms
                        timer.Tick += (timerSender, timerEventArgs) =>
                        {
                            if (stopwatch.IsRunning)
                            {
                                double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                                double downloadSpeed = (totalBytesReceived / 1024d / 1024d) / elapsedSeconds;
                                speed_Label.Text = $"Download Speed: {downloadSpeed.ToString("0.00")} MB/s";
                            }
                        };

                        // Update Progress
                        wc.DownloadProgressChanged += (progressSender, progressChangedEventArgs) =>
                        {
                            if (!stopwatch.IsRunning)
                            {
                                stopwatch.Start();
                                timer.Start();
                            }

                            totalBytesReceived = progressChangedEventArgs.BytesReceived;
                            progressBar1.Value = progressChangedEventArgs.ProgressPercentage;
                        };

                        wc.DownloadFileCompleted += (completedSender, downloadFileCompletedEventArgs) =>
                        {
                            stopwatch.Reset();
                            timer.Stop();
                            downloadedFiles++;
                            progress_Label.Text = $"Downloaded: {downloadedFiles}/{totalFiles} Files";

                            // Check if all files are downloaded
                            if (downloadedFiles == totalFiles)
                            {
                                speed_Label.Text = "Download Speed: Completed";
                            }
                        };

                        // Update the progress label before starting the download
                        progress_Label.Text = $"Downloaded: {downloadedFiles}/{totalFiles} Files";

                        await wc.DownloadFileTaskAsync(new Uri(unrestrictedLink), localFilePath);
                    }
                }
            }
            else
            {
                MessageBox.Show("No links to copy.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string SanitizePath(string path)
        {
            // Remove invalid characters from the path and join them back together

            string[] parts = path.Split(new[] { '/' }, StringSplitOptions.None);

            char[] invalidChars = Path.GetInvalidFileNameChars();

            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = string.Join("", parts[i].Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            }

            return string.Join("/", parts);
        }

        #endregion Download
    }
}
