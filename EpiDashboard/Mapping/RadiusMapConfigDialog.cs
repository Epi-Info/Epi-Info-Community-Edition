using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Esri.ArcGISRuntime.Geometry;

namespace EpiDashboard.Mapping
{
    public partial class RadiusMapConfigDialog : Form
    {
        public RadiusMapConfigDialog()
        {
            InitializeComponent();
            txtRadius.KeyUp += new KeyEventHandler(txtRadius_KeyUp);
            cbxUnits.DataSource = Enum.GetNames(typeof(LinearUnit));
            cbxUnits.SelectedIndex = 35;
        }

        void txtRadius_KeyUp(object sender, KeyEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtRadius.Text))
            {
                decimal result = 0;
                if (decimal.TryParse(txtRadius.Text, out result))
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

        public Color SelectedColor
        {
            get
            {
                return pictureBox1.BackColor;
            }
        }

        public double Radius
        {
            get
            {
                return double.Parse(txtRadius.Text);
            }
        }

        public LinearUnit Unit
        {
            get
            {
                return (LinearUnit)Enum.Parse(typeof(LinearUnit), cbxUnits.SelectedItem.ToString());
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
    }
}
