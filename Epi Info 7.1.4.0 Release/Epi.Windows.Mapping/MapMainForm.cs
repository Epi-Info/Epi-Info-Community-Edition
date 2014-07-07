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

namespace Epi.Windows.Mapping
{
    public partial class MapMainForm : Form
    {
        private ElementHost host;
        private EpiDashboard.Mapping.StandaloneMapControl mapControl;

        public MapMainForm()
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Configuration.GetNewInstance().Settings.Language);

            InitializeComponent();

            host = new ElementHost();
            host.Dock = DockStyle.Fill;
            this.Controls.Add(host);

            mapControl = new EpiDashboard.Mapping.StandaloneMapControl();
            mapControl.DataSourceRequested += new EpiDashboard.Mapping.DataSourceRequestedHandler(mapControl_DataSourceRequested);
            mapControl.MouseCoordinatesChanged += new EpiDashboard.Mapping.MouseCoordinatesChangedHandler(mapControl_MouseCoordinatesChanged);
            mapControl.MapLoaded +=new EpiDashboard.Mapping.MapLoadedHandler(mapControl_MapLoaded);
            mapControl.ExpandRequested += new EventHandler(mapControl_ExpandRequested);
            mapControl.RestoreRequested += new EventHandler(mapControl_RestoreRequested);
            host.Child = mapControl;
            this.Width++;
            this.Width--;
        }

        void mapControl_RestoreRequested(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        }

        void mapControl_ExpandRequested(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Bounds = Screen.PrimaryScreen.Bounds;
        }

        void mapControl_MapLoaded(EpiDashboard.Mapping.StandaloneMapControl control, bool isTimeLapsePossible)
        {
            //btnAddLayer.Enabled = true;
            //btnOpen.Enabled = true;
            //btnSave.Enabled = true;
            //btnSaveImage.Enabled = true;
            //btnTimeLapse.Enabled = isTimeLapsePossible;
            //btnReference.Enabled = true;
        }

        void mapControl_MouseCoordinatesChanged(double latitude, double longitude)
        {
            this.Text = "Map - (" + latitude + ", " + longitude + ")";
        }

        List<object> mapControl_DataSourceRequested()
        {
            Epi.Windows.Dialogs.BaseReadDialog dlg = new Dialogs.BaseReadDialog();
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
            mapControl.OpenMap();
        }

        public void OpenMap(string filePath)
        {
            if (mapControl != null)
            {
                this.mapControl.OpenMap(filePath);
            }
        }

        public void SetWhiteBackground()
        {
            if (mapControl != null)
            {
                this.mapControl.MapBackground = System.Windows.Media.Brushes.White;
            }
        }

        public void SetMap(string filePath)
        {
            if (mapControl != null)
            {
                this.mapControl.SetMap(filePath);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            mapControl.SaveMap();
        }

        private void btnSaveImage_Click(object sender, EventArgs e)
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

        private void fromKMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapControl.AddKmlLayer();
        }

        private void choroplethKMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapControl.GenerateKmlChoropleth();
        }

        private void withShapeFileBoundariesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            mapControl.GenerateShapeFileChoropleth();
        }

        private void withMapServerBoundariesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            mapControl.GenerateMapServerChoropleth();
        }

        private void withKMLBoundariesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            mapControl.GenerateKmlChoropleth();
        }

        private void withKMLBoundariesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapControl.GenerateKmlDotDensity();
        }

        private void withShapeFileBoundariesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapControl.GenerateShapeFileDotDensity();
        }

        private void withMapServerBoundariesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapControl.GenerateMapServerDotDensity();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            mapControl.GeneratePointMap();
        }

        private void toolStripContainer1_ContentPanel_Load(object sender, EventArgs e)
        {

        }
    }
}
