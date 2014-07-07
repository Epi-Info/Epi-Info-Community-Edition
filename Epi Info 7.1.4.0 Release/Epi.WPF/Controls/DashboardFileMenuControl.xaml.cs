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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Epi.WPF.Controls
{
    /// <summary>
    /// Interaction logic for DashboardFileMenuControl.xaml
    /// </summary>
    public partial class DashboardFileMenuControl : UserControl
    {
        public event MouseButtonEventHandler FileButtonClicked;
        public event MouseButtonEventHandler AnalysisButtonClicked;
        public event MouseButtonEventHandler DataButtonClicked;
        public event MouseButtonEventHandler VariablesButtonClicked;
        public event MouseButtonEventHandler ReportButtonClicked;
        public event MouseButtonEventHandler ManagerButtonClicked;

        public DashboardFileMenuControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {            
            menuItemAnalysis.MouseLeftButtonUp += new MouseButtonEventHandler(menuItemAnalysis_MouseLeftButtonUp);
            menuItemData.MouseLeftButtonUp += new MouseButtonEventHandler(menuItemData_MouseLeftButtonUp);
            menuItemVariables.MouseLeftButtonUp += new MouseButtonEventHandler(menuItemVariables_MouseLeftButtonUp);
            menuItemReport.MouseLeftButtonUp += new MouseButtonEventHandler(menuItemReport_MouseLeftButtonUp);
            menuItemManager.MouseLeftButtonUp += new MouseButtonEventHandler(menuItemManager_MouseLeftButtonUp);

            menuItemAnalysis.Select();
        }

        void menuItemManager_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ManagerButtonClicked != null)
            {
                ManagerButtonClicked(this, e);
            }
            menuItemAnalysis.Deselect();
            menuItemData.Deselect();
            menuItemVariables.Deselect();
            menuItemReport.Deselect();
            menuItemManager.Select();
        }

        void menuItemReport_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ReportButtonClicked != null)
            {
                ReportButtonClicked(this, e);
            }
            menuItemAnalysis.Deselect();
            menuItemData.Deselect();
            menuItemVariables.Deselect();
            menuItemReport.Select();
            menuItemManager.Deselect();
        }

        void menuItemVariables_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (VariablesButtonClicked != null)
            {
                VariablesButtonClicked(this, e);
            }
            menuItemAnalysis.Deselect();
            menuItemData.Deselect();
            menuItemVariables.Select();
            menuItemReport.Deselect();
            menuItemManager.Deselect();
        }

        void menuItemData_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataButtonClicked != null)
            {
                DataButtonClicked(this, e);
            }
            menuItemAnalysis.Deselect();
            menuItemData.Select();
            menuItemVariables.Deselect();
            menuItemReport.Deselect();
            menuItemManager.Deselect();
        }

        void menuItemAnalysis_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (AnalysisButtonClicked != null)
            {
                AnalysisButtonClicked(this, e);
            }
            menuItemAnalysis.Select();
            menuItemData.Deselect();
            menuItemVariables.Deselect();
            menuItemReport.Deselect();
            menuItemManager.Deselect();
        }
    }
}
