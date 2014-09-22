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
using Epi.Data.Office.Forms;

namespace Epi.Data.Office
{
    /// <summary>
    /// Concrete Microsoft CSV File implementation
    /// </summary>
    public partial class CsvFile : OleDbDatabase
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public CsvFile() : base() { }

        #region Database Implementation
        /// <summary>
        /// Set Access database file path
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
                return "[CSV File] " + Location ?? "";
            }
        }

        protected override OleDbConnection GetNativeConnection(string connectionString)
        {
            OleDbConnectionStringBuilder oleDBCnnStrBuilder = new OleDbConnectionStringBuilder(connectionString);
            oleDBCnnStrBuilder.Provider = "Microsoft.Jet.OLEDB.4.0";
            oleDBCnnStrBuilder.Add("Extended Properties", "text;HDR=Yes;FMT=Delimited");
            return new OleDbConnection(oleDBCnnStrBuilder.ToString());
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
            string dataSource_Temp = dataSource.Insert(dataSource.LastIndexOf('.'), "_Temp");

            object[] paramObjects = new object[] 
                {
                    OleConnectionString,
                    "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\"" + dataSource_Temp + "\";Jet OLEDB:Engine Type=5"
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
        ///  Creates a table with the given columns
        /// </summary>
        /// <param name="tableName">Table name to be created</param>
        /// <param name="columns">List of columns</param>
        public override void CreateTable(string tableName, List<TableColumn> columns)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("create table ");
           // sb.Append(tableName);
            sb.Append(Util.InsertInSquareBrackets(tableName));
            sb.Append(" ( ");
            foreach (TableColumn column in columns)
            {
                if (column.Name.ToLower() != "uniquekey")
                {
                    if (column.IsIdentity)
                    {
                        sb.Append("[");
                        sb.Append(column.Name);
                        sb.Append("]");
                        sb.Append(" ");
                        sb.Append(" COUNTER ");
                    }
                    else
                    {
                        sb.Append("[");
                        sb.Append(column.Name);
                        sb.Append("]");
                        sb.Append(" ");
                        if (GetDbSpecificColumnType(column.DataType).Equals("text") && column.Length.HasValue && column.Length.Value > 255)
                        {
                            sb.Append("memo");
                        }
                        else
                        {
                            sb.Append(GetDbSpecificColumnType(column.DataType));
                        }
                    }
                }
                else
                {
                    //UniqueKey exists
                    sb.Append("[");
                    sb.Append(column.Name);
                    sb.Append("] counter ");
                }

                if (column.Length != null)
                {
                    if (GetDbSpecificColumnType(column.DataType).Equals("text") && column.Length.HasValue && column.Length.Value <= 255 && column.Length.Value > 0)
                    {
                        sb.Append("(");
                        sb.Append(column.Length.Value.ToString());
                        sb.Append(") ");
                    }
                }
                if (!column.AllowNull)
                {
                    sb.Append(" NOT ");
                }
                sb.Append(" null ");
                if (column.IsPrimaryKey)
                {
                    sb.Append(" constraint");
                    sb.Append(" PK_");
                    sb.Append(column.Name);
                    sb.Append("_");
                    sb.Append(tableName);
                    sb.Append(" primary key ");
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
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append(") ");

            ExecuteNonQuery(CreateQuery(sb.ToString()));
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
                    return AccessColumnType.Text;

                case GenericDbColumnType.Binary:
                    return "binary";

                case GenericDbColumnType.Boolean:
                    return AccessColumnType.YesNo; 

                case GenericDbColumnType.Byte:
                    return "byte";

                case GenericDbColumnType.Currency:
                    return AccessColumnType.Currency; 

                case GenericDbColumnType.Date:
                case GenericDbColumnType.DateTime:
                case GenericDbColumnType.Time:
                    return "datetime";

                case GenericDbColumnType.Decimal:
                case GenericDbColumnType.Double:
                    return "double";

                case GenericDbColumnType.Guid:
                    return "text";

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
            }
        }
        /// <summary>
        /// Read only attribute of connection description 
        /// </summary>
        public override string ConnectionDescription
        {
            get { return "CSV File: " + Location; }
        }
        /// <summary>
        /// Connection String attribute
        /// </summary>
        public override string ConnectionString
        {
            get
            {
                OleDbConnectionStringBuilder cnnBuilder = new OleDbConnectionStringBuilder(base.ConnectionString);
                cnnBuilder.Provider = "Microsoft.Jet.OLEDB.4.0";
                cnnBuilder.Add("Extended Properties", "text;HDR=Yes;FMT=Delimited");
                return cnnBuilder.ToString();
            }
            set
            {
                string connectionString;
                if (Util.IsFilePath(value))
                {
                    connectionString = CsvFile.BuildConnectionString(value, "");
                }
                else
                {
                    connectionString = value;
                }
                base.ConnectionString = connectionString;
            }
        }
        #endregion

        public static string BuildConnectionString(string filePath, string password)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=");

            builder.Append(EncodeOleDbConnectionStringValue(filePath));
            builder.Append(";Extended Properties=\"text;HDR=Yes;FMT=Delimited\";");

            // sanity check
            OleDbConnectionStringBuilder connectionBuilder = new OleDbConnectionStringBuilder(builder.ToString());
            return connectionBuilder.ToString();
        }

        /// <summary>
        /// Builds a connection string using default parameters given a database name
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        /// <returns>A connection string</returns>
        public static string BuildDefaultConnectionString(string databaseName)
        {
            string cnnString = BuildConnectionString(Configuration.GetNewInstance().Directories.Project + "\\" + databaseName + ".csv", string.Empty);
            cnnString += ";Extended Properties=\"text;HDR=Yes;FMT=Delimited\";";
            return cnnString;
        }

        /// <summary>
        /// Returns Code Table names for the project
        /// </summary>
        /// <param name="project">Project</param>
        /// <returns>DataTable</returns>
        public override DataTable GetCodeTableNamesForProject(Project project)
        {
            List<string> tables = project.GetDataTableList();
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
            return "CSV";
        }
    }
}
