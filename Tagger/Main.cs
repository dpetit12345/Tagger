using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlacLibSharp;
using System.IO;

namespace Tagger
{
    public partial class frmMain : Form
    {

        private TaggedFile currentFile = null;
        string lastPath = "";
        public frmMain()
        {
            InitializeComponent();


            // Add the text box to the form.
            
        }


        private static void ListDirectory(TreeView treeView, string path)
        {
            treeView.Nodes.Clear();

            var stack = new Stack<TreeNode>();
            var rootDirectory = new DirectoryInfo(path);
            var node = new TreeNode(rootDirectory.Name) { Tag = rootDirectory };
            stack.Push(node);

            while (stack.Count > 0)
            {
                var currentNode = stack.Pop();
                var directoryInfo = (DirectoryInfo)currentNode.Tag;
                foreach (var directory in directoryInfo.GetDirectories())
                {
                    var childDirectoryNode = new TreeNode(directory.Name) { Tag = directory };
                    currentNode.Nodes.Add(childDirectoryNode);
                    stack.Push(childDirectoryNode);
                }
                foreach (var file in directoryInfo.GetFiles())
                {
                    if (file.FullName.ToLower().EndsWith(".flac") || file.FullName.ToLower().EndsWith(".dsf"))
                        currentNode.Nodes.Add(new TreeNode(file.Name) { Tag = file });
                }
            }

            treeView.Nodes.Add(node);
        }

        private void ShowSingleFileTags(DataGridView dgv, string filename)
        {
            TaggedFile file = new TaggedFile(filename);
            currentFile = file;
            ShowSingleFileTags(dgv, file);

        }

        private void ShowSingleFileTags(DataGridView dgv, TaggedFile file)
        {
            dgv.Rows.Clear();


            foreach (var delTagKey in file.DeletedTags.Keys)
            {
                foreach (var value in file.GetTag(delTagKey))
                {
                    List<string> newRow = new List<string>();

                    newRow.Add(delTagKey);
                    newRow.Add(value);
                    newRow.Add(value);
                    dgv.Rows.Add(newRow.ToArray());
                }

            }
            foreach (var updTagKey in file.ChangedTags.Keys)
            {
                foreach (var value in file.GetTag(updTagKey))
                {
                    List<string> newRow = new List<string>();

                    newRow.Add(updTagKey);
                    newRow.Add(value);
                    newRow.Add(value);
                    dgv.Rows.Add(newRow.ToArray());
                }

            }

            foreach (var addedTagKey in file.AddedTags.Keys)
            {
                foreach (var value in file.GetTag(addedTagKey))
                {
                    List<string> newRow = new List<string>();

                    newRow.Add(addedTagKey);
                    newRow.Add(value);
                    newRow.Add(value);
                    dgv.Rows.Add(newRow.ToArray());
                }

            }

            foreach (var tag in file.AllTags.Keys)
            {
                if (file.ChangedTags.ContainsKey(tag) || file.AddedTags.ContainsKey(tag) || file.DeletedTags.ContainsKey(tag))
                    continue;                
                foreach (var value in file.GetTag(tag))
                {
                    List<string> newRow = new List<string>();

                    newRow.Add(tag);
                    newRow.Add(value);
                    newRow.Add(value);
                    dgv.Rows.Add(newRow.ToArray());

                }
            }
        }

        private string GetLastFolder()
        {
            return @"E:\Music";
        }

        private void tvFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.Unknown) return;
            switch (e.Action)
            {
                case TreeViewAction.ByKeyboard:
                    break;
                case TreeViewAction.ByMouse:
                    break;
            }
            if  (tvFiles.SelectedNode.Tag.GetType() == typeof(FileInfo))
            {
                FileInfo di = (FileInfo)tvFiles.SelectedNode.Tag;
                ShowSingleFileTags(dgvTags, di.FullName);
                //TODO: USe a cache of files
            }
            
                
            
        }


        private void tsbOpenFolder_Click(object sender, EventArgs e)
        {

            using(var fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = lastPath;
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    lastPath = fbd.SelectedPath;
                    ListDirectory(tvFiles, lastPath);
                   
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void tsbSaveFile_Click(object sender, EventArgs e)
        {
            currentFile.Save();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            lastPath = GetLastFolder();
            ListDirectory(tvFiles, lastPath);
        }

        private void dgvTags_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            TextBox txt = (TextBox)e.Control;
            txt.AutoCompleteSource = AutoCompleteSource.CustomSource;
            var source = new AutoCompleteStringCollection();
            source.Add("One");
            source.Add("Two");
            source.Add("Three");
            txt.AutoCompleteCustomSource = source;
            txt.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        }

        private void dgvTags_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvTags_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void RefreshTags(TaggedFile file)
        {
            string lastKey = "";
            List<string> currList = new List<string>();
            foreach (DataGridViewRow row in dgvTags.Rows)
            {
                string tagName = row.Cells[0].Value.ToString();
                string newValue = "";
                if (row.Cells[2].Value is null)
                    newValue = "";
                else
                    newValue = row.Cells[2].Value.ToString();
                if (tagName != lastKey)
                {
                    if (lastKey.Length >0)
                    {
                        file.SetTag(lastKey, currList);
                    }
                    currList = new List<string>();
                }
                currList.Add(newValue);
                lastKey = tagName;
                //create a dictionary by tag name
            }
            if (lastKey.Length > 0)
            {
                file.SetTag(lastKey, currList);
            }

            ShowSingleFileTags(dgvTags, currentFile);
        }

        private void dgvTags_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            RefreshTags(currentFile);
        }
    }
}
