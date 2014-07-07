using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Epi.Windows.MakeView.Forms
{
    public partial class WaitPanel : Panel
    {
        public WaitPanel(Size parentSize)
        {
            InitializeComponent();

            this.Size = new Size(450, 80);
            this.Location = new Point(parentSize.Width - Size.Width/2, 300);
            
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            
            labelStatus.Location = new Point(25, 41);
            this.Controls.Add(labelStatus);
        }

        public void Update(string status)
        {
            labelStatus.Text = status;
        }

        public void HidePanel()
        {
            base.Hide();
        }

        public void ShowPanel()
        {
            ShowPanel("Please wait...");
        }
        
        public void ShowPanel(string message)
        {
            labelStatus.Text = message;
            BringToFront();
            base.Show();
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
    }
}
