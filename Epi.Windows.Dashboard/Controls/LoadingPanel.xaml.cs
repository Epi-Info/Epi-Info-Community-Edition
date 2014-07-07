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
    /// Interaction logic for LoadingPanel.xaml
    /// </summary>
    public partial class LoadingPanel : UserControl
    {
        public LoadingPanel()
        {
            InitializeComponent();
            spinCursor.CursorBackground = this.Resources["windowDarkColor"] as SolidColorBrush;
        }

        public string HeadingText
        {
            get
            {
                return this.tblockHeading.Text;
            }
            set
            {
                this.tblockHeading.Text = value;
            }
        }

        public string DescriptionText
        {
            get
            {
                return this.tblockDescription.Text;
            }
            set
            {
                this.tblockDescription.Text = value;
            }
        }

        public new Brush Background
        {
            get
            {
                return this.grdMain.Background;
            }
            set
            {
                this.grdMain.Background = value;
                this.spinCursor.Background = value;
                this.spinCursor.CursorBackground = value;
            }
        }
    }
}
