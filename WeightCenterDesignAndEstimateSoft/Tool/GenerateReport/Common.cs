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
        //应用程序路径
        public static string AppPath = System.AppDomain.CurrentDomain.BaseDirectory;

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

        public static string getWeightCategoryPic(List<WeightData> wdList)
        {
            string picPath=Common.AppPath+@"Pic\weightCategory.jpg";

            if (wdList != null && wdList.Count != 0)
            {
                TreeData.TreeDataTable dt = new TreeData.TreeDataTable();
                //dt.AddTreeDataRow("1", "", "直升机总重", "");
                for(int i=0;i<wdList.Count;i++)
                {
                    WeightData wd = wdList[i];
                    dt.AddTreeDataRow((wd.nID+1)+"", (wd.nParentID==-1?"":(wd.nParentID+1).ToString()), wd.weightName, "");


                }
                TreeDiagram myTree = new TreeDiagram(dt);
                if (myTree != null)
                {
                    int length = 0;
                    foreach (WeightData wd in wdList)
                    {
                        if (length < wd.weightName.Length)
                        {
                            length = wd.weightName.Length;
                        }
                    }
                    myTree.BoxHeight = length*15;
                    myTree.BoxWidth = 20;
                    myTree.FontOrentation = StringFormatFlags.DirectionVertical;
                    PictureBox picTree = new System.Windows.Forms.PictureBox();
                    picTree.Image = Image.FromStream(myTree.GenerateTree(-1, -1, "1", System.Drawing.Imaging.ImageFormat.Bmp));
                    picTree.Image.Save(picPath, System.Drawing.Imaging.ImageFormat.Jpeg);
       
                }
                return picPath;
            }
            else
            {
                return null;
            }
        }


        //public void getFormulaData(FormulaData formulaData,WeightFormula formula)
        //{
        //    string errmsg;
        //    ZstExpression.CExpression expr = ZstExpression.CExpression.Parse(formula.Formula, out errmsg);
        //    if (expr != null)
        //    {
        //        formulaData.FormulaDetail = new string[formula.ParaList.Count];
        //        for (int j = 0; j < formula.ParaList.Count; j++)
        //        {
        //            ParaData pd = formula.ParaList[j];
        //            expr.SetAliasName(pd.paraName, pd.paraEnName);
        //            formulaData.FormulaDetail[j] = pd.paraEnName + " = " + pd.paraName;
        //        }
        //        formulaData.FormulaExpression = expr.GetAliasExpression();
        //    }
        //}

    }
}
