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
        public frmMain()
        {
            InitializeComponent();
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
                    currentNode.Nodes.Add(new TreeNode(file.Name));
            }

            treeView.Nodes.Add(node);
        }

        private static void ShowTags(DataGridView dgv, string filename)
        {
            dgv.Rows.Clear();
            using (FlacLibSharp.FlacFile file = new FlacFile(filename))
            {
                var vorbisComment = file.VorbisComment;
                if (vorbisComment != null)
                {
                    foreach (var tag in vorbisComment)
                    {
                        foreach (var value in tag.Value)
                        {
                            List<string> newRow = new List<string>();

                            newRow.Add(tag.Key);
                            newRow.Add(value);
                            newRow.Add(value);
                            dgv.Rows.Add(newRow.ToArray());

                        }
                    }
                }
            }



        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ShowTags(dataGridView1, @"E:\" + treeView1.SelectedNode.FullPath);
        }


        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            ListDirectory(treeView1, @"E:\Music");
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
