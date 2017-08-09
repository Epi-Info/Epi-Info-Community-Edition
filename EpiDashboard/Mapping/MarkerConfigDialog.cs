using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;

namespace EpiDashboard.Mapping
{
    public partial class MarkerConfigDialog : Form
    {
        public MarkerConfigDialog()
        {
            InitializeComponent();
            cbxStyle.DataSource = Enum.GetNames(typeof(SimpleMarkerSymbol.SimpleMarkerStyle));
            //cbxStyle.SelectedIndex = 35;
        }

        public SimpleMarkerSymbol Marker
        {
            get
            {
                SimpleMarkerSymbol symbol = new SimpleMarkerSymbol();
                symbol.Color = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(pictureBox1.BackColor.R, pictureBox1.BackColor.G, pictureBox1.BackColor.B));
                symbol.Size = double.Parse(txtSize.Text);
                symbol.Style = (SimpleMarkerSymbol.SimpleMarkerStyle)Enum.Parse(typeof(SimpleMarkerSymbol.SimpleMarkerStyle), cbxStyle.SelectedItem.ToString());
                return symbol;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtSize_KeyUp(object sender, KeyEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSize.Text))
            {
                decimal result = 0;
                if (decimal.TryParse(txtSize.Text, out result))
                {
                    btnOk.Enabled = true;
                }
                else
                {
                    btnOk.Enabled = false;
                }
            }
            else
            {
                btnOk.Enabled = false;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            DialogResult result = colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                pictureBox1.BackColor = colorDialog.Color;
            }
        }
    }
}
