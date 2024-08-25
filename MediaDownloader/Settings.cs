﻿using RealDebridAPI;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MediaDownloader
{
    public partial class Settings : Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
        private const int IDC_HAND = 32649;

        public Settings()
        {
            InitializeComponent();
            label1.Cursor = new Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));
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
            }
            else if (textBox_Key.Enabled == false)
            {
                textBox_Key.Enabled = true;
                numericUpDown_Pages.Enabled = true;
                checkedListBox_Providers.Enabled = true;
                checkBox_Remove.Enabled = true;
                button_Edit.Text = "Done";
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
            }
        }

        private void Settings_Load(object sender, EventArgs e)
        {
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
    }
}