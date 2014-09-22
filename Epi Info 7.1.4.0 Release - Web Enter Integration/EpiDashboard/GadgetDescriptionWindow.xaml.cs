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

namespace EpiDashboard
{
    /// <summary>
    /// Interaction logic for GadgetDescriptionWindow.xaml
    /// </summary>
    public partial class GadgetDescriptionWindow : Window
    {
        public GadgetDescriptionWindow()
        {
            InitializeComponent();
        }

        public string GadgetTitle
        {
            get
            {
                return this.txtTitle.Text;
            }
            set
            {
                this.txtTitle.Text = value;
            }
        }

        public string GadgetDescription
        {
            get
            {
                return this.txtDesc.Text;
            }
            set
            {
                this.txtDesc.Text = value;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = null;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        //public double GadgetTitleFontSize
        //{
        //    get
        //    {
        //        return (double)cmbTitleSize.SelectedItem;
        //    }
        //    set
        //    {
        //        cmbTitleSize.SelectedItem = value.ToString();
        //    }
        //}
    }
}
