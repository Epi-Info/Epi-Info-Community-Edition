using System;
using System.Data;

using Epi;
using Epi.Data;

namespace Epi.Fields
{
    /// <summary>
    /// Table Base Drop Down Column abstract class for DDLColumnOf* grid columns.
    /// </summary>
    public abstract class TableBasedDropDownColumn : GridColumnBase
    {
        #region Private Class Members
        private bool isExclusiveTable;
        private string sourceTableName = string.Empty;
        private bool shouldSort = false;
        private string tableName = string.Empty;
        #endregion Private Class Members

        #region Protected Members
        /// <summary>
        /// Name of code (value) field in code table
        /// </summary>
        protected string codeColumnName = string.Empty;
        /// <summary>
        /// Name of text (display) field in code table
        /// </summary>
        protected string textColumnName = string.Empty;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="grid">Grid Field that contains Dropdown column.</param>
        public TableBasedDropDownColumn(GridField gridField)
            : base(gridField)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gridRow">DataRow of column attributes</param>
        /// <param name="grid">Grid Field that contains Dropdown column.</param>
        public TableBasedDropDownColumn(DataRow gridDataRow, GridField gridField)
            : base(gridDataRow, gridField)
        {
            tableName = gridDataRow[ColumnNames.DATA_TABLE_NAME].ToString();
            if (gridDataRow[ColumnNames.IS_EXCLUSIVE_TABLE] != null &&
                gridDataRow[ColumnNames.IS_EXCLUSIVE_TABLE] != DBNull.Value) 
            {
                isExclusiveTable = (bool)gridDataRow[ColumnNames.IS_EXCLUSIVE_TABLE];
            }
            sourceTableName = gridDataRow[ColumnNames.SOURCE_TABLE_NAME].ToString();
            textColumnName = gridDataRow[ColumnNames.TEXT_COLUMN_NAME].ToString();
            if (gridDataRow[ColumnNames.CODE_COLUMN_NAME] != null)
            {
                codeColumnName = gridDataRow[ColumnNames.CODE_COLUMN_NAME].ToString();
            }
            
            if (gridDataRow[ColumnNames.SORT] != null &&
                gridDataRow[ColumnNames.SORT] != DBNull.Value)
            {
                shouldSort = (bool)gridDataRow[ColumnNames.SORT];
            }

        }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Gets/sets the Exclusive Table flag.
        /// </summary>
        public bool IsExclusiveTable
        {
            get
            {
                return (isExclusiveTable);
            }
            set
            {
                isExclusiveTable = value;
            }
        }

        /// <summary>
        /// Gets/sets the Source table name.
        /// </summary>
        public string SourceTableName
        {
            get
            {
                return sourceTableName;
            }
            set
            {
                sourceTableName = value;
            }
        }

        /// <summary>
        /// Gets/sets the Code column name.
        /// </summary>
        public virtual string CodeColumnName
        {
            get
            {
                return codeColumnName;
            }
            set
            {
                codeColumnName = value;
            }
        }

        /// <summary>
        /// Gets/sets the Text column name.
        /// </summary>
        public virtual string TextColumnName
        {
            get
            {
                return textColumnName;
            }
            set
            {
                textColumnName = value;
            }
        }

        /// <summary>
        /// Gets/sets the Should sort field flag.
        /// </summary>
        public bool ShouldSort
        {
            get
            {
                return (shouldSort);
            }
            set
            {
                shouldSort = value;
            }
        }

        ///// <summary>
        ///// TableBasedColumn data type
        ///// </summary>
        //public override String DataTypes
        //{
        //    get { return SqlDataTypes.NVARCHAR; }
        //}
        #endregion Public Properties

        #region Public Methods

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns the source data for this DDL
        /// </summary>
        /// <returns></returns>
        public DataTable GetSourceData()
        {
            if (string.IsNullOrEmpty(SourceTableName))
            {
                throw new System.ApplicationException(SharedStrings.SOURCE_TABLE_NOT_SET);
            }
            return GetMetadata().GetCodeTableData(SourceTableName);
        }

        /// <summary>
        /// Returns the source data for this DDL
        /// </summary>
        /// <param name="columnNames"></param>
        /// <returns></returns>
        public DataTable GetSourceData(string columnNames)
        {
            if (string.IsNullOrEmpty(SourceTableName))
            {
                throw new System.ApplicationException(SharedStrings.SOURCE_TABLE_NOT_SET);
            }
            return GetMetadata().GetCodeTableData(SourceTableName, columnNames);
        }

        /// <summary>
        /// Returns the source data for this DDL
        /// </summary>
        /// <param name="columnNames"></param>
        /// <param name="sortCriteria"></param>
        /// <returns></returns>
        public DataTable GetSourceData(string columnNames, string sortCriteria)
        {
            if (string.IsNullOrEmpty(SourceTableName))
            {
                throw new System.ApplicationException(SharedStrings.SOURCE_TABLE_NOT_SET);
            }
            return GetMetadata().GetCodeTableData(SourceTableName, columnNames, sortCriteria);
        }
        #endregion
    }
}
