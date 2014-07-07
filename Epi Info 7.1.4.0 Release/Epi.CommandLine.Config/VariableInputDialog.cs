using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Epi;

namespace Epi.CommandLine.Config
{
    public partial class VariableInputDialog : Form
    {
        private System.Windows.UIElement control;
        private ElementHost host;
        private string result;

        public VariableInputDialog(DataType dataType, string prompt)
        {
            InitializeComponent();

            host = new ElementHost();
            host.Dock = DockStyle.Fill;
            switch (dataType)
            {
                case DataType.Text:
                    control = new TextVariableControl();
                    break;
                case DataType.Number:
                    control = new NumericVariableControl();
                    break;
                case DataType.DateTime:
                    control = new DateVariableControl();
                    break;
                case DataType.Boolean:
                    control = new BooleanVariableControl();
                    break;
                default:
                    control = new TextVariableControl();
                    break;
            }
            if (control != null)
            {
                host.Child = control;
                ((IVariableControl)control).VariableResultSet += new VariableDialogResultHandler(VariableInputDialog_VariableResultSet);
                ((IVariableControl)control).Prompt = prompt;
                this.Controls.Add(host);
            }
        }

        void VariableInputDialog_VariableResultSet(string result)
        {
            this.result = result;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        public string Result
        {
            get
            {
                return this.result;
            }
        }
    }
}
