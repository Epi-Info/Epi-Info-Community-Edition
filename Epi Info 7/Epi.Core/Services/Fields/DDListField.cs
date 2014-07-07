#region Namespaces

using System;
using System.ComponentModel;
using System.Data;
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
    public class DDListField : TableBasedDropDownField
    {
        #region Private Members

        private XmlElement viewElement;
        private XmlNode fieldNode;
        private string associatedFieldInformation = string.Empty;
        private string sourceTableName;
        private string fieldName;
        private DataTable codeTable;
        //pairAssociated contains the information for the associated controls
        public Dictionary<string, int> pairAssociated = new Dictionary<string, int>();
        private Dictionary<string, Control> observerControls = new Dictionary<string, Control>();
        private BackgroundWorker _updater;
        private BackgroundWorker _inserter;

        #endregion Private Members

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page</param>
        public DDListField(Page page)
            : base(page)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view</param>
        public DDListField(View view)
            : base(view)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page</param>
        /// <param name="viewElement">XML View element</param>
        public DDListField(Page page, XmlElement viewElement)
            : base(page)
        {
            this.viewElement = viewElement;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view</param>
        /// <param name="fieldNode">XML field node</param>
        public DDListField(View view, XmlNode fieldNode)
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
                return MetaFieldType.List;
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

        /// <summary>
        /// Associated Filed Information
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
            }
        }

        /// <summary>
        /// Control Factory
        /// </summary>
        public IControlFactory ControlFactory { get; set; }

        #endregion Public Properties

        #region Public Methods
        /// <summary>
        /// Load From Row
        /// </summary>
        /// <param name="row">DataRow</param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
            sourceTableName = row[ColumnNames.SOURCE_TABLE_NAME].ToString();
            string st = row[ColumnNames.RELATE_CONDITION].ToString();
            fieldName = row[ColumnNames.TEXT_COLUMN_NAME].ToString().Trim();
            string[] words = st.Split(',');
            foreach (string s in words)
            {
                if (!(String.IsNullOrEmpty(s)))
                {
                    string[] pair = s.Split(':');
                    this.pairAssociated.Add(pair[0], Convert.ToInt16(pair[1]));
                }
            }
        }

        /// <summary>
        /// Load the observer controls for the codes field
        /// </summary>
        public void LoadDDLCodeObservers()
        {
            if (this.observerControls == null || this.observerControls.Count == 0)
            {
                foreach (KeyValuePair<string, int> pair in pairAssociated)
                {
                    //get field and its control
                    int fieldId = pair.Value;
                    List<Control> assocCtrls = new List<Control>();
                    foreach (Epi.Fields.Field f in view.Fields)
                    {
                        if (f.Id == fieldId)
                        {
                            Epi.Fields.Field field = f;
                            assocCtrls = this.ControlFactory.GetAssociatedControls(field);
                            break;
                        }
                    }
                    if (assocCtrls.Count > 1)
                    {
                        foreach (Control c in assocCtrls)
                        {
                            if (c is TextBox)
                                this.observerControls.Add(pair.Key, c);
                        }
                    }
                }
            }
            if (sourceTableName.Length != 0 && codeTable == null)
            {
                codeTable = view.GetProject().CodeData.GetCodeTableData(sourceTableName);
                DataColumn[] pk = new DataColumn[] { codeTable.Columns[fieldName] };
                codeTable.PrimaryKey = pk;
            }
        }


        /// <summary>
        /// Publish the observer controls
        /// </summary>
        public void PublishObservercontrols(string pID)
        {
            DataRow foundrow = codeTable.Rows.Find(pID);

            for (int i = 1; i < codeTable.Columns.Count; i++)
            {
                try
                {
                    Control c = observerControls[codeTable.Columns[i].ColumnName];
                    ((TextBox)c).Text = foundrow[codeTable.Columns[i]].ToString();
                }
                catch (Exception ex)
                {
                    //ignore the "can't find" error
                    Logger.Log(DateTime.Now + ":  " + ex.Message);
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