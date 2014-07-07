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
	/// Command button field
	/// </summary>
	public class CommandButtonField : FieldWithoutSeparatePrompt, IFieldWithCheckCodeClick
	{
		#region Private Members
		private string checkCodeClick = string.Empty;
        private string checkCodeAfter = string.Empty;
        private XmlElement viewElement;
        private XmlNode fieldNode;
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;
		#endregion

		#region Public Events
		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for the class
		/// </summary>
		/// <param name="page">The page this field belongs to</param>
        public CommandButtonField(Page page) : base(page)
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page the field belongs to</param>
        /// <param name="viewElement">The Xml Element of the CommandButton field.</param>
        public CommandButtonField(Page page, XmlElement viewElement)
            : base(page)
        {
            this.viewElement = viewElement;
            this.Page = page;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view the field belongs to</param>
		public CommandButtonField(View view) : base(view)
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view the field belongs to</param>
        /// <param name="fieldNode">The Xml Node representation of the fie</param>
        public CommandButtonField(View view, XmlNode fieldNode) : base(view)
        {
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, this.fieldNode);
        }

        /// <summary>
        /// Load Command Button field from a <see cref="System.Data.DataRow"/>
        /// </summary>
        /// <param name="row">Row of Command Button field data.</param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
            //checkCodeClick = row["ControlAfterCheckCode"].ToString();
        }

        public CommandButtonField Clone()
        {
            CommandButtonField clone = (CommandButtonField)this.MemberwiseClone();
            base.AssignMembers(clone);
            return clone;
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
                return MetaFieldType.CommandButton;
            }
        }


		/// <summary>
		/// Gets/sets the field's "after" check code when the field is created
		/// </summary>
		public string CheckCodeClick
		{
			get
			{
				return checkCodeClick;
			}
			set
			{
				checkCodeClick = value;
			}
		}

        /// <summary>
        /// Gets/Sets the field's "after" check code when the field 
        /// has been selected and is being parsed
        /// </summary>
        public string CheckCodeAfter
        {
            get
            {
                return checkCodeClick;
            }
            set
            {
                checkCodeClick = value;
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
		private void Control_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(CheckCodeClick))
			{
//				View.RunCheckCode(CheckCodeClick);
			}
		}
		#endregion      

	

    }
}
