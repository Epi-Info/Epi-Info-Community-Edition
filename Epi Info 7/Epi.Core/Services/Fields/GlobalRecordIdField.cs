using System;
using System.Data;
using System.Drawing;
using System.Xml;

using Epi.Data.Services;

namespace Epi.Fields
{
	/// <summary>
	/// Global Record Id Field
	/// </summary>
	public class GlobalRecordIdField : PredefinedDataField, IMirrorable
	{
		#region Constructors

		/// <summary>
		/// Instantiates a field
		/// </summary>
		/// <param name="view">The view this field belongs to</param>
        public GlobalRecordIdField(View view)
            : base(view)
		{
			Construct();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="viewElement">Xml view element</param>
        public GlobalRecordIdField(View view, XmlElement viewElement)
            : base(view)          
        {
            this.viewElement = viewElement;            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="fieldNode">Xml field node</param>
        public GlobalRecordIdField(View view, XmlNode fieldNode)
            : base(view)
        {
            this.fieldNode = fieldNode;
            //dpb this.view.Project.Metadata.GetFieldData(this, fieldNode);           
        }

		private void Construct()
		{
            this.Name = ColumnNames.GLOBAL_RECORD_ID;
            this.dbColumnType = DbType.String;
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
                return MetaFieldType.GlobalRecordId;
            }
        }

        /// <summary>
        /// Prompt's text
        /// </summary>
        public override string PromptText
        {
            get
            {
                return SharedStrings.GLOBAL_RECORD_ID;
            }
            set 
            {
                throw new GeneralException("Prompt for Unique Identifier ID is pre-defined");
            }
        }

        public string GlobalRecordId
        {
            get
            {
                return this.CurrentRecordValueObject.ToString();
            }
            set
            {
                this.CurrentRecordValueString = value;
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

        public void NewValue()
        {
            if (CurrentRecordValueString.Equals(string.Empty))
            {
                CurrentRecordValueObject = Guid.NewGuid().ToString();
            }
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