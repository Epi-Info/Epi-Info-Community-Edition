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
    /// 
    /// </summary>
	public class LabelField : FieldWithoutSeparatePrompt
	{

		#region Private Members
		// private DragableLabel label;
        private XmlElement viewElement;
        private XmlNode fieldNode;
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;
		#endregion Private Members

		#region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page the field belongs to</param>
		public LabelField(Page page) : base(page)
		{
		}
		
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page the field belongs to</param>
        /// <param name="viewElement">Xml Element of a LabelField.</param>
        public LabelField(Page page, XmlElement viewElement)
            : base(page)
        {
            this.viewElement = viewElement;
            this.Page = page;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view the field belongs to</param>
		public LabelField(View view) : base(view)
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view the field belongs to</param>
        /// <param name="fieldNode">Xml Node representation of a LabelField.</param>
        public LabelField(View view, XmlNode fieldNode)
            : base(view)
        {
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, this.fieldNode);
        }

        public LabelField Clone()
        {
            LabelField clone = (LabelField)this.MemberwiseClone();
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
                return MetaFieldType.LabelTitle;
            }
        }
        #endregion Public Properties

        #region Protected Properties

        #endregion Protected Properties      

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
		
		#endregion  //Protected Methods

		#region Public Methods
        /// <summary>
        /// Delete
        /// </summary>
		public override void Delete()
		{
            GetMetadata().DeleteField(this);
            view.MustRefreshFieldCollection = true;
		}
		#endregion  //Public Methods

		#region Event Handlers		
		#endregion

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
