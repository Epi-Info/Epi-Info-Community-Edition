using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EpiDashboard.Controls
{
    public enum StrataListPanelMode
    {
        StrataMode = 1,
        GroupMode = 2
    }

    /// <summary>
    /// Interaction logic for GadgetStrataListPanel.xaml
    /// </summary>
    public partial class GadgetStrataListPanel : UserControl
    {
        private DataTable table;
        private DataView dv;
        public event StrataGridRowClickedHandler RowClicked;
        public event StrataGridExpandAllClickedHandler ExpandAllClicked;
        private Rectangle highlightRowRectangle;
        private StrataListPanelMode panelMode = StrataListPanelMode.StrataMode;

        public GadgetStrataListPanel()
        {
            InitializeComponent();
            InitTable();
        }

        public GadgetStrataListPanel(StrataListPanelMode panelMode)
        {
            InitializeComponent();
            this.PanelMode = panelMode;
            InitTable();
        }

        private void InitTable() 
        {
            table = new DataTable();
            DataColumn dc1 = new DataColumn("Strata", typeof(string));
            DataColumn dc2 = new DataColumn("OutcomeRateExposure", typeof(decimal));
            DataColumn dc3 = new DataColumn("OutcomeRateNoExposure", typeof(decimal));
            DataColumn dc4 = new DataColumn("RiskRatio", typeof(decimal));
            DataColumn dc5 = new DataColumn("RiskLower", typeof(decimal));
            DataColumn dc6 = new DataColumn("RiskUpper", typeof(decimal));
            DataColumn dc7 = new DataColumn("OddsRatio", typeof(decimal));
            DataColumn dc8 = new DataColumn("OddsLower", typeof(decimal));
            DataColumn dc9 = new DataColumn("OddsUpper", typeof(decimal));

            table.Columns.Add(dc1);
            table.Columns.Add(dc2);
            table.Columns.Add(dc3);
            table.Columns.Add(dc4);
            table.Columns.Add(dc5);
            table.Columns.Add(dc6);
            table.Columns.Add(dc7);
            table.Columns.Add(dc8);
            table.Columns.Add(dc9);

            dv = new DataView(table, string.Empty, "RiskRatio", DataViewRowState.CurrentRows);

            AddGridHeader();
        }

        public StrataListPanelMode PanelMode
        {
            get
            {
                return this.panelMode;
            }
            set
            {
                this.panelMode = value;
                if (PanelMode == StrataListPanelMode.StrataMode)
                {
                    txtExpanderHeader.Text = "Stratas";
                    toolTipExpandAll.Content = "Expands all stratification output tables.";
                    toolTipRemoveSort.Content = "Removes any sorting being conducted on the stratification grid list.";
                }
                else
                {
                    txtExpanderHeader.Text = "Exposure";
                    toolTipExpandAll.Content = "Expands all exposure output tables.";
                    toolTipRemoveSort.Content = "Removes any sorting being conducted on the exposure grid list.";
                }
            }
        }

        private void AddGridHeader()
        {
            grdMain.ColumnDefinitions.Add(new ColumnDefinition());
            grdMain.ColumnDefinitions.Add(new ColumnDefinition());
            grdMain.ColumnDefinitions.Add(new ColumnDefinition());
            grdMain.ColumnDefinitions.Add(new ColumnDefinition());
            grdMain.ColumnDefinitions.Add(new ColumnDefinition());
            grdMain.ColumnDefinitions.Add(new ColumnDefinition());
            grdMain.ColumnDefinitions.Add(new ColumnDefinition());
            grdMain.ColumnDefinitions.Add(new ColumnDefinition());
            grdMain.ColumnDefinitions.Add(new ColumnDefinition());

            grdMain.RowDefinitions.Add(new RowDefinition());
            grdMain.RowDefinitions[0].Height = GridLength.Auto;

            foreach (ColumnDefinition cd in grdMain.ColumnDefinitions)
            {
                cd.Width = GridLength.Auto;
            }

            Rectangle r1 = new Rectangle();
            r1.Style = this.Resources["gridHeaderCellRectangle"] as Style;            
            Grid.SetColumn(r1, 0);            
            grdMain.Children.Add(r1);

            Rectangle r2 = new Rectangle();
            r2.Style = this.Resources["gridHeaderCellRectangle"] as Style;
            Grid.SetColumn(r2, 1);
            grdMain.Children.Add(r2);

            Rectangle r3 = new Rectangle();
            r3.Style = this.Resources["gridHeaderCellRectangle"] as Style;
            Grid.SetColumn(r3, 2);
            grdMain.Children.Add(r3);

            Rectangle r4 = new Rectangle();
            r4.Style = this.Resources["gridHeaderCellRectangle"] as Style;
            Grid.SetColumn(r4, 3);
            grdMain.Children.Add(r4);

            Rectangle r5 = new Rectangle();
            r5.Style = this.Resources["gridHeaderCellRectangle"] as Style;
            Grid.SetColumn(r5, 4);
            grdMain.Children.Add(r5);

            Rectangle r6 = new Rectangle();
            r6.Style = this.Resources["gridHeaderCellRectangle"] as Style;
            Grid.SetColumn(r6, 5);
            grdMain.Children.Add(r6);

            Rectangle r7 = new Rectangle();
            r7.Style = this.Resources["gridHeaderCellRectangle"] as Style;
            Grid.SetColumn(r7, 6);
            grdMain.Children.Add(r7);

            Rectangle r8 = new Rectangle();
            r8.Style = this.Resources["gridHeaderCellRectangle"] as Style;
            Grid.SetColumn(r8, 7);
            grdMain.Children.Add(r8);

            Rectangle r9 = new Rectangle();
            r9.Style = this.Resources["gridHeaderCellRectangle"] as Style;
            Grid.SetColumn(r9, 8);
            grdMain.Children.Add(r9);

            r1.MouseEnter += new MouseEventHandler(rctHeader_MouseEnter);
            r1.MouseLeave += new MouseEventHandler(rctHeader_MouseLeave);
            r1.MouseLeftButtonUp += new MouseButtonEventHandler(rctHeader_MouseLeftButtonUp);

            r2.MouseEnter += new MouseEventHandler(rctHeader_MouseEnter);
            r2.MouseLeave += new MouseEventHandler(rctHeader_MouseLeave);
            r2.MouseLeftButtonUp += new MouseButtonEventHandler(rctHeader_MouseLeftButtonUp);

            r3.MouseEnter += new MouseEventHandler(rctHeader_MouseEnter);
            r3.MouseLeave += new MouseEventHandler(rctHeader_MouseLeave);
            r3.MouseLeftButtonUp += new MouseButtonEventHandler(rctHeader_MouseLeftButtonUp);

            r4.MouseEnter += new MouseEventHandler(rctHeader_MouseEnter);
            r4.MouseLeave += new MouseEventHandler(rctHeader_MouseLeave);
            r4.MouseLeftButtonUp += new MouseButtonEventHandler(rctHeader_MouseLeftButtonUp);

            r5.MouseEnter += new MouseEventHandler(rctHeader_MouseEnter);
            r5.MouseLeave += new MouseEventHandler(rctHeader_MouseLeave);
            r5.MouseLeftButtonUp += new MouseButtonEventHandler(rctHeader_MouseLeftButtonUp);

            r6.MouseEnter += new MouseEventHandler(rctHeader_MouseEnter);
            r6.MouseLeave += new MouseEventHandler(rctHeader_MouseLeave);
            r6.MouseLeftButtonUp += new MouseButtonEventHandler(rctHeader_MouseLeftButtonUp);

            r7.MouseEnter += new MouseEventHandler(rctHeader_MouseEnter);
            r7.MouseLeave += new MouseEventHandler(rctHeader_MouseLeave);
            r7.MouseLeftButtonUp += new MouseButtonEventHandler(rctHeader_MouseLeftButtonUp);

            r8.MouseEnter += new MouseEventHandler(rctHeader_MouseEnter);
            r8.MouseLeave += new MouseEventHandler(rctHeader_MouseLeave);
            r8.MouseLeftButtonUp += new MouseButtonEventHandler(rctHeader_MouseLeftButtonUp);

            r9.MouseEnter += new MouseEventHandler(rctHeader_MouseEnter);
            r9.MouseLeave += new MouseEventHandler(rctHeader_MouseLeave);
            r9.MouseLeftButtonUp += new MouseButtonEventHandler(rctHeader_MouseLeftButtonUp);

            Border b1 = new Border();
            b1.Style = this.Resources["gridCellBorder"] as Style;
            b1.BorderThickness = new Thickness(1, 1, 1, 1);
            Grid.SetColumn(b1, 0);
            grdMain.Children.Add(b1);

            Border b2 = new Border();
            b2.Style = this.Resources["gridCellBorder"] as Style;
            b2.BorderThickness = new Thickness(0, 1, 1, 1);
            Grid.SetColumn(b2, 1);
            grdMain.Children.Add(b2);

            Border b3 = new Border();
            b3.Style = this.Resources["gridCellBorder"] as Style;
            b3.BorderThickness = new Thickness(0, 1, 1, 1);
            Grid.SetColumn(b3, 2);
            grdMain.Children.Add(b3);

            Border b4 = new Border();
            b4.Style = this.Resources["gridCellBorder"] as Style;
            b4.BorderThickness = new Thickness(0, 1, 1, 1);
            Grid.SetColumn(b4, 3);
            grdMain.Children.Add(b4);

            Border b5 = new Border();
            b5.Style = this.Resources["gridCellBorder"] as Style;
            b5.BorderThickness = new Thickness(0, 1, 1, 1);
            Grid.SetColumn(b5, 4);
            grdMain.Children.Add(b5);

            Border b6 = new Border();
            b6.Style = this.Resources["gridCellBorder"] as Style;
            b6.BorderThickness = new Thickness(0, 1, 1, 1);
            Grid.SetColumn(b6, 5);
            grdMain.Children.Add(b6);

            Border b7 = new Border();
            b7.Style = this.Resources["gridCellBorder"] as Style;
            b7.BorderThickness = new Thickness(0, 1, 1, 1);
            Grid.SetColumn(b7, 6);
            grdMain.Children.Add(b7);

            Border b8 = new Border();
            b8.Style = this.Resources["gridCellBorder"] as Style;
            b8.BorderThickness = new Thickness(0, 1, 1, 1);
            Grid.SetColumn(b8, 7);
            grdMain.Children.Add(b8);

            Border b9 = new Border();
            b9.Style = this.Resources["gridCellBorder"] as Style;
            b9.BorderThickness = new Thickness(0, 1, 1, 1);
            Grid.SetColumn(b9, 8);
            grdMain.Children.Add(b9);

            TextBlock t1 = new TextBlock();
            t1.Style = this.Resources["columnHeadingText"] as Style;
            //t1.Text = "Strata";
            t1.Text = txtExpanderHeader.Text;
            t1.IsHitTestVisible = false;
            Grid.SetColumn(t1, 0);
            grdMain.Children.Add(t1);

            TextBlock t2 = new TextBlock();
            t2.Style = this.Resources["columnHeadingText"] as Style;
            t2.Text = "Outcome Rate" + Environment.NewLine + "Exposure";
            t2.IsHitTestVisible = false;
            Grid.SetColumn(t2, 1);
            grdMain.Children.Add(t2);

            TextBlock t3 = new TextBlock();
            t3.Style = this.Resources["columnHeadingText"] as Style;
            t3.Text = "Outcome Rate" + Environment.NewLine + "No Exposure";
            t3.IsHitTestVisible = false;
            Grid.SetColumn(t3, 2);
            grdMain.Children.Add(t3);

            TextBlock t4 = new TextBlock();
            t4.Style = this.Resources["columnHeadingText"] as Style;
            t4.Text = "Risk Ratio";
            t4.IsHitTestVisible = false;
            Grid.SetColumn(t4, 3);
            grdMain.Children.Add(t4);

            TextBlock t5 = new TextBlock();
            t5.Style = this.Resources["columnHeadingText"] as Style;
            t5.Text = "Risk Lower";
            t5.IsHitTestVisible = false;
            Grid.SetColumn(t5, 4);
            grdMain.Children.Add(t5);

            TextBlock t6 = new TextBlock();
            t6.Style = this.Resources["columnHeadingText"] as Style;
            t6.Text = "Risk Upper";
            t6.IsHitTestVisible = false;
            Grid.SetColumn(t6, 5);
            grdMain.Children.Add(t6);

            TextBlock t7 = new TextBlock();
            t7.Style = this.Resources["columnHeadingText"] as Style;
            t7.Text = "Odds Ratio";
            t7.IsHitTestVisible = false;
            Grid.SetColumn(t7, 6);
            grdMain.Children.Add(t7);

            TextBlock t8 = new TextBlock();
            t8.Style = this.Resources["columnHeadingText"] as Style;
            t8.Text = "Odds Lower";
            t8.IsHitTestVisible = false;
            Grid.SetColumn(t8, 7);
            grdMain.Children.Add(t8);

            TextBlock t9 = new TextBlock();
            t9.Style = this.Resources["columnHeadingText"] as Style;
            t9.Text = "Odds Upper";
            t9.IsHitTestVisible = false;
            Grid.SetColumn(t9, 8);
            grdMain.Children.Add(t9);
        }

        public void Clear()
        {
            grdMain.Children.Clear();
            grdMain.RowDefinitions.Clear();
            grdMain.ColumnDefinitions.Clear();
            table.Rows.Clear();
            InitTable();
        }

        public string ExpanderHeader
        {
            get
            {
                return txtExpanderHeader.Text;
            }
            set
            {
                this.txtExpanderHeader.Text = value;
            }
        }

        public void AddRow(StrataGridListRow strataGridListRow, bool addToTable = true)
        {
            if (addToTable)
            {
                if (table.Select("[" + table.Columns[0].ColumnName + "] = '" + strataGridListRow.StrataLabel.Replace("'", "''") + "'", "").Length > 0)
                {
                    // don't add.
                    return;
                }
                else
                {
                    table.Rows.Add(strataGridListRow.StrataLabel, strataGridListRow.OutcomeRateExposure, strataGridListRow.OutcomeRateNoExposure, strataGridListRow.RiskRatio, strataGridListRow.RiskLower, strataGridListRow.RiskUpper, strataGridListRow.OddsRatio, strataGridListRow.OddsLower, strataGridListRow.OddsUpper);
                }
            }

            grdMain.RowDefinitions.Add(new RowDefinition());

            grdMain.RowDefinitions[grdMain.RowDefinitions.Count - 1].Height = GridLength.Auto;

            Rectangle r1 = new Rectangle();
            r1.Style = this.Resources["gridCellRectangle"] as Style;
            Grid.SetColumn(r1, 0);
            Grid.SetColumnSpan(r1, 10);
            Grid.SetRow(r1, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(r1);

            r1.MouseEnter += new MouseEventHandler(rowDef_MouseEnter);
            r1.MouseLeave += new MouseEventHandler(rowDef_MouseLeave);
            r1.MouseUp += new MouseButtonEventHandler(rowDef_MouseUp);

            Border b1 = new Border();
            b1.Style = this.Resources["gridCellBorder"] as Style;
            b1.BorderThickness = new Thickness(1, 0, 1, 1);
            b1.IsHitTestVisible = false;
            Grid.SetColumn(b1, 0);
            Grid.SetRow(b1, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(b1);

            Border b2 = new Border();
            b2.Style = this.Resources["gridCellBorder"] as Style;
            b2.IsHitTestVisible = false;
            Grid.SetColumn(b2, 1);
            Grid.SetRow(b2, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(b2);

            Border b3 = new Border();
            b3.Style = this.Resources["gridCellBorder"] as Style;
            b3.IsHitTestVisible = false;
            Grid.SetColumn(b3, 2);
            Grid.SetRow(b3, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(b3);

            Border b4 = new Border();
            b4.Style = this.Resources["gridCellBorder"] as Style;
            b4.IsHitTestVisible = false;
            Grid.SetColumn(b4, 3);
            Grid.SetRow(b4, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(b4);

            Border b5 = new Border();
            b5.Style = this.Resources["gridCellBorder"] as Style;
            b5.IsHitTestVisible = false;
            Grid.SetColumn(b5, 4);
            Grid.SetRow(b5, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(b5);

            Border b6 = new Border();
            b6.Style = this.Resources["gridCellBorder"] as Style;
            b6.IsHitTestVisible = false;
            Grid.SetColumn(b6, 5);
            Grid.SetRow(b6, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(b6);

            Border b7 = new Border();
            b7.Style = this.Resources["gridCellBorder"] as Style;
            b7.IsHitTestVisible = false;
            Grid.SetColumn(b7, 6);
            Grid.SetRow(b7, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(b7);

            Border b8 = new Border();
            b8.Style = this.Resources["gridCellBorder"] as Style;
            b8.IsHitTestVisible = false;
            Grid.SetColumn(b8, 7);
            Grid.SetRow(b8, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(b8);

            Border b9 = new Border();
            b9.Style = this.Resources["gridCellBorder"] as Style;
            b9.IsHitTestVisible = false;
            Grid.SetColumn(b9, 8);
            Grid.SetRow(b9, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(b9);

            TextBlock t1 = new TextBlock();
            t1.Text = strataGridListRow.StrataLabel;
            t1.Margin = (Thickness)this.Resources["genericTextMargin"];
            t1.IsHitTestVisible = false;
            Grid.SetColumn(t1, 0);
            Grid.SetRow(t1, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(t1);

            TextBlock t2 = new TextBlock();
            if(strataGridListRow.OutcomeRateExposure.HasValue) t2.Text = strataGridListRow.OutcomeRateExposure.Value.ToString("F4");
            t2.Margin = (Thickness)this.Resources["genericTextMargin"];
            t2.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            t2.IsHitTestVisible = false;
            Grid.SetColumn(t2, 1);
            Grid.SetRow(t2, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(t2);

            TextBlock t3 = new TextBlock();
            if (strataGridListRow.OutcomeRateNoExposure.HasValue) t3.Text = strataGridListRow.OutcomeRateNoExposure.Value.ToString("F4");
            t3.Margin = (Thickness)this.Resources["genericTextMargin"];
            t3.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            t3.IsHitTestVisible = false;
            Grid.SetColumn(t3, 2);
            Grid.SetRow(t3, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(t3);

            TextBlock t4 = new TextBlock();
            if (strataGridListRow.RiskRatio.HasValue) t4.Text = strataGridListRow.RiskRatio.Value.ToString("F4");
            t4.Margin = (Thickness)this.Resources["genericTextMargin"];
            t4.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            t4.IsHitTestVisible = false;
            Grid.SetColumn(t4, 3);
            Grid.SetRow(t4, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(t4);

            TextBlock t5 = new TextBlock();
            if (strataGridListRow.RiskLower.HasValue) t5.Text = strataGridListRow.RiskLower.Value.ToString("F4");
            t5.Margin = (Thickness)this.Resources["genericTextMargin"];
            t5.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            t5.IsHitTestVisible = false;
            Grid.SetColumn(t5, 4);
            Grid.SetRow(t5, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(t5);

            TextBlock t6 = new TextBlock();
            if (strataGridListRow.RiskUpper.HasValue) t6.Text = strataGridListRow.RiskUpper.Value.ToString("F4");
            t6.Margin = (Thickness)this.Resources["genericTextMargin"];
            t6.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            t6.IsHitTestVisible = false;
            Grid.SetColumn(t6, 5);
            Grid.SetRow(t6, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(t6);

            TextBlock t7 = new TextBlock();
            if (strataGridListRow.OddsRatio.HasValue) t7.Text = strataGridListRow.OddsRatio.Value.ToString("F4");
            t7.Margin = (Thickness)this.Resources["genericTextMargin"];
            t7.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            t7.IsHitTestVisible = false;
            Grid.SetColumn(t7, 6);
            Grid.SetRow(t7, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(t7);

            TextBlock t8 = new TextBlock();
            if (strataGridListRow.OddsLower.HasValue) t8.Text = strataGridListRow.OddsLower.Value.ToString("F4");
            t8.Margin = (Thickness)this.Resources["genericTextMargin"];
            t8.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            t8.IsHitTestVisible = false;
            Grid.SetColumn(t8, 7);
            Grid.SetRow(t8, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(t8);

            TextBlock t9 = new TextBlock();
            if (strataGridListRow.OddsUpper.HasValue) t9.Text = strataGridListRow.OddsUpper.Value.ToString("F4");
            t9.Margin = (Thickness)this.Resources["genericTextMargin"];
            t9.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            t9.IsHitTestVisible = false;
            Grid.SetColumn(t9, 8);
            Grid.SetRow(t9, grdMain.RowDefinitions.Count - 1);
            grdMain.Children.Add(t9);
        }

        void rowDef_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int row = Grid.GetRow((FrameworkElement)sender);
            int column = 0;
            //UIElement element = grid.Children.Cast<UIElement>().First(x => Grid.GetRow(x) == row && Grid.GetColumn(x) == column);
            //if (element is TextBlock)
            //{
            //    if (RecordSelected != null)
            //    {
            //        RecordSelected(int.Parse(((TextBlock)element).Text));
            //    }
            //}
            //else if (element is Rectangle)
            //{
            IEnumerable<UIElement> elements = grdMain.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == row && Grid.GetColumn(x) == column);

            foreach (UIElement element in elements)
            {
                if (element is TextBlock)
                {
                    if (RowClicked != null)
                    {
                        RowClicked(element, (element as TextBlock).Text);
                    }
                }
            }
            //}
        }

        void rowDef_MouseLeave(object sender, MouseEventArgs e)
        {
            if (highlightRowRectangle != null && grdMain.Children.Contains(highlightRowRectangle))
            {
                grdMain.Children.Remove(highlightRowRectangle);
            }
            this.Cursor = Cursors.Arrow;
        }

        void rowDef_MouseEnter(object sender, MouseEventArgs e)
        {
            int row = Grid.GetRow((UIElement)sender);

            if (sender is Rectangle)
            {
                Rectangle rectangle = sender as Rectangle;
                if (rectangle.Parent is Border)
                {
                    Border border = rectangle.Parent as Border;
                    row = Grid.GetRow(border);
                }
            }

            if (highlightRowRectangle == null)
            {
                highlightRowRectangle = new Rectangle();
                highlightRowRectangle.IsHitTestVisible = false;
                highlightRowRectangle.Fill = Brushes.LightBlue;
                highlightRowRectangle.Opacity = 0.35;
                highlightRowRectangle.Margin = new Thickness(0);
                Grid.SetZIndex(highlightRowRectangle, 2000);
            }
            Grid.SetRow(highlightRowRectangle, row);
            Grid.SetColumn(highlightRowRectangle, 0);
            Grid.SetColumnSpan(highlightRowRectangle, grdMain.ColumnDefinitions.Count);
            grdMain.Children.Add(highlightRowRectangle);

            this.Cursor = Cursors.Hand;
        }

        private void Redraw()
        {
            grdMain.Children.Clear();
            grdMain.RowDefinitions.Clear();
            grdMain.ColumnDefinitions.Clear();
            AddGridHeader();

            foreach (DataRowView rowView in dv)
            {
                DataRow row = rowView.Row;

                StrataGridListRow sRow = new StrataGridListRow();
                sRow.StrataLabel = row[0].ToString();
                if (row[1] == DBNull.Value)
                {
                    sRow.OutcomeRateExposure = null;
                }
                else
                {
                    sRow.OutcomeRateExposure = Convert.ToDecimal(row[1]);
                }

                if (row[2] == DBNull.Value)
                {
                    sRow.OutcomeRateNoExposure = null;
                }
                else
                {
                    sRow.OutcomeRateNoExposure = Convert.ToDecimal(row[2]);
                }                

                if (row[3] == DBNull.Value)
                {
                    sRow.RiskRatio = null;
                }
                else
                {
                    sRow.RiskRatio = Convert.ToDecimal(row[3]);
                }

                if (row[4] == DBNull.Value)
                {
                    sRow.RiskLower = null;
                }
                else
                {
                    sRow.RiskLower = Convert.ToDecimal(row[4]);
                }

                if (row[5] == DBNull.Value)
                {
                    sRow.RiskUpper = null;
                }
                else
                {
                    sRow.RiskUpper = Convert.ToDecimal(row[5]);
                }

                if (row[6] == DBNull.Value)
                {
                    sRow.OddsRatio = null;
                }
                else
                {
                    sRow.OddsRatio = Convert.ToDecimal(row[6]);
                }

                if (row[7] == DBNull.Value)
                {
                    sRow.OddsLower = null;
                }
                else
                {
                    sRow.OddsLower = Convert.ToDecimal(row[7]);
                }

                if (row[8] == DBNull.Value)
                {
                    sRow.OddsUpper = null;
                }
                else
                {
                    sRow.OddsUpper = Convert.ToDecimal(row[8]);
                }

                AddRow(sRow, false);
            }
        }

        private void PathExpandAll_MouseEnter(object sender, MouseEventArgs e)
        {
            expandAllRectangle.Style = this.Resources["swapValuesRectangleHover"] as Style;
            this.Cursor = Cursors.Hand;
        }

        private void PathExpandAll_MouseLeave(object sender, MouseEventArgs e)
        {
            expandAllRectangle.Style = this.Resources["swapValuesRectangle"] as Style;
            this.Cursor = Cursors.Arrow;
        }

        private void PathExpandAll_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ExpandAllClicked != null)
            {
                ExpandAllClicked();
            }
        }

        private void PathRemoveSort_MouseEnter(object sender, MouseEventArgs e)
        {
            removeSortRectangle.Style = this.Resources["swapValuesRectangleHover"] as Style;
            this.Cursor = Cursors.Hand;
        }

        private void PathRemoveSort_MouseLeave(object sender, MouseEventArgs e)
        {
            removeSortRectangle.Style = this.Resources["swapValuesRectangle"] as Style;
            this.Cursor = Cursors.Arrow;
        }

        private void PathRemoveSort_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dv.Sort = string.Empty;
            Redraw();
        }

        void rctHeader_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Rectangle)
            {
                Rectangle rect = sender as Rectangle;
                rect.Fill = this.Resources["lineListRowHighlightBrush"] as SolidColorBrush;
                rect.Opacity = 0.4;
                this.Cursor = Cursors.Hand;
            }
        }

        void rctHeader_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Rectangle)
            {
                Rectangle rect = sender as Rectangle;
                rect.Fill = this.Resources["lineListRowHighlightBrush"] as SolidColorBrush;
                rect.Opacity = 1.0;
                this.Cursor = Cursors.Arrow;
            }
        }

        void rctHeader_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle)
            {
                Rectangle rect = sender as Rectangle;

                int column = Grid.GetColumn(rect);

                string sortColumnName = table.Columns[column].ColumnName;

                if (dv.Sort.Equals(sortColumnName + " ASC"))
                {
                    dv.Sort = sortColumnName + " DESC";
                }
                else if (dv.Sort.Equals(sortColumnName + " DESC"))
                {
                    dv.Sort = sortColumnName + " ASC";
                }
                else
                {
                    dv.Sort = sortColumnName + " DESC";
                }
                
                Redraw();
            }
        }
    }
}
