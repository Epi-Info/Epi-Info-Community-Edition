using System;
using System.ComponentModel;
using System.Data;
using System.Xml;
using Epi;
using Epi.Data;

namespace Epi.Fields
{	
	/// <summary>
	/// Check box field
	/// </summary>
	public class CheckBoxField : InputFieldWithoutSeparatePrompt, IFieldWithCheckCodeAfter, IFieldWithCheckCodeBefore, IFieldWithCheckCodeClick
	{
		#region Private Members

        private string checkCodeBefore = string.Empty;
		private string checkCodeAfter = string.Empty;
        private string checkCodeClick = string.Empty;
        private string checkCodeValue = string.Empty;
        private XmlElement viewElement;
        private XmlNode fieldNode;
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;
        private bool _boxOnRight = false;
		// private DragableCheckBox checkbox;

		#endregion  //Private Members

		#region Public Events
		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for the class
		/// </summary>
		/// <param name="page">The page this field belongs to</param>
		public CheckBoxField(Page page) : base(page)
		{
			Construct();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page the field belongs to</param>
        /// <param name="viewElement"></param>
        public CheckBoxField(Page page, XmlElement viewElement)
            : base(page)
        {
            this.viewElement = viewElement;
            this.Page = page;
            Construct();
        }
		
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view the field belongs to</param>
		public CheckBoxField(View view) : base(view)
		{            
			Construct();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view the field belongs to</param>
        /// <param name="fieldNode">Xml Node representation of the CheckBoxField</param>
        public CheckBoxField(View view, XmlNode fieldNode) : base(view)
        {
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, this.fieldNode);
            Construct();
        }

		private void Construct()
		{
            genericDbColumnType = GenericDbColumnType.Boolean;
            this.dbColumnType = DbType.Boolean;
			IsControlResizable = false;
		}

        /// <summary>
        /// Load CheckBoxField from a System.Data.DataRow.
        /// </summary>
        /// <param name="row">Data Row</param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);

            object pattern = row["Pattern"];

            if (pattern != null && (pattern is System.DBNull == false) && pattern.ToString() == "BoxOnRight")
            {
                _boxOnRight = true;
            }
        }

        public CheckBoxField Clone()
        {
            CheckBoxField clone = (CheckBoxField)this.MemberwiseClone();
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
                return MetaFieldType.Checkbox;
            }
        }

        /// <summary>
        /// Returns a fully-typed current record value
        /// </summary>
        public bool CurrentRecordValue
        {
            get
            {
                if (base.CurrentRecordValueObject == null) return false;
                else return (bool)base.CurrentRecordValueObject;
            }
            set
            {
                base.CurrentRecordValueObject = value;
            }
        }

		/// <summary>
		/// Value of the check box. This property is available only in DataEntry mode.
		/// </summary>
		public override string CurrentRecordValueString
		{
			get
			{
				Page.AssertDataEntryMode();          
                return checkCodeValue;
			}
			set
			{
                checkCodeValue = value;
			}
		}

        public bool BoxOnRight
        {
            get { return _boxOnRight; }
            set { _boxOnRight = value; }
        }

		/// <summary>
		/// Gets/sets the field's "after" check code
		/// </summary>
		public string CheckCodeAfter
		{
			get
			{
				return checkCodeAfter;
			}
			set
			{
				checkCodeAfter = value;
			}
		}


        /// <summary>
        /// Gets/sets the field's "before" check code
        /// </summary>
        public string CheckCodeBefore
        {
            get
            {
                return checkCodeBefore;
            }
            set
            {
                checkCodeBefore = value;
            }
        }

        /// <summary>
        /// Gets/sets the field's "click" check code
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

	}
}