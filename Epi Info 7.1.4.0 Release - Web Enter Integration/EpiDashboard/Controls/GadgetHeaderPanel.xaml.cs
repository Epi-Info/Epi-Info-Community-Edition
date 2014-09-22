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
    /// Interaction logic for GadgetHeaderPanel.xaml
    /// </summary>
    public partial class GadgetHeaderPanel : UserControl
    {
        public event GadgetCloseButtonHandler GadgetCloseButtonClicked;
        public event GadgetConfigButtonHandler GadgetConfigButtonClicked;
        public event GadgetOutputExpandButtonHandler GadgetOutputExpandButtonClicked;
        public event GadgetOutputCollapseButtonHandler GadgetOutputCollapseButtonClicked;
        public event GadgetDescriptionButtonHandler GadgetDescriptionButtonClicked;
        public event GadgetFilterButtonHandler GadgetFilterButtonClicked;

        private bool isOutputCollapsed = false;

        public GadgetHeaderPanel()
        {
            InitializeComponent();
            toolTipDataFilters.Content = DashboardSharedStrings.TOOLTIP_GADGET_DATA_FILTERS;
            toolTipConfig.Content = DashboardSharedStrings.TOOLTIP_GADGET_CONFIG;
            toolTipDesc.Content = DashboardSharedStrings.TOOLTIP_GADGET_DESC_PANEL;
            toolTipClose.Content = DashboardSharedStrings.TOOLTIP_CLOSE_GADGET;
            toolTipCollapseOutput.Content = DashboardSharedStrings.TOOLTIP_COLLAPSE_GADGET;
        }

        public string Text
        {
            get
            {
                return this.gadgetTypeText.Text;
            }
            set
            {
                this.gadgetTypeText.Text = value;
            }
        }

        public void HideButtons()
        {
            grdConfigure.Visibility = System.Windows.Visibility.Collapsed;
            grdCustomFilter.Visibility = System.Windows.Visibility.Collapsed;
            grdDescription.Visibility = System.Windows.Visibility.Collapsed;
            grdErrorIcon.Visibility = System.Windows.Visibility.Collapsed;
            grdExpandCollapse.Visibility = System.Windows.Visibility.Collapsed;
        }

        public void ShowButtons()
        {
            grdConfigure.Visibility = System.Windows.Visibility.Visible;
            grdCustomFilter.Visibility = System.Windows.Visibility.Visible;
            grdDescription.Visibility = System.Windows.Visibility.Visible;
            grdErrorIcon.Visibility = System.Windows.Visibility.Visible;
            grdExpandCollapse.Visibility = System.Windows.Visibility.Visible;
        }

        private void PathDesc_MouseEnter(object sender, MouseEventArgs e)
        {
            descRectangle1.Style = this.Resources["descRectangleHover"] as Style;
            this.Cursor = Cursors.Hand;
        }

        private void PathDesc_MouseLeave(object sender, MouseEventArgs e)
        {
            descRectangle1.Style = this.Resources["descRectangle"] as Style;
            this.Cursor = Cursors.Arrow;
        }

        private void PathDesc_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GadgetDescriptionButtonClicked != null)
            {
                GadgetDescriptionButtonClicked();
            }
        }

        private void PathFilter_MouseEnter(object sender, MouseEventArgs e)
        {
            filterRectangle1.Style = this.Resources["filterRectangleHover"] as Style;
            this.Cursor = Cursors.Hand;
        }

        private void PathFilter_MouseLeave(object sender, MouseEventArgs e)
        {
            filterRectangle1.Style = this.Resources["filterRectangle"] as Style;
            this.Cursor = Cursors.Arrow;
        }

        private void PathFilter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GadgetFilterButtonClicked != null)
            {
                GadgetFilterButtonClicked();
            }
        }

        private void PathConfig_MouseEnter(object sender, MouseEventArgs e)
        {
            configRectangle1.Style = this.Resources["configRectangleHover"] as Style;
            this.Cursor = Cursors.Hand;
        }

        private void PathConfig_MouseLeave(object sender, MouseEventArgs e)
        {
            configRectangle1.Style = this.Resources["configRectangle"] as Style;
            this.Cursor = Cursors.Arrow;
        }

        private void PathConfig_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GadgetConfigButtonClicked != null)
            {
                GadgetConfigButtonClicked();
            }  
        }

        private void PathCollapse_MouseEnter(object sender, MouseEventArgs e)
        {
            expandCollapseRectangle1.Style = this.Resources["collapseRectangleHover"] as Style;
            if (isOutputCollapsed)
            {
                pathTriangle.Style = this.Resources["expandTriangleHover"] as Style;
            }
            else
            {
                pathTriangle.Style = this.Resources["collapseTriangleHover"] as Style;
            }
            this.Cursor = Cursors.Hand;
        }

        private void PathCollapse_MouseLeave(object sender, MouseEventArgs e)
        {
            expandCollapseRectangle1.Style = this.Resources["collapseRectangle"] as Style;
            if (isOutputCollapsed)
            {
                pathTriangle.Style = this.Resources["expandTriangle"] as Style;
            }
            else
            {
                pathTriangle.Style = this.Resources["collapseTriangle"] as Style;
            }
            this.Cursor = Cursors.Arrow;
        }

        private void PathCollapse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isOutputCollapsed)
            {
                isOutputCollapsed = false;
                if (GadgetOutputExpandButtonClicked != null)
                {
                    GadgetOutputExpandButtonClicked();
                }
                pathTriangle.Style = this.Resources["collapseTriangle"] as Style;
            }
            else
            {
                isOutputCollapsed = true;
                if (GadgetOutputCollapseButtonClicked != null)
                {
                    GadgetOutputCollapseButtonClicked();
                }
                pathTriangle.Style = this.Resources["expandTriangle"] as Style;
            }
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
            if (GadgetCloseButtonClicked != null)
            {
                GadgetCloseButtonClicked();
            }
        }
        
        public bool IsCloseButtonAvailable
        {
            get
            {
                if (this.gridHeader.ColumnDefinitions[6].Width.Value == 20)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == true)
                {
                    this.gridHeader.ColumnDefinitions[6].Width = new GridLength(20);
                }
                else
                {
                    this.gridHeader.ColumnDefinitions[6].Width = new GridLength(0);
                }
            }
        }

        public bool IsOutputCollapseButtonAvailable
        {
            get
            {
                if (this.gridHeader.ColumnDefinitions[5].Width.Value == 20)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == true)
                {
                    this.gridHeader.ColumnDefinitions[5].Width = new GridLength(20);
                }
                else
                {
                    this.gridHeader.ColumnDefinitions[5].Width = new GridLength(0);
                }
            }
        }

        public bool IsDescriptionButtonAvailable
        {
            get
            {
                if (this.gridHeader.ColumnDefinitions[4].Width.Value == 20)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == true)
                {
                    this.gridHeader.ColumnDefinitions[4].Width = new GridLength(20);
                }
                else
                {
                    this.gridHeader.ColumnDefinitions[4].Width = new GridLength(0);
                }
            }
        }

        public bool IsSettingsButtonAvailable
        {
            get
            {
                if (this.gridHeader.ColumnDefinitions[3].Width.Value == 20)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == true)
                {
                    this.gridHeader.ColumnDefinitions[3].Width = new GridLength(20);
                }
                else
                {
                    this.gridHeader.ColumnDefinitions[3].Width = new GridLength(0);
                }
            }
        }

        public bool IsFilterButtonAvailable
        {
            get
            {
                if (this.gridHeader.ColumnDefinitions[2].Width.Value == 20)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == true)
                {
                    this.gridHeader.ColumnDefinitions[2].Width = new GridLength(20);
                }
                else
                {
                    this.gridHeader.ColumnDefinitions[2].Width = new GridLength(0);
                }
            }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.IBeam;
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }
    }
}
