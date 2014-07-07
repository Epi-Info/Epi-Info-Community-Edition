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
                        tblockDescription.Visibility = System.Windows.Visibility.Collapsed;
                        txtOutputDescription.Visibility = System.Windows.Visibility.Collapsed;
                        tblockOutputDescription.Visibility = System.Windows.Visibility.Visible;
                        panelDescription.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case DescriptionPanelMode.EditMode:
                        tblockDescription.Visibility = System.Windows.Visibility.Visible;
                        txtOutputDescription.Visibility = System.Windows.Visibility.Visible;
                        tblockOutputDescription.Visibility = System.Windows.Visibility.Collapsed;
                        panelDescription.Visibility = System.Windows.Visibility.Visible;
                        break;
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
                this.txtOutputDescription.Text = value;
            }
        }

        private void txtOutputDescription_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tblockOutputDescription.Text = txtOutputDescription.Text;
                this.PanelMode = DescriptionPanelMode.DisplayMode;
            }
            else if (e.Key == Key.Escape)
            {
                txtOutputDescription.Text = string.Empty;
                this.PanelMode = DescriptionPanelMode.DisplayMode;
            }
        }

        private void txtOutputDescription_LostFocus(object sender, RoutedEventArgs e)
        {
            txtOutputDescription.Text = string.Empty;
            this.PanelMode = DescriptionPanelMode.DisplayMode;
        }

        private void tblockOutputDescription_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void tblockOutputDescription_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void tblockOutputDescription_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.PanelMode = DescriptionPanelMode.EditMode;
            this.txtOutputDescription.Text = this.tblockOutputDescription.Text;
        }
    }
}
