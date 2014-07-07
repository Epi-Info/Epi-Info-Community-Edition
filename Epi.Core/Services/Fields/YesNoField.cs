using System;
using System.ComponentModel;
using System.Data;
using System.Xml;
using Epi.Data;
using Epi;

namespace Epi.Fields
{
    /// <summary>
    /// Class YesNoField
    /// </summary>
    public class YesNoField : DropDownField, IFieldWithCheckCodeAfter, IFieldWithCheckCodeBefore
    {
        #region Private Data Members

        private XmlElement viewElement;
        private XmlNode fieldNode;
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;

        /// <summary>
        /// Check Code After
        /// </summary>
        protected string checkCodeAfter = string.Empty;
        
        /// <summary>
        /// Check Code Before
        /// </summary>
        protected string checkCodeBefore = string.Empty;

        #endregion  //Private Data Members

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page the field belongs to</param>
        public YesNoField(Page page) : base(page)
		{
			Construct();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page the field belongs to</param>
        /// <param name="viewElement">Xml view element</param>
        public YesNoField(Page page, XmlElement viewElement)
            : base(page, viewElement)
        {
            this.viewElement = viewElement;
            this.Page = page;
            Construct();
        }  

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
		public YesNoField(View view) : base(view)
		{
			Construct();
		}

        /// <summary>
        /// Load from row
        /// </summary>
        /// <param name="row">Row</param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
        }

        public YesNoField Clone()
        {
            YesNoField clone = (YesNoField)this.MemberwiseClone();
            base.AssignMembers(clone);
            return clone;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="fieldNode">Xml field node</param>
        public YesNoField(View view, XmlNode fieldNode) : base(view)
        {
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, fieldNode);
        }

		private void Construct()
		{
            genericDbColumnType = GenericDbColumnType.Byte;
            this.dbColumnType = DbType.Byte;
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
                return MetaFieldType.YesNo;
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
		#endregion     

        #region Members

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
        public string CheckCodeBefore
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

        public override string GetDbSpecificColumnType()
        {
            return GetProject().CollectedData.GetDatabase().GetDbSpecificColumnType(GenericDbColumnType.Byte);
        }
	}
}
