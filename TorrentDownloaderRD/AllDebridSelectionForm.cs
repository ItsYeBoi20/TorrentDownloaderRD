using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace TorrentDownloaderRD
{
    public partial class AllDebridSelectionForm : Form
    {
        private readonly List<(string Name, string Link, long Size)> _files;
        public List<(string Name, string Link, long Size)> SelectedFiles { get; private set; } = new List<(string, string, long)>();

        public AllDebridSelectionForm(List<(string Name, string Link, long Size)> files)
        {
            InitializeComponent();
            _files = files;

            PopulateTreeView();
        }

        private void PopulateTreeView()
        {
            tvFiles.Nodes.Clear();
            tvFiles.CheckBoxes = true;

            var sortedFiles = _files.OrderBy(file => file.Name).ToList();

            TreeNode parentNode = new TreeNode("Files");

            foreach (var file in sortedFiles)
            {
                TreeNode node = new TreeNode($"{file.Name} ({ConvertFileSize(file.Size)})");
                node.Tag = file;
                parentNode.Nodes.Add(node);
            }

            tvFiles.Nodes.Add(parentNode);
            tvFiles.ExpandAll();

            tvFiles.AfterCheck += TvFiles_AfterCheck;
        }

        private void TvFiles_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // Detach event to avoid recursive calls
            tvFiles.AfterCheck -= TvFiles_AfterCheck;

            // Ensure that all child nodes reflect the parent node's checked state
            ApplyCheckStateToChildren(e.Node, e.Node.Checked);

            // Ensure that the parent nodes reflect the state of their child nodes
            UpdateParentCheckState(e.Node);

            // Reattach the event
            tvFiles.AfterCheck += TvFiles_AfterCheck;
        }

        private void ApplyCheckStateToChildren(TreeNode parentNode, bool isChecked)
        {
            foreach (TreeNode node in parentNode.Nodes)
            {
                node.Checked = isChecked;
                ApplyCheckStateToChildren(node, isChecked);
            }
        }

        private void UpdateParentCheckState(TreeNode parentNode)
        {
            if (parentNode.Parent != null)
            {
                // Check if all siblings are checked
                bool allSiblingsChecked = parentNode.Parent.Nodes.Cast<TreeNode>().All(n => n.Checked);

                // Update parent node's check state based on children
                parentNode.Parent.Checked = allSiblingsChecked;

                // Recursively update parent nodes
                UpdateParentCheckState(parentNode.Parent);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            SelectedFiles.Clear();

            foreach (TreeNode node in tvFiles.Nodes)
            {
                if (node.Text == "Files")
                {
                    foreach (TreeNode childNode in node.Nodes)
                    {
                        if (childNode.Checked)
                        {
                            if (childNode.Tag is ValueTuple<string, string, long> file)
                            {
                                SelectedFiles.Add(file);
                            }
                        }
                    }
                }
            }

            if (SelectedFiles.Count == 0)
            {
                MessageBox.Show("No files selected for download.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
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