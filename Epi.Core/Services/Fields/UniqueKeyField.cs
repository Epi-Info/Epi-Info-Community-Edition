using System;
using System.Data;
using System.Drawing;
using System.Xml;

using Epi.Data.Services;

namespace Epi.Fields
{
	/// <summary>
	/// Unique Key Field
	/// </summary>
	public class UniqueKeyField : PredefinedDataField, IMirrorable
	{
		#region Constructors

		/// <summary>
		/// Instantiates a field
		/// </summary>
		/// <param name="view">The view this field belongs to</param>
		public UniqueKeyField(View view) : base(view)
		{
			Construct();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="viewElement">Xml view element</param>
        public UniqueKeyField(View view, XmlElement viewElement)
            : base(view)          
        {
            this.viewElement = viewElement;            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="fieldNode">Xml field node</param>
        public UniqueKeyField(View view, XmlNode fieldNode)
            : base(view)
        {
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, fieldNode);           
        }

		private void Construct()
		{
			this.Name = ColumnNames.UNIQUE_KEY;
            this.dbColumnType = DbType.Int32;
		}
		
		#endregion Constructors

        #region Private Data Members
        private XmlElement viewElement;
        private XmlNode fieldNode;
        #endregion  //Private Data Members

        #region Public Properties
        /// <summary>
        /// Returns field type
        /// </summary>
        public override MetaFieldType FieldType
        {
            get
            {
                return MetaFieldType.UniqueKey;
            }
        }

        /// <summary>
        /// Prompt's text
        /// </summary>
        public override string PromptText
        {
            get
            {
                return SharedStrings.UNIQUE_KEY;
            }
            set 
            {
                throw new GeneralException("Prompt for Unique key is pre-defined");
            }
        }
        #endregion Public Properties

        #region Public Methods

        /// <summary>
		/// Saves the field to the database
		/// </summary>
		protected override void InsertField()
		{
			if (this.Id == 0)
			{
                this.Id = GetMetadata().CreateField(this);                			
			}
			else
			{
				throw new System.ApplicationException("Unique key field already exists");
			}
		}		

		/// <summary>
		/// Deletes the field
		/// </summary>
		public override void Delete()
		{
			GetMetadata().DeleteField(this);
		}

        /// <summary>
        /// Returns the string value that is reflected my a mirror field.
        /// </summary>
        /// <returns>reflected value string</returns>
        public virtual string GetReflectedValue()
        {
            return this.CurrentRecordValueString;
        }
        #endregion

        /// <summary>
        /// The view element of the field
        /// </summary>
        public XmlElement ViewElement
        {
            get
            {
                return viewElement;
            }
            set
            {
                viewElement = value;
            }
        }

	}
}