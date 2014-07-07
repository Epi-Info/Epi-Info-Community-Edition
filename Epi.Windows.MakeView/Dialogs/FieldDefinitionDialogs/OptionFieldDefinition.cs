using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Fields;
using System.Collections;

namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    /// <summary>
    /// Field definition dialog for option fields
    /// </summary>
    public partial class OptionFieldDefinition : GenericFieldDefinition
    {
		#region Fields
		private OptionField field;
        private List<string> optionList = new List<string>();
        private int groupBottom_formBottom;
        private FontStyle promptStyle = FontStyle.Regular;
        private FontStyle optionStyle = FontStyle.Regular;

        #endregion //Fields

		#region Constructors
        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public OptionFieldDefinition()
        {
            InitializeComponent();
            ScrapeControlLayout();
        }
		/// <summary>
		/// Constructor for the class
		/// </summary>
        /// <param name="frm">The parent form</param>
		/// <param name="page">The current page</param>
		public OptionFieldDefinition(MainForm frm, Page page) : base(frm)
		{
            InitializeComponent();
            ScrapeControlLayout();
            this.mode = FormMode.Create;
			this.page = page;
		}

        private void ScrapeControlLayout()
        {
            groupBottom_formBottom = Size.Height - this.optionGroupBox.Bottom;
        }

		/// <summary>
		/// Constructor for the class
		/// </summary>
        /// <param name="frm">The parent form</param>
		/// <param name="field">The fied to be edited</param>
		public OptionFieldDefinition(MainForm frm, OptionField field) : base(frm)
		{
            InitializeComponent();
            ScrapeControlLayout();
            this.mode = FormMode.Edit;
			this.field = field;
			this.page = field.Page;
            
			LoadFormData();
		}
		#endregion Constructors

		#region Private Methods
		private void LoadFormData()
		{
            if (((OptionField)this.field).ShowTextOnRight)
            {
                this.textOnRight.Checked = true;
            }
            else
            {
                this.textOnLeft.Checked = true;
            }

            if (((OptionField)this.field).Pattern.Contains(Enums.OptionLayout.Horizontal.ToString()))
            {
                this.radioButtonHorizontal.Checked = true;
            }
            else
            {
                this.radioButtonVertical.Checked = true;
            }

            if (((OptionField)this.field).Pattern.Contains(Enums.OptionLayout.Right.ToString()))
            {
                this.radioButtonStartOnRight.Checked = true;
            }
            else
            {
                this.radioButtonStartOnLeft.Checked = true;
            }

            optionList.AddRange(((OptionField)this.field).Options);

            this.optionGroupBox.Controls.Clear();

            Configuration config = Configuration.GetNewInstance();

            txtPrompt.Text = field.PromptText;
            txtFieldName.Text = field.Name;

            // prompt font

            if (config.Settings.EditorFontBold)
            {
                promptStyle |= FontStyle.Bold;
            }
            if (config.Settings.EditorFontItalics)
            {
                promptStyle |= FontStyle.Italic;
            }
            
            if ((field.PromptFont == null) || ((field.PromptFont.Name == "Microsoft Sans Serif") && (field.PromptFont.Size == 8.5)))
            {
                field.PromptFont = new Font(config.Settings.EditorFontName, (float)config.Settings.EditorFontSize, promptStyle);
            }

            promptFont = field.PromptFont;
            
            // control font

            if (config.Settings.ControlFontBold)
            {
                optionStyle |= FontStyle.Bold;
            }
            if (config.Settings.ControlFontItalics)
            {
                optionStyle |= FontStyle.Italic;
            }

            if ((field.ControlFont == null) || ((field.ControlFont.Name == "Microsoft Sans Serif") && (field.ControlFont.Size == 8.5)))
            {
                field.ControlFont = new Font(config.Settings.ControlFontName, (float)config.Settings.ControlFontSize, optionStyle);
            }

            controlFont = field.ControlFont;

            if (field.Options.Count == 0)
            {
                this.btnOk.Enabled = false;
            }

            if (field.Options.Count < numUpDown.Minimum)
            {
                numUpDown.Value = numUpDown.Minimum;
            }
            else
            {
                numUpDown.Value = field.Options.Count;
            }

            LoadRadioButtons((int)numUpDown.Value);
		}

        private void LoadRadioButtons(int pTotal )
        {
            foreach (Control control in optionGroupBox.Controls)
            {
                if (control is TextBox)
                {
                    this.optionList.Add(control.Text);
                }
            }

            this.optionGroupBox.Controls.Clear();

            this.ResizeRedraw = true;

            int leftControlX = radioButtonForPositioning.Location.X;
            int leftControlY; 
            int rightControlX;
            int rightControlY;
            
            if (textOnRight.Checked)
            {
                // (O) Words
                leftControlY = radioButtonForPositioning.Location.Y;
                rightControlX = textBoxForPositioning.Location.X;
                rightControlY = textBoxForPositioning.Location.Y;
            }
            else
            {
                // Words (O)
                leftControlY = textBoxForPositioning.Location.Y;
                rightControlX = leftControlX + textBoxForPositioning.Width + 8;
                rightControlY = radioButtonForPositioning.Location.Y;
            }

            if (pTotal > 1)
            {
                for (int i = 0; i < pTotal; i++)
                {
                    RadioButton radioButton = new RadioButton();
                    TextBox textBox = new TextBox();

                    radioButton.Size = radioButtonForPositioning.Size;
                    textBox.Size = textBoxForPositioning.Size;

                    if (textOnRight.Checked)
                    {
                        // (O) Words
                        radioButton.Location = new Point(leftControlX, leftControlY);
                        textBox.Location = new Point(rightControlX, rightControlY);
                    }
                    else
                    {
                        // Words (O)
                        radioButton.Location = new Point(rightControlX, rightControlY);
                        textBox.Location = new Point(leftControlX, leftControlY);
                    }
                    
                    if (optionList.Count > 0 && optionList.Count > i)
                    {
                        textBox.Text = optionList[i];
                    }

                    textBox.TextChanged += new EventHandler(textBox_TextChanged);

                    this.optionGroupBox.Controls.Add(radioButton);
                    this.optionGroupBox.Controls.Add(textBox);

                    leftControlY += textBoxForPositioning.Height + 8;
                    rightControlY += textBoxForPositioning.Height + 8;

                    this.optionGroupBox.Height = textBox.Bottom + 12;
                }
            }
            
            Size = new Size(
                Size.Width,
                optionGroupBox.Bottom + groupBottom_formBottom);
            
            btnOk.Location = new Point
                (btnOk.Location.X,
                Size.Height - groupBottom_formBottom - btnOk.Height);

            btnCancel.Location = new Point
                (btnCancel.Location.X,
                Size.Height - groupBottom_formBottom - btnCancel.Height);

            optionList.Clear();
        }

		#endregion //Private Methods

		#region Public Methods

        /// <summary>
        /// Sets the field's properties based on GUI values
        /// </summary>
        protected override void SetFieldProperties()
        {
            field.PromptText = txtPrompt.Text;
            field.Name = txtFieldName.Text;

            field.PromptFont = promptFont;
            field.ControlFont = controlFont;
        }

		/// <summary>
		/// Gets the field defined by this field definition dialog
		/// </summary>
		public override RenderableField Field
		{
			get
			{
				return field;
			}
		}
		#endregion	//Public Methods

        private void btnOk_Click(object sender, EventArgs e)
        {
            foreach (Control control in optionGroupBox.Controls)
            {
                if (control is TextBox)
                {
                    this.optionList.Add(control.Text);
                }
            }

            ((OptionField)field).Options = optionList;
            ((OptionField)field).ShowTextOnRight = textOnRight.Checked;
            ((OptionField)field).PromptFont = promptFont;
            ((OptionField)field).ControlFont = controlFont;

            string vertHorz = Enums.OptionLayout.Vertical.ToString();
            if (radioButtonHorizontal.Checked)
            {
                vertHorz = Enums.OptionLayout.Horizontal.ToString();
            }
            
            string leftRight = Enums.OptionLayout.Left.ToString();
            if (radioButtonStartOnRight.Checked)
            {
                leftRight = Enums.OptionLayout.Right.ToString();
            }

            ((OptionField)field).Pattern = string.Format("{0},{1}",vertHorz, leftRight);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (this.optionGroupBox.Controls.Count > 0)
            {
                this.optionGroupBox.Controls.Clear();
            }

            if (this.optionList.Count > 0)
            {
                this.optionList.Clear();
            }
        }

        private void numUpDown_ValueChanged(object sender, EventArgs e)
        {
            LoadRadioButtons((int)numUpDown.Value);
            EnableDisableButtons();
        }

        private void textOnRight_CheckedChanged(object sender, EventArgs e)
        {
            LoadRadioButtons((int)numUpDown.Value);
        }

        private void textOnLeft_CheckedChanged(object sender, EventArgs e)
        {
            LoadRadioButtons((int)numUpDown.Value);
        }

        void textBox_TextChanged(object sender, EventArgs e)
        {
            int selIndex = ((TextBox)sender).SelectionStart;
            ((TextBox)sender).Text = ((TextBox)sender).Text.Replace(",", "");
            ((TextBox)sender).SelectionStart = selIndex;
            
            EnableDisableButtons();
        }

        private void EnableDisableButtons()
        {
            bool enableOK = true;

            this.txtPrompt.TextChanged -= new System.EventHandler(this.txtPrompt_TextChanged);
            txtPrompt.Text = txtPrompt.Text.TrimStart();
            this.txtPrompt.TextChanged += new System.EventHandler(this.txtPrompt_TextChanged);

            this.txtFieldName.TextChanged -= new System.EventHandler(this.txtFieldName_TextChanged);
            txtFieldName.Text = txtFieldName.Text.TrimStart();
            this.txtFieldName.TextChanged += new System.EventHandler(this.txtFieldName_TextChanged);
            
            foreach (Control control in this.optionGroupBox.Controls)
            {
                if (control is TextBox)
                {
                    control.TextChanged -= new EventHandler(textBox_TextChanged);
                    control.Text = control.Text.TrimStart();
                    control.TextChanged += new EventHandler(textBox_TextChanged);
                    
                    if (control.Text == string.Empty) enableOK = false; 
                }
            }

            if (txtPrompt.Text == string.Empty) enableOK = false;
            if (txtFieldName.Text == string.Empty) enableOK = false; 

            btnOk.Enabled = enableOK;
        }

        private void txtPrompt_TextChanged(object sender, EventArgs e)
        {
            EnableDisableButtons();
        }

        private void txtFieldName_TextChanged(object sender, EventArgs e)
        {
            EnableDisableButtons();
        }
    }
}
    

