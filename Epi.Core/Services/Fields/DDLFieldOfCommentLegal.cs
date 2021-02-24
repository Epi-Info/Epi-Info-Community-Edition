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
	/// Legal values Field
	/// </summary>
	public class DDLFieldOfCommentLegal : TableBasedDropDownField
    {
        #region Private Data Members

        private XmlElement viewElement;
        private XmlNode fieldNode;
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;

        #endregion //Private Data Members

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page</param>
        public DDLFieldOfCommentLegal(Page page) : base(page)
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view</param>
		public DDLFieldOfCommentLegal(View view) : base(view)
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page</param>
        /// <param name="viewElement">XML view element</param>
        public DDLFieldOfCommentLegal(Page page, XmlElement viewElement)
            : base(page)
        {
            this.viewElement = viewElement;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view</param>
        /// <param name="fieldNode">XML field node</param>
        public DDLFieldOfCommentLegal(View view, XmlNode fieldNode)
            : base(view)
        {
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, this.fieldNode);
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
                return MetaFieldType.CommentLegal;
            }
        }

        /// <summary>
        /// Comment
        /// </summary>
        public override object CurrentRecordValueObject
        {
            get
            {
                if (base.currentRecordValueObject == null)
                {
                    return string.Empty;
                }
                else
                {
                    return base.currentRecordValueObject;
                }
            }
            set
            {
                base.currentRecordValueObject = value;
            }
        }

        /// <summary>
        /// Comment - Long Name
        /// </summary>
        public override string CurrentRecordValueString
        {
            get
            {
                if (base.currentRecordValueObject == null || base.currentRecordValueObject is System.DBNull || string.IsNullOrEmpty((string)base.currentRecordValueObject) == true)
                {
                    return string.Empty;
                }
                else
                {
                    return ((string)base.currentRecordValueObject).Trim();
                }
            }
            set
            {
                int firstHyphenIndex = value.Contains("-") ? value.IndexOf('-') : value.Length;
                string comment = value.Substring(0, (value.Length - (value.Length - firstHyphenIndex)));
                base.currentRecordValueObject = comment.Trim();
            }
        }

        #endregion Public Properties

		#region Public Methods

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
        
		#endregion

		#region Event Handlers
		#endregion Event Handlers

		#region Protected Methods

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