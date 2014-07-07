using System;
using System.Data;

using Epi.Data;
using Epi.Data.Services;
using EpiInfo.Plugin;

namespace Epi.Fields
{
	/// <summary>
	/// Unique Key and RecStatus are predefined data fields. Their names and sizes can't be changed.
	/// </summary>
	public abstract class PredefinedDataField : Field, IDataField
    {
        #region Private Members
        private object currentRecordValueObject = null;
        private string tableName = string.Empty;
        /// <summary>
        /// dbColumnType
        /// </summary>
        protected DbType dbColumnType;
        private string _Namespace;
        #endregion Private Members

        #region Constructors
        /// <summary>
		/// Instantiates a field
		/// </summary>
		/// <param name="view">The view this field belongs to</param>
        public PredefinedDataField(View view)
            : base(view)
		{
            //construct();  
        }

        /// <summary>
        /// Load from row
        /// </summary>
        /// <param name="row">Row</param>
        public override void LoadFromRow(DataRow row)
        {
            //construct();
            base.LoadFromRow(row);
            tableName = row[ColumnNames.DATA_TABLE_NAME].ToString();
        }

        #endregion Constructors

        #region Public properties
        /// <summary>
        /// Retuns the datatype of the field.
        /// </summary>
        /// 
        
        //public abstract DataType DataType{get; set;}
        public DataType DataType
        { 
            get
            {
                return (DataType)AppData.Instance.FieldTypesDataTable.GetDataTypeByFieldTypeId((int)FieldType);
            }
            set
            {
                //this.DataType = value;
                return;
            }
        }
        
        /// <summary>
        /// Retuns the EpiInfo.Plugin.datatype of the field.
        /// </summary>
        EpiInfo.Plugin.DataType EpiInfo.Plugin.IVariable.DataType
        {
            get
            {
                return (EpiInfo.Plugin.DataType)AppData.Instance.FieldTypesDataTable.GetDataTypeByFieldTypeId((int)FieldType);
            }
            set
            {
                return;
            }
        }

        /// <summary>
        /// VarType
        /// </summary>
        public VariableType VarType
        {
            get
            {
                return VariableType.DataSource;
            }
        }



        /// <summary>
        /// The variable type of a field is always DataSourceField.
        /// </summary>
        public VariableScope VariableScope
        {
            get
            {
                return VariableScope.DataSource;
            }
            set { return; }
        }

        /// <summary>
        /// Field namespace
        /// </summary>
        public string Namespace { get { return this._Namespace; } set { this._Namespace = value; } }
        /// <summary>
        /// Field's table name
        /// </summary>
        public string TableName
        {
            get
            {
                return tableName;
            }
            set
            {
                tableName = value;
            }
        }

        /// <summary>
        /// The prompt's text
        /// </summary>
        public abstract string PromptText { get;set;}
        public string Prompt { get { return this.PromptText; } set { return; } }
        /// <summary>
        /// Value indicating the record status
        /// </summary>
        public int CurrentRecordValue
        {
            get
            {
                if (currentRecordValueObject == null || string.IsNullOrEmpty(currentRecordValueObject.ToString())) return 0;
                else return int.Parse(currentRecordValueObject.ToString());
            }
            set
            {
                currentRecordValueObject = value;
            }
        }

        /// <summary>
        /// Gets/sets the data contents of the field
        /// </summary>
        public virtual object CurrentRecordValueObject
        {
            get
            {
                return currentRecordValueObject;
            }
            set
            {
                currentRecordValueObject = value;
            }
        }

        /// <summary>
        /// Gets/sets the String representation of the current record value.
        /// </summary>
        public virtual string CurrentRecordValueString
        {
            get
            {
                if (CurrentRecordValueObject == null) return string.Empty;
                else return CurrentRecordValueObject.ToString();
            }
            set
            {
                CurrentRecordValueObject = value;
            }
        }

        /// <summary>
        /// Returns current record value as a DB Query parameter.
        /// </summary>
        public QueryParameter CurrentRecordValueAsQueryParameter
        {
            get
            {
                object paramValue = DBNull.Value;
                if (!Util.IsEmpty(CurrentRecordValueObject))
                {
                    paramValue = this.CurrentRecordValueObject;
                }
                return new QueryParameter("@" + this.Name, this.dbColumnType, paramValue);
            }
        }

        /// <summary>
        /// Expression for the variable is the table name plus field name separated by a dot
        /// </summary>
        public string Expression
        {
            get
            {
                return GetProject().CollectedData.GetDatabase().InsertInEscape(tableName) + "." + this.Name;
            }
            set
            {
                throw new ApplicationException("Expression can't be set");
            }
        }
        
        #endregion Public properties

        #region Public Methods
        /// <summary>
        /// Determines if this field is of a given type(s)
        /// </summary>
        /// <param name="typeCombination"></param>
        /// <returns></returns>
        public bool IsVarType(VariableType typeCombination)
        {
            return VariableBase.IsVarType(this.VarType, typeCombination);
        }

        public void SetNewRecordValue()
        {
        }

        #endregion Public Methods
    }
}
