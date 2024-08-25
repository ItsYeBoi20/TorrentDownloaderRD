using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MediaDownloader.Scrapers;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections;
using System.Net.Http;
using RealDebridAPI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics;

namespace MediaDownloader
{
    public partial class Main : Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
        private const int IDC_HAND = 32649;

        public string formTitle = "Torrent Searcher";
        public bool isFinished = false;
        public static Dictionary<string, string> magnetLinksCSV = new Dictionary<string, string>(); //torrents-csv magnet links
        public static Dictionary<string, string> magnetLinksPirate = new Dictionary<string, string>(); //piratebay magnet links
        public static Dictionary<string, string> magnetLinksGalaxy = new Dictionary<string, string>(); //torrentgalaxy magnet links

        public Main()
        {
            InitializeComponent();
            label_Saved.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            SetDoubleBuffer(dataGridView_Torrents, true);

            var numericUpDownTextBox = numericUpDown_Seeders.Controls[1] as System.Windows.Forms.TextBox;
            if (numericUpDownTextBox != null)
            {
                numericUpDownTextBox.TextChanged += NumericUpDownTextBox_TextChanged;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox_Content.SelectedIndex = 0;
            comboBox_SortBy.SelectedIndex = 0;
        }

        static void SetDoubleBuffer(Control dgv, bool DoubleBuffered)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, dgv, new object[] { DoubleBuffered });
        }

        #region Controls

        private async void dataGridView_Torrents_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var selectedRow = dataGridView_Torrents.Rows[e.RowIndex];
                string name = selectedRow.Cells[0].Value.ToString();
                string size = selectedRow.Cells[1].Value.ToString();
                int seeders = int.Parse(selectedRow.Cells[2].Value.ToString());
                int leechers = int.Parse(selectedRow.Cells[3].Value.ToString());
                string url = selectedRow.Cells[4].Value.ToString();

                int maxRetries = 5; // Increased retries
                int retries = 0;
                bool success = false;

                while (!success && retries < maxRetries)
                {
                    try
                    {
                        if (url.Contains("1337x.to"))
                        {
                            var xToWeb = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument xToDoc = xToWeb.Load(url);

                            var magnetLinkNode = xToDoc.DocumentNode.SelectSingleNode("//*[@id='openPopup']");
                            string magnetLink = magnetLinkNode != null ? magnetLinkNode.GetAttributeValue("href", "") : "Magnet link not found";

                            var descriptionNode = xToDoc.DocumentNode.SelectSingleNode("//*[@id='description']");
                            string description = descriptionNode != null ? descriptionNode.InnerHtml : "Description not found";

                            string pattern = @"^magnet:\?xt=urn:[a-zA-Z0-9]+:[a-zA-Z0-9]{32,40}(&.*)?$";
                            Regex regex = new Regex(pattern);
                            if (regex.IsMatch(magnetLink))
                            {
                                success = true;
                            }
                            else
                            {
                                throw new Exception("Magnet link node not found");
                            }

                            if (success)
                            {
                                Data c = new Data();
                                Data.TorrentName = name;
                                Data.TorrentSize = size;
                                Data.Seeders = seeders;
                                Data.Leechers = leechers;
                                Data.Url = url;
                                Data.Magnet = magnetLink;
                                Data.Description = description;

                                c.StartPosition = FormStartPosition.Manual;
                                int x = this.Location.X + (this.Width - c.Width) / 2;
                                int y = this.Location.Y + (this.Height - c.Height) / 2;
                                c.Location = new Point(x, y);
                                c.Show();
                            }
                        }
                        else if (url.Contains("nyaa.si"))
                        {
                            var nyaaWeb = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument nyaaDoc = nyaaWeb.Load(url);

                            var magnetLinkNode = nyaaDoc.DocumentNode.SelectSingleNode("/html/body/div/div[1]/div[3]/a[2]");
                            string magnetLink = magnetLinkNode != null ? magnetLinkNode.GetAttributeValue("href", "") : "Magnet link not found";

                            var descriptionNode = nyaaDoc.DocumentNode.SelectSingleNode("/html/body/div/div[2]/div");
                            string description = descriptionNode != null ? descriptionNode.InnerHtml : "Description not found";

                            string pattern = @"^magnet:\?xt=urn:[a-zA-Z0-9]+:[a-zA-Z0-9]{32,40}(&.*)?$";
                            Regex regex = new Regex(pattern);
                            if (regex.IsMatch(magnetLink))
                            {
                                success = true;
                            }
                            else
                            {
                                throw new Exception("Magnet link node not found");
                            }

                            if (success)
                            {
                                Data c = new Data();
                                Data.TorrentName = name;
                                Data.TorrentSize = size;
                                Data.Seeders = seeders;
                                Data.Leechers = leechers;
                                Data.Url = url;
                                Data.Magnet = magnetLink;
                                Data.Description = description;

                                c.StartPosition = FormStartPosition.Manual;
                                int x = this.Location.X + (this.Width - c.Width) / 2;
                                int y = this.Location.Y + (this.Height - c.Height) / 2;
                                c.Location = new Point(x, y);
                                c.Show();
                            }
                        }
                        else if (url.Contains("limetorrents.lol"))
                        {
                            var limeWeb = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument limeDoc = limeWeb.Load(url);

                            var magnetLinkNode = limeDoc.DocumentNode.SelectSingleNode("//a[contains(@href, 'magnet:')]");
                            string magnetLink = magnetLinkNode != null ? magnetLinkNode.GetAttributeValue("href", "") : "Magnet link not found";

                            var descriptionNode = limeDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'description')]");
                            string description = descriptionNode != null ? descriptionNode.InnerHtml : "Description not found";

                            string pattern = @"^magnet:\?xt=urn:[a-zA-Z0-9]+:[a-zA-Z0-9]{32,40}(&.*)?$";
                            Regex regex = new Regex(pattern);
                            if (regex.IsMatch(magnetLink))
                            {
                                success = true;
                            }
                            else
                            {
                                throw new Exception("Magnet link node not found");
                            }

                            if (success)
                            {
                                Data c = new Data();
                                Data.TorrentName = name;
                                Data.TorrentSize = size;
                                Data.Seeders = seeders;
                                Data.Leechers = leechers;
                                Data.Url = url;
                                Data.Magnet = magnetLink;
                                Data.Description = description;

                                c.StartPosition = FormStartPosition.Manual;
                                int x = this.Location.X + (this.Width - c.Width) / 2;
                                int y = this.Location.Y + (this.Height - c.Height) / 2;
                                c.Location = new Point(x, y);
                                c.Show();
                            }
                        }
                        else if (url.Contains("torrents-csv"))
                        {
                            string magnetLink = magnetLinksCSV.ContainsKey(name) ? magnetLinksCSV[name] : "Magnet link not found";

                            string pattern = @"^magnet:\?xt=urn:[a-zA-Z0-9]+:[a-zA-Z0-9]{32,40}(&.*)?$";
                            Regex regex = new Regex(pattern);
                            if (regex.IsMatch(magnetLink))
                            {
                                success = true;
                            }
                            else
                            {
                                throw new Exception("Magnet link node not found");
                            }

                            if (success)
                            {
                                Data c = new Data();
                                Data.TorrentName = name;
                                Data.TorrentSize = size;
                                Data.Seeders = seeders;
                                Data.Leechers = leechers;
                                Data.Url = url;
                                Data.Magnet = magnetLink;
                                Data.Description = "No Description Available";

                                c.StartPosition = FormStartPosition.Manual;
                                int x = this.Location.X + (this.Width - c.Width) / 2;
                                int y = this.Location.Y + (this.Height - c.Height) / 2;
                                c.Location = new Point(x, y);
                                c.Show();
                            }
                        }
                        else if (url.Contains("torrentproject.cc"))
                        {
                            var tproject = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument tprojectDoc = tproject.Load(url);

                            var scraperTorrentProject = new ScraperTorrentProject();
                            var magnetLink = await scraperTorrentProject.DownloadTorrentAsync(url);

                            var descriptionNode = tprojectDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'description')]");
                            string description = descriptionNode != null ? descriptionNode.InnerHtml : "Description not found";

                            string pattern = @"^magnet:\?xt=urn:[a-zA-Z0-9]+:[a-zA-Z0-9]{32,40}(&.*)?$";
                            Regex regex = new Regex(pattern);
                            if (regex.IsMatch(magnetLink))
                            {
                                success = true;
                            }
                            else
                            {
                                throw new Exception("Magnet link node not found");
                            }

                            if (success)
                            {
                                Data c = new Data();
                                Data.TorrentName = name;
                                Data.TorrentSize = size;
                                Data.Seeders = seeders;
                                Data.Leechers = leechers;
                                Data.Url = url;
                                Data.Magnet = magnetLink;
                                Data.Description = description;

                                c.StartPosition = FormStartPosition.Manual;
                                int x = this.Location.X + (this.Width - c.Width) / 2;
                                int y = this.Location.Y + (this.Height - c.Height) / 2;
                                c.Location = new Point(x, y);
                                c.Show();
                            }
                        }
                        else if (url.Contains("thepiratebay.org"))
                        {
                            var pirateWeb = new HtmlAgilityPack.HtmlWeb();
                            string magnetLink = magnetLinksPirate.ContainsKey(name) ? magnetLinksPirate[name] : "Magnet link not found";

                            string pattern = @"^magnet:\?xt=urn:[a-zA-Z0-9]+:[a-zA-Z0-9]{32,40}(&.*)?$";
                            Regex regex = new Regex(pattern);
                            if (regex.IsMatch(magnetLink))
                            {
                                success = true;
                            }
                            else
                            {
                                throw new Exception("Magnet link node not found");
                            }

                            if (success)
                            {
                                Data c = new Data();
                                Data.TorrentName = name;
                                Data.TorrentSize = size;
                                Data.Seeders = seeders;
                                Data.Leechers = leechers;
                                Data.Url = url;
                                Data.Magnet = magnetLink;
                                Data.Description = "No Description Available";

                                c.StartPosition = FormStartPosition.Manual;
                                int x = this.Location.X + (this.Width - c.Width) / 2;
                                int y = this.Location.Y + (this.Height - c.Height) / 2;
                                c.Location = new Point(x, y);
                                c.Show();
                            }
                        }
                        else if (url.Contains("torlock2.com"))
                        {
                            var tproject = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument tprojectDoc = tproject.Load(url);

                            var magnetLinkNode = tprojectDoc.DocumentNode.SelectSingleNode("/html/body/article/div[2]/div/div[2]/dl[3]/dd");
                            string magnetLink = "magnet:?xt=urn:btih:" + magnetLinkNode.InnerHtml;

                            var descriptionNode = tprojectDoc.DocumentNode.SelectSingleNode("//*[@id=\"description\"]/div[2]/blockquote");
                            string description = descriptionNode != null ? descriptionNode.InnerHtml : "Description not found";

                            string pattern = @"^magnet:\?xt=urn:[a-zA-Z0-9]+:[a-zA-Z0-9]{32,40}(&.*)?$";
                            Regex regex = new Regex(pattern);
                            if (regex.IsMatch(magnetLink))
                            {
                                success = true;
                            }
                            else
                            {
                                throw new Exception("Magnet link node not found");
                            }

                            if (success)
                            {
                                Data c = new Data();
                                Data.TorrentName = name;
                                Data.TorrentSize = size;
                                Data.Seeders = seeders;
                                Data.Leechers = leechers;
                                Data.Url = url;
                                Data.Magnet = magnetLink;
                                Data.Description = description;

                                c.StartPosition = FormStartPosition.Manual;
                                int x = this.Location.X + (this.Width - c.Width) / 2;
                                int y = this.Location.Y + (this.Height - c.Height) / 2;
                                c.Location = new Point(x, y);
                                c.Show();
                            }
                        }
                        else if (url.Contains("yourbittorrent.com"))
                        {
                            var tproject = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument tprojectDoc = tproject.Load(url);

                            var magnetLinkNode = tprojectDoc.DocumentNode.SelectSingleNode("/html/body/div/div[1]/div[2]/div/div[2]/div[8]/div[2]/kbd");
                            string magnetLink = "magnet:?xt=urn:btih:" + magnetLinkNode.InnerHtml;

                            string pattern = @"^magnet:\?xt=urn:[a-zA-Z0-9]+:[a-zA-Z0-9]{32,40}(&.*)?$";
                            Regex regex = new Regex(pattern);
                            if (regex.IsMatch(magnetLink))
                            {
                                success = true;
                            }
                            else
                            {
                                throw new Exception("Magnet link node not found");
                            }

                            if (success)
                            {
                                Data c = new Data();
                                Data.TorrentName = name;
                                Data.TorrentSize = size;
                                Data.Seeders = seeders;
                                Data.Leechers = leechers;
                                Data.Url = url;
                                Data.Magnet = magnetLink;
                                Data.Description = "No Description Available";

                                c.StartPosition = FormStartPosition.Manual;
                                int x = this.Location.X + (this.Width - c.Width) / 2;
                                int y = this.Location.Y + (this.Height - c.Height) / 2;
                                c.Location = new Point(x, y);
                                c.Show();
                            }
                        }
                        else if (url.Contains("torrentdownload.info"))
                        {
                            var torrentWeb = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument torrentDoc = torrentWeb.Load(url);

                            var magnetLinkNode = torrentDoc.DocumentNode.SelectSingleNode("/html/body/div/table[2]/tbody/tr[3]/td[2]");
                            string magnetLink = "magnet:?xt=urn:btih:" + magnetLinkNode.InnerHtml;

                            string pattern = @"^magnet:\?xt=urn:[a-zA-Z0-9]+:[a-zA-Z0-9]{32,40}(&.*)?$";
                            Regex regex = new Regex(pattern);
                            if (regex.IsMatch(magnetLink))
                            {
                                success = true;
                            }
                            else
                            {
                                throw new Exception("Magnet link node not found");
                            }

                            if (success)
                            {
                                Data c = new Data();
                                Data.TorrentName = name;
                                Data.TorrentSize = size;
                                Data.Seeders = seeders;
                                Data.Leechers = leechers;
                                Data.Url = url;
                                Data.Magnet = magnetLink;
                                Data.Description = "No Description Available";

                                c.StartPosition = FormStartPosition.Manual;
                                int x = this.Location.X + (this.Width - c.Width) / 2;
                                int y = this.Location.Y + (this.Height - c.Height) / 2;
                                c.Location = new Point(x, y);
                                c.Show();
                            }
                        }
                        else if (url.Contains("torrentgalaxy.to"))
                        {
                            string magnetLink = magnetLinksGalaxy.ContainsKey(name) ? magnetLinksGalaxy[name] : "Magnet link not found";
                            string pattern = @"^magnet:\?xt=urn:[a-zA-Z0-9]+:[a-zA-Z0-9]{32,40}(&.*)?$";
                            Regex regex = new Regex(pattern);
                            if (regex.IsMatch(magnetLink))
                            {
                                success = true;
                            }
                            else
                            {
                                throw new Exception("Magnet link node not found");
                            }

                            if (success)
                            {
                                Data c = new Data();
                                Data.TorrentName = name;
                                Data.TorrentSize = size;
                                Data.Seeders = seeders;
                                Data.Leechers = leechers;
                                Data.Url = url;
                                Data.Magnet = magnetLink;
                                Data.Description = "No Description Available";

                                c.StartPosition = FormStartPosition.Manual;
                                int x = this.Location.X + (this.Width - c.Width) / 2;
                                int y = this.Location.Y + (this.Height - c.Height) / 2;
                                c.Location = new Point(x, y);
                                c.Show();
                            }
                        }
                        else if (url.Contains("bitsearch.to"))
                        {
                            var xToWeb = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument xToDoc = xToWeb.Load(url);

                            var magnetLinkNode = xToDoc.DocumentNode.SelectSingleNode("//*[@id=\"alart-box\"]/div[1]/div[1]/div[3]/a[2]");
                            string magnetLink = magnetLinkNode != null ? magnetLinkNode.GetAttributeValue("href", "") : "Magnet link not found";
                            magnetLink = magnetLink.Replace("&#x3D;", "=");

                            var descriptionNode = xToDoc.DocumentNode.SelectSingleNode("//*[@id='description']");
                            string description = descriptionNode != null ? descriptionNode.InnerHtml : "Description not found";

                            string pattern = @"^magnet:\?xt=urn:[a-zA-Z0-9]+:[a-zA-Z0-9]{32,40}(&.*)?$";
                            Regex regex = new Regex(pattern);
                            if (regex.IsMatch(magnetLink))
                            {
                                success = true;
                            }
                            else
                            {
                                throw new Exception("Magnet link node not found");
                            }

                            if (success)
                            {
                                Data c = new Data();
                                Data.TorrentName = name;
                                Data.TorrentSize = size;
                                Data.Seeders = seeders;
                                Data.Leechers = leechers;
                                Data.Url = url;
                                Data.Magnet = magnetLink;
                                Data.Description = description;

                                c.StartPosition = FormStartPosition.Manual;
                                int x = this.Location.X + (this.Width - c.Width) / 2;
                                int y = this.Location.Y + (this.Height - c.Height) / 2;
                                c.Location = new Point(x, y);
                                c.Show();
                            }
                        }
                        else if (url.Contains("therarbg.com"))
                        {
                            var xToWeb = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument xToDoc = xToWeb.Load(url);

                            var magnetLinkNode = xToDoc.DocumentNode.SelectSingleNode("//table[@class='detailTable']//a[contains(@href, 'magnet:')]");
                            string magnetLink = magnetLinkNode != null ? magnetLinkNode.GetAttributeValue("href", "") : "Magnet link not found";

                            var descriptionNode = xToDoc.DocumentNode.SelectSingleNode("//*[@id='description']");
                            string description = descriptionNode != null ? descriptionNode.InnerHtml : "Description not found";

                            string pattern = @"^magnet:\?xt=urn:[a-zA-Z0-9]+:[a-zA-Z0-9]{32,40}(&.*)?$";
                            Regex regex = new Regex(pattern);
                            if (regex.IsMatch(magnetLink))
                            {
                                success = true;
                            }
                            else
                            {
                                throw new Exception("Magnet link node not found");
                            }

                            if (success)
                            {
                                Data c = new Data();
                                Data.TorrentName = name;
                                Data.TorrentSize = size;
                                Data.Seeders = seeders;
                                Data.Leechers = leechers;
                                Data.Url = url;
                                Data.Magnet = magnetLink;
                                Data.Description = description;

                                c.StartPosition = FormStartPosition.Manual;
                                int x = this.Location.X + (this.Width - c.Width) / 2;
                                int y = this.Location.Y + (this.Height - c.Height) / 2;
                                c.Location = new Point(x, y);
                                c.Show();
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        retries++;
                        this.Text = "Torrent Searcher - Attempt " + retries + " failed" + " | " + dataGridView_Torrents.Rows.Count + " Torrents Found";

                        if (retries < maxRetries)
                        {
                            // 1.5x backoff delay
                            // int delay = (int)(2000 * Math.Pow(1.5, retries - 1));
                            int delay = (int)Math.Pow(1.5, retries) * 1000;
                            await Task.Delay(delay);
                        }
                        else
                        {
                            this.Text = formTitle + " | " + dataGridView_Torrents.Rows.Count + " Torrents Found";
                        }
                    }
                }

                this.Text = formTitle + " | " + dataGridView_Torrents.Rows.Count + " Torrents Found";
            }
        }
        
        private void button_Search_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker_Search.IsBusy)
            {
                timer_Title.Start();
                backgroundWorker_Search.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("BackgroundWorker is already running.");
            }
        }

        private static Settings settingsForm;
        private void button_Settings_Click(object sender, EventArgs e)
        {
            if (settingsForm == null || settingsForm.IsDisposed)
            {
                settingsForm = new Settings();
                settingsForm.StartPosition = FormStartPosition.Manual;

                // Calculate the center position
                int x = this.Location.X + (this.Width - settingsForm.Width) / 2;
                int y = this.Location.Y + (this.Height - settingsForm.Height) / 2;
                settingsForm.Location = new Point(x, y);

                settingsForm.Show();
            }
            else
            {
                settingsForm.BringToFront();
            }
        }

        private void textBox_Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button_Search_Click(this, new EventArgs());
                e.SuppressKeyPress = true;
            }
        }

        private void label_Saved_Click(object sender, EventArgs e)
        {
            if (saved == null || saved.IsDisposed)
            {
                saved = new SavedTorrents();
                saved.StartPosition = FormStartPosition.Manual;

                // Calculate the center position
                int x = this.Location.X + (this.Width - saved.Width) / 2;
                int y = this.Location.Y + (this.Height - saved.Height) / 2;
                saved.Location = new Point(x, y);

                saved.Show();
            }
            else
            {
                saved.BringToFront();
            }
        }

        #endregion Controls

        #region Searchers

        private void backgroundWorker_Search_DoWork(object sender, DoWorkEventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_Search.Text))
            {
                MessageBox.Show("Text Box Cannot Be Empty");
                return;
            }
            else
            {
                dataGridView_Torrents.Invoke(new MethodInvoker(delegate { dataGridView_Torrents.Visible = true; }));
                textBox_Filter.Invoke(new MethodInvoker(delegate { textBox_Filter.Text = ""; }));
                numericUpDown_Seeders.Invoke(new MethodInvoker(delegate { numericUpDown_Seeders.Value = 0; }));
                formTitle = "Torrent Searcher - Searching";
                this.Invoke(new MethodInvoker(delegate { this.Text = formTitle; }));
            }

            dataGridView_Torrents.Invoke(new MethodInvoker(delegate { dataGridView_Torrents.Rows.Clear(); }));

            string searchText = textBox_Search.Text;
            string contentItem = "";
            string sortByItem = "";

            comboBox_Content.Invoke(new MethodInvoker(delegate
            {
                contentItem = comboBox_Content.SelectedIndex != 0 ? comboBox_Content.SelectedItem.ToString() : "";
            }));

            comboBox_SortBy.Invoke(new MethodInvoker(delegate
            {
                sortByItem = comboBox_SortBy.SelectedIndex != 0 ? comboBox_SortBy.SelectedItem.ToString() : "";
            }));

            #region checkSettings

            int websiteSearches = 1;
            bool search1337x = false;
            bool searchLimeTorrents = false;
            bool searchNyaa = false;
            bool searchPiratebay = false;
            bool searchTorlock2 = false;
            bool searchTorrentProject = false;
            bool searchTorrentsCSV = false;
            bool searchTorrentDownload = false;
            bool searchYourBittorrent = false;
            bool searchTorrentGalaxy = false;
            bool searchBitSearch = false;
            bool searchTheRarbg = false;

            if (File.Exists("Settings.txt"))
            {
                string[] lines = File.ReadAllLines("Settings.txt");
                websiteSearches = Convert.ToInt32(lines[1].Replace("Website Searches: ", ""));

                // Start reading from the third line for CheckedListBox items
                for (int i = 3; i < lines.Length; i++)
                {
                    string[] parts = lines[i].Split(':');
                    if (parts.Length == 2)
                    {
                        string itemText = parts[0].Trim();
                        bool isChecked = Convert.ToBoolean(parts[1].Trim());

                        switch (itemText)
                        {
                            case "1337x":
                                search1337x = isChecked;
                                break;
                            case "LimeTorrents":
                                searchLimeTorrents = isChecked;
                                break;
                            case "Nyaa":
                                searchNyaa = isChecked;
                                break;
                            case "Piratebay":
                                searchPiratebay = isChecked;
                                break;
                            case "Torlock2":
                                searchTorlock2 = isChecked;
                                break;
                            case "TorrentProject":
                                searchTorrentProject = isChecked;
                                break;
                            case "Torrents-CSV":
                                searchTorrentsCSV = isChecked;
                                break;
                            case "TorrentDownload":
                                searchTorrentDownload = isChecked;
                                break;
                            case "YourBittorrent":
                                searchYourBittorrent = isChecked;
                                break;
                            case "TorrentGalaxy":
                                searchTorrentGalaxy = isChecked;
                                break;
                            case "BitSearch":
                                searchBitSearch = isChecked;
                                break;
                            case "TheRarbg":
                                searchTheRarbg = isChecked;
                                break;
                        }
                    }
                }
            }

            if (!search1337x && !searchLimeTorrents && !searchNyaa && !searchPiratebay && !searchTorlock2 && !searchTorrentProject
                && !searchTorrentsCSV && !searchTorrentDownload && !searchYourBittorrent && !searchTorrentGalaxy && !searchBitSearch && !searchTheRarbg)
            {
                MessageBox.Show("No search provider selected. Please enable at least one search provider in the settings.");
                return;
            }

            var tasks = new List<Task>();

            if (search1337x)
            {
                tasks.Add(Scraper1337x.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView));
            }
            if (searchNyaa)
            {
                tasks.Add(ScraperNyaasi.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView));
            }
            if (searchLimeTorrents)
            {
                tasks.Add(ScraperLimeTorrents.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView));
            }
            if (searchTorrentsCSV)
            {
                tasks.Add(ScraperTorrentsCSV.ScrapeTorrentsAsync(searchText, 100, UpdateDataGridView));
            }
            if (searchTorrentProject)
            {
                tasks.Add(ScraperTorrentProject.ScrapeTorrentsAsync(searchText, websiteSearches, UpdateDataGridView));
            }
            if (searchPiratebay)
            {
                tasks.Add(ScraperPirateBay.ScrapeTorrentsAsync(searchText, 100, contentItem, UpdateDataGridView));
            }
            if (searchTorlock2)
            {
                tasks.Add(ScraperTorlock.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView));
            }
            if (searchTorrentDownload)
            {
                tasks.Add(ScraperTorrentDownload.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView));
            }
            if (searchYourBittorrent)
            {
                tasks.Add(ScraperYourBittorent.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView));
            }
            if (searchTorrentGalaxy)
            {
                tasks.Add(ScraperTorrentGalaxy.ScrapeTorrentsAsync(searchText, UpdateDataGridView));
            }
            if (searchBitSearch)
            {
                tasks.Add(ScraperBitSearch.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView));
            }
            if (searchTheRarbg)
            {
                tasks.Add(ScraperTheRarbg.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView));
            }

            #endregion checkSettings

            // Wait for all tasks to complete
            Task.WhenAll(tasks).Wait();

            // Code to execute after all tasks have completed
            formTitle = "Torrent Searcher";
            //this.Invoke(new MethodInvoker(delegate { this.Text = formTitle; }));

            int colw = dataGridView_Torrents.Columns[0].Width;
            dataGridView_Torrents.Invoke(new MethodInvoker(delegate { dataGridView_Torrents.Columns[0].MinimumWidth = 60; }));
            dataGridView_Torrents.Invoke(new MethodInvoker(delegate { dataGridView_Torrents.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None; }));
            dataGridView_Torrents.Invoke(new MethodInvoker(delegate { dataGridView_Torrents.Columns[0].Width = colw; }));
        }

        private void UpdateDataGridView(TorrentInfo torrent)
        {
            dataGridView_Torrents.Invoke(new MethodInvoker(delegate
            {
                dataGridView_Torrents.Rows.Add(torrent.Name, torrent.Size, torrent.Seeders, torrent.Leechers, torrent.Url);
            }));
        }

        private void backgroundWorker_Search_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            timer_Title.Stop();
            this.Text = formTitle + " | " + dataGridView_Torrents.Rows.Count + " Torrents Found";
        }

        private void timer_Title_Tick(object sender, EventArgs e)
        {
            this.Text = formTitle + " | " + dataGridView_Torrents.Rows.Count + " Torrents Found";
        }

        #endregion Searchers

        #region SizeSort

        private SortOrder sizeColumnSortOrder = SortOrder.None;

        private void dataGridView_Torrents_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == 1) // Size column
            {
                // Toggle sorting order
                if (sizeColumnSortOrder == SortOrder.None || sizeColumnSortOrder == SortOrder.Ascending)
                {
                    dataGridView_Torrents.Sort(new SizeComparer(SortOrder.Descending));
                    sizeColumnSortOrder = SortOrder.Descending;
                }
                else
                {
                    dataGridView_Torrents.Sort(new SizeComparer(SortOrder.Ascending));
                    sizeColumnSortOrder = SortOrder.Ascending;
                }

                FilterDataGridView();
            }
        }

        private void textBox_Filter_TextChanged(object sender, EventArgs e)
        {
            FilterDataGridView();
        }

        private void numericUpDown_Seeders_ValueChanged(object sender, EventArgs e)
        {
            FilterDataGridView();
        }

        private void NumericUpDownTextBox_TextChanged(object sender, EventArgs e)
        {
            // Try to parse the text entered by the user
            if (int.TryParse(((System.Windows.Forms.TextBox)sender).Text, out int newValue))
            {
                // Temporarily unsubscribe from ValueChanged to avoid a loop
                numericUpDown_Seeders.ValueChanged -= numericUpDown_Seeders_ValueChanged;

                try
                {
                    // Update the NumericUpDown value
                    numericUpDown_Seeders.Value = newValue;
                }
                catch
                { }

                // Re-subscribe to ValueChanged
                numericUpDown_Seeders.ValueChanged += numericUpDown_Seeders_ValueChanged;

                // Update the DataGridView
                FilterDataGridView();
            }
        }

        private void FilterDataGridView()
        {
            string[] filterParts = textBox_Filter.Text.ToLower().Split(new string[] { ", " }, StringSplitOptions.None);
            int minSeeders = (int)numericUpDown_Seeders.Value;

            foreach (DataGridViewRow row in dataGridView_Torrents.Rows)
            {
                bool nameMatches = row.Cells[0].Value != null && filterParts.All(part => row.Cells[0].Value.ToString().ToLower().Contains(part));
                bool seedersMatch = row.Cells[2].Value != null && int.TryParse(row.Cells[2].Value.ToString(), out int seeders) && seeders >= minSeeders;

                row.Visible = nameMatches && seedersMatch;
            }
        }

        public class SizeComparer : IComparer
        {
            private SortOrder sortOrder;

            public SizeComparer(SortOrder sortOrder)
            {
                this.sortOrder = sortOrder;
            }

            public int Compare(object x, object y)
            {
                DataGridViewRow row1 = x as DataGridViewRow;
                DataGridViewRow row2 = y as DataGridViewRow;

                long size1 = Scraper1337x.ConvertToBytes(row1.Cells[1].Value.ToString());
                long size2 = Scraper1337x.ConvertToBytes(row2.Cells[1].Value.ToString());

                int result = size1.CompareTo(size2);

                return sortOrder == SortOrder.Ascending ? result : -result;
            }
        }

        #endregion SizeSort

        private static SavedTorrents saved;
    }
}
