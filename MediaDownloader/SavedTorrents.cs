using RealDebridAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
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
                            else
                            {
                                this.Invoke((Action)(() =>
                                {
                                    MessageBox.Show("No torrents found or an error occurred.");
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

        private void lstDownloadLinks_MouseDoubleClick_1(object sender, MouseEventArgs e)
        {
            string[] parts = Regex.Split(lstDownloadLinks.SelectedItem.ToString(), ": ");
            System.Windows.Forms.Clipboard.SetText(parts[1]);
            MessageBox.Show(parts[1] + "\nCopied to Clipboard!");
        }
    }
}
