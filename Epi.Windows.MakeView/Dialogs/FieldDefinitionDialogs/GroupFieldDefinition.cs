#region Namespaces
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Epi;
using Epi.Fields;

#endregion

namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    /// <summary>
    /// The Grour Field Definition dialog
    /// </summary>
    public partial class GroupFieldDefinition : GenericFieldDefinition
    {
		#region	Private Controls
		private GroupField field;
		#endregion

		#region	Constructors
        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public GroupFieldDefinition()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for Label Field Definition
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="page">Page to render label field</param>
		public GroupFieldDefinition(MainForm frm, Page page) : base(frm)
		{
			InitializeComponent();
			this.mode = FormMode.Create;
			this.page = page;
		}

        /// <summary>
        /// Constructor for Label Field Definition
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="field">The label field</param>
		public GroupFieldDefinition(MainForm frm, GroupField field) : base(frm)
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
            if ((field.ControlFont == null) || ((field.ControlFont.Name == "Microsoft Sans Serif") && (field.ControlFont.Size == 8.5)))
            {
                field.ControlFont = new Font(config.Settings.EditorFontName, (float)config.Settings.EditorFontSize, style);
            }
			
            txtPrompt.Text = field.PromptText;
			txtFieldName.Text = field.Name;

            promptFont = field.ControlFont;
            controlFont = field.ControlFont;
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

            if (promptFont != null)
            {
                field.PromptFont = promptFont;
                field.ControlFont = promptFont;
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

		#region Event Handlers
		private void btnPromptFont_Click(object sender, System.EventArgs e)
		{
			FontDialog dialog = new FontDialog();
			if (controlFont != null)
			{
				dialog.Font = controlFont;
			}
			
            DialogResult result = dialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				controlFont = dialog.Font;
                promptFont = dialog.Font;
                ((GenericFieldDefinition)this).Controls["txtPrompt"].Focus();  
			}
		}
        
        private void btnOk_Click(object sender, EventArgs e)
        {

        }
		#endregion			

        private void btnColor_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new ColorDialog();
            
            colorDialog.Color = field.BackgroundColor;
            
            DialogResult result = colorDialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                field.BackgroundColor = colorDialog.Color;
            }
        }
	}
}
