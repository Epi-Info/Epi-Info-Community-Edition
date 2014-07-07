using System;
using System.ComponentModel;
using System.Data;
using System.Xml;
using Epi;
using Epi.Data;

namespace Epi.Fields
{
	/// <summary>
	/// Number Field.
	/// </summary>
	public class NumberField : InputTextBoxField, IPatternable
	{
		#region Private Members
		private string pattern = string.Empty;
        private string lower = string.Empty;
        private string upper = string.Empty;
        private XmlElement viewElement;
        private XmlNode fieldNode;
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;
		#endregion  Private Members
		
		#region Constructors
		
		/// <summary>
		/// Constructor for the class
		/// </summary>
		/// <param name="page">The page this field belongs to</param>
		public NumberField(Page page) : base(page)
		{
			Construct();
		}

        /// <summary>
        /// NumberField
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="viewElement">XML view element</param>
        public NumberField(Page page, XmlElement viewElement)
            : base(page)
        {
            this.viewElement = viewElement;
            this.Page = page;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
		public NumberField(View view) : base(view)
		{
			Construct();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="fieldNode">Xml Field Node</param>
        public NumberField(View view, XmlNode fieldNode) : base(view)
        {
            Construct();
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, this.fieldNode);
        }

        /// <summary>
        /// Load from row
        /// </summary>
        /// <param name="row">Row</param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
            pattern = row[ColumnNames.PATTERN].ToString();
            pattern = pattern == "None" ? "" : pattern;
            lower = row[ColumnNames.LOWER].ToString();
            upper = row[ColumnNames.UPPER].ToString();
        }

        public NumberField Clone()
        {
            NumberField clone = (NumberField)this.MemberwiseClone();
            base.AssignMembers(clone);
            return clone;
        }

		private void Construct()
		{
            genericDbColumnType = GenericDbColumnType.Double;
            this.dbColumnType = DbType.Double;
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
                return MetaFieldType.Number;
            }
        }

        public override string GetDbSpecificColumnType()
        {
            return GetProject().CollectedData.GetDatabase().GetDbSpecificColumnType(GenericDbColumnType.Double);
        }

        ///// <summary>
        ///// Returns a fully-typed current record value
        ///// </summary>
        //public Single CurrentRecordValue
        //{
        //    get
        //    {
        //        if (base.CurrentRecordValueObject == null) return ;
        //        else return CurrentRecordValueObject.ToString();
        //    }
        //    set
        //    {
        //        base.CurrentRecordValueObject = value;
        //    }
        //}

        /// <summary>
        /// Pattern
        /// </summary>
		public string Pattern
		{
			get
			{
                return (pattern);
			}
			set
			{
				pattern = value == "None" ? "" : value;
			}
		}

        /// <summary>
        /// Lower
        /// </summary>
        public string Lower
        {
            get
            {
                return (lower);
            }
            set
            {
                lower = value;
            }
        }

        /// <summary>
        /// Upper
        /// </summary>
        public string Upper
        {
            get
            {
                return (upper);
            }
            set
            {
                upper = value;
            }
        }

		#endregion Public Properties

		#region Public Methods

		///// <summary>
		///// Saves the current field location
		///// </summary>
        //public override void SaveFieldLocation()
        //{
        //    Metadata.UpdateControlPosition(this);
        //    Metadata.UpdatePromptPosition(this);
        //}

		#endregion

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

		#endregion

		#region Event Handlers


		#endregion Event Handlers

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
