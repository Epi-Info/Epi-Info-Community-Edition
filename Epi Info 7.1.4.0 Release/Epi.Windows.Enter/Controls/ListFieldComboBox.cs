#region Namespaces
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Epi.Data;
#endregion

namespace Epi.Windows.Enter.Controls
{
    /// <summary>
    /// The legal values list
    /// </summary>
	public class ListFieldComboBox : ComboBox
	{

        #region Constructors
        /// <summary>
		/// Constructor for the class
		/// </summary>
        public ListFieldComboBox()
            : base()
		{                   
            this.KeyPress += new KeyPressEventHandler(this.OnKeyPress);
		}

        protected override void Dispose(bool disposing)
        {
            this.KeyPress -= new KeyPressEventHandler(this.OnKeyPress);
            base.Dispose(disposing);
        }
		#endregion
        
        /// <summary>
        /// Handles the KeyPress event
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied key event parameters</param>
        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {            
            ListFieldComboBox legalValuesComboBox = sender as ListFieldComboBox;
            if (!e.KeyChar.Equals('\b'))
            {
                SearchValues(legalValuesComboBox, ref e);
            }
            else
            {
                e.Handled = false;
            }            
        }

        private void SearchValues(ListFieldComboBox legalValuesComboBox, ref KeyPressEventArgs e)
        {
            // Code below adapted from Raj Cool's AutoComplete combo box tutorial
            int selectionStart = legalValuesComboBox.SelectionStart;
            int selectionLength = legalValuesComboBox.SelectionLength; 
            int selectionEnd = selectionStart + selectionLength; 
            int index; 
            StringBuilder sb = new StringBuilder();
            // Create a new word with the user's last input key as the last char in the word
            sb.Append(legalValuesComboBox.Text.Substring(0, selectionStart)).Append(e.KeyChar.ToString()).Append(legalValuesComboBox.Text.Substring(selectionEnd));
            // Get the first item in the string that starts with our word, created in the string builder above
            index = legalValuesComboBox.FindString(sb.ToString());
            // if the index isn't found...
            if (index == -1)
            {
                e.Handled = false;
            }
            // otherwise, do the selection
            else
            {
                legalValuesComboBox.SelectedIndex = index;
                legalValuesComboBox.Select(selectionStart + 1, legalValuesComboBox.Text.Length - (selectionStart + 1));
                e.Handled = true;
            }
        }


        /// <summary>
        /// Assigns values to all linked fields based on a code table.
        /// </summary>
        /// <param name="sender">The control that contains the link references</param>
        public void FilterList(object sender, System.EventArgs e)
        {
            string TestValue = null;
            if(sender is ListFieldComboBox)
            {
                ListFieldComboBox lvcb = (ListFieldComboBox) sender;
                
                TestValue = lvcb.SelectedValue.ToString();
            }
            else
            {
                TestValue = ((Control)sender).Text;
            }

            this.DataSource = null;
            Epi.Fields.DDListField field = (Epi.Fields.DDListField) Epi.Windows.Enter.PresentationLogic.ControlFactory.Instance.GetAssociatedField(this);
            string SourceTable = field.SourceTable;
            string relateField = null;
            string displayfield = field.TextColumnName;

            foreach(System.Collections.Generic.KeyValuePair<string,int> kvp in field.pairAssociated)
            {
                relateField = kvp.Key;
                break;
            }

            System.Data.DataTable drv = DBReadExecute.GetDataTable(field.GetProject().FilePath, string.Format("Select * from {0} Where {1} Like '{2}%'", SourceTable, relateField, TestValue));
            this.Items.Clear();
            this.DataSource = drv;
            this.DisplayMember = displayfield;
            this.ValueMember = relateField;
            this.RefreshItems();

            
        }
    }
}
