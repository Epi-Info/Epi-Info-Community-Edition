using System;
using System.ComponentModel;
using System.Data;
using System.Xml;
using Epi;
using Epi.Data;

namespace Epi.Fields
{
	/// <summary>
	/// Multiline text field
	/// </summary>
	public class MultilineTextField : TextField
    {
        #region Private Data Members

        private XmlElement viewElement;
        private XmlNode fieldNode;
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;

        #endregion  //Private Data Members

        #region Constructors

        private void Construct()
        {
            genericDbColumnType = GenericDbColumnType.StringLong;
        }

        public MultilineTextField(Page page) : base(page)
		{
			Construct();
		}

        public MultilineTextField(View view) : base(view)
        {
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page this field belongs to</param>
        /// <param name="viewElement">Xml view element</param>
        public MultilineTextField(Page page, XmlElement viewElement)
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
        public MultilineTextField(View view, XmlNode fieldNode) : base(view)
        {
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, this.fieldNode);
            Construct();
        }

		#endregion

        #region Public Properties

        /// <summary>
        /// Field Type
        /// </summary>
        public override MetaFieldType FieldType
        {
            get
            {
                return MetaFieldType.Multiline;
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

        #region Public Methods
        /// <summary>
        /// Gets the SQL Data type
        /// </summary>
        public override string GetDbSpecificColumnType()
        {
            return GetProject().CollectedData.GetDatabase().GetDbSpecificColumnType(genericDbColumnType);
        }

        public MultilineTextField Clone()
        {
            MultilineTextField clone = (MultilineTextField)this.MemberwiseClone();
            base.AssignMembers(clone);
            return clone;
        }
        #endregion Public Methods
    }
}