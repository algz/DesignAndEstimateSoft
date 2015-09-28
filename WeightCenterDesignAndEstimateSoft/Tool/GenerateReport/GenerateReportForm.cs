using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XCommon;
using System.IO;

namespace WeightCenterDesignAndEstimateSoft.Tool.GenerateReport
{
    public partial class GenerateReportForm : Form
    {
        public MainForm mainForm;

        /// <summary>
        /// reportDatas[0]:重量分类
        /// 
        /// reportDatas[1]:百分比估算
        /// reportDatas[2]:公式估算
        /// reportDatas[3]:优化值(修正量)
        /// 
        /// reportDatas[4]:空机重量
        /// reportDatas[5]:重心前/后限
        /// reportDatas[6]:转动惯量
        /// 
        /// reportDatas[7]:估算值(先进技术因子取值)
        /// reprotDatas[8]:实际值(先进技术因子取值)
        /// </summary>
        ReportData[][] reportDatas = new ReportData[9][];

        /// <summary>
        /// 重量公式
        /// </summary>
        FormulaData[] weightFormulaData=null; 

        /// <summary>
        /// 纵向重心公式
        /// </summary>
        FormulaData[] coreEnvelopeFormulaData=null; 

        WeightSortData weightCategory = null;
        //WeightArithmetic weightArithmetic = null;
        //WeightAdjustmentResultData weightAdjustmentResultData = null;

        public GenerateReportForm()
        {
            InitializeComponent();

            weightCategoryTreeView.HideSelection = false;
            //自已绘制
            this.weightCategoryTreeView.DrawMode = TreeViewDrawMode.OwnerDrawText;
            this.weightCategoryTreeView.DrawNode += new DrawTreeNodeEventHandler(weightCategoryTreeView_DrawNode);
        }

        private void GenerateReportForm_Load(object sender, EventArgs e)
        {
            this.mainForm = (MainForm)this.Owner;
            if (this.mainForm == null)
            {
                return;
            }

            //加载百分比表格
            addListBox(this.percentArithmeticListBox, this.mainForm.designProjectData.lstWeightArithmetic, null);
            
            //加载公式法表格
            addListBox(this.formulaArithmeticListBox, this.mainForm.designProjectData.lstWeightArithmetic, null);

            //加载重量修正表格
            addListBox(this.weightAdjustListBox, null, this.mainForm.designProjectData.lstAdjustmentResultData);


            if (this.mainForm.designProjectData != null && this.mainForm.designProjectData.lstCoreEnvelopeDesign!=null)
            {
                //绑定纵向重心(重心包线设计结果)
                foreach (CoreEnvelopeArithmetic cea in this.mainForm.designProjectData.lstCoreEnvelopeDesign)
                {
                    this.coreDesignListBox.Items.Add(cea);
                    this.coreDesignListBox.DisplayMember = "DataName";
                }
            }
            if (this.mainForm.designProjectData != null && this.mainForm.designProjectData.lstIntertiaArithmetic != null)
            {
                //绑定转动惯量(转动惯量设计结果)
                foreach (InertiaArithmetic ia in this.mainForm.designProjectData.lstIntertiaArithmetic)
                {
                    this.inertiaArithmeticListBox.Items.Add(ia);
                    this.inertiaArithmeticListBox.DisplayMember = "DataName";
                }
            }
        }


        private void weightCategoryTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level != 1)
            {
                return;
            }
            weightCategoryTreeView.SelectedNode = e.Node;
            //所选的重量分类
            TreeNode selNode = weightCategoryTreeView.SelectedNode;
            this.weightCategory = this.mainForm.designProjectData.lstWeightArithmetic[Convert.ToInt32(selNode.Tag)].ExportDataToWeightSort();
        }

        #region 自定义方法

        
        /// <summary>
        /// 增加百分比估算/公式估算/系统修正量的集合值
        /// </summary>
        /// <param name="listBox"></param>
        /// <param name="waList">重量设计</param>
        /// <param name="wardList">重量调整</param>
        /// <param name="matchWDList"> 需匹配的重量分类 </param>
        private void addListBox(ListBox listBox,List<WeightArithmetic> waList,List<WeightAdjustmentResultData> wardList,List<WeightData> matchWDList=null)
        {

            listBox.Items.Clear();

            if (waList != null)
            {
                foreach (WeightArithmetic wa in waList)
                {
                    if (matchWDList!=null&&!Common.matchWeightData(matchWDList, wa.ExportDataToWeightSort().lstWeightData))
                    {
                        continue;
                    }
                    listBox.Items.Add(wa);
                    listBox.DisplayMember = "DataName";
                }
            }
            else if (wardList != null)
            {
                foreach (WeightAdjustmentResultData ward in wardList)
                {
                    if (matchWDList != null && !Common.matchWeightData(matchWDList, ward.basicWeightData.lstWeightData))
                    {
                        continue;
                    }
                    listBox.Items.Add(ward);
                    listBox.DisplayMember = "WeightAdjustName";
                }
            }
            
        }

        
        ///// <summary>
        ///// 绑定重量结构树数据子节点
        ///// </summary>
        //private static void BindTreeNode(TreeNode ParentNode, int nParentID, List<WeightData> lstWeightData)
        //{
        //    IEnumerable<WeightData> selection = from wd in lstWeightData where wd.nParentID == nParentID select wd;
        //    foreach (WeightData wd in selection)
        //    {
        //        string strKey = ParentNode.Name + "\\" + wd.weightName;
        //        string strText = wd.weightName;// +"[" + Math.Round(wd.weightValue, digit).ToString() + " 千克" + "]";
        //        TreeNode node = ParentNode.Nodes.Add(strKey, strText);

        //        //BindTreeNode(node, wd.nID, lstWeightData);
        //    }
        //}

        #endregion 

        /// <summary>
        /// 取消按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancle_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        /// <summary>
        /// 确认按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (this.reportNameTxt.Text == "")
            {
                MessageBox.Show("名称不能为空.");
                return;
            }
            else if(this.digitBitTxt.Text=="")
            {
                MessageBox.Show("请输入有效位数.");
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Word 文档(*.docx)|*.docx";
            sfd.FilterIndex = 1;
            sfd.InitialDirectory = @"c:\";
            sfd.FileName = this.reportNameTxt.Text+".docx";
            List<WeightData> wdList =(List<WeightData>)this.weightCategoryTreeView.Tag;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (WordProcess.generateWordFile(sfd.FileName, this.reportDatas, this.weightFormulaData, this.coreEnvelopeFormulaData, Common.getWeightCategoryPic(wdList),Convert.ToInt32(this.digitBitTxt.Text)))
                {
                    MessageBox.Show("文档生成完成.");
                };

                //this.Close();
            }
            
        }


        private void weightCategoryTreeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            e.DrawDefault = true; //我这里用默认颜色即可，只需要在TreeView失去焦点时选中节点仍然突显
        }

        private void checkedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            CheckedListBox checkedListBox = (CheckedListBox)sender;

            if (e.NewValue == CheckState.Unchecked)
            {
                if (checkedListBox.CheckedItems.Count == 0 || checkedListBox.CheckedIndices[0] == e.Index)
                {
                    checkedListBox.SelectedItem = null;
                }

                if (checkedListBox.Name == "percentArithmeticListBox")
                {
                    //百分比估算法
                    this.reportDatas[1] = null;
                }
                else if (checkedListBox.Name == "formulaArithmeticListBox")
                {
                    //公式估算法
                    this.weightFormulaData = null;
                    this.reportDatas[2] = null;
                }
                else if (checkedListBox.Name == "weightAdjustListBox")
                {
                    //修正量
                    this.reportDatas[3] = null;
                }

                if (this.percentArithmeticListBox.SelectedItem == null &&
                    this.formulaArithmeticListBox.SelectedItem == null &&
                    this.weightAdjustListBox.SelectedItem == null)
                {
                    //重新初始化百分比/公式/修正量/重量分类

                    this.weightCategoryTreeView.Nodes.Clear();
                    this.weightCategoryTreeView.Tag = null;
                    this.reportDatas[0] = null;
                    this.clearEstimateBtn_Click(this.clearEstimateBtn, null);
                    this.clearEstimateBtn_Click(this.clearRealityBtn, null);
                    
                    //加载百分比表格
                    addListBox(this.percentArithmeticListBox, this.mainForm.designProjectData.lstWeightArithmetic, null);

                    //加载公式法表格
                    addListBox(this.formulaArithmeticListBox, this.mainForm.designProjectData.lstWeightArithmetic, null);

                    //加载重量修正表格
                    addListBox(this.weightAdjustListBox, null, this.mainForm.designProjectData.lstAdjustmentResultData);
                }
            }
            else
            {
                //checkedListBox.SelectedItem = null;
                foreach (int i in checkedListBox.CheckedIndices)
                {
                    checkedListBox.SetItemChecked(i, false);
                    //checkedListBox.SetSelected(i, false);
                    //checkedListBox.SetItemCheckState(i, CheckState.Unchecked); //设置单选核心代码
                }
                checkedListBox.SelectedItem = checkedListBox.Items[e.Index];
            }
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckedListBox box = (CheckedListBox)sender;


            if (box.Name == "coreDesignListBox")
            {
                //3.	纵向重心设计
                if (box.CheckedItems.Count == 0)
                {
                    this.reportDatas[5] = null;
                    return;
                }
                List<NodeFormula> nfList = ((CoreEnvelopeArithmetic)box.CheckedItems[0]).FormulaList;
                this.reportDatas[5] = new ReportData[1];
                this.reportDatas[5][0] = new ReportData();
                this.reportDatas[5][0].topMargin = this.reportDatas[5][0].bottomMargin = nfList[0].XFormula.Value.ToString();
                foreach (NodeFormula nf in nfList)
                {
                    if (nf.XFormula.Value <= Convert.ToDouble(this.reportDatas[5][0].topMargin))
                    {
                        this.reportDatas[5][0].topMargin = (Math.Round(nf.XFormula.Value,6) / 1000).ToString() ;
                    }
                    else if (nf.XFormula.Value >= Convert.ToDouble(this.reportDatas[5][0].bottomMargin))
                    {
                        this.reportDatas[5][0].bottomMargin = (Math.Round(nf.XFormula.Value,6) / 1000).ToString();
                    }
                }

            }
            else if (box.Name == "inertiaArithmeticListBox")
            {
                if (box.CheckedItems.Count == 0)
                {
                    this.reportDatas[6] = null;
                    return;
                }
                //4.	转动惯量
                List<WeightFormula> wfList = ((InertiaArithmetic)box.CheckedItems[0]).FormulaList;
                ReportData[] reportData =this.reportDatas[6]= new ReportData[wfList.Count];
                this.reportDatas[6] = new ReportData[wfList.Count];
               
                for (int j = 0; j < wfList.Count; j++)
                {
                    WeightFormula wf = wfList[j];
                    this.reportDatas[6][j] = new ReportData();
                    this.reportDatas[6][j].name = wf.NodePath;
                    this.reportDatas[6][j].inertiaValue = wf.Value.ToString();
                }
            }else
            {
                //if (percentArithmeticListBox.CheckedItems.Count != 0)
                //{
                //    wdList = ((WeightArithmetic)percentArithmeticListBox.CheckedItems[0]).ExportDataToWeightSort().lstWeightData;
                //}
                //else if (formulaArithmeticListBox.CheckedItems.Count != 0)
                //{
                //    wdList = ((WeightArithmetic)formulaArithmeticListBox.CheckedItems[0]).ExportDataToWeightSort().lstWeightData;
                //}
                //else if (weightAdjustListBox.CheckedItems.Count != 0)
                //{
                //    wdList = ((WeightAdjustmentResultData)weightAdjustListBox.CheckedItems[0]).weightAdjustData.lstWeightData;
                //}
                //绑定重量分类
                BindTreeList(weightCategoryTreeView,null);

                List<WeightData> wdList = null;
                ReportData[] reportData = null;
                if (box.Name == "percentArithmeticListBox")
                {
                    //2.1	百分比估算
                    if (box.CheckedItems.Count == 0)
                    {
                        this.reportDatas[1] = null;
                        if (this.reportDatas[1] == null && this.reportDatas[2] == null && this.reportDatas[3] == null)
                        {
                            //百分比,公式估算,重量修正都为空时,空机重量即为空.
                            this.reportDatas[4] = null;
                            this.weightCategoryTreeView.Nodes.Clear();
                        }
                        return;
                    }
                    else
                    {
                        wdList = ((WeightArithmetic)box.CheckedItems[0]).ExportDataToWeightSort().lstWeightData;
                        IEnumerable<WeightData> selection = from wd in wdList where wd.nParentID == -1 || wd.nParentID == 0 select wd;
                        wdList = selection.ToList<WeightData>();
                        reportData = this.reportDatas[1] = new ReportData[wdList.Count];
                    }

                }else if (box.Name == "formulaArithmeticListBox")
                {
                    if (box.CheckedItems.Count == 0)
                    {
                        this.reportDatas[2] = null;
                        if (this.reportDatas[1] == null && this.reportDatas[2] == null && this.reportDatas[3] == null)
                        {
                            //百分比,公式估算,重量修正都为空时,空机重量即为空.
                            this.reportDatas[4] = null;
                            this.weightCategoryTreeView.Nodes.Clear();
                        }
                        return;
                    }
                    else
                    {
                        //2.2	公式估算法   =
                        WeightArithmetic wa=(WeightArithmetic)box.CheckedItems[0];
                        //wa.Remark加载到模板中.

                        wdList = ((WeightArithmetic)box.CheckedItems[0]).ExportDataToWeightSort().lstWeightData;
                        IEnumerable<WeightData> selection = from wd in wdList where wd.nParentID == -1 || wd.nParentID == 0 select wd;
                        wdList = selection.ToList<WeightData>();
                        reportData = this.reportDatas[2] = new ReportData[wdList.Count];

                        #region 3.2.1计算公式

                        //WeightArithmetic wa = (WeightArithmetic)box.CheckedItems[0];
                        this.weightFormulaData = new FormulaData[wa.FormulaList.Count];
                        for (int i = 0; i < wa.FormulaList.Count; i++)
                        {
                            WeightFormula wf = wa.FormulaList[i];
                            this.weightFormulaData[i] = new FormulaData();
                            string[] arrStr=wf.NodePath.Split(new char[]{'\\'});
                            this.weightFormulaData[i].MainTile = arrStr.Last();

                            string errmsg;
                            ZstExpression.CExpression expr = ZstExpression.CExpression.Parse(wf.Formula, out errmsg);
                            if (expr != null)
                            {
                                this.weightFormulaData[i].FormulaDetail = new string[wf.ParaList.Count];
                                for (int j = 0; j < wf.ParaList.Count; j++)
                                {
                                    ParaData pd = wf.ParaList[j];
                                    expr.SetAliasName(pd.paraName, pd.paraEnName);
                                    this.weightFormulaData[i].FormulaDetail[j] = pd.paraEnName + " = " + pd.paraName;
                                }
                                this.weightFormulaData[i].FormulaExpression = expr.GetAliasExpression();
                            }
                        }

                        #endregion
                    }

                }
                else if (box.Name == "weightAdjustListBox")
                {
                    if (box.CheckedItems.Count == 0)
                    {
                        this.reportDatas[3] = null;
                        if (this.reportDatas[1] == null && this.reportDatas[2] == null && this.reportDatas[3] == null)
                        {
                            //百分比,公式估算,重量修正都为空时,空机重量即为空.
                            this.reportDatas[4] = null;
                            this.weightCategoryTreeView.Nodes.Clear();
                            return;
                        }
                    }
                    else
                    {
                        //2.3	系统重量修正值
                        wdList = ((WeightAdjustmentResultData)box.CheckedItems[0]).weightAdjustData.lstWeightData;
                        IEnumerable<WeightData> selection = from wd in wdList where wd.nParentID == -1 || wd.nParentID == 0 select wd;
                        wdList = selection.ToList<WeightData>();
                        reportData = this.reportDatas[3] = new ReportData[wdList.Count];
                    }
                }


                for (int j = 0; wdList != null && j < wdList.Count; j++)
                {
                    #region 2.4	重量效率(空机重量)

                    WeightData wd = wdList[j];
                    if (this.reportDatas[4] == null)
                    {
                        this.reportDatas[4] = new ReportData[1];
                        this.reportDatas[4][0] = new ReportData();
                        this.reportDatas[4][0].name = "空机重量";
                        this.reportDatas[4][0].weight = "0";
                    }
                    if (j == 0 && Convert.ToDouble(this.reportDatas[4][0].weight) < wd.weightValue)
                    {
                        this.reportDatas[4][0].weight = wd.weightValue.ToString();
                    }

                    #endregion

                    if (wd.nParentID.ToString() == "-1" || wd.nParentID.ToString() == "0")
                    {
                        reportData[j] = new ReportData();
                        reportData[j].name = wd.weightName;
                        reportData[j].weight = Math.Round(wd.weightValue,6).ToString();
                        //按百分比计算
                        reportData[j].percent = (wd.weightValue / wdList[0].weightValue*100).ToString();
                    }
                }
                
            }
        }



        /// <summary>
        /// 百分比估算
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void percentArithmeticListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckedListBox box=(CheckedListBox)sender;

            List<WeightData> wdList=this.bindWeightCategory(box);
            if (wdList == null)
            {
                return;
            }


            ReportData[] reportData = this.reportDatas[1] = new ReportData[wdList.Count];

            for (int j = 0; wdList != null && j < wdList.Count; j++)
            {
                #region 2.4	重量效率(空机重量)

                WeightData wd = wdList[j];
                if (this.reportDatas[4] == null)
                {
                    this.reportDatas[4] = new ReportData[1];
                    this.reportDatas[4][0] = new ReportData();
                    this.reportDatas[4][0].name = "空机重量";
                    this.reportDatas[4][0].weight = "0";
                }
                if (j == 0 && Convert.ToDouble(this.reportDatas[4][0].weight) < wd.weightValue)
                {
                    this.reportDatas[4][0].weight = wd.weightValue.ToString();
                }

                #endregion

                if (wd.nParentID.ToString() == "-1" || wd.nParentID.ToString() == "0")
                {
                    reportData[j] = new ReportData();
                    reportData[j].name = wd.weightName;
                    reportData[j].weight = Math.Round(wd.weightValue, 6).ToString();
                    //按百分比计算
                    reportData[j].percent = (wd.weightValue / wdList[0].weightValue * 100).ToString();
                }
            }
        }

        /// <summary>
        /// 公式估算
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void formulaArithmeticListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckedListBox box = (CheckedListBox)sender;

                List<WeightData> wdList = this.bindWeightCategory(box);
                if (wdList == null||wdList.Count==0)
                {
                    return;
                }


            ReportData[] reportData = this.reportDatas[2] = new ReportData[wdList.Count];

            for (int j = 0; wdList != null && j < wdList.Count; j++)
            {
                WeightData wd = wdList[j];

                if (wd.nParentID.ToString() == "-1" || wd.nParentID.ToString() == "0")
                {
                    reportData[j] = new ReportData();
                    reportData[j].name = wd.weightName;
                    reportData[j].weight = Math.Round(wd.weightValue, 6).ToString();
                }
            }

            #region 3.2.1计算公式

            WeightArithmetic wa = (WeightArithmetic)box.CheckedItems[0];
            reportDatas[2][0].Remark = wa.Remark;//算法备注
            
            this.weightFormulaData = new FormulaData[wa.FormulaList.Count];
            for (int i = 0; i < wa.FormulaList.Count; i++)
            {
                WeightFormula wf = wa.FormulaList[i];
                this.weightFormulaData[i] = new FormulaData();
                string[] arrStr = wf.NodePath.Split(new char[] { '\\' });
                this.weightFormulaData[i].MainTile = arrStr.Last();

                string errmsg;
                ZstExpression.CExpression expr = ZstExpression.CExpression.Parse(wf.Formula, out errmsg);
                if (expr != null)
                {
                    this.weightFormulaData[i].FormulaDetail = new string[wf.ParaList.Count];
                    for (int j = 0; j < wf.ParaList.Count; j++)
                    {
                        ParaData pd = wf.ParaList[j];
                        expr.SetAliasName(pd.paraName, pd.paraEnName);
                        this.weightFormulaData[i].FormulaDetail[j] = pd.paraEnName + " = " + pd.paraName;
                    }
                    this.weightFormulaData[i].FormulaExpression = expr.GetAliasExpression();
                }
            }

            #endregion
        }

        /// <summary>
        /// 优化值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void weightAdjustListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckedListBox box = (CheckedListBox)sender;

            List<WeightData> wdList = this.bindWeightCategory(box);
            if (wdList == null)
            {
                return;
            }


            ReportData[] reportData = this.reportDatas[3] = new ReportData[wdList.Count];

            for (int j = 0; wdList != null && j < wdList.Count; j++)
            {
                WeightData wd = wdList[j];

                if (wd.nParentID.ToString() == "-1" || wd.nParentID.ToString() == "0")
                {
                    reportData[j] = new ReportData();
                    reportData[j].name = wd.weightName;
                    reportData[j].weight = Math.Round(wd.weightValue, 6).ToString();
                }
            }
        }

        /// <summary>
        /// 纵向重心
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void coreDesignListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckedListBox box = (CheckedListBox)sender;

            if (box.CheckedItems.Count == 0)
            {
                this.reportDatas[5] = null;
                return;
            }
            CoreEnvelopeArithmetic cea=(CoreEnvelopeArithmetic)box.CheckedItems[0];
            List<NodeFormula> nfList = cea.FormulaList;
            this.coreEnvelopeFormulaData = new FormulaData[nfList.Count];
            this.reportDatas[5] = new ReportData[1];
            this.reportDatas[5][0] = new ReportData();
            this.reportDatas[5][0].topMargin = this.reportDatas[5][0].bottomMargin = nfList[0].XFormula.Value.ToString();
            this.reportDatas[5][0].Remark = cea.Remark;//公式算法
            for (int i = 0; i < nfList.Count;i++ )
            {
                NodeFormula nf = nfList[i];
                this.coreEnvelopeFormulaData[i] = new FormulaData();
                this.coreEnvelopeFormulaData[i].MainTile = nf.NodeName;
                string errmsg;
                ZstExpression.CExpression expr = ZstExpression.CExpression.Parse(nf.XFormula.Formula, out errmsg);
                if (expr != null)
                {
                    this.coreEnvelopeFormulaData[i].FormulaDetail = new string[nf.XFormula.ParaList.Count];
                    for (int j = 0; j < nf.XFormula.ParaList.Count; j++)
                    {
                        ParaData pd = nf.XFormula.ParaList[j];
                        expr.SetAliasName(pd.paraName, pd.paraEnName);
                        this.coreEnvelopeFormulaData[i].FormulaDetail[j] = pd.paraEnName + " = " + pd.paraName;
                    }
                    this.coreEnvelopeFormulaData[i].FormulaExpression = expr.GetAliasExpression();
                }


                if (nf.XFormula.Value <= Convert.ToDouble(this.reportDatas[5][0].topMargin))
                {
                    this.reportDatas[5][0].topMargin = (Math.Round(nf.XFormula.Value, 6) / 1000).ToString();
                }
                else if (nf.XFormula.Value >= Convert.ToDouble(this.reportDatas[5][0].bottomMargin))
                {
                    this.reportDatas[5][0].bottomMargin = (Math.Round(nf.XFormula.Value, 6) / 1000).ToString();
                }
            }
        }

        /// <summary>
        /// 转动惯量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inertiaArithmeticListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckedListBox box = (CheckedListBox)sender;
            if (box.CheckedItems.Count == 0)
            {
                this.reportDatas[6] = null;
                return;
            }

            //List<WeightFormula> wfList = ((InertiaArithmetic)box.CheckedItems[0]).FormulaList;
            ReportData[] reportData = this.reportDatas[6] = new ReportData[box.CheckedItems.Count];
            this.reportDatas[6] = new ReportData[box.CheckedItems.Count];
            for (int i = 0; i < box.CheckedItems.Count; i++)
            {
                InertiaArithmetic ia= (InertiaArithmetic)box.CheckedItems[i];
            
                this.reportDatas[6][i] = new ReportData();
                this.reportDatas[6][i].name =ia.DataName ;
                this.reportDatas[6][i].Remark = ia.Remark;//算法备注
                this.reportDatas[6][i].formulaContent = new FormulaData[ia.FormulaList.Count];
                for (int j = 0; j < ia.FormulaList.Count; j++)
                {
                    WeightFormula wf = ia.FormulaList[j];
                    this.reportDatas[6][i].formulaContent[j] = new FormulaData();
                    this.reportDatas[6][i].formulaContent[j].MainTile = wf.NodePath;
                    this.reportDatas[6][i].formulaContent[j].FormulaValue = wf.Value.ToString();
                 }
            }
        }

        /// <summary>
        /// 导入估算值(先进技术因子)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importEstimateBtn_Click(object sender, EventArgs e)
        {
            if (reportDatas[0] == null)
            {
                return;
            }

            Button btn = (Button)sender;

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "xml文件 (*.xml)|*.xml|Excle文件 (*.xls)|*.xls|All files (*.*)|*.*";
            fileDialog.RestoreDirectory = true;
            fileDialog.FilterIndex = 1;

            //获取重量分类
            WeightSortData sortData = null;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string strFilePath = fileDialog.FileName;
                

                if (strFilePath.EndsWith(".xls"))
                {
                    sortData = WeightSortData.GetXlsImportSortData(strFilePath);
                }
                else if (strFilePath.EndsWith(".xml"))
                {
                    sortData = WeightSortData.GetXmlImporSortData(strFilePath);
                }
                else
                {
                    XLog.Write("导入文件\"" + strFilePath + "\"格式错误");
                    MessageBox.Show("导入文件\"" + strFilePath + "\"格式错误");
                    return;
                }

                if (sortData == null)
                {
                    return;
                }
                else if (sortData != null && sortData.lstWeightData.Count == 0)
                {
                    MessageBox.Show("导入文件\"" + strFilePath + "\"没有数据");
                    XLog.Write("导入文件\"" + strFilePath + "\"没有数据");
                    return;
                }
                else
                {
                    List<WeightData> wdList = sortData.lstWeightData;
                    List<WeightData> matchWDList = (List<WeightData>)this.weightCategoryTreeView.Tag;
                    
                    if (wdList != null && !Common.matchWeightData(matchWDList, wdList))
                    {
                        MessageBox.Show("重量数据不匹配");
                    }
                    else
                    {
                        //IEnumerable<WeightData> selection = from wd in wdList where wd.nParentID == -1 || wd.nParentID == 0 select wd;
                        //wdList = selection.ToList<WeightData>(); //重新转换wdList
                       ComputeCorrectionFactorFrm ccf= new ComputeCorrectionFactorFrm();
                        wdList = ccf.GetListWeightData(sortData);

                        ReportData[] temReportDatas=null;
                        if (btn.Name == "importEstimateBtn")
                        {
                            temReportDatas= this.reportDatas[7] = new ReportData[wdList.Count];
                            this.importEstimateTxt.Text = Path.GetFileName(strFilePath);
                        }
                        else if (btn.Name == "importRealityBtn")
                        {
                            temReportDatas = this.reportDatas[8] = new ReportData[wdList.Count];
                            this.importRealityTxt.Text = Path.GetFileName(strFilePath);
                        }
                        
                        for(int i=0;i<wdList.Count;i++)
                        {
                            temReportDatas[i] = new ReportData();
                            temReportDatas[i].name = wdList[i].weightName;
                            temReportDatas[i].weight = wdList[i].weightValue.ToString();
                        }
                    }

                }
                
            }
        }

        /// <summary>
        /// 清空导入数据值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearEstimateBtn_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Name == "clearEstimateBtn")
            {
                this.importEstimateTxt.Text = "";
                this.reportDatas[7] = null;
            }
            else if (btn.Name == "clearRealityBtn")
            {
                this.importRealityTxt.Text = "";
                this.reportDatas[8] = null;
            }

        }



        #region 绑定重量分类结构树

        /// <summary>
        /// 绑定分类重量
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        private List<WeightData> bindWeightCategory(CheckedListBox box)
        {
            if (box.CheckedItems.Count == 0)
            {
                return null;
            }

            string dataName = null;
            List<WeightData> wdList = null;
            if (box.Name == "weightAdjustListBox")
            {
                dataName = ((WeightAdjustmentResultData)box.CheckedItems[0]).WeightAdjustName;
                wdList = ((WeightAdjustmentResultData)box.CheckedItems[0]).weightAdjustData.lstWeightData;
            }
            else
            {
                dataName = ((WeightArithmetic)box.CheckedItems[0]).DataName;
                wdList = ((WeightArithmetic)box.CheckedItems[0]).ExportDataToWeightSort().lstWeightData;
            }


            if (this.weightCategoryTreeView.Nodes.Count == 0)
            {
                //this.weightCategory = wdList;
                //加载百分比表格
                addListBox(this.percentArithmeticListBox, this.mainForm.designProjectData.lstWeightArithmetic, null, wdList);

                //加载公式法表格
                addListBox(this.formulaArithmeticListBox, this.mainForm.designProjectData.lstWeightArithmetic, null, wdList);

                //加载重量修正表格
                addListBox(this.weightAdjustListBox, null, this.mainForm.designProjectData.lstAdjustmentResultData, wdList);

            }

            //还原CheckBox
            for (int i = 0; i < box.Items.Count; i++)
            {
                string s = box.Items[i].ToString();
                if (box.Name == "weightAdjustListBox")
                {
                    if (((WeightAdjustmentResultData)box.Items[i]).WeightAdjustName == dataName)
                    {
                        box.SetItemChecked(i, true);
                        break;
                    }
                }
                else
                {
                    if (((WeightArithmetic)box.Items[i]).DataName == dataName)
                    {
                        box.SetItemChecked(i, true);
                        break;
                    }
                }

            }


            //绑定重量分类
            if (!BindTreeList(this.weightCategoryTreeView, wdList))
            {
                MessageBox.Show("数据不匹配,绑定失败.");
                return null;
            }

            IEnumerable<WeightData> selection = from wd in wdList where wd.nParentID == -1 || wd.nParentID == 0 select wd;
            wdList = selection.ToList<WeightData>(); //重新转换wdList
            return wdList;
        }

        /// <summary>
        /// 绑定重量结构树数据子节点
        /// </summary>
        private void BindTreeNode(TreeNode ParentNode, int nParentID, List<WeightData> wdList)
        {
            IEnumerable<WeightData> selection = from wd in wdList where wd.nParentID == nParentID select wd;
            foreach (WeightData wd in selection)
            {
                TreeNode node = ParentNode.Nodes.Add(ParentNode.Name + "\\" + wd.weightName, wd.weightName);
                node.ToolTipText = wd.strRemark;

                BindTreeNode(node, wd.nID, wdList);
            }
        }

        /// <summary>
        /// 绑定重量分类结构树数据
        /// </summary>
        private bool BindTreeList(TreeView tree, List<WeightData> wdList)
        {

            if (wdList == null)
            {
                this.reportDatas[0] = null;
                return false;
            }

            if (tree.Nodes.Count == 0)
            {
                //窗口加载树结点
                tree.Nodes.Clear();
                //TreeNode rootNode = new TreeNode("重量分类");
                //tree.Nodes.Add(rootNode);
                IEnumerable<WeightData> selection = from wd in wdList where wd.nParentID == -1 select wd;
                foreach (WeightData wd in selection)
                {
                    TreeNode node = tree.Nodes.Add(wd.weightName, wd.weightName);
                    node.ToolTipText = wd.strRemark;

                    BindTreeNode(node, wd.nID, wdList);
                }
                tree.Tag = wdList;
                tree.ExpandAll();

                selection = from wd in wdList where wd.nParentID == -1 || wd.nParentID == 0 select wd;
                this.reportDatas[0] = new ReportData[selection.Count()];

                int j = 0;
                foreach (WeightData wd in selection)
                {
                    this.reportDatas[0][j] = new ReportData();
                    this.reportDatas[0][j].name = wd.weightName;
                    j++;
                    //TreeNode node = new TreeNode();
                    //node.Name = wd.weightName;
                    //node.Text = wd.weightName;// +"[" + Math.Round(wd.weightValue, digit).ToString() + " 千克" + "]";

                    //if (wd.nParentID == -1)
                    //{
                    //    rootNode.Nodes.Add(node);
                    //}
                    //else
                    //{
                    //    rootNode.Nodes[0].Nodes.Add(node);
                    //}

                }
                //tree.Tag = wdList;
                //tree.ExpandAll();
            }
            else
            {
                //重量分类结构树不为空,则匹配数据判断.

                if (!Common.matchWeightData((List<WeightData>)(tree.Tag), wdList))
                {
                    //MessageBox.Show("重量分类不匹配.");
                    return false;
                }
            }
            return true;

        }


        #endregion









    }
}
