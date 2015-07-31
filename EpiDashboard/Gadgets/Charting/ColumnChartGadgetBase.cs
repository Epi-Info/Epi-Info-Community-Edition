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
                        if (chartData.S == null || string.IsNullOrEmpty(chartData.S.ToString().Trim()))
                        {
                            chartData.S = Config.Settings.RepresentationOfMissing;
                        }
                        dataList.Add(chartData);
                    }

                    //---
                       var query = from chartData in dataList
                                orderby chartData.S ascending
                                select chartData;

                       XYColumnChartData firstObj = query.First();
                       XYColumnChartData lastObj = query.Last();
                      
                           if (chtParameters.XAxisStart > 0 &&
                               (table.Columns[0].DataType.ToString().Equals("System.Single") ||
                               table.Columns[0].DataType.ToString().Equals("System.Double") ||
                               table.Columns[0].DataType.ToString().Equals("System.Decimal")))
                           {
                               XYColumnChartData fillerFirst = new XYColumnChartData();
                               fillerFirst.Y = 0;
                               fillerFirst.X = strataValue;
                               fillerFirst.S = chtParameters.XAxisStart;
                               dataList.Add(fillerFirst);

                               List<XYColumnChartData> dataListtoRemove = new List<XYColumnChartData>();
                               foreach (XYColumnChartData xyc in dataList)
                               {
                                   XYColumnChartData columnchartdatalistitem = new XYColumnChartData();
                                   columnchartdatalistitem.X = xyc.X;
                                   columnchartdatalistitem.Y = xyc.Y;
                                   columnchartdatalistitem.S = xyc.S;
                                   double svalue = Convert.ToDouble(columnchartdatalistitem.S);
                                   if (svalue < chtParameters.XAxisStart)
                                   { dataListtoRemove.Add(xyc); }
                               }
                               foreach (XYColumnChartData xyc in dataListtoRemove)
                               {
                                   if (dataList.Contains(xyc))
                                   { dataList.Remove(xyc); }
                               }
                           }

                           if (chtParameters.XAxisEnd > 0 &&
                               (table.Columns[0].DataType.ToString().Equals("System.Single") ||
                               table.Columns[0].DataType.ToString().Equals("System.Double") ||
                               table.Columns[0].DataType.ToString().Equals("System.Decimal")))
                           {
                               XYColumnChartData fillerLast = new XYColumnChartData();
                               fillerLast.Y = 0;
                               fillerLast.X = strataValue;
                               fillerLast.S = chtParameters.XAxisEnd;
                               dataList.Add(fillerLast);
                               List<XYColumnChartData> dataListtoRemove = new List<XYColumnChartData>();
                               foreach (XYColumnChartData xyc in dataList)
                               {
                                   XYColumnChartData columnchartdatalistitem = new XYColumnChartData();
                                   columnchartdatalistitem.X = xyc.X;
                                   columnchartdatalistitem.Y = xyc.Y;
                                   columnchartdatalistitem.S = xyc.S;
                                   double svalue = Convert.ToDouble(columnchartdatalistitem.S);
                                   if (svalue > chtParameters.XAxisEnd)
                                   { dataListtoRemove.Add(xyc); }
                               }
                               foreach (XYColumnChartData xyc in dataListtoRemove)
                               {
                                   if (dataList.Contains(xyc))
                                   { dataList.Remove(xyc); }
                               }
                           }
                       
                    //-- 
                }
                this.Dispatcher.BeginInvoke(new SetChartDataDelegate(SetChartData), dataList, strata);
            }

            return true;
        }
    }
}
