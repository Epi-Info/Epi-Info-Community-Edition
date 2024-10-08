using System;
using System.Data;
using System.Data.OleDb;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using Epi.Collections;
using System.IO;
using System.Text;
using Epi.Data;
using System.Globalization;
using Epi.Data.SQLite.Forms;

namespace Epi.Data.SQLite
{
    /// <summary>
    /// Concrete SQLite database implementation
    /// </summary>
    public partial class SQLiteDatabase : OleDbDatabase
    {
        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public SQLiteDatabase() : base()
        {

        }
        #endregion Constructors

        #region Database Implementation
        /// <summary>
        /// Set SQLite database file path
        /// </summary>
        /// <param name="filePath"></param>
        public override void SetDataSourceFilePath(string filePath)
        {
            this.ConnectionString = filePath;
        }

        /// <summary>
        /// Returns the full name of the data source
        /// </summary>
        public override string FullName // Implements Database.FullName
        {
            get
            {
                return "[SQLite] " + Location ?? "";
            }
        }

        /// <summary>
        /// Compact the database
        /// << may only apply to Access databases >>
        /// </summary>
        public override bool CompactDatabase()
        {
            bool success = true;

            Type typeJRO = Type.GetTypeFromProgID("JRO.JetEngine");

            if (typeJRO == null)
            {
                throw new InvalidOperationException("JRO.JetEngine can not be created... please check if it is installed");
            }

            object JRO = Activator.CreateInstance(typeJRO);

            string dataSource = DataSource;
            string dataSource_Temp = dataSource.Insert(dataSource.LastIndexOf('.'),"_Temp");

            object[] paramObjects = new object[] 
                {
                    OleConnectionString,
                    "Provider=Epi.Data.SQLite.1.0.0.0;Data Source=\"" + dataSource_Temp + "\";Jet OLEDB:Engine Type=5"
                };

            try
            {
                JRO.GetType().InvokeMember
                    (
                        "CompactDatabase",
                        System.Reflection.BindingFlags.InvokeMethod,
                        null,
                        JRO,
                        paramObjects
                    );
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(JRO);
                JRO = null;
            }

            if (success)
            {
                System.IO.File.Delete(dataSource);
                System.IO.File.Move(dataSource_Temp, dataSource);
            }

            return success;
        }

        /// <summary>
        /// Adds a column to the table
        /// </summary>
        /// <param name="tableName">name of the table</param>
        /// <param name="column">The column</param>
        /// <returns>Boolean</returns>
        public override bool AddColumn(string tableName, TableColumn column)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("ALTER TABLE ");
            sb.Append(tableName);
            sb.Append(" ADD COLUMN ");
            sb.Append(column.Name);
            sb.Append(" ");
            sb.Append(GetDbSpecificColumnType(column.DataType));
            if (column.Length != null)
            {
                sb.Append("(");
                sb.Append(column.Length.Value.ToString());
                sb.Append(") ");
            }

            ExecuteNonQuery(CreateQuery(sb.ToString()));
            return false;
        }

        /// <summary>
        ///  Creates a table with the given columns
        /// </summary>
        /// <param name="tableName">Table name to be created</param>
        /// <param name="columns">List of columns</param>
        public override void CreateTable(string tableName, List<TableColumn> columns)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sqlitesb = new StringBuilder();

            sb.Append("create table ");
            sb.Append(Util.InsertInSquareBrackets(tableName));
            sb.Append(" ( ");
            sqlitesb.Append("create table ");
            sqlitesb.Append(Util.InsertInSquareBrackets(tableName));
            sqlitesb.Append(" ( ");
            foreach (TableColumn column in columns)
            {
                if (column == null)
                {
                    continue;
                }
                
                if (column.Name.ToLowerInvariant() != "uniquekey")
                {
                    if (column.IsIdentity)
                    {
                        sb.Append("[");
                        sb.Append(column.Name);
                        sb.Append("]");
                        sb.Append(" ");
                        sb.Append(" COUNTER ");
                        sqlitesb.Append("[");
                        sqlitesb.Append(column.Name);
                        sqlitesb.Append("]");
                        sqlitesb.Append(" ");
                        sqlitesb.Append(" INTEGER PRIMARY KEY ");
                    }
                    else
                    {
                        if (column.Name.Contains("."))
                        {
                            sb.Append("[");
                            sb.Append(column.Name.Replace(".","_"));
                            sb.Append("]");
                            sqlitesb.Append("[");
                            sqlitesb.Append(column.Name.Replace(".", "_"));
                            sqlitesb.Append("]");
                        }
                        else
                        {
                            sb.Append("[");
                            sb.Append(column.Name);
                            sb.Append("]");
                            sqlitesb.Append("[");
                            sqlitesb.Append(column.Name);
                            sqlitesb.Append("]");
                        }                        
                        sb.Append(" ");
                        sqlitesb.Append(" ");
                        if (GetDbSpecificColumnType(column.DataType).Equals("text") && column.Length.HasValue && column.Length.Value > 255)
                        {
                            sb.Append("memo");
                            sqlitesb.Append("blob");
                        }
                        else
                        {
                            sb.Append(GetDbSpecificColumnType(column.DataType));
                            sqlitesb.Append(GetDbSpecificColumnType(column.DataType));
                        }
                    }
                }
                else
                {
                    //UniqueKey exists
                    sb.Append("[");
                    sb.Append(column.Name);
                    sb.Append("] counter ");
                    sqlitesb.Append("[");
                    sqlitesb.Append(column.Name);
                    sqlitesb.Append("] INTEGER PRIMARY KEY ");
                }

                if (column.Length != null && column.Name.ToLowerInvariant() != "uniquekey")
                {
                    if (GetDbSpecificColumnType(column.DataType).Equals("text") && column.Length.HasValue && column.Length.Value <= 255 && column.Length.Value > 0)
                    {
                        sb.Append("(");
                        sb.Append(column.Length.Value.ToString());
                        sb.Append(") ");
                        sqlitesb.Append("(");
                        sqlitesb.Append(column.Length.Value.ToString());
                        sqlitesb.Append(") ");
                    }
                }
                if (!column.AllowNull)
                {
                    sb.Append(" NOT ");
                    sqlitesb.Append(" NOT ");
                }
                sb.Append(" null ");
                sqlitesb.Append(" null ");
                if (column.IsPrimaryKey)
                {
                    sb.Append(" constraint");
                    sb.Append(" PK_");
                    sb.Append(column.Name);
                    sb.Append("_");
                    sb.Append(tableName);
                    sb.Append(" primary key ");
                    if (!column.IsIdentity)
                        sqlitesb.Append(" primary key ");
                }
                if (!string.IsNullOrEmpty(column.ForeignKeyColumnName) && !string.IsNullOrEmpty(column.ForeignKeyTableName))
                {
                    sb.Append(" references ");
                    sb.Append(column.ForeignKeyTableName);
                    sb.Append("([");
                    sb.Append(column.ForeignKeyColumnName);
                    sb.Append("]) ");
                    if (column.CascadeDelete)
                    {
                        sb.Append(" on delete cascade");
                    }
                }
                sb.Append(", ");
                sqlitesb.Append(", ");
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append(") ");
            sqlitesb.Remove(sqlitesb.Length - 2, 2);
            sqlitesb.Append(") ");

            ExecuteNonQuery(CreateQuery(sqlitesb.ToString()));
        }
        #endregion

        #region Private Members

        /// <summary>
        /// Gets the database-specific column data type.
        /// </summary>
        /// <param name="dataType">An extension of the System.Data.DbType that adds StringLong (Text, Memo) data type that Epi Info commonly uses.</param>
        /// <returns>Database-specific column data type.</returns>
        public override string GetDbSpecificColumnType(GenericDbColumnType dataType)
        {
            switch (dataType)
            {
                case GenericDbColumnType.AnsiString:
                case GenericDbColumnType.AnsiStringFixedLength:
                    return SQLiteColumnType.Text;

                case GenericDbColumnType.Binary:
                    return "binary";

                case GenericDbColumnType.Boolean:
                    return SQLiteColumnType.YesNo; // "yesno";

                case GenericDbColumnType.Byte:
                    return "byte";

                case GenericDbColumnType.Currency:
                    return SQLiteColumnType.Currency; // "CURRENCY";

                case GenericDbColumnType.Date:
                case GenericDbColumnType.DateTime:
                case GenericDbColumnType.Time:
                    return "datetime";

                case GenericDbColumnType.Decimal:
                case GenericDbColumnType.Double:
                    return "double";

                case GenericDbColumnType.Guid:
                    return "GUID";

                case GenericDbColumnType.Int16:
                case GenericDbColumnType.UInt16:
                    return "SHORT";

                case GenericDbColumnType.Int32:
                case GenericDbColumnType.UInt32:
                    return "integer";

                case GenericDbColumnType.Int64:
                case GenericDbColumnType.UInt64:
                    return "LONG";

                case GenericDbColumnType.Object:
                case GenericDbColumnType.Image: 
                    return "LONGBINARY";

                case GenericDbColumnType.SByte:
                    return "byte";

                case GenericDbColumnType.Single:
                    return "single";

                case GenericDbColumnType.String:
                case GenericDbColumnType.StringFixedLength:
                    return "text";

                case GenericDbColumnType.StringLong:
                case GenericDbColumnType.Xml:
                    return "MEMO";

                case GenericDbColumnType.VarNumeric:
                    return "double";

                default:
                    throw new GeneralException("genericDbColumnType is unknown");
                // return "text";
            }
        }
        /// <summary>
        /// Read only attribute of connection description 
        /// </summary>
        public override string ConnectionDescription
        {
            get { return "SQLite Database: " + Location; }
        }
        /// <summary>
        /// Connection String attribute
        /// </summary>
        public override string ConnectionString
        {
            get
            {
                return base.ConnectionString;
            }
            set
            {
                string connectionString;
                if (Util.IsFilePath(value))
                {
                    connectionString = SQLiteDatabase.BuildConnectionString(value, "");
                }
                else
                {
                    connectionString = value;
                }

                //we can't test the connection now because the database
                // may not be existant (yet)
                base.ConnectionString = connectionString;
            }
        }

        #endregion

        /// <summary>
        /// Gets table schema information about an OLE database
        /// </summary>
        /// <returns>DataTable with schema information</returns>
        //This function has been fixed in parent class, It should work for all database type  //  zack 1/30/08  
        //public override DataTable GetTableSchema()
        //{
        //DataTable schema = base.GetSchema("Tables");
        //foreach (DataRow row in schema.Rows)
        //{
        //    // TODO: This isn't asbolute, should be replaced with search on...
        //    // exact system table names

        //    if (row[ColumnNames.SCHEMA_TABLE_NAME].ToString().StartsWith("MSys", true, CultureInfo.InvariantCulture))
        //    {
        //        row.Delete();
        //    }
        //}
        //schema.AcceptChanges();
        //    return schema;
        //}

        public static string BuildConnectionString(string filePath, string password)
        {
            StringBuilder builder = new StringBuilder();

            if (filePath.EndsWith(".db", true, System.Globalization.CultureInfo.InvariantCulture))
            {
                builder.Append("Provider=Epi.Data.SQLite.1.0.0.0;Data Source=");
            }

            builder.Append(EncodeOleDbConnectionStringValue(filePath));
            builder.Append(";");

            builder.Append("User Id=admin;Password=;");

            if (!string.IsNullOrEmpty(password))
            {
                builder.Append("Jet OLEDB:Database Password=");
                builder.Append(EncodeOleDbConnectionStringValue(password));
                builder.Append(";");
            }


            // sanity check
            OleDbConnectionStringBuilder connectionBuilder = new OleDbConnectionStringBuilder(builder.ToString());
            return connectionBuilder.ToString();
        }

        /// <summary>
        /// Builds a connection string using default parameters given a database name
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="projectName">Name of the project, if any</param>
        /// <returns>A connection string</returns>
        public static string BuildDefaultConnectionString(string databaseName, string projectName = "")
        {
            string configProjectPath = Configuration.GetNewInstance().Directories.Project;

            if (!configProjectPath.EndsWith("\\"))
            {
                configProjectPath = configProjectPath + "\\";
            }

            if (databaseName.EndsWith(".db"))
            {
                if (!string.IsNullOrEmpty(projectName.Trim()))
                {
                    return BuildConnectionString(configProjectPath + projectName.Trim() + "\\" + databaseName, string.Empty);
                }
                else
                {
                    return BuildConnectionString(configProjectPath + databaseName, string.Empty);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(projectName.Trim()))
                {
                    return BuildConnectionString(configProjectPath + projectName.Trim() + "\\" + databaseName + ".db", string.Empty);
                }
                else
                {
                    return BuildConnectionString(configProjectPath + databaseName + ".db", string.Empty);
                }
            }
        }

        /// <summary>
        /// Returns Code Table names for the project
        /// </summary>
        /// <param name="project">Project</param>
        /// <returns>DataTable</returns>
        public override DataTable GetCodeTableNamesForProject(Project project)
        {
            List<string> tables = new List<string>(); //dpb//project.GetDataTableList();
            DataSets.TableSchema.TablesDataTable codeTables = project.GetCodeTableList();

            foreach (DataSets.TableSchema.TablesRow row in codeTables)
            {
                tables.Add(row.TABLE_NAME);
            }

            DataTable bindingTable = new DataTable();
            bindingTable.Columns.Add(ColumnNames.NAME);
            
            foreach (string table in tables)
            {
                if (!string.IsNullOrEmpty(table))
                {
                    if (project.CollectedData.TableExists(table))
                    {
                        bindingTable.Rows.Add(new string[] { table });
                    }
                }
            }

            return bindingTable;
        }

        /// <summary>
        /// Returns code table list
        /// </summary>
        /// <param name="db">IDbDriver</param>
        /// <returns>Epi.DataSets.TableSchema.TablesDataTable</returns>
        public override Epi.DataSets.TableSchema.TablesDataTable GetCodeTableList(IDbDriver db)
        {
            DataSets.TableSchema.TablesDataTable tables = db.GetTableSchema();

            //remove tables without prefix "code"
            DataRow[] rowsFiltered = tables.Select("TABLE_NAME not like 'code%'");
            foreach (DataRow rowFiltered in rowsFiltered)
            {
                tables.Rows.Remove(rowFiltered);
            }
            foreach (DataRow row in tables)
            {
                if (String.IsNullOrEmpty(row.ItemArray[2].ToString()))
                {
                    //remove a row with an empty string
                    tables.Rows.Remove(row);
                }
            }
            DataRow[] rowsCode = tables.Select("TABLE_NAME like 'code%'");

            return tables;
        }

        /// <summary>
        /// Identify Database
        /// Note: This will need to be revisited
        /// </summary>
        /// <returns></returns>
        public override string IdentifyDatabase()
        {
            return "SQLite";
        }
    }
}
