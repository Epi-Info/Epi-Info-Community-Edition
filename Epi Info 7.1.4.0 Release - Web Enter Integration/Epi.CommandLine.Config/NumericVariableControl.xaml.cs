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

namespace Epi.CommandLine.Config
{
    /// <summary>
    /// Interaction logic for TextVariableControl.xaml
    /// </summary>
    public partial class NumericVariableControl : UserControl, IVariableControl
    {
        public event VariableDialogResultHandler VariableResultSet;

        public NumericVariableControl()
        {
            InitializeComponent();
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            txtInput.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
        }

        void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtInput.Text))
            {
                btnOk.IsEnabled = false;
            }
            else
            {
                double val = 0;
                if (double.TryParse(txtInput.Text, out val))
                    btnOk.IsEnabled = true;
                else
                    btnOk.IsEnabled = false;
            }
        }

        public string Prompt
        {
            set
            {
                lblPrompt.Text = value;
            }
        }

        void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (VariableResultSet != null)
            {
                VariableResultSet(double.Parse(txtInput.Text).ToString());
            }
        }
        
    }
}
