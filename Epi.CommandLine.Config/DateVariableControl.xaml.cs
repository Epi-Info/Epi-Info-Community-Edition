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
    public partial class DateVariableControl : UserControl, IVariableControl
    {
        public event VariableDialogResultHandler VariableResultSet;

        public DateVariableControl()
        {
            InitializeComponent();
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            txtInput.SelectedDateChanged += new EventHandler<SelectionChangedEventArgs>(txtInput_SelectedDateChanged);
            txtInput.KeyUp += new KeyEventHandler(txtInput_KeyUp);
        }

        void txtInput_KeyUp(object sender, KeyEventArgs e)
        {
            ValidateText();
        }

        private void ValidateText()
        {
            if (string.IsNullOrEmpty(txtInput.Text))
            {
                btnOk.IsEnabled = false;
            }
            else
            {
                DateTime val = DateTime.Today;
                if (DateTime.TryParse(txtInput.Text, out val))
                    btnOk.IsEnabled = true;
                else
                    btnOk.IsEnabled = false;
            }
        }


        void txtInput_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateText();
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
                VariableResultSet(DateTime.Parse(txtInput.Text).ToString("u"));
            }
        }
        
    }
}
