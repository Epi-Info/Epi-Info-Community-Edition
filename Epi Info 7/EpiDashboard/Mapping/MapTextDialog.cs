using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Client.Symbols;

namespace EpiDashboard.Mapping
{
    public partial class MapTextDialog : Form
    {

        private Font selectedFont;

        public MapTextDialog()
        {
            InitializeComponent();
            selectedFont = new Font(FontFamily.GenericSansSerif, 12);
            txtFont.Text = selectedFont.Name;
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

        private void btnFont_Click(object sender, EventArgs e)
        {
            FontDialog dialog = new FontDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtFont.Text = dialog.Font.Name;
                selectedFont = dialog.Font;
            }
        }

        public TextSymbol MapText
        {
            get
            {
                TextSymbol textSymbol = new TextSymbol()
                {
                    FontFamily = new System.Windows.Media.FontFamily(txtFont.Text),
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(pictureBox1.BackColor.R, pictureBox1.BackColor.G, pictureBox1.BackColor.B)),
                    FontSize = double.Parse(selectedFont.Size.ToString()), 
                    Text = txtText.Text
                };
                return textSymbol;
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

        private void txtText_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtText.Text))
            {
                btnOk.Enabled = true;
            }
            else
            {
                btnOk.Enabled = false;
            }
        }
    }
}
