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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Epi.WPF.Dashboard.Controls
{
    /// <summary>
    /// Interaction logic for GadgetWaitPanel.xaml
    /// </summary>
    public partial class GadgetWaitPanel : UserControl
    {
        public GadgetWaitPanel()
        {
            InitializeComponent();
        }

        public string Text
        {
            get
            {
                return this.statusText.Text;
            }
            set
            {
                this.statusText.Text = value;
            }
        }

        public double CircleThickness
        {
            get
            {
                return this.spinCursor.CircleThickness;
            }
            set
            {
                this.spinCursor.CircleThickness = value;
            }
        }
    }
}
