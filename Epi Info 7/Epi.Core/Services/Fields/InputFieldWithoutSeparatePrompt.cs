using System;
using System.Data;
using Epi.Data;
using Epi.Data.Services;
using EpiInfo.Plugin;

namespace Epi.Fields
{
    /// <summary>
    /// Input Field Without Separate Prompt abstract class
    /// </summary>
    public abstract class InputFieldWithoutSeparatePrompt : FieldWithoutSeparatePrompt, IMirrorable, IInputField
    {
        #region Private Class Members
        private bool isReadOnly = false;
        private bool isRequired = false;
        private bool shouldRepeatLast = false;
        private string tableName = string.Empty;
        private object currentRecordValueObject = null;
        private string _Namespace;
        #endregion Private Class Members

        #region Protected class members
        /// <summary>
        /// Db column type
        /// </summary>
        protected GenericDbColumnType genericDbColumnType = GenericDbColumnType.String;
        /// <summary>
        /// dbColumnType
        /// </summary>
        protected DbType dbColumnType = DbType.String;
        #endregion Protected class members

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page the field belongs to</param>
        public InputFieldWithoutSeparatePrompt(Page page)
            : base(page)
        {
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view the field belongs to</param>
        public InputFieldWithoutSeparatePrompt(View view)
            : base(view)
        {
            Construct();
        }

        private void Construct()
        {
        }

        /// <summary>
        /// Load an Input Field without Separate Prompt from a
        ///  <see cref="System.Data.DataRow"/>
        /// </summary>
        /// <param name="row">Row containing InputFieldWIthouthSeparatePrompt data.</param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
            isRequired = (bool)row[ColumnNames.IS_REQUIRED];
            isReadOnly = (bool)row[ColumnNames.IS_READ_ONLY];
            shouldRepeatLast = (bool)row[ColumnNames.SHOULD_REPEAT_LAST];
            tableName = row[ColumnNames.DATA_TABLE_NAME].ToString();
        }

        public override void AssignMembers(Object field)
        {
            (field as InputFieldWithoutSeparatePrompt).isRequired = this.isRequired;
            (field as InputFieldWithoutSeparatePrompt).isReadOnly = this.isReadOnly;
            (field as InputFieldWithoutSeparatePrompt).shouldRepeatLast = this.shouldRepeatLast;
            (field as InputFieldWithoutSeparatePrompt).tableName = this.tableName;
            base.AssignMembers(field);
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// Retuns the datatype of the field.
        /// </summary>
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
        }/**/


        /// <summary>
        /// The variable type of a field is always DataSourceField.
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
        /// Returns a string representation of Current Record Value
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
        /// Gets/sets Read only flag
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return (isReadOnly);
            }
            set
            {
                isReadOnly = value;
            }
        }

        /// <summary>
        /// Gets/sets Required flag.
        /// </summary>
        public bool IsRequired
        {
            get
            {
                return (isRequired);
            }
            set
            {
                isRequired = value;
            }
        }

        /// <summary>
        /// Gets/sets should repeat last flag
        /// </summary>
        public bool ShouldRepeatLast
        {
            get
            {
                return (shouldRepeatLast);
            }
            set
            {
                shouldRepeatLast = value;
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
        /// Gets the SQL Data type
        /// </summary>
        public virtual string GetDbSpecificColumnType()
        {
            if (genericDbColumnType != GenericDbColumnType.Unknown)
            {
                return GetProject().CollectedData.GetDatabase().GetDbSpecificColumnType(genericDbColumnType);
            }
            else
            {
                throw new GeneralException("genericDbColumnType is not set: " + this.FieldType.ToString());
            }
        }

        public string Prompt { get { return this.Name; } set { return; } }
        #endregion Public Properties

        #region Public Methods
        /// <summary>
        /// Returns the Variable type enumeration value.
        /// </summary>
        /// <param name="typeCombination">Variable Type enum combination.</param>
        /// <returns>True/False on variable type test.</returns>
        public bool IsVarType(VariableType typeCombination)
        {
            return VariableBase.IsVarType(this.VarType, typeCombination);
        }

        /// <summary>
        /// Returns the string value that is reflected my a mirror field.
        /// </summary>
        /// <returns>reflected value string</returns>
        public virtual string GetReflectedValue()
        {
            return this.CurrentRecordValueString;
        }

        public void SetNewRecordValue()
        {
            if (shouldRepeatLast == false)
            {
                if (genericDbColumnType == GenericDbColumnType.String)
                {
                    CurrentRecordValueString = string.Empty;
                }
                else
                {
                    CurrentRecordValueObject = null;
                }
            }
        }

        #endregion Public Methods
    }
}
