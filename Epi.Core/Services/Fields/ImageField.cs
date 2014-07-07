using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Xml;
using Epi;
using Epi.Data;

namespace Epi.Fields
{	
	/// <summary>
	/// Image field
	/// </summary>
	public class ImageField : InputFieldWithSeparatePrompt
	{
		#region Private Members
        /// <summary>
        /// Boolean setting that determines if the image should keep its true size thus effectively cropped to the size of the image control.
        /// </summary>
        private bool shouldRetainImageSize;
        /// <summary>
        /// The full path of the image file to be displayed.
        /// </summary>
        private string fileName = string.Empty;
        /// <summary>
        /// The Xml view element of the ImageField.
        /// </summary>
        private XmlElement viewElement;
        /// <summary>
        /// Xml Node representation of ImageField.
        /// </summary>
        private XmlNode fieldNode;
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;
		#endregion Private Members

		#region Public Events
		#endregion Public Events

		#region Constructors
		/// <summary>
		/// Constructor for the class
		/// </summary>
		/// <param name="page">The page this field belongs to</param>
		public ImageField(Page page) : base(page)
		{
            construct();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page this field belongs to</param>
        /// <param name="viewElement">The Xml view element of the ImageField.</param>
        public ImageField(Page page, XmlElement viewElement) : base(page)
        {
            construct();
            this.viewElement = viewElement;
            this.Page = page;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view this field belongs to</param>
		public ImageField(View view) : base(view)
		{
            construct();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view this field belongs to</param>
        /// <param name="fieldNode">Xml Node representation of ImageField.</param>
        public ImageField(View view, XmlNode fieldNode) : base(view)
        {
            construct();
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, this.fieldNode);
        }

        private void construct()
        {
            genericDbColumnType = GenericDbColumnType.Object;
            this.dbColumnType = DbType.Object;
        }

        /// <summary>
        /// Load ImageField from a System.Data.DataRow
        /// </summary>
        /// <param name="row"></param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
            shouldRetainImageSize = (bool)row["ShouldRetainImageSize"];
        }

        public ImageField Clone()
        {
            ImageField clone = (ImageField)this.MemberwiseClone();
            base.AssignMembers(clone);
            return clone;
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
                return MetaFieldType.Image;
            }
        }

		/// <summary>
		/// Gets/sets whether the image size is retained
		/// </summary>
		public bool ShouldRetainImageSize
		{
			get
			{
				return shouldRetainImageSize;
			}
			set
			{
				shouldRetainImageSize = value;
			}
		}

        /// <summary>
        /// Gets/sets the Xml view element of the field
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
        /// Returns a fully-typed current record value
        /// </summary>
        public byte[] CurrentRecordValue
        {
            get
            {
                if (base.CurrentRecordValueObject == null) return null;
                else return (byte[])CurrentRecordValueObject;
            }
            set
            {
                base.CurrentRecordValueObject = value;
            }
        }

        public override string GetDbSpecificColumnType()
        {
            return GetProject().CollectedData.GetDatabase().GetDbSpecificColumnType(GenericDbColumnType.Image);
        }
		#endregion Public Properties

		#region Protected Properties
		#endregion Protected Properties

		#region Public Methods
		/// <summary>
		/// Deletes the field
		/// </summary>
		public override void Delete()
		{
            GetMetadata().DeleteField(this);
            view.MustRefreshFieldCollection = true;
		}
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

	}
}