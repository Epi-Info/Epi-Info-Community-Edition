using System;
using System.ComponentModel;
using System.Data;
using System.Xml;
using Epi;
using Epi.Data;

namespace Epi.Fields
{
	/// <summary>
	/// Phone Number Field.
	/// </summary>
	public class PhoneNumberField : InputTextBoxField, IPatternable
	{
		#region Private Members

        private XmlElement viewElement;
        private XmlNode fieldNode;
		private string pattern = string.Empty;
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;

		#endregion
		
		#region Constructors
		
		/// <summary>
		/// Constructor for the class
		/// </summary>
		/// <param name="page">The page this field belongs to</param>
		public PhoneNumberField(Page page) : base(page)
		{
			Construct();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
		public PhoneNumberField(View view) : base(view)
		{
			Construct();
		}	

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="viewElement">Xml view element</param>
        public PhoneNumberField(Page page, XmlElement viewElement)
            : base(page)
        {
            this.viewElement = viewElement;
            this.Page = page;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="fieldNode">Xml field node</param>
        public PhoneNumberField(View view, XmlNode fieldNode) : base(view)
        {
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, this.fieldNode);
        }

		private void Construct()
		{
            genericDbColumnType = GenericDbColumnType.String;
            this.dbColumnType = DbType.String;
		}

        /// <summary>
        /// Load from row
        /// </summary>
        /// <param name="row">Row</param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
            pattern = row[ColumnNames.PATTERN].ToString();
        }
        
        public PhoneNumberField Clone()
        {
            PhoneNumberField clone = (PhoneNumberField)this.MemberwiseClone();
            base.AssignMembers(clone);
            return clone;
        }
		
        #endregion Constructors

		#region Public Events
		#endregion

        #region Public Properties
        /// <summary>
        /// Returns field type
        /// </summary>
        public override MetaFieldType FieldType
        {
            get
            {
                return MetaFieldType.PhoneNumber;
            }
        }

        /// <summary>
        /// Returns a fully-typed current record value
        /// </summary>
        public string CurrentRecordValue
        {
            get
            {
                if (base.CurrentRecordValueObject == null) return string.Empty;
                else return CurrentRecordValueObject.ToString();
            }
            set
            {
                base.CurrentRecordValueObject = value;
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

		#region IPatternableField Members

        /// <summary>
        /// Gets and sets Pattern for field
        /// </summary>
		public string Pattern
		{
			get
			{
				return (pattern);
			}
			set
			{
				pattern = value;
			}
		}

		#endregion

		#region Public Methods
        /// <summary>
        /// Returns specific data type for the target DB.
        /// </summary>
        /// <returns></returns>
        public override string GetDbSpecificColumnType()
        {
            return base.GetDbSpecificColumnType() + "(20)";
        }

		#endregion Public Methods

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

        ///// <summary>
        ///// Inserts the field to the database
        ///// </summary>
        //protected override void InsertField()
        //{
        //    insertStarted = true;
        //    _inserter = new BackgroundWorker();
        //    _inserter.DoWork += new DoWorkEventHandler(inserter_DoWork);
        //    _inserter.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_inserter_RunWorkerCompleted);
        //    _inserter.RunWorkerAsync();
        //}

        //void _inserter_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    OnFieldInserted(this);
        //}

        //void inserter_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    fieldsWaitingToUpdate++;
        //    lock (view.FieldLockToken)
        //    {                
        //        this.Id = GetMetadata().CreateField(this);
        //        base.OnFieldAdded();
        //        fieldsWaitingToUpdate--;
        //    }
        //}

        ///// <summary>
        ///// Update the field to the database
        ///// </summary>
        //protected override void UpdateField()
        //{
        //    _updater = new BackgroundWorker();
        //    _updater.DoWork += new DoWorkEventHandler(DoWork);
        //    _updater.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_updater_RunWorkerCompleted);
        //    _updater.RunWorkerAsync();
        //}

        //void _updater_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    OnFieldUpdated(this);
        //}

        //private void DoWork(object sender, DoWorkEventArgs e)
        //{
        //    fieldsWaitingToUpdate++;
        //    lock (view.FieldLockToken)
        //    {
        //        GetMetadata().UpdateField(this);
        //        fieldsWaitingToUpdate--;
        //    }
        //}

		#endregion Private Methods

		#region Event Handlers
		
		#endregion Event Handler
	}
}