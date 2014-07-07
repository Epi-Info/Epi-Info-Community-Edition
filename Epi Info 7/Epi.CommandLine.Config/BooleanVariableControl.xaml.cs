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
    public partial class BooleanVariableControl : UserControl, IVariableControl
    {
        public event VariableDialogResultHandler VariableResultSet;

        public BooleanVariableControl()
        {
            InitializeComponent();
            btnYes.Click += new RoutedEventHandler(btnYes_Click);
            btnNo.Click += new RoutedEventHandler(btnNo_Click);
        }        

        public string Prompt
        {
            set
            {
                lblPrompt.Text = value;
            }
        }

        void btnNo_Click(object sender, RoutedEventArgs e)
        {
            if (VariableResultSet != null)
            {
                VariableResultSet(false.ToString());
            }
        }

        void btnYes_Click(object sender, RoutedEventArgs e)
        {
            if (VariableResultSet != null)
            {
                VariableResultSet(true.ToString());
            }
        }
        
    }
}
