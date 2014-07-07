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
    public partial class TextVariableControl : UserControl, IVariableControl
    {
        public event VariableDialogResultHandler VariableResultSet;

        public TextVariableControl()
        {
            InitializeComponent();
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            txtInput.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
        }

        void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnOk.IsEnabled = !string.IsNullOrEmpty(txtInput.Text);
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
                VariableResultSet(txtInput.Text);
            }
        }
        
    }
}
