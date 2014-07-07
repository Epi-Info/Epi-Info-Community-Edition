#region Namespaces

using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Xml;
using Epi;
using System.Collections.Generic;
using Epi.Core;
using System.Windows.Forms;
#endregion

namespace Epi.Fields
{
    /// <summary>
    /// Legal values Field
    /// </summary>
    public class DDLFieldOfCodes : TableBasedDropDownField
    {
        #region Private Members

        private XmlElement viewElement;
        private XmlNode fieldNode;
        private string associatedFieldInformation = string.Empty;
        private string sourceTableName;
        private string fieldName;
        private DataTable codeTable;
        private Dictionary<string, int> pairAssociated = new Dictionary<string, int>();
        private Dictionary<string, Control> observerControls = new Dictionary<string, Control>();
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;
        private string _cascadeFilter;

        #endregion Private Members

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page</param>
        public DDLFieldOfCodes(Page page)
            : base(page)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view</param>
        public DDLFieldOfCodes(View view)
            : base(view)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page</param>
        /// <param name="viewElement">XML View element</param>
        public DDLFieldOfCodes(Page page, XmlElement viewElement)
            : base(page)
        {
            this.viewElement = viewElement;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view</param>
        /// <param name="fieldNode">XML field node</param>
        public DDLFieldOfCodes(View view, XmlNode fieldNode)
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
                return MetaFieldType.Codes;
            }
        }

        /// <summary>
        /// Returns field name
        /// </summary>
        public String FieldName
        {
            get
            {
                return fieldName;
            }
        }

        public String CascadeFilter
        {
            get
            {
                return _cascadeFilter;
            }
            set
            {
                _cascadeFilter = value;
            }
        }


        /// <summary>
        /// Associated Field Information
        /// </summary>
        public string AssociatedFieldInformation
        {
            get
            {
                return associatedFieldInformation;
            }
            set
            {
                associatedFieldInformation = value;
                string[] pairValueString = associatedFieldInformation.Split(',');
                pairAssociated.Clear();

                foreach (string pair in pairValueString)
                {
                    if (!(String.IsNullOrEmpty(pair)))
                    {
                        string[] name_id = pair.Split(':');

                        string name = name_id[0];
                        int fieldId = Convert.ToInt16(name_id[1]);

                        pairAssociated.Add(name, fieldId);
                    }
                }
            }
        }

        /// <summary>
        /// Associated Field Information
        /// </summary>
        public string RelateConditionString
        {
            get
            {
                string condition = string.Empty;

                foreach(System.Collections.Generic.KeyValuePair<string,int> kvp in pairAssociated)
                {
                    condition = string.Format("{0}{1}:{2},", condition, kvp.Key, kvp.Value.ToString());
                }

                return condition.TrimEnd(',');
            }
        }

        public Dictionary<string, int> PairAssociated
        {
            get { return pairAssociated; }
        }

        /// <summary>
        /// Control Factory
        /// </summary>
        public IControlFactory ControlFactory { get; set; }

        #endregion Public Properties

        #region Public Methods

        public DDLFieldOfCodes Clone()
        {
            DDLFieldOfCodes clone = (DDLFieldOfCodes)this.MemberwiseClone();
            base.AssignMembers(clone);
            return clone;
        }
        
        /// <summary>
        /// Load From Row
        /// </summary>
        /// <param name="row">DataRow</param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
            sourceTableName = row[ColumnNames.SOURCE_TABLE_NAME].ToString();
            string relateCondition = row[ColumnNames.RELATE_CONDITION].ToString();
            fieldName = row[ColumnNames.TEXT_COLUMN_NAME].ToString().Trim();
            string[] words = relateCondition.Split(',');
            foreach (string word in words)
            {
                if (!(String.IsNullOrEmpty(word)))
                {
                    string[] pair = word.Split(':');
                    this.pairAssociated.Add(pair[0], Convert.ToInt16(pair[1]));
                }
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
        private void Combobox_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        #endregion Event Handlers

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