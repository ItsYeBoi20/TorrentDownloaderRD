﻿using RealDebridAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MediaDownloader
{
    public partial class Settings : Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
        private const int IDC_HAND = 32649;

        public static string currentVersion = "1.0.4";

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
        }

        private void button_Edit_Click(object sender, EventArgs e)
        {
            if (textBox_Key.Enabled == true)
            {
                textBox_Key.Enabled = false;
                numericUpDown_Pages.Enabled = false;
                checkedListBox_Providers.Enabled = false;
                checkBox_Remove.Enabled = false;
                button_Edit.Text = "Edit";

                label_None.Enabled = false;
                label_All.Enabled = false;
                label_Anime.Enabled = false;
                label_Games.Enabled = false;
                label_Movies.Enabled = false;
                label_Media.Enabled = false;
            }
            else if (textBox_Key.Enabled == false)
            {
                textBox_Key.Enabled = true;
                numericUpDown_Pages.Enabled = true;
                checkedListBox_Providers.Enabled = true;
                checkBox_Remove.Enabled = true;
                button_Edit.Text = "Done";

                label_None.Enabled = true;
                label_All.Enabled = true;
                label_Anime.Enabled = true;
                label_Games.Enabled = true;
                label_Movies.Enabled = true;
                label_Media.Enabled = true;
            }
        }

        private void button_Save_Click(object sender, EventArgs e)
        {
            // Write the initial settings to the file
            File.WriteAllText("Settings.txt", 
                "API Key: " + textBox_Key.Text + "\n" + 
                "Website Searches: " + numericUpDown_Pages.Value + "\n" + 
                "DeleteAfter: " + checkBox_Remove.Checked + "\n");

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
                textBox_Key.Enabled = false;
                numericUpDown_Pages.Enabled = false;
                checkBox_Remove.Enabled = false;
                button_Edit.Text = "Edit";

                label_None.Enabled = false;
                label_All.Enabled = false;
                label_Anime.Enabled = false;
                label_Games.Enabled = false;
                label_Movies.Enabled = false;
                label_Media.Enabled = false;
            }
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            label2.Text = "Current Version: " + currentVersion;

            if (File.Exists("Settings.txt"))
            {
                string[] lines = File.ReadAllLines("Settings.txt");
                textBox_Key.Text = lines[0].Replace("API Key: ", "");
                numericUpDown_Pages.Value = Convert.ToDecimal(lines[1].Replace("Website Searches: ", ""));
                checkBox_Remove.Checked = Convert.ToBoolean(lines[2].Replace("DeleteAfter: ", ""));

                // Start reading from the third line for CheckedListBox items
                for (int i = 3; i < lines.Length; i++)
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
                string APIKey = textBox_Key.Text;
                var client = new RealDebridClient(APIKey);

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
                "Torrents-CSV", "TorrentDownload", "YourBittorrent", "TorrentGalaxy", "BitSearch", "TheRarbg" };

            CheckNoItems();

            CheckSpecificItems(checkedListBox_Providers, itemsToCheck);
        }

        private void label_Media_Click(object sender, EventArgs e)
        {
            List<string> itemsToCheck = new List<string> { "1337x", "LimeTorrents", "Nyaa", "AnimeTosho" , "Piratebay", "Torlock2", "TorrentProject",
                "Torrents-CSV", "TorrentDownload", "YourBittorrent", "TorrentGalaxy", "BitSearch", "TheRarbg" };

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
    }
}
