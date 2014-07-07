#region Namespaces

using System;
using System.ComponentModel;
using System.Data;
using System.Xml;
using Epi;

#endregion  //Namespaces

namespace Epi.Fields
{
	/// <summary>
	/// Mirror Field.
	/// </summary>
	public class MirrorField : FieldWithSeparatePrompt, IDependentField
	{
		#region Private Members
        /// <summary>
        /// The Xml view element of the MirrorField.
        /// </summary>
        private XmlElement viewElement;
        /// <summary>
        /// Xml Node representation of ImageField.
        /// </summary>
        private XmlNode fieldNode;
        /// <summary>
        /// sourceFieldId - the field Id of control being mirrored.
        /// </summary>
        private int sourceFieldId;
        /// <summary>
        /// IDataField - the field being mirrored.
        /// </summary>
        private IDataField sourceField;
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;
		
        #endregion Private Members

		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="page">The page this field belongs to</param>
		public MirrorField(Page page) : base(page)
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
		public MirrorField(View view) : base(view)
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="viewElement">Xml view element</param>
        public MirrorField(Page page, XmlElement viewElement) : base(page)
        {
            this.viewElement = viewElement;
            this.Page = page;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="fieldNode">Xml field node</param>
        public MirrorField(View view, XmlNode fieldNode) : base(view)
        {
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
            if (!string.IsNullOrEmpty(row["SourceFieldId"].ToString()))
            {
                sourceFieldId = int.Parse(row["SourceFieldId"].ToString());
            }
        }

        public MirrorField Clone()
        {
            MirrorField clone = (MirrorField)this.MemberwiseClone();
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
                return MetaFieldType.Mirror;
            }
        }

        /// <summary>
        /// Source Field Id
        /// </summary>
		public int SourceFieldId // Implements IDependentField.SourceFieldId
		{
			get
			{
				return sourceFieldId;
			}
			set
			{
				sourceFieldId = value;
			}
		}

        /// <summary>
        /// Source Field
        /// </summary>
		public IDataField SourceField // Implements IDependentField.SourceField
		{
			get
			{
                View view = Page.GetView();
				if ((sourceField == null) && (this.SourceFieldId > 0))
				{
                    Field field = view.GetFieldById(this.SourceFieldId);
                    if (field is IDataField)
                    {
                        sourceField = (IDataField)field;
                    }
               }
				return (sourceField) ;
			}
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

		#endregion Public Methods

		#region protected Methods

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
		
		#endregion protected Methods

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
