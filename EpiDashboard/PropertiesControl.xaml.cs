using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
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
using Microsoft.Windows.Controls;
using Epi;
using Epi.Core;
using Epi.Data;
using Epi.Fields;
using EpiDashboard;
using EpiDashboard.Rules;

namespace EpiDashboard
{    
    /// <summary>
    /// Interaction logic for PropertiesControl.xaml
    /// </summary>
    public partial class PropertiesControl : UserControl
    {
        #region Private Members
        private DashboardHelper dashboardHelper;
        private DashboardControl dashboardControl;
        private Project currentProject;
        #endregion // Private Members

        #region Public Events
        public event GadgetClosingHandler GadgetClosing;        
        #endregion // Public Events

        #region Delegates
        #endregion // Delegates

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>        
        /// <param name="dashboardHelper">The dashboard helper to attach</param>
        public PropertiesControl(DashboardHelper dashboardHelper, DashboardControl dashboardControl)
        {
            InitializeComponent();

            this.dashboardHelper = dashboardHelper;
            this.dashboardControl = dashboardControl;

            imgClose.MouseEnter += new MouseEventHandler(imgClose_MouseEnter);
            imgClose.MouseLeave += new MouseEventHandler(imgClose_MouseLeave);
            imgClose.MouseDown += new MouseButtonEventHandler(imgClose_MouseDown);

            ConfigGrid.MaxWidth = 500;

            pnlError.Visibility = System.Windows.Visibility.Collapsed;
            pnlWarning.Visibility = System.Windows.Visibility.Collapsed;
            pnlSave.Visibility = System.Windows.Visibility.Collapsed;

            cmbTableName.Items.Clear();            
            // Data Source Properties
            if (dashboardHelper.IsUsingEpiProject)
            {
                txtProjectPath.Visibility = System.Windows.Visibility.Visible;
                tblockProjectPath.Visibility = System.Windows.Visibility.Visible;
                btnProjectPathBrowse.Visibility = System.Windows.Visibility.Visible;

                tblockTableName.Visibility = System.Windows.Visibility.Visible;
                cmbTableName.Visibility = System.Windows.Visibility.Visible;

                txtDataSource.Visibility = System.Windows.Visibility.Collapsed;
                tblockConnectionString.Visibility = System.Windows.Visibility.Collapsed;

                txtSQLQuery.Visibility = System.Windows.Visibility.Collapsed;
                tblockSQLQuery.Visibility = System.Windows.Visibility.Collapsed;

                txtProjectPath.Text = dashboardHelper.View.Project.FilePath;

                if (System.IO.File.Exists(txtProjectPath.Text))
                {
                    Project project = new Project(txtProjectPath.Text);
                    foreach (View view in project.Views)
                    {
                        cmbTableName.Items.Add(view.Name);
                    }
                }

                cmbTableName.Text = dashboardHelper.View.Name;
            }
            else
            {
                grdDataSource.Visibility = System.Windows.Visibility.Collapsed;

                txtProjectPath.Visibility = System.Windows.Visibility.Collapsed;
                tblockProjectPath.Visibility = System.Windows.Visibility.Collapsed;
                btnProjectPathBrowse.Visibility = System.Windows.Visibility.Collapsed;

                tblockTableName.Visibility = System.Windows.Visibility.Collapsed;
                cmbTableName.Visibility = System.Windows.Visibility.Collapsed;

                txtDataSource.Visibility = System.Windows.Visibility.Visible;
                tblockConnectionString.Visibility = System.Windows.Visibility.Visible;

                if (!string.IsNullOrEmpty(dashboardHelper.CustomQuery))
                {
                    grdDataSource.Visibility = System.Windows.Visibility.Visible;

                    txtDataSource.Visibility = System.Windows.Visibility.Collapsed;
                    tblockConnectionString.Visibility = System.Windows.Visibility.Collapsed;

                    txtSQLQuery.Visibility = System.Windows.Visibility.Visible;
                    tblockSQLQuery.Visibility = System.Windows.Visibility.Visible;
                    txtSQLQuery.Text = dashboardHelper.CustomQuery;
                }
                else
                { 
                    txtSQLQuery.Visibility = System.Windows.Visibility.Collapsed;
                    tblockSQLQuery.Visibility = System.Windows.Visibility.Collapsed;
                    txtSQLQuery.Text = string.Empty;
                }
                //txtDataSource.Text = dashboardHelper.Database.ConnectionString;
            }

            if (dashboardHelper.ConnectionsForRelate.Count > 0)
            {
                grdRelatedDataSources.Visibility = System.Windows.Visibility.Visible;
                // Related Data
                foreach (RelatedConnection rConn in dashboardHelper.ConnectionsForRelate)
                {
                    RowDefinition rowDefHeader = new RowDefinition();
                    grdRelatedDataSourceList.RowDefinitions.Add(rowDefHeader);

                    //Image imgRemove = new Image();
                    //imgRemove.Source = imgClose.Source;
                    //imgRemove.Width = 16;
                    //imgRemove.Height = 16;
                    //Grid.SetRow(imgRemove, grdRelatedDataSourceList.RowDefinitions.Count - 1);
                    //Grid.SetColumn(imgRemove, 0);
                    //grdRelatedDataSourceList.Children.Add(imgRemove);

                    TextBlock tblockCount = new TextBlock();
                    tblockCount.Text = grdRelatedDataSourceList.RowDefinitions.Count.ToString();
                    tblockCount.MaxWidth = 300;
                    Grid.SetRow(tblockCount, grdRelatedDataSourceList.RowDefinitions.Count - 1);
                    Grid.SetColumn(tblockCount, 0);
                    grdRelatedDataSourceList.Children.Add(tblockCount);

                    TextBlock tblockRelatedData = new TextBlock();
                    tblockRelatedData.Text = rConn.db.ConnectionString;
                    tblockRelatedData.MaxWidth = 300;
                    Grid.SetRow(tblockRelatedData, grdRelatedDataSourceList.RowDefinitions.Count - 1);
                    Grid.SetColumn(tblockRelatedData, 2);
                    grdRelatedDataSourceList.Children.Add(tblockRelatedData);
                }
            }
            else
            {
                grdRelatedDataSources.Visibility = System.Windows.Visibility.Collapsed;
            }

            // HTML Output Properties
            txtTitle.Text = dashboardControl.CustomOutputHeading;
            txtSummary.Text = dashboardControl.CustomOutputSummaryText;
            txtConclusion.Text = dashboardControl.CustomOutputConclusionText;
            checkboxGadgetHeadings.IsChecked = dashboardControl.ShowGadgetHeadingsInOutput;
            checkboxGadgetSettings.IsChecked = dashboardControl.ShowGadgetSettingsInOutput;
            checkboxAlternateColors.IsChecked = dashboardControl.UseAlternatingColorsInOutput;
            checkboxCanvasSummary.IsChecked = dashboardControl.ShowCanvasSummaryInfoInOutput;

            // Misc info
            tblockRows.Text = dashboardHelper.DataSet.Tables[0].Rows.Count.ToString() + " unfiltered rows";
            tblockColumns.Text = dashboardHelper.DataSet.Tables[0].Columns.Count.ToString() + " columns";
            tblockCacheDateTime.Text = "Data last cached at " + dashboardHelper.LastCacheTime.ToShortDateString() + " " + dashboardHelper.LastCacheTime.ToShortTimeString();
            tblockCacheTimeElapsed.Text = "Took " + dashboardHelper.TimeToCache + " to locally cache data";

            Epi.ApplicationIdentity appId = new Epi.ApplicationIdentity(typeof(Configuration).Assembly);
            tblockCurrentEpiVersion.Text = "Epi Info " + appId.Version;

            cmbTableName.SelectionChanged += new SelectionChangedEventHandler(cmbTableName_SelectionChanged);
            txtProjectPath.TextChanged += new TextChangedEventHandler(txtProjectPath_TextChanged);
        }
        #endregion // Constructors

        #region Public Properties
        public Project CurrentProject
        {
            get
            {
                return this.currentProject;
            }
            set
            {
                this.currentProject = value;
            }
        }

        public string ConnectionString
        {
            get
            {
                return this.txtDataSource.Text;
            }
            set
            {
                this.txtDataSource.Text = value;
            }
        }

        public string TableName
        {
            get
            {
                return this.cmbTableName.Text;
            }
            set
            {
                this.cmbTableName.Text = value;
            }
        }

        public bool EncryptConnectionString
        {
            get
            {
                return (bool)this.checkboxEncryptConnectionString.IsChecked;
            }
            set
            {
                this.checkboxEncryptConnectionString.IsChecked = value;
            }
        }

        public bool ForceReCaching
        {
            get
            {
                return (bool)this.checkboxRecache.IsChecked;
            }
            set
            {
                this.checkboxRecache.IsChecked = value;
            }
        }

        #endregion // Public Properties

        public void MinimizeGadget()
        {
            ConfigGrid.Height = 50;
            //triangleCollapsed = true;
        }

        #region Event Handlers
        /// <summary>
        /// Fires the 'Close' button event that will remove the gadget from the dashboard
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void imgClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.IsEnabled = false;
            int speed = 400;
            Storyboard storyboard = new Storyboard();
            storyboard.Completed += new EventHandler(storyboard_Completed);
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, (int)speed); //            

            DoubleAnimation animation = new DoubleAnimation { From = 1.0, To = 0.0, Duration = new Duration(duration) };
            if (this.Opacity == 0.0)
            {
                animation = new DoubleAnimation { From = 0.0, To = 1.0, Duration = new Duration(duration) };
            }

            Storyboard.SetTargetName(animation, this.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity", 0));
            storyboard.Children.Add(animation);

            storyboard.Begin(this); 
        }

        /// <summary>
        /// Fired when the user removes the mouse cursor from inside of the close button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void imgClose_MouseLeave(object sender, MouseEventArgs e)
        {
            Uri uriSource = new Uri("Images/x.png", UriKind.Relative);
            imgClose.Source = new BitmapImage(uriSource);
        }

        /// <summary>
        /// Fired when the user places the mouse cursor inside of the close button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void imgClose_MouseEnter(object sender, MouseEventArgs e)
        {
            Uri uriSource = new Uri("Images/x_over.png", UriKind.Relative);
            imgClose.Source = new BitmapImage(uriSource);
        }
        #endregion // Event Handlers

        #region Private Methods
     
        /// <summary>
        /// Closes the gadget
        /// </summary>
        private void CloseGadget()
        {
            if (GadgetClosing != null)
                GadgetClosing(this);

            GadgetClosing = null;
        }

        /// <summary>
        /// Checks for problems and displays messages
        /// </summary>
        private void CheckForProblems()
        {
            if (dashboardHelper.IsUsingEpiProject)
            {                
                if (cmbTableName.Text.ToLowerInvariant() != dashboardHelper.View.Name.ToLowerInvariant() || dashboardHelper.View.Project.FilePath != txtProjectPath.Text)
                {
                    pnlWarning.Visibility = System.Windows.Visibility.Visible;
                    txtWarning.Text = "Warning: Changing projects or forms within a project may cause some or all of the dashboard's gadgets and filters to fail.";
                }
            }
        }

        #endregion // Private Methods

        #region Private Properties
        private View View
        {
            get
            {
                return this.dashboardHelper.View;
            }
        }

        private IDbDriver Database
        {
            get
            {
                return this.dashboardHelper.Database;
            }
        }
        #endregion // Private Properties   

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            int speed = 400;
            Storyboard storyboard = new Storyboard();
            storyboard.Completed += new EventHandler(storyboard_Completed);
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, (int)speed); //            

            DoubleAnimation animation = new DoubleAnimation { From = 1.0, To = 0.0, Duration = new Duration(duration) };
            if (this.Opacity == 0.0)
            {
                animation = new DoubleAnimation { From = 0.0, To = 1.0, Duration = new Duration(duration) };
            }

            Storyboard.SetTargetName(animation, this.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity", 0));
            storyboard.Children.Add(animation);

            storyboard.Begin(this);            
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dashboardHelper.IsUsingEpiProject)
            {
                bool projectPathIsValid = true;
                Project tempProject = new Project();
                try
                {
                    tempProject = new Project(txtProjectPath.Text);
                }
                catch
                {
                    projectPathIsValid = false;
                }

                if (!projectPathIsValid)
                {
                    pnlError.Visibility = System.Windows.Visibility.Visible;
                    txtError.Text = "Error: The specified project path is not valid. Settings were not saved.";
                    return;
                }

                if (!tempProject.Views.Contains(cmbTableName.Text))
                {
                    pnlError.Visibility = System.Windows.Visibility.Visible;
                    txtError.Text = "Error: Form not found in project. Settings were not saved.";
                    return;
                }

                pnlError.Visibility = System.Windows.Visibility.Collapsed;
                txtError.Text = string.Empty;

                if (dashboardHelper.IsUsingEpiProject)
                {
                    if (cmbTableName.Text.ToLowerInvariant() == dashboardHelper.View.Name.ToLowerInvariant() && dashboardHelper.View.Project.FilePath == txtProjectPath.Text)
                    {
                        pnlWarning.Visibility = System.Windows.Visibility.Collapsed;
                        txtWarning.Text = string.Empty;
                    }
                }

                dashboardHelper.ResetView(tempProject.Views[cmbTableName.Text]);
            }
            else if (!string.IsNullOrEmpty(txtSQLQuery.Text))
            {
                dashboardHelper.SetCustomQuery(txtSQLQuery.Text);
            }

            this.IsEnabled = false;

            dashboardControl.CustomOutputConclusionText = txtConclusion.Text;
            dashboardControl.CustomOutputHeading = txtTitle.Text;
            dashboardControl.CustomOutputSummaryText = txtSummary.Text;
            dashboardControl.UseAlternatingColorsInOutput = (bool)checkboxAlternateColors.IsChecked;
            dashboardControl.ShowCanvasSummaryInfoInOutput = (bool)checkboxCanvasSummary.IsChecked;
            dashboardControl.ShowGadgetHeadingsInOutput = (bool)checkboxGadgetHeadings.IsChecked;
            dashboardControl.ShowGadgetSettingsInOutput = (bool)checkboxGadgetSettings.IsChecked;
            dashboardControl.SortGadgetsTopToBottom = (bool)checkboxTopToBottom.IsChecked;

            pnlSave.Visibility = System.Windows.Visibility.Visible;
            txtSave.Text = "Settings were saved at " + DateTime.Now.ToShortTimeString();

            //TransitionSpeed speed = TransitionSpeed.Normal;
            int speed = 900;
            Storyboard storyboard = new Storyboard();
            storyboard.Completed += new EventHandler(storyboard_Completed);
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, (int)speed); //            

            DoubleAnimation animation = new DoubleAnimation { From = 1.0, To = 0.0, Duration = new Duration(duration) };
            if (this.Opacity == 0.0)
            {
                animation = new DoubleAnimation { From = 0.0, To = 1.0, Duration = new Duration(duration) };
            }
            animation.BeginTime = new TimeSpan(0, 0, 0, 0, (int)speed + 300);

            Storyboard.SetTargetName(animation, this.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity", 0));
            storyboard.Children.Add(animation);

            storyboard.Begin(this);   
        }

        private void storyboard_Completed(object sender, EventArgs e)
        {
            this.CloseGadget();
        }

        private void txtProjectPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckForProblems();

            if (System.IO.File.Exists(txtProjectPath.Text))
            {
                Project project = new Project(txtProjectPath.Text);
                foreach (View view in project.Views)
                {
                    cmbTableName.Items.Add(view.Name);
                }
            }
        }

        private void cmbTableName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTableName.SelectedIndex >= 0)
            {
                cmbTableName.Text = cmbTableName.SelectedItem.ToString();
            }
            CheckForProblems();
        }

        private void btnProjectPathBrowse_Click(object sender, RoutedEventArgs e)
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
                dlg.InitialDirectory = this.dashboardHelper.Config.Directories.Project;
            }

            if (dlg.ShowDialog().Value)
            {
                txtProjectPath.Text = dlg.FileName;
            }
        }
    }
}
