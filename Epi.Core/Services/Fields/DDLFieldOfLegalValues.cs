using System;
using System.ComponentModel;
using System.Data;
using System.Xml;
using Epi;

namespace Epi.Fields
{
	/// <summary>
	/// Legal values Field
	/// </summary>
	public class DDLFieldOfLegalValues : TableBasedDropDownField
    {
        #region Private Data Members

        private XmlElement viewElement;
        private XmlNode fieldNode;
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;

        #endregion  //Private Data Members

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page</param>
        public DDLFieldOfLegalValues(Page page) : base(page)
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view</param>
		public DDLFieldOfLegalValues(View view) : base(view)
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page</param>
        /// <param name="viewElement">Xml view element</param>
        public DDLFieldOfLegalValues(Page page, XmlElement viewElement)
            : base(page)
        {
            this.viewElement = viewElement;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view</param>
        /// <param name="fieldNode">Xml field node</param>
        public DDLFieldOfLegalValues(View view, XmlNode fieldNode)
            : base(view)
        {
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, this.fieldNode);
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
                return MetaFieldType.LegalValues;
            }
        }

        /// <summary>
        /// Column name of the code
        /// </summary>
		public override string CodeColumnName
		{
			get
			{
				if (string.IsNullOrEmpty(codeColumnName))
				{
					codeColumnName = TextColumnName;
				}
				return codeColumnName;
			}
			set
			{
				base.CodeColumnName = value;
			}
		}

        /// <summary>
        /// Column name
        /// </summary>
		public override string TextColumnName
		{
			get
			{
				return base.TextColumnName;
			}
			set
			{
				if (string.IsNullOrEmpty(codeColumnName))
				{
					codeColumnName = value;
				}
				base.TextColumnName = value;
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

		///// <summary>
		///// Saves the current field location
		///// </summary>
        //public override void SaveFieldLocation()
        //{
        //    Metadata.UpdateControlPosition(this);
        //    Metadata.UpdatePromptPosition(this);
        //}

		#endregion

		#region Protected Methods

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

		#endregion Protected Methods
    }
}