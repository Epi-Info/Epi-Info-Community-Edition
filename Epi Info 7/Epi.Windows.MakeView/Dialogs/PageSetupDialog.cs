using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Epi.Windows.Dialogs;
using Epi.Windows.MakeView.Forms;

namespace Epi.Windows.MakeView.Dialogs
{
	/// <summary>
	/// Dialog for setting background color and background image
	/// </summary>
    public partial class PageSetupDialog : DialogBase
    {
        private enum PaperSizeUnit { Inches, Millimeters, Pixels };
        
        #region Private Variables
        private string bgLayout=String.Empty;
        private Color bgColor=Color.Empty;
        private string bgImagePath=String.Empty;
        private Configuration config;
        private System.Collections.Generic.Dictionary<string, Size> _sizes;
        private MakeViewMainForm mainForm;
        private string _useAsDefaultPageSize = string.Empty;
        private string _widthText, _heightText;
        #endregion

        #region Public Variables
        public event EventHandler SettingsSaved;
        #endregion

        #region Public Constructors

        /// <summary>
		/// Default Constructor - Design mode only
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public PageSetupDialog()
		{
			InitializeComponent();
            config = Configuration.GetNewInstance();
		}

        /// <summary>
        /// Constructor for the class
        /// </summary>
        public PageSetupDialog(MainForm main) : base(main)
        {
            InitializeComponent();
            mainForm = main as MakeViewMainForm;
            
            _sizes = new System.Collections.Generic.Dictionary<String, Size>();
            string displayLabel;
            Size size;

            /// INCHES
            FormatSizeKeyValue("Letter (default)", PaperSizeUnit.Inches, 8.5, 11, out displayLabel, out size);
            _sizes.Add(displayLabel, size);
            FormatSizeKeyValue("Legal", PaperSizeUnit.Inches, 8.5, 14, out displayLabel, out size);
            _sizes.Add(displayLabel, size);
            FormatSizeKeyValue("Executive", PaperSizeUnit.Inches, 7.25, 10.5, out displayLabel, out size);
            _sizes.Add(displayLabel, size);

            /// MILLIMETERS
            FormatSizeKeyValue("A0", PaperSizeUnit.Millimeters, 841, 1189, out displayLabel, out size);
            _sizes.Add(displayLabel, size);
            FormatSizeKeyValue("A1", PaperSizeUnit.Millimeters, 594, 841, out displayLabel, out size);
            _sizes.Add(displayLabel, size);
            FormatSizeKeyValue("A2", PaperSizeUnit.Millimeters, 420, 594, out displayLabel, out size);
            _sizes.Add(displayLabel, size);
            FormatSizeKeyValue("A3", PaperSizeUnit.Millimeters, 297, 420, out displayLabel, out size);
            _sizes.Add(displayLabel, size);
            FormatSizeKeyValue("A4", PaperSizeUnit.Millimeters, 210, 297, out displayLabel, out size);
            _sizes.Add(displayLabel, size);
            FormatSizeKeyValue("A5", PaperSizeUnit.Millimeters, 148, 210, out displayLabel, out size);
            _sizes.Add(displayLabel, size);
            FormatSizeKeyValue("A6", PaperSizeUnit.Millimeters, 105, 148, out displayLabel, out size);
            _sizes.Add(displayLabel, size);

            /// ANDROID
            FormatSizeKeyValue("Android Galaxy Tab 2 7.0", PaperSizeUnit.Pixels, 341, 553, out displayLabel, out size);
            _sizes.Add(displayLabel, size);

            displayLabel = "Custom Size";
            _sizes.Add(displayLabel, new Size(780, 1016));
            _useAsDefaultPageSize = displayLabel;

            InitSettings();
        }

        private void FormatSizeKeyValue(string sizeName, PaperSizeUnit paperSizeUnit, double width, double height, out string displayLabel, out Size size)
        {
            double ratio = 94.55;
            string unitAsString = string.Empty;
            switch(paperSizeUnit)
            {
                case PaperSizeUnit.Inches: 
                    unitAsString = "in";
                    ratio = 94.55;
                    break;
                case PaperSizeUnit.Millimeters:
                    unitAsString = "mm";
                    ratio = ratio / 25.4;
                    break;
                case PaperSizeUnit.Pixels:
                    unitAsString = "px";
                    ratio = 1; 
                    break;
            }

            size = new Size((int)((width - 0.25) * ratio), (int)((height - 0.25) * ratio));
            displayLabel = string.Format("{0} ({1:#####.##}x{2:#####.##}{3})", sizeName, width, height, unitAsString);
        }

        #endregion

        #region Private Methods

        private void InitSettings()
        {
            config = Configuration.GetNewInstance();
            View view = mainForm.mediator.ProjectExplorer.SelectedPage.GetView();
            DataRow selectedPageSetupDataRow = mainForm.mediator.Project.Metadata.GetPageSetupData(view);
            string[] sizeNames = new string[_sizes.Count];
            _sizes.Keys.CopyTo(sizeNames, 0);
            comboBoxSize.Items.AddRange(sizeNames);

            if (((String)selectedPageSetupDataRow[ColumnNames.PAGE_ORIENTATION]).ToLowerInvariant() == "landscape")
            {
                radioButtonLandscape.Select();
            }
            else
            {
                radioButtonPortrait.Select();
            }

            if (((String)selectedPageSetupDataRow[ColumnNames.PAGE_LABEL_ALIGN]).ToLowerInvariant() == "horizontal")
            {
                radioButtonLabelAlignHorizontal.Select();
            }
            else
            {
                radioButtonLabelAlignVertical.Select();
            }

            int selectedPageWidth = (int)selectedPageSetupDataRow[ColumnNames.PAGE_WIDTH];
            int selectedPageHeight = (int)selectedPageSetupDataRow[ColumnNames.PAGE_HEIGHT];
            Size selectedPageSize = new Size(selectedPageWidth, selectedPageHeight);

            comboBoxSize.SelectedItem = _useAsDefaultPageSize;
            textBoxWidth.Text = selectedPageWidth.ToString();
            textBoxHeight.Text = selectedPageHeight.ToString();

            _widthText = textBoxWidth.Text;
            _heightText = textBoxHeight.Text;

            foreach (KeyValuePair<string, Size> kvp in _sizes)
            {
                if (((Size)kvp.Value).Equals(selectedPageSize))
                {
                    comboBoxSize.SelectedItem = (string)kvp.Key;
                    break;
                }
            }
        }
	
		/// <summary>
		/// Cancel button closes this dialog 
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

        private void btnOK_Click(object sender, EventArgs e)
        {
            int parsedInt;
            int.TryParse(textBoxWidth.Text, out parsedInt);
            mainForm.CurrentPage.view.PageWidth = parsedInt;

            int.TryParse(textBoxHeight.Text, out parsedInt);
            mainForm.CurrentPage.view.PageHeight = parsedInt;

            mainForm.CurrentPage.view.PageOrientation = radioButtonPortrait.Checked ? "Portrait" : "Landscape";
            mainForm.CurrentPage.view.PageLabelAlign = radioButtonLabelAlignVertical.Checked ? "Vertical" : "Horizontal";

            View view = mainForm.mediator.Project.Views[mainForm.CurrentPage.view.Name];
            view.PageWidth = mainForm.CurrentPage.view.PageWidth;
            view.PageHeight = mainForm.CurrentPage.view.PageHeight;
            view.PageOrientation = mainForm.CurrentPage.view.PageOrientation;
            view.PageLabelAlign = mainForm.CurrentPage.view.PageLabelAlign;

            mainForm.CurrentPage.GetMetadata().UpdatePageSetupData
                (
                    mainForm.CurrentPage.view,
                    mainForm.CurrentPage.view.PageWidth,
                    mainForm.CurrentPage.view.PageHeight,
                    mainForm.CurrentPage.view.PageOrientation,
                    mainForm.CurrentPage.view.PageLabelAlign,
                    "Paper"
                );
            
            if (SettingsSaved != null)
            {
                SettingsSaved(this, new EventArgs());
            }

            this.Close();
        }

        void textBoxWidth_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar > 31 && (e.KeyChar < '0' || e.KeyChar > '9'))
            {
                e.Handled = true;
            }
        }

        void textBoxHeight_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar > 31 && (e.KeyChar < '0' || e.KeyChar > '9'))
            {
                e.Handled = true;
            }
        }

        void textBoxWidth_TextChanged(object sender, System.EventArgs e)
        {
            int value = 0;

            if (Int32.TryParse(((TextBox)sender).Text, out value))
            {
                if (value > 4425)
                {
                    ((TextBox)sender).Text = _widthText;
                }
            }
        }

        void textBoxHeight_TextChanged(object sender, System.EventArgs e)
        {
            int value = 0;

            if (Int32.TryParse(((TextBox)sender).Text, out value))
            {
                if (value > 4425)
                {
                    ((TextBox)sender).Text = _heightText;
                }
            }
        }

        #endregion

        private void pictureBoxLabelAlignVertical_Click(object sender, EventArgs e)
        {
            this.radioButtonLabelAlignVertical.Select();
        }

        private void pictureBoxLabelAlignHorizontal_Click(object sender, EventArgs e)
        {
            this.radioButtonLabelAlignHorizontal.Select();
        }

        private void comboBoxSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            string key = ((ComboBox)sender).Text.Trim();

            if (key.ToLowerInvariant().Equals("custom size"))
            {
                this.textBoxHeight.Enabled = true;
                this.textBoxWidth.Enabled = true;

                this.textBoxHeight.ReadOnly = false;
                this.textBoxWidth.ReadOnly = false;
            }
            else if (_sizes.ContainsKey(key))
            {
                Size selectedSize = _sizes[key];
                textBoxWidth.Text = selectedSize.Width.ToString();
                textBoxHeight.Text = selectedSize.Height.ToString();

                this.textBoxHeight.Enabled = false;
                this.textBoxWidth.Enabled = false;

                this.textBoxHeight.ReadOnly = true;
                this.textBoxWidth.ReadOnly = true;
            }
        }
    }
}

