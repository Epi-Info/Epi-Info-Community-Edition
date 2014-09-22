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
    /// Interaction logic for StatusBarArrangeButton.xaml
    /// </summary>
    public partial class StatusBarArrangeButton : UserControl
    {
        bool isSelected = false;

        public event StatusStripButtonHandler Click;

        public StatusBarArrangeButton()
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
                    borderOuter.Style = this.Resources["mainOuterBorderSelected"] as Style;
                }
                else
                {
                    borderOuter.Style = this.Resources["mainOuterBorder"] as Style;
                }
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
