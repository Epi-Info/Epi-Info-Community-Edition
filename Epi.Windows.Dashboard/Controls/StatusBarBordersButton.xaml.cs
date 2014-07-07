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
    /// Interaction logic for StatusBarBordersButton.xaml
    /// </summary>
    public partial class StatusBarBordersButton : UserControl
    {
        private bool isSelected = false;
        private Brush selectedBackground = new SolidColorBrush(Color.FromRgb(46, 82, 150));
        private Brush unselectedBackground;

        public event StatusStripButtonHandler Click;

        public StatusBarBordersButton()
        {
            InitializeComponent();
        }

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }
            set
            {
                this.isSelected = value;

                if (IsSelected)
                {
                    panelMain.Background = selectedBackground;
                }
                else
                {
                    panelMain.Background = unselectedBackground;
                }
            }
        }

        new public Brush Background
        {
            get
            {
                return unselectedBackground;
            }
            set
            {
                unselectedBackground = value;
                panelMain.Background = unselectedBackground;
            }
        }

        public Brush SelectedBackground
        {
            get
            {
                return selectedBackground;
            }
            set
            {
                selectedBackground = value;
            }
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsSelected = !IsSelected;

            if (Click != null)
            {
                Click();
            }
        }
    }
}
