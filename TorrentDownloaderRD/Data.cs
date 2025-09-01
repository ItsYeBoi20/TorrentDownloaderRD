using RealDebridAPI;
using System;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics;
using System.Linq;
using System.Globalization;
using static TorrentDownloaderRD.Processing.AllDebridClient;
using TorrentDownloaderRD.Processing;
using static System.Net.WebRequestMethods;
using TorrentDownloaderRD;
using static RealDebridAPI.RealDebridClient;

namespace MediaDownloader
{
    public partial class Data : Form
    {
        private RealDebridClient _realDebridClient;
        private AllDebridClient _allDebridClient;
        private bool _deleteAfter = false;
        private bool _isOperationRunning = false;
        private bool _isDownloadRunning = false;
        private bool _cancellationToken = false;
        private System.Windows.Forms.Timer timer;

        public static string TorrentName { get; set; }
        public static string TorrentSize { get; set; }
        public static int Seeders { get; set; }
        public static int Leechers { get; set; }
        public static string Url { get; set; }
        public static string Magnet { get; set; }
        public static string Description { get; set; }

        public Data()
        {
            InitializeComponent();
        }

        private void Data_Load(object sender, EventArgs e)
        {
            // Read API Key from Settings.txt
            if (System.IO.File.Exists("Settings.txt"))
            {
                try
                {
                    string[] lines = System.IO.File.ReadAllLines("Settings.txt");
                    string selectedAPI = lines[0].Replace("API Provider: ", "");
                    string realDebridApiKey = lines[1].Replace("Real-Debrid API Key: ", "");
                    string allDebridApiKey = lines[2].Replace("AllDebrid API Key: ", "");

                    if (selectedAPI == "Real-Debrid")
                    {
                        if (realDebridApiKey == "")
                        {
                            MessageBox.Show("Real Debrid API Key Not Found\nUsing Its Service Will Be Disabled");
                            button_Insert.Enabled = false;
                        }
                        else
                        {
                            _realDebridClient = new RealDebridClient(realDebridApiKey);
                            this.Text = "Torrent Data - Real Debrid";
                        }
                    }
                    else if (selectedAPI == "AllDebrid")
                    {
                        if (allDebridApiKey == "")
                        {
                            MessageBox.Show("AllDebrid API Key Not Found\nUsing Its Service Will Be Disabled");
                            button_Insert.Enabled = false;
                        }
                        else
                        {
                            _allDebridClient = new AllDebridClient(allDebridApiKey);
                            this.Text = "Torrent Data - AllDebrid";
                        }
                    }

                    _deleteAfter = Convert.ToBoolean(lines[4].Replace("DeleteAfter: ", ""));
                }
                catch
                {
                    MessageBox.Show("API Key Not Found\nUsing Its Service Will Be Disabled");
                    button_Insert.Enabled = false;
                }
            }
            else
            {
                MessageBox.Show("API Key Not Found\nUsing Its Service Will Be Disabled");
                button_Insert.Enabled = false;
            }

            textBox1.Text = TorrentName;
            textBox2.Text = TorrentSize;
            textBox3.Text = Seeders.ToString();
            textBox4.Text = Leechers.ToString();
            textBox5.Text = Url;
            textBox6.Text = Magnet;

            string htmlInput = Description;
            string plainTextOutput = HtmlToTextConverter.ConvertHtmlToPlainText(htmlInput);
            textBox7.Text = plainTextOutput;
        }

        private void button_Copy_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(textBox6.Text);
            button_Copy.Text = "Done!";

            if (timer == null)
            {
                timer = new System.Windows.Forms.Timer();
                timer.Interval = 3000;
                timer.Tick += Timer_Tick;
            }

            timer.Start();
        }

        private async void button_Insert_Click(object sender, EventArgs e)
        {
            bool cancelled = false;
            bool check = false;
            bool linksOnUpload = false;
            string magnetLink = textBox6.Text;
            string provider = "";

            // Check if the operation is already running
            if (_isOperationRunning)
            {
                MessageBox.Show("Operation is already in progress.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                lstDownloadLinks.Items.Clear();
            }
            _isOperationRunning = true;

            // Read the provider from Settings.txt
            if (System.IO.File.Exists("Settings.txt"))
            {
                string[] lines = System.IO.File.ReadAllLines("Settings.txt");
                provider = lines[0].Replace("API Provider: ", "");
            }
            else
            {
                MessageBox.Show("Provider not found. Please ensure the settings are configured correctly.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isOperationRunning = false;
                return;
            }

            // Validate the magnet link
            if (string.IsNullOrWhiteSpace(magnetLink))
            {
                MessageBox.Show("Please enter a valid magnet link.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isOperationRunning = true;
                return;
            }

            if (provider == "Real-Debrid")
            {
                try
                {
                    // Add Magnet Link
                    string torrentId = await _realDebridClient.AddMagnetLinkAsync(magnetLink);
                    string latestTorrentId = await _realDebridClient.GetLatestTorrentIdAsync();

                    // Get the latest torrent info
                    RealDebridAPI.RealDebridClient.TorrentInfo torrentInfo = await _realDebridClient.GetTorrentInfoAsync(torrentId);
                    progressBar1.Style = ProgressBarStyle.Continuous;

                    // Display files to the user and allow selection
                    var fileSelectionForm = new FileSelectionForm(torrentInfo.Files);

                    fileSelectionForm.StartPosition = FormStartPosition.Manual;
                    int x = this.Location.X + (this.Width - fileSelectionForm.Width) / 2;
                    int y = this.Location.Y + (this.Height - fileSelectionForm.Height) / 2;
                    fileSelectionForm.Location = new Point(x, y);

                    if (fileSelectionForm.ShowDialog() == DialogResult.OK)
                    {
                        var selectedFileIds = fileSelectionForm.SelectedFileIds;

                        if (selectedFileIds == null || selectedFileIds.Length == 0)
                        {
                            MessageBox.Show("No files selected for download.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        // Select Files given from user
                        await _realDebridClient.SelectFilesAsync(torrentId, selectedFileIds);

                        // Make sure its not downloaded yet
                        while (torrentInfo.Status != "downloaded")
                        {
                            try
                            {
                                // Get the latest torrent info
                                torrentInfo = await _realDebridClient.GetTorrentInfoAsync(torrentId);

                                // Handle the different response statuses
                                if (torrentInfo.Status == "queued" || torrentInfo.Status == "downloading" || torrentInfo.Status == "uploading" || torrentInfo.Status == "compressing")
                                {
                                    if (check == false)
                                    {
                                        MessageBox.Show("Download to the Real-Debrid Server is Starting");
                                        check = true;
                                    }

                                    if (progressBar1.Value != 100)
                                        progressBar1.Value = (int)torrentInfo.Progress;

                                    if (torrentInfo.Status == "uploading")
                                    {
                                        speed_Label.Text = "Download Speed: Uploading";

                                        if (progressBar1.Value != 100)
                                        {
                                            progressBar1.Value = 100;
                                        }

                                        try
                                        {
                                            // Try getting the download links, sometimes they're available while the status is uploading
                                            var downloadLinks = await _realDebridClient.GetDownloadLinksAsync(torrentId, latestTorrentId);

                                            if (downloadLinks.Count != 0)
                                            {
                                                string downloadLinksToCopy = "";

                                                if (selectedFileIds.Length != downloadLinks.Count)
                                                {
                                                    // Display download links to the user by unrestricting the link
                                                    foreach (var linkInfo in downloadLinks)
                                                    {
                                                        // Call GetFileName and retrieve both the file name and size
                                                        var (fileName, combinedFileSize) = await _realDebridClient.GetFileName(linkInfo.DownloadLink);
                                                        string fileSize = FormatFileSize(combinedFileSize > 0 ? combinedFileSize : linkInfo.FileSize);

                                                        // Format the combined file size (if it’s joined)
                                                        lstDownloadLinks.Items.Add($"{fileName}: {linkInfo.DownloadLink} [{fileSize}]");
                                                        downloadLinksToCopy += linkInfo.DownloadLink + " ";
                                                    }
                                                }
                                                else
                                                {
                                                    // Display download links to the user
                                                    foreach (var linkInfo in downloadLinks)
                                                    {
                                                        string fileSize = FormatFileSize(linkInfo.FileSize);
                                                        lstDownloadLinks.Items.Add($"{linkInfo.FileName}: {linkInfo.DownloadLink} [{fileSize}]");
                                                        downloadLinksToCopy += linkInfo.DownloadLink + " ";
                                                    }
                                                }
                                                System.Windows.Forms.Clipboard.SetText(downloadLinksToCopy);
                                            }
                                        }
                                        catch
                                        {

                                        }
                                    }
                                    else
                                    {
                                        // Convert to MB/s
                                        double speedInMbps = torrentInfo.Speed / (1024.0 * 1024.0);
                                        speed_Label.Text = $"Download Speed: {speedInMbps:F2} MB/s";
                                    }
                                }

                                // Check different torrent statuses 
                                if (torrentInfo.Status == "magnet_conversion")
                                {
                                    speed_Label.Text = "Download Speed: Converting";
                                }
                                if (torrentInfo.Status == "compressing")
                                {
                                    speed_Label.Text = "Download Speed: Compressing";
                                }
                                if (torrentInfo.Status == "downloaded")
                                {
                                    break;
                                }
                                else if (torrentInfo.Status == "error" || torrentInfo.Status == "virus" || torrentInfo.Status == "dead" || torrentInfo.Status == "magnet_error")
                                {
                                    MessageBox.Show($"Torrent download failed with status: {torrentInfo.Status}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                await Task.Delay(5000);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"An error occurred while updating progress: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                        if (linksOnUpload == false)
                        {
                            // Retrieve Download Links
                            var downloadLinks = await _realDebridClient.GetDownloadLinksAsync(torrentId, latestTorrentId);

                            string downloadLinksToCopy = "";

                            if (selectedFileIds.Length != downloadLinks.Count)
                            {
                                // Display download links to the user by unrestricting the link
                                foreach (var linkInfo in downloadLinks)
                                {
                                    // Call GetFileName and retrieve both the file name and size
                                    var (fileName, combinedFileSize) = await _realDebridClient.GetFileName(linkInfo.DownloadLink);
                                    string fileSize = FormatFileSize(combinedFileSize > 0 ? combinedFileSize : linkInfo.FileSize);

                                    // Format the combined file size (if it’s joined)
                                    lstDownloadLinks.Items.Add($"{fileName}: {linkInfo.DownloadLink} [{fileSize}]");
                                    downloadLinksToCopy += linkInfo.DownloadLink + " ";
                                }
                            }
                            else
                            {
                                // Display download links to the user
                                foreach (var linkInfo in downloadLinks)
                                {
                                    string fileSize = FormatFileSize(linkInfo.FileSize);
                                    lstDownloadLinks.Items.Add($"{linkInfo.FileName}: {linkInfo.DownloadLink} [{fileSize}]");
                                    downloadLinksToCopy += linkInfo.DownloadLink + " ";
                                }
                            }
                            System.Windows.Forms.Clipboard.SetText(downloadLinksToCopy);
                        }

                        if (_deleteAfter)
                        {
                            // Delete Torrent
                            await _realDebridClient.DeleteTorrentAsync(torrentId);
                        }

                        MessageBox.Show("Download links retrieved and copied to clipboard.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // Delete Torrent on cancellation
                        await _realDebridClient.DeleteTorrentAsync(torrentId);
                        cancelled = true;
                        MessageBox.Show("File selection was canceled.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    long totalSizeInBytes = 0;
                    foreach (string item in lstDownloadLinks.Items)
                    {
                        var match = Regex.Match(item, @"\[(\d+\.?\d*)\s*(KB|MB|GB|TB)\]");

                        if (match.Success)
                        {
                            double fileSize = double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);

                            string unitPart = match.Groups[2].Value.ToUpper();

                            long sizeInBytes = 0;
                            switch (unitPart)
                            {
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


                    progress_Label.Text = $"Retrieved Links: {lstDownloadLinks.Items.Count}, Total Size: {FormatFileSize(totalSizeInBytes)}";

                    if (!cancelled)
                    {
                        speed_Label.Text = "Download Speed: Completed";
                        progressBar1.Value = 0;
                    }
                }
            }
            else if (provider == "AllDebrid")
            {
                try
                {
                    string torrentID = null;

                    bool success = false;
                    int retries = 0;
                    while (retries < 5 && !success)
                    {
                        try
                        {
                            var result = await _allDebridClient.UploadMagnetAsync(magnetLink);
                            torrentID = $"{result.Id}";

                            success = true;
                        }
                        catch { }

                        if (!success)
                        {
                            await Task.Delay(1000);
                            retries++;
                        }
                    }
                    
                    var magnet = await _allDebridClient.GetStatusByIDAsync((long)Convert.ToDouble(torrentID));
                    if (magnet != null && torrentID != null)
                    {
                        while (magnet.StatusCode != 4)
                        {
                            try
                            {
                                magnet = await _allDebridClient.GetStatusByIDAsync((long)Convert.ToDouble(torrentID));

                                if (magnet.StatusCode == 0)
                                {
                                    speed_Label.Text = "Download Speed: In Queue";
                                }
                                else if (magnet.StatusCode == 1)
                                {
                                    speed_Label.Text = "Download Speed: Downloading to Server";

                                    if (magnet.Downloaded.HasValue && magnet.Size != 0)
                                    {
                                        double downloaded = magnet.Downloaded.Value;

                                        double size = magnet.Size;
                                        int progressPercentage = (int)((downloaded / size) * 100);
                                        progressBar1.Value = progressPercentage;

                                        double speedInMbps = magnet.DownloadSpeed.HasValue ? magnet.DownloadSpeed.Value / (1024.0 * 1024.0) : 0.0;
                                        speed_Label.Text = $"Download Speed: {speedInMbps:F2} MB/s";
                                    }                                    
                                }
                                else if (magnet.StatusCode == 2)
                                {
                                    speed_Label.Text = "Download Speed: Compressing/Moving";
                                }
                                else if (magnet.StatusCode == 3)
                                {
                                    speed_Label.Text = "Download Speed: Uploading";
                                }

                                await Task.Delay(5000);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"An error occurred while updating progress: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                        if (magnet.StatusCode == 4)
                        {
                            var files = await _allDebridClient.GetDownloadableFilesAsync((long)Convert.ToDouble(torrentID));

                            if (files.Count == 0)
                            {
                                MessageBox.Show("No downloadable files found.");
                                return;
                            }

                            var allDebridSelectionForm = new AllDebridSelectionForm(files);

                            allDebridSelectionForm.StartPosition = FormStartPosition.CenterParent;

                            if (allDebridSelectionForm.ShowDialog() == DialogResult.OK)
                            {
                                var selectedFiles = allDebridSelectionForm.SelectedFiles;

                                if (selectedFiles.Count == 0)
                                {
                                    MessageBox.Show("No files selected for download.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return;
                                }

                                // Add selected files to the ListBox
                                foreach (var file in selectedFiles)
                                {
                                    lstDownloadLinks.Items.Add($"{file.Name}: {file.Link} [{ConvertFileSize(file.Size)}]");
                                    System.Windows.Forms.Clipboard.SetText(file.Link);
                                }
                            }
                            MessageBox.Show("Download links retrieved and copied to clipboard.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        if (_deleteAfter)
                        {
                            await _allDebridClient.DeleteMagnetAsync((long)Convert.ToDouble(torrentID));
                        }
                    }
                    else
                    {
                        MessageBox.Show("Magnet not found or error retrieving status.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    long totalSizeInBytes = 0;
                    foreach (string item in lstDownloadLinks.Items)
                    {
                        var match = Regex.Match(item, @"\[(\d+\.?\d*)\s*(KB|MB|GB|TB)\]");

                        if (match.Success)
                        {
                            double fileSize = double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);

                            string unitPart = match.Groups[2].Value.ToUpper();

                            long sizeInBytes = 0;
                            switch (unitPart)
                            {
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


                    progress_Label.Text = $"Retrieved Links: {lstDownloadLinks.Items.Count}, Total Size: {FormatFileSize(totalSizeInBytes)}";

                    if (!cancelled)
                    {
                        speed_Label.Text = "Download Speed: Completed";
                        progressBar1.Value = 0;
                    }
                }
            }

            _isOperationRunning = false;

            if (lstDownloadLinks.Items.Count > 0)
            {
                button1.Enabled = true;
            }
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

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            button_Copy.Text = "Copy Magnet Link";
        }

        private void lstDownloadLinks_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string[] parts = Regex.Split(lstDownloadLinks.SelectedItem.ToString(), ": ");
            System.Windows.Forms.Clipboard.SetText(parts[1]);
            MessageBox.Show(parts[1] + "\nCopied to Clipboard!");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (lstDownloadLinks.Items.Count == 0)
            {
                MessageBox.Show("No links to download.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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
                button1.Text = "Cancel Download";
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

            button1.Text = "Download";
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

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("This will open the magnet link inside QBittorrent,\nAre you sure?", "Confirmation", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = textBox6.Text,
                        UseShellExecute = true
                    });
                    Console.WriteLine("Magnet link opened successfully.");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine("No application is associated with magnet links. Please install or configure a torrent client.");
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    Console.WriteLine("An error occurred while trying to open the magnet link: " + ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An unexpected error occurred: " + ex.Message);
                }
            }
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
                                        
                    long totalSizeInBytes = 0;
                    foreach (string item in lstDownloadLinks.Items)
                    {
                        var match = Regex.Match(item, @"\[(\d+\.?\d*)\s*(KB|MB|GB|TB)\]");

                        if (match.Success)
                        {
                            double fileSize = double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);

                            string unitPart = match.Groups[2].Value.ToUpper();

                            long sizeInBytes = 0;
                            switch (unitPart)
                            {
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
                    progress_Label.Text = $"Retrieved Links: {lstDownloadLinks.Items.Count}, Total Size: {FormatFileSize(totalSizeInBytes)}";
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
    }
}
