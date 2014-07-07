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
    /// Interaction logic for DashboardMainTabControl.xaml
    /// </summary>
    public partial class DashboardMainTabControl : UserControl
    {
        public event MouseButtonEventHandler AnalysisButtonClicked;
        public event MouseButtonEventHandler DataButtonClicked;
        public event MouseButtonEventHandler VariablesButtonClicked;
        public event MouseButtonEventHandler ReportButtonClicked;

        public DashboardMainTabControl()
        {
            InitializeComponent();            
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dtiAnalysis.MouseLeftButtonUp += new MouseButtonEventHandler(dtiAnalysis_MouseLeftButtonUp);
            dtiData.MouseLeftButtonUp += new MouseButtonEventHandler(dtiData_MouseLeftButtonUp);
            dtiVariables.MouseLeftButtonUp += new MouseButtonEventHandler(dtiVariables_MouseLeftButtonUp);
            dtiReport.MouseLeftButtonUp += new MouseButtonEventHandler(dtiReport_MouseLeftButtonUp);

            dtiAnalysis.Select();
        }

        void dtiReport_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ReportButtonClicked != null)
            {
                ReportButtonClicked(this, e);
            }
            dtiAnalysis.Deselect();
            dtiData.Deselect();
            dtiReport.Select();
            dtiVariables.Deselect();
        }

        void dtiVariables_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (VariablesButtonClicked != null)
            {
                VariablesButtonClicked(this, e);
            }
            dtiAnalysis.Deselect();
            dtiData.Deselect();
            dtiReport.Deselect();
            dtiVariables.Select();
        }

        void dtiData_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataButtonClicked != null)
            {
                DataButtonClicked(this, e);
            }
            dtiAnalysis.Deselect();
            dtiData.Select();
            dtiReport.Deselect();
            dtiVariables.Deselect();
        }

        void dtiAnalysis_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (AnalysisButtonClicked != null)
            {
                AnalysisButtonClicked(this, e);
            }
            dtiAnalysis.Select();
            dtiData.Deselect();
            dtiReport.Deselect();
            dtiVariables.Deselect();
        }
    }
}
