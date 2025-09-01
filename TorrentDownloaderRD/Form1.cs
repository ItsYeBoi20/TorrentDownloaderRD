using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MediaDownloader.Scrapers;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Collections;
using System.Threading;
using RealDebridAPI;
using System.Diagnostics;
using System.Net;
using System.Globalization;
using TorrentDownloaderRD.Processing;
using static RealDebridAPI.RealDebridClient;
using static TorrentDownloaderRD.Processing.AllDebridClient;
using System.Runtime.InteropServices.ComTypes;
using TorrentDownloaderRD;

namespace MediaDownloader
{
    public partial class Main : Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
        private const int IDC_HAND = 32649;

        private CancellationTokenSource _cancellationTokenSource;
        private System.Timers.Timer _timer;

        public string formTitle = "Torrent Searcher";
        public bool isFinished = false;
        private bool _isDownloadRunning = false;
        private bool _detailedView = true;
        private bool _isOperationRunning = false;
        private bool _isExtended = false;
        private bool _cancellationToken = false;
        private RealDebridClient _realDebridClient;
        public static Dictionary<string, string> magnetLinksCSV = new Dictionary<string, string>(); //torrents-csv magnet links
        public static Dictionary<string, string> magnetLinksPirate = new Dictionary<string, string>(); //piratebay magnet links
        public static Dictionary<string, string> magnetLinksGalaxy = new Dictionary<string, string>(); //torrentgalaxy magnet links
        public static Dictionary<string, string> magnetLinksFitGirl = new Dictionary<string, string>(); //fitgirl magnet links
        public static Dictionary<string, string> magnetLinksEmpress = new Dictionary<string, string>(); //empress magnet links
        public static Dictionary<string, string> magnetLinksDodi = new Dictionary<string, string>(); //dodi magnet links
        public static Dictionary<string, string> magnetLinksGOG = new Dictionary<string, string>(); //gog magnet links
        public static Dictionary<string, string> magnetLinksOnlineFix = new Dictionary<string, string>(); //onlinefix magnet 
        public static Dictionary<string, string> magnetLinksTinyRepacks = new Dictionary<string, string>(); //tinyrepacks magnet links
        public static Dictionary<string, string> magnetLinksXatab = new Dictionary<string, string>(); //xatab magnet links

        public Main()
        {
            InitializeComponent();
            label_Saved.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_Close.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
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
                string description = "Description not found";
                string magnetLink = "Magnet link not found";

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
                            magnetLink = magnetLinkNode != null ? magnetLinkNode.GetAttributeValue("href", "") : "Magnet link not found";

                            var descriptionNode = xToDoc.DocumentNode.SelectSingleNode("//*[@id='description']");
                            description = descriptionNode != null ? descriptionNode.InnerHtml : "Description not found";

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
                        }
                        else if (url.Contains("nyaa.si"))
                        {
                            var nyaaWeb = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument nyaaDoc = nyaaWeb.Load(url);

                            var magnetLinkNode = nyaaDoc.DocumentNode.SelectSingleNode("/html/body/div/div[1]/div[3]/a[2]");
                            magnetLink = magnetLinkNode != null ? magnetLinkNode.GetAttributeValue("href", "") : "Magnet link not found";

                            var descriptionNode = nyaaDoc.DocumentNode.SelectSingleNode("/html/body/div/div[2]/div");
                            description = descriptionNode != null ? descriptionNode.InnerHtml : "Description not found";

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
                        }
                        else if (url.Contains("limetorrents.lol"))
                        {
                            var limeWeb = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument limeDoc = limeWeb.Load(url);

                            var magnetLinkNode = limeDoc.DocumentNode.SelectSingleNode("//a[contains(@href, 'magnet:')]");
                            magnetLink = magnetLinkNode != null ? magnetLinkNode.GetAttributeValue("href", "") : "Magnet link not found";

                            var descriptionNode = limeDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'description')]");
                            description = descriptionNode != null ? descriptionNode.InnerHtml : "Description not found";

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
                        }
                        else if (url.Contains("torrents-csv"))
                        {
                            magnetLink = magnetLinksCSV.ContainsKey(name) ? magnetLinksCSV[name] : "Magnet link not found";

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
                        }
                        else if (url.Contains("torrentproject.cc"))
                        {
                            var tproject = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument tprojectDoc = tproject.Load(url);

                            var scraperTorrentProject = new ScraperTorrentProject();
                            magnetLink = await scraperTorrentProject.DownloadTorrentAsync(url);

                            var descriptionNode = tprojectDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'description')]");
                            description = descriptionNode != null ? descriptionNode.InnerHtml : "Description not found";

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
                        }
                        else if (url.Contains("thepiratebay.org"))
                        {
                            var pirateWeb = new HtmlAgilityPack.HtmlWeb();
                            magnetLink = magnetLinksPirate.ContainsKey(name) ? magnetLinksPirate[name] : "Magnet link not found";

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
                        }
                        else if (url.Contains("torlock2.com"))
                        {
                            var tproject = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument tprojectDoc = tproject.Load(url);

                            var magnetLinkNode = tprojectDoc.DocumentNode.SelectSingleNode("/html/body/article/div[2]/div/div[2]/dl[3]/dd");
                            magnetLink = "magnet:?xt=urn:btih:" + magnetLinkNode.InnerHtml;

                            var descriptionNode = tprojectDoc.DocumentNode.SelectSingleNode("//*[@id=\"description\"]/div[2]/blockquote");
                            description = descriptionNode != null ? descriptionNode.InnerHtml : "Description not found";

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
                        }
                        else if (url.Contains("yourbittorrent.com"))
                        {
                            var tproject = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument tprojectDoc = tproject.Load(url);

                            var magnetLinkNodes = tprojectDoc.DocumentNode.SelectNodes("/html/body/div/div[1]/div[2]/div/div[2]//kbd");

                            magnetLink = null;
                            string pattern = @"^magnet:\?xt=urn:[a-zA-Z0-9]+:[a-zA-Z0-9]{32,40}(&.*)?$";
                            Regex regex = new Regex(pattern);
                            if (magnetLinkNodes != null)
                            {
                                foreach (var node in magnetLinkNodes)
                                {
                                    string potentialMagnetLink = "magnet:?xt=urn:btih:" + node.InnerHtml;
                                    if (regex.IsMatch(potentialMagnetLink))
                                    {
                                        magnetLink = potentialMagnetLink;
                                        break;
                                    }
                                }
                            }

                            if (magnetLink != null)
                            {
                                success = true;
                            }
                            else
                            {
                                throw new Exception("Magnet link node not found");
                            }
                        }
                        else if (url.Contains("torrentdownload.info"))
                        {
                            var torrentWeb = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument torrentDoc = torrentWeb.Load(url);

                            var magnetLinkNode = torrentDoc.DocumentNode.SelectSingleNode("/html/body/div/table[2]/tbody/tr[3]/td[2]");
                            magnetLink = "magnet:?xt=urn:btih:" + magnetLinkNode.InnerHtml;

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
                        }
                        else if (url.Contains("torrentgalaxy.to"))
                        {
                            magnetLink = magnetLinksGalaxy.ContainsKey(name) ? magnetLinksGalaxy[name] : "Magnet link not found";
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
                        }
                        else if (url.Contains("bitsearch.to"))
                        {
                            var xToWeb = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument xToDoc = xToWeb.Load(url);

                            var magnetLinkNode = xToDoc.DocumentNode.SelectSingleNode("//*[@id=\"alart-box\"]/div[1]/div[1]/div[3]/a[2]");
                            magnetLink = magnetLinkNode != null ? magnetLinkNode.GetAttributeValue("href", "") : "Magnet link not found";
                            magnetLink = magnetLink.Replace("&#x3D;", "=");

                            var descriptionNode = xToDoc.DocumentNode.SelectSingleNode("//*[@id='description']");
                            description = descriptionNode != null ? descriptionNode.InnerHtml : "Description not found";

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
                        }
                        else if (url.Contains("therarbg.com"))
                        {
                            var xToWeb = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument xToDoc = xToWeb.Load(url);

                            var magnetLinkNode = xToDoc.DocumentNode.SelectSingleNode("//table[@class='detailTable']//a[contains(@href, 'magnet:')]");
                            magnetLink = magnetLinkNode != null ? magnetLinkNode.GetAttributeValue("href", "") : "Magnet link not found";

                            var descriptionNode = xToDoc.DocumentNode.SelectSingleNode("//*[@id='description']");
                            description = descriptionNode != null ? descriptionNode.InnerHtml : "Description not found";

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
                        }
                        else if (url.Contains("animetosho.org"))
                        {
                            var xToWeb = new HtmlAgilityPack.HtmlWeb();
                            HtmlAgilityPack.HtmlDocument xToDoc = xToWeb.Load(url);

                            var magnetLinkNode = xToDoc.DocumentNode.SelectSingleNode("//*[@id='content']/table[1]//a[contains(@href, 'magnet:')]");
                            magnetLink = magnetLinkNode != null ? magnetLinkNode.GetAttributeValue("href", "") : "Magnet link not found";


                            var splitUrl = Regex.Split(url, @"\.");
                            int splitUrlLength = splitUrl.Length;
                            if (splitUrl[splitUrlLength - 1].StartsWith("n"))
                            {
                                try
                                {
                                    var cacheToWeb = new HtmlAgilityPack.HtmlWeb();
                                    HtmlAgilityPack.HtmlDocument cacheToDoc = cacheToWeb.Load("https://cache.animetosho.org/nyaasi/view/" + splitUrl[splitUrlLength - 1].Replace("n", ""));

                                    var descriptionNode = cacheToDoc.DocumentNode.SelectSingleNode("//*[@id=\"entry_description\"]");
                                    description = descriptionNode != null ? descriptionNode.InnerHtml : "Description not found";
                                }
                                catch
                                {

                                }
                            }

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
                        }
                        else if (url.Contains("kickasstorrents.to"))
                        {
                            var xToWeb = new HtmlAgilityPack.HtmlWeb();
                            xToWeb.UserAgent = ScraperKickAssTorrents.ChromeUserAgent;

                            HtmlAgilityPack.HtmlDocument xToDoc = xToWeb.Load(url);

                            var magnetLinkNode = xToDoc.DocumentNode.SelectSingleNode("//a[@title='Magnet link']");
                            Console.WriteLine(magnetLinkNode != null ? magnetLinkNode.InnerHtml : "Magnet link node not found");
                            magnetLink = magnetLinkNode != null ? magnetLinkNode.GetAttributeValue("href", "") : "Magnet link not found";

                            var descriptionNode = xToDoc.DocumentNode.SelectSingleNode("//div[@id='desc']");
                            description = descriptionNode != null ? descriptionNode.InnerHtml : "Description not found";

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
                        }
                        else if (url.Equals("FitGirl"))
                        {
                            magnetLink = magnetLinksFitGirl.ContainsKey(name) ? magnetLinksFitGirl[name] : "Magnet link not found";
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
                        }
                        else if (url.Equals("Empress"))
                        {
                            magnetLink = magnetLinksEmpress.ContainsKey(name) ? magnetLinksEmpress[name] : "Magnet link not found";
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
                        }
                        else if (url.Equals("Dodi"))
                        {
                            magnetLink = magnetLinksDodi.ContainsKey(name) ? magnetLinksDodi[name] : "Magnet link not found";
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
                        }
                        else if (url.Equals("GOG"))
                        {
                            magnetLink = magnetLinksGOG.ContainsKey(name) ? magnetLinksGOG[name] : "Magnet link not found";
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
                        }
                        else if (url.Equals("OnlineFix"))
                        {
                            magnetLink = magnetLinksOnlineFix.ContainsKey(name) ? magnetLinksOnlineFix[name] : "Magnet link not found";
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
                        }
                        else if (url.Equals("TinyRepacks"))
                        {
                            magnetLink = magnetLinksTinyRepacks.ContainsKey(name) ? magnetLinksTinyRepacks[name] : "Magnet link not found";
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
                        }
                        else if (url.Equals("Xatab"))
                        {
                            magnetLink = magnetLinksXatab.ContainsKey(name) ? magnetLinksXatab[name] : "Magnet link not found";
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

                if (File.Exists("Settings.txt"))
                {
                    string[] lines = File.ReadAllLines("Settings.txt");
                    _detailedView = Convert.ToBoolean(lines[5].Replace("DetailedView: ", ""));
                }

                if (_detailedView == true)
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
                    Rectangle screenBounds = Screen.FromControl(this).WorkingArea;
                    if (x < screenBounds.Left)
                    {
                        x = screenBounds.Left;
                    }
                    else if (x + c.Width > screenBounds.Right)
                    {
                        x = screenBounds.Right - c.Width;
                    }
                    if (y < screenBounds.Top)
                    {
                        y = screenBounds.Top;
                    }
                    else if (y + c.Height > screenBounds.Bottom)
                    {
                        y = screenBounds.Bottom - c.Height;
                    }
                    c.Location = new Point(x, y);
                    c.Show();

                    this.Text = formTitle + " | " + dataGridView_Torrents.Rows.Count + " Torrents Found";
                }
                else if (_detailedView == false)
                {
                    if (File.Exists("Settings.txt"))
                    {
                        try
                        {
                            string[] lines = File.ReadAllLines("Settings.txt");
                            string GetAPIProvider = lines[0].Replace("API Provider: ", "");
                            string realDebridAPIKey = lines[1].Replace("Real-Debrid API Key: ", "");
                            string allDebridAPIKey = lines[2].Replace("AllDebrid API Key: ", "");
                            bool deleteWhenDone = Convert.ToBoolean(lines[4].Replace("DeleteAfter: ", ""));

                            if (GetAPIProvider == "Real-Debrid")
                            {
                                if (realDebridAPIKey == "")
                                {
                                    MessageBox.Show("Real Debrid API Key Not Found\nUsing Its Service Will Be Disabled");
                                }
                                else
                                {
                                    _realDebridClient = new RealDebridClient(realDebridAPIKey);

                                    if (_isExtended == false)
                                    {
                                        #region changeSize

                                        label_Close.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        lstDownloadLinks.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        progress_Label.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        speed_Label.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        button_Download.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        progressBar1.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        //
                                        textBox_Search.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        textBox_Filter.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        comboBox_Content.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        comboBox_SortBy.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        numericUpDown_Seeders.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        button_Search.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        button_Settings.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        label1.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        label_Content.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        label_SortBy.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        label_Seeders.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        label_Saved.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        dataGridView_Torrents.Anchor = AnchorStyles.Left | AnchorStyles.Top;

                                        this.Width = this.Width + 307;

                                        label_Close.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        lstDownloadLinks.Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                                        progress_Label.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                                        speed_Label.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                                        button_Download.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                                        progressBar1.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                                        //
                                        textBox_Search.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                                        textBox_Filter.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        comboBox_Content.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        comboBox_SortBy.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        numericUpDown_Seeders.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        button_Search.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        button_Settings.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        label1.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        label_Content.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        label_SortBy.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        label_Seeders.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        label_Saved.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        dataGridView_Torrents.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;

                                        #endregion changeSize

                                        lstDownloadLinks.Visible = true;
                                        progress_Label.Visible = true;
                                        speed_Label.Visible = true;
                                        progressBar1.Visible = true;

                                        _isExtended = true;
                                    }

                                    fetchLinks(magnetLink, deleteWhenDone);
                                }
                            }
                            else if (GetAPIProvider == "AllDebrid")
                            {
                                if (allDebridAPIKey == "")
                                {
                                    MessageBox.Show("AllDebrid API Key Not Found\nUsing Its Service Will Be Disabled");
                                }
                                else
                                {
                                    _allDebridClient = new AllDebridClient(allDebridAPIKey);

                                    if (_isExtended == false)
                                    {
                                        #region changeSize

                                        label_Close.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        lstDownloadLinks.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        progress_Label.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        speed_Label.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        button_Download.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        progressBar1.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        //
                                        textBox_Search.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        textBox_Filter.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        comboBox_Content.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        comboBox_SortBy.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        numericUpDown_Seeders.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        button_Search.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        button_Settings.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        label1.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        label_Content.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        label_SortBy.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        label_Seeders.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        label_Saved.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                        dataGridView_Torrents.Anchor = AnchorStyles.Left | AnchorStyles.Top;

                                        this.Width = this.Width + 307;

                                        label_Close.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        lstDownloadLinks.Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                                        progress_Label.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                                        speed_Label.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                                        button_Download.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                                        progressBar1.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                                        //
                                        textBox_Search.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                                        textBox_Filter.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        comboBox_Content.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        comboBox_SortBy.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        numericUpDown_Seeders.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        button_Search.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        button_Settings.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        label1.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        label_Content.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        label_SortBy.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        label_Seeders.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        label_Saved.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                        dataGridView_Torrents.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;

                                        #endregion changeSize

                                        lstDownloadLinks.Visible = true;
                                        progress_Label.Visible = true;
                                        speed_Label.Visible = true;
                                        progressBar1.Visible = true;

                                        _isExtended = true;
                                    }

                                    fetchLinksAllDebrid(magnetLink, deleteWhenDone);
                                }
                            }
                            
                        }
                        catch
                        {
                            MessageBox.Show("Real Debrid API Key Not Found\nUsing Its Service Will Be Disabled");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Real Debrid API Key Not Found\nUsing Its Service Will Be Disabled");
                    }
                }
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

                int x = this.Location.X + (this.Width - settingsForm.Width) / 2;
                int y = this.Location.Y + (this.Height - settingsForm.Height) / 2;
                Rectangle screenBounds = Screen.FromControl(this).WorkingArea;
                if (x < screenBounds.Left)
                {
                    x = screenBounds.Left;
                }
                else if (x + settingsForm.Width > screenBounds.Right)
                {
                    x = screenBounds.Right - settingsForm.Width;
                }
                if (y < screenBounds.Top)
                {
                    y = screenBounds.Top;
                }
                else if (y + settingsForm.Height > screenBounds.Bottom)
                {
                    y = screenBounds.Bottom - settingsForm.Height;
                }
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

                int x = this.Location.X + (this.Width - saved.Width) / 2;
                int y = this.Location.Y + (this.Height - saved.Height) / 2;
                Rectangle screenBounds = Screen.FromControl(this).WorkingArea;
                if (x < screenBounds.Left)
                {
                    x = screenBounds.Left;
                }
                else if (x + saved.Width > screenBounds.Right)
                {
                    x = screenBounds.Right - saved.Width;
                }
                if (y < screenBounds.Top)
                {
                    y = screenBounds.Top;
                }
                else if (y + saved.Height > screenBounds.Bottom)
                {
                    y = screenBounds.Bottom - saved.Height;
                }
                saved.Location = new Point(x, y);
                saved.Show();
            }
            else
            {
                saved.BringToFront();
            }
        }

        private async void button_Download_Click(object sender, EventArgs e)
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
                    MessageBox.Show("Download is Starting");
                    string downloadDirectory = folderBrowserDialog.SelectedPath;
                    await DownloadFilesAsync(downloadDirectory);
                }
            }

            button_Download.Text = "Download";
            _cancellationToken = false;
            _isDownloadRunning = false;
        }

        private void label_Close_Click(object sender, EventArgs e)
        {
            label_Close.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            lstDownloadLinks.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            progress_Label.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            speed_Label.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            button_Download.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            progressBar1.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            //
            textBox_Search.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            textBox_Filter.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            comboBox_Content.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            comboBox_SortBy.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            numericUpDown_Seeders.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            button_Search.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            button_Settings.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            label1.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            label_Content.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            label_SortBy.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            label_Seeders.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            label_Saved.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            dataGridView_Torrents.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            this.Width = this.Width - 307;

            label_Close.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            lstDownloadLinks.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            progress_Label.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            speed_Label.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            button_Download.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            progressBar1.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            //
            textBox_Search.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            textBox_Filter.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            comboBox_Content.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            comboBox_SortBy.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            numericUpDown_Seeders.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            button_Search.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            button_Settings.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            label1.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            label_Content.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            label_SortBy.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            label_Seeders.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            label_Saved.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            dataGridView_Torrents.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;

            lstDownloadLinks.Visible = false;
            progress_Label.Visible = false;
            speed_Label.Visible = false;
            progressBar1.Visible = false;

            _isExtended = false;
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
                    progress_Label.Text = $"Total Links: {lstDownloadLinks.Items.Count}, Total Size: {FormatFileSize(totalSizeInBytes)}";
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
            bool searchFitGirl = false;
            bool searchEmpress = false;
            bool searchDodi = false;
            bool searchGOG = false;
            bool searchOnlineFix = false;
            bool searchTinyRepacks = false;
            bool searchXatab = false;
            bool searchAnimeTosho = false;
            bool searchKickAssTorrents = false;

            if (File.Exists("Settings.txt"))
            {
                string[] lines = File.ReadAllLines("Settings.txt");
                websiteSearches = Convert.ToInt32(lines[3].Replace("Website Searches: ", ""));

                // Start reading from the third line for CheckedListBox items
                for (int i = 6; i < lines.Length; i++)
                {
                    string[] parts = lines[i].Split(':');
                    if (parts.Length == 2)
                    {
                        string itemText = parts[0].Trim();
                        bool isChecked = Convert.ToBoolean(parts[1].Trim());

                        switch (itemText)
                        {
                            case "1337x": search1337x = isChecked; break;
                            case "LimeTorrents": searchLimeTorrents = isChecked; break;
                            case "Nyaa": searchNyaa = isChecked; break;
                            case "Piratebay": searchPiratebay = isChecked; break;
                            case "Torlock2": searchTorlock2 = isChecked; break;
                            case "TorrentProject": searchTorrentProject = isChecked; break;
                            case "Torrents-CSV": searchTorrentsCSV = isChecked; break;
                            case "TorrentDownload": searchTorrentDownload = isChecked; break;
                            case "YourBittorrent": searchYourBittorrent = isChecked; break;
                            case "TorrentGalaxy": searchTorrentGalaxy = isChecked; break;
                            case "BitSearch": searchBitSearch = isChecked; break;
                            case "TheRarbg": searchTheRarbg = isChecked; break;
                            case "FitGirl": searchFitGirl = isChecked; break;
                            case "Empress": searchEmpress = isChecked; break;
                            case "Dodi": searchDodi = isChecked; break;
                            case "GOG": searchGOG = isChecked; break;
                            case "OnlineFix": searchOnlineFix = isChecked; break;
                            case "TinyRepacks": searchTinyRepacks = isChecked; break;
                            case "Xatab": searchXatab = isChecked; break;
                            case "AnimeTosho": searchAnimeTosho = isChecked; break;
                            case "KickAssTorrents": searchKickAssTorrents = isChecked; break;
                        }
                    }
                }
            }

            if (!search1337x && !searchLimeTorrents && !searchNyaa && !searchAnimeTosho && !searchPiratebay && !searchTorlock2 && !searchTorrentProject && 
                !searchTorrentsCSV && !searchTorrentDownload && !searchYourBittorrent && !searchTorrentGalaxy && !searchBitSearch && !searchTheRarbg && !searchKickAssTorrents && 
                !searchFitGirl && !searchEmpress && !searchDodi && !searchGOG && !searchOnlineFix && !searchTinyRepacks && !searchXatab)
            {
                MessageBox.Show("No search provider selected. Please enable at least one search provider in the settings.");
            }

            // Timer which checks if new items have been added
            _cancellationTokenSource = new CancellationTokenSource();
            _timer = new System.Timers.Timer(15000); // in MILLISECONDS
            _timer.Elapsed += TimerElapsed;
            _timer.AutoReset = false;
            _timer.Start();

            int timeoutDelay = 10; // in SECONDS

            var tasks = new List<Task>();

            try
            {

                if (search1337x)
                {
                    tasks.Add(Scraper1337x.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchNyaa)
                {
                    tasks.Add(ScraperNyaasi.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchLimeTorrents)
                {
                    tasks.Add(ScraperLimeTorrents.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchTorrentsCSV)
                {
                    tasks.Add(ScraperTorrentsCSV.ScrapeTorrentsAsync(searchText, 100, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchTorrentProject)
                {
                    tasks.Add(ScraperTorrentProject.ScrapeTorrentsAsync(searchText, websiteSearches, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchPiratebay)
                {
                    tasks.Add(ScraperPirateBay.ScrapeTorrentsAsync(searchText, 100, contentItem, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchTorlock2)
                {
                    tasks.Add(ScraperTorlock.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchTorrentDownload)
                {
                    tasks.Add(ScraperTorrentDownload.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchYourBittorrent)
                {
                    tasks.Add(ScraperYourBittorent.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchTorrentGalaxy)
                {
                    tasks.Add(ScraperTorrentGalaxy.ScrapeTorrentsAsync(searchText, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchBitSearch)
                {
                    tasks.Add(ScraperBitSearch.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchTheRarbg)
                {
                    tasks.Add(ScraperTheRarbg.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchFitGirl)
                {
                    tasks.Add(ScraperFitGirl.ScrapeTorrentsAsync(searchText, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchEmpress)
                {
                    tasks.Add(ScraperEmpress.ScrapeTorrentsAsync(searchText, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchDodi)
                {
                    tasks.Add(ScraperDodi.ScrapeTorrentsAsync(searchText, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchGOG)
                {
                    tasks.Add(ScraperGOG.ScrapeTorrentsAsync(searchText, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchOnlineFix)
                {
                    tasks.Add(ScraperOnlineFix.ScrapeTorrentsAsync(searchText, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchTinyRepacks)
                {
                    tasks.Add(ScraperTinyRepacks.ScrapeTorrentsAsync(searchText, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchXatab)
                {
                    tasks.Add(ScraperXatab.ScrapeTorrentsAsync(searchText, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchAnimeTosho)
                {
                    tasks.Add(ScraperAnimeTosho.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }
                if (searchKickAssTorrents)
                {
                    tasks.Add(ScraperKickAssTorrents.ScrapeTorrentsAsync(searchText, contentItem, sortByItem, websiteSearches, UpdateDataGridView, timeoutDelay, _cancellationTokenSource.Token));
                }

                #endregion checkSettings

            // Wait for all tasks to complete
            Task.WhenAll(tasks).Wait(); // HERE!!!!!!!!!!!
            }
            catch (OperationCanceledException)
            {
                // The task was canceled, but we still want to ensure final code runs.
            }
            finally
            {
                // Code to execute after task completion or cancellation
                formTitle = "Torrent Searcher";
                this.Invoke(new MethodInvoker(delegate { this.Text = formTitle; }));

                int colw = dataGridView_Torrents.Columns[0].Width;
                dataGridView_Torrents.Invoke(new MethodInvoker(delegate
                {
                    dataGridView_Torrents.Columns[0].MinimumWidth = 60;
                    dataGridView_Torrents.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dataGridView_Torrents.Columns[0].Width = colw;
                }));
            }
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _cancellationTokenSource.Cancel();
            _timer.Stop();
        }

        private void UpdateDataGridView(TorrentInfo torrent)
        {
            dataGridView_Torrents.Invoke(new MethodInvoker(delegate
            {
                dataGridView_Torrents.Rows.Add(torrent.Name, torrent.Size, torrent.Seeders, torrent.Leechers, torrent.Url);
                _timer.Stop();
                _timer.Start();
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

        #region downloadMethod

        public async Task DownloadFilesAsync(string downloadDirectory)
        {
            // Stores all items in the ListBox
            List<string> links = new List<string>();

            if (lstDownloadLinks.Items.Count > 0)
            {
                foreach (var item in lstDownloadLinks.Items)
                {
                    if (item.ToString().Contains("https://real-debrid.com/d/") || item.ToString().Contains("https://alldebrid.com/f/"))
                    {
                        links.Add(item.ToString());
                    }
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
                            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
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
                            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
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

        #endregion downloadMethod

        #region fetchLinksAllDebrid

        private async void fetchLinksAllDebrid(string magnetLink, bool deleteWhenDone)
        {
            // Check if the operation is already running
            if (_isOperationRunning)
            {
                MessageBox.Show("Operation is already in progress.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _isOperationRunning = true;

            bool cancelled = false;

            if (string.IsNullOrWhiteSpace(magnetLink))
            {
                MessageBox.Show("Please enter a valid magnet link.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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
                                bool itemExists = false;
                                for (int i = 0; i < lstDownloadLinks.Items.Count; i++)
                                {
                                    if (lstDownloadLinks.Items[i].ToString().Contains(magnet.Filename))
                                    {
                                        itemExists = true;
                                        lstDownloadLinks.Items[i] = $"[In Queue] {magnet.Filename}";
                                    }
                                }

                                if (!itemExists)
                                {
                                    lstDownloadLinks.Items.Add($"[In Queue] {magnet.Filename}");
                                }
                            }
                            else if (magnet.StatusCode == 1)
                            {
                                if (magnet.Downloaded.HasValue && magnet.Size != 0)
                                {
                                    double downloaded = magnet.Downloaded.Value;
                                    double size = magnet.Size;
                                    double progressPercentage = (downloaded / size) * 100;

                                    bool itemExists = false;
                                    for (int i = 0; i < lstDownloadLinks.Items.Count; i++)
                                    {
                                        if (lstDownloadLinks.Items[i].ToString().Contains(magnet.Filename))
                                        {
                                            itemExists = true;
                                            lstDownloadLinks.Items[i] = $"[{progressPercentage.ToString("0.0", CultureInfo.InvariantCulture)}%] {magnet.Filename}";
                                        }
                                    }

                                    if (!itemExists)
                                    {
                                        lstDownloadLinks.Items.Add($"[{progressPercentage.ToString("0.0", CultureInfo.InvariantCulture)}%] {magnet.Filename}");
                                    }
                                }
                                else
                                {
                                    bool itemExists = false;
                                    for (int i = 0; i < lstDownloadLinks.Items.Count; i++)
                                    {
                                        if (lstDownloadLinks.Items[i].ToString().Contains(magnet.Filename))
                                        {
                                            itemExists = true;
                                            lstDownloadLinks.Items[i] = $"[Downloading] {magnet.Filename}";
                                        }
                                    }

                                    if (!itemExists)
                                    {
                                        lstDownloadLinks.Items.Add($"[Downloading] {magnet.Filename}");
                                    }
                                }
                            }
                            else if (magnet.StatusCode == 2)
                            {
                                bool itemExists = false;
                                for (int i = 0; i < lstDownloadLinks.Items.Count; i++)
                                {
                                    if (lstDownloadLinks.Items[i].ToString().Contains(magnet.Filename))
                                    {
                                        itemExists = true;
                                        lstDownloadLinks.Items[i] = $"[Compressing] {magnet.Filename}";
                                    }
                                }

                                if (!itemExists)
                                {
                                    lstDownloadLinks.Items.Add($"[Compressing] {magnet.Filename}");
                                }
                            }
                            else if (magnet.StatusCode == 3)
                            {
                                bool itemExists = false;
                                for (int i = 0; i < lstDownloadLinks.Items.Count; i++)
                                {
                                    if (lstDownloadLinks.Items[i].ToString().Contains(magnet.Filename))
                                    {
                                        itemExists = true;
                                        lstDownloadLinks.Items[i] = $"[Uploading] {magnet.Filename}";
                                    }
                                }

                                if (!itemExists)
                                {
                                    lstDownloadLinks.Items.Add($"[Uploading] {magnet.Filename}");
                                }
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

                        int lstCount = 0;
                        lstDownloadLinks.Invoke(new MethodInvoker(delegate { lstCount = lstDownloadLinks.Items.Count; }));

                        try
                        {
                            for (int i = 0; i < lstCount; i++)
                            {
                                string lstItem = "";
                                lstDownloadLinks.Invoke(new MethodInvoker(delegate { lstItem = lstDownloadLinks.Items[i].ToString(); }));

                                if (lstItem.Contains(magnet.Filename))
                                {
                                    lstDownloadLinks.Invoke(new MethodInvoker(delegate { lstDownloadLinks.Items.RemoveAt(i); }));
                                }

                            }
                        }
                        catch { }


                        if (files.Count == 0)
                        {
                            MessageBox.Show("No downloadable files found.");
                            return;
                        }
                        else if (files.Count == 1)
                        {
                            lstDownloadLinks.Invoke(new MethodInvoker(delegate { lstDownloadLinks.Items.Add($"{files[0].Name}: {files[0].Link} [{ConvertFileSize(files[0].Size)}]  "); }));
                        }
                        else if (files.Count > 1)
                        {
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

                                foreach (var file in selectedFiles)
                                {
                                    lstDownloadLinks.Invoke(new MethodInvoker(delegate { lstDownloadLinks.Items.Add($"{file.Name}: {file.Link} [{ConvertFileSize(file.Size)}]  "); }));
                                }
                            }
                        }
                    }

                    if (deleteWhenDone)
                    {
                        await _allDebridClient.DeleteMagnetAsync((long)Convert.ToDouble(torrentID));
                    }

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
                    }

                    int lstCounter = 0;
                    foreach (string item in lstDownloadLinks.Items)
                    {
                        if (!item.StartsWith("["))
                        {
                            lstCounter++;
                        }
                    }

                    if (totalSizeInBytes == 0)
                    {
                        progress_Label.Text = $"Total Links: {lstCounter}";
                    }
                    else
                    {
                        progress_Label.Text = $"Total Links: {lstCounter}, Total Size: {FormatFileSize(totalSizeInBytes)}";
                    }
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (!cancelled)
                {
                    speed_Label.Text = "Download Speed: Completed";
                    progressBar1.Value = 0;
                }
            }

            _isOperationRunning = false;
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

        #endregion fetchLinksAllDebrid

        #region fetchLinks

        private async void fetchLinks(string magnetLink, bool deleteWhenDone)
        {
            // Check if the operation is already running
            if (_isOperationRunning)
            {
                MessageBox.Show("Operation is already in progress.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _isOperationRunning = true;

            bool cancelled = false;

            if (string.IsNullOrWhiteSpace(magnetLink))
            {
                MessageBox.Show("Please enter a valid magnet link.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Add Magnet Link
                string torrentId = await _realDebridClient.AddMagnetLinkAsync(magnetLink);
                string latestTorrentId = await _realDebridClient.GetLatestTorrentIdAsync();

                // Get the latest torrent info
                RealDebridAPI.RealDebridClient.TorrentInfo torrentInfo = await _realDebridClient.GetTorrentInfoAsync(torrentId);
                progressBar1.Style = ProgressBarStyle.Continuous;

                string[] selectedFileIds = Array.Empty<string>();

                if (torrentInfo.Files.Length == 1)
                {
                    selectedFileIds = new string[] { "1" }; // Correct data type and format
                    await _realDebridClient.SelectFilesAsync(torrentId, selectedFileIds);
                }
                else
                {
                    // Display files to the user and allow selection
                    var fileSelectionForm = new FileSelectionForm(torrentInfo.Files);

                    fileSelectionForm.StartPosition = FormStartPosition.Manual;
                    int x = this.Location.X + (this.Width - fileSelectionForm.Width) / 2;
                    int y = this.Location.Y + (this.Height - fileSelectionForm.Height) / 2;
                    fileSelectionForm.Location = new Point(x, y);

                    if (fileSelectionForm.ShowDialog() == DialogResult.OK)
                    {
                        selectedFileIds = fileSelectionForm.SelectedFileIds;

                        if (selectedFileIds == null || selectedFileIds.Length == 0)
                        {
                            MessageBox.Show("No files selected for download.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        await _realDebridClient.SelectFilesAsync(torrentId, selectedFileIds);
                    }
                    else
                    {
                        // Delete Torrent on cancellation
                        await _realDebridClient.DeleteTorrentAsync(torrentId);
                        cancelled = true;
                        MessageBox.Show("File selection was canceled.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                // Make sure its not downloaded yet
                if (torrentInfo.Status != "downloaded" && !cancelled)
                {
                    FetchLinks(torrentInfo, torrentId, latestTorrentId, selectedFileIds.Length, deleteWhenDone);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (!cancelled)
                {
                    speed_Label.Text = "Download Speed: Completed";
                    progressBar1.Value = 0;
                }
            }

            _isOperationRunning = false;
        }

        private async void FetchLinks(RealDebridAPI.RealDebridClient.TorrentInfo torrentInfo, string torrentId, string latestTorrentId, int selectedFileIds, bool deleteWhenDone)
        {
            bool _cancelDelete = false;

            await Task.Run(async () =>
            {
                //MessageBox.Show("Download to the Real-Debrid Server is Starting");

                while (torrentInfo.Status != "downloaded")
                {
                    try
                    {
                        torrentInfo = await _realDebridClient.GetTorrentInfoAsync(torrentId);

                        // here

                        if (torrentInfo.Status == "queued" || torrentInfo.Status == "downloading" || torrentInfo.Status == "uploading" || torrentInfo.Status == "compressing")
                        {
                            AddOrUpdateListBox(torrentInfo);
                        }

                        if (torrentInfo.Status == "downloaded")
                        {
                            break;
                        }

                        if (torrentInfo.Status == "error" || torrentInfo.Status == "virus" || torrentInfo.Status == "dead" || torrentInfo.Status == "magnet_error")
                        {
                            if (torrentInfo.Status != "downloaded")
                                Invoke(new Action(() => MessageBox.Show($"Torrent download failed with status: {torrentInfo.Status}")));

                            break;
                        }

                        await Task.Delay(5000);
                    }
                    catch
                    {
                        // Handle exception
                    }
                }

                if (_cancelDelete == false)
                {
                    int lstCount = 0;
                    lstDownloadLinks.Invoke(new MethodInvoker(delegate { lstCount = lstDownloadLinks.Items.Count; }));

                    try
                    {
                        for (int i = 0; i < lstCount; i++)
                        {
                            string lstItem = "";
                            lstDownloadLinks.Invoke(new MethodInvoker(delegate { lstItem = lstDownloadLinks.Items[i].ToString(); }));

                            if (lstItem.Contains(torrentInfo.FileName))
                            {
                                lstDownloadLinks.Invoke(new MethodInvoker(delegate { lstDownloadLinks.Items.RemoveAt(i); }));
                            }
                        }
                    }
                    catch { }

                    // Retrieve Download Links
                    var downloadLinks = await _realDebridClient.GetDownloadLinksAsync(torrentId, latestTorrentId);

                    string downloadLinksToCopy = "";

                    if (selectedFileIds != downloadLinks.Count)
                    {
                        // Display download links to the user by unrestricting the link
                        foreach (var linkInfo in downloadLinks)
                        {
                            // Call GetFileName and retrieve both the file name and size
                            var (fileName, combinedFileSize) = await _realDebridClient.GetFileName(linkInfo.DownloadLink);
                            string fileSize = FormatFileSize(combinedFileSize > 0 ? combinedFileSize : linkInfo.FileSize);

                            // Format the combined file size (if it’s joined)
                            lstDownloadLinks.Invoke(new MethodInvoker(delegate { lstDownloadLinks.Items.Add($"{fileName}: {linkInfo.DownloadLink} [{fileSize}]  "); }));
                            downloadLinksToCopy += linkInfo.DownloadLink + " ";
                        }
                    }
                    else
                    {
                        // Display download links to the user
                        foreach (var linkInfo in downloadLinks)
                        {
                            string fileSize = FormatFileSize(linkInfo.FileSize);
                            lstDownloadLinks.Invoke(new MethodInvoker(delegate { lstDownloadLinks.Items.Add($"{linkInfo.FileName}: {linkInfo.DownloadLink} [{fileSize}]  "); }));
                            downloadLinksToCopy += linkInfo.DownloadLink + " ";
                        }
                    }

                    Invoke(new Action(() =>
                    {
                        System.Windows.Forms.Clipboard.SetText(downloadLinksToCopy);
                    }));
                }

                if (deleteWhenDone)
                {
                    await _realDebridClient.DeleteTorrentAsync(torrentId);
                }
            });

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
            }

            int lstCounter = 0;
            foreach (string item in lstDownloadLinks.Items)
            {
                if (!item.StartsWith("["))
                {
                    lstCounter++;
                }
            }

            if (totalSizeInBytes == 0)
            {
                progress_Label.Text = $"Total Links: {lstCounter}";
            }
            else
            {
                progress_Label.Text = $"Total Links: {lstCounter}, Total Size: {FormatFileSize(totalSizeInBytes)}";
            }
        }

        private void AddOrUpdateListBox(RealDebridAPI.RealDebridClient.TorrentInfo torrentInfo)
        {
            Invoke(new Action(() =>
            {
                bool itemExists = false;

                for (int i = 0; i < lstDownloadLinks.Items.Count; i++)
                {
                    if (lstDownloadLinks.Items[i].ToString().Contains(torrentInfo.FileName))
                    {
                        itemExists = true;
                        if (torrentInfo.Status == "downloading" || torrentInfo.Status == "queued")
                        {
                            lstDownloadLinks.Items[i] = $"[{torrentInfo.Progress}%] {torrentInfo.FileName}";
                        }
                        else if (torrentInfo.Status == "uploaded" || torrentInfo.Status == "compressing")
                        {
                            lstDownloadLinks.Items[i] = $"[Uploading] {torrentInfo.FileName}";
                        }
                        else if (torrentInfo.Status == "uploaded" || torrentInfo.Status == "compressing")
                        {
                            lstDownloadLinks.Items[i] = $"[Compressing] {torrentInfo.FileName}";
                        }
                    }
                }

                if (!itemExists)
                {
                    lstDownloadLinks.Items.Add($"[{torrentInfo.Progress}%] {torrentInfo.FileName}");
                }
            }));
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

        #endregion fetchLinks

        private static SavedTorrents saved;

        private AllDebridClient _allDebridClient;
        private async void button1_Click(object sender, EventArgs e)
        {
            _allDebridClient = new AllDebridClient("cx4wX4O1YJcWj7soIvJP");

            var files = await _allDebridClient.GetDownloadableFilesAsync(371576004);

            if (files.Count == 0)
            {
                MessageBox.Show("No downloadable files found.");
                return;
            }

            var message = string.Join(Environment.NewLine, files.Select(f => $"{f.Name}:\n{f.Link}:\n{f.Size}"));
            MessageBox.Show(message, "Magnet Files");
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                _allDebridClient = new AllDebridClient("cx4wX4O1YJcWj7soIvJP");
                var magnet = await _allDebridClient.GetStatusByIDAsync(371576004);

            if (magnet != null)
            {
                string statusMessage = $"Magnet ID: {magnet.Id}\n" +
                                       $"Filename: {magnet.Filename}\n" +
                                       $"Status: {magnet.Status}\n" +
                                       $"Status Code: {magnet.StatusCode}\n" +
                                       $"Downloaded: {magnet.Downloaded?.ToString() ?? "N/A"}\n" +
                                       $"Total Size: {magnet.Size}\n" +
                                       $"Seeders: {magnet.Seeders?.ToString() ?? "N/A"}";

                MessageBox.Show(statusMessage, "Magnet Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Magnet not found or error retrieving status.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            _allDebridClient = new AllDebridClient("cx4wX4O1YJcWj7soIvJP");
            var unlockResult = await _allDebridClient.UnlockLinkAsync("https://alldebrid.com/f/8pl2XVEC77AIxXZLspzkzoMhLChdib_TlRLfu2Q-hbk");

            if (unlockResult != null)
            {
                MessageBox.Show($"Unlocked Link: {unlockResult.Link}");
            }
            else
            {
                MessageBox.Show("Failed to unlock link.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
