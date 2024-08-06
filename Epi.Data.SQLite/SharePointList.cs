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

namespace Epi.Data.SQLite
{
    /// <summary>
    /// Concrete Microsoft SharePoint list implementation
    /// </summary>
    public class SharePointList : OleDbDatabase
    {

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SharePointList():base(){}


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
                return "[SharePoint] " + Location ?? "";
            }
        }

        protected override OleDbConnection GetNativeConnection(string connectionString)
        {
            return new OleDbConnection(connectionString);
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
            sb.Append(tableName);
            sb.Append(" ( ");
            foreach (TableColumn column in columns)
            {
                if (column.Name.ToLowerInvariant() != "uniquekey")
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
                        sb.Append(GetDbSpecificColumnType(column.DataType));
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
                    if (GetDbSpecificColumnType(column.DataType).Equals("text"))
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
                    return AccessColumnType.YesNo; // "yesno";

                case GenericDbColumnType.Byte:
                    return "byte";

                case GenericDbColumnType.Currency:
                    return AccessColumnType.Currency; // "CURRENCY";

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
            get { return ConnectionString.ToUpperInvariant(); }
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
                base.ConnectionString = value.ToUpperInvariant();
            }
        }

        #endregion


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
            return "SharePoint";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFileString"></param>
        /// <param name="pTableName"></param>
        /// <param name="pIsConnectionString"></param>
        /// <returns></returns>
        public override bool CheckDatabaseTableExistance(string pFileString, string pTableName, bool pIsConnectionString = false)
        {
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFileString"></param>
        /// <returns></returns>
        public override bool CreateDataBase(string pFileString)
        {
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFileString"></param>
        /// <param name="pTableName"></param>
        /// <param name="pIsConnectionString"></param>
        /// <returns></returns>
        public override bool CheckDatabaseExistance(string pFileString, string pTableName, bool pIsConnectionString = false)
        {
            return false;
        }
    }
}