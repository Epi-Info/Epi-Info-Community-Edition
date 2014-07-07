#region Namespaces
using System;


#endregion	Namespaces

namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    /// <summary>
    /// The Patternable Text Field dialog
    /// </summary>
    public partial class PatternableTextFieldDefinition : TextFieldDefinition
	{
		#region	Protected Controls
		/// <summary>
		/// ???
		/// </summary>
		/// <summary>
		/// ???
		/// </summary>
		#endregion //Protected Controls

		#region	Fields
		
		private string pattern;
		#endregion	//Fields

        #region Events
        protected virtual void cbxPattern_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        #endregion


        #region Constructors
        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public PatternableTextFieldDefinition()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for Patternable Text Field Definition
        /// </summary>
        /// <param name="frm">The main form</param>
		public PatternableTextFieldDefinition(MainForm frm) : base(frm)
		{
			InitializeComponent();
		}
		#endregion	Constructors

		#region	Protected Properties
		/// <summary>
		/// The field's pattern
		/// </summary>
		protected string Pattern
		{
			get
			{
				return pattern;
			}
			set
			{
				pattern = value;
			}
		}
		#endregion	//Protected Properties

        

	}
}
