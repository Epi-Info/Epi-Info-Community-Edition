using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Epi;
using Epi.Fields;
using Epi.Data.Services;

namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    /// <summary>
    /// The Generic Field Definition Dialog
    /// </summary>
    public partial class GenericFieldDefinition : FieldDefinition
    {
        #region	Fields
        private string originalPromptText;
        private string originalFieldName;
        #endregion

        #region	Constructors
        /// <summary>
        /// Default Constructor for exclusive use by the designer
        /// </summary>
        public GenericFieldDefinition()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        public GenericFieldDefinition(MainForm frm)
            : base(frm)
        {
            InitializeComponent();
        }
        #endregion	Constructors

        #region	Protected Methods
        /// <summary>
        /// Validate user input on dialog
        /// </summary>
        protected override bool ValidateDialogInput()
        {
            bool isValid = true;
            if (this.ValidateInput())
            {
                ValidatePromptText();
                ValidateFieldName();

                if (ErrorMessages.Count > 0)
                {
                    isValid = false;
                    ShowErrorMessages();
                }
            }
            else
            {
                isValid = false;
                ShowErrorMessages();
            }
            return isValid;
        }

        #endregion	//Protected Methods

        #region Event Handlers

        /// <summary>
        /// Sets the Field Name
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args.</param>
        protected virtual void txtPrompt_Leave(object sender, System.EventArgs e)
        {
            if (!txtPrompt.Text.Equals(originalPromptText))
            {
                if (!string.IsNullOrEmpty(txtPrompt.Text) && (string.IsNullOrEmpty(txtFieldName.Text)))
                {
                    txtFieldName.Text = page.GetView().ComposeFieldNameFromPromptText(Util.Squeeze(txtPrompt.Text));
                    if (txtFieldName.Text.Length >= 64)
                    {
                        txtFieldName.Text = txtFieldName.Text.Substring(0, 64);
                    }
                    if (page.GetView().Fields.Exists(txtFieldName.Text))
                    {
                        MsgBox.ShowInformation(SharedStrings.DUPLICATE_FIELD_NAME);
                        txtFieldName.Text = "";
                        txtFieldName.Focus();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the change event for the Field Name textbox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        public virtual void txtFieldName_TextChanged(object sender, System.EventArgs e)
        {
            bool valid = false;
            ValidateFieldName();
            valid = ErrorMessages.Count > 0 ? false : true;
            ErrorMessages.Clear();

            toolTip.Active = valid ? false : true;
            btnOk.Enabled = valid ? true : false;

            if (valid)
            {
                txtFieldName.ForeColor = Color.Black; 
            }
            else
            {
                txtFieldName.ForeColor = Color.Red;
                toolTip.SetToolTip(this.txtFieldName, ErrorMessages.Count > 0 ? ErrorMessages[0] : string.Empty);
            }
        }

        /// <summary>
        /// Handles the leave event for the Field Name textbox.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtFieldName_Leave(object sender, EventArgs e)
        {
        }

        #endregion	//Event Handlers

        #region Private Methods
        /// <summary>
        /// Sets on the focus to the Prompt Text textbox
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void GenericFieldDefinition_Load(object sender, System.EventArgs e)
        {
            txtPrompt.Focus();
            originalFieldName = txtFieldName.Text;

            if (page != null && page.GetView() != null && page.GetView().GetProject() != null)
            {
                Project project = page.GetView().GetProject();
                string pageTableName = page.GetView().TableName + page.Id.ToString();
                bool hasDataTable = project.CollectedData.TableExists(pageTableName);
                bool fieldExists = false;
                
                if(string.IsNullOrEmpty(txtFieldName.Text) == false)
                {
                    fieldExists = project.CollectedData.ColumnExists(pageTableName, txtFieldName.Text);

                    if (this.Field is GridField)
                    {
                        string sourceTableName = ((GridField)this.Field).TableName;
                        fieldExists = project.CollectedData.TableExists(sourceTableName);
                    }                
                }
            }
            
            originalPromptText = txtPrompt.Text;
        }

        public bool FieldNameEnabled
        {
            get {return txtFieldName.Enabled;}
            set { txtFieldName.Enabled = value;}
        }

        /// <summary>
        /// Validates the prompt text on the dialog
        /// </summary>
        private void ValidatePromptText()
        {
            if (txtPrompt.Text.Trim().Length > 2047)
            {
                ErrorMessages.Add(SharedStrings.INVALID_PROMPT_TEXT);
            }
        }

        /// <summary>
        /// Validates the field name on the dialog
        /// </summary>
        private void ValidateFieldName()
        { 
            txtFieldName.Text = Util.Squeeze(txtFieldName.Text);

            if (txtFieldName.Text == originalFieldName && originalFieldName != string.Empty)
            {
                return;
            }
            
            if (string.IsNullOrEmpty(txtFieldName.Text))
            {
                ErrorMessages.Add(SharedStrings.ENTER_FIELD_NAME);
            }
            else if (!txtFieldName.ReadOnly)
            {
                string strTestForSymbols = txtFieldName.Text;
                Regex regex = new Regex("[\\w\\d]", RegexOptions.IgnoreCase); 
                string strResultOfSymbolTest = regex.Replace(strTestForSymbols, string.Empty);

                if (strResultOfSymbolTest.Length > 0)
                {
                    ErrorMessages.Add(string.Format(SharedStrings.INVALID_CHARS_IN_FIELD_NAME, strResultOfSymbolTest));
                }
                else
                {
                    if (txtFieldName.Text.Length > 64)
                    {
                        ErrorMessages.Add(SharedStrings.FIELD_NAME_TOO_LONG);
                    }
                    
                    if (!Util.IsFirstCharacterALetter(txtFieldName.Text))
                    {
                        ErrorMessages.Add(SharedStrings.FIELD_NAME_BEGIN_NUMERIC);
                    }

                    if (page.GetView().Fields.Exists(txtFieldName.Text) && originalFieldName != null)
                    {
                        ErrorMessages.Add(SharedStrings.DUPLICATE_FIELD_NAME);
                    }
                    
                    if (AppData.Instance.IsReservedWord(txtFieldName.Text))
                    {
                        ErrorMessages.Add(SharedStrings.FIELD_NAME_IS_RESERVED);
                    }
                }
            }
            
            if ((ErrorMessages.Count > 0) && (!(string.IsNullOrEmpty(txtFieldName.Text))))
            {
                txtFieldName.Focus();
            }
        }
        #endregion Private Methods

        private void txtPrompt_DoubleClick(object sender, EventArgs e)
        {
            if (!txtPrompt.Text.Equals(originalPromptText))
            {
                if (!string.IsNullOrEmpty(txtPrompt.Text))// && (string.IsNullOrEmpty(txtFieldName.Text)))
                {
                    txtFieldName.Text = page.GetView().ComposeFieldNameFromPromptText(Util.Squeeze(txtPrompt.SelectedText));
                }
            }
        }

        private void txtPrompt_Enter(object sender, EventArgs e)
        {
            if (this is FieldDefinition)
            {
                if (promptFont == null)
                {
                    txtPrompt.Font = controlFont;
                }
                else
                {
                    txtPrompt.Font = promptFont;
                }
            }
        }

        protected virtual void txtPrompt_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!txtPrompt.Text.Equals(originalPromptText) && (string.IsNullOrEmpty(txtFieldName.Text)))
                {
                    if (!string.IsNullOrEmpty(txtPrompt.Text) && (string.IsNullOrEmpty(txtFieldName.Text)))
                    {
                        txtFieldName.Text = page.GetView().ComposeFieldNameFromPromptText(Util.Squeeze(txtPrompt.Text));
                    }
                }
            }
        }

        protected virtual void txtPrompt_OnKeyDown(object sender, KeyEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control && (Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                switch (e.KeyCode)
                {
                    case Keys.G:
                        if (txtPrompt.Focused == true)
                        {
                            txtPrompt.Text += AddGarbage(); 
                        }
                        else if (txtFieldName.Focused == true)
                        {
                            txtFieldName.Text += AddGarbage();
                        }
                        break;
                }
            }       
        }

        /// <summary>
        /// Generates a random word to use for quickly filling in forms (developer testing only)
        /// TODO: Remove later
        /// </summary>
        /// <returns>Randomly-generated word from the string array</returns>
        private string AddGarbage()
        {
            string[] words = { "man", "the", "for", "and", "a", "with", "bird", "fox", "HAL9000", "tree", "is", "at", "she", "he", "has", "no", "yes", "butterfly", "rabbit", "pavement", "automobile", "dictionary", "output", "influenza", "spigot", "periwinkle", "osculate", "skullduggery", "moved", "ate", "late", "early", "time", "hepatitis", "name", "age", "DOB", "rival", "country", "city", "county", "help", "yell", "reward", "instant", "every", "cap", "sugar", "zero", "gremlin", "tan", "cold", "tape", "covered", "slime", "mud", "dirt", "rock", "sand", "grass", "concrete", "vaccinated", "vaccine", "tremble", "arm", "leg", "look", "looked", "ensorcell", "drizzle", "finagled", "drove", "salad" };

            Random rnd = new Random(Convert.ToInt32(DateTime.Now.Millisecond));
            int num = rnd.Next(words.Length);
            return words[num] + " ";
        }

        private void txtFieldName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                e.Handled = true;
            }
        }

        private void txtFieldName_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Space) e.IsInputKey = true;
        }
    }
}
