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
    /// Interaction logic for CircularButton.xaml
    /// </summary>
    public partial class CircularButton : UserControl
    {
        public CircularButton()
        {
            InitializeComponent();
        }

        public bool HasMinusSymbol
        {
            get
            {
                return (this.verticalLine.Visibility == System.Windows.Visibility.Visible);
            }
            set
            {
                if (value) this.verticalLine.Visibility = System.Windows.Visibility.Visible;
                else this.verticalLine.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }
}
