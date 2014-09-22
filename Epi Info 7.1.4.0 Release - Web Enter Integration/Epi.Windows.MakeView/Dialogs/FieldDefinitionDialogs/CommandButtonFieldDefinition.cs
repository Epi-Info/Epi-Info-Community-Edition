#region Namespaces
using System;
using System.Drawing;
using System.Windows.Forms;
using Epi;
using Epi.Fields;

#endregion	Namespaces

namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{

    public partial class CommandButtonFieldDefinition : GenericFieldDefinition
	{

		#region Fields
        /// <summary>
        /// The font button
        /// </summary>
		private CommandButtonField field;
		#endregion //Fields

		#region Constructors
        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public CommandButtonFieldDefinition()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for Command Button Field Definition 
        /// </summary>
        /// <param name="frm">The main form</param>
		public CommandButtonFieldDefinition(MainForm frm) : base(frm)
		{
			InitializeComponent();
		}

		/// <summary>
		/// Constructor for the class
		/// </summary>
        /// <param name="frm">The parent form</param>
		/// <param name="page">The current page</param>
		public CommandButtonFieldDefinition(MainForm frm, Page page) : base(frm)
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
		public CommandButtonFieldDefinition(MainForm frm, CommandButtonField field) : base(frm)
		{
			InitializeComponent();
			this.mode = FormMode.Edit;
			this.field = field;
			this.page = field.Page;
			LoadFormData();
		}
		#endregion //Constructors

		#region Event Handlers
		private void btnButtonFont_Click(object sender, System.EventArgs e)
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
		#endregion	//Event Handlers

		#region Private Methods
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
            controlFont = field.ControlFont;
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
            if (controlFont != null)
            {
                field.ControlFont = controlFont;
            }
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

				
	}
}
