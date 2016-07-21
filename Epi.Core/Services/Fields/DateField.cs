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
	/// Date Field.
	/// </summary>
	public class DateField : DateTimeField
	{
		#region Fields
        private string _pattern = string.Empty;
        private string _lower = string.Empty;
        private string _upper = string.Empty;
        private DateTime _lowerDateTime;
        private DateTime _upperDateTime;
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;
        private const string ISO8601 = "YYYY-MM-DD"; 
        //-EI-111
        private bool _notfuturedate;
       //
		#endregion Fields
		
		#region Constructors
		
		/// <summary>
		/// Constructor for the class
		/// </summary>
		/// <param name="page">The page this field belongs to</param>
		public DateField(Page page) 
            : base(page)
		{
            construct();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
		public DateField(View view) 
            : base(view)
		{
            construct();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="viewElement">Xml view element</param>
        public DateField(Page page, XmlElement viewElement)
            : base(page, viewElement)
        {
            construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="fieldNode">Field Node</param>
        public DateField(View view, XmlNode fieldNode) 
            : base(view, fieldNode)
        {
            construct();
        }

        private void construct()
        {
            genericDbColumnType = GenericDbColumnType.Date;
            this.dbColumnType = DbType.Date;
        }
        #endregion Constructors

        /// <summary>
        /// Load from row
        /// </summary>
        /// <param name="row">Row</param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);

            _pattern = row[ColumnNames.PATTERN].ToString();
            _lower = row[ColumnNames.LOWER].ToString();
            _upper = row[ColumnNames.UPPER].ToString();

            //---EI-111
            if (_upper == CommandNames.SYSTEMDATE)
            {
                _upper = GetRange(DateTime.Today);
                _notfuturedate = true;
             }
            //--

            LowerDate = GetRange(_lower);
            UpperDate = GetRange(_upper);
        }
        
        public DateTimeField Clone()
        {
            DateTimeField clone = (DateTimeField)this.MemberwiseClone();
            base.AssignMembers(clone);
            return clone;
        }

        public bool IsInRange(DateTime dateCandidate)
        {
            if (LowerDate.Ticks == 0 && UpperDate.Ticks == 0)
            {
                return true;
            }

            if (LowerDate <= dateCandidate && dateCandidate <= UpperDate )
            {
                return true;
            }
            else
            {
                string warningMessage = String.Format(
                    SharedStrings.INVALID_DATE_RANGE, 
                    LowerDate.ToShortDateString(), 
                    UpperDate.ToShortDateString());
                    //--Ei-111
                    if (_notfuturedate)
                    {
                        warningMessage = SharedStrings.INVALID_NOTFUTUREDATE;
                    }
                    //--
                System.Windows.Forms.MessageBox.Show(
                    warningMessage,
                    SharedStrings.WARNING,
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
            }

            return false;
        }

        #region Public Properties
        public override string Watermark
        {
            get
            {
                System.Globalization.DateTimeFormatInfo formatInfo = System.Globalization.DateTimeFormatInfo.CurrentInfo;
                return formatInfo.ShortDatePattern.ToUpperInvariant();
            }
        }
        
        /// <summary>
        /// Returns field type
        /// </summary>
        public override MetaFieldType FieldType
        {
            get
            {
                return MetaFieldType.Date;
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
                    if (CurrentRecordValueObject is DateTime)
                    { 
                        return ((DateTime)CurrentRecordValueObject).ToShortDateString();
                    }
                    else if (CurrentRecordValueObject is String)
                    { 
                        DateTime dateTime = new DateTime();

                        if (DateTime.TryParse((string)CurrentRecordValueObject, out dateTime))
                        {
                            CurrentRecordValueObject = dateTime;
                            return dateTime.ToShortDateString();
                        }
                    }
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Lower
        /// </summary>
        public string Lower
        {
            get
            {
                return (_lower);
            }
            set
            {
                _lower = value;
            }
        }

        /// <summary>
        /// Upper
        /// </summary>
        public string Upper
        {
            get
            {
                return (_upper);
            }
            set
            {
                _upper = value;
            }
        }
        //--EI-111
        /// <summary>
        /// Notfuturedate
        /// </summary>
        public bool Notfuturedate
        {
            get
            {
                return (_notfuturedate);
            }
            set
            {
                _notfuturedate = value;
            }
        }
        //--
        /// <summary>
        /// Pattern
        /// </summary>
        public string Pattern
        {
            get
            {
                return _pattern == "" ? ISO8601 : _pattern;
            }
            set
            {
                _pattern = value == "" ? ISO8601 : value; // ISO8601
            }
        }

        public DateTime LowerDate
        {
            get
            {
                return (_lowerDateTime);
            }
            set
            {
                if (((DateTime)value).Ticks > 0)
                {
                    _lowerDateTime = value;
                    _lower = GetRange(value);
                }
                else
                {
                    _lower = "";
                }
            }
        }

        public DateTime UpperDate
        {
            get
            {
                return (_upperDateTime);
            }
            set
            {
                if (((DateTime)value).Ticks > 0)
                {
                    _upperDateTime = value;
                    _upper = GetRange(value);
                }
                else
                {
                    _upper = "";
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

        protected string GetRange(DateTime dateTime)
        {
            string format = Pattern.Replace("-", "'-'");
            format = format.Replace("Y", "y");
            format = format.Replace("D", "d");

            return dateTime.ToString(format);
        }

        protected DateTime GetRange(String dateString)
        {
            DateTime dateCandidate = new DateTime();

            if (string.IsNullOrEmpty(dateString)) return dateCandidate;

            string format = Pattern.Replace("-", "'-'");
            format = format.Replace("Y", "y");
            format = format.Replace("D", "d");

            bool result = DateTime.TryParseExact(
                dateString,
                format,
                System.Globalization.DateTimeFormatInfo.InvariantInfo,
                System.Globalization.DateTimeStyles.None,
                out dateCandidate);

            return dateCandidate;
        }

		#endregion Private Methods	
	}
}