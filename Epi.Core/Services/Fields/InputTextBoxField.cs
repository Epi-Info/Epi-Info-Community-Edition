using System;
using System.Data;
using Epi;
using Epi.Data;

namespace Epi.Fields
{
	/// <summary>
	/// Input text box field
	/// </summary>
	public abstract class InputTextBoxField : InputFieldWithSeparatePrompt, IFieldWithCheckCodeAfter, IFieldWithCheckCodeBefore
	{

		#region Private Members
		
		private string checkCodeAfter = string.Empty;
		private string checkCodeBefore = string.Empty;
		// protected DragableTextBox textbox;

		#endregion

		#region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">Page</param>
		public InputTextBoxField(Page page) : base(page)
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
		public InputTextBoxField(View view): base(view)
		{
			Construct();
		}

        /// <summary>
        /// Load from row
        /// </summary>
        /// <param name="row">Row</param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
            //checkCodeAfter = row["ControlAfterCheckCode"].ToString();
            //checkCodeBefore = row["ControlBeforeCheckCode"].ToString();
        }
        
        public InputTextBoxField Clone()
        {
            InputTextBoxField clone = (InputTextBoxField)this.MemberwiseClone();
            base.AssignMembers(clone);
            return clone;
        }

        public override void AssignMembers(Object field)
        {
            base.AssignMembers(field);
            //(field as InputTextBoxField).checkCodeAfter = this.checkCodeAfter;
            //(field as InputTextBoxField).checkCodeBefore = this.checkCodeBefore;
        }

		private void Construct()
		{
			//this.Enter += new EnterEventHandler(InputTextBoxField_Enter);
		}
		#endregion Constructors

		#region Public Properties

        /// <summary>
        /// Check Code After
        /// </summary>
		public string CheckCodeAfter
		{
			get
			{
				return (checkCodeAfter);
			}
			set
			{
				checkCodeAfter = value;
			}
		}

        /// <summary>
        /// Check Code Before
        /// </summary>
		public string CheckCodeBefore
		{
			get
			{
				return (checkCodeBefore);
			}
			set
			{
				checkCodeBefore = value;
			}
		}

		#endregion Public Properties

		#region Protected Properties

		#endregion Protected Properties

		#region Public Methods

		/// <summary>
		/// Deletes the field
		/// </summary>
		public override void Delete()
		{
			GetMetadata().DeleteField(this);
            view.MustRefreshFieldCollection = true;
		}

		#endregion Public Methods		

		#region Private Methods

		#endregion

	}
}
