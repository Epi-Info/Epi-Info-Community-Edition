using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
namespace Epi.Windows.MakeView.Dialogs
{
    /// <summary>
    /// InputDialog Class
    /// </summary>
    public partial class InputGuidDialog : Form
    {
        private static Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);

        /// <summary>
        /// Input - object containing the user's input value.
        /// </summary>
        public object Input { get ; set; }

        public string TextInput { get { return this.txtTextInput.Text; } }
        /// <summary>
        /// InputControlLocation - the starting location of the first control.
        /// </summary>
        private Point InputControlLocation = new Point(12, 34);
        /// <summary>
        /// YesNo - is used to persist a state for the Yes and No buttons.
        /// </summary>
        private bool YesNo { get; set; }

        private EpiInfo.Plugin.DataType _dataType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">Text (aka Prompt) is shown on the form above the controls.</param>
        /// <param name="caption">Caption (aka Title) is shown on the title bar.</param>
        /// <param name="mask">Mask is used with masked controls.</param>
        /// <param name="input">Input is used to get the type of the object that will be displayed for user input.</param>
        public InputGuidDialog(string text, string caption, string mask, object input, EpiInfo.Plugin.DataType dataType = EpiInfo.Plugin.DataType.Unknown)
        {
            Input = input;
            _dataType = dataType;

            InitializeComponent();

            if (caption.Length > 75)
            {
                caption = caption.Substring(0, 75) + "...";
            }
            
            this.gbxMainGroup.Text = caption;
            this.lblPrompt.Text = text;

            if (_dataType == EpiInfo.Plugin.DataType.Unknown)
            {
                if (input is System.String)
                {
                    _dataType = EpiInfo.Plugin.DataType.Text;
                }
                else if (input is System.Double)
                {
                    _dataType = EpiInfo.Plugin.DataType.Number;
                }
                else if (input is DateTime)
                {
                    _dataType = EpiInfo.Plugin.DataType.DateTime;
                }
                else if (input is Boolean)
                {
                    _dataType = EpiInfo.Plugin.DataType.Boolean;
                }
            }

            Control control = null;

            if (_dataType == EpiInfo.Plugin.DataType.Text)
            {
                Int32 maxLength;
                if (string.IsNullOrEmpty(mask))
                {
                    control = this.txtTextInput;
                }
                else if (Int32.TryParse(mask, out maxLength))
                {
                    this.txtTextInput.MaxLength = maxLength;
                    control = this.txtTextInput;
                }
                else
                {
                    this.txtMaskedTextInput.Mask = mask;
                    control = this.txtMaskedTextInput;
                }
            }
            else if (_dataType == EpiInfo.Plugin.DataType.Number)
            {
                if (string.IsNullOrEmpty(mask))
                {
                    this.txtTextInput.TextChanged += new EventHandler(textBoxInput_TextChanged);
                    control = this.txtTextInput;
                }
                else
                {
                    this.txtMaskedTextInput.Mask = mask;
                    control = this.txtMaskedTextInput;
                }
            }
            else if (_dataType == EpiInfo.Plugin.DataType.Date || _dataType == EpiInfo.Plugin.DataType.Time || _dataType == EpiInfo.Plugin.DataType.DateTime)
            {
                this.txtTextInput.TextChanged += new EventHandler(textBoxInput_TextChanged);
                control = this.txtTextInput;
                control.Text = Watermark(_dataType);
            }
            else if (_dataType == EpiInfo.Plugin.DataType.Boolean)
            {
                control = this.btnYes;
            }
            else if (input is List<string>)
            {
                this.lbxInput.Items.AddRange(((List<string>)input).ToArray());
                control = this.lbxInput;
            }

            control.Parent = gbxMainGroup;
            control.Visible = true;
            int controlGap = 14;
            Point starting = new Point(20, gbxMainGroup.Location.Y + 12);
            control.Select();
            
            lblPrompt.Location = new Point(starting.X, starting.Y);
            control.Location = new Point(starting.X, starting.Y + lblPrompt.Height + controlGap - 4);
        
            gbxMainGroup.Size = new Size(gbxMainGroup.Size.Width, control.Location.Y + control.Size.Height + controlGap);
            btnOK.Location = new Point(btnOK.Location.X, gbxMainGroup.Location.Y + gbxMainGroup.Size.Height + controlGap);
            btnCancel.Location = new Point(btnCancel.Location.X, gbxMainGroup.Location.Y + gbxMainGroup.Size.Height + controlGap);
            this.Size = new Size(this.Size.Width, btnOK.Location.Y + btnOK.Size.Height + 48);

            if (_dataType == EpiInfo.Plugin.DataType.Boolean)
            {
                InitYesNoButtons();
            }
        }
        
        string Watermark(EpiInfo.Plugin.DataType dataType)
        {
            System.Globalization.DateTimeFormatInfo formatInfo = System.Globalization.DateTimeFormatInfo.CurrentInfo;
            string watermark = string.Empty;

            if (dataType == EpiInfo.Plugin.DataType.Date)
            {
                watermark = string.Format("{0}", formatInfo.ShortDatePattern.ToUpper());
            }
            else if (dataType == EpiInfo.Plugin.DataType.Time)
            {
                watermark = string.Format("{0}", formatInfo.LongTimePattern.ToUpper());
            }
            else
            {
                watermark = string.Format("{0} {1}", formatInfo.ShortDatePattern.ToUpper(), formatInfo.LongTimePattern.ToUpper());
            }

            return watermark;
        }

        void textBoxInput_TextChanged(object sender, EventArgs e)
        {
            if (Input is DateTime)
            {
                string watermark = Watermark(_dataType);

                if (((Control)sender).Text == "")
                {
                    ((Control)sender).Text = watermark;
                    ((TextBoxBase)sender).SelectionStart = 0;
                    ((TextBoxBase)sender).SelectionLength = 0;
                }
                else if (((Control)sender).Text.Contains(watermark) && ((Control)sender).Text != watermark)
                {
                    string displayText = ((Control)sender).Text.Replace(watermark, "");
                    ((Control)sender).Text = displayText;
                    ((TextBoxBase)sender).SelectionStart = 1;
                    ((TextBoxBase)sender).SelectionLength = 0;
                }

                if (((Control)sender).Text == watermark)
                {
                    ((Control)sender).ForeColor = System.Drawing.Color.Gray;
                }
                else
                {
                    ((Control)sender).ForeColor = System.Drawing.Color.Black;
                }

                DateTime dateTime;
                btnOK.Enabled = DateTime.TryParse(this.txtTextInput.Text, out dateTime);
            }
            else
            {
                Double num;
                btnOK.Enabled = double.TryParse(this.txtTextInput.Text, out num);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            if (Input is String)
            {
                if (!string.IsNullOrEmpty(txtMaskedTextInput.Text) && txtMaskedTextInput.Text.Length > 0)
                {
                    Input = txtMaskedTextInput.Text;
                }
                else
                {
                    Input = txtTextInput.Text;
                }
            }
            else if (Input is System.Double)
            {
                Double input = new Double();
                
                if (!string.IsNullOrEmpty(txtMaskedTextInput.Mask))
                {
                    Double.TryParse(this.txtMaskedTextInput.Text, out input);
                }
                else
                {
                    Double.TryParse(this.txtTextInput.Text, out input);
                }
                
                Input = input;
            }
            else if (Input is DateTime)
            {
                DateTime dateTime;
                DateTime.TryParse(this.txtTextInput.Text, out dateTime);
                Input = dateTime;
            }
            else if (Input is Boolean)
            {
                Input = this.YesNo;
            }
            else if (Input is List<string>)
            {
                Input = this.lbxInput.SelectedItem;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void InitYesNoButtons()
        {
            this.btnNo.Location = new Point(btnYes.Location.X + btnYes.Width + 8, btnYes.Location.Y);
            this.btnNo.Visible = true;
            this.btnOK.Visible = false;
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            YesNo = true;
            btnOK_Click(sender, e);
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            YesNo = false;
            btnOK_Click(sender, e);
        }

        public static bool IsGuid(string Guid)
        {
            bool isValid = false;


            if (Guid != null)
            {

                if (isGuid.IsMatch(Guid))
                {

                    isValid = true;
                }
            }

            return isValid;
        }
         
        private void txtTextInput_TextChanged(object sender, EventArgs e)
        {
            if (IsGuid(this.txtTextInput.Text.ToString()))
            {
                this.btnOK.Enabled = true;
            }
       }
    }
}
