#region Namespaces

using System;
using System.Data;
using System.Xml;
using System.Collections.Generic;
using Epi;
using Epi.Collections;

#endregion  //Namespaces

namespace Epi.Fields
{
    /// <summary>
    /// Grid field
    /// </summary>
    public class GridField : FieldWithSeparatePrompt
    {
        #region Private Members

        private XmlElement viewElement;
        private XmlNode fieldNode;
        private List<GridColumnBase> columns;
        private DataTable dataSource;
        #endregion Private Members

        #region Public Events
        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for the class. Receives Page object.
        /// </summary>
        /// <param name="page">The page this field belongs to.</param>
        public GridField(Page page)
            : base(page)
        {
        }

        /// <summary>
        /// Constructor for the class. Receives View object.
        /// </summary>
        /// <param name="view">The view this field belongs to.</param>
        public GridField(View view)
            : base(view)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page this field belongs to.</param>
        /// <param name="viewElement">The Xml view element of the GridField.</param>
        public GridField(Page page, XmlElement viewElement)
            : base(page)
        {
            this.viewElement = viewElement;
            this.Page = page;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="fieldNode"></param>
        public GridField(View view, XmlNode fieldNode)
            : base(view)
        {
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, this.fieldNode);
        }

        public GridField Clone()
        {
            GridField clone = (GridField)this.MemberwiseClone();
            base.AssignMembers(clone);
            return clone;
        }
        #endregion Constructors

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
        /// Constructs a GridColumnBase object from a MetaFieldType enum
        /// </summary>
        /// <param name="columnType">A MetaFieldType enum</param>
        /// <returns>A GridColumnBase object</returns>
        public GridColumnBase CreateGridColumn(MetaFieldType columnType)
        {
            switch (columnType)
            {
                case MetaFieldType.UniqueKey:
                    return new UniqueKeyColumn(this);
                case MetaFieldType.RecStatus:
                    return new RecStatusColumn(this);
                case MetaFieldType.ForeignKey:
                    return new ForeignKeyColumn(this);
                case MetaFieldType.Text:
                    return new TextColumn(this);
                case MetaFieldType.PhoneNumber:
                    return new PhoneNumberColumn(this);
                case MetaFieldType.Date:
                    return new DateColumn(this);
                case MetaFieldType.Number:
                    return new NumberColumn(this);
                default:
                    return new TextColumn(this);
            }
        }

        public void SetNewRecordValue()
        {
            if (DataSource is DataTable)
            {
                foreach (DataRow row in ((DataTable)DataSource).Rows)
                {
                    if (row.RowState != DataRowState.Deleted)
                    {
                        foreach (GridColumnBase column in Columns)
                        {
                            if (column is GridColumnBase && ((GridColumnBase)column).ShouldRepeatLast == false)
                            {
                                row[column.Name] = DBNull.Value;
                            }
                        }
                    }
                }
                DataSource.AcceptChanges();
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Database Table Name of the page
        /// </summary>
        public string TableName
        {
            get
            {
                if (this.Page == null)
                {
                    return null;
                }
                else
                {
                    return this.Page.TableName + this.Name;
                }
            }
        }

        /// <summary>
        /// Returns field type
        /// </summary>
        public override MetaFieldType FieldType
        {
            get
            {
                return MetaFieldType.Grid;
            }
        }

        /// <summary>
        /// Gets the column collection
        /// </summary>
        public List<GridColumnBase> Columns
        {
            get
            {
                if (this.columns == null && !this.Id.Equals(0))
                {
                    this.columns = GetMetadata().GetGridColumnCollection(this);
                    if (this.columns.Count.Equals(0))
                    {
                        UniqueRowIdColumn uniqueRowIdColumn = new UniqueRowIdColumn(this);
                        RecStatusColumn recStatusColumn = new RecStatusColumn(this);
                        ForeignKeyColumn foreignKeyColumn = new ForeignKeyColumn(this);
                        GlobalRecordIdColumn globalRecordIdColumn = new GlobalRecordIdColumn(this);
                        
                        uniqueRowIdColumn.SaveToDb();
                        recStatusColumn.SaveToDb();
                        foreignKeyColumn.SaveToDb();
                        globalRecordIdColumn.SaveToDb();
                        
                        columns = GetMetadata().GetGridColumnCollection(this);
                    }
                }
                return this.columns;
            }
            set
            {
                this.columns = value;
            }
        }
        /// <summary>
        /// In-memory storage of rows editing in Epi.Enter.
        /// This allows the datagrid control's contents to be persisted to the database.
        /// </summary>
        public DataTable DataSource
        {
            get
            {
                return dataSource;
            }
            set
            {
                dataSource = value;
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

        #region Protected Methods

        /// <summary>
        /// Inserts the field to the database
        /// </summary>
        protected override void InsertField()
        {
            this.Id = GetMetadata().CreateField(this);

            if (columns != null)
            { 
                foreach (GridColumnBase column in columns)
                {
                    column.Id = 0;
                    ((Epi.Fields.Field)(column.Grid)).Id = this.Id;
                }
            }

            SaveColumnsToDb();
            base.OnFieldAdded();
        }

        /// <summary>
        /// Update the field to the database
        /// </summary>
        protected override void UpdateField()
        {
            GetMetadata().UpdateField(this);
            SaveColumnsToDb();
        }
        #endregion Protected Methods

        #region Private Methods

        private void SaveColumnsToDb()
        {
            foreach (GridColumnBase gridColumn in Columns)
            {
                gridColumn.SaveToDb();
            }
        }

        #endregion Private Methods

        #region Event Handlers
        #endregion
    }
}
