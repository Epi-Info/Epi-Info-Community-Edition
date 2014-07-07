using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using Epi.Windows.Dialogs;
using Epi.Windows.MakeView.Forms;

namespace Epi.Windows.MakeView.Dialogs
{
	/// <summary>
	/// Dialog for setting background color and background image
	/// </summary>
    public partial class BackgroundDialog : DialogBase
    {
        #region Private Variables

        private string layout = String.Empty;
        private Color color = Color.Empty;
        private string imagePath = null;
        
        private Configuration config;
        private DataTable backgroundTable;
        private MakeViewMainForm mainForm;
        
        /// <summary>
        /// SettingsSaved
        /// </summary>
        public event EventHandler SettingsSaved;

        #endregion

        #region Public Constructors

        /// <summary>
		/// Default Constructor - Design mode only
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public BackgroundDialog()
		{
			InitializeComponent();
            config = Configuration.GetNewInstance();
		}

        /// <summary>
        /// Constructor for the class
        /// </summary>
        public BackgroundDialog(MainForm form)
            : base(form)
        {
            InitializeComponent();
            mainForm = form as MakeViewMainForm;
            InitSettings();
            cbImageLayout.SelectedIndexChanged += new EventHandler(cbImageLayout_SelectedIndexChanged);
            btnChangeColor.Click += new System.EventHandler(OnChangeColor);
            panelColor.Click += new EventHandler(OnChangeColor);
            btnChooseImage.Click += new EventHandler(OnChangeImage);
            pictureBoxImage.Click += new EventHandler(OnChangeImage);
            backgroundTable = mainForm.CurrentPage.GetMetadata().GetPageBackgroundData(mainForm.CurrentPage);

            if (backgroundTable.Rows.Count > 0)
            {
                layout = Convert.ToString(backgroundTable.Rows[0]["ImageLayout"]);

                if (backgroundTable.Rows[0]["Color"] != System.DBNull.Value)
                {
                    color = Color.FromArgb(Convert.ToInt32(backgroundTable.Rows[0]["Color"]));
                }
            }
        }

        #endregion

        #region Private Methods

        private void InitSettings()
        {
            config = Configuration.GetNewInstance();

            if (mainForm.CurrentBackgroundImage != null)
            {
                pictureBoxImage.Image = mainForm.CurrentBackgroundImage;
            }

            if (mainForm.CurrentBackgroundColor != Color.Empty)
            {
                panelColor.BackColor = mainForm.CurrentBackgroundColor;
            }
            else
            {
                panelColor.BackColor = SystemColors.Window;
            }

            if (!String.IsNullOrEmpty(mainForm.CurrentBackgroundImageLayout))
            {
                cbImageLayout.SelectedIndex = cbImageLayout.Items.IndexOf(mainForm.CurrentBackgroundImageLayout);
            }
            else
            {
                cbImageLayout.SelectedIndex = 0;
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

        private void OnChangeColor(object sender, EventArgs e)
        {
            colorDialog.Color = panelColor.BackColor;
            DialogResult result = colorDialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                panelColor.BackColor = colorDialog.Color;
                color = colorDialog.Color;
            }
        }

        private void cbImageLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbImageLayout.SelectedIndex > -1)
            {
                layout = cbImageLayout.Items[cbImageLayout.SelectedIndex].ToString();
            }
            else
            {
                layout = "None";
            }
        }

        private void OnChangeImage(object sender, EventArgs e)
        {
            openFileDialog.FileName = null;
            DialogResult result = openFileDialog.ShowDialog(this);

            if (result == DialogResult.OK && openFileDialog.FileName != null)
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(openFileDialog.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    pictureBoxImage.Image = Image.FromStream(fs);
                }
                imagePath = openFileDialog.FileName;
            }
            openFileDialog.Dispose();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Data.Services.IMetadataProvider metadata = mainForm.CurrentPage.GetMetadata();

            int imageId = 0;

            if (imagePath == null)
            {
                if (pictureBoxImage.Image != null)
                {
                    DataTable table = metadata.GetPageBackgroundData(mainForm.CurrentPage);
                    DataRow[] rows = table.Select("BackgroundId=" + mainForm.CurrentPage.BackgroundId);
                    imageId = (int)rows[0]["ImageId"];
                }
            }
            else
            { 
                imageId = metadata.InsertMetaImage(imagePath);
            }
            metadata.InsertPageBackgroundData(mainForm.CurrentPage, imageId, layout, color.ToArgb());
            mainForm.ApplyBackgroundToAllPages = rbApplyToAll.Checked;

            if (mainForm.ApplyBackgroundToAllPages && mainForm.CurrentPage.view != null)
            {
                foreach (Page page in mainForm.CurrentPage.view.Pages)
                {
                    page.BackgroundId = mainForm.CurrentPage.BackgroundId;

                    if (page != mainForm.CurrentPage)
                    {
                        metadata.InsertPageBackgroundData(page, imageId, layout, color.ToArgb());
                    }
                }
            }

            if (SettingsSaved != null)
            {
                SettingsSaved(this, new EventArgs());
            }
            this.Close();
        }

        private void btnClearImage_Click(object sender, EventArgs e)
        {
            pictureBoxImage.Image = null;
            imagePath = null;
            openFileDialog.FileName = String.Empty;
        }

        private void btnClearColor_Click(object sender, EventArgs e)
        {
            color = SystemColors.Window;
            panelColor.BackColor = color;
        }

        #endregion
    }
}

