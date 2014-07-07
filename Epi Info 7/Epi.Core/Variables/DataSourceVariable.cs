using System;

namespace Epi
{
    /// <summary>
    /// Class DataSourceVariable
    /// </summary>
    public class DataSourceVariable : VariableBase, IDataSourceVariable
    {
        #region Fields
        /// <summary>
        /// Table Name
        /// </summary>
        protected string tableName = string.Empty;
        /// <summary>
        /// Type of the field
        /// </summary>
        protected MetaFieldType fieldType;
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="dataType">Data Type</param>
        public DataSourceVariable(string name, DataType dataType)
            : base(name, dataType, VariableType.DataSource)
        {
        }
        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// name of the data source (Table)
        /// </summary>
        public string TableName
        {
            get { return tableName; }
            set { tableName = value; }
        }

        /// <summary>
        /// Field type
        /// </summary>
        [Obsolete("Field type is not relevant here. Need to recheck", false)]
        public MetaFieldType FieldType
        {
            get { return fieldType; }
            set { fieldType = value; }
        }

        /// <summary>
        /// Expression for the variable is the table name plus field name separated by a dot
        /// </summary>
        public override string Expression
        {
            get
            {
                return Util.InsertInSquareBrackets(tableName) + "." + this.Name;
            }
        }

        /// <summary>
        /// The value of the current record
        /// </summary>
        protected object currentRecordValueObject = null;

        /// <summary>
        /// Current Record Value
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

        #endregion Public Properties

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IVariable Redefine()
        {
            IVariable var = new DataSourceVariableRedefined(this.Name, this.DataType);
            var.Expression = this.Expression;
            return var;
        }
        #endregion Public Methods
    }
}