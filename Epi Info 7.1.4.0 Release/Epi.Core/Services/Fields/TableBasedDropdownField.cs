using System;
using System.Data;

using Epi;
using Epi.Data;

namespace Epi.Fields
{
    /// <summary>
    /// Table Based Drop Down Field abstract class
    /// </summary>
    public abstract class TableBasedDropDownField : DropDownField, IFieldWithCheckCodeAfter, IFieldWithCheckCodeBefore, IFieldWithCheckCodeClick
	{
		#region Private Class Members
		private bool isExclusiveTable;
		private string sourceTableName = string.Empty;
		private bool shouldSort = false;
        private DataTable codeTable;
		#endregion Private Class Members

        #region Protected Members
        protected string checkCodeAfter = string.Empty;
        protected string checkCodeBefore = string.Empty;
        protected string checkCodeClick = string.Empty;
        protected string codeColumnName = string.Empty;
        protected string textColumnName = string.Empty;
        #endregion

		#region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page this field belongs to</param>
		public TableBasedDropDownField(Page page) : base(page)
		{
            construct();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view this field belongs to</param>
		public TableBasedDropDownField(View view) : base(view)
		{
            construct();
        }

        private void construct()
        {
            genericDbColumnType = GenericDbColumnType.String;
            this.dbColumnType = DbType.String;
        }

        /// <summary>
        /// Load TableBasedDropdownField from a System.Data.DataRow.
        /// </summary>
        /// <param name="row">Data Row to load</param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
            isExclusiveTable = (bool)row[ColumnNames.IS_EXCLUSIVE_TABLE];
            sourceTableName = row[ColumnNames.SOURCE_TABLE_NAME].ToString();
            textColumnName = row[ColumnNames.TEXT_COLUMN_NAME].ToString();
            if (row[ColumnNames.CODE_COLUMN_NAME] != null)
            {
                codeColumnName = row[ColumnNames.CODE_COLUMN_NAME].ToString();
            }
            shouldSort = (bool)row[ColumnNames.SORT];
        }

        public TableBasedDropDownField Clone()
        {
            TableBasedDropDownField clone = (TableBasedDropDownField)this.MemberwiseClone();
            base.AssignMembers(clone);
            return clone;
        }

        public DataTable GetDisplayTable(string predicate, string expression, string displayMember)
        {
            DataTable displayTable = new DataTable();

            if (string.IsNullOrEmpty(expression))
            {
                DataView dataView = new DataView(CodeTable);

                if (dataView.Count > 0)
                {
                    displayTable = dataView.ToTable(true, this.TextColumnName);
                }
                else
                {
                    displayTable = null;
                    return displayTable;
                }
            }
            else
            {
                EnumerableRowCollection<DataRow> query =
                    from code in codeTable.AsEnumerable()
                    where code.Field<string>(predicate) == expression
                    select code;

                DataView view = query.AsDataView();
                displayTable = view.ToTable(true, this.TextColumnName);
            }

            displayTable.Columns[0].ColumnName = "Item";

            return displayTable;
        }

		#endregion Constructors

		#region Public Properties

        public DataTable CodeTable
        {
            get
            {
                if (codeTable == null)
                {
                    codeTable = GetSourceData();
                }
                
                return codeTable;
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
		/// Gets/sets Is Exclusive table flag.
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
        /// Gets/sets the table name of the source data.
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
		/// Gets/sets the column name of the code (value) field.
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
        /// Gets/sets the column name of the text (display) field.
        /// </summary>
		public virtual string TextColumnName
		{
			get
			{
				return textColumnName.Trim();
			}
			set
			{
				textColumnName = value;
			}
		}

        /// <summary>
        /// 
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

        //zack add Checkcode property for all dropdown list type fields,  5/14/08
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

        #region Private Methods

		/// <summary>
		/// Returns the source data for this DDL
		/// </summary>
		/// <returns>Source Data</returns>
		public DataTable GetSourceData()
		{
			if (string.IsNullOrEmpty(SourceTableName))
			{
                return null;
			}

            DataTable dataTable = new DataTable();

            View view = this.view;
            Page page = ((RenderableField)this).Page;
            Epi.Data.Services.IMetadataProvider metadata = view.GetMetadata();

            string tableName = SourceTableName;

            Project project = this.GetProject();
            string filterExpression = string.Empty;

            if (project.CollectedData.TableExists(tableName) == false)
            {
                string separator = " - ";

                if (tableName.Contains(separator))
                {
                    string[] view_page = tableName.Replace(separator, "^").Split('^');
                    string viewName = view_page[0].ToString();
                    string pageName = view_page[1].ToString();
                    View targetView = project.Metadata.GetViewByFullName(viewName);

                    if (targetView == null) return null;

                    DataTable targetPages = project.Metadata.GetPagesForView(targetView.Id);
                    DataView dataView = new DataView(targetPages);

                    filterExpression = string.Format("Name = '{0}'", pageName);
                    
                    DataRow[] pageArray = targetPages.Select(filterExpression);

                    if (pageArray.Length > 0)
                    {
                        int pageId = (int)pageArray[0]["PageId"];
                        tableName = viewName + pageId;
                    }
                }
            }

            if (this.GetProject().CollectedData.TableExists(tableName))
            {
                dataTable = GetProject().GetTableData(tableName);
            }

            return dataTable;
		}

        /// <summary>
        /// Returns the source data for this DDL
        /// </summary>
        /// <returns>Source Data</returns>
        public DataTable GetSourceData(string columnNames)
        {
            if (string.IsNullOrEmpty(SourceTableName))
            {
                throw new System.ApplicationException(SharedStrings.SOURCE_TABLE_NOT_SET);
            }
            return GetProject().GetTableData(SourceTableName, columnNames);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns specific data type for the target DB.
        /// </summary>
        /// <returns></returns>
        public override string GetDbSpecificColumnType()
        {
            return base.GetDbSpecificColumnType() + "(255)";
        }
        #endregion Public Methods
    }
}