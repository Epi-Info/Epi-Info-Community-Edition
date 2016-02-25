using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace StatCalc
{
    public partial class StatCalcForm : Form
    {
        private ElementHost host;
        public StatCalcForm()
        {
            InitializeComponent();
            host = new ElementHost();
            host.Dock = DockStyle.Fill;
            StatCalcMenu form = new StatCalcMenu();
            host.Child = form;
            this.Controls.Add(host);
        }
    }
}
