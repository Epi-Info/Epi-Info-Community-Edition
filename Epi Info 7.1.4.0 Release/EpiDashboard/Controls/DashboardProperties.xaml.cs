﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Epi;

namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for DashboardProperties.xaml
    /// </summary>
    public partial class DashboardProperties : UserControl
    {
        public DashboardProperties(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;

            if (DashboardHelper.IsUsingEpiProject)
            {
                txtProjectPath.Text = dashboardHelper.View.Project.FilePath;

                if (System.IO.File.Exists(txtProjectPath.Text))
                {
                    cmbFormName.Items.Clear();
                    Project project = new Project(txtProjectPath.Text);
                    foreach (View view in project.Views)
                    {
                        cmbFormName.Items.Add(view.Name);
                    }
                }

                cmbFormName.Text = dashboardHelper.View.Name;
            }
            else
            {
                if (!string.IsNullOrEmpty(dashboardHelper.CustomQuery))
                {
                    SqlQuery = DashboardHelper.CustomQuery;
                }
            }

            tblockRows.Text = dashboardHelper.DataSet.Tables[0].Rows.Count.ToString() + " unfiltered rows";
            tblockColumns.Text = dashboardHelper.DataSet.Tables[0].Columns.Count.ToString() + " columns";
            tblockCacheDateTime.Text = "Data last cached at " + dashboardHelper.LastCacheTime.ToShortDateString() + " " + dashboardHelper.LastCacheTime.ToShortTimeString();
            tblockCacheTimeElapsed.Text = "Took " + dashboardHelper.TimeToCache + " to locally cache data";

            Epi.ApplicationIdentity appId = new Epi.ApplicationIdentity(typeof(Configuration).Assembly);
            tblockCurrentEpiVersion.Text = "Epi Info " + appId.Version;

            lbxRelatedDataSources.Items.Clear();
            if (dashboardHelper.ConnectionsForRelate.Count > 0)
            {
                // Related Data
                foreach (RelatedConnection rConn in dashboardHelper.ConnectionsForRelate)
                {
                    lbxRelatedDataSources.Items.Add(rConn.db.ConnectionString);
                }
            }
        }

        public string Conclusion
        {
            get
            {
                return txtConclusion.Text;
            }
            set
            {
                txtConclusion.Text = value;
            }
        }

        public string Title
        {
            get
            {
                return txtTitle.Text;
            }
            set
            {
                txtTitle.Text = value;
            }
        }

        public string Summary
        {
            get
            {
                return txtSummary.Text;
            }
            set
            {
                txtSummary.Text = value;
            }
        }

        public bool ShowGadgetHeadings
        {
            get
            {
                return (bool)checkboxGadgetHeadings.IsChecked;
            }
            set
            {
                checkboxGadgetHeadings.IsChecked = value;
            }
        }

        public bool ShowGadgetSettings
        {
            get
            {
                return (bool)checkboxGadgetSettings.IsChecked;
            }
            set
            {
                checkboxGadgetSettings.IsChecked = value;
            }
        }

        public bool ShowCanvasSummary
        {
            get
            {
                return (bool)checkboxCanvasSummary.IsChecked;
            }
            set
            {
                checkboxCanvasSummary.IsChecked = value;
            }
        }

        public bool UseAlternatingColors
        {
            get
            {
                return (bool)checkboxAlternateColors.IsChecked;
            }
            set
            {
                checkboxAlternateColors.IsChecked = value;
            }
        }

        public bool UseTopToBottomGadgetOrdering
        {
            get
            {
                return (bool)checkboxTopToBottom.IsChecked;
            }
            set
            {
                checkboxTopToBottom.IsChecked = value;
            }
        }

        public event EventHandler Cancelled;
        public event EventHandler ChangesAccepted;

        public DashboardHelper DashboardHelper { get; private set; }

        public FileInfo ProjectFileInfo
        {
            get 
            {
                FileInfo fi = new FileInfo(txtProjectPath.Text);
                return fi;
            }
            set
            {
                txtProjectPath.Text = value.FullName;
                panelDataSourceProject.Visibility = Visibility.Visible;
                panelDataSourceOther.Visibility = Visibility.Collapsed;
                panelDataSourceAdvanced.Visibility = Visibility.Collapsed;
            }
        }

        public double? DefaultChartHeight
        {
            get
            {
                double value = -1;
                bool success = Double.TryParse(txtDefaultChartHeight.Text, out value);
                if (success)
                {
                    return value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    txtDefaultChartHeight.Text = value.ToString();
                }
                else
                {
                    txtDefaultChartHeight.Text = String.Empty;
                }
            }
        }

        public double? DefaultChartWidth
        {
            get
            {
                double value = -1;
                bool success = Double.TryParse(txtDefaultChartWidth.Text, out value);
                if (success)
                {
                    return value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    txtDefaultChartWidth.Text = value.ToString();
                }
                else
                {
                    txtDefaultChartWidth.Text = String.Empty;
                }
            }
        }

        public string FormName
        {
            get
            {
                return cmbFormName.SelectedItem.ToString();
            }
            set
            {
                cmbFormName.SelectedItem = value;
            }
        }

        public string ConnectionString
        {
            get
            {
                return txtDataSource.Text;
            }
            set
            {
                txtDataSource.Text = value;
                panelDataSourceProject.Visibility = Visibility.Collapsed;
                panelDataSourceOther.Visibility = Visibility.Visible;
                panelDataSourceAdvanced.Visibility = Visibility.Collapsed;
            }
        }

        public string SqlQuery
        {
            get
            {
                return txtSQLQuery.Text;
            }
            set
            {
                txtSQLQuery.Text = value;
                panelDataSourceProject.Visibility = Visibility.Collapsed;
                panelDataSourceOther.Visibility = Visibility.Collapsed;
                panelDataSourceAdvanced.Visibility = Visibility.Visible;
            }
        }

        private void tbtnInfo_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Visible;
        }

        private void tbtnCharts_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Visible;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnHTML_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Visible;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDataSource_Checked(object sender, RoutedEventArgs e)
        {
            if (panelDataSource == null) return;
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Visible;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void CheckButtonStates(ToggleButton sender)
        {
            foreach (UIElement element in panelSidebar.Children)
            {
                if (element is ToggleButton)
                {
                    ToggleButton tbtn = element as ToggleButton;
                    if (tbtn != sender)
                    {
                        tbtn.IsChecked = false;
                    }
                }
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (ChangesAccepted != null)
            {
                ChangesAccepted(this, new EventArgs());
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".prj";
            dlg.Filter = "Epi Info 7 Project|*.prj";
            if (!string.IsNullOrEmpty(txtProjectPath.Text))
            {
                dlg.FileName = txtProjectPath.Text;
            }
            else
            {
                dlg.InitialDirectory = this.DashboardHelper.Config.Directories.Project;
            }

            if (dlg.ShowDialog().Value)
            {
                txtProjectPath.Text = dlg.FileName;
            }
        }

        private void cmbFormName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFormName.SelectedIndex >= 0)
            {
                cmbFormName.Text = cmbFormName.SelectedItem.ToString();
            }
        }

        private void txtProjectPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            cmbFormName.Items.Clear();
            if (System.IO.File.Exists(txtProjectPath.Text))
            {
                Project project = new Project(txtProjectPath.Text);
                foreach (View view in project.Views)
                {
                    cmbFormName.Items.Add(view.Name);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }
    }
}