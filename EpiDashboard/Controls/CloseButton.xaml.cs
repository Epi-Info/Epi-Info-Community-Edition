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

namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for CloseButton.xaml
    /// </summary>
    public partial class CloseButton : UserControl
    {
        public event GadgetCloseButtonHandler Clicked;

        public CloseButton()
        {
            InitializeComponent();
        }

        private void PathX_MouseEnter(object sender, MouseEventArgs e)
        {
            closeRectangle1.Style = this.Resources["closeRectangleHover"] as Style;
            pathX.Style = this.Resources["closeXHover"] as Style;
            this.Cursor = Cursors.Hand;
        }

        private void PathX_MouseLeave(object sender, MouseEventArgs e)
        {
            closeRectangle1.Style = this.Resources["closeRectangle"] as Style;
            pathX.Style = this.Resources["closeX"] as Style;
            this.Cursor = Cursors.Arrow;
        }

        private void PathX_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null)
            {
                Clicked();
            }
        }
    }
}
