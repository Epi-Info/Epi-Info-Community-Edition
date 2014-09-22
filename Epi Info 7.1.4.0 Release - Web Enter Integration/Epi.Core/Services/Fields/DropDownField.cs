using System;
using System.Data;
using System.Xml;
using Epi.Data;
using Epi;

namespace Epi.Fields
{
    /// <summary>
    /// Drop Down Field abstract class
    /// </summary>
	public abstract class DropDownField : InputFieldWithSeparatePrompt
	{
		#region Private Class Members
		// protected DragableComboBox combobox;
		#endregion Private Class Members

		#region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page the field belongs to</param>
		public DropDownField(Page page) : base(page)
		{
			Construct();
		}
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view the field belongs to</param>
		public DropDownField(View view) : base(view)
		{
			Construct();
		}
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page the field belongs to</param>
        /// <param name="viewElement">The Xml view element of the ImageField.</param>
        public DropDownField(Page page, XmlElement viewElement)
            : base(page)
        {            
        }  

        /// <summary>
        /// Common constructor code.
        /// </summary>
		private void Construct()
		{
		}

		#endregion Constructors

        #region Public Properties
        #endregion Public Properties

        #region Protected Properties
        #endregion Protected Properties

        #region Public Methods
        /// <summary>
		/// Deletes the Drop Down field
		/// </summary>
		public override void Delete()
		{
			GetMetadata().DeleteField(this);
            view.MustRefreshFieldCollection = true;
		}

		#endregion

		#region Private Methods
		#endregion Private Methods
	}
}
