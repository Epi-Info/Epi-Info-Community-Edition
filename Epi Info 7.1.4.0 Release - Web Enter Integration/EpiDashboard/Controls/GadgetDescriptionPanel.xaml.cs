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
    /// Interaction logic for GadgetDescriptionPanel.xaml
    /// </summary>
    public partial class GadgetDescriptionPanel : UserControl
    {
        public enum DescriptionPanelMode
        {
            Collapsed = 1,
            EditMode = 2,
            DisplayMode = 3
        }

        private DescriptionPanelMode panelMode;

        public GadgetDescriptionPanel()
        {
            InitializeComponent();
        }

        public DescriptionPanelMode PanelMode
        {
            get
            {
                return this.panelMode;
            }
            set
            {
                this.panelMode = value;

                switch (PanelMode)
                {
                    case DescriptionPanelMode.Collapsed:
                        panelDescription.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                    case DescriptionPanelMode.DisplayMode:
                        tblockOutputDescription.Visibility = System.Windows.Visibility.Visible;
                        panelDescription.Visibility = System.Windows.Visibility.Visible;
                        break;
                    //case DescriptionPanelMode.EditMode:
                    //    tblockDescription.Visibility = System.Windows.Visibility.Visible;
                    //    txtOutputDescription.Visibility = System.Windows.Visibility.Visible;
                    //    tblockOutputDescription.Visibility = System.Windows.Visibility.Collapsed;
                    //    panelDescription.Visibility = System.Windows.Visibility.Visible;
                    //    break;
                }
            }
        }

        public string Text
        {
            get
            {
                return this.tblockOutputDescription.Text;
            }
            set
            {
                this.tblockOutputDescription.Text = value;
            }
        }
    }
}
