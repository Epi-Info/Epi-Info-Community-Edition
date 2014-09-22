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
    public enum MessagePanelType
    {
        ErrorPanel = 1,
        WarningPanel = 2,
        StatusPanel = 3
    }

    /// <summary>
    /// Interaction logic for GadgetMessagePanel.xaml
    /// </summary>
    public partial class GadgetMessagePanel : UserControl
    {
        private MessagePanelType messagePanelType = MessagePanelType.WarningPanel;

        private Brush errorBackgroundBrush = new SolidColorBrush(Colors.PeachPuff);
        private Brush errorBorderBrush = new SolidColorBrush(Colors.Red);

        private Brush warningBackgroundBrush = new SolidColorBrush(Colors.Cornsilk);
        private Brush warningBorderBrush = new SolidColorBrush(Colors.Orange);

        private Brush statusBackgroundBrush = new SolidColorBrush(Colors.Azure);
        private Brush statusBorderBrush = new SolidColorBrush(Colors.Green);

        public GadgetMessagePanel()
        {
            InitializeComponent();
            MessagePanelType = Controls.MessagePanelType.StatusPanel;
            Text = string.Empty;
        }

        public GadgetMessagePanel(MessagePanelType panelType, string message)
        {
            InitializeComponent();
            MessagePanelType = panelType;
            Text = message;
        }

        public string Text
        {
            get
            {
                return this.txtStatus.Text;
            }
            set
            {
                this.txtStatus.Text = value;
            }
        }

        public bool HasProgressBar
        {
            get
            {
                if (this.progressBar.Visibility == System.Windows.Visibility.Visible)
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
                if (value)
                {
                    this.progressBar.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    this.progressBar.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        public MessagePanelType MessagePanelType
        {
            get
            {
                return this.messagePanelType;
            }
            set
            {
                this.messagePanelType = value;

                grdWarningIcon.Visibility = System.Windows.Visibility.Collapsed;
                grdErrorIcon.Visibility = System.Windows.Visibility.Collapsed;

                switch (this.messagePanelType)
                {
                    case MessagePanelType.ErrorPanel:
                        pnlStatus.Background = errorBackgroundBrush;
                        pnlStatusTop.Background = errorBorderBrush;
                        grdErrorIcon.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case MessagePanelType.WarningPanel:
                        pnlStatus.Background = warningBackgroundBrush;
                        pnlStatusTop.Background = warningBorderBrush;
                        grdWarningIcon.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case MessagePanelType.StatusPanel:
                        pnlStatus.Background = statusBackgroundBrush;
                        pnlStatusTop.Background = statusBorderBrush;
                        break;
                }
            }
        }

        public void ShowProgressBar()
        {
            this.HasProgressBar = true;
        }

        public void HideProgressBar()
        {
            this.HasProgressBar = false;
        }

        public void SetProgressBarMax(double maxValue)
        {
            progressBar.Maximum = maxValue;
        }

        public void SetProgressBarValue(double value)
        {
            progressBar.Value = value;
        }
    }
}
