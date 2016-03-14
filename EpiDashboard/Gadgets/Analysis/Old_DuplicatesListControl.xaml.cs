using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Epi;
using Epi.Data;
using Epi.Fields;
using EpiDashboard;
using EpiDashboard.Rules;
using EpiDashboard.Controls;

namespace EpiDashboard
{
    /// <summary>
    /// Interaction logic for DuplicatesListControl.xaml
    /// </summary>
    /// <remarks>
    /// This gadget is used to generate a line list of records having duplicate values in key fields specified by the user. 
    /// </remarks>
    public partial class Old_DuplicatesListControl : GadgetBase
    {
        #region Private Members
        /// <summary>
        /// A custom heading to use for this gadget's output
        /// </summary>
        private string customOutputHeading;

        /// <summary>
        /// A custom description to use for this gadget's output
        /// </summary>
        private string customOutputDescription;

        /// <summary>
        /// A custom caption to use for this gadget's table/image output, if applicable
        /// </summary>
        private string customOutputCaption;

        /// <summary>
        /// Bool used to determine if the gadget is being called directly from the Enter module (e.g. 'Interactive Line List' option)
        /// </summary>
        private bool isHostedByEnter;

        private int draggedColumnIndex = -1;
        private string clickedColumnName = string.Empty;
        private bool columnWarningShown;
        private int rowCount = 1;
        private int columnCount = 1;
        private int maxColumns;

        private const int MAX_ROW_LIMIT = 2000;

        private List<string> columnOrder = new List<string>();
        private Rectangle highlightRowRectangle;

        #endregion // Private Members

        #region Delegates
        private delegate void AddFreqGridDelegate(string strataVar, string value);
        private new delegate void AddGridFooterDelegate(string strataValue, int rowNumber, int totalRows);
        private delegate void SetGridImageDelegate(string strataValue, byte[] imageBlob, TextBlockConfig textBlockConfig, FontWeight fontWeight);
        private delegate void RenderFrequencyHeaderDelegate(string strataValue, string freqVar, DataColumnCollection columns);
        private RequestUpdateStatusDelegate requestUpdateStatus;
        private CheckForCancellationDelegate checkForCancellation;

        #endregion // Delegates

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Old_DuplicatesListControl()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper object to attach</param>
        public Old_DuplicatesListControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            Construct();
            FillComboboxes();
        }

        #endregion // Constructors

        #region Private and Protected Methods

        /// <summary>
        /// Used to add a new Line List grid to the gadget's output
        /// </summary>
        /// <param name="groupVar">The name of the group variable selected, if any</param>
        /// <param name="value">The value by which this grid has been grouped by</param>
        private void AddLineListGrid(string groupVar, string value, int columnCount)
        {
            ScrollViewer sv = new ScrollViewer();
            sv.Style = this.Resources["gadgetScrollViewer"] as Style;
            sv.Tag = value;
            if (IsHostedByEnter)
            {
                sv.MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight - 180;
                sv.MaxWidth = System.Windows.SystemParameters.PrimaryScreenWidth - 150;
                sv.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            }
            else
            {
                //sv.MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight - 300;
                //sv.MaxWidth = System.Windows.SystemParameters.PrimaryScreenWidth - 250;

                int width = 800;
                int height = 600;

                //XAML DOESN'T HAVE MAX WIDTH OR MAX HEIGHT YET.
                //if (int.TryParse(txtMaxWidth.Text, out width))
                //{
                //    sv.MaxWidth = width;
                //}
                //else
                //{
                //    sv.MaxWidth = double.NaN;
                //}

                //if (int.TryParse(txtMaxHeight.Text, out height))
                //{
                //    sv.MaxHeight = height;
                //}
                //else
                //{
                //    sv.MaxHeight = double.NaN;
                //}
            }

            sv.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            Grid grid = new Grid();
            grid.Tag = value;
            grid.Style = this.Resources["genericOutputGrid"] as Style;
            grid.Visibility = System.Windows.Visibility.Collapsed;

            Border border = new Border();
            border.Style = this.Resources["genericOutputGridBorder"] as Style;
            border.Child = grid;

            for (int i = 0; i < columnCount; i++)
            {
                if (i > MaxColumns)
                {
                    break;
                }

                ColumnDefinition column = new ColumnDefinition();
                column.Width = GridLength.Auto;
                grid.ColumnDefinitions.Add(column);
            }

            if (checkboxLineColumn.IsChecked == false)
            {
                grid.ColumnDefinitions[0].Width = new GridLength(1);
            }

            ColumnDefinition totalColumn = new ColumnDefinition();
            totalColumn.Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(totalColumn);

            TextBlock txtGridLabel = new TextBlock();
            txtGridLabel.Text = value;
            txtGridLabel.HorizontalAlignment = HorizontalAlignment.Left;
            txtGridLabel.VerticalAlignment = VerticalAlignment.Bottom;
            txtGridLabel.Margin = new Thickness(2, 2, 2, 2);
            txtGridLabel.FontWeight = FontWeights.Bold;

            sv.Content = border;

            Expander expander = new Expander();

            TextBlock txtExpanderHeader = new TextBlock();
            txtExpanderHeader.Text = value;
            txtExpanderHeader.Style = this.Resources["genericOutputExpanderText"] as Style;
            expander.Header = txtExpanderHeader;

            if (string.IsNullOrEmpty(groupVar))
            {
                txtGridLabel.Visibility = System.Windows.Visibility.Collapsed;
                panelMain.Children.Add(sv);
            }
            else
            {
                expander.Margin = (Thickness)this.Resources["expanderMargin"]; //new Thickness(6, 2, 6, 6);                
                sv.Margin = new Thickness(0, 4, 0, 4);
                expander.Content = sv;
                expander.IsExpanded = true;
                panelMain.Children.Add(expander);
                StrataExpanderList.Add(expander);
            }

            StrataGridList.Add(grid);
            //groupSvList.Add(sv);
        }

        private void SetGridImage(string strataValue, byte[] imageBlob, TextBlockConfig textBlockConfig, FontWeight fontWeight)
        {
            Grid grid = new Grid();

            grid = GetStrataGrid(strataValue);

            Image image = new Image();
            System.IO.MemoryStream stream = new System.IO.MemoryStream(imageBlob);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            image.Source = bitmap;
            image.Height = bitmap.Height;
            image.Width = bitmap.Width;

            if (IsHostedByEnter)
            {
                image.MouseEnter += new MouseEventHandler(rowDef_MouseEnter);
                image.MouseLeave += new MouseEventHandler(rowDef_MouseLeave);
                image.MouseUp += new MouseButtonEventHandler(rowDef_MouseUp);
            }

            Grid.SetZIndex(image, 1000);
            Grid.SetRow(image, textBlockConfig.RowNumber);
            Grid.SetColumn(image, textBlockConfig.ColumnNumber);
            grid.Children.Add(image);
        }

        void colDef_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (draggedColumnIndex == -1 || IsProcessing)
            {
                return;
            }

            columnOrder = new List<string>();

            Grid grid = (Grid)((FrameworkElement)sender).Parent;
            int destinationColumn = Grid.GetColumn((FrameworkElement)sender);

            if (destinationColumn < draggedColumnIndex)
            {
                foreach (UIElement element in grid.Children)
                {
                    int currentColumn = Grid.GetColumn(element);
                    if (currentColumn == draggedColumnIndex)
                    {
                        Grid.SetColumn(element, destinationColumn);
                    }
                    else if (currentColumn == destinationColumn)
                    {
                        Grid.SetColumn(element, destinationColumn + 1);
                    }
                    else if (currentColumn >= destinationColumn && currentColumn < draggedColumnIndex)
                    {
                        Grid.SetColumn(element, currentColumn + 1);
                    }
                }
            }
            else if (destinationColumn > draggedColumnIndex)
            {
                foreach (UIElement element in grid.Children)
                {
                    int currentColumn = Grid.GetColumn(element);
                    if (currentColumn == draggedColumnIndex)
                    {
                        Grid.SetColumn(element, destinationColumn);
                    }
                    else if (currentColumn == destinationColumn)
                    {
                        Grid.SetColumn(element, destinationColumn - 1);
                    }
                    else if (currentColumn < destinationColumn && currentColumn > draggedColumnIndex)
                    {
                        Grid.SetColumn(element, currentColumn - 1);
                    }
                }
            }

            draggedColumnIndex = -1;
        }

        void colDef_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsProcessing)
            {
                draggedColumnIndex = -1;
            }
            else
            {
                Grid grid = (Grid)((FrameworkElement)sender).Parent;
                int column = Grid.GetColumn((FrameworkElement)sender);
                draggedColumnIndex = column;
            }
        }

        void rowDef_MouseEnter(object sender, MouseEventArgs e)
        {
            int row = Grid.GetRow((UIElement)sender);

            if(sender is Rectangle) 
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
                highlightRowRectangle.Opacity = 0.25;
                highlightRowRectangle.Margin = new Thickness(0);
                //highlightRowRectangle.StrokeThickness = 3;
                Grid.SetZIndex(highlightRowRectangle, 2000);
            }
            Grid.SetRow(highlightRowRectangle, row);
            Grid.SetColumn(highlightRowRectangle, 0);
            Grid.SetColumnSpan(highlightRowRectangle, StrataGridList[0].ColumnDefinitions.Count); //((Grid)((FrameworkElement)sender).Parent);
            //Grid.SetColumnSpan(highlightRowRectangle, (((Border)((FrameworkElement)sender).Parent).Child as Grid).ColumnDefinitions.Count); //((Grid)((FrameworkElement)sender).Parent);
            StrataGridList[0].Children.Add(highlightRowRectangle);
            
            
            //.ColumnDefinitions.Count);
            this.Cursor = Cursors.Hand;
        }

        void rowDef_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Grid grid = StrataGridList[0]; //new Grid();

            //if (sender is Rectangle)
            //{
            //    grid = (((sender as Rectangle).Parent) as Border).Parent as Grid;
            //}
            int row = Grid.GetRow((FrameworkElement)sender);
            int column = grid.ColumnDefinitions.Count - 1;
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
            IEnumerable<UIElement> elements = grid.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == row && Grid.GetColumn(x) == column);

            foreach (UIElement element in elements)
            {
                if (element is TextBlock)
                {
                    if (RecordSelected != null)
                    {
                        RecordSelected(int.Parse(((TextBlock)element).Text));
                    }
                }
            }
            //}
        }

        void rowDef_MouseLeave(object sender, MouseEventArgs e)
        {
            if (highlightRowRectangle != null && StrataGridList.Count > 0 && StrataGridList[0].Children.Contains(highlightRowRectangle))
            {
                StrataGridList[0].Children.Remove(highlightRowRectangle);
            }
            this.Cursor = Cursors.Arrow;
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

        void rctHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle && (sender as Rectangle).Tag != null)
            {
                clickedColumnName = (sender as Rectangle).Tag.ToString();
            }
        }

        void rctHeader_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle)
            {
                Rectangle rect = sender as Rectangle;

                if (rect.Tag != null && rect.Tag.ToString() == clickedColumnName)
                {
                    string columnName = rect.Tag.ToString();
                    if (!lbxDedupFields.Items.Contains(columnName + " (ascending)") && !lbxDedupFields.Items.Contains(columnName + " (descending)"))
                    {
                        InsertColumnInSortList(columnName, 0);
                        this.RefreshResults();
                    }
                    else if (lbxDedupFields.Items.Contains(columnName + " (ascending)") || lbxDedupFields.Items.Contains(columnName + " (descending)"))
                    {
                        SwapColumnOrderInSortList(columnName);
                        this.RefreshResults();
                    }
                }
            }
        }

        private void AddColumnToSortList(string columnName)
        {
            if (!lbxDedupFields.Items.Contains(columnName) && !lbxDedupFields.Items.Contains(columnName + " (ascending)"))
            {
                lbxDedupFields.Items.Add(columnName + " (ascending)");
            }
        }

        private void InsertColumnInSortList(string columnName, int insertAt)
        {
            if (!lbxDedupFields.Items.Contains(columnName) && !lbxDedupFields.Items.Contains(columnName + " (ascending)"))
            {
                lbxDedupFields.Items.Insert(insertAt, columnName + " (ascending)");
            }
        }

        private void SwapColumnOrderInSortList(string columnName)
        {
            if (lbxDedupFields.Items.Contains(columnName + " (ascending)"))
            {
                string sortColumn = columnName + " (descending)";
                lbxDedupFields.Items.Remove(columnName + " (ascending)");
                lbxDedupFields.Items.Add(sortColumn);
            }
            else if (lbxDedupFields.Items.Contains(columnName + " (descending)"))
            {
                string sortColumn = columnName + " (ascending)";
                lbxDedupFields.Items.Remove(columnName + " (descending)");
                lbxDedupFields.Items.Add(sortColumn);
            }
        }

        void mnuRemoveSorts_Click(object sender, RoutedEventArgs e)
        {
            lbxDedupFields.Items.Clear();
            RefreshResults();
        }

        void mnuSwapSortType_Click(object sender, RoutedEventArgs e)
        {
            if (lbxDedupFields.SelectedItems.Count == 1)
            {
                string selectedItem = lbxDedupFields.SelectedItem.ToString();
                int index = lbxDedupFields.SelectedIndex;

                if (selectedItem.ToLower().EndsWith("(ascending)"))
                {
                    selectedItem = selectedItem.Remove(selectedItem.Length - 11) + "(descending)";
                }
                else
                {
                    selectedItem = selectedItem.Remove(selectedItem.Length - 12) + "(ascending)";
                }

                lbxDedupFields.Items.Remove(lbxDedupFields.SelectedItem.ToString());
                lbxDedupFields.Items.Insert(index, selectedItem);
            }
        }

        void mnuRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lbxDedupFields.SelectedItems.Count == 1)
            {
                lbxDedupFields.Items.Remove(lbxDedupFields.SelectedItem.ToString());
            }
        }

        private void txtMaxRows_TextChanged(object sender, TextChangedEventArgs e)
        {
            int rows = 0;

            int.TryParse(txtMaxRows.Text, out rows);

            if (rows > MAX_ROW_LIMIT)
            {
                rows = MAX_ROW_LIMIT;
                txtMaxRows.Text = rows.ToString();
            }
        }

        private void SetGridText(string strataValue, TextBlockConfig textBlockConfig, FontWeight fontWeight)
        {
            Grid grid = new Grid();

            grid = GetStrataGrid(strataValue);

            TextBlock txt = new TextBlock();
            txt.Foreground = this.Resources["cellForeground"] as SolidColorBrush;
            txt.FontWeight = fontWeight;
            txt.Text = textBlockConfig.Text;
            txt.Margin = textBlockConfig.Margin;
            txt.VerticalAlignment = textBlockConfig.VerticalAlignment;
            txt.HorizontalAlignment = textBlockConfig.HorizontalAlignment;
            txt.TextAlignment = textBlockConfig.TextAlignment;
            txt.TextWrapping = TextWrapping.Wrap;
            txt.MaxWidth = 300;
            txt.Visibility = textBlockConfig.ControlVisibility;
            //if (IsHostedByEnter)
            //{
            //    txt.MouseEnter += new MouseEventHandler(rowDef_MouseEnter);
            //    txt.MouseLeave += new MouseEventHandler(rowDef_MouseLeave);
            //    txt.MouseUp += new MouseButtonEventHandler(rowDef_MouseUp);
            //}
            Grid.SetZIndex(txt, 1000);
            Grid.SetRow(txt, textBlockConfig.RowNumber);
            Grid.SetColumn(txt, textBlockConfig.ColumnNumber);
            grid.Children.Add(txt);

            //if (checkboxAllowUpdates.IsChecked == true)
            //{
            //    //txt.MouseLeftButtonDown += new MouseButtonEventHandler(gridCell_MouseLeftButtonDown); 
            //}
        }
        /// <summary>
        /// Copies a grid's output to the clipboard
        /// </summary>
        protected override void CopyToClipboard()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Grid grid in this.StrataGridList)
            {
                string gridName = grid.Tag.ToString();
                if (StrataGridList.Count > 1)
                {
                    sb.AppendLine(grid.Tag.ToString());
                }

                foreach (UIElement control in grid.Children)
                {
                    if (control is TextBlock)
                    {                        
                        int columnNumber = Grid.GetColumn(control);
                        string value = ((TextBlock)control).Text;
                        sb.Append(value + "\t");
                        if (columnNumber >= grid.ColumnDefinitions.Count - 2)
                        {
                            sb.AppendLine();
                        }
                    }
                }

                sb.AppendLine();
            }
            Clipboard.Clear();
            Clipboard.SetText(sb.ToString());
        }

        /// <summary>
        /// Handles the filling of the gadget's combo boxes
        /// </summary>
        private void FillComboboxes(bool update = false)
        {
            //LoadingCombos = true;

            //string prevField = string.Empty;
            //string prevWeightField = string.Empty;
            //List<string> prevStrataFields = new List<string>();

            //lbxDedupFields.ItemsSource = null;
            //lbxDedupFields.Items.Clear();
            //lbxDisplayFields.ItemsSource = null;
            //lbxDisplayFields.Items.Clear();

            //List<string> fieldNames = new List<string>();
            
            //ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;
            //fieldNames = DashboardHelper.GetFieldsAsList(columnDataType);

            //columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;

            //columnDataType = ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.Text | ColumnDataType.UserDefined;

            //fieldNames.Sort();

            //if (fieldNames.Contains("SYSTEMDATE"))
            //{
            //    fieldNames.Remove("SYSTEMDATE");
            //}

            //if (DashboardHelper.IsUsingEpiProject)
            //{
            //    if (fieldNames.Contains("RecStatus")) fieldNames.Remove("RecStatus");
            //}

            //lbxDedupFields.ItemsSource = fieldNames;
            //lbxDisplayFields.ItemsSource = fieldNames;

            //LoadingCombos = false;
        }

        /// <summary>
        /// Sets the text value of a grid cell in a given output grid
        /// </summary>
        /// <param name="strataValue">
        /// The strata value to which this grid cell belongs; used to search the list of grids and 
        /// return the proper System.Windows.Controls.Grid for text insertion.
        /// </param>
        /// <param name="textBlockConfig">The configuration options for this block of text</param>
        private void SetGridText(string strataValue, TextBlockConfig textBlockConfig)
        {
            Grid grid = new Grid();

            grid = GetStrataGrid(strataValue);

            TextBlock txt = new TextBlock();
            txt.Text = textBlockConfig.Text;
            txt.Margin = textBlockConfig.Margin;
            txt.VerticalAlignment = textBlockConfig.VerticalAlignment;
            txt.HorizontalAlignment = textBlockConfig.HorizontalAlignment;
            Grid.SetRow(txt, textBlockConfig.RowNumber);
            Grid.SetColumn(txt, textBlockConfig.ColumnNumber);
            grid.Children.Add(txt);
        }

        bool IsSortingBy(string columnName, out SortOrder sortOrder)
        {
            sortOrder = SortOrder.Ascending;

            foreach (KeyValuePair<string, string> kvp in this.GadgetOptions.InputVariableList)
            {
                if (kvp.Value.Equals("sortfield"))
                {
                    string key = kvp.Key.Substring(0, kvp.Key.Length - 4);
                    string order = kvp.Key.Substring(kvp.Key.Length - 5, 4);

                    if (order.Contains("DES"))
                    {
                        sortOrder = SortOrder.Descending;
                    }

                    key = key.Trim().TrimEnd(']').TrimStart('[');
                    if (key == columnName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Used to render the header (first row) of a given frequency output grid
        /// </summary>
        /// <param name="strataValue">
        /// The strata value to which this grid cell belongs; used to search the list of grids and 
        /// return the proper System.Windows.Controls.Grid for text insertion.
        /// </param>
        /// <param name="freqVar">The variable that the statistics were run on</param>        
        private void RenderFrequencyHeader(string strataValue, string freqVar, DataColumnCollection columns)
        {
            DuplicatesListParameters DuplicateParameters = (DuplicatesListParameters)Parameters;
            Grid grid = GetStrataGrid(strataValue);

            RowDefinition rowDefHeader = new RowDefinition();
            rowDefHeader.Height = new GridLength(30);
            grid.RowDefinitions.Add(rowDefHeader);

            for (int y = 0; y < grid.ColumnDefinitions.Count; y++)
            {
                Rectangle rctHeader = new Rectangle();
                rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;

                if (y >= 1)
                {
                    rctHeader.Tag = columns[y - 1].ColumnName.Trim();
                }

                //rctHeader.Fill = headerBackgroundBrush; // new SolidColorBrush(Color.FromRgb(112, 146, 190)); // SystemColors.MenuHighlightBrush;
                Grid.SetRow(rctHeader, 0);
                Grid.SetColumn(rctHeader, y);
                grid.Children.Add(rctHeader);

                rctHeader.MouseUp += new MouseButtonEventHandler(colDef_MouseUp);
                rctHeader.MouseDown += new MouseButtonEventHandler(colDef_MouseDown);

                rctHeader.MouseEnter += new MouseEventHandler(rctHeader_MouseEnter);
                rctHeader.MouseLeave += new MouseEventHandler(rctHeader_MouseLeave);
                rctHeader.MouseLeftButtonDown += new MouseButtonEventHandler(rctHeader_MouseLeftButtonDown);
                rctHeader.MouseLeftButtonUp += new MouseButtonEventHandler(rctHeader_MouseLeftButtonUp);
            }

            int maxColumnLength = MaxColumnLength;

            TextBlock txtRowTotalHeader = new TextBlock();
            txtRowTotalHeader.Text = "Line";
            txtRowTotalHeader.Style = this.Resources["columnHeadingText"] as Style;

            Grid.SetRow(txtRowTotalHeader, 0);
            Grid.SetColumn(txtRowTotalHeader, 0);
            grid.Children.Add(txtRowTotalHeader);

            //if (checkboxColumnHeaders.IsChecked == false)
            //{
            //    grid.RowDefinitions[0].Height = new GridLength(1);
            //}

            for (int i = 0; i < columns.Count; i++)
            {
                if (i > MaxColumns)
                {
                    break;
                }
                TextBlock txtColHeader = new TextBlock();
                string columnName = columns[i].ColumnName.Trim();

                SortOrder order = SortOrder.Ascending;
                if (IsSortingBy(columnName, out order))
                {
                    if (order == SortOrder.Ascending)
                    {
                        Controls.AscendingSortArrow arrowAsc = new Controls.AscendingSortArrow();
                        arrowAsc.IsHitTestVisible = false;
                        arrowAsc.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        arrowAsc.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                        arrowAsc.Opacity = 0.5;
                        arrowAsc.Margin = new Thickness(2);
                        Grid.SetRow(arrowAsc, 0);
                        Grid.SetColumn(arrowAsc, i + 1);
                        grid.Children.Add(arrowAsc);
                    }
                    else
                    {
                        Controls.DescendingSortArrow arrowDesc = new Controls.DescendingSortArrow();
                        arrowDesc.IsHitTestVisible = false;
                        arrowDesc.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        arrowDesc.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                        arrowDesc.Opacity = 0.5;
                        arrowDesc.Margin = new Thickness(2);
                        Grid.SetRow(arrowDesc, 0);
                        Grid.SetColumn(arrowDesc, i + 1);
                        grid.Children.Add(arrowDesc);
                    }
                }

                if (GadgetOptions.InputVariableList.ContainsKey("usepromptsforcolumnnames") && GadgetOptions.InputVariableList["usepromptsforcolumnnames"].ToLower().Equals("true"))
                {
                    Field field = DashboardHelper.GetAssociatedField(columns[i].ColumnName);
                    if (field != null && field is RenderableField)
                    {
                        columnName = ((RenderableField)field).PromptText;
                    }
                }

                if (columnName.Length > maxColumnLength)
                {
                    columnName = columnName.Substring(0, maxColumnLength) + StringLiterals.ELLIPSIS;
                }

                txtColHeader.Text = columnName;
                txtColHeader.IsHitTestVisible = false;
                txtColHeader.Style = this.Resources["columnHeadingText"] as Style;

                txtColHeader.MouseUp += new MouseButtonEventHandler(colDef_MouseUp);
                txtColHeader.MouseDown += new MouseButtonEventHandler(colDef_MouseDown);

                Grid.SetRow(txtColHeader, 0);
                Grid.SetColumn(txtColHeader, i + 1);
                if (IsHostedByEnter || checkboxAllowUpdates.IsChecked == true)
                {
                    if (i < columns.Count - 1)
                    {
                        grid.Children.Add(txtColHeader);
                    }
                }
                else
                {
                    grid.Children.Add(txtColHeader);
                }
            }
        }

        /// <summary>
        /// Used to render the footer (last row) of a given frequency output grid
        /// </summary>
        /// <param name="strataValue">
        /// The strata value to which this grid cell belongs; used to search the list of grids and 
        /// return the proper System.Windows.Controls.Grid for text insertion.
        /// </param>
        /// <param name="footerRowIndex">The row index of the footer</param>
        /// <param name="totalRows">The total number of rows in this grid</param>
        private void RenderFrequencyFooter(string strataValue, int footerRowIndex, int totalRows)
        {
            Grid grid = GetStrataGrid(strataValue);

            RowDefinition rowDefTotals = new RowDefinition();
            rowDefTotals.Height = new GridLength(26);
            grid.RowDefinitions.Add(rowDefTotals); 

            TextBlock txtValTotals = new TextBlock();
            txtValTotals.Text = SharedStrings.TOTAL;
            txtValTotals.Margin = new Thickness(4, 0, 4, 0);
            txtValTotals.VerticalAlignment = VerticalAlignment.Center;
            txtValTotals.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtValTotals, footerRowIndex);
            Grid.SetColumn(txtValTotals, 0);
            grid.Children.Add(txtValTotals); 

            TextBlock txtFreqTotals = new TextBlock();
            txtFreqTotals.Text = totalRows.ToString();
            txtFreqTotals.Margin = new Thickness(4, 0, 4, 0);
            txtFreqTotals.VerticalAlignment = VerticalAlignment.Center;
            txtFreqTotals.HorizontalAlignment = HorizontalAlignment.Right;
            txtFreqTotals.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtFreqTotals, footerRowIndex);
            Grid.SetColumn(txtFreqTotals, 1);
            grid.Children.Add(txtFreqTotals);

            TextBlock txtPctTotals = new TextBlock();
            txtPctTotals.Text = (1).ToString("P");//SharedStrings.DASHBOARD_100_PERCENT_LABEL;
            txtPctTotals.Margin = new Thickness(4, 0, 4, 0);
            txtPctTotals.VerticalAlignment = VerticalAlignment.Center;
            txtPctTotals.HorizontalAlignment = HorizontalAlignment.Right;
            txtPctTotals.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtPctTotals, footerRowIndex);
            Grid.SetColumn(txtPctTotals, 2);
            grid.Children.Add(txtPctTotals);

            TextBlock txtAccuTotals = new TextBlock();
            txtAccuTotals.Text = (1).ToString("P");
            txtAccuTotals.Margin = new Thickness(4, 0, 4, 0);
            txtAccuTotals.VerticalAlignment = VerticalAlignment.Center;
            txtAccuTotals.HorizontalAlignment = HorizontalAlignment.Right;
            txtAccuTotals.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtAccuTotals, footerRowIndex);
            Grid.SetColumn(txtAccuTotals, 3);
            grid.Children.Add(txtAccuTotals);

            TextBlock txtCILowerTotals = new TextBlock();
            txtCILowerTotals.Text = StringLiterals.SPACE + StringLiterals.SPACE + StringLiterals.SPACE;
            txtCILowerTotals.Margin = new Thickness(4, 0, 4, 0);
            txtCILowerTotals.VerticalAlignment = VerticalAlignment.Center;
            txtCILowerTotals.HorizontalAlignment = HorizontalAlignment.Right;
            txtCILowerTotals.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtCILowerTotals, footerRowIndex);
            Grid.SetColumn(txtCILowerTotals, 4);
            grid.Children.Add(txtCILowerTotals);

            TextBlock txtUpperTotals = new TextBlock();
            txtUpperTotals.Text = StringLiterals.SPACE + StringLiterals.SPACE + StringLiterals.SPACE;
            txtUpperTotals.Margin = new Thickness(4, 0, 4, 0);
            txtUpperTotals.VerticalAlignment = VerticalAlignment.Center;
            txtUpperTotals.HorizontalAlignment = HorizontalAlignment.Right;
            txtUpperTotals.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtUpperTotals, footerRowIndex);
            Grid.SetColumn(txtUpperTotals, 5);
            grid.Children.Add(txtUpperTotals);

            Rectangle rctTotalsBar = new Rectangle();
            rctTotalsBar.Width = 0.1;// 100;
            rctTotalsBar.Fill = this.Resources["frequencyPercentBarBrush"] as SolidColorBrush;
            rctTotalsBar.Margin = new Thickness(2, 6, 2, 6);
            rctTotalsBar.HorizontalAlignment = HorizontalAlignment.Left;
            Grid.SetRow(rctTotalsBar, footerRowIndex);
            Grid.SetColumn(rctTotalsBar, 6);
            grid.Children.Add(rctTotalsBar);

            DoubleAnimation daBar = new DoubleAnimation();
            daBar.From = 1;
            daBar.To = 100;
            daBar.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            rctTotalsBar.BeginAnimation(Rectangle.WidthProperty, daBar);
        }

        private void DrawFrequencyBorders(string groupValue)
        {
            Grid grid = GetStrataGrid(groupValue);

            waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            int rdcount = 0;

            foreach (RowDefinition rd in grid.RowDefinitions)
            {
                int cdcount = 0;
                foreach (ColumnDefinition cd in grid.ColumnDefinitions)
                {
                    Rectangle rctBorder = new Rectangle();
                    Border border = new Border();
                    if (rdcount > 0)
                    {
                        border.Style = this.Resources["gridCellBorder"] as Style;
                    }
                    else
                    {
                        border.Style = this.Resources["gridHeaderCellBorder"] as Style;
                    }

                    if (rdcount == 0)
                    {
                        border.BorderThickness = new Thickness(border.BorderThickness.Left, border.BorderThickness.Bottom, border.BorderThickness.Right, border.BorderThickness.Bottom);
                    }
                    if (cdcount == 0)
                    {
                        border.BorderThickness = new Thickness(border.BorderThickness.Right, border.BorderThickness.Top, border.BorderThickness.Right, border.BorderThickness.Bottom);
                    }

                    border.Child = rctBorder;

                    Grid.SetRow(border, rdcount);
                    Grid.SetColumn(border, cdcount);
                    grid.Children.Add(border);
                    if (rdcount > 0)
                    {
                        Grid.SetZIndex(rctBorder, 0);

                        rctBorder.Style = this.Resources["gridCellRectangle"] as Style;// .Fill = Brushes.White;

                        if (IsHostedByEnter)
                        {
                            //rctBorder.MouseEnter += new MouseEventHandler(rowDef_MouseEnter);
                            //rctBorder.MouseLeave += new MouseEventHandler(rowDef_MouseLeave);
                            //rctBorder.MouseUp += new MouseButtonEventHandler(rowDef_MouseUp);
                            rctBorder.IsHitTestVisible = false;
                        }
                    }
                    cdcount++;
                }
                rdcount++;
            }

            SetGadgetToFinishedState();
        }

        /// <summary>
        /// Sets the gadget's state to 'finished' mode
        /// </summary>
        protected override void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (Grid freqGrid in StrataGridList)
            {
                freqGrid.Visibility = Visibility.Visible;
            }

            messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        /// <summary>
        /// Sets the gadget's state to 'finished with warning' mode
        /// </summary>
        /// <remarks>
        /// Common scenario for the usage of this method is when the distinct list of frequency values
        /// exceeds some built-in row limit. The output is limited to prevent the UI from locking up,
        /// and we want to let the user know that the output is limited while still showing them something.
        /// Thus we finish the rendering, but still show a message.
        /// </remarks>
        /// <param name="errorMessage">The warning message to display</param>
        protected override void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed; //waitCursor.Visibility = Visibility.Hidden;

            foreach (Grid freqGrid in StrataGridList)
            {
                freqGrid.Visibility = Visibility.Visible;
            }

            messagePanel.MessagePanelType = Controls.MessagePanelType.WarningPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        /// <summary>
        /// Sets the gadget's state to 'finished with error' mode
        /// </summary>
        /// <param name="errorMessage">The error message to display</param>
        protected override void RenderFinishWithError(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = Controls.MessagePanelType.ErrorPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            panelMain.Children.Clear();

            HideConfigPanel();
            CheckAndSetPosition();
        }

        /// <summary>
        /// Used to generate the list of variables and options for the GadgetParameters object
        /// </summary> 
        private void CreateInputVariableList()
        {
            //Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            //GadgetOptions.MainVariableName = string.Empty;
            //GadgetOptions.ColumnNames = new List<string>();

            //if (lbxDedupFields.SelectedItems.Count > 0)
            //{
            //    GadgetOptions.MainVariableNames = new List<string>();
            //    foreach (string s in lbxDedupFields.SelectedItems)
            //    {
            //        GadgetOptions.MainVariableNames.Add(s);
            //    }
            //}

            //GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            //GadgetOptions.InputVariableList = inputVariableList;

            ////Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            //////GadgetOptions.MainVariableName = string.Empty;
            //////GadgetOptions.WeightVariableName = string.Empty;
            //////GadgetOptions.StrataVariableNames = new List<string>();
            //////GadgetOptions.CrosstabVariableName = string.Empty;

            ////List<string> listFields = new List<string>();

            ////if (lbxDedupFields.SelectedItems.Count > 0)
            ////{
            ////    foreach (string item in lbxDedupFields.SelectedItems)
            ////    {
            ////        if (!string.IsNullOrEmpty(item))
            ////        {
            ////            listFields.Add(item);
            ////        }
            ////    }
            //}

            //listFields.Sort();

            //foreach (string field in listFields)
            //{
            //    inputVariableList.Add(field, "listfield");
            //}

            //if (!inputVariableList.ContainsKey("sortcolumnsbytaborder"))
            //{
            //    if (checkboxTabOrder.IsChecked == true)
            //    {
            //        inputVariableList.Add("sortcolumnsbytaborder", "true");
            //    }
            //    else
            //    {
            //        inputVariableList.Add("sortcolumnsbytaborder", "false");
            //    }
            //}

            //if (!inputVariableList.ContainsKey("usepromptsforcolumnnames"))
            //{
            //    if (checkboxUsePrompts.IsChecked == true)
            //    {
            //        inputVariableList.Add("usepromptsforcolumnnames", "true");
            //    }
            //    else
            //    {
            //        inputVariableList.Add("usepromptsforcolumnnames", "false");
            //    }
            //}

            //if (!inputVariableList.ContainsKey("showcolumnheadings"))
            //{
            //    if (checkboxColumnHeaders.IsChecked == true)
            //    {
            //        inputVariableList.Add("showcolumnheadings", "true");
            //    }
            //    else
            //    {
            //        inputVariableList.Add("showcolumnheadings", "false");
            //    }
            //}

            //if (!inputVariableList.ContainsKey("showlinecolumn"))
            //{
            //    if (checkboxLineColumn.IsChecked == true)
            //    {
            //        inputVariableList.Add("showlinecolumn", "true");
            //    }
            //    else
            //    {
            //        inputVariableList.Add("showlinecolumn", "false");
            //    }
            //}

            //if (!inputVariableList.ContainsKey("shownulllabels"))
            //{
            //    if (checkboxShowNulls.IsChecked == true)
            //    {
            //        inputVariableList.Add("shownulllabels", "true");
            //    }
            //    else
            //    {
            //        inputVariableList.Add("shownulllabels", "false");
            //    }
            //}

            //if (checkboxListLabels.IsChecked == true)
            //{
            //    GadgetOptions.ShouldShowCommentLegalLabels = true;
            //}
            //else
            //{
            //    GadgetOptions.ShouldShowCommentLegalLabels = false;
            //}

            //if (lbxSortFields.Items.Count > 0)
            //{
            //    foreach (string item in lbxSortFields.Items)
            //    {
            //        if (!string.IsNullOrEmpty(item))
            //        {
            //            string baseStr = item;

            //            if (baseStr.EndsWith("(ascending)"))
            //            {
            //                baseStr = "[" + baseStr.Remove(baseStr.Length - 12) + "] ASC";
            //            }
            //            if (baseStr.EndsWith("(descending)"))
            //            {
            //                baseStr = "[" + baseStr.Remove(baseStr.Length - 13) + "] DESC";
            //            }
            //            inputVariableList.Add(baseStr, "sortfield");
            //        }
            //    }
            //}

            //if (cbxGroupField.SelectedIndex >= 0)
            //{
            //    if (!string.IsNullOrEmpty(cbxGroupField.SelectedItem.ToString()))
            //    {
            //        GadgetOptions.StrataVariableNames.Add(cbxGroupField.SelectedItem.ToString());
            //    }
            //}

            //if (StrataGridList.Count >= 1)
            //{
            //    Grid grid = StrataGridList[0];
            //    SortedDictionary<int, string> sortColumnDictionary = new SortedDictionary<int, string>();

            //    foreach (UIElement element in grid.Children)
            //    {
            //        if (Grid.GetRow(element) == 0 && element is TextBlock)
            //        {
            //            TextBlock txtColumnName = element as TextBlock;
            //            //columnOrder.Add(txtColumnName.Text);
            //            sortColumnDictionary.Add(Grid.GetColumn(element), txtColumnName.Text);
            //        }
            //    }

            //    columnOrder = new List<string>();
            //    foreach (KeyValuePair<int, string> kvp in sortColumnDictionary)
            //    {
            //        columnOrder.Add(kvp.Value);
            //    }

            //    if (columnOrder.Count == listFields.Count || columnOrder.Count == (listFields.Count + 1))
            //    {
            //        bool same = true;
            //        foreach (string s in listFields)
            //        {
            //            if (!columnOrder.Contains(s))
            //            {
            //                same = false;
            //            }
            //        }

            //        if (same)
            //        {
            //            WordBuilder wb = new WordBuilder("^");
            //            foreach (string s in columnOrder)
            //            {
            //                wb.Add(s);
            //            }

            //            inputVariableList.Add("customusercolumnsort", wb.ToString());
            //        }
            //        else
            //        {
            //            columnOrder = new List<string>();
            //        }
            //    }
            //    else
            //    {
            //        columnOrder = new List<string>();
            //    }
            //}

            //inputVariableList.Add("maxcolumns", MaxColumns.ToString());
            //inputVariableList.Add("maxrows", MaxRows.ToString());

            //GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            //GadgetOptions.InputVariableList = inputVariableList;

        }

        /// <summary>
        /// Used to construct the gadget and assign events
        /// </summary>        
        protected override void Construct()
        {
            this.Parameters = new DuplicatesListParameters();

            if (!string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)"))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            StrataGridList = new List<Grid>();
            StrataExpanderList = new List<Expander>();
            mnuCopy.Click += new RoutedEventHandler(mnuCopy_Click);
            mnuSendDataToHTML.Click += new RoutedEventHandler(mnuSendDataToHTML_Click);

#if LINUX_BUILD
            mnuSendDataToExcel.Visibility = Visibility.Collapsed;
#else
            mnuSendDataToExcel.Visibility = Visibility.Visible;
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot; Microsoft.Win32.RegistryKey excelKey = key.OpenSubKey("Excel.Application"); bool excelInstalled = excelKey == null ? false : true; key = Microsoft.Win32.Registry.ClassesRoot;
            excelKey = key.OpenSubKey("Excel.Application");
            excelInstalled = excelKey == null ? false : true;

            if (!excelInstalled)
            {
                mnuSendDataToExcel.Visibility = Visibility.Collapsed;
            }
            else
            {
                mnuSendDataToExcel.Click += new RoutedEventHandler(mnuSendDataToExcel_Click);
            }
#endif

            mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
            mnuClose.Click += new RoutedEventHandler(mnuClose_Click);
            this.IsProcessing = false;
            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            #region Translation
            ConfigExpandedTitle.Text = "Deduplication"; //TODO: Add to Shared Strings
                                                       //DashboardSharedStrings.GADGET_CONFIG_TITLE_FREQUENCY;
            tblockMainVariable.Text = "Deduplication of:";  //DashboardSharedStrings.GADGET_FREQUENCY_VARIABLE;  //TODO: Add to Shared Strings
            tblockDisplayVariable.Text = "Additional fields to display:";  //DashboardSharedStrings.GADGET_FREQUENCY_VARIABLE;  //TODO: Add to Shared Strings

            //tblockMainVariable.Text = DashboardSharedStrings.GADGET_FREQUENCY_VARIABLE;
            btnRun.Content = DashboardSharedStrings.GADGET_RUN_BUTTON;
            #endregion // Translation

            base.Construct();
        }

        /// <summary>
        /// Shows the field's column name in the first column heading for the output grid. 
        /// </summary>
        private void ShowFieldName()
        {
            foreach (Grid grid in this.StrataGridList)
            {
                IEnumerable<UIElement> elements = grid.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == 0 && Grid.GetColumn(x) == 0);
                TextBlock txt = null;
                foreach (UIElement element in elements)
                {
                    if (element is TextBlock)
                    {
                        txt = element as TextBlock;
                        break;
                    }
                }

                if (txt != null)
                {
                    txt.Text = GadgetOptions.MainVariableName;
                }
            }
        }

        /// <summary>
        /// Shows the field's prompt value (if applicable) in the first column heading for the output grid.
        /// </summary>
        private void ShowFieldPrompt()
        {
            foreach (Grid grid in this.StrataGridList)
            {
                IEnumerable<UIElement> elements = grid.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == 0 && Grid.GetColumn(x) == 0);
                TextBlock txt = null;
                foreach (UIElement element in elements)
                {
                    if (element is TextBlock)
                    {
                        txt = element as TextBlock;
                        break;
                    }
                }

                if (txt != null)
                {
                    Field field = DashboardHelper.GetAssociatedField(GadgetOptions.MainVariableName);
                    if (field != null && field is IDataField)
                    {
                        txt.Text = (field as IDataField).PromptText;
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see whether a valid numeric digit was pressed for numeric-only conditions
        /// </summary>
        /// <param name="keyChar">The key that was pressed</param>
        /// <returns>Whether the input was a valid number character</returns>
        private bool ValidNumberChar(string keyChar)
        {
            System.Globalization.NumberFormatInfo numberFormatInfo = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;

            for (int i = 0; i < keyChar.Length; i++)
            {
                char ch = keyChar[i];
                if (!Char.IsDigit(ch))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Collapses the output for the gadget
        /// </summary>
        public override void CollapseOutput()
        {
            foreach (Expander expander in this.StrataExpanderList)
            {
                expander.Visibility = System.Windows.Visibility.Collapsed;
            }

            foreach (Grid grid in this.StrataGridList)
            {
                grid.Visibility = System.Windows.Visibility.Collapsed;
                Border border = new Border();
                if (grid.Parent is Border)
                {
                    border = (grid.Parent) as Border;
                    border.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

            if (!string.IsNullOrEmpty(this.infoPanel.Text))
            {
                this.infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            }

            this.messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            this.infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
            IsCollapsed = true;
        }

        /// <summary>
        /// Expands the output for the gadget
        /// </summary>
        public override void ExpandOutput()
        {
            foreach (Expander expander in this.StrataExpanderList)
            {
                expander.Visibility = System.Windows.Visibility.Visible;
            }

            foreach (Grid grid in this.StrataGridList)
            {
                grid.Visibility = System.Windows.Visibility.Visible;
                Border border = new Border();
                if (grid.Parent is Border)
                {
                    border = (grid.Parent) as Border;
                    border.Visibility = System.Windows.Visibility.Visible;
                }
            }

            if (this.messagePanel.MessagePanelType != Controls.MessagePanelType.StatusPanel)
            {
                this.messagePanel.Visibility = System.Windows.Visibility.Visible;
            }

            if (!string.IsNullOrEmpty(this.infoPanel.Text))
            {
                this.infoPanel.Visibility = System.Windows.Visibility.Visible;
            }
            IsCollapsed = false;
        }

        /// <summary>
        /// Closes the gadget
        /// </summary>
        protected override void CloseGadget()
        {
            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            if (worker != null)
            {
                worker.DoWork -= new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerCompleted -= new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
            }
            if (baseWorker != null)
            {
                baseWorker.DoWork -= new System.ComponentModel.DoWorkEventHandler(Execute);
            }

            this.GadgetStatusUpdate -= new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation -= new GadgetCheckForCancellationHandler(IsCancelled);

            for (int i = 0; i < StrataGridList.Count; i++)
            {
                StrataGridList[i].Children.Clear();
            }
            for (int i = 0; i < StrataExpanderList.Count; i++)
            {
                StrataExpanderList[i].Content = null;
            }
            this.StrataExpanderList.Clear();
            this.StrataGridList.Clear();
            this.panelMain.Children.Clear();

            base.CloseGadget();

            GadgetOptions = null;
        }

        /// <summary>
        /// Clears the gadget's output
        /// </summary>
        private void ClearResults()
        {
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

            foreach (Grid grid in StrataGridList)
            {
                grid.Children.Clear();
                grid.RowDefinitions.Clear();
                if (grid.Parent is Border)
                {
                    Border border = (grid.Parent) as Border;                    
                    panelMain.Children.Remove(border);
                }                
            }

            foreach (Expander expander in StrataExpanderList)
            {
                if (panelMain.Children.Contains(expander))
                {
                    panelMain.Children.Remove(expander);
                }
            }

            panelMain.Children.Clear();

            StrataGridList.Clear();
            StrataExpanderList.Clear();
        }

        private void properties_ChangesAccepted(object sender, EventArgs e)
        {
            Controls.GadgetProperties.DuplicatesListProperties properties = Popup.Content as Controls.GadgetProperties.DuplicatesListProperties;
            this.Parameters = properties.Parameters;
            this.DataFilters = properties.DataFilters;
            this.CustomOutputHeading = Parameters.GadgetTitle;
            this.CustomOutputDescription = Parameters.GadgetDescription;
            Popup.Close();
            if (properties.HasSelectedFields)
            {
                RefreshResults();
            }
        }

        private void properties_Cancelled(object sender, EventArgs e)
        {
            Popup.Close();
        }

        #endregion // Private and Protected Methods

        #region Public Methods
        #region IGadget Members

        private void txtMaxColumns_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Util.IsWholeNumber(e.Text);
            base.OnPreviewTextInput(e);
        }

        //private int MaxRows
        //{
        //    get
        //    {
        //        int maxRows = 50;
        //        bool success = int.TryParse(txtMaxRows.Text, out maxRows);
        //        if (!success)
        //        {
        //            return 50;
        //        }
        //        else
        //        {
        //            if (maxRows >= MAX_ROW_LIMIT)
        //            {
        //                return MAX_ROW_LIMIT;
        //            }
        //            else
        //            {
        //                return maxRows;
        //            }
        //        }
        //    }
        //}

        private int MaxColumns
        {
            get
            {
                return this.maxColumns;
            }
            set
            {
                if (value <= 1)
                {
                    this.maxColumns = 1;
                }
                else
                {
                    this.maxColumns = value;
                }
            }
        }

        private int MaxColumnLength
        {
            get
            {
                int maxColumnLength = 24;
                bool success = int.TryParse(txtMaxColumnLength.Text, out maxColumnLength);
                if (!success)
                {
                    return 24;
                }
                else
                {
                    return maxColumnLength;
                }
            }
        }

        private void SetCustomColumnSort(DataTable table)
        {
            #region Input Validation
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            #endregion // Input Validation

            if (columnOrder == null || columnOrder.Count == 0)
            {
                return;
            }

            if (columnOrder.Contains("Line"))
            {
                columnOrder.Remove("Line");
            }

            for (int i = 0; i < columnOrder.Count; i++)
            {
                string s = columnOrder[i];
                if (table.Columns.Contains(s))
                {
                    table.Columns[s].SetOrdinal(i);
                }
            }
        }

        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
            base.SetGadgetToProcessingState();
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            base.SetGadgetToFinishedState();
        }

        /// <summary>
        /// Initiates a refresh of the gadget's output
        /// </summary>
        public override void RefreshResults()
        {
            if (!LoadingCombos && Parameters != null)
            {
                //CreateInputVariableList();

                if (IsHostedByEnter)
                {
                    HideConfigPanel();
                }
                waitPanel.Visibility = System.Windows.Visibility.Visible;

                messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

                baseWorker = new BackgroundWorker();
                //baseWorker.WorkerSupportsCancellation = true;
                baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                baseWorker.RunWorkerAsync();

                base.RefreshResults();
                
                //FROM FREQUENCY CONTROL//
                //infoPanel.Visibility = System.Windows.Visibility.Collapsed;
                //waitPanel.Visibility = System.Windows.Visibility.Visible;
                //messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                //descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                //baseWorker = new BackgroundWorker();
                //baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                //baseWorker.RunWorkerAsync();
                //base.RefreshResults();
            }
            else
            {
                ClearResults();
                waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public override void UpdateVariableNames()
        {
            //FillComboboxes(true);
        }

        /// <summary>
        /// Generates Xml representation of this gadget
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            //Dictionary<string, string> inputVariableList = GadgetOptions.InputVariableList;

            //CustomOutputHeading = headerPanel.Text;            
            ////CustomOutputDescription = txtOutputDescription.Text.Replace("<", "&lt;");
            //CustomOutputDescription = descriptionPanel.Text; //txtOutputDescription.Text.Replace("<", "&lt;");

            string xmlString = String.Empty;
            //"<maxRows>" + MaxRows.ToString() + "</maxRows>" +
            //"<maxColumnNameLength>" + MaxColumnLength.ToString() + "</maxColumnNameLength>" +
            //"<sortColumnsByTabOrder>" + checkboxTabOrder.IsChecked.ToString() + "</sortColumnsByTabOrder>" +
            //"<useFieldPrompts>" + checkboxUsePrompts.IsChecked.ToString() + "</useFieldPrompts>" +
            //"<showListLabels>" + checkboxListLabels.IsChecked + "</showListLabels>" +
            //"<showLineColumn>" + checkboxLineColumn.IsChecked.ToString() + "</showLineColumn>" +
            //"<showColumnHeadings>" + checkboxColumnHeaders.IsChecked.ToString() + "</showColumnHeadings>" +
            //"<showNullLabels>" + checkboxShowNulls.IsChecked.ToString() + "</showNullLabels>" + 
            ////"<alternatingRowColors>" + checkboxAltRowColors.IsChecked.ToString() + "</alternatingRowColors>" +
            ////"<allowUpdates>" + checkboxAllowUpdates.IsChecked.ToString() + "</allowUpdates>" +
            //"<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            //"<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>" +
            //"<customCaption>" + CustomOutputCaption + "</customCaption>";

            //xmlString = xmlString + SerializeAnchors();

            //if (inputVariableList.ContainsKey("customusercolumnsort"))
            //{
            //    string columns = inputVariableList["customusercolumnsort"];
            //    xmlString = xmlString + "<customusercolumnsort>" + columns + "</customusercolumnsort>";                
            //}
            //else if (columnOrder != null && columnOrder.Count > 0) // when user has re-ordered columns but not refreshed
            //{
            //    WordBuilder wb = new WordBuilder("^");
            //    for (int i = 0; i < columnOrder.Count; i++)
            //    {
            //        wb.Add(columnOrder[i]);
            //    }
            //    xmlString = xmlString + "<customusercolumnsort>" + wb.ToString() + "</customusercolumnsort>";
            //}

            System.Xml.XmlElement element = doc.CreateElement("duplicatesListGadget");
            element.InnerXml = xmlString;
            element.AppendChild(SerializeFilters(doc));

            System.Xml.XmlAttribute id = doc.CreateAttribute("id");
            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

            id.Value = this.UniqueIdentifier.ToString();
            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");
            collapsed.Value = IsCollapsed.ToString();
            type.Value = "EpiDashboard.LineListControl";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);

            //if (lbxDedupFields.Items.Count > 0 && lbxDedupFields.SelectedItems.Count > 0)
            //{
            //    string xmlListItemString = string.Empty;
            //    XmlElement listItemElement = doc.CreateElement("listFields");

            //    foreach (string s in lbxDedupFields.SelectedItems)
            //    {
            //        xmlListItemString = xmlListItemString + "<listField>" + s.Replace("<", "&lt;") + "</listField>";
            //    }

            //    listItemElement.InnerXml = xmlListItemString;
            //    element.AppendChild(listItemElement);
            //}

            //if (lbxSortFields.Items.Count > 0)
            //{
            //    string xmlSortString = string.Empty;
            //    XmlElement sortElement = doc.CreateElement("sortFields");

            //    foreach (string s in lbxSortFields.Items)
            //    {
            //        xmlSortString = xmlSortString + "<sortField>" + s.Replace("<", "&lt;") + "</sortField>";
            //    }

            //    sortElement.InnerXml = xmlSortString;
            //    element.AppendChild(sortElement);
            //}

            return element;
        }

        /// <summary>
        /// Creates the frequency gadget from an Xml element
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        public override void CreateFromXml(XmlElement element)
        {
            //this.LoadingCombos = true;
            //HideConfigPanel();
            //infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            //messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            //foreach (XmlElement child in element.ChildNodes)
            //{
            //    switch (child.Name.ToLower())
            //    {
            //        case "mainvariables":
            //            lbxDedupFields.SelectedItems.Add(child.InnerText.Replace("&lt;", "<"));
            //            break;
            //        case "usefieldprompts":
            //            bool usePrompts = false;
            //            bool.TryParse(child.InnerText, out usePrompts);
            //            break;
            //        case "customheading":
            //            if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
            //            {
            //                this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<"); ;
            //            }
            //            break;
            //        case "customdescription":
            //            if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
            //            {
            //                this.CustomOutputDescription = child.InnerText.Replace("&lt;", "<");
            //                if (!string.IsNullOrEmpty(CustomOutputDescription) && !CustomOutputHeading.Equals("(none)"))
            //                {
            //                    descriptionPanel.Text = CustomOutputDescription;
            //                    descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
            //                }
            //                else
            //                {
            //                    descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
            //                }
            //            }
            //            break;
            //        case "customcaption":
            //            this.CustomOutputCaption = child.InnerText;
            //            break;
            //        case "datafilters":
            //            this.DataFilters = new DataFilters(this.DashboardHelper);
            //            this.DataFilters.CreateFromXml(child);
            //            break;
            //    }
            //}

            //base.CreateFromXml(element);
            
            //this.LoadingCombos = false;
            //RefreshResults();
            //HideConfigPanel();
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false)
        {
            if (IsCollapsed) return string.Empty;

            StringBuilder htmlBuilder = new StringBuilder();
            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            if (CustomOutputHeading == null || (string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)")))
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Duplicates List</h2>");
            }
            else if (CustomOutputHeading != "(none)")
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            htmlBuilder.AppendLine("<p class=\"gadgetOptions\"><small>");
            htmlBuilder.AppendLine("<br />");

            if (Parameters.ColumnNames.Count > 0)
            {
                WordBuilder wb = new WordBuilder(", ");
                foreach (string s in Parameters.ColumnNames)
                {
                    wb.Add(s);
                }
                htmlBuilder.AppendLine("<em>Strata variable(s):</em> <strong>" + wb.ToString() + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }

            htmlBuilder.AppendLine("<br />");
            htmlBuilder.AppendLine("</small></p>");

            if (!string.IsNullOrEmpty(CustomOutputDescription))
            {
                htmlBuilder.AppendLine("<p class=\"gadgetsummary\">" + CustomOutputDescription + "</p>");
            }

            if (!string.IsNullOrEmpty(messagePanel.Text) && messagePanel.Visibility == Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + messagePanel.Text + "</strong></small></p>");
            }

            if (!string.IsNullOrEmpty(infoPanel.Text) && infoPanel.Visibility == Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + infoPanel.Text + "</strong></small></p>");
            }

            foreach (Grid grid in this.StrataGridList)
            {
                string gridName = grid.Tag.ToString();

                string summaryText = "This tables represents fields of duplicate values. ";
                
                htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" summary=\"" + summaryText + "\">");

                if (string.IsNullOrEmpty(CustomOutputCaption))
                {
                    if (Parameters.ColumnNames.Count > 0)
                    {
                        htmlBuilder.AppendLine("<caption>" + grid.Tag + "</caption>");
                    }
                }
                else
                {
                    htmlBuilder.AppendLine("<caption>" + CustomOutputCaption + "</caption>");
                }

                double barWidth = 0.0;

                foreach (UIElement control in grid.Children)
                {
                    if (control is TextBlock)
                    {
                        int rowNumber = Grid.GetRow(control);
                        int columnNumber = Grid.GetColumn(control);
                        string tableDataTagOpen = "<td>";
                        string tableDataTagClose = "</td>";

                        if (rowNumber == 0)
                        {
                            tableDataTagOpen = "<th>";
                            tableDataTagClose = "</th>";
                        }
                        if (columnNumber == 0)
                        {
                            if (((double)rowNumber) % 2.0 == 1)
                            {
                                htmlBuilder.AppendLine("<tr class=\"altcolor\">");
                            }
                            else
                            {
                                htmlBuilder.AppendLine("<tr>");
                            }
                        }
                        if (columnNumber == 0 && rowNumber > 0)
                        {
                            tableDataTagOpen = "<td class=\"value\">";
                        }

                        string value = ((TextBlock)control).Text;

                        if (string.IsNullOrEmpty(value))
                        {
                            value = "&nbsp;";
                        }
                        string formattedValue = value;

                        if (rowNumber == grid.RowDefinitions.Count - 1)
                        {
                            formattedValue = "<span class=\"total\">" + value + "</span>";
                        }

                        htmlBuilder.AppendLine(tableDataTagOpen + formattedValue + tableDataTagClose);

                        if (columnNumber == 2 && rowNumber > 0)
                        {
                            barWidth = 0;
                            double.TryParse(value.Trim().TrimEnd('%').Trim(), out barWidth);
                        }

                        if (columnNumber >= grid.ColumnDefinitions.Count - 2)
                        {
                            if (rowNumber > 0)
                            {
                                htmlBuilder.AppendLine("<td class=\"value\"><div class=\"percentBar\" style=\"width: " + ((int)barWidth * 2).ToString() + "px;\"></td>");
                            }
                            else
                            {
                                htmlBuilder.AppendLine(tableDataTagOpen + tableDataTagClose);
                            }
                            htmlBuilder.AppendLine("</tr>");
                        }
                    }
                }

                htmlBuilder.AppendLine("</table>");
            }
            return htmlBuilder.ToString();
        }

        /// <summary>
        /// Gets/sets the gadget's custom output heading
        /// </summary>
        public override string CustomOutputHeading
        {
            get
            {
                return this.customOutputHeading;
            }
            set
            {
                this.customOutputHeading = value;
                headerPanel.Text = CustomOutputHeading;
            }
        }

        /// <summary>
        /// Gets/sets the gadget's custom output description
        /// </summary>
        public override string CustomOutputDescription
        {
            get
            {
                return this.customOutputDescription;
            }
            set
            {
                this.customOutputDescription = value;
                descriptionPanel.Text = CustomOutputDescription;
            }
        }

        /// <summary>
        /// Gets/sets the gadget's custom output caption for its table or image output components, if applicable
        /// </summary>
        public override string CustomOutputCaption
        {
            get
            {
                return this.customOutputCaption;
            }
            set
            {
                this.customOutputCaption = value;
            }
        }
        #endregion

        /// <summary>
        /// Returns the gadget's description as a string.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "Duplicates List Gadget";
        }

        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
            Controls.GadgetProperties.DuplicatesListProperties properties = new Controls.GadgetProperties.DuplicatesListProperties(this.DashboardHelper, this, (DuplicatesListParameters)Parameters, StrataGridList, columnOrder);

            properties.Width = 800;
            properties.Height = 600;

            if ((System.Windows.SystemParameters.PrimaryScreenWidth / 1.2) > properties.Width)
            {
                properties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.2);
            }

            if ((System.Windows.SystemParameters.PrimaryScreenHeight / 1.2) > properties.Height)
            {
                properties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.2);
            }

            properties.Cancelled += new EventHandler(properties_Cancelled);
            properties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);
            Popup.Content = properties;
            Popup.Show();
        }

        #endregion

        #region Event Handlers

        public event Mapping.RecordSelectedHandler RecordSelected;

        /// <summary>
        /// Handles the Checked event for checkboxUsePrompts
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void checkboxUsePrompts_Checked(object sender, RoutedEventArgs e)
        {
            ShowFieldPrompt();
        }

        /// <summary>
        /// Handles the Unchecked event for checkboxUsePrompts
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void checkboxUsePrompts_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowFieldName();
        }

        /// <summary>
        /// Handles the MouseLeave event for the value header in the output grid
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void txtValHeader_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Handles the MouseEnter event for the value header in the output grid
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void txtValHeader_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// Handles the MouseLeftButtonUp event for the value header in the output grid
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void txtValHeader_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (checkboxUsePrompts.IsChecked == true) checkboxUsePrompts.IsChecked = false;
            //else checkboxUsePrompts.IsChecked = true;
        }

        /// <summary>
        /// Handles the check / unchecked events
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void checkboxCheckChanged(object sender, RoutedEventArgs e)
        {
            RefreshResults();
        }

        /// <summary>
        /// Fired when the user changes a field selection
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cbxField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public bool IsHostedByEnter
        {
            get
            {
                return isHostedByEnter;
            }
            set
            {
                isHostedByEnter = value;
                if (value)
                {
                    //imgClose.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    //imgClose.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }
        /// <summary>
        /// Fired when the user clicks the Run button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (LoadingCombos)
            {
                return;
            }
            RefreshResults();
        }

        /// <summary>
        /// Fired when the user changes a column selection
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void lbxColumns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //Debug.Print("Background worker thread for frequency gadget was cancelled or ran to completion.");
            System.Threading.Thread.Sleep(100);
            this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
        }

        /// <summary>
        /// Handles the DoWorker event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                //Dictionary<string, string> inputVariableList = ((GadgetParameters)e.Argument).InputVariableList;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                DuplicatesListParameters DuplicateParameters = (DuplicatesListParameters)Parameters;

                AddOutputGridDelegate addGrid = new AddOutputGridDelegate(AddLineListGrid);
                SetGridTextDelegate setText = new SetGridTextDelegate(SetGridText);
                SetGridImageDelegate setImage = new SetGridImageDelegate(SetGridImage);
                AddGridRowDelegate addRow = new AddGridRowDelegate(AddGridRow);
                RenderFrequencyHeaderDelegate renderHeader = new RenderFrequencyHeaderDelegate(RenderFrequencyHeader);
                DrawFrequencyBordersDelegate drawBorders = new DrawFrequencyBordersDelegate(DrawFrequencyBorders);

                Configuration config = DashboardHelper.Config;
                string yesValue = config.Settings.RepresentationOfYes;
                string noValue = config.Settings.RepresentationOfNo;

                string groupVar = string.Empty;
                int maxRows = 50;
                bool exceededMaxRows = false;
                bool exceededMaxColumns = false;
                bool showLineColumn = true;
                bool showColumnHeadings = true;
                bool showNullLabels = true;

                //if (inputVariableList.ContainsKey("maxcolumns"))
                //{
                //    maxColumns = int.Parse(inputVariableList["maxcolumns"]);
                //}

                //if (inputVariableList.ContainsKey("maxrows"))
                //{
                //    maxRows = int.Parse(inputVariableList["maxrows"]);
                //}

                //if (inputVariableList.ContainsKey("showcolumnheadings"))
                //{
                //    showColumnHeadings = bool.Parse(inputVariableList["showcolumnheadings"]);
                //}
                showColumnHeadings = DuplicateParameters.ShowColumnHeadings;

                //if (inputVariableList.ContainsKey("showlinecolumn"))
                //{
                //    showLineColumn = bool.Parse(inputVariableList["showlinecolumn"]);
                //}
                showLineColumn = DuplicateParameters.ShowLineColumn;

                //if (inputVariableList.ContainsKey("shownulllabels"))
                //{
                //    showNullLabels = bool.Parse(inputVariableList["shownulllabels"]);
                //}
                showNullLabels = DuplicateParameters.ShowNullLabels;

                //System.Threading.Thread.Sleep(4000); // Artifically inflating process time to see how the 'loading' screen looks. TODO: REMOVE LATER

                if (IsHostedByEnter)
                {
                    if (System.Windows.SystemParameters.IsSlowMachine == true)
                    {
                        maxColumns = 512;
                    }
                    else
                    {
                        maxColumns = 1024;
                    }
                }
                else
                {
                    int renderingTier = (RenderCapability.Tier >> 16);
                    if (renderingTier >= 2)
                    {
                        maxColumns = 128;
                    }
                    else if (renderingTier >= 1)
                    {
                        maxColumns = 64;
                    }
                    else
                    {
                        maxColumns = 24;
                    }

                    if (System.Windows.SystemParameters.IsSlowMachine == true)
                    {
                        maxColumns = 24;
                    }
                }

                try
                {
                    //GadgetOptions.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    //GadgetOptions.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);
                    if (this.DataFilters != null)
                    {
                        DuplicateParameters.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                    }
                    else
                    {
                        DuplicateParameters.CustomFilter = string.Empty;
                    }

                    // Commented this out so the build would compile
                    //List<DataTable> lineListTables = DashboardHelper.GenerateLineList(GadgetOptions);
                    List<DataTable> lineListTables = DashboardHelper.GenerateDeduplicationList(DuplicateParameters);

                    if (lineListTables == null || lineListTables.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        Debug.Print("Deduplication thread cancelled");
                        return;
                    }
                    else if (lineListTables.Count == 1 && lineListTables[0].Rows.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        Debug.Print("Deduplication thread cancelled");
                        return;
                    }
                    else if (worker.CancellationPending)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        Debug.Print("Deduplication thread cancelled");
                        return;
                    }
                    else
                    {
                        string formatString = string.Empty;

                        foreach (DataTable listTable in lineListTables)
                        {
                            string strataValue = listTable.TableName;
                            if (listTable.Rows.Count == 0)
                            {
                                continue;
                            }
                            this.Dispatcher.BeginInvoke(addGrid, groupVar, listTable.TableName, listTable.Columns.Count);
                        }

                        SetGadgetStatusMessage(SharedStrings.DASHBOARD_GADGET_STATUS_DISPLAYING_OUTPUT);

                        foreach (DataTable listTable in lineListTables)
                        {
                            string strataValue = listTable.TableName;
                            if (listTable.Rows.Count == 0)
                            {
                                continue;
                            }
                            string tableHeading = listTable.TableName;

                            if (lineListTables.Count > 1)
                            {
                                //tableHeading = freqVar; ???
                            }

                            SetCustomColumnSort(listTable);

                            this.Dispatcher.BeginInvoke(renderHeader, strataValue, tableHeading, listTable.Columns);

                            rowCount = 1;

                            if (listTable.Columns.Count == 0)
                            {
                                throw new ApplicationException("There are no columns to display in this list. If specifying a group variable, ensure the group variable contains data fields.");
                            }

                            int[] totals = new int[listTable.Columns.Count - 1];
                            columnCount = 1;

                            foreach (System.Data.DataRow row in listTable.Rows)
                            {
                                this.Dispatcher.Invoke(addRow, strataValue, -1);
                                this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(rowCount.ToString(), new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Stretch, TextAlignment.Center, rowCount, 0, Visibility.Visible), FontWeights.Normal);
                                columnCount = 1;

                                foreach (DataColumn column in listTable.Columns)
                                {
                                    if (columnCount > maxColumns + 1)
                                    {
                                        exceededMaxColumns = true;
                                        break;
                                    }

                                    string displayValue = row[column.ColumnName].ToString();

                                    if (DashboardHelper.IsUserDefinedColumn(column.ColumnName))
                                    {
                                        displayValue = DashboardHelper.GetFormattedOutput(column.ColumnName, row[column.ColumnName]);
                                    }
                                    else
                                    {
                                        Field field = null;
                                        string columnType = string.Empty;

                                        foreach (DataRow fieldRow in DashboardHelper.FieldTable.Rows)
                                        {
                                            if (fieldRow["columnname"].Equals(column.ColumnName))
                                            {
                                                columnType = fieldRow["datatype"].ToString();
                                                if (fieldRow["epifieldtype"] is Field)
                                                {
                                                    field = fieldRow["epifieldtype"] as Field;
                                                }
                                                break;
                                            }
                                        }

                                        if ((field != null && (field is YesNoField || field is CheckBoxField)) || column.DataType.ToString().Equals("System.Boolean"))
                                        {
                                            if (row[column.ColumnName].ToString().Equals("1") || row[column.ColumnName].ToString().ToLower().Equals("true"))
                                                displayValue = yesValue;
                                            else if (row[column.ColumnName].ToString().Equals("0") || row[column.ColumnName].ToString().ToLower().Equals("false"))
                                                displayValue = noValue;
                                        }
                                        else if ((field != null && field is DateField) || (!DashboardHelper.DateColumnRequiresTime(listTable, listTable.Columns[column.ColumnName].ColumnName)))
                                        {
                                            displayValue = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:d}", row[column.ColumnName]);
                                        }
                                        else if (field != null && field is TimeField)
                                        {
                                            displayValue = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:T}", row[column.ColumnName]);
                                        }
                                        else
                                        {
                                            displayValue = DashboardHelper.GetFormattedOutput(column.ColumnName, row[column.ColumnName]);
                                        }
                                    }

                                    if (string.IsNullOrEmpty(displayValue))
                                    {
                                        if (showNullLabels)
                                        {
                                            displayValue = config.Settings.RepresentationOfMissing;
                                        }
                                        else
                                        {
                                            displayValue = string.Empty;
                                        }
                                    }

                                    if (column.DataType.ToString().Equals("System.DateTime") || column.DataType.ToString().Equals("System.Int32") || column.DataType.ToString().Equals("System.Double") || column.DataType.ToString().Equals("System.Single"))
                                    {
                                        if (column.ColumnName.Equals("UniqueKey"))
                                        {
                                            this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(displayValue, new Thickness(8, 6, 8, 6), VerticalAlignment.Stretch, HorizontalAlignment.Stretch, TextAlignment.Right, rowCount, columnCount, Visibility.Collapsed), FontWeights.Normal);
                                        }
                                        else
                                        {
                                            this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(displayValue, new Thickness(8, 6, 8, 6), VerticalAlignment.Stretch, HorizontalAlignment.Stretch, TextAlignment.Right, rowCount, columnCount, Visibility.Visible), FontWeights.Normal);
                                        }
                                    }
                                    else if (column.DataType == typeof(byte[]) && displayValue != config.Settings.RepresentationOfMissing && IsHostedByEnter)
                                    {
                                        this.Dispatcher.BeginInvoke(setImage, strataValue, (byte[])row[column.ColumnName], new TextBlockConfig(displayValue, new Thickness(8, 6, 8, 6), VerticalAlignment.Stretch, HorizontalAlignment.Stretch, TextAlignment.Left, rowCount, columnCount, Visibility.Visible), FontWeights.Normal);
                                    }
                                    else
                                    {
                                        this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(displayValue, new Thickness(8, 6, 8, 6), VerticalAlignment.Stretch, HorizontalAlignment.Stretch, TextAlignment.Left, rowCount, columnCount, Visibility.Visible), FontWeights.Normal);
                                    }
                                    columnCount++;
                                }

                                rowCount++;

                                if (rowCount > maxRows)
                                {
                                    break;
                                }
                            }

                            this.Dispatcher.BeginInvoke(drawBorders, strataValue);

                            if (rowCount > maxRows)
                            {
                                exceededMaxRows = true;
                            }
                        }

                        for (int i = 0; i < lineListTables.Count; i++)
                        {
                            lineListTables[i].Dispose();
                        }
                        lineListTables.Clear();
                    }

                    if (exceededMaxRows && exceededMaxColumns)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithWarningDelegate(RenderFinishWithWarning), "Warning: Some rows and columns were not displayed due to gadget settings. Showing top " + maxRows.ToString() + " rows and top " + maxColumns.ToString() + " columns only.");
                    }
                    else if (exceededMaxColumns)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithWarningDelegate(RenderFinishWithWarning), "Warning: Some columns were not displayed due to gadget settings. Showing top " + maxColumns.ToString() + " columns only.");
                    }
                    else if (exceededMaxRows)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithWarningDelegate(RenderFinishWithWarning), string.Format(SharedStrings.DASHBOARD_GADGET_STATUS_ROW_LIMIT, maxRows.ToString()));
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                    }
                    // Now called after drawing all the borders.
                    // this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState)); 
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                finally
                {
                    stopwatch.Stop();
                    Debug.Print("Line list gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + DashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }

            //FROM FREQUENCY CONTROL worker_DoWork//
            //{
            //    Dictionary<string, string> inputVariableList = ((GadgetParameters)e.Argument).InputVariableList;
            
            //    Stopwatch stopwatch = new Stopwatch();
            //    stopwatch.Start();

            //    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));                
            //    this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

            //    SetGridTextDelegate setText = new SetGridTextDelegate(SetGridText);
            //    AddGridRowDelegate addRow = new AddGridRowDelegate(AddGridRow);
            //    RenderFrequencyHeaderDelegate renderHeader = new RenderFrequencyHeaderDelegate(RenderFrequencyHeader);
            //    DrawFrequencyBordersDelegate drawBorders = new DrawFrequencyBordersDelegate(DrawOutputGridBorders);

            //    string freqVar = GadgetOptions.MainVariableName;
            //    string weightVar = GadgetOptions.WeightVariableName;
            //    string strataVar = string.Empty;
            //    bool includeMissing = GadgetOptions.ShouldIncludeMissing;
            //    int? rowsToDisplay = GadgetOptions.RowsToDisplay;
                               
            //    List<string> stratas = new List<string>();
            //    if (!string.IsNullOrEmpty(strataVar))
            //    {
            //        stratas.Add(strataVar);
            //    }

            //    string precisionFormat = "F2";
            //    string precisionPercentFormat = "P2";

            //    if (GadgetOptions.InputVariableList.ContainsKey("precision"))
            //    {
            //        precisionFormat = GadgetOptions.InputVariableList["precision"];
            //        precisionPercentFormat = "P" + precisionFormat;
            //        precisionFormat = "F" + precisionFormat;
            //    }

            //    try
            //    {
            //        Configuration config = DashboardHelper.Config;
            //        string yesValue = config.Settings.RepresentationOfYes;
            //        string noValue = config.Settings.RepresentationOfNo;

            //        RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
            //        CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

            //        GadgetOptions.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
            //        GadgetOptions.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

            //        if (this.DataFilters != null && this.DataFilters.Count > 0)
            //        {
            //            GadgetOptions.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
            //        }
            //        else
            //        {
            //            GadgetOptions.CustomFilter = string.Empty;
            //        }

            //        List<DataTable> lineListTables = DashboardHelper.GenerateDeduplicationList(GadgetOptions);

            //        if (lineListTables  == null || lineListTables.Count == 0)
            //        {
            //            this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), DashboardSharedStrings.GADGET_MSG_NO_DATA);
            //            //this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));                        
            //            return;
            //        }else if (worker.CancellationPending)
            //        {
            //            this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), DashboardSharedStrings.GADGET_MSG_OPERATION_CANCELLED);
            //            //this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
            //            return;                        
            //        }
            //        else
            //        {
            //            string formatString = string.Empty;

            //            foreach (DataTable tableDedup in lineListTables)
            //            {
            //                string strataValue = tableDedup.TableName;

            //                double count = 0;
            //                //foreach (something in tableDedup.Rows)
            //                //{
            //                //    count = count + ds.observations;
            //                //}

            //                if (count == 0 && lineListTables.Count == 1)
            //                {
            //                    //// this is the only table and there are no records, so let the user know
            //                    //this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), DashboardSharedStrings.GADGET_MSG_NO_DATA);
            //                    ////this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));                                
            //                    //return;
            //                }
            //                else if (count == 0)
            //                {
            //                    //continue;
            //                }
            //                //DataTable frequencies = tableDedup.Key;

            //                //if (frequencies.Rows.Count == 0)
            //                //{
            //                //    //continue;
            //                //}

            //                //this.Dispatcher.BeginInvoke(strataVar, frequencies.TableName);
            //            }

            //            foreach (DataTable tableDedup in lineListTables)
            //            {
            //                string strataValue = tableDedup.TableName;

            //                double count = 0;
            //                //foreach (DescriptiveStatistics ds in tableKvp.Value)
            //                //{
            //                //    //count = count + ds.observations;
            //                //}

            //                //if (count == 0)
            //                //{
            //                //    //continue;
            //                //}
            //                //DataTable frequencies = tableKvp.Key;

            //                //if (frequencies.Rows.Count == 0)
            //                //{
            //                //    //continue;
            //                //}

            //                //string tableHeading = tableKvp.Key.TableName;

            //                //if (stratifiedFrequencyTables.Count > 1)
            //                //{
            //                //    //tableHeading = freqVar;// +": " + strataVar + " = " + frequencies.TableName;
            //                //}

            //                Field field = null;
            //                string columnType = string.Empty;

            //                //foreach (DataRow fieldRow in DashboardHelper.FieldTable.Rows)
            //                //{
            //                //    //if (fieldRow["columnname"].Equals(freqVar))
            //                //    //{
            //                //    //    //columnType = fieldRow["datatype"].ToString();
            //                //    //    //if (fieldRow["epifieldtype"] is Field)
            //                //    //    //{
            //                //    //    //    //field = fieldRow["epifieldtype"] as Field;
            //                //    //    //}
            //                //    //    //break;
            //                //    //}
            //                //}

            //                //this.Dispatcher.BeginInvoke(renderHeader, strataValue, tableHeading);

            //                double AccumulatedTotal = 0;
            //                //                            List<ConfLimit> confLimits = new List<ConfLimit>();
            //                int rowCount = 1;

            //                this.Dispatcher.BeginInvoke(new AddGridFooterDelegate(RenderFrequencyFooter), strataValue, rowCount, (int)count);
            //                this.Dispatcher.BeginInvoke(drawBorders, strataValue);                            
            //            }

            //            lineListTables.Clear();
            //        }
            //        this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
            //        //this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
            //    }
            //    catch (Exception ex)
            //    {
            //        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
            //        //this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
            //    }
            //    finally
            //    {
            //        stopwatch.Stop();
            //        Debug.Print("Deduplication gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + DashboardHelper.RecordCount.ToString() + " records and the following filters:");
            //        Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
            //    }
            //}
        }
        #endregion // Event Handlers        
        
        //#region Private Properties
        ///// <summary>
        ///// Gets whether or not the main variable is a drop-down list
        ///// </summary>
        //private bool IsDropDownList
        //{
        //    get
        //    {
        //        return this.isDropDownList;
        //    }
        //}

        ///// <summary>
        ///// Gets whether the main variable is a comment legal field
        ///// </summary>
        //private bool IsCommentLegal
        //{
        //    get
        //    {
        //        return this.isCommentLegal;
        //    }
        //}

        ///// <summary>
        ///// Gets whether the main variable is an option field
        ///// </summary>
        //private bool IsOptionField
        //{
        //    get
        //    {
        //        return this.isOptionField;
        //    }
        //}

        /// <summary>
        /// Gets whether the main variable is a recoded variable
        /// </summary>
        //private bool IsRecoded
        //{
        //    get
        //    {
        //        return this.isRecoded;
        //    }
        //}
        //#endregion // Private Properties
    }
}
