using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Epi.Windows.Enter
{
    public delegate void RecordSelectedHandler(int id);

    public partial class SNAViewer : Form
    {
        public event EpiDashboard.SphereSelectedHandler RecordSelected;
        private EpiDashboard.SocialNetworkGraph control;
        private ElementHost host;

        public SNAViewer(View view, Epi.Data.IDbDriver db)
        {
            try
            {
                InitializeComponent();
                host = new ElementHost();
                host.Dock = DockStyle.Fill;
                control = new EpiDashboard.SocialNetworkGraph(view, db);
                control.SphereSelected += new EpiDashboard.SphereSelectedHandler(control_RecordSelected);
                host.Child = control;
                this.toolStripContainer1.ContentPanel.Controls.Add(host);
            }
            catch (Exception ex)
            {
                //catching all for debugging purposes
            }
        }

        private void control_RecordSelected(int viewId, int recordId)
        {
            this.Hide();
            if (RecordSelected != null)
                RecordSelected(viewId, recordId);
        }

        private void SNAViewer_Shown(object sender, EventArgs e)
        {
            this.Width++;
            this.Width--;
        }

        private void btnSaveAsImage_Click(object sender, EventArgs e)
        {
            control.SaveAsImage();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            control.Print();
        }

    }
}
