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

namespace Epi.Windows.Enter
{
    public partial class AnalyticsViewer : System.Windows.Forms.Form// Epi.Windows.Docking.DockWindow
    {

        private EpiDashboard.DashboardControl dashboard;
        private ElementHost host;
        private EnterMainForm enterMainForm;
        private View currentView;
        private IDbDriver db;
        private Epi.EnterCheckCodeEngine.RunTimeView loadedRuntimeView;

        public AnalyticsViewer(EnterMainForm enterMainForm)
        {
            InitializeComponent();

            if (System.Windows.Application.Current == null) new System.Windows.Application();

            host = new ElementHost();
            host.Dock = DockStyle.Fill;
            this.toolStripContainer1.ContentPanel.Controls.Add(host);
            this.enterMainForm = enterMainForm;

            this.toolStripContainer1.TopToolStripPanel.Visible = false;

            this.Shown += new EventHandler(AnalyticsViewer_Shown);
        }

        void AnalyticsViewer_Shown(object sender, EventArgs e)
        {
            //Seemingly the best way to force a redraw of the hosted WPF control.
            //Done because sometimes the WPF host does not load the control at first try.
            this.Width++;
            this.Width--;
            this.WindowState = FormWindowState.Maximized;
        }

        public void UnSubscribe()
        {
            this.Controls.Clear();
            this.host = null;            
            this.dashboard = null;
            this.host = null;
            this.enterMainForm = null;
            this.currentView = null;
            this.db = null;
            this.loadedRuntimeView = null;
        }

        public void Render(Epi.View view)
        {
            try
            {
                if (!view.IsRelatedView)
                {
                    EpiDashboard.DashboardHelper helper = new EpiDashboard.DashboardHelper(view, DBReadExecute.GetDataDriver(view.Project.FilePath));
                    dashboard = new EpiDashboard.DashboardControl(helper);

                    this.WindowState = FormWindowState.Maximized;

                    helper.SetDashboardControl(dashboard);
                    dashboard.RecordCountChanged += new EpiDashboard.RecordCountChangedHandler(dashboard_RecordCountChanged);
                    host.Child = dashboard;
                    dashboard.UpdateRecordCount();
                    dashboard.ReCacheDataSource();
                }
            }
            catch (Exception ex)
            {
                //temporarily catch all
            }
        }

        void dashboard_RecordCountChanged(int recordCount, string dataSourceName)
        {
            lblRecordCount.Visible = true;
            txtRecordCount.Visible = true;
            if (recordCount > int.MinValue)
            {
                txtRecordCount.Text = recordCount.ToString();
            }
            else
            {
                txtRecordCount.Text = "...";
            }
        }

        private void btnAddGadget_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This Epi Info 7 preview comes bundled with three gadgets - Frequency, 2 x 2 Tables, and Graph. The final release will enable users to add downloaded gadgets at runtime to fully customize the dashboard experience.", "Epi Info 7 Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            this.dashboard.OpenCanvas();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.dashboard.SaveCanvas();
        }

        private void btnSaveHtml_Click(object sender, EventArgs e)
        {
            this.dashboard.SaveOutputAsHTML();
        }

    }
}
