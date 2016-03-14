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
    public class ChartGadgetBase : GadgetBase, IChartGadget
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

        protected void cmbXAxisLabelType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MOVED TO INDIVIDUAL CHART PROPERTIES PANELS
            //LEFT SHELL TO ENABLE OTHER CHARTS TO COMPILE
        //    TextBox txtXAxisLabelValue = this.FindName("txtXAxisLabelValue") as TextBox;
        //    ComboBox cmbXAxisLabelType = this.FindName("cmbXAxisLabelType") as ComboBox;

        //    if (LoadingCombos || txtXAxisLabelValue == null) return;
        //    SetXAxisLabelControls();
        }

        protected void SetXAxisLabelControls()
        {
            //MOVED TO INDIVIDUAL CHART PROPERTIES PANELS
            //LEFT SHELL TO ENABLE OTHER CHARTS TO COMPILE

        //    XAxisLabelType xAxisLabelType = ((ChartParametersBase)Parameters).XAxisLabelType;
        //    //TextBox txtXAxisLabelValue = this.FindName("txtXAxisLabelValue") as TextBox;
        //    //ComboBox cmbXAxisLabelType = this.FindName("cmbXAxisLabelType") as ComboBox;

        //    object element = this.FindName("cmbXAxisLabelType");


        //    switch (cmbXAxisLabelType.SelectedIndex)
        //    {
        //        case 3:
        //            txtXAxisLabelValue.IsEnabled = true;
        //            break;
        //        case 0:
        //        case 1:
        //        case 2:
        //            txtXAxisLabelValue.IsEnabled = false;
        //            txtXAxisLabelValue.Text = string.Empty;
        //            break;
        //    }
        }

        protected override void CopyToClipboard()
        {
            object el = this.FindName("panelMain");
            if (el is StackPanel)
            {
                StringBuilder sb = new StringBuilder();

                foreach(UIElement element in (el as StackPanel).Children) 
                {
                    if (element is Controls.Charting.IChart)
                    {
                        sb.AppendLine((element as Controls.Charting.IChart).SendDataToString());
                    }
                }

                Clipboard.Clear();
                Clipboard.SetText(sb.ToString());
            }
        }

        protected virtual void cmbField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MOVED TO INDIVIDUAL CHART PROPERTIES PANELS
            //LEFT SHELL TO ENABLE OTHER CHARTS TO COMPILE

            //if(sender is ComboBox) 
            //{
            //    ComboBox cmbField = sender as ComboBox;

            //    CheckBox checkboxAllValues = null;
            //    CheckBox checkboxCommentLegalLabels = null;

            //    object element = this.FindName("checkboxAllValues");
            //    if(element != null && element is CheckBox) 
            //    {
            //        checkboxAllValues = element as CheckBox;
            //    }

            //    element = this.FindName("checkboxCommentLegalLabels");
            //    if(element != null && element is CheckBox) 
            //    {
            //        checkboxCommentLegalLabels = element as CheckBox;
            //    }

            //    if (cmbField.SelectedIndex >= 0)
            //    {
            //        Field field = DashboardHelper.GetAssociatedField(cmbField.SelectedItem.ToString());
            //        if (field != null && field is RenderableField)
            //        {
            //            FieldFlags flags = SetFieldFlags(field as RenderableField);

            //            if (checkboxAllValues != null)
            //            {
            //                if (flags.IsDropDownListField || flags.IsRecodedField)
            //                {
            //                    checkboxAllValues.IsEnabled = true;
            //                }
            //                else
            //                {
            //                    checkboxAllValues.IsEnabled = false;
            //                    checkboxAllValues.IsChecked = false;
            //                }
            //            }

            //            if (checkboxCommentLegalLabels != null)
            //            {
            //                if (flags.IsCommentLegalField || flags.IsOptionField)
            //                {
            //                    checkboxCommentLegalLabels.IsEnabled = true;
            //                }
            //                else
            //                {
            //                    checkboxCommentLegalLabels.IsEnabled = false;
            //                    checkboxCommentLegalLabels.IsChecked = false;
            //                }

            //                if (!flags.IsCommentLegalField && !flags.IsOptionField)
            //                {
            //                    checkboxCommentLegalLabels.IsChecked = flags.IsCommentLegalField;
            //                }
            //            }
            //        }
            //    }
            //}
        }

        protected void chart_MouseLeave(object sender, MouseEventArgs e)
        {
            //SelectedChart = null;
        }

        protected void chart_MouseEnter(object sender, MouseEventArgs e)
        {
            //object element = this.FindName("gadgetContextMenu");
            //if ((element is ContextMenu) && !(element as ContextMenu).IsOpen)
            //{
            //    SelectedChart = sender as Controls.Charting.IChart;
            //}
            //else
            //{
            //    SelectedChart = null;
            //}
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false)
        {
            TextBox txtChartTitle = null;
            StackPanel panelMain = null;

            object el = FindName("txtChartTitle");
            if (el is TextBox)
            {
                txtChartTitle = el as TextBox;
            }

            el = FindName("panelMain");
            if (el is StackPanel)
            {
                panelMain = el as StackPanel;
            }

            if (txtChartTitle == null || panelMain == null) return string.Empty;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<h2>" + txtChartTitle.Text + "</h2>");

            foreach (UIElement element in panelMain.Children)
            {
                if (element is EpiDashboard.Controls.Charting.IChart)
                {
                    sb.AppendLine(((EpiDashboard.Controls.Charting.IChart)element).ToHTML(htmlFileName, count, true, false));
                }
            }

            return sb.ToString();
        }

        protected enum Y2Type
        {
            None,
            SingleField,
            RatePer100kPop,
            CumulativePercent
        }

        protected virtual void GenerateChartData(Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables, Strata strata = null)
        {
            lock (syncLockData)
            {
                string second_y_var = string.Empty;

                Y2Type y2type = Y2Type.None;

                ///ToDo: Remove GadgetOptions code when Parameters obj fully implemented. //////////////////////////////////////
                //if (GadgetOptions.InputVariableList.ContainsKey("second_y_var"))
                //{
                //    second_y_var = GadgetOptions.InputVariableList["second_y_var"];
                //}
                //if (GadgetOptions.InputVariableList.ContainsKey("second_y_var_type") && GadgetOptions.InputVariableList["second_y_var_type"].Equals("rate_per_100k"))
                //{
                //    y2type = Y2Type.RatePer100kPop;
                //}
                //else if (GadgetOptions.InputVariableList.ContainsKey("second_y_var_type") && GadgetOptions.InputVariableList["second_y_var_type"].Equals("cumulative_percent"))
                //{
                //    y2type = Y2Type.CumulativePercent;
                //}
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                switch ((Parameters as IChartGadgetParameters).Y2AxisType)
                {
                    case 1:
                        y2type = Y2Type.SingleField;//EI-430
                        if (Parameters.ColumnNames.Count > 1 && !String.IsNullOrEmpty(Parameters.ColumnNames[1]))
                        {
                            second_y_var = Parameters.ColumnNames[1];
                        }
                        break;
                    case 2:
                        y2type = Y2Type.RatePer100kPop;
                        if (Parameters.ColumnNames.Count > 1 && !String.IsNullOrEmpty(Parameters.ColumnNames[1]))
                        {
                            second_y_var = Parameters.ColumnNames[1];
                        }
                        break;
                    case 3:
                        y2type = Y2Type.CumulativePercent;
                        break;
                }

                List<XYColumnChartData> dataList = new List<XYColumnChartData>();

                foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> tableKvp in stratifiedFrequencyTables)
                {
                    double count = 0;
                    foreach (DescriptiveStatistics ds in tableKvp.Value)
                    {
                        count = count + ds.observations;
                    }

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
                                //if (((row[0].ToString().Equals(dRow[GadgetOptions.MainVariableName].ToString()))||(row[0].ToString().Equals(dRow[Parameters.ColumnNames[0]].ToString()))) && (y2type == Y2Type.CumulativePercent || dRow[second_y_var] != DBNull.Value))
                                if ((row[0].ToString().Equals(dRow[Parameters.ColumnNames[0]].ToString())) && (y2type == Y2Type.CumulativePercent || dRow[second_y_var] != DBNull.Value))
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
                        if(chartData.S == null || string.IsNullOrEmpty(chartData.S.ToString().Trim())) 
                        {
                            chartData.S = Config.Settings.RepresentationOfMissing;
                        }
                        dataList.Add(chartData);
                    }
                }

                this.Dispatcher.BeginInvoke(new SetChartDataDelegate(SetChartData), dataList, strata);
            }
        }

        protected void ChartGadgetBase_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectedChart = null;
            object panel = this.FindName("panelMain");
            if (panel is StackPanel)
            {
                foreach (UIElement element in (panel as StackPanel).Children)
                {
                    if (element.IsMouseOver)
                    {
                        SelectedChart = element as Controls.Charting.IChart;
                        break;
                    }
                }
            }

            if (SelectedChart != null)
            {                
                object el = this.FindName("separatorCurrentChart");
                if (el is Separator) (el as Separator).Visibility = System.Windows.Visibility.Visible;
                el = this.FindName("mnuCurrentChart");
                if (el is MenuItem) (el as MenuItem).Visibility = System.Windows.Visibility.Visible;
                el = this.FindName("mnuSaveSelectedChartAsImage");
                if (el is MenuItem) (el as MenuItem).Visibility = System.Windows.Visibility.Visible;
                el = this.FindName("mnuCopySelectedChartImage");
                if (el is MenuItem) (el as MenuItem).Visibility = System.Windows.Visibility.Visible;
                el = this.FindName("mnuCopySelectedChartData");
                if (el is MenuItem) (el as MenuItem).Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                object el = this.FindName("separatorCurrentChart");
                if (el is Separator) (el as Separator).Visibility = System.Windows.Visibility.Collapsed;
                el = this.FindName("mnuCurrentChart");
                if (el is MenuItem) (el as MenuItem).Visibility = System.Windows.Visibility.Collapsed;
                el = this.FindName("mnuSaveSelectedChartAsImage");
                if (el is MenuItem) (el as MenuItem).Visibility = System.Windows.Visibility.Collapsed;
                el = this.FindName("mnuCopySelectedChartImage");
                if (el is MenuItem) (el as MenuItem).Visibility = System.Windows.Visibility.Collapsed;
                el = this.FindName("mnuCopySelectedChartData");
                if (el is MenuItem) (el as MenuItem).Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        protected virtual void mnuSaveSelectedChartAsImage_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedChart != null)
            {
                SelectedChart.SaveImageToFile();
            }
        }

        protected virtual void mnuCopySelectedChartImage_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedChart != null)
            {
                SelectedChart.CopyImageToClipboard();
            }
        }

        protected virtual void mnuCopySelectedChartData_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedChart != null)
            {
                SelectedChart.CopyAllDataToClipboard();
            }
        }
    }
}
