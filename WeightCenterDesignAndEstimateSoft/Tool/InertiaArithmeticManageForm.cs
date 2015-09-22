using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using XCommon;
using System.IO;

namespace WeightCenterDesignAndEstimateSoft.Tool
{
    public partial class InertiaArithmeticManageForm : Form
    {
        private TreeNode selNode = null;

        static private string strPath = System.AppDomain.CurrentDomain.BaseDirectory;

        private MainForm mainForm = null;

        public InertiaArithmeticManageForm(MainForm form)
        {
            InitializeComponent();
            mainForm = form;
            ParaData.ResetWeightParameterList();
        }

        static public Dictionary<string, string> GetArithmeticItems()
        {
            System.IO.Directory.SetCurrentDirectory(strPath);

            Dictionary<string, string> waDict = new Dictionary<string, string>();

            string dirname = "InertiaArithmetic";
            string[] files = System.IO.Directory.GetFiles(dirname, "*.iam");
            for (int i = 0; i < files.Length; ++i)
            {
                string strname = files[i];
                strname = strname.Substring(strname.LastIndexOf('\\') + 1);
                strname = strname.Substring(0, strname.Length - 4);
                waDict.Add(strname, files[i]);
            }

            return waDict;
        }

        private void IntertiaArithmeticManageForm_Load(object sender, EventArgs e)
        {
            treeViewArithmeticList.ImageList = imageListTreeView;

            Dictionary<string, string> waItems = GetArithmeticItems();

            TreeNode rootnode1 = treeViewArithmeticList.Nodes.Add("InertiaArithmetic", "转动惯量算法", 0, 1);

            foreach (KeyValuePair<string, string> pair in waItems)
            {
                rootnode1.Nodes.Add(pair.Value, pair.Key, 4, 5);
            }

            rootnode1.ExpandAll();

            if (mainForm == null)
            {
                return;
            }
            DesignProjectData dpData = mainForm.designProjectData;

            if (dpData == null)
            {
                return;
            }
            if (dpData.lstIntertiaArithmetic != null)
            {
                TreeNode rootnode2 = treeViewArithmeticList.Nodes.Add("设计结果列表", "设计结果列表", 0, 1);
                foreach (InertiaArithmetic wa in dpData.lstIntertiaArithmetic)
                {
                    TreeNode node = rootnode2.Nodes.Add(wa.DataName, wa.DataName, 4, 5);
                    node.Tag = wa;
                }
                rootnode2.Expand();
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            InertiaArithmeticOperForm form = new InertiaArithmeticOperForm("new", selNode);
            if (form.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            TreeNode node = treeViewArithmeticList.Nodes[0];
            string key = node.Name + "\\" + form.strIntertiaArithmeticFileName + ".iam";
            if (!node.Nodes.ContainsKey(key))
            {
                node.Nodes.Add(key, form.strIntertiaArithmeticFileName, 4, 5);
            }
            if (node.IsExpanded == false)
            {
                node.Expand();
            }

            ParaData.GetWeightParameterList()[10].Clear();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            InertiaArithmeticOperForm form = new InertiaArithmeticOperForm("edit", selNode);
            if (form.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            ParaData.GetWeightParameterList()[10].Clear();
        }

        private void btnJYNew_Click(object sender, EventArgs e)
        {
            InertiaArithmeticOperForm form = new InertiaArithmeticOperForm("jynew", selNode);
            if (form.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            TreeNode node = treeViewArithmeticList.Nodes[0];
            node.Nodes.Add(node.Name + "\\" + form.strIntertiaArithmeticFileName + ".iam", form.strIntertiaArithmeticFileName, 4, 5);
            if (node.IsExpanded == false)
            {
                node.Expand();
            }

            ParaData.GetWeightParameterList()[10].Clear();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("是否删除" + selNode.Name + "?", "删除提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                {
                    System.IO.Directory.SetCurrentDirectory(strPath);
                    System.IO.File.Delete(selNode.Name);

                    selNode.Parent.Nodes.Remove(selNode);
                }
            }
            catch
            {
                MessageBox.Show("删除文件出现错误！");
                return;
            }

        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                System.Windows.Forms.OpenFileDialog dlg = new OpenFileDialog();
                dlg.RestoreDirectory = true;
                dlg.Filter = "Iam files (*.iam)|*.iam";
                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                InertiaArithmetic wa = InertiaArithmetic.ReadArithmeticData(dlg.FileName);
                ParaData.GetWeightParameterList()[10].Clear();

                if (!InertiaArithmeticOperForm.WriteArithmeticFile(wa, true))
                {
                    return;
                }
                //加节点
                TreeNode treenode = treeViewArithmeticList.Nodes[0];
                string filename = "InertiaArithmetic\\" + wa.Name + ".iam";
                if (!treenode.Nodes.ContainsKey(filename))
                {
                    treenode.Nodes.Add(filename, wa.Name, 4, 5);
                    if (treenode.IsExpanded == false)
                    {
                        treenode.Expand();
                    }
                }
            }
            catch 
            {
                XCommon.XLog.Write("导入转动惯量算法文件错误");
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = selNode.Text;

            InertiaArithmetic wa = null;
            if (selNode.Tag != null)
            {
                wa = selNode.Tag as InertiaArithmetic;
                dlg.FileName = wa.Name;
            }

            dlg.OverwritePrompt = false;
            dlg.DefaultExt = "iam";
            dlg.Filter = "Iam files (*.iam)|*.iam";
            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (selNode.Tag == null)
            {
                if (selNode.Name != dlg.FileName)
                {
                    string strSourcefile = System.AppDomain.CurrentDomain.BaseDirectory + selNode.Name;
                    try
                    {
                        if (File.Exists(dlg.FileName))
                        {
                            if (MessageBox.Show("文件\"" + dlg.FileName + "\"已存在，是否覆盖？", "文件已存在", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                System.IO.File.Copy(strSourcefile, dlg.FileName, true);
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            System.IO.File.Copy(strSourcefile, dlg.FileName, true);
                        }
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }
                }
            }
            else
            {
                wa = selNode.Tag as InertiaArithmetic;
                if (!wa.WriteArithmeticFile(dlg.FileName, true))
                {
                    return;
                }
            }

            XCommon.XLog.Write("成功导出算法到文件\"" + dlg.FileName + "\"！");
        }

        private void treeViewArithmeticList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            selNode = e.Node;

            btnEdit.Enabled = (selNode.ImageIndex == 4) && (selNode.Tag == null);
            btnJYNew.Enabled = (selNode.ImageIndex == 4);
            btnDelete.Enabled = (selNode.ImageIndex == 4) && (selNode.Tag == null);
            btnExport.Enabled = (selNode.ImageIndex == 4);
        }
    }
}
