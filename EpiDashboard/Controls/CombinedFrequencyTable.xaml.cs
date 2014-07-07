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
    /// <summary>
    /// Interaction logic for CombinedFrequencyTable.xaml
    /// </summary>
    public partial class CombinedFrequencyTable : UserControl
    {
        private Dictionary<string, List<DataTable>> TableDictionary { get; set; }

        public CombinedFrequencyTable(Dictionary<string, List<DataTable>> tableDictionary)
        {
            InitializeComponent();
            TableDictionary = tableDictionary;
            ConstructTable();
        }

        private void ConstructTable()
        {
            // Column for value label row
            grdMain.ColumnDefinitions.Add(new ColumnDefinition());

            // For primary heading row
            grdMain.RowDefinitions.Add(new RowDefinition());
            if (TableDictionary.Count >= 2)
            {
                // For secondary heading row, if needed
                grdMain.RowDefinitions.Add(new RowDefinition());
            }            

            //int requiredColumns = 0;
            int columnCount = 1;
            foreach (KeyValuePair<string, List<DataTable>> kvp in TableDictionary)
            {
                int columnSpan = kvp.Value.Count;
                if (TableDictionary.Count == 1 && columnSpan == 1)
                {
                    // Column for percent row (only show for non-stratified output)
                    grdMain.ColumnDefinitions.Add(new ColumnDefinition());
                }

                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    grdMain.ColumnDefinitions.Add(new ColumnDefinition());
                }

                //requiredColumns = requiredColumns + kvp.Value.Count;
                Rectangle rctHeader = new Rectangle();
                rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
                Grid.SetRow(rctHeader, 0);
                Grid.SetColumn(rctHeader, columnCount);
                Grid.SetColumnSpan(rctHeader, columnSpan);
                grdMain.Children.Add(rctHeader);

                TextBlock txtValHeader = new TextBlock();
                txtValHeader.Text = kvp.Key;
                txtValHeader.VerticalAlignment = VerticalAlignment.Center;
                txtValHeader.HorizontalAlignment = HorizontalAlignment.Center;
                txtValHeader.Margin = new Thickness(6);
                txtValHeader.FontWeight = FontWeights.Bold;
                txtValHeader.Foreground = Brushes.White;
                Grid.SetRow(txtValHeader, 0);
                Grid.SetColumn(txtValHeader, columnCount);
                Grid.SetColumnSpan(rctHeader, columnSpan);
                grdMain.Children.Add(txtValHeader);

                Border border = new Border();
                border.Style = this.Resources["gridCellBorder"] as Style;
                border.BorderThickness = new Thickness(border.BorderThickness.Left, border.BorderThickness.Bottom, border.BorderThickness.Right, border.BorderThickness.Bottom);
                Grid.SetRow(border, 0);
                Grid.SetColumn(border, columnCount);
                Grid.SetColumnSpan(rctHeader, columnSpan);
                grdMain.Children.Add(border);                

                foreach (DataTable dt in kvp.Value)
                {                    
                    Rectangle rctInnerHeader = new Rectangle();
                    rctInnerHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
                    Grid.SetRow(rctInnerHeader, 1);
                    Grid.SetColumn(rctInnerHeader, columnCount);
                    Grid.SetColumnSpan(rctInnerHeader, columnSpan);
                    grdMain.Children.Add(rctInnerHeader);

                    TextBlock txtInnerHeader = new TextBlock();
                    txtInnerHeader.Text = kvp.Key;
                    txtInnerHeader.VerticalAlignment = VerticalAlignment.Center;
                    txtInnerHeader.HorizontalAlignment = HorizontalAlignment.Center;
                    txtInnerHeader.Margin = new Thickness(6);
                    txtInnerHeader.FontWeight = FontWeights.Bold;
                    txtInnerHeader.Foreground = Brushes.White;
                    Grid.SetRow(txtInnerHeader, 1);
                    Grid.SetColumn(txtInnerHeader, columnCount);
                    grdMain.Children.Add(txtInnerHeader);

                    border = new Border();
                    border.Style = this.Resources["gridCellBorder"] as Style;
                    border.BorderThickness = new Thickness(border.BorderThickness.Left, border.BorderThickness.Bottom, border.BorderThickness.Right, border.BorderThickness.Bottom);
                    Grid.SetRow(border, 1);
                    Grid.SetColumn(border, columnCount);
                    grdMain.Children.Add(border);

                    columnCount++;
                }

                grdMain.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
