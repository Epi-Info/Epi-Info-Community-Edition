#region Namespaces
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Xml;
using Epi;
#endregion

namespace Epi.Fields
{
    /// <summary>
    /// 
    /// </summary>
	public class GroupField : FieldWithoutSeparatePrompt
	{
		#region Private Members
        private XmlElement viewElement;
        private XmlNode fieldNode;
        private System.Drawing.Color backgroundColor;
        private string childFieldNames;
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;
		#endregion Private Members

		#region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page the field belongs to</param>
		public GroupField(Page page) : base(page)
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page the field belongs to</param>
        public GroupField(DataRow row, Page page)
            : base(page)
        {
            LoadFromRow(row);
            this.Page = page;
        }
		
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page the field belongs to</param>
        /// <param name="viewElement">Xml Element of a GroupField.</param>
        public GroupField(Page page, XmlElement viewElement)
            : base(page)
        {
            this.viewElement = viewElement;
            this.Page = page;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view the field belongs to</param>
		public GroupField(View view) : base(view)
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view the field belongs to</param>
        /// <param name="fieldNode">Xml Node representation of a GroupField.</param>
        public GroupField(View view, XmlNode fieldNode)
            : base(view)
        {
            this.fieldNode = fieldNode;
            //this.view.Project.Metadata.GetFieldData(this, this.fieldNode);
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
                return MetaFieldType.Group;
            }
        }

        /// <summary>
        /// Gets/sets the background color.
        /// </summary>
        public System.Drawing.Color BackgroundColor
        {
            get
            {
                if (backgroundColor.IsEmpty == true)
                {
                    return SystemColors.Window;
                }
                else
                {
                    return backgroundColor;
                }
            }
            set
            {
                backgroundColor = value;
            }
        }
        public string ChildFieldNames
        {
            get
            {
                childFieldNames = childFieldNames == null ? string.Empty : childFieldNames;
                return childFieldNames;
            }
            set { childFieldNames = value; }
        }

        public string[] ChildFieldNameArray
        {
            get
            {
                childFieldNames = childFieldNames == null ? string.Empty : childFieldNames;
                return childFieldNames.Split(new char[] { ',' }); ;
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
        /// Load from row
        /// </summary>
        /// <param name="row">Row</param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
            if (row["BackgroundColor"].ToString().Length > 0)
            {
                this.BackgroundColor = Color.FromArgb((int)(row["BackgroundColor"]) + unchecked((int)0xFF000000)); 
            }
            if (row["List"].ToString().Length > 0)
            {
                this.childFieldNames = row["List"].ToString();
            }
        }

        public GroupField Clone()
        {
            GroupField clone = (GroupField)this.MemberwiseClone();
            base.AssignMembers(clone);
            return clone;
        }

        public bool HasChildNamed(String possibleChildName)
        {
            foreach (string childName in (String[])ChildFieldNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (childName.Equals(possibleChildName)) return true;
            }
            return false;
        }
        
        /// <summary>
        /// Gets the name of the Field Group.
        /// </summary>
        /// <returns>Name of the field group.</returns>
        public override string ToString()
        {
            return Name;
        }
        
        /// <summary>
        /// Delete
        /// </summary>
		public override void Delete()
		{
            GetMetadata().DeleteField(this);
            Page.GroupFields.Remove(this);
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
