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
using Epi;



namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for LoadingPanel.xaml
    /// </summary>
    public partial class LoadingPanel : UserControl
    {
        public LoadingPanel()
        {
            InitializeComponent();
            tblockHeading.Text = SharedStrings.DASHBOARD_GADGET_PROCESSING;
            tblockDescription.Text = SharedStrings.DASHBOARD_GADGET_STATUS_RELATING_PAGES_NO_PROGRESS;
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
            }
        }
    }
}
