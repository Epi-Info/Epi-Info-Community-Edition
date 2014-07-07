using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for SettingsToggleButton.xaml
    /// </summary>
    public partial class SettingsToggleButton : ToggleButton
    {
        public SettingsToggleButton()
        {
            InitializeComponent();
        }

        public string Title
        {
            get
            {
                return this.tblockTitle.Text;
            }
            set
            {
                this.tblockTitle.Text = value;
            }
        }

        public string Description
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
