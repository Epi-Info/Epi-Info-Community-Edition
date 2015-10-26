using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Epi.Fields;
using EpiDashboard;
using EpiDashboard.Controls;
using EpiDashboard.Gadgets;
using EpiDashboard.Gadgets.Charting;

namespace EpiDashboard.Gadgets.Charting
{
    public class ColumnChartGadgetBase : ChartGadgetBase
    {
        public Controls.Charting.IChart SelectedChart { get; set; }

        protected object syncLockData = new object();

        protected delegate void SetChartDataDelegate(List<XYColumnChartData> dataList, Strata strata);

        protected virtual void SetChartData(List<XYColumnChartData> dataList, Strata strata)
        {
        }

        protected override void Construct()
        {
            object element = this.FindName("txtLegendFontSize");
            if (element != null && element is TextBox)
            {
                TextBox textBox = element as TextBox;
                textBox.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            }

            element = this.FindName("txtHeight");
            if (element != null && element is TextBox)
            {
                TextBox textBox = element as TextBox;
                textBox.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            }

            element = this.FindName("txtWidth");
            if (element != null && element is TextBox)
            {
                TextBox textBox = element as TextBox;
                textBox.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            }

            element = this.FindName("txtTransTop");
            if (element != null && element is TextBox)
            {
                TextBox textBox = element as TextBox;
                textBox.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            }

            element = this.FindName("txtTransBottom");
            if (element != null && element is TextBox)
            {
                TextBox textBox = element as TextBox;
                textBox.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            }

            element = this.FindName("txtXAxisAngle");
            if (element != null && element is TextBox)
            {
                TextBox textBox = element as TextBox;
                textBox.PreviewKeyDown += new KeyEventHandler(txtInput_IntegerOnly_PreviewKeyDown);
            }

            element = this.FindName("expanderAdvancedOptions");
            if (element != null && element is Expander)
            {
                Expander expanderAdvancedOptions = element as Expander;
                expanderAdvancedOptions.Header = DashboardSharedStrings.GADGET_ADVANCED_OPTIONS;
            }

            element = this.FindName("expanderDisplayOptions");
            if (element != null && element is Expander)
            {
                Expander expanderDisplayOptions = element as Expander;
                expanderDisplayOptions.Header = DashboardSharedStrings.GADGET_DISPLAY_OPTIONS;
            }

            base.Construct();
        }
        
        protected virtual bool GenerateColumnChartData(Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables, Strata strata = null)
        {
            int missingValueCount = 0;
            
            lock (syncLockData)
            {
                ColumnChartParameters chtParameters = (ColumnChartParameters)Parameters;

                string second_y_var = string.Empty;

                Y2Type y2type = Y2Type.None;

                if (chtParameters.ColumnNames.Count > 1 && !String.IsNullOrEmpty(chtParameters.ColumnNames[1]))
                {
                    second_y_var = chtParameters.ColumnNames[1];
                }

                if (chtParameters.Y2AxisType == 2)
                {
                    y2type = Y2Type.RatePer100kPop;
                }
                else if (chtParameters.Y2AxisType == 3)
                {
                    y2type = Y2Type.CumulativePercent;
                }

                List<XYColumnChartData> dataList = new List<XYColumnChartData>();

                foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> tableKvp in stratifiedFrequencyTables)
                {
                    double count = 0;
                    foreach (DescriptiveStatistics ds in tableKvp.Value)
                    {
                        count = count + ds.observations;
                    }

                    // If there is only one table and the total for that table is zero, then no data can be displayed and thus no chart can be generated.
                    // Show a message to the user to this effect so they don't wonder why they're seeing a blank gadget.

                    // Commented out for now because of scenarios where the "One chart for each value of" option is used, and we're unsure how to handle
                    // showing this message in that case.

                    //if (count == 0 && stratifiedFrequencyTables.Count == 1)
                    //{
                    //    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), DashboardSharedStrings.GADGET_MSG_NO_DATA);
                    //    return false;
                    //}

                    string strataValue = tableKvp.Key.TableName;
                    DataTable table = tableKvp.Key;

                    double cumulative_percent = 0;

                    foreach (DataRow row in table.Rows)
                    {
                        XYColumnChartData chartData = new XYColumnChartData();
                        chartData.X = strataValue;
                        chartData.Y = (double)row[1];

                        if (y2type != Y2Type.None)
                        {
                            foreach (DataRow dRow in DashboardHelper.DataSet.Tables[0].Rows)
                            {
                                if (row[0].ToString().Equals(dRow[chtParameters.ColumnNames[0]].ToString()) && (y2type == Y2Type.CumulativePercent || dRow[second_y_var] != DBNull.Value))
                                {
                                    if (y2type == Y2Type.RatePer100kPop)
                                    {
                                        chartData.Y2 = chartData.Y / (Convert.ToDouble(dRow[second_y_var]) / 100000);
                                    }
                                    else if (y2type == Y2Type.CumulativePercent)
                                    {
                                        chartData.Y2 = cumulative_percent + (chartData.Y / count);
                                        cumulative_percent = chartData.Y2.Value;
                                    }
                                    else
                                    {
                                        chartData.Y2 = Convert.ToDouble(dRow[second_y_var]);
                                    }
                                    break;
                                }
                            }
                        }

                        else if (y2type == Y2Type.CumulativePercent)
                        {
                            foreach (DataRow dRow in DashboardHelper.DataSet.Tables[0].Rows)
                            {
                            }

                        }

                        chartData.S = row[0];

                        string valueString = chartData.S.ToString().Trim();

                        if (chartData.S == null || string.IsNullOrEmpty(valueString))
                        {
                            chartData.S = Config.Settings.RepresentationOfMissing;
                            missingValueCount++;
                        }
                        dataList.Add(chartData);
                    }

                    dataList.RemoveAll(ValueMissing);
                    dataList.RemoveAll(OutsideLimits);
                    dataList.Sort(new ChartDataComparer());
                }
                this.Dispatcher.BeginInvoke(new SetChartDataDelegate(SetChartData), dataList, strata);
            }

            return true;
        }

        private bool OutsideLimits(XYColumnChartData chartData)
        {
            return OutsideLimits(chartData.S);
        }

        private bool OutsideLimits(object value)
        {
            ColumnChartParameters chtParameters = (ColumnChartParameters)Parameters;

            bool isOutsidelimits = false;

            if (value is IConvertible == false)
            {
                return false;
            }

            Type referenceValueType = value.GetType();

            object xAxisStart = null;
            object xAxisEnd = null;

            Convert.ChangeType(value, referenceValueType);

            dynamic referenceValue = value;
            Type dynamicType = referenceValue.GetType();
            System.Reflection.MethodInfo methodInfo = dynamicType.GetMethod("Parse", new Type[] { typeof(string)});

            if(methodInfo != null)
            {
                object classInstance = Activator.CreateInstance(referenceValueType, null);
                try
                {
                    if (string.IsNullOrEmpty(chtParameters.XAxisStart) == false)
                    {
                        xAxisStart = methodInfo.Invoke(classInstance, new object[] { chtParameters.XAxisStart });
                        if (referenceValue.CompareTo(xAxisStart) < 0)
                        {
                            isOutsidelimits = true;
                        }
                    }
                }
                catch { }

                try
                {
                    if (string.IsNullOrEmpty(chtParameters.XAxisEnd) == false)
                    {
                        xAxisEnd = methodInfo.Invoke(classInstance, new object[] { chtParameters.XAxisEnd });
                        if (referenceValue.CompareTo(xAxisEnd) > 0)
                        {
                            isOutsidelimits = true;
                        }
                    }
                }
                catch { }
            }
            else if (referenceValueType.Name == "String")
            {
                if (string.IsNullOrEmpty(chtParameters.XAxisStart) == false)
                {
                    if (referenceValue.CompareTo(chtParameters.XAxisStart) < 0)
                    {
                        isOutsidelimits = true;
                    }
                }

                if (string.IsNullOrEmpty(chtParameters.XAxisEnd) == false)
                {
                    if (referenceValue.CompareTo(chtParameters.XAxisEnd) > 0)
                    {
                        isOutsidelimits = true;
                    }
                }
            }

            return isOutsidelimits;
        } 
        
        private bool ValueMissing(XYColumnChartData chartData)
        {
            return (chartData.S.ToString() == Config.Settings.RepresentationOfMissing);
        }
    }

    public class ChartDataComparer : System.Collections.Generic.IComparer<XYColumnChartData>
    {
        public int Compare(XYColumnChartData dataOne, XYColumnChartData dataTwo)
        {
            int returnValue;
            string compareType;
            Type datType = dataOne.S.GetType(); // dpb should come from a metadata
            compareType = datType.ToString();

            switch (compareType)
            {
                case "System.DateTime":
                    returnValue = ((DateTime)dataOne.S).CompareTo((DateTime)dataTwo.S);
                    break;
                case "System.String":
                    returnValue = ((string)dataOne.S).CompareTo((string)dataTwo.S);
                    break;
                case "System.Single":
                    returnValue = ((Single)dataOne.S).CompareTo((Single)dataTwo.S);
                    break;
                case "System.Double":
                    returnValue = ((double)dataOne.S).CompareTo((double)dataTwo.S);
                    break;
                case "System.Decimal":
                    returnValue = ((decimal)dataOne.S).CompareTo((decimal)dataTwo.S);
                    break;
                default:
                    returnValue = ((double)dataOne.S).CompareTo((double)dataTwo.S);
                    break;
            }

            return returnValue;
        }
    }
}
