#region Namespaces

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Epi.Data
{

    /// <summary>
    /// A generic table column that the specific databases use to create tables
    /// </summary>
    public class TableColumn : Epi.Data.ITableColumn
    {

        #region Private Members

        private string name;
        private GenericDbColumnType dataType;
        private int? length;
        private int? precision;
        private int? scale;
        private bool isIdentity;
        private bool allowNull;
        private bool isPrimaryKey;
        private string foreignKeyTableName;
        private string foreignKeyColumnName;
        private bool cascadeDelete = false;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the TableColumn class.
        /// </summary>
        /// <param name="name">Name of the column</param>
        /// <param name="dataType">The column's data type</param>
        /// <param name="allowNull">A value indicating whether or not the column can be null</param>
        public TableColumn(string name, GenericDbColumnType dataType, bool allowNull)
        {
            this.name = name;
            this.dataType = dataType;
            this.allowNull = allowNull;
        }

        /// <summary>
        /// Initializes a new instance of the TableColumn class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="allowNull"></param>
        /// <param name="isPrimaryKey"></param>
        public TableColumn(string name, GenericDbColumnType dataType, bool allowNull, bool isPrimaryKey)
        {
            this.name = name;
            this.dataType = dataType;
            this.allowNull = allowNull;
            this.isPrimaryKey = isPrimaryKey;
        }

        /// <summary>
        /// Initializes a new instance of the TableColumn class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="allowNull"></param>
        /// <param name="isPrimaryKey"></param>
        /// <param name="isIdentity"></param>
        public TableColumn(string name, GenericDbColumnType dataType, bool allowNull, bool isPrimaryKey, bool isIdentity)
        {
            this.name = name;
            this.dataType = dataType;
            this.allowNull = allowNull;
            this.isPrimaryKey = isPrimaryKey;
            this.isIdentity = isIdentity;
        }

        /// <summary>
        /// Initializes a new instance of the TableColumn class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="allowNull"></param>
        /// <param name="foreignKeyTableName"></param>
        /// <param name="foreignKeyColumnName"></param>
        /// <param name="cascadeDelete">A value indicating whether or not to cascade on delete</param>
        public TableColumn(string name, GenericDbColumnType dataType, bool allowNull, string foreignKeyTableName, string foreignKeyColumnName, bool cascadeDelete)
        {
            this.name = name;
            this.dataType = dataType;
            this.allowNull = allowNull;
            this.foreignKeyTableName = foreignKeyTableName;
            this.foreignKeyColumnName = foreignKeyColumnName;
            this.cascadeDelete = cascadeDelete;
        }

        /// <summary>
        /// Initializes a new instance of the TableColumn class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="allowNull"></param>
        /// <param name="foreignKeyTableName"></param>
        /// <param name="foreignKeyColumnName"></param>
        public TableColumn(string name, GenericDbColumnType dataType, bool allowNull, string foreignKeyTableName, string foreignKeyColumnName)
        {
            this.name = name;
            this.dataType = dataType;
            this.allowNull = allowNull;
            this.foreignKeyTableName = foreignKeyTableName;
            this.foreignKeyColumnName = foreignKeyColumnName;
        }



        /// <summary>
        /// Initializes a new instance of the TableColumn class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="length"></param>
        /// <param name="allowNull"></param>
        public TableColumn(string name, GenericDbColumnType dataType, int length, bool allowNull)
        {
            this.name = name;
            this.dataType = dataType;
            this.length = length;
            this.allowNull = allowNull;
        }

        /// <summary>
        /// Initializes a new instance of the TableColumn class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="length"></param>
        /// <param name="allowNull"></param>
        /// <param name="isPrimaryKey"></param>
        public TableColumn(string name, GenericDbColumnType dataType, int length, bool allowNull, bool isPrimaryKey)
        {
            this.name = name;
            this.dataType = dataType;
            this.length = length;
            this.allowNull = allowNull;
            this.isPrimaryKey = isPrimaryKey;
        }

        /// <summary>
        /// Initializes a new instance of the TableColumn class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="length"></param>
        /// <param name="allowNull"></param>
        /// <param name="foreignKeyTableName"></param>
        /// <param name="foreignKeyColumnName"></param>
        public TableColumn(string name, GenericDbColumnType dataType, int length, bool allowNull, string foreignKeyTableName, string foreignKeyColumnName)
        {
            this.name = name;
            this.dataType = dataType;
            this.length = length;
            this.allowNull = allowNull;
            this.foreignKeyTableName = foreignKeyTableName;
            this.foreignKeyColumnName = foreignKeyColumnName;
        }


        /// <summary>
        /// Initializes a new instance of the TableColumn class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <param name="allowNull"></param>
        public TableColumn(string name, GenericDbColumnType dataType, int precision, int scale, bool allowNull)
        {
            this.name = name;
            this.dataType = dataType;
            this.precision = precision;
            this.scale = scale;
            this.allowNull = allowNull;
        }

        /// <summary>
        /// Initializes a new instance of the TableColumn class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <param name="allowNull"></param>
        /// <param name="isPrimaryKey"></param>
        public TableColumn(string name, GenericDbColumnType dataType, int precision, int scale, bool allowNull, bool isPrimaryKey)
        {
            this.name = name;
            this.dataType = dataType;
            this.precision = precision;
            this.scale = scale;
            this.allowNull = allowNull;
            this.isPrimaryKey = isPrimaryKey;
        }

        /// <summary>
        /// Initializes a new instance of the TableColumn class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <param name="allowNull"></param>
        /// <param name="foreignKeyTableName"></param>
        /// <param name="foreignKeyColumnName"></param>
        public TableColumn(string name, GenericDbColumnType dataType, int precision, int scale, bool allowNull, string foreignKeyTableName, string foreignKeyColumnName)
        {
            this.name = name;
            this.dataType = dataType;
            this.precision = precision;
            this.scale = scale;
            this.allowNull = allowNull;
            this.foreignKeyTableName = foreignKeyTableName;
            this.foreignKeyColumnName = foreignKeyColumnName;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets/sets the name
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Gets/sets the data type
        /// </summary>
        public GenericDbColumnType DataType
        {
            get
            {
                return dataType;
            }
            set
            {
                dataType = value;
            }
        }

        /// <summary>
        /// Gets/sets the length
        /// </summary>
        public int? Length
        {
            get
            {
                return length;
            }
            set
            {
                length = value;
            }
        }

        /// <summary>
        /// Gets/sets the precision
        /// </summary>
        public int? Precision
        {
            get
            {
                return precision;
            }
            set
            {
                precision = value;
            }
        }

        /// <summary>
        /// Gets/sets the scale
        /// </summary>
        public int? Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
            }
        }

        /// <summary>
        /// Gets/sets a value indicating if the column is null
        /// </summary>
        public bool AllowNull
        {
            get
            {
                return allowNull;
            }
            set
            {
                allowNull = value;
            }
        }

        /// <summary>
        /// Gets/sets a value indicating if the column is a primary key
        /// </summary>
        public bool IsPrimaryKey
        {
            get
            {
                return isPrimaryKey;
            }
            set
            {
                isPrimaryKey = value;
            }
        }

        /// <summary>
        /// Gets/sets the foreign key table name
        /// </summary>
        public string ForeignKeyTableName
        {
            get
            {
                return foreignKeyTableName;
            }
            set
            {
                foreignKeyTableName = value;
            }
        }

        /// <summary>
        /// Gets/sets the foreign key column name
        /// </summary>
        public string ForeignKeyColumnName
        {
            get
            {
                return foreignKeyColumnName;
            }
            set
            {
                foreignKeyColumnName = value;
            }
        }

        /// <summary>
        /// Property indicating if this column needs to be defined as cascade delete
        /// </summary>
        public bool CascadeDelete
        {
            get { return cascadeDelete; }
            set { cascadeDelete = value; }
        }

        /// <summary>
        /// Gets/sets a value indicating whether this is an identity column
        /// </summary>
        public bool IsIdentity
        {
            get
            {
                return isIdentity;
            }
            set
            {
                isIdentity = value;
            }
        }

        #endregion
        
    }
}