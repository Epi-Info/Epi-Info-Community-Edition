using System;
using System.Data;
using System.Xml;

using Epi;

namespace Epi.Fields
{
	/// <summary>
	/// Upper-Case Text Field.
	/// </summary>
	public class UpperCaseTextField : SingleLineTextField
	{
		#region Constructors

		/// <summary>
		/// Constructor for the class
		/// </summary>
		/// <param name="page">The page this field belongs to</param>
		public UpperCaseTextField(Page page) : base(page)
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
		public UpperCaseTextField(View view) : base(view)
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="viewElement">Xml view element</param>
        public UpperCaseTextField(Page page, XmlElement viewElement)
            : base(page, viewElement)
        {
        }       
      
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="fieldNode">Xml field node</param>
        public UpperCaseTextField(View view, XmlNode fieldNode) : base(view, fieldNode)
        {
        }

		#endregion

        #region Public Properties
        /// <summary>
        /// Returns field type
        /// </summary>
        public override MetaFieldType FieldType
        {
            get
            {
                return MetaFieldType.TextUppercase;
            }
        }
        #endregion Public Properties

		#region Public Methods

		#endregion Public Methods

	}
}
