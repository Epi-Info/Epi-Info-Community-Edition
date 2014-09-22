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
    public partial class MapViewer : System.Windows.Forms.Form 
    {
        private ElementHost host;
        private EnterMainForm enterMainForm;
        private View currentView;
        private IDbDriver db;
        private BackgroundWorker worker;
        private EpiDashboard.Mapping.StandaloneMapControl mapControl;

        public MapViewer(EnterMainForm enterMainForm)
        {
            InitializeComponent();

            host = new ElementHost();
            host.Dock = DockStyle.Fill;
            this.toolStripContainer1.ContentPanel.Controls.Add(host);

            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();

            this.enterMainForm = enterMainForm;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mapControl = new EpiDashboard.Mapping.StandaloneMapControl();
            mapControl.RecordSelected += new EpiDashboard.Mapping.RecordSelectedHandler(mapControl_RecordSelected);
            mapControl.DataSourceRequested += new EpiDashboard.Mapping.DataSourceRequestedHandler(mapControl_DataSourceRequested);
            mapControl.MapLoaded += new EpiDashboard.Mapping.MapLoadedHandler(mapControl_MapLoaded);
            host.Child = mapControl;
            this.Width++;
            this.Width--;
        }

        void mapControl_MapLoaded(EpiDashboard.Mapping.StandaloneMapControl mapForm, bool isTimeLapsePossible)
        {
            btnDataLayer.Enabled = true;
            btnOpen.Enabled = true;
            btnSave.Enabled = true;
            btnSaveAsImage.Enabled = true;
            btnTimeLapse.Enabled = isTimeLapsePossible;
            btnReference.Enabled = true;
        }

        List<object> mapControl_DataSourceRequested()
        {
            if (MessageBox.Show("Use external data?", "Data source", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                Epi.Windows.Dialogs.BaseReadDialog dlg = new Epi.Windows.Dialogs.BaseReadDialog();
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    List<object> resultsArray = new List<object>();
                    resultsArray.Add(dlg.SelectedDataSource);
                    resultsArray.Add(dlg.SelectedDataMember);
                    return resultsArray;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                List<object> resultsArray = new List<object>();
                resultsArray.Add(currentView.Project);
                resultsArray.Add(currentView.Name);
                return resultsArray;
            }
        }

        void mapControl_RecordSelected(int id)
        {
            enterMainForm.LoadRecord(id);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(1000);
        }

        public void Render(Epi.View view)
        {
            try
            {
                if (!view.IsRelatedView)
                {
                    this.currentView = view;
                }
            }
            catch (Exception ex)
            {
                //temporarily catch all
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            mapControl.OpenMap();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            mapControl.SaveMap();
        }

        private void btnSaveAsImage_Click(object sender, EventArgs e)
        {
            mapControl.SaveAsImage();
        }

        private void caseClusterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapControl.GenerateCaseCluster();
        }

        private void choroplethToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapControl.GenerateShapeFileChoropleth();
        }

        private void btnTimeLapse_Click(object sender, EventArgs e)
        {
            mapControl.CreateTimeLapse();
        }

        private void fromMapServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapControl.AddMapServerLayer();
        }

        private void fromShapeFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapControl.AddShapeFileLayer();
        }

        private void choroplethMapServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapControl.GenerateMapServerChoropleth();
        }

        private void withShapeFileBoundariesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapControl.GenerateShapeFileChoropleth();
        }

        private void withMapServerBoundariesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapControl.GenerateMapServerChoropleth();
        }

        private void withKMLBoundariesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapControl.GenerateKmlChoropleth();
        }

        private void withShapeFileBoundariesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            mapControl.GenerateShapeFileDotDensity();
        }

        private void withMapServerBoundariesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            mapControl.GenerateMapServerDotDensity();
        }

        private void withKMLBoundariesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            mapControl.GenerateKmlDotDensity();
        }

        private void fromKMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapControl.AddKmlLayer();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            mapControl.GeneratePointMap();
        }

    }
}
