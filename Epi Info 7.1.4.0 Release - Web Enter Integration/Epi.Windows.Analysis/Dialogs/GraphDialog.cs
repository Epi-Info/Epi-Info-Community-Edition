using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi;
using Epi.Windows;
using Epi.Analysis;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for List command
	/// </summary>
    public partial class GraphDialog : CommandDesignDialog
	{
		List<ComboBox> _variableSelectComboBoxes;
        
        #region Constructor

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>

        /// <summary>
        /// Constructor for the List dialog
        /// </summary>
        /// <param name="frm"></param>
        public GraphDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            Construct();
        }

        private void Construct()
        {
            if (!this.DesignMode)           // designer throws an error
            {
                btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
                btnOK.Click += new System.EventHandler(this.btnOK_Click);

                _variableSelectComboBoxes = new List<ComboBox>();
                _variableSelectComboBoxes.Add(comboBoxMainVariable);
                _variableSelectComboBoxes.Add(comboBoxWeightVar);
                _variableSelectComboBoxes.Add(comboBoxStrataVar);
                _variableSelectComboBoxes.Add(comboBoxBarOfEachValueOf);

                foreach(ComboBox varComboBox in _variableSelectComboBoxes)
                {
                    varComboBox.Validated += new EventHandler(cmbVar_Validated);
                    varComboBox.Validating += new CancelEventHandler(cmbVar_Validating);
                    varComboBox.TextChanged += new EventHandler(cmbVar_TextChanged);
                }
            }
        }

		#endregion Constructors

		#region Event Handlers
		/// <summary>
		/// Handles click event of Clear button
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnClear_Click(object sender, System.EventArgs e)
		{
			listBoxVariables.Items.Clear();
			comboBoxMainVariable.SelectedIndex = 0;
            comboBoxShowValueOf.SelectedIndex = 0;
            comboBoxWeightVar.SelectedIndex = 0;
            comboBoxStrataVar.SelectedIndex = 0;
            comboBoxBarOfEachValueOf.SelectedIndex = 0;
		}

        /// <summary>
		/// Handles the load event of List dialog
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void ListDialog_Load(object sender, System.EventArgs e)
		{
			LoadVariables();
		}
		/// <summary>
		/// Handles the SelectedIndexChanged event
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmbVar_SelectedIndexChanged(object sender, System.EventArgs e)
		{
            this.Validate();
		}

        private void SelectedIndexChangedInVariableSelectControl(object sender, System.EventArgs e)
        {
            if (((ComboBox)sender).SelectedItem == string.Empty) return;
            
            foreach (ComboBox comboBox in _variableSelectComboBoxes)
            {
                if (comboBox != sender)
                {
                    comboBox.Items.Remove(((ComboBox)sender).SelectedItem);
                }
            }
        }

        private void VariableSelectComboBoxesTextChanged(object sender, System.EventArgs e)
        {
        }

        private void lbxVariables_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (listBoxVariables.SelectedIndex != -1)
            {
                comboBoxMainVariable.Items.Add(listBoxVariables.Items[listBoxVariables.SelectedIndex].ToString());
                listBoxVariables.Items.RemoveAt(listBoxVariables.SelectedIndex);
                if (listBoxVariables.Items.Count < 1 && comboBoxMainVariable.Text!=StringLiterals.STAR)
                {
                    btnOK.Enabled = false;
                    btnSaveOnly.Enabled = false;
                }
                else if (listBoxVariables.Items.Count < 1 && comboBoxMainVariable.Text == StringLiterals.STAR)
                {
                    btnOK.Enabled = true;
                    btnSaveOnly.Enabled = true;
                }
            }
        }

        private void cmbVar_Validated(object sender, EventArgs e)
        {
            if (comboBoxMainVariable.SelectedItem != null && comboBoxMainVariable.Text != StringLiterals.STAR)
            {
                if (false == listBoxVariables.Items.Contains(comboBoxMainVariable.SelectedItem) && !string.IsNullOrEmpty(comboBoxMainVariable.SelectedItem.ToString()))
                {
                    if (comboBoxGraphType.Text == "Pie")
                    {
                        listBoxVariables.Items.Clear();
                    }
                    
                    listBoxVariables.Items.Add(comboBoxMainVariable.SelectedItem);
                }

                if (listBoxVariables.Items.Count > 0)
                {
                    btnOK.Enabled = true;
                    btnSaveOnly.Enabled = true;
                }
            }
        }

        private void cmbVar_Validating(object sender, CancelEventArgs e)
        {
            if (comboBoxMainVariable.SelectedItem == null)
            {
                if (!String.IsNullOrEmpty(comboBoxMainVariable.Text))
                {
                    e.Cancel = true;
                    comboBoxMainVariable.Text = String.Empty;                    
                }
            }
        }

        private void cmbVar_TextChanged(object sender, EventArgs e)
        {
            if (comboBoxMainVariable.Text == StringLiterals.STAR)
            {
                btnOK.Enabled = true;
                btnSaveOnly.Enabled = true;
            }
            else if (comboBoxMainVariable.Text != StringLiterals.STAR && listBoxVariables.Items.Count < 1)
            {
                btnOK.Enabled = false;
                btnSaveOnly.Enabled = false;
            }
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-graph.html");
        }

        #endregion //Event Handlers

		#region Protected Methods
		/// <summary>
		/// Generates the command text
		/// </summary>
		protected override void GenerateCommand()
		{
			WordBuilder command = new WordBuilder();
			command.Append(CommandNames.GRAPH);
			
            if (listBoxVariables.Items.Count > 0)
			{
				foreach (string item in listBoxVariables.Items)
				{
					command.Append(FieldNameNeedsBrackets(item) ? Util.InsertInSquareBrackets(item) : item);
				}
			}
			else
			{
                command.Append(FieldNameNeedsBrackets(comboBoxMainVariable.Text) ? Util.InsertInSquareBrackets(comboBoxMainVariable.Text) : comboBoxMainVariable.Text);
			}

            if (comboBoxBarOfEachValueOf.Text != string.Empty)
            {
                command.Append(string.Format("* {0}", FieldNameNeedsBrackets(comboBoxBarOfEachValueOf.Text) ? Util.InsertInSquareBrackets(comboBoxBarOfEachValueOf.Text) : comboBoxBarOfEachValueOf.Text));
            }

            if (comboBoxGraphType.Text != string.Empty)
            {
                if (comboBoxGraphType.Text == "EAR (Early Aberration Reporting)")
                {
                    command.Append("GRAPHTYPE=\"EAR\"");
                }
                else
                {
                    command.Append(string.Format("GRAPHTYPE=\"{0}\"", comboBoxGraphType.Text));
                }
            }

            if (comboBoxStrataVar.Text != string.Empty)
            {
                command.Append(string.Format("STRATAVAR={0}", FieldNameNeedsBrackets(comboBoxStrataVar.Text) ? Util.InsertInSquareBrackets(comboBoxStrataVar.Text) : comboBoxStrataVar.Text));
            }

            if (comboBoxWeightVar.Text != string.Empty)
            {
                string weightVar = FieldNameNeedsBrackets(comboBoxWeightVar.Text) ? Util.InsertInSquareBrackets(comboBoxWeightVar.Text) : comboBoxWeightVar.Text;
                switch (comboBoxShowValueOf.Text)
                {
                    case "Average":
                        weightVar = string.Format("AVG({0})", weightVar);
                        break;
                    case "Count":
                        weightVar = string.Format("COUNT({0})", weightVar);
                        break;
                    case "Sum":
                        weightVar = string.Format("SUM({0})", weightVar);
                        break;
                    case "Minimum":
                        weightVar = string.Format("MIN({0})", weightVar);
                        break;
                    case "Maximum":
                        weightVar = string.Format("MAX({0})", weightVar);
                        break;
                    case "Count %":
                        weightVar = string.Format("PERCENT({0})", weightVar);
                        break;
                    case "Sum %":
                        weightVar = string.Format("SUMPCT({0})", weightVar);
                        break;
                }
                command.Append(string.Format("WEIGHTVAR={0}", weightVar));
            }
            else
            {
                if (comboBoxShowValueOf.Text == "Count %")
                {
                    command.Append("WEIGHTVAR=PERCENT()");
                }
            }

            if (textBoxTitle.Text != string.Empty)
            {
                command.Append(string.Format("TITLETEXT=\"{0}\"", textBoxTitle.Text));
            } 
            
            if (textBoxXAxisLabel.Text != string.Empty)
            {
                command.Append(string.Format("XTITLE=\"{0}\"", textBoxXAxisLabel.Text));
            } 
            
            if (textBoxYAxisLabel.Text != string.Empty)
            {
                command.Append(string.Format("YTITLE=\"{0}\"", textBoxYAxisLabel.Text));
            }

            if (comboBoxDateFormat.Text != string.Empty)
            {
                command.Append(string.Format("DATEFORMAT=\"{0}\"", comboBoxDateFormat.Text));
            }

            if (comboBoxIntervalType.Text != string.Empty)
            {
                command.Append(string.Format("INTERVAL=\"{0} {1}\"", textBoxInterval.Text, comboBoxIntervalType.Text));
            }

			CommandText = command.ToString();
		}
		#endregion //Protected Methods

		#region Private Methods
		private void LoadVariables()
		{
            VariableType scopeWord = 
                VariableType.DataSource | 
                VariableType.DataSourceRedefined | 
                VariableType.Standard;
           
            FillVariableCombo(comboBoxMainVariable, scopeWord);
            comboBoxMainVariable.Items.Insert(0, "");
            comboBoxMainVariable.SelectedIndex = 0;

            FillVariableCombo(comboBoxStrataVar, scopeWord);
            comboBoxStrataVar.Items.Insert(0, "");
            comboBoxStrataVar.SelectedIndex = 0;

            FillVariableCombo(comboBoxWeightVar, scopeWord);
            comboBoxWeightVar.Items.Insert(0, "");
            comboBoxWeightVar.SelectedIndex = 0;

            FillVariableCombo(comboBoxBarOfEachValueOf, scopeWord);
            comboBoxBarOfEachValueOf.Items.Insert(0, "");
            comboBoxBarOfEachValueOf.SelectedIndex = 0;
            
            comboBoxMainVariable.SelectedIndexChanged += new System.EventHandler(this.cmbVar_SelectedIndexChanged); 

            foreach (ComboBox varComboBox in _variableSelectComboBoxes)
            {
                varComboBox.SelectedIndexChanged += new EventHandler(SelectedIndexChangedInVariableSelectControl);
                varComboBox.SelectedIndexChanged += new EventHandler(SelectedIndexChangedInVariableSelectControl);
                varComboBox.SelectedIndexChanged += new EventHandler(SelectedIndexChangedInVariableSelectControl);
                varComboBox.SelectedIndexChanged += new EventHandler(SelectedIndexChangedInVariableSelectControl);
            }
        }
		#endregion Private Methods

        private void comboBoxGraphType_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool enableIntervalFields = false;
            if ((sender as ComboBox).Text == "Epi Curve")
            {
                enableIntervalFields = true;
            }

            if ((sender as ComboBox).Text == "Pie")
            {
                listBoxVariables.Items.Clear();
                comboBoxBarOfEachValueOf.Enabled = false;
                comboBoxBarOfEachValueOf.Text = string.Empty;
                comboBoxBarOfEachValueOf.SelectedIndex = -1;
            }
            else
            {
                comboBoxBarOfEachValueOf.Enabled = true;
            }

            if (((System.Windows.Forms.ComboBox)(sender)).Text.ToLower() == "scatter")
            {
                FillVariableCombo(comboBoxMainVariable);
                comboBoxMainVariable.Items.Insert(0, "");
                comboBoxMainVariable.SelectedIndex = 0;
            }
            else 
            {
                VariableType scopeWord =VariableType.DataSource | VariableType.DataSourceRedefined | VariableType.Standard;
                FillVariableCombo(comboBoxMainVariable, scopeWord);
                comboBoxMainVariable.Items.Insert(0, "");
                comboBoxMainVariable.SelectedIndex = 0;
            
            }
            comboBoxIntervalType.Enabled = enableIntervalFields;
            textBoxInterval.Enabled = enableIntervalFields;
            textBoxFirstValue.Enabled = enableIntervalFields;
        }
	}
}
