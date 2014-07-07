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
    /// Interaction logic for InstructionsPanel.xaml
    /// </summary>
    public partial class InstructionsPanel : UserControl
    {
        public InstructionsPanel()
        {
            InitializeComponent();

            #region Translation
            tblockHeading.Text = DashboardSharedStrings.DATA_PROCESSING_FINISHED;
            tblockDescription.Text = DashboardSharedStrings.INSTRUCTIONS_FINISHED_CACHING;
            #endregion // Translation

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
            }
        }

        public new Brush Foreground
        {
            get
            {
                return this.tblockDescription.Foreground;
            }
            set
            {
                this.tblockDescription.Foreground = value;
                this.tblockHeading.Foreground = value;
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
    }
}
