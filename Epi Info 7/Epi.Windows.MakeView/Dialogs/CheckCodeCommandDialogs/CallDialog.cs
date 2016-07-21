#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi.Windows.Dialogs;
using Epi.Fields;
using System.Text.RegularExpressions;
using System.Collections.Generic;
#endregion  //Namespaces

namespace Epi.Windows.MakeView.Dialogs.CheckCodeCommandDialogs
{
	/// <summary>
	/// ***** This is a wireframe and currently contains no functionality *****
	/// </summary>
    public partial class CallDialog : CheckCodeDesignDialog
    {
        private RichTextBox txtTextArea;

        #region Constructors     
  
        


        /// <summary>
        /// Constructor for the class
        /// </summary>
        public CallDialog(Epi.Windows.MakeView.Forms.CheckCode frm):
            base(frm.mainForm)
        {
            InitializeComponent();
            this.txtTextArea = frm.codeText;
        }

        #endregion  //Constructors

        #region Private Event Handlers        
        
        /// <summary>
		/// Cancel button closes this dialog 
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

        /// <summary>
        /// Handled the index selection change of fields in listbox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void lbxFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = (lbxFields.SelectedItem != null);
        }

        /// <summary>
        /// Handles the Click event of the OK button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET suppied event parameters</param>
        private void btnOk_Click(object sender, System.EventArgs e)
        {
            Output = "CALL" + StringLiterals.SPACE;            
//          Output = "CLEAR ";
            
            for (int i = 0; i <= lbxFields.SelectedItems.Count - 1; i++)
            {
                Output += lbxFields.SelectedItems[i].ToString() + StringLiterals.SPACE;
            }

            Output = Output.Trim();
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/form-designer/Introduction.html");
        }

        #endregion  //Private Event Handlers

        #region Public Properties        
        
        /// <summary>
        /// Sets the View for the dialog
        /// </summary>
        public override View View
        {
            set 
            {
                this.AddSubroutinesToListBox();
            }
        }
        #endregion  //Public Properties

            private void AddSubroutinesToListBox()
            {
                System.Text.RegularExpressions.Regex re = null;

                re = new Regex("\\ssub\\s+[\\w_\\[\\]\\.]*\\b|^sub\\s+[\\w_\\[\\]\\.]*\\b", RegexOptions.IgnoreCase);
                Dictionary<string, string> IdList = new Dictionary<string, string>();
               
                MatchCollection MC = re.Matches(this.txtTextArea.Text.ToLowerInvariant());
                for(int i = 0; i < MC.Count; i++)
                {
                    int index2 = this.txtTextArea.Text.ToLowerInvariant().IndexOf(' ', MC[i].Index);
                    
                    if (index2 > -1)
                    {
                        int index3 = this.txtTextArea.Text.ToLowerInvariant().IndexOf('\n', index2);
                        if (index3 > -1)
                        {
                            string id = this.txtTextArea.Text.Substring(index2, index3 - index2).Trim();
                            if (!IdList.ContainsKey(id.ToUpperInvariant()))
                            {
                                lbxFields.Items.Add(id);
                            }
                        }
                    }
                }

        }
    }
}

