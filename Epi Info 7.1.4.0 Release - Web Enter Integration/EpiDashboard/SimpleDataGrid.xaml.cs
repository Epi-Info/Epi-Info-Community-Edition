using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using Microsoft.Windows.Controls;
using Epi;
using Epi.Data;

namespace EpiDashboard
{
    /// <summary>
    /// Interaction logic for SimpleDataGrid.xaml
    /// </summary>
    public partial class SimpleDataGrid : UserControl
    {

        public event Mapping.RecordSelectedHandler RecordSelected;
        private DashboardHelper dashboardHelper;
        private DataFilteringControl dataFilteringControl;
        private DataRecodingControl dataRecodingControl;
        private View view;
        private IDbDriver db;
        private LineListControl lineList;

        public SimpleDataGrid()
        {
            InitializeComponent();
        }

        public SimpleDataGrid(View view, IDbDriver db)
        {
            InitializeComponent();

            this.view = view;
            this.db = db;
            this.dashboardHelper = new DashboardHelper(view, db);
            this.dashboardHelper.PopulateDataSet();

            //dgResults.MouseDoubleClick += new MouseButtonEventHandler(dgResults_MouseDoubleClick);
            //dgResults.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(dgResults_AutoGeneratingColumn);
            this.Loaded += new RoutedEventHandler(SimpleDataGrid_Loaded);
            this.SizeChanged += new SizeChangedEventHandler(SimpleDataGrid_SizeChanged);
        }

        void SimpleDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas.SetLeft(scrollViewer, 50);
            Canvas.SetTop(scrollViewer, 0);
            if (dataRecodingControl != null)
                Canvas.SetTop(dataRecodingControl, (grdMain.ActualHeight / 2.0) - (dataRecodingControl.ActualHeight / 2.0));
            if (dataFilteringControl != null)
                Canvas.SetTop(dataFilteringControl, (grdMain.ActualHeight / 2.0) - (dataFilteringControl.ActualHeight / 2.0));
            if (grdMain.ActualWidth - 100 <= 0)
            {
                scrollViewer.Width = 0;
            }
            else
            {
                scrollViewer.Width = grdMain.ActualWidth - 100;
            }
            
            scrollViewer.Height = grdMain.ActualHeight;
        }

        void AddSelectionCriteria()
        {
            dataFilteringControl = new DataFilteringControl(dashboardHelper);
            dataFilteringControl.MouseEnter += new MouseEventHandler(selectionCriteriaControl_MouseEnter);
            dataFilteringControl.MouseLeave += new MouseEventHandler(selectionCriteriaControl_MouseLeave);
            dataFilteringControl.SelectionCriteriaChanged += new EventHandler(selectionCriteriaControl_SelectionCriteriaChanged);
            dataFilteringControl.Loaded += new RoutedEventHandler(dataFilteringControl_Loaded);
            grdMain.Children.Add(dataFilteringControl);
            
            DragCanvas.SetCanBeDragged(dataFilteringControl, false);
        }

        void dataFilteringControl_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetRight(dataFilteringControl, -540);
            Canvas.SetTop(dataFilteringControl, (grdMain.ActualHeight / 2.0) - (dataFilteringControl.ActualHeight / 2.0));
        }

        void selectionCriteriaControl_SelectionCriteriaChanged(object sender, EventArgs e)
        {
            lineList.RefreshResults();
        }

        void dataRecodingControl_RecodingRulesChanged(object sender, EventArgs e)
        {
            lineList.RefreshResults();
        }

        void selectionCriteriaControl_MouseLeave(object sender, MouseEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = Canvas.GetRight(dataFilteringControl);
            anim.To = -540;
            anim.DecelerationRatio = 0.8;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            dataFilteringControl.BeginAnimation(Canvas.RightProperty, anim);
        }

        void selectionCriteriaControl_MouseEnter(object sender, MouseEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = Canvas.GetRight(dataFilteringControl);
            anim.To = -20;
            anim.AccelerationRatio = 0.8;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            dataFilteringControl.BeginAnimation(Canvas.RightProperty, anim);
        }

        void AddFormatOptions()
        {
            dataRecodingControl = new DataRecodingControl(dashboardHelper);
            dataRecodingControl.MouseEnter += new MouseEventHandler(dataRecodingControl_MouseEnter);
            dataRecodingControl.MouseLeave += new MouseEventHandler(dataRecodingControl_MouseLeave);
            dataRecodingControl.UserVariableChanged += new EventHandler(dataRecodingControl_RecodingRulesChanged);
            dataRecodingControl.Loaded += new RoutedEventHandler(dataRecodingControl_Loaded);
            grdMain.Children.Add(dataRecodingControl);
            
            DragCanvas.SetCanBeDragged(dataRecodingControl, false);
        }

        void dataRecodingControl_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetLeft(dataRecodingControl, -385);
            Canvas.SetTop(dataRecodingControl, (grdMain.ActualHeight / 2.0) - (dataRecodingControl.ActualHeight / 2.0));
        }

        void dataRecodingControl_MouseLeave(object sender, MouseEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = Canvas.GetLeft(dataRecodingControl);
            anim.To = -385;
            anim.DecelerationRatio = 0.8;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            dataRecodingControl.BeginAnimation(Canvas.LeftProperty, anim);
        }

        void dataRecodingControl_MouseEnter(object sender, MouseEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = Canvas.GetLeft(dataRecodingControl);
            anim.To = -20;
            anim.AccelerationRatio = 0.8;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            dataRecodingControl.BeginAnimation(Canvas.LeftProperty, anim);
        }

        void SimpleDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetLeft(scrollViewer, 50);
            Canvas.SetTop(scrollViewer, 0);

            scrollViewer.Width = grdMain.ActualWidth - 100;
            scrollViewer.Height = grdMain.ActualHeight;
            lineList = new LineListControl(dashboardHelper, true);
            lineList.IsHostedByEnter = true;
            lineList.RecordSelected += new Mapping.RecordSelectedHandler(lineList_RecordSelected);
            dgResults.Children.Add(lineList);

            try
            {
                lineList.SelectPageNumber(1);
            }
            catch (ApplicationException ex)
            {
                Epi.Windows.MsgBox.ShowError("A problem was detected with the metadata for this form; the page at position 0 could not be located.", ex);
            }

            lineList.RefreshResults();

            AddSelectionCriteria();
            AddFormatOptions();
        }

        void lineList_RecordSelected(int id)
        {
            if (RecordSelected != null)
            {
                RecordSelected(id);
            }
        }
    }
}
