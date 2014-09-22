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
    /// Interaction logic for GadgetInfoPanel.xaml
    /// </summary>
    public partial class GadgetInfoPanel : UserControl
    {
        public GadgetInfoPanel()
        {
            InitializeComponent();
            Text = string.Empty;
            Construct();
        }

        public GadgetInfoPanel(string message)
        {
            InitializeComponent();
            Text = message;
            Construct();
        }

        private void Construct()
        {
            System.Drawing.Icon icon = System.Drawing.SystemIcons.Information;
            BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle,
            Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            img.Source = bs;
            img.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            img.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            img.Height = 16;
            img.Width = 16;
            img.Margin = new Thickness(6);
            iconPanel.Children.Add(img);            
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
    }
}
