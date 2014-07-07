using System;
using System.Data;

using Epi.Data.Services;

namespace Epi.Fields
{
	/// <summary>
	/// Unique Key and RecStatus are predefined data fields. Their names and sizes can't be changed.
	/// </summary>
	public abstract class PredefinedColumn : GridColumnBase
    {
        #region Private Members
        private string val = string.Empty;
        private string tableName = string.Empty;
        #endregion Private Members

        #region Constructors

		/// <summary>
		/// Constructor for the class
		/// </summary>
		public PredefinedColumn(GridField grid) : base(grid)
		{
		}
        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="gridRow">A DataRow containing the grid row's data</param>
        /// <param name="grid">The grid that contains the gridrow</param>
        public PredefinedColumn(DataRow gridRow, GridField grid)
            : base(gridRow, grid)
		{
        }

        #endregion Constructors

        #region Public properties
        /// <summary>
        /// Retuns the datatype of the field.
        /// </summary>
        public DataType DataType
        {
            get
            {
                return (DataType)AppData.Instance.FieldTypesDataTable.GetDataTypeByFieldTypeId((int)GridColumnType);
            }
            set
            {
                this.DataType = value;
            }
        }
        /// <summary>
        /// Always returns the VariableType.DataSource.
        /// </summary>
        public VariableType VarType
        {
            get
            {
                return VariableType.DataSource;
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
        /// Returns the prompt for UI.
        /// </summary>
        public abstract string PromptText { get;set;}

        /// <summary>
        /// Value indicating the record status.
        /// </summary>
        public string CurrentRecordValueString
        {
            get
            {
                return val;
            }
            set
            {
                val = value;
            }
        }

        /// <summary>
        /// Expression for the variable is the table name plus field name separated by a dot
        /// </summary>
        public string Expression
        {
            get
            {
                return Grid.GetProject().CollectedData.GetDatabase().InsertInEscape(tableName) + "." + Name;
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
        #endregion Public Methods
    }
}