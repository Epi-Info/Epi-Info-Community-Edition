using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Epi;
using Epi.Data;

namespace Epi.Windows.Enter
{
    public partial class LineListingViewer : Form
    {
        public event RecordSelectedHandler RecordSelected;
        private EpiDashboard.SimpleDataGrid control;
        private ElementHost host;

        public LineListingViewer(View view, IDbDriver db, string title)
        {
            try
            {
                InitializeComponent();
                host = new ElementHost();
                host.Dock = DockStyle.Fill;
                control = new EpiDashboard.SimpleDataGrid(view, db);
                control.RecordSelected += new EpiDashboard.Mapping.RecordSelectedHandler(control_RecordSelected);
                host.Child = control;
                this.Controls.Add(host);
                this.Text += " - " + title;
                this.Shown += new EventHandler(LineListingViewer_Shown);
            }
            catch (Exception ex)
            {
                //catching all for debugging purposes
            }
        }

        void LineListingViewer_Shown(object sender, EventArgs e)
        {
            //Seemingly the best way to force a redraw of the hosted WPF control.
            //Done because sometimes the WPF host does not load the control at first try.
            this.Width++;
            this.Width--;
            this.WindowState = FormWindowState.Maximized;
        }

        void control_RecordSelected(int id)
        {
            if (RecordSelected != null)
                RecordSelected(id);
        }


        public LineListingViewer()
        {
            InitializeComponent();
        }


    }
}
