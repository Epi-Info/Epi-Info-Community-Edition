using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Epi;
using Epi.Data;
using VariableCollection = Epi.Collections.NamedObjectCollection<Epi.IVariable>;

namespace Epi.Core.AnalysisInterpreter
{
    /// <summary>
    /// Class DataSourceInfo
    /// </summary>
    public class DataSourceInfo : IDisposable
    {
        #region Private Attributes
        private DataTable primaryTable = null;
        private ArrayList joinTableList = null;
        private VariableCollection standardVariables = null;
        // DataTable inner = null;
        private string selectCriteria = String.Empty;
        private SortCriteria sortCriteria = null;
        #endregion Private Attributes


        #region Public Interface

        #region Constructors
        /// <summary>
        /// Constructor for the class
        /// </summary>
        public DataSourceInfo()
        {
            sortCriteria = new SortCriteria();
        }
        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// Property for the currenet selection criteria
        /// </summary>
        public string SelectCriteria
        {
            get
            {
                return selectCriteria;
            }
        }
        /// <summary>
        /// Accessor for primary table
        /// </summary>
        public DataTable PrimaryTable
        {
            get
            {
                return primaryTable;
            }
            set
            {
                primaryTable = value;
            }
        }

        /// <summary>
        /// Read-only accessor for join table list
        /// </summary>
        public ArrayList JoinTableList
        {
            get
            {
                if (this.joinTableList == null)
                {
                    joinTableList = new ArrayList();
                }
                return joinTableList;
            }
        }

        /// <summary>
        /// Read-only accessor for bridge
        /// </summary>
        public IDbDriver Db
        {
            get
            {
                return ((ITable)PrimaryTable).Database;
            }
        }

        /// <summary>
        /// Sort criteria for the data source
        /// </summary>
        public SortCriteria SortCriteria
        {
            get
            {
                return this.sortCriteria;
            }
        }

        ///// <summary>
        ///// Sort part
        ///// </summary>
        /////
        ////zack 11/26/2007
        //public string SqlStatementPartSort
        //{
        //    get 
        //    { 
        //        return this.sortCriteria;
        //    }
        //    set 
        //    { 
        //        this.sortCriteria = value; 
        //    }
        //}

        /// <summary>
        /// where part
        /// </summary>
        /// <returns></returns>
        /// 
        //zack add 11/19/2007
        public string SqlStatementPartWhere
        {
            get
            {
                return this.selectCriteria;
            }
            set
            {
                this.selectCriteria = value;
            }
        }

        /// <summary>
        /// Order By part
        /// </summary>
        /// <returns></returns>
        public string GetSqlStatementPartSortBy()
        {
            string sortClause = sortCriteria.ToString();
            if (!string.IsNullOrEmpty(sortClause))
            {
                return (" order by " + sortClause);
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Releases all resources used by Analysis DataTable
        /// </summary>
        public void Dispose() // Implements IDisposable.Dispose
        {
            if (joinTableList != null)
            {
                joinTableList.Clear();
                joinTableList = null;
            }
            if (standardVariables != null)
            {
                standardVariables.Clear();
                standardVariables = null;
            }
        }


        /// <summary>
        /// Tests the (partial) where clause that the user has entered by trying to execute an sql command
        /// </summary>
        /// <remarks> 
        /// This was neccessary because, when the user put in the name of a nonexistant variable
        /// the user was getting errors from the underlying database that was less than helpful
        /// At this time it is called by select command and disallows the user to generate a where that doesn't work.
        /// Defect 187
        /// </remarks>
        /// <param name="expression"></param>
        /// <returns>bool</returns>
        public bool ValidWhereClause(Rule_Context pContext, string expression)
        {
            try
            {
                string testExpression = expression;

                testExpression = testExpression.Replace(StringLiterals.EPI_REPRESENTATION_OF_TRUE, "1");
                testExpression = testExpression.Replace(StringLiterals.EPI_REPRESENTATION_OF_FALSE, "0");
                //DataRow[] rows = pContext.GetOutput().Select(testExpression);
                return true;
            }
            catch (Exception)
            {
                throw new GeneralException(string.Format(SharedStrings.INVALID_EXPRESSION, "\"" + expression.Trim() + "\""));
            }
        }
        /// <summary>
        /// Gets a count of records in a column
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        /// <returns>Number of records in a column</returns>
        public int GetRecordCount(Rule_Context pContext, string columnName)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException("columnName");
            }
            #endregion Input Validation
            WordBuilder queryBuilder = new WordBuilder();
            queryBuilder.Append("select count" + Util.InsertInParantheses(columnName));
            queryBuilder.Append(GetSqlStatementPartFrom(pContext));
            if (Db.ColumnExists(PrimaryTable.TableName, ColumnNames.REC_STATUS))
            {
                string whereStatement = GetSqlStatementPartWhere();
                queryBuilder.Append(whereStatement);
            }
            else if (!string.IsNullOrEmpty(selectCriteria))
            {
                string whereClause = " where " + this.selectCriteria;
                queryBuilder.Append(whereClause);
            }
            Query query = Db.CreateQuery(queryBuilder.ToString());
            //recast to int before return to remove cast run time error
            int result = Int32.Parse((Db.ExecuteScalar(query)).ToString());
            return result;
        }


        /// <summary>
        /// Logically delete or undelete the records (change RecStatus) that match the criteria
        /// </summary>
        /// <remarks>
        /// Criteria may be '*', which is all records; observes current selection criteria
        /// </remarks>
        /// <param name="criteria"></param>
        /// <param name="delete">Delete or undelete the records that match the criteria</param>
        /// <returns>The number of rows affected</returns>
        public int LogicallyDeleteRecords(string criteria, bool delete)
        {
            string tableName = PrimaryTable.TableName;
            string where = GetSqlStatementPartWhere();
            if (!string.IsNullOrEmpty(criteria))
            {
                if (string.IsNullOrEmpty(where))
                {
                    where = " where ";
                }
                else if (criteria != "*")
                {
                    where += " and ";
                }
                where += criteria;
            }
            string sql = "Update " + tableName + " set recstatus = " + ((delete) ? "0" : "1") + where;
            Epi.Data.Query qry = Db.CreateQuery(sql);
            return Db.ExecuteNonQuery(qry);

        }

        /// <summary>
        /// Physically (permanently) delete the records that match the criteria
        /// </summary>
        /// <remarks>
        /// Criteria may be '*', which is all records; observes current selection criteria
        /// </remarks>
        /// <param name="criteria"></param>
        /// <returns>The number of rows affected</returns>
        public int PhysicallyDeleteRecords(string criteria)
        {
            string tableName = PrimaryTable.TableName;
            string where = GetSqlStatementPartWhere();
            if (!string.IsNullOrEmpty(criteria))
            {
                if (string.IsNullOrEmpty(where))
                {
                    where = " where ";
                }
                else if (criteria != "*")
                {
                    where += " and ";
                }
                where += criteria;
            }

            string sql = "Delete from " + tableName + where;
            Epi.Data.Query qry = Db.CreateQuery(sql);
            return Db.ExecuteNonQuery(qry);

        }

        /// <summary>
        /// Gets count of all the records in a table
        /// </summary>
        /// <returns>Number of records in a table</returns>
        public int GetRecordCount(Rule_Context pContext)
        {
            return GetRecordCount(pContext, StringLiterals.STAR);
        }

        /// <summary>
        /// Get Data of a single column and all strata
        /// </summary>
        /// <remarks>
        /// If the variable is a defined variable, use the expression
        /// </remarks>
        /// <param name="var"></param>
        /// <param name="strata"></param>
        /// <returns></returns>
        public DataTable GetData(Rule_Context pContext, IVariable var, string[] strata)
        {
            #region Preconditions
            if (strata == null)
            {
                return GetData(pContext, var);
            }
            #endregion

            StringBuilder sql = new StringBuilder("select ");
            if (var.VarType == VariableType.DataSource)
            {
                sql.Append("[").Append(var.Name).Append("]");
            }
            else
            {
                sql.Append(var.Expression);
            }
            if (strata.GetLength(0) > 0)
            {
                int count = strata.GetLength(0);
                for (int i = 0; i < count; i++)
                {
                    sql.Append(", [").Append(strata[i].ToString()).Append("]");
                }
            }
            sql.Append(this.GetSqlStatementPartFrom(pContext));
            sql.Append(this.GetSqlStatementPartWhere());
            sql.Append(this.GetSqlStatementPartSortBy());
            Query query = Db.CreateQuery(sql.ToString());
            return Db.Select(query);
        }
        /// <summary>
        /// Get Data of a single column
        /// </summary>
        /// <remarks>
        /// If the variable is a defined variable, use the expression
        /// </remarks>
        /// <param name="var"></param>
        /// <returns></returns>
        public DataTable GetData(Rule_Context pContext, IVariable var)
        {
            StringBuilder sql = new StringBuilder("select ");
            if (var.VarType == VariableType.DataSource)
            {
                sql.Append("[").Append(var.Name).Append("]");
            }
            else
            {
                sql.Append(var.Expression);
            }
            sql.Append(this.GetSqlStatementPartFrom(pContext));
            sql.Append(this.GetSqlStatementPartWhere());
            sql.Append(this.GetSqlStatementPartSortBy());
            Query query = Db.CreateQuery(sql.ToString());
            return Db.Select(query);
        }


        /// <summary>
        /// Fills the inner table by fetching data, adding standard and global variables.
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetData(Rule_Context pContext)
        {
            string queryString = "select * "
                                + this.GetSqlStatementPartFrom(pContext)
                                + this.GetSqlStatementPartWhere()
                                + this.GetSqlStatementPartSortBy();
            Query query = Db.CreateQuery(queryString);
            return Db.Select(query);
        }

        string ExcludeMissing(string currentWhereClause, string[] names)
        {
            StringBuilder sb = new StringBuilder(currentWhereClause);
            if (!Configuration.GetNewInstance().Settings.IncludeMissingValues)
            {
                foreach (string name in names)
                {
                    if (string.IsNullOrEmpty(sb.ToString()))
                    {
                        sb.Append(" where ");
                    }
                    else
                    {
                        sb.Append(" and [").Append(name).Append("] is not null");
                    }
                }
            }
            return sb.ToString();

        }



        /// <summary>
        /// Returns a 2x2 table for the tables command
        /// </summary>
        /// <remarks>The cells are numbered as they are in Epi3</remarks>
        /// <param name="outcome"></param>
        /// <param name="exposure"></param>
        /// <returns>DataTable</returns>
        public DataSet GetDataSet2x2(Rule_Context pContext, string exposure, string outcome)
        {
            DataSet ds = null;
            try
            {
                StringBuilder sb = new StringBuilder("select ");
                sb.Append(exposure).Append(" AS [").Append(ColumnNames.EXPOSURE).Append("], [");
                sb.Append(outcome).Append("] AS [").Append(ColumnNames.OUTCOME);
                sb.Append("], count([").Append(outcome).Append("]) AS [").Append(ColumnNames.COUNT);
                sb.Append("] ");
                sb.Append(GetSqlStatementPartFrom(pContext));

                //if there is no project, there will not be a column Rec Status
                if (PrimaryTable.Columns.Contains(ColumnNames.REC_STATUS))
                {
                    sb.Append(ExcludeMissing(GetSqlStatementPartWhere(),
                                     new string[] { exposure, outcome }));
                }
                //a natively read MDB, and SELECT chosen
                else if (!string.IsNullOrEmpty(selectCriteria))
                {
                    string whereClause = " where " + this.selectCriteria;
                    sb.Append(ExcludeMissing(whereClause, new string[] { exposure, outcome }));
                }
                sb.Append(" group by [").Append(exposure).Append("], [").Append(outcome);
                sb.Append("] order by [").Append(exposure);
                sb.Append("], [").Append(outcome).Append("];");

                DataTable Table2x2 = DBReadExecute.GetDataTable(pContext.CurrentRead.File, sb.ToString());

                //Query query = Db.CreateQuery(sb.ToString());
                //DataTable Table2x2 = Db.Select(query);
                Table2x2.TableName = "Table2x2";
                ds = new DataSet("dsTable2x2");
                ds.Tables.Add(Table2x2);
                //DataTable distinctOutcomes = DistinctColumn(outcome);
                //distinctOutcomes.TableName = "DistinctOutcomes";
                //ds.Tables.Add(distinctOutcomes);
            }
            catch (Exception ex)
            {
                throw new GeneralException(SharedStrings.UNABLE_CREATE_2X2, ex);
            }
            return ds;
        }

        /// <summary>
        /// Gets the frequencies of an outcome variable stratified by one or more strata
        /// </summary>
        /// <param name="var">The outcome variable (IVariable)</param>
        /// <param name="strata">An array of the names of strata variables</param>
        /// <param name="weightVar">Weighted variable.</param>
        /// <returns>The frequency table</returns>
        public DataSet GetFrequencies(Rule_Context pContext, IVariable var, string[] strata, string weightVar)
        {
            StringBuilder sb = new StringBuilder("select ");
            if (var.VarType != VariableType.DataSource)
            {
                sb.Append("(").Append(var.Expression).Append(") AS ");
            }
            sb.Append(var.Name).Append(", count(*) AS " + ColumnNames.FREQUENCY);

            if (!string.IsNullOrEmpty(weightVar))  //if weighted.
            {
                sb.Append(", sum(").Append(weightVar).Append(") as ").Append(ColumnNames.WEIGHT);
            }
            else
            {
                sb.Append(", 1 as ").Append(ColumnNames.WEIGHT);
            }
            if (strata != null)
            {
                foreach (string stratum in strata)
                {
                    sb.Append(", ").Append(stratum);
                }
            }
            sb.Append(GetSqlStatementPartFrom(pContext));
            string s = null;
            if (Db.ColumnExists(PrimaryTable.TableName, ColumnNames.REC_STATUS))
            {
                s = GetSqlStatementPartWhere();
            }
            else if (!string.IsNullOrEmpty(selectCriteria))
            {
                s = " where " + this.selectCriteria;
            }
            if (!string.IsNullOrEmpty(s))
            {
                sb.Append(s);
            }
            if (!Configuration.GetNewInstance().Settings.IncludeMissingValues && strata != null)
            {
                sb.Append((string.IsNullOrEmpty(s)) ? " where " : " and ");
                foreach (string stratum in strata)
                {
                    sb.Append(stratum).Append(" is not null and ");
                }
                sb.Length -= 4;
            }
            sb.Append(" group by ");
            if (var.VarType == VariableType.DataSource)
            {
                sb.Append(var.Name);
            }
            else
            {
                sb.Append(var.Expression);
            }
            if (strata != null)
            {
                foreach (string stratum in strata)
                {
                    sb.Append(", ").Append(stratum);
                }
                sb.Append(" order by ");
                foreach (string stratum in strata)
                {
                    sb.Append(stratum).Append(",");
                }
                sb.Append(var.Name).Append(" desc;");
            }
            string queryString = sb.ToString();
            if (string.IsNullOrEmpty(queryString))
            {
                return null;
            }
            else
            {
                DataSet ds = new DataSet("FreqDataSet");
                if (strata != null)
                {
                    ds.Tables.Add(DistinctStrata(pContext, strata));
                }

                DataTable freq = Db.Select(Db.CreateQuery(queryString));
                freq.TableName = "Frequencies";
                ds.Tables.Add(freq);
                return ds;
            }
        }



        /// <summary>
        /// Gets the frequency table of a column
        /// </summary>
        /// <param name="variable"></param>
        /// <returns>The frequency table</returns>
        public DataTable GetFrequency(Rule_Context pContext, IVariable variable)
        {
            string columnName = variable.Name;
            string s = (string.IsNullOrEmpty(this.SqlStatementPartWhere)) ? " where " : " and ";
            string queryString = "select " + columnName + ", Count(*) AS " + ColumnNames.FREQUENCY;
            queryString += " From (select ";
            if (variable.VarType != VariableType.DataSource)
            {
                queryString += variable.Expression + " as ";
            }
            queryString += columnName + this.GetSqlStatementPartFrom(pContext)
                + this.GetSqlStatementPartWhere()
                + ") as TRecode Group by " + columnName;
            if (string.IsNullOrEmpty(queryString))
            {
                return null;
            }
            else
            {
                Query query = Db.CreateQuery(queryString);
                return Db.Select(query);
            }
        }

        /// <summary>
        /// Gets a list of all the columns in a view
        /// </summary>
        /// <returns>DataTable containing a list of columns</returns>
        //public List<string> GetColumnNames()
        //{
        //    List<string> columnNames = new List<string>();
        //    columnNames.AddRange(PrimaryTable.TableColumnNames);

        //    foreach (JoinTable joinTable in this.joinTableList)
        //    {
        //        columnNames.AddRange(joinTable.Table.TableColumnNames);
        //    }
        //    return columnNames;

        //}

        /// <summary>
        /// where part
        /// </summary>
        /// <returns></returns>
        //zack change this method to public, 11/21/07
        public string GetSqlStatementPartWhere()
        {
            string whereClause = string.Empty;
            if (!string.IsNullOrEmpty(selectCriteria))
            {
                whereClause = " where " + this.selectCriteria;
            }
            int scope = Configuration.GetNewInstance().Settings.RecordProcessingScope;
            if (scope != (int)(RecordProcessingScope.Both))
            {
                whereClause += (string.IsNullOrEmpty(whereClause)) ? " where " : " and ";
                whereClause += "[" + ColumnNames.REC_STATUS + "] = ";
                whereClause += (scope == (int)(RecordProcessingScope.Deleted)) ? "0" : "1";
            }
            return whereClause;
        }

        /// <summary>
        /// Call DistictinctStrata with only one column name
        /// </summary>
        /// <param name="colName"></param>
        /// <returns></returns>
        public DataTable DistinctColumn(Rule_Context pContext, string colName)
        {
            #region Preconditions
            if (string.IsNullOrEmpty(colName))
            {
                return null;
            }
            #endregion preconditions
            return DistinctStrata(pContext, new string[1] { colName });
        }

        /// <summary>
        /// returns a DataTable with the DISTINCT number of values per stratum
        /// </summary>
        /// <param name="strata"></param>
        /// <returns></returns>
        public DataTable DistinctStrata(Rule_Context pContext, string[] strata)
        {
            #region Preconditions
            if (strata == null)
            {
                return null;
            }
            #endregion preconditions

            StringBuilder strataQuery = new StringBuilder("select distinct ");
            foreach (string stratum in strata)
            {
                strataQuery.Append(stratum).Append(", ");
            }
            strataQuery.Length -= 2;
            strataQuery.Append(GetSqlStatementPartFrom(pContext));

            //for distinguishing between native MDB vs projects
            if (PrimaryTable.Columns.Contains(ColumnNames.REC_STATUS))
            {
                strataQuery.Append(ExcludeMissing(GetSqlStatementPartWhere(), strata));
            }
            else if (!string.IsNullOrEmpty(selectCriteria))
            {
                string whereClause = " where " + this.selectCriteria;
                strataQuery.Append(ExcludeMissing(whereClause, strata));
            }

            strataQuery.Append(" order by ");
            foreach (string stratum in strata)
            {
                strataQuery.Append(stratum).Append(" desc, ");
            }
            strataQuery.Length -= 2;
            DataTable strataTable = new DataTable("Strata");
            strataTable = Db.Select(Db.CreateQuery(strataQuery.ToString()), strataTable);
            return strataTable;
        }

        #endregion Public Methods


        #endregion Public Interface

        #region Private Methods


        /// <summary>
        /// from part 
        /// </summary>
        /// <returns></returns>
        private string GetSqlStatementPartFrom(Rule_Context pContext)
        {
            string fromClause = " from " + pContext.CurrentRead.Identifier;
            foreach (JoinTable joinTable in JoinTableList)
            {
                fromClause += " inner join " + joinTable.Table.TableName;
                fromClause += " on " + joinTable.Condition;
            }
            return fromClause;
        }


        #endregion Private Methods

    }

    /// <summary>
    /// The JoinTable class
    /// </summary>
    public class JoinTable
    {
        #region Private Class Members
        private ITable table = null;
        private string condition = string.Empty;
        #endregion Private Class Members

        #region Constructors
        /// <summary>
        /// Default Constructor 
        /// </summary>
        public JoinTable()
        {
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition">Condition for the join</param>
        public JoinTable(ITable table, string condition)
        {
            this.Table = table;
            this.Condition = condition;
        }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Accessor for table
        /// </summary>
        public ITable Table
        {
            get
            {
                return table;
            }
            set
            {
                table = value;
            }
        }

        /// <summary>
        /// Accessor for condition
        /// </summary>
        public string Condition
        {
            get
            {
                return this.condition;
            }
            set
            {
                this.condition = value;
            }
        }
        #endregion Public Properties
    }

}
