#region Namespaces

using System;
using System.Data;
using System.Drawing;
using Epi;
using Epi.Data.Services;
using System.Windows.Forms;
#endregion

namespace Epi.Fields
{

    /// <summary>
	/// Renderable Field.
	/// </summary>
	public abstract class RenderableField : Field
	{
		#region Private Members
		private Page page = null;
		private bool hasTabStop = false;
		private bool isControlResizable = true;
        protected bool insertStarted;
        private double tabIndex = 0;
		private double controlTopPositionPercentage = 0;
		private double controlLeftPositionPercentage = 0;
		private double controlHeightPercentage = 0;
		private double controlWidthPercentage = 0;
		private string promptText = string.Empty;
		private System.Drawing.Font promptFont = null;
		private System.Drawing.Font controlFont = null;

		/// <summary>
		/// Tag is a placeholder to temporarily store any information about the field.
		/// </summary>
		private object tag = null;

		#endregion
        
		#region Constructors

		/// <summary>
        /// Constructor
		/// </summary>
		/// <param name="page">The page the field belongs to</param>
		public RenderableField(Page page) : base(page.GetView())
		{            
			this.page = page;
		}
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view the field belongs to</param>
		public RenderableField (View view) : base(view)
		{		
		}

        /// <summary>
        /// Load Renderable Field from a <see cref="System.Data.DataRow"/>
        /// </summary>
        /// <param name="row"></param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
            int pageId = (int)row[ColumnNames.PAGE_ID];

            // Fixes a defect where users can't copy/paste fields to another View because the PageId will never
            // be found in such a case using GetPageById(pageId) in the destination View. (EK 12/29/2010)
            bool pageExistsInView = false;

            foreach (Page page in this.view.Pages)
            {
                if (pageId == page.Id)
                {
                    pageExistsInView = true;
                }
            }

            // the source page exists in the same view as the destination page; proceed with assignment
            if (pageExistsInView)
            {
                Page = this.view.GetPageById(pageId);
            }
            // the source page does not exist in the same view as the destination page; this is a copy/paste
            // operation and the page will be re-assigned later, so set it to null for the moment. We can't use
            // a page ID from another view anyway because the page Ids are unique within an Epi Info project.
            else
            {
                Page = null;
            }

            controlHeightPercentage = (double)row["ControlHeightPercentage"];
            controlWidthPercentage = (double)row["ControlWidthPercentage"];
            controlLeftPositionPercentage = (double)row["ControlLeftPositionPercentage"];
            controlTopPositionPercentage = (double)row["ControlTopPositionPercentage"];
            promptText = row["PromptText"].ToString();
            tabIndex = Convert.ToDouble(row["TabIndex"]);
            hasTabStop = (bool)row["HasTabStop"];
            controlFont = new System.Drawing.Font(row["ControlFontFamily"].ToString(), float.Parse(row["ControlFontSize"].ToString()), (FontStyle)System.Enum.Parse(typeof(FontStyle), row["ControlFontStyle"].ToString(), true));
            promptFont = GetMetadata().ExtractPromptFont(row);
        }


        public override void AssignMembers(Object field)
        {
            (field as RenderableField).Page = this.Page;
            (field as RenderableField).controlHeightPercentage = this.controlHeightPercentage;
            (field as RenderableField).controlWidthPercentage = this.controlWidthPercentage;
            (field as RenderableField).controlLeftPositionPercentage = this.controlLeftPositionPercentage;
            (field as RenderableField).controlTopPositionPercentage = this.controlTopPositionPercentage;

            (field as RenderableField).promptText = this.promptText;
            (field as RenderableField).tabIndex = this.tabIndex;
            (field as RenderableField).hasTabStop = this.hasTabStop;
            (field as RenderableField).controlFont = this.controlFont;
            (field as RenderableField).promptFont = this.promptFont;

            base.AssignMembers(field);
        }

		#endregion Constructors

		#region Public Properties
		
		public Control Control { get; set; }
        
        /// <summary>
		/// Gets/sets the page that this field belongs to.
		/// </summary>
		public Page Page
		{
			get
			{
				return this.page;
			}
			set
			{
				this.page = value;
			}
		}

        /// <summary>
        /// Gets/sets the prompt's font
        /// </summary>
		public Font PromptFont
		{
			get
			{
				if (promptFont == null)
				{
					return new System.Drawing.Font(FontFamily.GenericSansSerif, 8.5f);
				}
				else
				{
					return promptFont;
				}
			}
			set
			{
				promptFont = value;
			}
		}

        /// <summary>
        /// Gets/sets the control's font
        /// </summary>
		public Font ControlFont
		{
			get
			{
				if (controlFont == null)
				{
					return new System.Drawing.Font(FontFamily.GenericSansSerif, 8.5f);
				}
				else
				{
					return controlFont;
				}
			}
			set
			{
				controlFont = value;
			}
		}

        /// <summary>
        /// Gets/sets the flag that indicates if this field has a tab stop.
        /// </summary>
		public bool HasTabStop
		{
			get
			{
				return hasTabStop;
			}
			set
			{
				hasTabStop = value;
			}		
		}

        /// <summary>
        /// Gets/sets the field's tab index
        /// </summary>
        public double TabIndex
		{
			get
			{
				return tabIndex;
			}
			set
			{
				tabIndex = value;
			}
		}

		/// <summary>
		/// Gets/sets top position percentage for UI control.
		/// </summary>
        public double ControlTopPositionPercentage
		{
			get
			{
                return controlTopPositionPercentage - (int)controlTopPositionPercentage;
			}
			set
			{
                controlTopPositionPercentage = value - (int)value;
			}
		}

		/// <summary>
		/// Gets/sets left position percentage for UI control.
		/// </summary>
        public double ControlLeftPositionPercentage
		{
			get
			{
                return controlLeftPositionPercentage - (int)controlLeftPositionPercentage;
			}
			set
			{
                controlLeftPositionPercentage = value - (int)value;
			}
		}

		/// <summary>
		/// Gets/sets height percentage for UI control.
		/// </summary>
        public double ControlHeightPercentage
		{
			get
			{
				return controlHeightPercentage;
			}
			set
			{
				controlHeightPercentage = value;
			}
		}

        /// <summary>
        /// Gets/sets width percentage for UI control.
        /// </summary>
		public double ControlWidthPercentage
		{
			get
			{
				return controlWidthPercentage;
			}
			set
			{
				controlWidthPercentage = value;
			}
		}

		/// <summary>
		/// Gets/sets prompt text for UI control.
		/// </summary>
        public string PromptText
		{
			get
			{
				return promptText;
			}
			set
			{
				promptText = value;
			}
		}
		
		/// <summary>
		/// Tag is a placeholder to temporarily store any information about the field.
		/// Typical usage is to store the control object when form is generated in Enter.
		/// </summary>
		public object Tag
		{
			get
			{
				return tag;
			}
			set
			{
				tag = value;
			}
		}

		#endregion

		#region Protected Properties

		/// <summary>
		/// Gets/sets control resizable flag for UI control.
		/// </summary>
        public bool IsControlResizable
		{
			get
			{
				return isControlResizable;
			}
			set
			{
				isControlResizable = value;
			}
		}
		#endregion Protected Properties

		#region Public Methods

		/// <summary>
		/// Saves the field to the database
		/// </summary>
		public override void SaveToDb()
		{
			if (Id == 0)
			{
                InsertField();
			}
			else
			{
				UpdateField();
			}
		}

        /// <summary>
        /// Updates the control position
        /// </summary>
        public void UpdateControlPosition()
        {
            GetMetadata().UpdateControlPosition(this);
        }

        /// <summary>
        /// Updates the control size
        /// </summary>
        public void UpdateControlSize()
        {
            GetMetadata().UpdateControlSize(this);
        }
        
		#endregion

		#region Protected Methods

        /// <summary>
        /// Updates the field in Metadata
        /// </summary>
		protected abstract void UpdateField();

		#endregion

		#region Private Methods
		
		#endregion Private Methods

	}
}