using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.IO;
using Epi;
using Epi.Data;

namespace Epi.Windows.AnalysisDashboard
{
    public partial class DashboardMainForm : Form
    {
        private ElementHost host;
        private EpiDashboard.DashboardControl dashboard;
        private EpiDashboard.DashboardHelper dashboardHelper;
        private string htmlOutputPath;        

        public DashboardMainForm()
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Configuration.GetNewInstance().Settings.Language);
            InitializeComponent();

            if (System.Windows.Application.Current == null) new System.Windows.Application();

            host = new ElementHost();
            host.Dock = DockStyle.Fill;            

            this.toolStripContainer1.ContentPanel.Controls.Add(host);
            htmlOutputPath = string.Empty;
            dashboard = new EpiDashboard.DashboardControl();
            dashboard.DashboardHelperRequested += new EpiDashboard.DashboardHelperRequestedHandler(dashboard_DashboardHelperRequested);
            dashboard.RelatedDataRequested += new EpiDashboard.RelatedDataRequestedHandler(dashboard_RelatedDataRequested);
            dashboard.HTMLGenerated += new EpiDashboard.HTMLGeneratedHandler(dashboard_HTMLGenerated);
            dashboard.RecordCountChanged += new EpiDashboard.RecordCountChangedHandler(dashboard_RecordCountChanged);
            dashboard.CanvasChanged += new EpiDashboard.CanvasChangedHandler(dashboard_CanvasChanged);
            this.FormClosed += new FormClosedEventHandler(DashboardMainForm_FormClosed);
            host.Child = dashboard;
            this.Width++;
            this.Width--;

            this.toolStripContainer1.TopToolStripPanel.Visible = false;

            this.Text = SharedStrings.DASHBOARD_TITLE;
        }

        void DashboardMainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        void dashboard_CanvasChanged(string canvasFilePath)
        {
            if (dashboard.DashboardHelper == null)
            {
                this.Text = SharedStrings.DASHBOARD_TITLE;
                return;
            }

            if (!string.IsNullOrEmpty(canvasFilePath))
            {
                this.Text = SharedStrings.DASHBOARD_TITLE + StringLiterals.SPACE + StringLiterals.COLON + StringLiterals.SPACE + canvasFilePath;
            }
            else
            {
                this.Text = SharedStrings.DASHBOARD_TITLE + StringLiterals.SPACE + StringLiterals.COLON + StringLiterals.SPACE + "Unsaved canvas";
            }

            if (dashboard.IsCanvasDirty)
            {
                this.Text = this.Text + StringLiterals.SPACE + StringLiterals.STAR;
            }
        }

        void dashboard_RecordCountChanged(int recordCount, string dataSourceName)
        {
            lblRecordCount.Visible = true;
            lblRecordsPrefix.Visible = true;
            lblDataSource.Visible = true;
            txtRecordCount.Visible = true;
            txtDataSource.Visible = true;
            btnSave.Enabled = true;
            btnSaveHtml.Enabled = true;
            toolStripBtnAddRelate.Enabled = true;
            if (recordCount > int.MinValue)
            {
                txtRecordCount.Text = recordCount.ToString("N0");
                txtDataSource.Text = dataSourceName;
            }
            else
            {
                txtRecordCount.Text = "...";
            }
        }

        public bool IsGeneratingHTMLFromCommandLine
        {
            get
            {
                if (string.IsNullOrEmpty(htmlOutputPath))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        void dashboard_HTMLGenerated()
        {
            if (IsGeneratingHTMLFromCommandLine)
            {
                System.Threading.Thread.Sleep(1000);
                this.Close();
            }
        }

        EpiDashboard.RelatedConnection dashboard_RelatedDataRequested()
        {
            if (this.dashboard.DashboardHelper == null)
            {
                Epi.Windows.MsgBox.ShowInformation(SharedStrings.DASHBOARD_CANNOT_RELATE_NO_DATA_SOURCE);
                return null;
            }

            // May arrive here by opening a canvas file without going through normal request routine.
            this.dashboardHelper = this.dashboard.DashboardHelper;

            Epi.Windows.Dialogs.ReadRelatedDialog dlg = new Dialogs.ReadRelatedDialog(this.dashboard.DashboardHelper.TableColumnNames);

            if (dashboard.DashboardHelper.IsUsingEpiProject)
            {
                Dictionary<string, string> tableColumnNamesFiltered = new Dictionary<string, string>();

                foreach (KeyValuePair<string, string> kvp in this.dashboardHelper.TableColumnNames)
                {
                    if (kvp.Value.ToLower() != "epi.fields.groupfield")
                    {
                        tableColumnNamesFiltered.Add(kvp.Key, kvp.Value);
                    }
                }

                dlg = new Dialogs.ReadRelatedDialog(this.dashboard.DashboardHelper.View.Project, tableColumnNamesFiltered);
            }
            EpiDashboard.RelatedConnection relatedConnection = null;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (dlg.SelectedDataSource is Project && dlg.SelectedItemType == Dialogs.SelectedItemType.Form)
                {
                    Project project = (Project)dlg.SelectedDataSource;
                    View view = project.GetViewByName(dlg.SelectedDataMember);
                    IDbDriver dbDriver = DBReadExecute.GetDataDriver(project.FilePath);                    
                    relatedConnection = new EpiDashboard.RelatedConnection(view, dbDriver, dlg.ParentKeyField, dlg.ChildKeyField, dlg.UseUnmatched, false);

                    if (dashboardHelper.IsUsingEpiProject && dashboardHelper.View.Project.FilePath == project.FilePath)
                    {
                        relatedConnection.SameDataSource = true;
                    }
                }
                else if (dlg.SelectedDataSource is Project && dlg.SelectedItemType == Dialogs.SelectedItemType.Table)
                {
                    Project project = (Project)dlg.SelectedDataSource;
                    IDbDriver dbDriver = project.CollectedData.GetDbDriver();
                    relatedConnection = new EpiDashboard.RelatedConnection(dlg.SelectedDataMember, dbDriver, dlg.ParentKeyField, dlg.ChildKeyField, dlg.UseUnmatched, false);

                    if (dashboardHelper.IsUsingEpiProject && dashboardHelper.View.Project.FilePath == project.FilePath)
                    {
                        relatedConnection.SameDataSource = true;
                    }
                }
                else
                {
                    IDbDriver dbDriver = (IDbDriver)dlg.SelectedDataSource;
                    relatedConnection = new EpiDashboard.RelatedConnection(dlg.SelectedDataMember, dbDriver, dlg.ParentKeyField, dlg.ChildKeyField, dlg.UseUnmatched, false);
                }

                dashboard.ReCacheDataSource(false);

                return relatedConnection;
            }
            return null;
        }

        EpiDashboard.DashboardHelper dashboard_DashboardHelperRequested()
        {
            Epi.Windows.Dialogs.BaseReadDialog dlg = new Dialogs.BaseReadDialog(this);

            if (dashboardHelper != null && dashboardHelper.Database != null && !dashboardHelper.IsUsingEpiProject)
            {
                dlg = new Dialogs.BaseReadDialog(this, dashboardHelper.Database);
                if (dashboardHelper.CustomQuery != null && !string.IsNullOrEmpty(dashboardHelper.CustomQuery.Trim()))
                {
                    dlg.SQLQuery = dashboardHelper.CustomQuery;
                }
            }
            else if (dashboardHelper != null && dashboardHelper.Database != null && dashboardHelper.IsUsingEpiProject)
            {
                dlg = new Dialogs.BaseReadDialog(this, dashboardHelper.View.Project);
            }

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (dlg.SelectedDataSource is Project)
                {
                    Project project = (Project)dlg.SelectedDataSource;

                    if (dlg.IsFormSelected)
                    {
                        View view = project.GetViewByName(dlg.SelectedDataMember);
                        if (!File.Exists(project.FilePath))
                        {
                            MsgBox.ShowInformation(string.Format(SharedStrings.DASHBOARD_ERROR_PROJECT_NOT_FOUND, project.FilePath));
                            return null;
                        }
                        IDbDriver dbDriver = DBReadExecute.GetDataDriver(project.FilePath);
                        if (!dbDriver.TableExists(view.TableName))
                        {
                            MsgBox.ShowInformation(string.Format(SharedStrings.DATA_TABLE_NOT_FOUND, view.Name));
                            return null;
                        }
                        else
                        {
                            dashboardHelper = new EpiDashboard.DashboardHelper(view, dbDriver);
                        }
                    }
                    else
                    {
                        // Note: This pathway breaks when you try and re-set the data source, however, the UI that allows this code to be 
                        //    hit has been temporarily disabled.
                        IDbDriver dbDriver = project.CollectedData.GetDatabase();
                        dashboardHelper = new EpiDashboard.DashboardHelper(dlg.SelectedDataMember, dbDriver);
                    }
                }
                else
                {
                    IDbDriver dbDriver = (IDbDriver)dlg.SelectedDataSource;
                    if (string.IsNullOrEmpty(dlg.SQLQuery))
                    {
                        dashboardHelper = new EpiDashboard.DashboardHelper(dlg.SelectedDataMember, dbDriver);
                    }
                    else
                    {
                        dashboardHelper = new EpiDashboard.DashboardHelper(dlg.SelectedDataMember, dlg.SQLQuery, dbDriver);
                    }
                }

                return dashboardHelper;
            }
            return null;
        }

        List<object> mapControl_DataSourceRequested()
        {
            Epi.Windows.Dialogs.BaseReadDialog dlg = new Dialogs.BaseReadDialog(this);
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<object> resultsArray = new List<object>();
                resultsArray.Add(dlg.SelectedDataSource);
                resultsArray.Add(dlg.SelectedDataMember);
                return resultsArray;
            }
            else
                return null;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenCanvas();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveCanvas();
        }

        private void btnSaveHtml_Click(object sender, EventArgs e)
        {
            //this.dashboard.SaveOutputAsHTML();
            if (dashboard != null)
            {
                this.dashboard.SaveOutputAsHTMLAndDisplay();
            }
        }

        private void btnDataSource_Click(object sender, EventArgs e)
        {
            this.dashboard.SetDataSource();
        }

        private void toolStripBtnAddRelate_Click(object sender, EventArgs e)
        {
            if (dashboard != null)
            {
                this.dashboard.AddRelatedData();
            }
        }

        public void SaveCanvas()
        {
            if (dashboard != null)
            {
                this.dashboard.SaveCanvas();
            }
        }

        public void OpenCanvas()
        {
            if (dashboard == null)
            {
                return;
            }

            if (dashboard.HasGadgets)
            {
                DialogResult result = MessageBox.Show(this, SharedStrings.DASHBOARD_SAVE_CANVAS, SharedStrings.SAVE, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result.Equals(DialogResult.Yes))
                {
                    bool saved = this.dashboard.SaveCanvas();

                    if (!saved)
                    {
                        return;
                    }
                }
                else if (result.Equals(DialogResult.Cancel))
                {
                    return;
                }
            }

            this.dashboard.OpenCanvas();
        }

        public void OpenCanvas(string filePath)
        {
            if (dashboard != null)
            {
                this.dashboard.OpenCanvas(filePath);
            }
        }

        internal void SetHTMLOutputPath(string filePath) 
        {
            htmlOutputPath = filePath;
            this.dashboard.IsGeneratingHTMLFromCommandLine = true;
            this.dashboard.HTMLFilePath = filePath;
        }

        private void DashboardMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!dashboard.CanCloseDashboard())
            {
                e.Cancel = true;
            }
        }

        
    }
}
