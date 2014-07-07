
#region Imported Namespaces

using Epi;
using Epi.Data;
using Epi.Fields;

using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Xml;

#endregion

namespace Epi.Fields
{
    /// <summary>
    /// Provides a GUID field in a view
    /// </summary>
    public class GUIDField : TextField, IMirrorable
    {
        #region Private Members

        private const bool isReadOnly = true;
        private const int maxLength = 38;

        private Guid currentValue;
        private XmlNode fieldNode;
        private int sourceFieldId;
        private XmlElement viewElement;
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">Page</param>
        public GUIDField(Page page)
            : base(page)
        {
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        public GUIDField(View view)
            : base(view)
        {
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page this field belongs to</param>
        /// <param name="viewElement">Xml view element</param>
        public GUIDField(Page page, XmlElement viewElement)
            : base(page)
        {
            this.viewElement = viewElement;
            this.Page = page;
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="fieldNode">Xml field node</param>
        public GUIDField(View view, XmlNode fieldNode)
            : base(view)
        {
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, fieldNode);
            Construct();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns field type
        /// </summary>
        public override MetaFieldType FieldType
        {
            get
            {
                return MetaFieldType.GUID;
            }
        }

        /// <summary>
        /// Returns a fully-typed current record value
        /// </summary>
        public new string CurrentRecordValue
        {
            get
            {
                string RetVal = string.Empty;

                if (base.CurrentRecordValueObject == null)
                {
                    RetVal = Guid.NewGuid().ToString().Replace(StringLiterals.CURLY_BRACE_LEFT, string.Empty).Replace(StringLiterals.CURLY_BRACE_RIGHT, string.Empty);
                }
                else
                {
                    RetVal = base.CurrentRecordValueObject.ToString().Replace(StringLiterals.CURLY_BRACE_LEFT, string.Empty).Replace(StringLiterals.CURLY_BRACE_RIGHT, string.Empty);
                }

                return RetVal;
            }
            set
            {
                base.CurrentRecordValueObject = value;
            }
        }

        /// <summary>
        /// Returns IsReadOnly
        /// </summary>
        public new bool IsReadOnly
        {
            get
            {
                return (isReadOnly);
            }
        }

        /// <summary>
        /// Returns MaxLength
        /// </summary>
        public new int MaxLength
        {
            get
            {
                return (maxLength);
            }
        }

        /// <summary>
        /// Returns Source Field Id
        /// </summary>
        public new int SourceFieldId
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns specific data type for the target DB.
        /// </summary>
        /// <returns></returns>
        public override string GetDbSpecificColumnType()
        {
            return base.GetDbSpecificColumnType();
        }

        /// <summary>
        /// Allows the GUIDField type to generate a new GUID
        /// </summary>
        /// <returns></returns>
        public Guid NewGuid()
        {
            currentValue = Guid.NewGuid();
            CurrentRecordValue = currentValue.ToString();
            return currentValue;
        }

        /// <summary>
        /// Allows the GUIDField type to generate a new GUID
        /// </summary>
        /// <returns></returns>
        public string SampleGuid()
        {
            return "7cb780b9-20dc-425c-88df-d6d27e13d566";
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Used to construct the field object
        /// </summary>
        private void Construct()
        {
            genericDbColumnType = GenericDbColumnType.Guid;
            this.dbColumnType = DbType.Guid;
            this.currentValue = Guid.NewGuid();
            base.CurrentRecordValue = currentValue.ToString().Replace(StringLiterals.CURLY_BRACE_LEFT, string.Empty).Replace(StringLiterals.CURLY_BRACE_RIGHT, string.Empty);
        }

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

        /// <summary>
        /// Load from row
        /// </summary>
        /// <param name="row">Row</param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
            if (row["SourceFieldId"].ToString().Length > 0)
            {
                sourceFieldId = int.Parse(row["SourceFieldId"].ToString());
            }
        }

        public GUIDField Clone()
        {
            GUIDField clone = (GUIDField)this.MemberwiseClone();
            base.AssignMembers(clone);
            return clone;
        }

        ///// <summary>
        ///// Update the field to the database
        ///// </summary>
        //protected override void UpdateField()
        //{
        //    _updater = new BackgroundWorker();
        //    _updater.DoWork += new DoWorkEventHandler(DoWork);
        //    _updater.RunWorkerAsync();
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
    }
}
