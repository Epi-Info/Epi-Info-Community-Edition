using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;
using Epi;
using Epi.Core;
using Epi.Data;

namespace EpiDashboard
{
    /// <summary>
    /// Represents the allowable types of variables that may store recoded data through the dashboard
    /// </summary>
    public enum ConditionJoinType
    {
        And = 0,
        Or = 1
    }

    /// <summary>
    /// Represents a data filter, made up of one or more filter conditions
    /// </summary>
    public class DataFilters : IEnumerable<FilterCondition>
    {
        #region Private Members
        private Dictionary<string, string> operandTypes;
        private DataTable conditionTable;
        private RecordProcessingScope recordProcessScope;
        private DashboardHelper dashboardHelper;
        #endregion // Private Members

        #region Constants
        public const string COLUMN_ID = "id";          // the column ID, e.g. 1, 2, 3, 4. Used to sort the conditions.
        public const string COLUMN_JOIN = "join";      // how the current row should be joined to this condition; should be either AND or OR        
        public const string COLUMN_FILTER = "filter";  // the actual filterCondition object
        public const string COLUMN_GROUP = "group";    // the grouping; used for advanced conditions where parenthesis may be needed, e.g. SELECT ... WHERE (SEX = 'M' AND Age > 25) OR (SEX = 'F' AND Age < 55)
        public const string COLUMN_ACTIVE = "active";  // column used for determining whether the condition is active. Should not be used except in tables that are returned from this class
        public const string COLUMN_FRIENDLY_FILTER = "Filter criteria";
        public const string COLUMN_FRIENDLY_JOIN = "Join";     
        #endregion // Constants

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public DataFilters(DashboardHelper dashboardHelper)
        {
            operandTypes = new Dictionary<string, string>();
            operandTypes.Add(SharedStrings.FRIENDLY_OPERATOR_BETWEEN, string.Empty);
            operandTypes.Add(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO, StringLiterals.EQUAL);
            operandTypes.Add(SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN, StringLiterals.GREATER_THAN);
            operandTypes.Add(SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN_OR_EQUAL, StringLiterals.GREATER_THAN_OR_EQUAL);
            operandTypes.Add(SharedStrings.FRIENDLY_OPERATOR_LESS_THAN, StringLiterals.LESS_THAN);
            operandTypes.Add(SharedStrings.FRIENDLY_OPERATOR_LESS_THAN_OR_EQUAL, StringLiterals.LESS_THAN_OR_EQUAL);
            operandTypes.Add(SharedStrings.FRIENDLY_OPERATOR_LIKE, "like");
            operandTypes.Add(SharedStrings.FRIENDLY_OPERATOR_MISSING, "is null");
            operandTypes.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_EQUAL_TO, "<>");
            operandTypes.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING, "is not null");
            operandTypes.Add(SharedStrings.FRIENDLY_OPERATOR_AND, "and");
            operandTypes.Add(SharedStrings.FRIENDLY_OPERATOR_OR, "or");
            operandTypes.Add(SharedStrings.FRIENDLY_OPERATOR_IS_ANY_OF, "in");
            operandTypes.Add(SharedStrings.FRIENDLY_OPERATOR_IS_NOT_ANY_OF, "not in");

            DataColumn idColumn = new DataColumn(COLUMN_ID, typeof(int));
            DataColumn filterColumn = new DataColumn(COLUMN_FILTER, typeof(FilterCondition));
            
            idColumn.AllowDBNull = false;
            filterColumn.AllowDBNull = false;

            recordProcessScope = RecordProcessingScope.Undeleted;

            conditionTable = new DataTable("FilterTable");
            ConditionTable.Columns.Add(idColumn);
            ConditionTable.Columns.Add(COLUMN_JOIN, typeof(string));
            ConditionTable.Columns.Add(filterColumn);
            ConditionTable.Columns.Add(COLUMN_GROUP, typeof(int));
            
            this.dashboardHelper = dashboardHelper;
            
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets the number of elements contained in the DataFilter
        /// </summary>
        public int Count
        {
            get
            {
                if (this.ConditionTable != null)
                {
                    return this.ConditionTable.Rows.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Gets/sets the current record processing scope
        /// </summary>
        /// <remarks>Only valid for Epi Info 7 projects, as non-Epi projects generally do not implement a RECSTATUS column</remarks>
        public RecordProcessingScope RecordProcessScope
        {
            get
            {
                return this.recordProcessScope;
            }
            set
            {
                recordProcessScope = value;
            }
        }
        #endregion // Public Properties

        #region Private Properties
        /// <summary>
        /// Gets the data table containing the various filter conditions
        /// </summary>
        private DataTable ConditionTable 
        {
            get
            {
                return this.conditionTable;
            }
        }

        /// <summary>
        /// Gets the attached dashboard helper
        /// </summary>
        private DashboardHelper DashboardHelper
        {
            get
            {
                return this.dashboardHelper;
            }
        }
        #endregion // Private Properties

        #region Public Methods
        /// <summary>
        /// Adds a new filter condition to the data filter
        /// </summary>
        /// <param name="condition">The condition to add</param>
        /// <returns>Boolean; whether the addition was successful</returns>
        public bool AddFilterCondition(FilterCondition condition, ConditionJoinType joinType)
        {
            #region Input Validation
            if (condition == null || string.IsNullOrEmpty(condition.ColumnName))
            {
                throw new ApplicationException("Invalid filter condition. Cannot add condition to data filter.");
            }
            #endregion // Input Validation

            // check for duplicate conditions
            foreach (DataRow row in ConditionTable.Rows)
            {
                FilterCondition rCondition = (FilterCondition)row[COLUMN_FILTER];
                
                if (rCondition.FriendlyCondition.Equals(condition.FriendlyCondition))
                {
                    // cannot add a duplicate condition, so return false
                    return false;
                }
            }

            // find the highest ID value of all the conditions presently in the table
            int maxInt = int.MinValue;

            foreach (DataRow row in ConditionTable.Rows)
            {
                int value = row.Field<int>(COLUMN_ID);                
                maxInt = Math.Max(maxInt, value);
            }

            // get the string representation of the joinType enum
            string join = ConvertJoinType(joinType);

            if (this.Count == 0)
            {
                maxInt = 0;
                join = string.Empty;
            }

            // assume group = 1
            ConditionTable.Rows.Add(maxInt + 1, join, condition, 1);
            
            // sort by ID
            this.conditionTable = ConditionTable.Select("", COLUMN_ID).CopyToDataTable().DefaultView.ToTable("FilterTable");

            return true;
        }

        /// <summary>
        /// Clears all filter conditions
        /// </summary>
        public void ClearFilterConditions()
        {
            this.ConditionTable.Rows.Clear();
        }

        /// <summary>
        /// Create the data filters from an XML element
        /// </summary>
        /// <param name="element">The XML element from which to create the filter</param>
        public void CreateFromXml(XmlElement element)
        {
            string joinType = "and";

            foreach (System.Xml.XmlElement iChild in element.ChildNodes)
            {
                if (iChild.Name.Equals("filterCondition"))
                {
                    FilterCondition newCondition = new FilterCondition();
                    newCondition.CreateFromXml(iChild);

                    foreach (XmlElement jChild in iChild.ChildNodes)
                    {
                        if (jChild.Name.Equals("joinType"))
                        {
                            joinType = jChild.InnerText;
                        }
                    }

                    switch (joinType.ToLower())
                    {
                        case "or":
                            this.AddFilterCondition(newCondition, ConditionJoinType.Or);
                            break;
                        default:
                            this.AddFilterCondition(newCondition, ConditionJoinType.And);
                            break;
                    }                    
                }
            }
        }

        /// <summary>
        /// Convert the data filter conditions into XML
        /// </summary>
        /// <returns>XmlElement</returns>
        public XmlElement Serialize(XmlDocument doc)
        {            
            System.Xml.XmlElement root = doc.CreateElement("dataFilters");

            System.Xml.XmlAttribute type = doc.CreateAttribute("recordProcessScope");

            switch (this.RecordProcessScope)
            {
                case RecordProcessingScope.Undeleted:
                    type.Value = "undeleted";
                    break;
                case RecordProcessingScope.Deleted:
                    type.Value = "deleted";
                    break;
                case RecordProcessingScope.Both:
                    type.Value = "both";
                    break;
            }
            
            root.Attributes.Append(type);

            foreach (DataRow row in ConditionTable.Rows)
            {
                FilterCondition filterCondition = (FilterCondition)row[COLUMN_FILTER];

                root.AppendChild(filterCondition.Serialize(doc, row[COLUMN_FRIENDLY_JOIN].ToString().ToLower()));
            }
            return root;
        }

        /// <summary>
        /// Removes a filter condition
        /// </summary>
        /// <returns>Bool</returns>
        public bool RemoveFilterCondition(string friendlyCondition)
        {
            int i; 
            int rowToRemove = -1;
            DataRow row = null;

            for(i = 0; i < ConditionTable.Rows.Count; i++)
            {
                row = ConditionTable.Rows[i];
                string str = row[COLUMN_FILTER].ToString();

                if (str.Equals(friendlyCondition))
                {
                    rowToRemove = i;
                    break;
                }
            }

            if (rowToRemove >= 0)
            {
                ConditionTable.Rows.Remove(ConditionTable.Rows[rowToRemove]);

                if (ConditionTable.Rows.Count > 0)
                {
                    this.conditionTable = ConditionTable.Select("", COLUMN_ID).CopyToDataTable().DefaultView.ToTable("FilterTable");

                    if (rowToRemove == 0)
                    {
                        ConditionTable.Rows[0][COLUMN_JOIN] = string.Empty;
                    }
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Generates a FilterCondition object using primitive type inputs
        /// </summary>        
        /// <param name="columnName">The name of the column on which to filter</param>
        /// <param name="rawColumnName">The name of the column on which to filter, without brackets</param>
        /// <param name="columnType">The data type of the column on which to filter</param>
        /// <param name="friendlyOperand">The friendly operand</param>
        /// <param name="friendlyValue">The friendly value</param>
        /// <returns>FilterCondition</returns>
        public FilterCondition GenerateFilterCondition(string columnName, string rawColumnName, string columnType, string friendlyOperand, string friendlyValue)
        {
            #region Input Validation
            if(string.IsNullOrEmpty(columnName))
            {
                throw new ApplicationException("Column name cannot be empty.");
            }
            else if (string.IsNullOrEmpty(friendlyOperand))
            {
                throw new ApplicationException("Friendly operand cannot be empty.");
            }
            else if (string.IsNullOrEmpty(columnType))
            {
                throw new ApplicationException("Column type cannot be empty.");
            }
            else if (!operandTypes.ContainsKey(friendlyOperand))
            {
                throw new ApplicationException("Operand not found in dictionary.");
            }
            #endregion // Input Validation

            Configuration config = dashboardHelper.Config;
            
            Epi.Fields.Field field = null;

            foreach (DataRow fieldRow in dashboardHelper.FieldTable.Rows)
            {
                if (fieldRow["columnname"].Equals(rawColumnName))
                {   
                    if (fieldRow["epifieldtype"] is Epi.Fields.Field)
                    {
                        field = fieldRow["epifieldtype"] as Epi.Fields.Field;
                    }
                    break;
                }
            }

            string operand = string.Empty;
            string value = string.Empty;
            string friendlyCondition = string.Empty;
            
            operand = operandTypes[friendlyOperand];

            switch (columnType)
            {
                case "System.DateTime":
                    value = "#" + friendlyValue.Trim() + "#";
                    break;
                case "System.String":

                    if (operand.Equals(operandTypes[SharedStrings.FRIENDLY_OPERATOR_IS_ANY_OF]) ||
                        operand.Equals(operandTypes[SharedStrings.FRIENDLY_OPERATOR_IS_NOT_ANY_OF]))
                    {
                        string[] values = friendlyValue.Split(',');
                        WordBuilder wb = new WordBuilder(",");

                        foreach (string str in values)
                        {
                            wb.Add("'" + str.Trim().Replace("'", "''") + "'");
                        }

                        value = "(" + wb.ToString() + ")";
                    }
                    else
                    {
                        value = "'" + friendlyValue.Trim().Replace("'", "''") + "'";
                        if (value.Equals("''") && operand.Equals(operandTypes[SharedStrings.FRIENDLY_OPERATOR_MISSING]))
                        {
                            operand = operandTypes[SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO];
                        }
                        else if (value.Equals("''") && operand.Equals(operandTypes[SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING]))
                        {
                            operand = operandTypes[SharedStrings.FRIENDLY_OPERATOR_NOT_EQUAL_TO];
                        }
                    }
                    break;
                case "System.Boolean":
                    if (friendlyValue.Equals(config.Settings.RepresentationOfYes))
                    {
                        value = "1";
                    }
                    else
                    {
                        value = "0";
                    }
                    break;
                case "System.Int16":
                case "System.Byte":
                    if (field != null && field is Epi.Fields.YesNoField)
                    {
                        if (friendlyValue.Equals(config.Settings.RepresentationOfYes))
                        {
                            value = "1";
                        }
                        else if (friendlyValue.Equals(config.Settings.RepresentationOfNo))
                        {
                            value = "0";
                        }
                        else if (friendlyValue.Equals(config.Settings.RepresentationOfMissing))
                        {
                            value = string.Empty;
                        }
                    }
                    else
                    {
                        value = friendlyValue.Trim();
                    }
                    break;
                default:
                    value = friendlyValue.Trim();
                    break;
            }

            if (friendlyValue.Equals(config.Settings.RepresentationOfMissing))
            {
                value = string.Empty;
                friendlyValue = string.Empty;
                if (friendlyOperand.Equals(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING) || friendlyOperand.Equals(SharedStrings.FRIENDLY_OPERATOR_NOT_EQUAL_TO))
                {
                    operand = operandTypes[SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING]; // "is not null";
                }
                else if (friendlyOperand.Equals(SharedStrings.FRIENDLY_OPERATOR_MISSING))
                {
                    operand = operandTypes[SharedStrings.FRIENDLY_OPERATOR_MISSING]; // "is null";
                    value = string.Empty;
                }
                else
                {
                    operand = operandTypes[SharedStrings.FRIENDLY_OPERATOR_MISSING]; // "is null";
                }
            }

            if (operand.Equals("like"))
            {
                value = value.Trim().Replace('*', '%');
            }

            if (operand.Equals(operandTypes[SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING]) || operand.Equals(operandTypes[SharedStrings.FRIENDLY_OPERATOR_MISSING]))
            {
                value = string.Empty;
            }

            if (columnType.Equals("System.DateTime") && !string.IsNullOrEmpty(value))
            {
                DateTime dt = DateTime.Parse(friendlyValue, System.Globalization.CultureInfo.InvariantCulture);
                friendlyValue = dt.ToString("d", System.Globalization.CultureInfo.CurrentCulture);
            }

            friendlyCondition = string.Format(SharedStrings.FRIENDLY_CONDITION_DATA_FILTER, columnName, friendlyOperand, friendlyValue);
            FilterCondition newCondition = new FilterCondition(friendlyCondition, columnName, rawColumnName, columnType, operand, friendlyOperand, value, friendlyValue);

            return newCondition;
        }

        /// <summary>
        /// Generates a FilterCondition object using primitive type inputs
        /// </summary>
        /// <param name="columnName">The name of the column on which to filter</param>
        /// <param name="rawColumnName">The name of the column on which to filter, without brackets</param>
        /// <param name="columnType">The data type of the column on which to filter</param>
        /// <param name="friendlyOperand">The friendly operand</param>
        /// <param name="friendlyLowValue">The friendly lower value</param>
        /// <param name="friendlyHighValue">The friendly upper value</param>
        /// <returns>FilterCondition</returns>
        public FilterCondition GenerateFilterCondition(string columnName, string rawColumnName, string columnType, string friendlyOperand, string friendlyLowValue, string friendlyHighValue)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ApplicationException("Column name cannot be empty.");
            }
            else if (string.IsNullOrEmpty(friendlyOperand))
            {
                throw new ApplicationException("Friendly operand cannot be empty.");
            }
            else if (string.IsNullOrEmpty(columnType))
            {
                throw new ApplicationException("Column type cannot be empty.");
            }
            else if (!operandTypes.ContainsKey(friendlyOperand))
            {
                throw new ApplicationException("Operand not found in dictionary.");
            }
            #endregion // Input Validation

            Configuration config = dashboardHelper.Config;

            string operand = operandTypes[SharedStrings.FRIENDLY_OPERATOR_AND];

            string lowValue = string.Empty;
            string highValue = string.Empty;

            string friendlyCondition = string.Empty;

            switch (columnType)
            {
                case "System.DateTime":
                    lowValue = "#" + friendlyLowValue.Trim() + "#";
                    highValue = "#" + friendlyHighValue.Trim() + " 23:59:59#";
                    break;
                default:
                    lowValue = friendlyLowValue.Trim();
                    highValue = friendlyHighValue.Trim();
                    break;
            }

            if (columnType.Equals("System.DateTime"))
            {
                DateTime lowDt = DateTime.Parse(friendlyLowValue, System.Globalization.CultureInfo.InvariantCulture);
                friendlyLowValue = lowDt.ToString("d", System.Globalization.CultureInfo.CurrentCulture);

                DateTime highDt = DateTime.Parse(friendlyHighValue, System.Globalization.CultureInfo.InvariantCulture);
                friendlyHighValue = highDt.ToString("d", System.Globalization.CultureInfo.CurrentCulture);
            }
            
            friendlyCondition = string.Format(SharedStrings.FRIENDLY_CONDITION_DATA_FILTER_BETWEEN, columnName, friendlyLowValue, friendlyHighValue);
            FilterCondition newCondition = new FilterCondition(friendlyCondition, columnName, rawColumnName, columnType, operand, friendlyOperand, lowValue, highValue, friendlyLowValue, friendlyHighValue);

            return newCondition;
        }

        /// <summary>
        /// Gets a list of strings representing all of the filter conditions making up the data filter
        /// </summary>
        /// <returns>List of strings; represents all of the filter conditions making up the data filter</returns>
        public List<string> GetFilterConditionsAsList()
        {
            List<string> conditions = new List<string>();

            DataRow[] rows = GetSortedDataRows();

            foreach (DataRow row in rows)
            {
                FilterCondition condition = (FilterCondition)row[COLUMN_FILTER];
                string join = (string)row[COLUMN_JOIN];
                string friendlyCondition = condition.FriendlyCondition; //join + StringLiterals.SPACE + condition.FriendlyValue;

                if (string.IsNullOrEmpty(join))
                {
                    friendlyCondition = condition.FriendlyValue;
                }

                conditions.Add(friendlyCondition);
            }

            return conditions;
        }

        /// <summary>
        /// Gets a data table of filter conditions that together compose the data filter
        /// </summary>
        /// <returns>DataTable of filter conditions; represents all of the filter conditions that together compose the data filter</returns>
        public DataTable GetFilterConditionsAsTable()
        {
            DataTable table = new DataTable();

            table = new DataTable("FilterTable");
            table.Columns.Add(COLUMN_ACTIVE, typeof(bool));
            table.Columns.Add(COLUMN_JOIN, typeof(string));
            table.Columns.Add(COLUMN_FILTER, typeof(string));

            DataRow[] rows = GetSortedDataRows();

            foreach (DataRow row in rows)
            {                
                int id = (int)row[COLUMN_ID];
                string join = (string)row[COLUMN_JOIN];
                FilterCondition condition = (FilterCondition)row[COLUMN_FILTER];
                bool active = condition.IsEnabled;
                int group = (int)row[COLUMN_GROUP];                

                table.Rows.Add(active, /*id,*/ join, condition.ToString()/*, group*/);
            }

            return table;
        }

        /// <summary>
        /// Generates a readable data filter string; for display purposes only
        /// </summary>
        /// <returns>String representing the current data filter written out as an English sentence</returns>
        public string GenerateReadableDataFilterString()
        {
            string str = string.Empty;

            DataTable table = this.GetFilterConditionsAsTable();

            foreach (DataRow row in table.Rows)
            {
                string condition = row[2].ToString();
                string join = row[1].ToString();
                if (!string.IsNullOrEmpty(join))
                {
                    join = join + " ";
                    if (condition.StartsWith("T"))
                    {
                        condition = condition.Remove(0, 1);
                        condition = "t" + condition;
                    }
                }

                str = str + join + condition;
                str = str + " ";
            }

            str = str.Trim();

            return str;
        }

        /// <summary>
        /// Generates a usable data filter string for use in .NET DataTable SELECT queries
        /// </summary>
        /// <returns>String representing the current data filter</returns>
        public string GenerateDataFilterString(bool addRecStatus = true)
        {
            StringBuilder filterCriteria = new StringBuilder(string.Empty);

            DataRow[] rows = GetSortedDataRows();

            bool addedFirstCondition = false;

            for(int i = 0; i < rows.Length; i++)
            {
                DataRow row = rows[i];
                FilterCondition condition = (FilterCondition)row[COLUMN_FILTER];                

                if (condition.IsEnabled)
                {
                    if (!addedFirstCondition)
                    {
                        filterCriteria.Append(condition.Condition);
                        addedFirstCondition = true;
                    }
                    else
                    {
                        string join = row[COLUMN_JOIN].ToString();

                        if (string.IsNullOrEmpty(join))
                        {
                            // should never be the case, but if so, throw an exception so we can backtrack and find out why it's happening
                            // otherwise we'll run into a syntax error when the filter is applied
                            throw new ApplicationException("The value for 'join' should not be null for this row.");
                        }

                        filterCriteria.Append(join);
                        filterCriteria.Append(StringLiterals.SPACE);
                        filterCriteria.Append(condition.Condition);
                    }

                    filterCriteria.Append(StringLiterals.SPACE);
                }
            }

            if (dashboardHelper.IsUsingEpiProject && addRecStatus)
            {
                string criteriaWithRecStatus = string.Empty;

                if (filterCriteria.Length > 0)
                {
                    criteriaWithRecStatus = "(" + filterCriteria.ToString() + ") AND ";
                }

                if (dashboardHelper.RecordProcessScope == RecordProcessingScope.Undeleted)
                {                    
                    criteriaWithRecStatus = criteriaWithRecStatus + " " + ColumnNames.REC_STATUS + " > 0";
                }
                else if (dashboardHelper.RecordProcessScope == RecordProcessingScope.Deleted)
                {
                    criteriaWithRecStatus = criteriaWithRecStatus + " " + ColumnNames.REC_STATUS + " = 0";
                }

                filterCriteria = new StringBuilder(criteriaWithRecStatus);
            }

            return filterCriteria.ToString();
        }

        /// <summary>
        /// Generates a data filter query of only certain filter conditions. This method should only be used in specific circumstances.
        /// </summary>
        /// <returns>Query</returns>
        internal Query GenerateDataFilterQuery(string tableName, string columnNames = "")
        {
            Configuration config = dashboardHelper.Config;
            List<QueryParameter> queryParameters = new List<QueryParameter>();

            StringBuilder filterCriteria = new StringBuilder(string.Empty);
            if (string.IsNullOrEmpty(columnNames) || columnNames.Equals("*"))
            {
                filterCriteria.Append("select * from " + tableName + " where ");
            }
            else
            {
                filterCriteria.Append("select " + columnNames + " from " + tableName + " where ");
            }

            DataRow[] rows = GetSortedDataRows();

            bool addedFirstCondition = false;
            bool tryFilter = true;

            for (int i = 0; i < rows.Length; i++)
            {
                DataRow row = rows[i];
                if (row[COLUMN_JOIN].ToString().ToLower().Contains("or"))
                {
                    tryFilter = false;
                    break;
                }
            }

            if (tryFilter)
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    DataRow row = rows[i];
                    FilterCondition condition = (FilterCondition)row[COLUMN_FILTER];

                    bool useCondition = true;
                    string trimmedColumnName = condition.RawColumnName;

                    if (DashboardHelper.GetColumnType(trimmedColumnName).StartsWith("System.Date") || condition.Operand.Contains("like"))
                    {
                        useCondition = false;
                        continue;
                    }

                    for (int j = 0; j < trimmedColumnName.Length; j++)
                    {
                        string viewChar = trimmedColumnName.Substring(j, 1);
                        System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(viewChar, "[A-Za-z0-9_]");
                        if (!m.Success)
                        {
                            useCondition = false;
                        }
                    }

                    if (useCondition && !condition.IsBetween && condition.IsEnabled && !string.IsNullOrEmpty(condition.Operand) && !condition.Operand.Contains("null"))
                    {
                        if (!addedFirstCondition)
                        {
                            filterCriteria.Append(" " +
                                StringLiterals.LEFT_SQUARE_BRACKET +
                                trimmedColumnName +
                                StringLiterals.RIGHT_SQUARE_BRACKET +
                                StringLiterals.SPACE +
                                condition.Operand +
                                StringLiterals.SPACE +
                                "@" + trimmedColumnName);
                            addedFirstCondition = true;
                        }
                        else
                        {
                            string join = row[COLUMN_JOIN].ToString();

                            if (string.IsNullOrEmpty(join))
                            {
                                // should never be the case, but if so, throw an exception so we can backtrack and find out why it's happening
                                // otherwise we'll run into a syntax error when the filter is applied
                                throw new ApplicationException("The value for 'join' should not be null for this row.");
                            }

                            filterCriteria.Append(join);
                            filterCriteria.Append(StringLiterals.SPACE);
                            filterCriteria.Append(" " +
                                StringLiterals.LEFT_SQUARE_BRACKET +
                                trimmedColumnName +
                                StringLiterals.RIGHT_SQUARE_BRACKET +
                                StringLiterals.SPACE +
                                condition.Operand +
                                StringLiterals.SPACE +
                                "@" + trimmedColumnName);
                        }

                        if (dashboardHelper.IsColumnBoolean(trimmedColumnName))
                        {
                            bool? value = null;

                            if (!string.IsNullOrEmpty(condition.Value))
                            {
                                if (condition.Value.Equals("1"))
                                {
                                    value = true;
                                }
                                else
                                {
                                    value = false;
                                }
                            }

                            queryParameters.Add(new QueryParameter("@" + trimmedColumnName, DashboardHelper.GetColumnDbType(trimmedColumnName), value));
                        }
                        else
                        {
                            queryParameters.Add(new QueryParameter("@" + trimmedColumnName, DashboardHelper.GetColumnDbType(trimmedColumnName), condition.FriendlyValue));
                        }
                        filterCriteria.Append(StringLiterals.SPACE);
                    }
                }
            }

            Query selectQuery = DashboardHelper.Database.CreateQuery(filterCriteria.ToString());
            foreach (QueryParameter param in queryParameters)
            {
                selectQuery.Parameters.Add(param);
            }

            if (selectQuery.SqlStatement.EndsWith("where "))
            {
                return DashboardHelper.Database.CreateQuery("select * from " + tableName);
            }

            return selectQuery;
        }
        #endregion // Public Methods

        #region Private Methods
        /// <summary>
        /// Converts a joinType enumeration to its corresponding string representation
        /// </summary>
        /// <param name="joinType">The enumerable join type</param>
        /// <returns>String representation of the joinType method parameter</returns>
        private string ConvertJoinType(ConditionJoinType joinType) 
        {
            switch (joinType)
            {
                case ConditionJoinType.And:
                    return "and";
                case ConditionJoinType.Or:
                    return "or";
                default:
                    return "and";
            }
        }

        /// <summary>
        /// Gets an array of data rows from the ConditionTable, sorted by the column ID
        /// </summary>
        /// <returns>Array of data rows</returns>
        private DataRow[] GetSortedDataRows()
        {
            return ConditionTable.Select(string.Empty, COLUMN_ID);
        }
        #endregion // Private Methods

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<FilterCondition> GetEnumerator()
        {
            return new DataFiltersEnumerator(conditionTable);
        }

        public void Dispose() { }

        #endregion

        /// <summary>
        /// Enumerator class for the data filter class
        /// </summary>
        protected class DataFiltersEnumerator : IEnumerator<FilterCondition>
        {
            #region Private Members
            private DataTable conditionTable;
            private int currentIndex;
            #endregion Private Members

            public DataFiltersEnumerator(DataTable table)
			{
                conditionTable = table;
				Reset();
			}


            public void Reset()
            {
                currentIndex = -1;
            }

            public FilterCondition Current
            {
                get
                {
                    return (FilterCondition)conditionTable.Rows[currentIndex][COLUMN_FILTER];                    
                }
            }

            object IEnumerator.Current
            {
                get 
                { 
                    return Current; 
                }
            }

            public bool MoveNext()
            {
                currentIndex++;
				return currentIndex < conditionTable.Rows.Count;
            }

            public void Dispose() { }
        }
    }
}
