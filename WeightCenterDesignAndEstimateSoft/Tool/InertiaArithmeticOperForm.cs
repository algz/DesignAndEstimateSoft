﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using XCommon;

namespace WeightCenterDesignAndEstimateSoft.Tool
{
    public partial class InertiaArithmeticOperForm : Form
    {
        public string strIntertiaArithmeticFileName = "";

        private string strType = string.Empty;

        private InertiaArithmetic waData = new InertiaArithmetic();
        private Dictionary<string, string> dictTempPara = new Dictionary<string, string>();

        public InertiaArithmeticOperForm()
        {
            InitializeComponent();
        }

        public InertiaArithmeticOperForm(string str_Type, TreeNode selNode)
        {
            InitializeComponent();
            strType = str_Type;

            switch (strType)
            {
                case "new":
                    {
                        txtName.ReadOnly = false;
                        txtName.Text = "新建算法";
                        break;
                    }
                case "edit":
                    {
                        waData = InertiaArithmetic.ReadArithmeticData(selNode.Name);
                        dateTimePickerCreateTime.Text = waData.CreateTime;
                        dateTimePickerLastModifyTime.Text = waData.LastModifyTime;
                        txtRemark.Text = waData.Remark;

                        txtName.Text = waData.Name;
                        txtName.ReadOnly = true;
                        break;
                    }
                case "jynew":
                    {
                        if (selNode.Tag == null)
                        {
                            waData = InertiaArithmetic.ReadArithmeticData(selNode.Name);
                        }
                        else
                        {
                            InertiaArithmetic tempwa = selNode.Tag as InertiaArithmetic;
                            waData = (tempwa != null) ? tempwa.Clone() : (new InertiaArithmetic());
                        }
                        dateTimePickerCreateTime.Text = waData.CreateTime;
                        dateTimePickerLastModifyTime.Text = waData.LastModifyTime;
                        txtRemark.Text = waData.Remark;
                      
                        txtName.Text = waData.Name + " 副本";
                        txtName.ReadOnly = false;
                        break;
                    }
                default: 
                    {
                        return;
                    }
            }

            foreach (WeightFormula wf in waData.FormulaList)
            {
                treeViewWeightSortNode.Nodes.Add(wf.NodePath, wf.NodePath).Tag = wf;
            }
        }

        private void treeViewWeightSortNode_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (textBoxFormula.Modified)
            {
                if (MessageBox.Show("公式已更改，是否放弃？", "公式已更改", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    textBoxFormula.Modified = false;
                }
            }
        }

        private void SetParaList(IEnumerable<ParaData> paras)
        {
            listViewPara.Items.Clear();

            foreach (ParaData wp in paras)
            {
                ListViewItem lvi = listViewPara.Items.Add(wp.paraName);
                lvi.SubItems.Add(wp.paraUnit);
                lvi.SubItems.Add(ParaData.ParaTypeList[wp.paraType]);
                lvi.SubItems.Add(wp.strRemark);
            }
        }

        private void GetFinalNodeList(TreeNode node, ref List<TreeNode> ret)
        {
            if (node.Nodes.Count == 0)
            {
                ret.Add(node);
            }
            else
            {
                foreach (TreeNode subnode in node.Nodes)
                {
                    GetFinalNodeList(subnode, ref ret);
                }
            }
        }

        private void treeViewWeightSortNode_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SetEditMode(1);

            TreeNode node = e.Node;
            //if (node.Nodes.Count == 0)
            //{
                WeightFormula wf = (WeightFormula)node.Tag;

                if (wf == null)
                {
                    MessageBox.Show("异常错误！节点无对应公式！");
                    return;
                }
                textBoxFormula.Text = wf.Formula;

                SetParaList(wf.ParaList);

                buttonEdit.Enabled = true;
            //}
            //else
            //{
            //    listViewPara.Items.Clear();

            //    List<TreeNode> ret = new List<TreeNode>();
            //    GetFinalNodeList(node, ref ret);

            //    HashSet<WeightParameter> wpSet = new HashSet<WeightParameter>();

            //    foreach (TreeNode finalnode in ret)
            //    {
            //        WeightFormula wf = (WeightFormula)finalnode.Tag;
            //        if (wf == null)
            //        {
            //            MessageBox.Show("异常错误！节点无对应公式！");
            //            return;
            //        }
            //        foreach (WeightParameter wp in wf.ParaList)
            //        {
            //            wpSet.Add(wp);
            //        }
            //    }
            //    SetParaList(wpSet);

            //    textBoxFormula.Text = "组合节点无公式";
            //    buttonEdit.Enabled = false;
            //}
        }

        private void buttonFunction_Click(object sender, EventArgs e)
        {
            Button btnpress = (Button)sender;

            textBoxFormula.Paste(btnpress.Text);
            textBoxFormula.SelectionStart--;

            textBoxFormula.Focus();
        }

        private void buttonOperator_Click(object sender, EventArgs e)
        {
            Button btnpress = (Button)sender;

            textBoxFormula.Paste(btnpress.Text);

            textBoxFormula.Focus();
        }

        private void SetEditMode(int nstate)
        {
            groupBoxParaList.Visible = (nstate == 1);

            groupBoxFunction.Visible = (nstate == 2);
            groupBoxParaInput.Visible = (nstate == 2);

            groupBoxFormula.Visible = (nstate != 3);
            textBoxFormula.ReadOnly = (nstate == 1);

            panelApply.Visible = (nstate != 1);
            panelEdit.Visible = (nstate == 1);

        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            TreeNode node = treeViewWeightSortNode.SelectedNode;
            WeightFormula wf = (WeightFormula)node.Tag;

            dictTempPara.Clear();

            foreach (ParaData wp in wf.ParaList)
            {
                //if (IntertiaArithmetic.FindGlobleParameters(wp.ParaName, null).Count > 1)
                //{
                dictTempPara.Add(wp.paraName, wp.paraUnit);
                //}
            }

            SetEditMode(2);
            textBoxFormula.Focus();
            textBoxFormula.SelectionStart = textBoxFormula.TextLength;
        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            SetEditMode(3);
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            WeightFormula wf = null;

            string errmsg;
            ZstExpression.CExpression expr = ZstExpression.CExpression.Parse(textBoxFormula.Text, out errmsg);

            if (expr == null)
            {
                MessageBox.Show(errmsg);
                return;
            }
            textBoxFormula.Text = expr.GetExpression();

            TreeNode node = treeViewWeightSortNode.SelectedNode;
            wf = (WeightFormula)node.Tag;

            wf.Formula = textBoxFormula.Text;
            List<string> paras = new List<string>();
            expr.GetVariables(ref paras);

            wf.ParaList.Clear();

            foreach (string paraname in paras)
            {
                ParaData wp = null;
                if (dictTempPara.ContainsKey(paraname))
                {
                    wp = FindParameter(paraname, dictTempPara[paraname]);
                }
                else
                {
                    wp = FindParameter(paraname, null);
                }
                if (wp == null)
                {
                    wp = new ParaData();
                    wp.paraName = paraname;
                    wp.paraType = 10;
                }
                wf.ParaList.Add(wp);
            }

            SetParaList(wf.ParaList);
            textBoxFormula.Modified = false;
            SetEditMode(1);

            dateTimePickerLastModifyTime.Value = DateTime.Today;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            TreeNode node = treeViewWeightSortNode.SelectedNode;
            WeightFormula wf = (WeightFormula)node.Tag;
            textBoxFormula.Text = wf.Formula;
            textBoxFormula.Modified = false;
            SetEditMode(1);
        }

        private void listBoxParaType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxParaType.SelectedIndex == -1)
            {
                return;
            }
            listViewParaAll.Items.Clear();

            List<List<ParaData>> WeightParaList = ParaData.GetWeightParameterList();

            List<ParaData> wplist = WeightParaList[listBoxParaType.SelectedIndex];
            for (int i = 0; i < wplist.Count; ++i)
            {
                ListViewItem lvi = listViewParaAll.Items.Add(wplist[i].paraName);
                lvi.SubItems.Add(wplist[i].paraUnit);
                lvi.SubItems.Add(wplist[i].strRemark);
                lvi.ToolTipText = wplist[i].strRemark;
            }

            textBoxFormula.Focus();
        }

        private void listViewParaAll_DoubleClick(object sender, EventArgs e)
        {
            if (listViewParaAll.SelectedItems.Count == 1)
            {
                string paraName = listViewParaAll.SelectedItems[0].Text;

                if (dictTempPara.ContainsKey(paraName))
                {
                    string formula = textBoxFormula.Text.Substring(0, textBoxFormula.SelectionStart) + " " + textBoxFormula.Text.Substring(textBoxFormula.SelectionStart + textBoxFormula.SelectionLength);
                    string[] separator = new string[] { " ", "+", "-", "*", "/", "^", "(", ")" };
                    string[] ArrayValue = formula.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    bool bfind = false;
                    foreach (string strvalue in ArrayValue)
                    {
                        if (string.Compare(paraName, strvalue, true) == 0)
                        {
                            bfind = true;
                            break;
                        }
                    }

                    string unit = listViewParaAll.SelectedItems[0].SubItems[1].Text;
                    if (bfind)
                    {
                        if (unit != dictTempPara[paraName])
                        {
                            MessageBox.Show("与公式中的现有参数重名，但单位不一致，不能添加该参数！");
                            return;
                        }
                    }
                    else
                    {
                        dictTempPara[paraName] = unit;
                    }
                }
                else
                {
                    //if (WeightArithmetic.FindGlobleParameters(paraName, null).Count > 1)
                    //{
                        string unit = listViewParaAll.SelectedItems[0].SubItems[1].Text;
                        dictTempPara.Add(paraName, unit);
                    //}
                }

                textBoxFormula.Paste(paraName);

                textBoxFormula.Focus();
            }
        }

        public ParaData FindParameter(string name, string unit)
        {
            ParaData localwp = waData.FindParameter(name, unit);
            if (localwp != null)
            {
                return localwp;
            }

            List<List<ParaData>> WeightParaList = ParaData.GetWeightParameterList();

            for (int i = 0; i < WeightParaList.Count; ++i)
            {
                foreach (ParaData temp in WeightParaList[i])
                {
                    if (temp.paraName == name)
                    {
                        if (unit == null || (temp.paraUnit == unit))
                        {
                            return new ParaData(temp);
                        }
                    }
                }
            }

            return null;
        }

        static public bool WriteArithmeticFile(InertiaArithmetic wa, bool bOverWritePrompt)
        {
            string filepath = "InertiaArithmetic\\" + wa.Name + ".iam";

            return wa.WriteArithmeticFile(filepath, bOverWritePrompt);
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (txtName.Text == string.Empty)
            {
                MessageBox.Show("算法名称不能为空!");
                return;
            }
            else
            {
                if (Verification.IsCheckSignleString(txtName.Text))
                {
                    MessageBox.Show("算法名称包含非法字符！");
                    txtName.Focus();
                    return;
                }
            }

            if (txtRemark.Text != string.Empty)
            {
                if (Verification.IsCheckRemarkString(txtRemark.Text))
                {
                    MessageBox.Show("算法备注包含非法字符！");
                    return;
                }
            }

            List<ParaData> templistpara = waData.GetParaList();
            bool bParaPrompt = false;
            foreach (ParaData wp in templistpara)
            {
                //if (wp.ParaType == 10 && wp.ParaUnit.Length == 0 && wp.ParaRemark.Length == 0)
                if (wp.paraType == 10)
                {
                    bParaPrompt = true;
                    break;
                }
            }

            if (bParaPrompt)
            {
                if (MessageBox.Show("算法中含有未定义参数(临时参数)！\r\n保存算法前是否对这些参数进行设定？", "参数定义", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    List<ParaData> listparaforset = new List<ParaData>();
                    foreach (ParaData wp in templistpara)
                    {
                        //if (wp.ParaType == 10 && wp.ParaUnit.Length == 0 && wp.ParaRemark.Length == 0)
                        if (wp.paraType == 10)
                        {
                            listparaforset.Add(wp);
                        }
                    }
                    TempWeightParaSet form = new TempWeightParaSet(listparaforset);
                    form.ShowDialog();
                }
            }

            bool bprompt = (strType == "edit") ? false : true;

            waData.Name = txtName.Text;
            waData.CreateTime = dateTimePickerCreateTime.Text;
            waData.LastModifyTime = dateTimePickerLastModifyTime.Text;
            waData.Remark = txtRemark.Text;

            if (WriteArithmeticFile(waData, bprompt) == false)
            {
                return;
            }

            strIntertiaArithmeticFileName = waData.Name;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void txtRemark_ModifiedChanged(object sender, EventArgs e)
        {
            dateTimePickerLastModifyTime.Value = DateTime.Today;
        }


    }

}
