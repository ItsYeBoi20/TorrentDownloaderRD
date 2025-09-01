using RealDebridAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TorrentDownloaderRD;
using TorrentDownloaderRD.Processing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MediaDownloader
{
    public partial class Settings : Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
        private const int IDC_HAND = 32649;

        public static string currentVersion = "1.1.1";
        public static string selectedProvider = "Real-Debrid";

        public Settings()
        {
            InitializeComponent();
            label1.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label2.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_All.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_None.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_Anime.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_Games.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_Movies.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label_Media.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label3.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
            label4.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
        }

        private void button_Save_Click(object sender, EventArgs e)
        {
            // Write the initial settings to the file
            File.WriteAllText("Settings.txt",
                "API Provider: " + selectedProvider + "\n" +
                "Real-Debrid API Key: " + textBox_Key.Text + "\n" +
                "AllDebrid API Key: " + textBox_Key1.Text + "\n" +
                "Website Searches: " + numericUpDown_Pages.Value + "\n" + 
                "DeleteAfter: " + checkBox_Remove.Checked + "\n" +
                "DetailedView: " + checkBox_Detailed.Checked + "\n");

            // Append each item in the CheckedListBox to the file
            using (StreamWriter writer = new StreamWriter("Settings.txt", true))
            {
                foreach (var item in checkedListBox_Providers.Items)
                {
                    bool isChecked = checkedListBox_Providers.GetItemChecked(checkedListBox_Providers.Items.IndexOf(item));
                    writer.WriteLine($"{item}: {isChecked}");
                }
            }

            // Disable the textBox and numericUpDown if they are enabled
            if (textBox_Key.Enabled == true)
            {
                checkedListBox_Providers.Enabled = false;
            }

            button_Save.Text = "Saved Settings!";
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            label2.Text = "Current Version: " + currentVersion;

            if (File.Exists("Settings.txt"))
            {
                string[] lines = File.ReadAllLines("Settings.txt");
                string selectedAPI = lines[0].Replace("API Provider: ", "");
                textBox_Key.Text = lines[1].Replace("Real-Debrid API Key: ", "");
                textBox_Key1.Text = lines[2].Replace("AllDebrid API Key: ", "");
                numericUpDown_Pages.Value = Convert.ToDecimal(lines[3].Replace("Website Searches: ", ""));
                checkBox_Remove.Checked = Convert.ToBoolean(lines[4].Replace("DeleteAfter: ", ""));
                checkBox_Detailed.Checked = Convert.ToBoolean(lines[5].Replace("DetailedView: ", ""));

                if (selectedAPI == "Real-Debrid")
                {
                    selectedProvider = "Real-Debrid";
                    label3.ForeColor = Color.DarkGreen;
                    label4.ForeColor = Color.Black;
                    textBox_Key.Enabled = true;
                    textBox_Key.Visible = true;

                    textBox_Key1.Enabled = false;
                    textBox_Key1.Visible = false;
                }
                else if (selectedAPI == "AllDebrid")
                {
                    selectedProvider = "AllDebrid";
                    label3.ForeColor = Color.Black;
                    label4.ForeColor = Color.DarkGreen;
                    textBox_Key.Enabled = false;
                    textBox_Key.Visible = false;

                    textBox_Key1.Enabled = true;
                    textBox_Key1.Visible = true;
                }

                // Start reading from the fifth line for CheckedListBox items
                for (int i = 5; i < lines.Length; i++)
                {
                    string[] parts = lines[i].Split(':');
                    if (parts.Length == 2)
                    {
                        string itemText = parts[0].Trim();
                        bool isChecked = Convert.ToBoolean(parts[1].Trim());

                        // Find the item in the CheckedListBox and set its checked status
                        int index = checkedListBox_Providers.Items.IndexOf(itemText);
                        if (index != -1)
                        {
                            checkedListBox_Providers.SetItemChecked(index, isChecked);
                        }
                    }
                }
            }
        }

        private async void label1_Click(object sender, EventArgs e)
        {
            if (textBox_Key.Text != "")
            {
                if (selectedProvider == "Real-Debrid")
                {
                    var client = new RealDebridClient(textBox_Key.Text);

                    bool isPremium = await client.IsPremiumUserAsync();

                    if (isPremium)
                    {
                        MessageBox.Show("User is a Premium member", "Premium Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("User is not a Premium member", "Premium Status", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else if (selectedProvider == "AllDebrid")
                {
                    var client = new AllDebridClient(textBox_Key1.Text);

                    bool isPremium = await client.IsPremiumUserAsync();

                    if (isPremium)
                    {
                        MessageBox.Show("User is a Premium member", "Premium Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("User is not a Premium member", "Premium Status", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            else
            {
                MessageBox.Show("Textbox is Empty!");
            }
        }

        private async void label2_Click(object sender, EventArgs e)
        {
            await CheckForUpdateAsync();
        }

        private async Task CheckForUpdateAsync()
        {
            string latestVersion = await GetLatestVersionAsync();

            if (latestVersion != null)
            {
                if (IsNewerVersion(latestVersion, currentVersion))
                {
                    MessageBox.Show($"A new version ({latestVersion}) is available! Please update.");
                }
                else
                {
                    MessageBox.Show("You are using the latest version.");
                }
            }
            else
            {
                MessageBox.Show("Error getting current version!");
            }
        }

        private bool IsNewerVersion(string latestVersion, string currentVersion)
        {
            string[] latestVersionParts = latestVersion.Split('.');
            string[] currentVersionParts = currentVersion.Split('.');

            for (int i = 0; i < Math.Max(latestVersionParts.Length, currentVersionParts.Length); i++)
            {
                int latestPart = i < latestVersionParts.Length ? int.Parse(latestVersionParts[i]) : 0;
                int currentPart = i < currentVersionParts.Length ? int.Parse(currentVersionParts[i]) : 0;

                if (latestPart > currentPart)
                {
                    return true;
                }
                else if (latestPart < currentPart)
                {
                    return false;
                }
            }
            return false; // Versions are equal
        }

        private async Task<string> GetLatestVersionAsync()
        {
            string fileUrl = "https://raw.githubusercontent.com/ItsYeBoi20/TorrentDownloaderRD/main/TorrentDownloaderRD/Settings.cs";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string fileContent = await client.GetStringAsync(fileUrl);
                    return ExtractVersion(fileContent);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching version: {ex.Message}");
                    return null;
                }
            }
        }

        private string ExtractVersion(string fileContent)
        {
            string pattern = @"public\s+static\s+string\s+currentVersion\s*=\s*""([^""]+)"";";
            Match match = Regex.Match(fileContent, pattern);
            return match.Success ? match.Groups[1].Value : null;
        }

        private void label_All_Click(object sender, EventArgs e)
        {
            CheckAllItems();
        }

        private void label_None_Click(object sender, EventArgs e)
        {
            CheckNoItems();
        }

        private void label_Anime_Click(object sender, EventArgs e)
        {
            List<string> itemsToCheck = new List<string> { "Nyaa", "AnimeTosho" };

            CheckNoItems();

            CheckSpecificItems(checkedListBox_Providers, itemsToCheck);
        }

        private void label_Games_Click(object sender, EventArgs e)
        {
            List<string> itemsToCheck = new List<string> { "FitGirl", "Empress", "Dodi", "GOG", "OnlineFix", "TinyRepacks", "Xatab" };

            CheckNoItems();

            CheckSpecificItems(checkedListBox_Providers, itemsToCheck);
        }

        private void label_Movies_Click(object sender, EventArgs e)
        {
            List<string> itemsToCheck = new List<string> { "1337x", "LimeTorrents", "Piratebay", "Torlock2", "TorrentProject", 
                "Torrents-CSV", "TorrentDownload", "YourBittorrent", "TorrentGalaxy", "BitSearch", "TheRarbg", "KickAssTorrents" };

            CheckNoItems();

            CheckSpecificItems(checkedListBox_Providers, itemsToCheck);
        }

        private void label_Media_Click(object sender, EventArgs e)
        {
            List<string> itemsToCheck = new List<string> { "1337x", "LimeTorrents", "Nyaa", "AnimeTosho" , "Piratebay", "Torlock2", "TorrentProject",
                "Torrents-CSV", "TorrentDownload", "YourBittorrent", "TorrentGalaxy", "BitSearch", "TheRarbg", "KickAssTorrents" };

            CheckNoItems();

            CheckSpecificItems(checkedListBox_Providers, itemsToCheck);
        }

        private void CheckAllItems()
        {
            for (int i = 0; i < checkedListBox_Providers.Items.Count; i++)
            {
                checkedListBox_Providers.SetItemChecked(i, true);
            }
        }

        private void CheckNoItems()
        {
            for (int i = 0; i < checkedListBox_Providers.Items.Count; i++)
            {
                checkedListBox_Providers.SetItemChecked(i, false);
            }
        }

        private void CheckSpecificItems(CheckedListBox checkedListBox, List<string> itemsToCheck)
        {
            for (int i = 0; i < checkedListBox.Items.Count; i++)
            {
                string item = checkedListBox.Items[i].ToString();
                if (itemsToCheck.Contains(item))
                {
                    checkedListBox.SetItemChecked(i, true);
                }
            }
        }

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Write the initial settings to the file
            File.WriteAllText("Settings.txt",
                "API Provider: " + selectedProvider + "\n" +
                "Real-Debrid API Key: " + textBox_Key.Text + "\n" +
                "AllDebrid API Key: " + textBox_Key1.Text + "\n" +
                "Website Searches: " + numericUpDown_Pages.Value + "\n" + 
                "DeleteAfter: " + checkBox_Remove.Checked + "\n" +
                "DetailedView: " + checkBox_Detailed.Checked + "\n");

            // Append each item in the CheckedListBox to the file
            using (StreamWriter writer = new StreamWriter("Settings.txt", true))
            {
                foreach (var item in checkedListBox_Providers.Items)
                {
                    bool isChecked = checkedListBox_Providers.GetItemChecked(checkedListBox_Providers.Items.IndexOf(item));
                    writer.WriteLine($"{item}: {isChecked}");
                }
            }
        }

        private static TestProviders testForm;
        private void button_Test_Click(object sender, EventArgs e)
        {
            if (testForm == null || testForm.IsDisposed)
            {
                testForm = new TestProviders();
                testForm.StartPosition = FormStartPosition.Manual;

                int x = this.Location.X + (this.Width - testForm.Width) / 2;
                int y = this.Location.Y + (this.Height - testForm.Height) / 2;
                Rectangle screenBounds = Screen.FromControl(this).WorkingArea;
                if (x < screenBounds.Left)
                {
                    x = screenBounds.Left;
                }
                else if (x + testForm.Width > screenBounds.Right)
                {
                    x = screenBounds.Right - testForm.Width;
                }
                if (y < screenBounds.Top)
                {
                    y = screenBounds.Top;
                }
                else if (y + testForm.Height > screenBounds.Bottom)
                {
                    y = screenBounds.Bottom - testForm.Height;
                }
                testForm.Location = new Point(x, y);
                testForm.Show();
            }
            else
            {
                testForm.BringToFront();
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {
            if (selectedProvider == "AllDebrid")
            {
                selectedProvider = "Real-Debrid";
                label3.ForeColor = Color.DarkGreen;
                label4.ForeColor = Color.Black;
                textBox_Key.Enabled = true;
                textBox_Key.Visible = true;

                textBox_Key1.Enabled = false;
                textBox_Key1.Visible = false;
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {
            if (selectedProvider == "Real-Debrid")
            {
                selectedProvider = "AllDebrid";
                label3.ForeColor = Color.Black;
                label4.ForeColor = Color.DarkGreen;
                textBox_Key.Enabled = false;
                textBox_Key.Visible = false;

                textBox_Key1.Enabled = true;
                textBox_Key1.Visible = true;
            }
        }
    }
}
