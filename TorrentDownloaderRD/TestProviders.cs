using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TorrentDownloaderRD
{
    public partial class TestProviders : Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
        private const int IDC_HAND = 32649;        

        public TestProviders()
        {
            InitializeComponent();
            label_1337x.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_LT.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_Nyaa.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_AT.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_PB.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_TL2.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_TP.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_CSV.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_TD.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_YBT.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_TG.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_BS.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_Rarbg.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_KAT.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_FG.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_Empress.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_Dodi.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_GOG.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_OnlineFix.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_TinyRepacks.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_Xatab.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
        }

        private async void TestProviders_Load(object sender, EventArgs e)
        {
            await CheckMultipleWebsites(this);
        }

        private static readonly HttpClient client = new HttpClient(new HttpClientHandler
        {
            MaxConnectionsPerServer = 10, // Allow more parallel connections
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate // Speed up with compression
        })
        {
            Timeout = TimeSpan.FromSeconds(15) // Set timeout for faster retries
        };

        public static async Task<(bool isUp, int statusCode)> IsWebsiteUpAsync(string url)
        {
            const int TIMEOUT_STATUS = -1;

            int retryCount = 3;
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
                    using (HttpResponseMessage response =
                           await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cts.Token))
                    {
                        return (response.IsSuccessStatusCode, (int)response.StatusCode);
                    }
                }
                catch (TaskCanceledException)   // 15-s timeout
                {
                    if (i == retryCount - 1) return (false, TIMEOUT_STATUS);
                    await Task.Delay(1000);
                }
                catch (HttpRequestException)    // network / dns / ssl errors
                {
                    return (false, TIMEOUT_STATUS);
                }
            }
            return (false, TIMEOUT_STATUS);
        }

        public static async Task CheckWebsiteAndUpdateLabel(string url, Label label, ProgressBar progressBar, IProgress<int> progress)
        {
            try
            {
                var (isUp, statusCode) = await IsWebsiteUpAsync(url);

                label.Invoke(new Action(() =>
                {
                    if (isUp)
                    {
                        label.ForeColor = Color.Green;
                        // strip any previous status suffix
                        label.Text = label.Text.Split('-')[0].Trim();
                    }
                    else
                    {
                        string statusText = statusCode == -1
                            ? "error loading"
                            : statusCode.ToString();

                        label.ForeColor = Color.Red;
                        label.Text = $"{label.Text.Split('-')[0].Trim()} - {statusText}";
                    }
                }));
            }
            catch
            {
                label.Invoke(new Action(() =>
                {
                    label.ForeColor = Color.Red;
                    label.Text = $"{label.Text.Split('-')[0].Trim()} - error loading";
                }));
            }
            finally
            {
                progress.Report(1);
            }
        }

        public static async Task CheckMultipleWebsites(Form form)
        {
            var websites = new (string url, Label label)[]
            {
                ("https://1337x.to/search/harry+potter/1/", form.Controls["label_1337x"] as Label),
                ("https://www.limetorrents.lol/search/all/harry-potter//1/", form.Controls["label_LT"] as Label),
                ("https://nyaa.si/?f=0&c=0_0&q=one+piece&p=1", form.Controls["label_Nyaa"] as Label),
                ("https://animetosho.org/search?q=one+piece&page=1", form.Controls["label_AT"] as Label),
                ("https://apibay.org/q.php?q=harry+potter", form.Controls["label_PB"] as Label),
                ("https://www.torlock2.com/all/torrents/harry%20potter.html?&page=1", form.Controls["label_TL2"] as Label),
                ("https://torrentproject.cc/?t=harry+potter&p=1", form.Controls["label_TP"] as Label),
                ("https://torrents-csv.com/service/search?q=harry%20potter&size=1", form.Controls["label_CSV"] as Label),
                ("https://www.torrentdownload.info/search?q=harry+potter&p=1", form.Controls["label_TD"] as Label),
                ("https://yourbittorrent.com/?q=harry-potter&page=1", form.Controls["label_YBT"] as Label),
                ("https://torrentgalaxy.to/torrentdump", form.Controls["label_TG"] as Label),
                ("https://bitsearch.to/search?q=harry+potter&page=1", form.Controls["label_BS"] as Label),
                ("https://therarbg.com/get-posts/keywords:harry%20potter/?page=1", form.Controls["label_Rarbg"] as Label),
                ("https://kickasstorrents.to/search/harry%20potter/1/", form.Controls["label_KAT"] as Label),
                ("https://hydralinks.cloud/sources/fitgirl.json", form.Controls["label_FG"] as Label),
                ("https://hydralinks.cloud/sources/empress.json", form.Controls["label_Empress"] as Label),
                ("https://hydralinks.cloud/sources/dodi.json", form.Controls["label_Dodi"] as Label),
                ("https://hydralinks.cloud/sources/gog.json", form.Controls["label_GOG"] as Label),
                ("https://hydralinks.cloud/sources/onlinefix.json", form.Controls["label_OnlineFix"] as Label),
                ("https://hydralinks.cloud/sources/tinyrepacks.json", form.Controls["label_TinyRepacks"] as Label),
                ("https://hydralinks.cloud/sources/xatab.json", form.Controls["label_Xatab"] as Label),
            };

            /*var tasks = websites
            .Where(site => site.label != null)
            .Select(site => CheckWebsiteAndUpdateLabel(site.url, site.label));

            await Task.WhenAll(tasks);*/

            int totalWebsites = websites.Length;
            var progressBar = form.Controls["progressBar1"] as ProgressBar;

            progressBar.Invoke(new Action(() =>
            {
                progressBar.Minimum = 0;
                progressBar.Maximum = totalWebsites;
                progressBar.Value = 0;
            }));

            int progressCount = 0;
            var progress = new Progress<int>(value =>
            {
                progressCount += value;
                progressBar.Invoke(new Action(() =>
                {
                    progressBar.Value = progressCount;
                }));
            });

            var tasks = websites
                .Where(site => site.label != null)
                .Select(site => CheckWebsiteAndUpdateLabel(site.url, site.label, progressBar, progress));

            await Task.WhenAll(tasks);
        }

        #region openURLs

        private void label_1337x_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Process.Start("https://1337x.to/");
        }

        private void label_LT_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.limetorrents.lol/");
        }

        private void label_Nyaa_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://nyaa.si/");
        }

        private void label_AT_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://animetosho.org/");
        }

        private void label_PB_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://apibay.org/q.php?q=");
        }

        private void label_TL2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.torlock2.com/");
        }

        private void label_CSV_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://torrents-csv.com/");
        }

        private void label_TP_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://torrentproject.cc/");
        }

        private void label_TD_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.torrentdownload.info/");
        }

        private void label_YBT_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://yourbittorrent.com/");
        }

        private void label_TG_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://torrentgalaxy.to/");
        }

        private void label_BS_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://bitsearch.to/");
        }

        private void label_Rarbg_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://therarbg.com/");
        }

        private void label_KAT_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://kickasstorrents.to/");
        }

        private void label_FG_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://hydralinks.cloud/sources/fitgirl.json");
        }

        private void label_Empress_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://hydralinks.cloud/sources/empress.json");
        }

        private void label_Dodi_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://hydralinks.cloud/sources/dodi.json");
        }

        private void label_GOG_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://hydralinks.cloud/sources/gog.json");
        }

        private void label_OnlineFix_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://hydralinks.cloud/sources/onlinefix.json");
        }

        private void label_TinyRepacks_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://hydralinks.cloud/sources/tinyrepacks.json");
        }

        private void label_Xatab_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://hydralinks.cloud/sources/xatab.json");
        }

        #endregion openURLs
    }
}
