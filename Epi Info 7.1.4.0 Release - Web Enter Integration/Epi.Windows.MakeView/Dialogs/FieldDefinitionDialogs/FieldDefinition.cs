using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Epi;
using Epi.Fields;
using Epi.Windows.Dialogs;


namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    /// <summary>
    /// The Field Definition dialog
    /// </summary>
    public partial class FieldDefinition : DialogBase 
	{
		#region Fields

        /// <summary>
        /// The x-coordinate
        /// </summary>
		protected int xCoordinate;

        /// <summary>
        /// The y- coordinate
        /// </summary>
		protected int yCoordinate;

        /// <summary>
        /// The form mode
        /// <remarks> dpb - this propery is never used. remove when we have time </remarks>
        /// </summary>
        //[Obsolete("FormMode mode property is deprecated", false)]
		protected FormMode mode;
        
        /// <summary>
        /// The page
        /// </summary>
		protected Page page;
		
        /// <summary>
        /// The prompt's font
        /// </summary>
        protected Font promptFont;

        /// <summary>
        /// The control's font
        /// </summary>
		protected Font controlFont;

		#endregion Fields
		
		#region	Constructors
        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public FieldDefinition()
        {
            InitializeComponent();
        }
		/// <summary>
		/// Constructor for the class
		/// </summary>
        public FieldDefinition(MainForm frm) : base(frm)
		{
			InitializeComponent();
		}
		#endregion Constructors

		#region	Protected Methods

		/// <summary>
		/// Validate dialog input
		/// </summary>
		/// <returns>True</returns>
		protected virtual bool ValidateDialogInput()
		{
			//Code logic in Generic Field Definition form
			return true;
		}

        /// <summary>
        /// Sets the field's properties based on GUI values
        /// </summary>
        protected virtual void SetFieldProperties()
        {
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="field"></param>
        ///// <param name="newFieldName"></param>
        ///// <returns></returns>
        //protected bool TrySetFieldName(Field field, string newFieldName)
        //{
        //    // << if it's a new field , just go ahead and set it >>
        //    if (field.Id == 0)
        //    {
        //        field.Name = newFieldName;
        //        return true;
        //    }

        //    // << if the field name is already set to the new field name, return true >>
        //    if (field.Name == newFieldName) return true;

        //    // << check to see if name already exists >>
            
        //    if (field.Id > 0 && (field.Name != newFieldName))
        //    {
        //        MessageBox.Show(SharedStrings.FIELD_NAME_IS_RESERVED);
        //    }
        //    else
        //    {
        //    }
            
        //    return true;
        //}

		#endregion	//Protected Methods

		#region	Public Methods
		/// <summary>
		/// Creates a field in the MakeView designer
		/// </summary>
		/// <param name="xLocation">X coordinate of the control</param>
		/// <param name="yLocation">Y coordinate of the control</param>
		public void CreateField(int xLocation, int yLocation)
		{
			this.xCoordinate = xLocation;
			this.yCoordinate = yLocation;
			this.mode = FormMode.Create;
		}

		/// <summary>
		/// Gets the field defined by this field definition dialog
		/// </summary>
		public virtual RenderableField Field
		{
			get
			{
				return null;
			}
		}
		#endregion	//Public Methods

		#region Event Handlers

		protected virtual void btnOk_Click(object sender, System.EventArgs e)
		{
			if (ValidateDialogInput())
			{
                SetFieldProperties();
                page.GetView().MustRefreshFieldCollection = true;
				this.DialogResult = DialogResult.OK;
				this.Hide();
			}
		}

		private void btnPromptFont_Click(object sender, System.EventArgs e)
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

		private void btnFieldFont_Click(object sender, System.EventArgs e)
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
			}
		}

        private bool ShouldUseDefaultPromptFont(RenderableField field)
        {
            if ((field.PromptFont == null) || ((field.PromptFont.Name == "Microsoft Sans Serif") && (field.PromptFont.Size == 8.5)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ShouldUseDefaultControlFont(RenderableField field)
        {
            if ((field.ControlFont == null) || ((field.ControlFont.Name == "Microsoft Sans Serif") && (field.ControlFont.Size == 8.5)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected void SetFontStyles(RenderableField field)
        {
            if (ShouldUseDefaultPromptFont(field))
            {
                Configuration config = Configuration.GetNewInstance();
                FontStyle promptFontStyle = FontStyle.Regular;
                if (config.Settings.EditorFontBold)
                {
                    promptFontStyle |= FontStyle.Bold;
                }
                if (config.Settings.EditorFontItalics)
                {
                    promptFontStyle |= FontStyle.Italic;
                }
                field.PromptFont = new Font(config.Settings.EditorFontName, (float)config.Settings.EditorFontSize, promptFontStyle);
            }

            if (ShouldUseDefaultControlFont(field))
            {
                Configuration config = Configuration.GetNewInstance();
                FontStyle controlFontStyle = FontStyle.Regular;
                if (config.Settings.ControlFontBold)
                {
                    controlFontStyle |= FontStyle.Bold;
                }
                if (config.Settings.ControlFontItalics)
                {
                    controlFontStyle |= FontStyle.Italic;
                }
                field.ControlFont = new Font(config.Settings.ControlFontName, (float)config.Settings.ControlFontSize, controlFontStyle);
            }
            
            promptFont = field.PromptFont;
            controlFont = field.ControlFont;
        }
		#endregion	//Event Handlers
	}
}
