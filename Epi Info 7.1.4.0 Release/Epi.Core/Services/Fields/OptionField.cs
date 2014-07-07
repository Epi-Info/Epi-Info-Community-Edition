using System;
using System.ComponentModel;
using System.Data;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Xml;
using Epi;
using Epi.Collections;
using Epi.Data;
using Epi.Data.Services;

namespace Epi.Fields
{
    /// <summary>
    /// Option Field
    /// </summary>
    public class OptionField : InputFieldWithoutSeparatePrompt, IFieldWithCheckCodeAfter, IFieldWithCheckCodeClick
    {
        #region Private Class Members

        private XmlElement viewElement;
        private XmlNode fieldNode;
        private bool showTextOnRight = true;
        private string checkCodeAfter = string.Empty;
        private string checkCodeClick = string.Empty;
        private string pattern = string.Empty;
        private string locations = string.Empty;
        private List<string> options = null;
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;

        #endregion

        #region Public Events
        #endregion

        #region Constructors
        /// <summary>
        /// OptionField Constructor
        /// </summary>
        /// <param name="page"><see cref="Epi.View"/> <see cref="Epi.Page"/> object.</param>
        public OptionField(Page page)
            : base(page)
        {
            genericDbColumnType = GenericDbColumnType.Int16;
            dbColumnType = DbType.Int16;
        }
        /// <summary>
        /// OptionField Constructor
        /// </summary>
        /// <param name="view"><see cref="Epi.Project"/> <see cref="Epi.View"/> object.</param>
        public OptionField(View view)
            : base(view)
        {
            genericDbColumnType = GenericDbColumnType.Int16;
            dbColumnType = DbType.Int16;
        }
        /// <summary>
        /// OptionField Constructor
        /// </summary>
        /// <param name="page"><see cref="Epi.View"/> <see cref="Epi.Page"/> object.</param>
        /// <param name="viewElement">View data represented as an XML element.</param>
        public OptionField(Page page, XmlElement viewElement)
            : base(page)
        {
            this.viewElement = viewElement;
            this.Page = page;
        }
        /// <summary>
        /// OptionField Constructor
        /// </summary>
        /// <param name="view"><see cref="Epi.Project"/> <see cref="Epi.View"/> object.</param>
        /// <param name="fieldNode">OptionField data represented as an XML node.</param>
        public OptionField(View view, XmlNode fieldNode)
            : base(view)
        {
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, this.fieldNode);
        }
        /// <summary>
        /// Load OptionField data from data row.
        /// </summary>
        /// <param name="row">A row of data in a <see cref="System.Data.DataTable"/>.</param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
            showTextOnRight = (bool)row["ShowTextOnRight"];

            pattern = string.Empty;

            if (row["Pattern"] is string)
            {
                pattern = (string)row["Pattern"];
            }

            string list = row["List"].ToString();
            if (list.Contains("||"))
            { 
                list = list.Substring(0, list.IndexOf("||"));
            }
            string[] listArray = (list).Split(',');
            this.options = new List<string>(listArray);
        }

        public OptionField Clone()
        {
            OptionField clone = (OptionField)this.MemberwiseClone();
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
                return MetaFieldType.Option;
            }
        }

        /// <summary>
        /// Returns a fully-typed current record value
        /// </summary>
        public string CurrentRecordValue
        {
            get
            {
                if (base.CurrentRecordValueObject == null) return string.Empty;
                else return CurrentRecordValueObject.ToString();
            }
            set
            {
                base.CurrentRecordValueObject = value;
            }
        }

        /// <summary>
        /// Gets/sets the Show Text on Right flag.
        /// </summary>
        public bool ShowTextOnRight
        {
            get
            {
                return (showTextOnRight);
            }
            set
            {
                showTextOnRight = value;
            }
        }

        /// <summary>
        /// Gets/sets the after Check Code.
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
        /// Gets/sets the click Check Code.
        /// </summary>
        public string CheckCodeClick
        {
            get
            {
                return (checkCodeClick);
            }
            set
            {
                checkCodeClick = value;
            }
        }

        public string Pattern
        {
            get { return pattern; }
            set { pattern = value; }
        }

        public string Locations
        {
            get { return locations; }
            set { locations = value; }
        }

        /// <summary>
        /// Gets/sets the options collection
        /// </summary>
        public List<string> Options
        {
            get
            {
                if (options == null)
                {
                    return new List<string>();
                }
                else
                {
                    return options;
                }
            }
            set
            {
                this.options = value;
            }
        }
        #endregion

        #region Protected Properties
        #endregion Protected Properties

        #region Public Methods

        public override string GetDbSpecificColumnType()
        {
            return GetProject().CollectedData.GetDatabase().GetDbSpecificColumnType(GenericDbColumnType.Int16);
        }

        /// <summary>
        /// Deletes the field
        /// </summary>
        public override void Delete()
        {
            GetMetadata().DeleteField(this);
            view.MustRefreshFieldCollection = true;
        }

        public string GetOptionsString()
        {
            string optionsString = string.Empty;
            foreach (string option in Options)
            {
                optionsString = optionsString + option + ',';
            }
            
            optionsString = optionsString.TrimEnd(new char[] { ',' });
            optionsString = optionsString + "||" + locations;
            
            return optionsString;
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

        #endregion Protected Methods

        #region Event Handlers
        #endregion Event Handlers
    }
}
