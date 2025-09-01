using RealDebridAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TorrentDownloaderRD.Processing;
using static RealDebridAPI.RealDebridClient;
using static TorrentDownloaderRD.Processing.AllDebridClient;

namespace MediaDownloader
{
    public partial class SavedTorrents : Form
    {
        private RealDebridClient _realDebridClient;
        private AllDebridClient _allDebridClient;
        private bool _isDownloadRunning = false;
        private bool _cancellationToken = false;
        private List<string> listIDs = new List<string>();
        private string currentProvider;

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

            if (currentProvider == "Real-Debrid")
            {
                try
                {
                    string selectedItem = customListBox1.SelectedItem.ToString();
                    string match = listIDs[customListBox1.SelectedIndex];
                    //var match = Regex.Split(selectedItem, ", ID: ");

                    RealDebridClient.TorrentInfo torrentInfo = await _realDebridClient.GetTorrentInfoAsync(match); //match[1]
                    torrentStatus = torrentInfo.Status;

                    Dictionary<string, string> backupDownloadLinks = null;
                    backupDownloadLinks = await _realDebridClient.GetDownloadLinksFromIDAsync(match); //match[1]

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

                            System.Windows.Forms.Clipboard.SetText(downloadLinksToCopy);
                        }
                        else
                        {
                            // If the torrent isnt downloaded, ask user to delete

                            DialogResult result = MessageBox.Show($"Torrent Status is Currently: {torrentStatus},\nDelete from Real-Debrid? ", "Confirmation", MessageBoxButtons.YesNo);
                            if (result == DialogResult.Yes)
                            {
                                await _realDebridClient.DeleteTorrentAsync(match); //match[1]
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
                finally
                {
                    long totalSizeInBytes = 0;
                    foreach (string item in lstDownloadLinks.Items)
                    {
                        var match = Regex.Match(item, @"\[(\d+\.?\d*)\s*(B|KB|MB|GB|TB)\]");

                        if (match.Success)
                        {
                            double fileSize = double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);

                            string unitPart = match.Groups[2].Value.ToUpper();

                            long sizeInBytes = 0;
                            switch (unitPart)
                            {
                                case "B":
                                    sizeInBytes = (long)fileSize;
                                    break;
                                case "KB":
                                    sizeInBytes = (long)(fileSize * 1024);
                                    break;
                                case "MB":
                                    sizeInBytes = (long)(fileSize * 1024 * 1024);
                                    break;
                                case "GB":
                                    sizeInBytes = (long)(fileSize * 1024 * 1024 * 1024);
                                    break;
                                case "TB":
                                    sizeInBytes = (long)(fileSize * 1024 * 1024 * 1024 * 1024L);
                                    break;
                                default:
                                    MessageBox.Show("Unknown file size unit: " + unitPart);
                                    break;
                            }
                            totalSizeInBytes += sizeInBytes;
                        }
                        else
                        {
                            MessageBox.Show("Failed to parse size from item: " + item);
                        }
                    }
                    double totalSizeInGB = totalSizeInBytes / (1024.0 * 1024 * 1024);

                    progress_Label.Visible = true;
                    button_Download.Enabled = true;
                    progress_Label.Text = "Retrieved Links: " + lstDownloadLinks.Items.Count.ToString() + " , Total Size: " + FormatFileSize(totalSizeInBytes);
                }
            }
            else if (currentProvider == "AllDebrid")
            {
                try
                {
                    string selectedItem = customListBox1.SelectedItem.ToString();
                    string match = listIDs[customListBox1.SelectedIndex];
                    var usedLinks = new HashSet<string>();

                    var magnet = await _allDebridClient.GetStatusByIDAsync((long)Convert.ToDouble(match));
                    if (magnet.StatusCode == 4)
                    {
                        string downloadLinksToCopy = "";

                        var files = await _allDebridClient.GetDownloadableFilesAsync((long)Convert.ToDouble(match));

                        if (files.Count == 0)
                        {
                            MessageBox.Show("No files available for download.");
                        }
                        else
                        {
                            foreach (var file in files)
                            {
                                if (!string.IsNullOrWhiteSpace(file.Name) && !usedLinks.Contains(file.Link))
                                {
                                    lstDownloadLinks.Items.Add($"{file.Name}: {file.Link} [{ConvertFileSize(file.Size)}]");
                                    usedLinks.Add(file.Link);

                                    downloadLinksToCopy += file.Link + " ";
                                }
                            }

                            System.Windows.Forms.Clipboard.SetText(downloadLinksToCopy);
                        }                        
                    }
                }
                catch
                {
                    MessageBox.Show("Error Getting Download Links");
                }
                finally
                {
                    long totalSizeInBytes = 0;
                    foreach (string item in lstDownloadLinks.Items)
                    {
                        var match = Regex.Match(item, @"\[(\d+\.?\d*)\s*(B|KB|MB|GB|TB)\]");

                        if (match.Success)
                        {
                            double fileSize = double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);

                            string unitPart = match.Groups[2].Value.ToUpper();

                            long sizeInBytes = 0;
                            switch (unitPart)
                            {
                                case "B":
                                    sizeInBytes = (long)fileSize;
                                    break;
                                case "KB":
                                    sizeInBytes = (long)(fileSize * 1024);
                                    break;
                                case "MB":
                                    sizeInBytes = (long)(fileSize * 1024 * 1024);
                                    break;
                                case "GB":
                                    sizeInBytes = (long)(fileSize * 1024 * 1024 * 1024);
                                    break;
                                case "TB":
                                    sizeInBytes = (long)(fileSize * 1024 * 1024 * 1024 * 1024L);
                                    break;
                                default:
                                    MessageBox.Show("Unknown file size unit: " + unitPart);
                                    break;
                            }
                            totalSizeInBytes += sizeInBytes;
                        }
                        else
                        {
                            MessageBox.Show("Failed to parse size from item: " + item);
                        }
                    }
                    double totalSizeInGB = totalSizeInBytes / (1024.0 * 1024 * 1024);

                    progress_Label.Visible = true;
                    button_Download.Enabled = true;
                    progress_Label.Text = "Retrieved Links: " + lstDownloadLinks.Items.Count.ToString() + " , Total Size: " + FormatFileSize(totalSizeInBytes);
                }
            }
        }

        private string ConvertFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return string.Format(CultureInfo.InvariantCulture, "{0:0.00} {1}", len, sizes[order]);
        }

        public string FormatFileSize(long bytes)
        {
            if (bytes >= 1_099_511_627_776) // TB
                return (bytes / 1_099_511_627_776D).ToString("0.00", CultureInfo.InvariantCulture) + " TB";
            if (bytes >= 1_073_741_824) // GB
                return (bytes / 1_073_741_824D).ToString("0.00", CultureInfo.InvariantCulture) + " GB";
            if (bytes >= 1_048_576) // MB
                return (bytes / 1_048_576D).ToString("0.00", CultureInfo.InvariantCulture) + " MB";
            if (bytes >= 1024) // KB
                return (bytes / 1024D).ToString("0.00", CultureInfo.InvariantCulture) + " KB";
            return bytes + " bytes";
        }

        private void getFiles()
        {
            // Reads Settings.txt for the API Key
            // Gets all the Torrent IDs stored in Real Debrid

            listIDs.Clear();
            if (File.Exists("Settings.txt"))
            {
                try
                {
                    string[] lines = File.ReadAllLines("Settings.txt");
                    string GetAPIProvider = lines[0].Replace("API Provider: ", "");
                    string APIKey = "";

                    if (GetAPIProvider == "Real-Debrid")
                    {
                        APIKey = lines[1].Replace("Real-Debrid API Key: ", "");
                        currentProvider = "Real-Debrid";
                        this.Text = "Saved Torrents - Real Debrid";

                        if (APIKey == "")
                        {
                            MessageBox.Show("Real-Debrid API Key Not Found\nViewing Your Torrents won't be Possible");
                        }
                        else
                        {
                            _realDebridClient = new RealDebridClient(APIKey);

                            Task.Run(async () =>
                            {
                                var torrents = await _realDebridClient.GetAllTorrentIdAsync();
                                if (torrents != null)
                                {
                                    this.Invoke((Action)(() =>
                                    {
                                        foreach (var torrent in torrents)
                                        {
                                            listIDs.Add(torrent.Id);
                                            customListBox1.Items.Add($"{torrent.Filename}");
                                            //customListBox1.Items.Add($"{torrent.Filename}, ID: {torrent.Id}");
                                        }
                                    }));
                                }
                            });
                        }
                    }
                    else if (GetAPIProvider == "AllDebrid")
                    {
                        APIKey = lines[2].Replace("AllDebrid API Key: ", "");
                        currentProvider = "AllDebrid";
                        this.Text = "Saved Torrents - AllDebrid";

                        if (APIKey == "")
                        {
                            MessageBox.Show("Real-Debrid API Key Not Found\nViewing Your Torrents won't be Possible");
                        }
                        else
                        {
                            _allDebridClient = new AllDebridClient(APIKey);

                            Task.Run(async () =>
                            {
                                var torrents = await _allDebridClient.GetAllTorrentsAsync();

                                if (torrents != null)
                                {
                                    var readyTorrents = torrents.Where(t => t.StatusCode == 4).ToList();

                                    this.Invoke((Action)(() =>
                                    {
                                        foreach (var torrent in readyTorrents)
                                        {
                                            listIDs.Add(torrent.Id.ToString());
                                            customListBox1.Items.Add($"{torrent.Filename}");
                                        }
                                    }));
                                }
                            });
                        }
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
            if (!_isDownloadRunning && (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete))
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

        private async void customListBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
            {
                DialogResult result = MessageBox.Show($"Delete from Real-Debrid? ", "Are You Sure?", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    await _realDebridClient.DeleteTorrentAsync(listIDs[customListBox1.SelectedIndex]); //match[1]
                    customListBox1.Items.Clear();
                    getFiles();
                }
            }
        }

        #endregion Controls

        #region Download

        private async void button_Download_Click(object sender, EventArgs e)
        {
            if (lstDownloadLinks.Items.Count == 0)
            {
                MessageBox.Show("No links to download.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if the operation is already running
            if (_isDownloadRunning)
            {
                DialogResult result = MessageBox.Show("Operation already in progress, \nDo you want to stop the process?", "Stop Process", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    _cancellationToken = true;
                }

                return;
            }
            else
            {
                customListBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                lstDownloadLinks.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                progress_Label.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                speed_Label.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                button_Download.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                progressBar1.Anchor = AnchorStyles.Top | AnchorStyles.Left;

                this.Size = new Size(this.Width, this.Height + 31);
                speed_Label.Visible = true;
                progress_Label.Visible = true;
                progressBar1.Visible = true;

                button_Download.Text = "Cancel Download";
            }
            _isDownloadRunning = true;

            // Open Folder Browser Dialog
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Select the directory to download files to";
                folderBrowserDialog.SelectedPath = AppDomain.CurrentDomain.BaseDirectory;

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string downloadDirectory = folderBrowserDialog.SelectedPath;
                    await DownloadFilesAsync(downloadDirectory);
                }
            }

            button_Download.Text = "Download";
            _cancellationToken = false;
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
                    if (_cancellationToken)
                    {
                        progress_Label.Text = $"Downloaded: {downloadedFiles}/{totalFiles} Files";
                        speed_Label.Text = "Download Speed: Completed";
                        break;
                    }

                    var splitLinks = Regex.Split(item.ToString(), ": http");

                    var completeUrl = Regex.Split("http" + splitLinks[1], " ");
                    string downloadUrl = completeUrl[0];

                    string fullPath = splitLinks[0].Trim();

                    fullPath = SanitizePath(fullPath);

                    // Get the directory where the executable is stored.
                    string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

                    // Remove any leading directory separators from the fullPath.
                    string relativePath = fullPath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    // Combine the executable directory and the relative path to form the complete local file path.
                    string localFilePath = Path.Combine(downloadDirectory, relativePath);

                    if (splitLinks[1].Contains("real-debrid.com"))
                    {
                        string[] lines = System.IO.File.ReadAllLines("Settings.txt");
                        string APIKey = lines[1].Replace("Real-Debrid API Key: ", "");

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

                        for (int i = lstDownloadLinks.Items.Count - 1; i >= 0; i--)
                        {
                            if (lstDownloadLinks.Items[i].ToString().Contains(item))
                            {
                                lstDownloadLinks.Items.RemoveAt(i);
                            }
                        }
                    }
                    else if (splitLinks[1].Contains("alldebrid.com"))
                    {
                        string[] lines = System.IO.File.ReadAllLines("Settings.txt");
                        string APIKey = lines[2].Replace("AllDebrid API Key: ", "");

                        if (APIKey == "")
                        {
                            MessageBox.Show("AllDebrid API Key Not Found\nUsing Its Service Will Be Disabled");
                            return;
                        }
                        else
                        {
                            _allDebridClient = new AllDebridClient(APIKey);
                        }

                        var unrestrictedLink = await _allDebridClient.UnlockLinkAsync(downloadUrl);

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

                            await wc.DownloadFileTaskAsync(new Uri(unrestrictedLink.Link), localFilePath);
                        }

                        for (int i = lstDownloadLinks.Items.Count - 1; i >= 0; i--)
                        {
                            if (lstDownloadLinks.Items[i].ToString().Contains(item))
                            {
                                lstDownloadLinks.Items.RemoveAt(i);
                            }
                        }
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
