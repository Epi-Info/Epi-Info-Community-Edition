using System;
using System.Data;

using Epi;
using Epi.Data;
using Epi.Data.Services;
using EpiInfo.Plugin;

namespace Epi.Fields
{
    /// <summary>
    /// Base class for all fields (in a view) that accept input and have a prompt separate from the control
    /// </summary>
	public abstract class InputFieldWithSeparatePrompt :  FieldWithSeparatePrompt, IMirrorable, IInputField
	{
		#region Private Class Members
		private bool isReadOnly = false;
		private bool isRequired = false;
		private bool shouldRepeatLast = false;
        private string tableName = string.Empty;
        private string _Namespace;

        /// <summary>
        /// Current Record Value Object
        /// </summary>
        protected object currentRecordValueObject = null;
		#endregion Private Class Members

		#region Protected class members
        /// <summary>
        /// SQL data type.
        /// </summary>
        protected GenericDbColumnType genericDbColumnType = GenericDbColumnType.String;
        /// <summary>
        /// dbColumnType
        /// </summary>
        protected DbType dbColumnType = DbType.String;
		#endregion Protected class members
		
		#region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="page"><see cref="Epi.Page"/></param>
		public InputFieldWithSeparatePrompt(Page page) : base(page)
		{
			Construct();
		}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="view"><see cref="Epi.View"/></param>
		public InputFieldWithSeparatePrompt(View view) : base(view)
		{
			Construct();
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
                return (DataType) AppData.Instance.FieldTypesDataTable.GetDataTypeByFieldTypeId((int)FieldType);
            }
            set
            {
                this.DataType = value;
            }
        }

        /// <summary>
        /// Field variable type
        /// </summary>
        public VariableType VarType
        {
            get
            {
                return VariableType.DataSource;
            }
        }



        /// <summary>
        /// Field variable Scope
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


        public string Prompt { get { return this.PromptText; } set { return; } }

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
                    if (this.currentRecordValueObject is System.Data.DataRowView)
                    {
                        paramValue = ((System.Data.DataRowView)this.CurrentRecordValueObject).Row[0];
                    }
                    else
                    {
                        paramValue = this.CurrentRecordValueObject;
                    }
                }
                return new QueryParameter("@" + this.Name, this.dbColumnType, paramValue);
            }
        }

        /// <summary>
        /// Gets/sets the Expression for the variable is the table name plus field name separated by a dot
        /// </summary>
        public string Expression
        {
            get
            {
                if (this.GetProject() != null && this.GetProject().CollectedData.GetDatabase() != null)
                {
                    return this.GetProject().CollectedData.GetDatabase().InsertInEscape(tableName) + "." + this.Name;
                }
                else
                {
                    return this.Name;
                }
                
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
        /// Gets/sets the Read Only field flag.
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
		/// Required field flag.
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
        /// Gets/sets the Should repeat last field flag.
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
		#endregion Public Properties

        #region Public Methods
        /// <summary>
        /// Tests the variable type.
        /// </summary>
        /// <param name="typeCombination">Variable Type to test</param>
        /// <returns>True/False</returns>
        public bool IsVarType(VariableType typeCombination)
        {
            return VariableBase.IsVarType(this.VarType, typeCombination);
        }

        /// <summary>
        /// Load InputFieldWithSeparatePrompt from a <see cref="System.Data.DataRow"/>
        /// </summary>
        /// <param name="row"></param>
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
            (field as InputFieldWithSeparatePrompt).isRequired = this.isRequired;
            (field as InputFieldWithSeparatePrompt).isReadOnly = this.isReadOnly;
            (field as InputFieldWithSeparatePrompt).shouldRepeatLast = this.shouldRepeatLast;
            (field as InputFieldWithSeparatePrompt).tableName = this.tableName;
            base.AssignMembers(field);
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

        #region Private Methods
        private void Construct()
        {
        }
        #endregion Private Methods
    }
}
