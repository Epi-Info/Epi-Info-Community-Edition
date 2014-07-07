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
    /// Interaction logic for DashboardTabItem.xaml
    /// </summary>
    public partial class DashboardTabItem : UserControl
    {
        public bool isSelected = false;
        public event MouseButtonEventHandler MouseLeftButtonUp;

        public DashboardTabItem()
        {
            InitializeComponent();
        }

        public void Select()
        {
            borderOuter.BorderThickness = new Thickness(1, 0, 1, 1);
            borderOuter.Margin = new Thickness(0, 0, 0, 3);
            borderInner.BorderThickness = new Thickness(0, 0, 0, 2);
            panelMain.Background = Brushes.White;
            tblockTab.Foreground = this.Resources["statusBarBackground"] as Brush;
            isSelected = true;
            borderOuter.Style = null;
        }

        public void Deselect()
        {
            borderOuter.BorderThickness = new Thickness(0);
            borderOuter.Margin = new Thickness(0);
            borderInner.BorderThickness = new Thickness(0);
            panelMain.Background = Brushes.Transparent;
            tblockTab.Foreground = this.Resources["windowTextColor"] as Brush;
            isSelected = false;
            borderOuter.Style = this.Resources["outerDeselectedBorderStyle"] as Style;
        }

        public string Text
        {
            get
            {
                return this.tblockTab.Text;
            }
            set
            {
                tblockTab.Text = value;
            }
        }

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }
        }

        //private void rectangle_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    if (!IsSelected)
        //    {
        //        panelMain.Background = Brushes.DarkGray;
        //    }
        //}

        //private void rectangle_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    if (!IsSelected)
        //    {
        //        panelMain.Background = Brushes.Transparent;
        //    }
        //}

        private void border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (MouseLeftButtonUp != null)
            {
                MouseLeftButtonUp(this, e);
            }            
        }
    }
}
