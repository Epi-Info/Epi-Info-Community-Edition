
using System;
using System.Data;
using System.Drawing;
using System.Xml;

using Epi.Data.Services;

namespace Epi.Fields
{
	/// <summary>
	/// RecStatus Field
	/// </summary>
	public class RecStatusField : PredefinedDataField, IMirrorable
	{
		#region Private Members
		private string value = string.Empty;
        private string tableName = string.Empty;
        private XmlElement viewElement;
        private XmlNode fieldNode;
		#endregion Private Members

		#region Constructors

		/// <summary>
		/// Instantiates a field
		/// </summary>
		/// <param name="view">The view this field belongs to</param>
		public RecStatusField(View view) : base(view)
		{
			Construct();
		}
		private void Construct()
		{
			this.Name = ColumnNames.REC_STATUS;
            base.CurrentRecordValueObject = Constants.NORMAL;
            this.dbColumnType = DbType.Int32;

		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="viewElement">Xml view element</param>
        public RecStatusField(View view, XmlElement viewElement)
            : base(view)
        {
            this.viewElement = viewElement;            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="fieldNode">Xml field node</param>
        public RecStatusField(View view, XmlNode fieldNode)
            : base(view)
        {
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, fieldNode);           
        }
		
		#endregion Constructors

        #region Public Properties
        /// <summary>
        /// Returns field type
        /// </summary>
        public override MetaFieldType FieldType
        {
            get
            {
                return MetaFieldType.RecStatus;
            }
        }

        /// <summary>
        /// Returns the promt for UI
        /// </summary>
        public override string PromptText
        {
            get
            {
                return SharedStrings.REC_STATUS;
            }
            set
            {
                throw new GeneralException("Prompt is pre-defined");
            }
        }

        /// <summary>
        /// Gets or sets the current record value. Translates null to normal. This will never let the value to be null.
        /// </summary>
        public override object CurrentRecordValueObject
        {
            get
            {
                if (base.CurrentRecordValueObject == null)
                    base.CurrentRecordValueObject = Constants.NORMAL;
                return base.CurrentRecordValueObject;
            }
            set
            {
                if (value == null)
                {
                    base.CurrentRecordValueObject = Constants.NORMAL;
                }
                else
                {
                    base.CurrentRecordValueObject = value;
                }
            }
        }

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
				throw new System.ApplicationException("Rec status field already exists");
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

     
	}
}
