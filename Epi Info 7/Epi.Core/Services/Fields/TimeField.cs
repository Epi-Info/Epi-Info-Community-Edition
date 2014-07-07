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
	/// Time Field.
	/// </summary>
	public class TimeField : DateTimeField
	{
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;

		#region Constructors
		
		/// <summary>
		/// Constructor for the class
		/// </summary>
		/// <param name="page">The page this field belongs to</param>
		public TimeField(Page page) 
            : base(page)
		{
            construct();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
		public TimeField(View view) 
            : base(view)
		{
            construct();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">Page the field belongs to</param>
        /// <param name="viewElement">Xml view element</param>
        public TimeField(Page page, XmlElement viewElement)
            : base(page, viewElement)
        {
            construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="fieldNode">Xml field node</param>
        public TimeField(View view, XmlNode fieldNode) 
            : base(view, fieldNode)
        {
            construct();
        }

        private void construct()
        {
            genericDbColumnType = GenericDbColumnType.Time;
            this.dbColumnType = DbType.Time;
        }

		#endregion Constructors

        #region Public Properties
        public override string Watermark
        {
            get 
            {
                System.Globalization.DateTimeFormatInfo formatInfo = System.Globalization.DateTimeFormatInfo.CurrentInfo;
                return formatInfo.LongTimePattern.ToUpper();
            }
        }
        
        /// <summary>
        /// Returns field type
        /// </summary>
        public override MetaFieldType FieldType
        {
            get
            {
                return MetaFieldType.Time;
            }
        }

        /// <summary>
        /// Returns a string representation of Current Record Value
        /// </summary>
        public sealed override string CurrentRecordValueString
        {
            get
            {
                if (CurrentRecordValueObject == null || CurrentRecordValueObject.Equals(DBNull.Value))
                {
                    return string.Empty;
                }
                else
                {
                    return ((DateTime)CurrentRecordValueObject).ToLongTimeString();
                }
            }
        }

        #endregion Public Properties

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

		#endregion Private Methods
	}
}