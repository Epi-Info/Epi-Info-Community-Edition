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

namespace Epi.WPF.Controls
{
    /// <summary>
    /// Interaction logic for DashboardFileMenuItem.xaml
    /// </summary>
    public partial class DashboardFileMenuItem : UserControl
    {
        public bool isSelected = false;
        public event MouseButtonEventHandler MouseLeftButtonUp;

        public DashboardFileMenuItem()
        {
            InitializeComponent();
        }

        public string Text
        {
            get
            {
                return this.tblockMain.Text;
            }
            set
            {
                this.tblockMain.Text = value;
            }
        }

        public void Select()
        {
            borderMain.Style = this.Resources["StandardBorderSelectedStyle"] as Style;
            tblockMain.Foreground = this.Resources["windowDarkColor"] as SolidColorBrush;
            isSelected = true;            
        }

        public void Deselect()
        {
            borderMain.Style = this.Resources["StandardBorderStyle"] as Style;
            tblockMain.Foreground = this.Resources["windowTextColor"] as SolidColorBrush;
            isSelected = false;            
        }

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }
        }

        private void border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (MouseLeftButtonUp != null)
            {
                MouseLeftButtonUp(this, e);
            }
        }
    }
}
