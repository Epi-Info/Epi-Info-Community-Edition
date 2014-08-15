using System;
using System.ComponentModel;
using System.Data;
using System.Xml;
using Epi;
using Epi.Data;
using Epi.Data.Services;

namespace Epi.Fields
{
	/// <summary>
	/// Date Time Field.
	/// </summary>
	public class DateTimeField : InputTextBoxField
	{
		#region Private Members
        private XmlElement viewElement;
        private XmlNode fieldNode;
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;
		#endregion Private members
		
		#region Constructors
		
		/// <summary>
		/// Constructor for the class
		/// </summary>
		/// <param name="page">The page this field belongs to</param>
		public DateTimeField(Page page) 
            : base(page)
		{
            construct();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
		public DateTimeField(View view) 
            : base(view)
		{
            construct();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="viewElement">Xml view element</param>
        public DateTimeField(Page page, XmlElement viewElement)
            : base(page)
        {
            construct();
            this.viewElement = viewElement;
            this.Page = page;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="fieldNode">Xml field node</param>
        public DateTimeField(View view, XmlNode fieldNode) 
            : base(view)
        {
            construct();
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, this.fieldNode);
        }

        private void construct()
        {
            genericDbColumnType = GenericDbColumnType.DateTime;
            this.dbColumnType = DbType.DateTime;
        }
        
        /// <summary>
        /// Load from row
        /// </summary>
        /// <param name="row">Row</param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
        }

        public DateTimeField Clone()
        {
            DateTimeField clone = (DateTimeField)this.MemberwiseClone();
            base.AssignMembers(clone);
            return clone;
        }

		#endregion Constructors
		
        #region Public Properties

        public virtual string Watermark
        {
            get
            {
                System.Globalization.DateTimeFormatInfo formatInfo = System.Globalization.DateTimeFormatInfo.CurrentInfo;
                return string.Format("{0} {1}", formatInfo.ShortDatePattern.ToUpper(), formatInfo.LongTimePattern.ToUpper());
            }
            set { ;}
        }
        
        /// <summary>
        /// Returns field type
        /// </summary>
        public override MetaFieldType FieldType
        {
            get
            {
                return MetaFieldType.DateTime;
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

        /// <summary>
        /// Returns a string representation of Current Record Value
        /// </summary>
        public override string CurrentRecordValueString
        {
            get
            {
                if (CurrentRecordValueObject == null || CurrentRecordValueObject.Equals(DBNull.Value))
                {
                    return string.Empty;
                }
                else
                {
                    if (CurrentRecordValueObject is DateTime)
                    { 
                        return ((DateTime)CurrentRecordValueObject).ToString();
                    }
                    else if (CurrentRecordValueObject is String)
                    { 
                        DateTime dateTime = new DateTime();

                        if (DateTime.TryParse((string)CurrentRecordValueObject, out dateTime))
                        {
                            CurrentRecordValueObject = dateTime;
                            return dateTime.ToString();
                        }
                    }
                }

                return string.Empty;
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    bool result;
                    DateTime dateTimeEntered;
                    
                    result = DateTime.TryParse(value, out dateTimeEntered);

                    if (result)
                    {
                        CurrentRecordValueObject = dateTimeEntered;
                    }
                    else 
                    {
                        CurrentRecordValueObject = null;
                    }
                }
                else
                {
                    CurrentRecordValueObject = null;
                }
            }
        }

        #endregion Public Properties

		#region Public Methods

        /// <summary>
        /// Returns the string value that is reflected my a mirror field.
        /// </summary>
        /// <returns>reflected value string</returns>
        public virtual string GetReflectedValue()
        {
            return this.CurrentRecordValueObject.ToString();
        }

		#endregion Public methods

		#region Private Methods

        /// <summary>
        /// Inserts the field to the database
        /// </summary>
        protected override void InsertField()
        {
            this.Id = GetMetadata().CreateField(this);
            base.OnFieldAdded();
        }

        /// <summary>
        /// Update the field to the database
        /// </summary>
        protected override void UpdateField()
        {
            GetMetadata().UpdateField(this);
        }

		#endregion Protected Methods

		#region Event Handlers

		#endregion
	}
}