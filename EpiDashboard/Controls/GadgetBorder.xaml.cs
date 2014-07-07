using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for GadgetBorder.xaml
    /// </summary>
    public partial class GadgetBorder : UserControl
    {
        public GadgetBorder()
        {
            InitializeComponent();
        }

        private void borderAll_MouseEnter(object sender, MouseEventArgs e)
        {
            object el = this.FindName("mainGadgetBorderBackground");
        }

        private void borderAll_MouseLeave(object sender, MouseEventArgs e)
        {
            
        }
    }
}
