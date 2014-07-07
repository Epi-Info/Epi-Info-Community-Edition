using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Fields;


namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    public partial class ImageFieldDefinition : GenericFieldDefinition
    {
        private ImageField field;

        #region	Constructors
        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public ImageFieldDefinition()
        {
            InitializeComponent();
        }

		/// <summary>
		/// Constructor for the class
		/// </summary>
        /// <param name="frm">The parent form</param>
		/// <param name="page">Page the field will belong to</param>
		public ImageFieldDefinition(MainForm frm, Page page) : base(frm)
		{
			InitializeComponent();
			this.mode = FormMode.Create;
			this.page = page;
		}

		/// <summary>
		/// Constructor for the class
		/// </summary>
        /// <param name="frm">The parent form</param>
		/// <param name="field">The fied to be edited</param>
		public ImageFieldDefinition(MainForm frm, ImageField field) : base(frm)
		{
			InitializeComponent();
			this.mode = FormMode.Edit;
			this.field = field;
			this.page = field.Page;
			LoadFormData();
		}
		#endregion	Constructors

        #region	Private Methods

        private void LoadFormData()
        {
            Configuration config = Configuration.GetNewInstance();
            FontStyle style = FontStyle.Regular;
            if (config.Settings.EditorFontBold)
            {
                style |= FontStyle.Bold;
            }
            if (config.Settings.EditorFontItalics)
            {
                style |= FontStyle.Italic;
            }
            if ((field.PromptFont == null) || ((field.PromptFont.Name == "Microsoft Sans Serif") && (field.PromptFont.Size == 8.5)))
            {
                field.PromptFont = new Font(config.Settings.EditorFontName, (float)config.Settings.EditorFontSize, style);
            }
            txtPrompt.Text = field.PromptText;
            txtFieldName.Text = field.Name;
            chkRetainSize.Checked = field.ShouldRetainImageSize;
            promptFont = field.PromptFont;
        }
        #endregion	//Private Methods

        #region Public Methods

        /// <summary>
        /// Sets the field's properties based on GUI values
        /// </summary>
        protected override void SetFieldProperties()
        {
            field.PromptText = txtPrompt.Text;
            field.Name = txtFieldName.Text;
            field.ShouldRetainImageSize = chkRetainSize.Checked;
            if (promptFont != null)
            {
                field.PromptFont = promptFont;
            }
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


        private void btnFont_Click(object sender, EventArgs e)
        {
            FontDialog dialog = new FontDialog();
            if (promptFont != null)
            {
                dialog.Font = promptFont;
            }
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                promptFont = dialog.Font;
                ((GenericFieldDefinition)this).Controls["txtPrompt"].Focus();  
            }
        }

        private void ImageFieldDefinition_Load(object sender, EventArgs e)
        {

        }
    }
}
