using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using XCommon;

namespace WeightCenterDesignAndEstimateSoft.Tool
{
    public partial class TempWeightParaSet : Form
    {
        private List<ParaData> TempParaList = null;

        public TempWeightParaSet(List<ParaData> paraList)
        {
            InitializeComponent();
            TempParaList = paraList;
            foreach(ParaData wp in TempParaList)
            {
                listViewPara.Items.Add(wp.paraName);
            }
            listViewPara.Items[0].Selected = true;
        }

        private void listViewPara_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewPara.SelectedItems.Count != 0)
            {
                ParaData wp = TempParaList[listViewPara.SelectedItems[0].Index];
                textBoxParaEnName.Text = wp.paraEnName;
                textBoxParaUnit.Text = wp.paraUnit;
                textBoxParaRemark.Text = wp.strRemark;
            }
            else
            {
                textBoxParaEnName.Text = "";
                textBoxParaUnit.Text = "";
                textBoxParaRemark.Text = "";
            }
            textBoxParaEnName.Modified = false;
            textBoxParaUnit.Modified = false;
            textBoxParaRemark.Modified = false;        
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            if (textBoxParaEnName.Modified || textBoxParaUnit.Modified || textBoxParaRemark.Modified)
            {
                if (textBoxParaEnName.Text.Contains(' ') || XCommon.Verification.IsCheckString(textBoxParaEnName.Text))
                {
                    MessageBox.Show("英文名称含有非法字符！");
                    return;
                }
                if (textBoxParaUnit.Text.Contains(' ') || XCommon.Verification.IsCheckString(textBoxParaUnit.Text))
                {
                    MessageBox.Show("单位含有非法字符！");
                    return;
                }
                if (XCommon.Verification.IsCheckRemarkString(textBoxParaRemark.Text))
                {
                    MessageBox.Show("备注含有非法字符！");
                    return;
                }

                ParaData wp = TempParaList[listViewPara.SelectedItems[0].Index];

                if (wp.paraEnName == textBoxParaEnName.Text && wp.paraUnit == textBoxParaUnit.Text && wp.strRemark == textBoxParaRemark.Text)
                {
                    return;
                }

                wp.paraEnName = textBoxParaEnName.Text;
                wp.paraUnit = textBoxParaUnit.Text;
                wp.strRemark = textBoxParaRemark.Text;

                listViewPara.SelectedItems[0].SubItems[0].Text = wp.paraName + " *";

                textBoxParaEnName.Modified = false;
                textBoxParaUnit.Modified = false;
                textBoxParaRemark.Modified = false;
            }
        }
    }
}
