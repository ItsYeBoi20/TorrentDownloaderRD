using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static RealDebridAPI.RealDebridClient;

namespace MediaDownloader
{
    public partial class FileSelectionForm : Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        private const int IDC_HAND = 32649;


        private readonly TorrentFile[] _files;
        public string[] SelectedFileIds { get; private set; }

        public FileSelectionForm(TorrentFile[] files)
        {
            InitializeComponent();
            _files = files;

            PopulateTreeView();
        }

        private void PopulateTreeView()
        {
            var rootNode = new TreeNode("Files");

            foreach (var file in _files)
            {
                var pathParts = file.Path.Trim('/').Split('/');
                TreeNode currentNode = rootNode;

                foreach (var part in pathParts)
                {
                    var existingNode = currentNode.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == part);

                    if (existingNode == null)
                    {
                        existingNode = new TreeNode(part);
                        currentNode.Nodes.Add(existingNode);
                    }

                    currentNode = existingNode;
                }

                // Add the file size to the node text
                currentNode.Text += $" ({ConvertFileSize(file.Bytes)})";
                currentNode.Tag = file;
            }

            tvFiles.Nodes.Add(rootNode);
            tvFiles.ExpandAll();

            // Attach the AfterCheck event handler
            tvFiles.AfterCheck += tvFiles_AfterCheck;
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
            return $"{len:0.##} {sizes[order]}";
        }

        private void tvFiles_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // Detach event to avoid recursive calls
            tvFiles.AfterCheck -= tvFiles_AfterCheck;

            // Ensure that all child nodes reflect the parent node's checked state
            ApplyCheckStateToChildren(e.Node, e.Node.Checked);

            // Ensure that the parent nodes reflect the state of their child nodes
            UpdateParentCheckState(e.Node);

            // Reattach the event
            tvFiles.AfterCheck += tvFiles_AfterCheck;
        }

        private void ApplyCheckStateToChildren(TreeNode node, bool isChecked)
        {
            foreach (TreeNode childNode in node.Nodes)
            {
                // Force the checked state on all children to match the parent
                childNode.Checked = isChecked;
                ApplyCheckStateToChildren(childNode, isChecked);
            }
        }

        private void UpdateParentCheckState(TreeNode node)
        {
            if (node.Parent != null)
            {
                // Check if all siblings are checked
                bool allSiblingsChecked = node.Parent.Nodes.Cast<TreeNode>().All(n => n.Checked);

                // Update parent node's check state based on children
                node.Parent.Checked = allSiblingsChecked;

                // Recursively update parent nodes
                UpdateParentCheckState(node.Parent);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            var selectedFiles = new List<string>();

            foreach (TreeNode node in tvFiles.Nodes)
            {
                GetCheckedFiles(node, selectedFiles);
            }

            SelectedFileIds = selectedFiles.ToArray();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void GetCheckedFiles(TreeNode node, List<string> selectedFiles)
        {
            // Add the file ID to the list if it's checked
            if (node.Tag is TorrentFile file && node.Checked)
            {
                selectedFiles.Add(file.Id);
            }

            foreach (TreeNode childNode in node.Nodes)
            {
                GetCheckedFiles(childNode, selectedFiles);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
