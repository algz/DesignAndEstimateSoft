using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TreeDiagramControl;
using XCommon;

namespace WeightCenterDesignAndEstimateSoft.Tool.GenerateReport
{
    class Common
    {
        /// <summary>
        /// 匹配重量分类结构
        /// </summary>
        /// <param name="srcList"></param>
        /// <param name="destList"></param>
        /// <returns></returns>
        public static bool matchWeightData(List<WeightData> srcList, List<WeightData> destList)
        {
            if (srcList.Count == destList.Count)
            {
                foreach (WeightData destWD in destList)
                {
                    int j = 0;
                    for (; j < srcList.Count; j++)
                    {
                        if (srcList[j].weightName == destWD.weightName)
                        {
                            break;
                        }
                    }
                    if (j == srcList.Count)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }


        public void getFormulaData(FormulaData formulaData,WeightFormula formula)
        {
            string errmsg;
            ZstExpression.CExpression expr = ZstExpression.CExpression.Parse(formula.Formula, out errmsg);
            if (expr != null)
            {
                formulaData.FormulaDetail = new string[formula.ParaList.Count];
                for (int j = 0; j < formula.ParaList.Count; j++)
                {
                    ParaData pd = formula.ParaList[j];
                    expr.SetAliasName(pd.paraName, pd.paraEnName);
                    formulaData.FormulaDetail[j] = pd.paraEnName + " = " + pd.paraName;
                }
                formulaData.FormulaExpression = expr.GetAliasExpression();
            }
        }

        public string getWeightCategoryPic(List<WeightData> wdList)
        {
            if (wdList != null && wdList.Count != 0)
            {
                TreeData.TreeDataTable dt = new TreeData.TreeDataTable();
                dt.AddTreeDataRow("1", "", "直升机总重", "");
                for(int i=0;i<wdList.Count;i++)
                {
                    WeightData wd = wdList[i];
                    dt.AddTreeDataRow(wd.nID+"", wd.nParentID+"", wd.weightName, "");


                }
                TreeDiagram myTree = new TreeDiagram(dt);
                if (myTree != null)
                {
                    myTree.BoxWidth = 20;
                    myTree.FontOrentation = StringFormatFlags.DirectionVertical;
                    PictureBox picTree = new System.Windows.Forms.PictureBox();
                    picTree.Image = Image.FromStream(myTree.GenerateTree(-1, -1, "1", System.Drawing.Imaging.ImageFormat.Bmp));
                    picTree.Image.Save(@"c:\1.jpg",System.Drawing.Imaging.ImageFormat.Jpeg);
       
                }   
                return @"c:\1.jpg";
            }
            else
            {
                return null;
            }
        }
    }
}
