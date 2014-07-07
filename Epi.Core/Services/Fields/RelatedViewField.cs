#region Namespaces

using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Xml;
using Epi;

#endregion  //Namespaces

namespace Epi.Fields
{
    /// <summary>
    /// Related view field
    /// </summary>
    public class RelatedViewField : FieldWithoutSeparatePrompt, IFieldWithCheckCodeClick //RenderableField
    {
        #region Private Members

        private XmlElement viewElement;
        private XmlElement fieldElement;
        private XmlNode fieldNode;

        private string condition = string.Empty;
        private bool shouldReturnToParent;
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;
        #endregion Private Members

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page the field belongs to</param>
        public RelatedViewField(Page page)
            : base(page)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view the field belongs to</param>
        public RelatedViewField(View view)
            : base(view)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page the field belongs to</param>
        /// <param name="viewElement">Xml Element representation of Related View Field</param>
        public RelatedViewField(Page page, XmlElement viewElement)
            : base(page)
        {
            this.viewElement = viewElement;
            this.Page = page;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view the field belongs to</param>
        /// <param name="fieldNode"></param>
        public RelatedViewField(View view, XmlNode fieldNode)
            : base(view)
        {
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, this.fieldNode);
        }

        /// <summary>
        /// Load a Related View Field from a <see cref="System.Data.DataRow"/>
        /// </summary>
        /// <param name="row"></param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
            condition = row["RelateCondition"].ToString();
            //checkCodeBefore = row["ControlBeforeCheckCode"].ToString();
            //checkCodeAfter = row["ControlAfterCheckCode"].ToString();
            shouldReturnToParent = (bool)row["ShouldReturnToParent"];
            if (row["RelatedViewId"] != System.DBNull.Value)
            {
                relatedViewID = (int)row["RelatedViewId"];
            }
        }

        public RelatedViewField Clone()
        {
            RelatedViewField clone = (RelatedViewField)this.MemberwiseClone();
            base.AssignMembers(clone);
            return clone;
        }
        #endregion

        #region Public Events

        /// <summary>
        /// Occurs when a user requests to see the related view of this field
        /// </summary>
        public event ChildViewRequestedEventHandler ChildViewRequested;

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns field type
        /// </summary>
        public override MetaFieldType FieldType
        {
            get
            {
                return MetaFieldType.Relate;
            }
        }

        /// <summary>
        /// Gets the related view of the field
        /// </summary>
        public View ChildView
        {
            get
            {
                return GetMetadata().GetChildView(this);
            }
        }

        /// <summary>
        /// Gets/sets the condition for going to related view
        /// </summary>
        public string Condition
        {
            get
            {
                return (condition);
            }
            set
            {
                condition = value;
            }
        }

        /// <summary>
        /// Gets/sets whether the related view should return to its parent
        /// </summary>
        public bool ShouldReturnToParent
        {
            get
            {
                return (shouldReturnToParent);
            }
            set
            {
                shouldReturnToParent = value;
            }
        }

        private int relatedViewID;
        /// <summary>
        /// Id of view list as related to this field
        /// </summary>
        public int RelatedViewID
        {
            get { return relatedViewID; }
            set { relatedViewID = value; }
        }

        //isu6 - Implementing checkcode for Related view field.
        /// <summary>
        /// CheckCode After property for all DropDown list fields
        /// </summary>
        public string CheckCodeAfter
        {
            get
            {
                return (checkCodeAfter);
            }
            set
            {
                checkCodeAfter = value;
            }
        }

        /// <summary>
        /// CheckCode Before property for all DropDown list fields
        /// </summary>
        public string CheckCodeClick
        {
            get
            {
                return (checkCodeBefore);
            }
            set
            {
                checkCodeBefore = value;
            }
        }


        #endregion

        #region Protected Properties
        /// <summary>
        /// Check Code After member variable
        /// </summary>
        protected string checkCodeAfter = string.Empty;
        /// <summary>
        /// Check Code Before member variable
        /// </summary>
        protected string checkCodeBefore = string.Empty;
        
        #endregion Protected Properties

        #region Public Methods

        /// <summary>
        /// Deletes the field
        /// </summary>
        public override void Delete()
        {
            View childView = this.GetProject().GetViewById(RelatedViewID);

            if (childView != null)
            {
                childView.IsRelatedView = false;
                GetMetadata().UpdateView(childView);
            }
            
            GetMetadata().DeleteField(this);
            view.MustRefreshFieldCollection = true;
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

        #region Event Handlers

        /// <summary>
        /// Handles the click event of the Edit Field menu item
        /// </summary>
        /// <param name="sender">.NET supplied object</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void MnuEditRelate_Click(object sender, EventArgs e)
        {
            //base.RaiseEventFieldDefinitionRequested();
        }

        /// <summary>
        /// Handles the click event of the Related View menu item
        /// </summary>
        /// <param name="sender">.NET supplied object</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void MnuRelatedView_Click(object sender, EventArgs e)
        {
            if (ChildViewRequested != null)
            {
                ChildViewRequested(this, new ChildViewRequestedEventArgs(ChildView));
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Creates an Xml attribute and sets a value.
        /// (NOTE: fieldElement must be set to viewElement.OwnerDocument.CreateElement("Field").)
        /// </summary>
        /// <param name="Attribute">System.Xml.XmlAttribute.</param>
        /// <param name="Value">Sets the value of the node.</param>
        private void AppendAttributeValue(string Attribute, string Value)
        {
            try
            {
                XmlAttribute xmlAttribute = viewElement.OwnerDocument.CreateAttribute(Attribute);
                xmlAttribute.Value = Value;
                if (fieldElement == null) fieldElement = viewElement.OwnerDocument.CreateElement("Field");
                fieldElement.Attributes.Append(xmlAttribute);

            }
            catch (ArgumentException ex)
            {
                
                throw new GeneralException(SharedStrings.EXCEPTION_OCCURRED, ex);
            }
        }

        #endregion  //Private Methods






    }
}
