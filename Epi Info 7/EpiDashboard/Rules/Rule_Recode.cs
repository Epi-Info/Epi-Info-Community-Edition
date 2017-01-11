using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Epi;
using Epi.Core;

namespace EpiDashboard.Rules
{
    /// <summary>
    /// Represents the allowable types of variables that may store recoded data through the dashboard
    /// </summary>
    public enum DashboardVariableType
    {
        None = 0,
        YesNo = 1,
        Numeric = 2,
        Text = 3,
        Date = 4, 
        Group = 5
    }

    /// <summary>
    /// A class designed to recode data from a form field to a new, temporary user-defined field based on a rule defined by the user
    /// </summary>
    public class Rule_Recode : DataAssignmentRule
    {
        #region Private Members
        private string sourceColumnName;
        private string sourceColumnType;
        private DataTable recodeInputTable;
        private DataTable recodeTable;
        private bool shouldMaintainSortOrder;
        private bool shouldUseWildcards;
        private string configYesValue = "Yes";
        private string configNoValue = "No";
        private string configMissingValue = "Missing";
        private string elseValue;
        #endregion // Private Members

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public Rule_Recode(DashboardHelper dashboardHelper)
        {
            this.dashboardHelper = dashboardHelper;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper to attach</param>
        /// <param name="friendlyRule">The human-readable version of the rule</param>
        /// <param name="sourceColumnName">The name of the column from which to read the values</param>
        /// <param name="destinationColumnName">The name of the column in which the recoded values will reside</param>
        /// <param name="variableType">The type of variable being created to hold the recoded values</param>
        /// <param name="recodeInputTable">The table containing the source and destination values that describe how the recode should take place</param>
        /// <param name="maintainSortOrder">Whether statistics run on the recoded variable should maintain the sort order specified in the recode dialog, if appropriate for that statistic command</param>
        public Rule_Recode(DashboardHelper dashboardHelper, string friendlyRule, string sourceColumnName, string sourceColumnType, string destinationColumnName, DashboardVariableType variableType, DataTable recodeInputTable, string elseValue, bool maintainSortOrder, bool shouldUseWildcards)
        {
            this.dashboardHelper = dashboardHelper;
            this.friendlyRule = friendlyRule;
            this.sourceColumnName = sourceColumnName;
            this.sourceColumnType = sourceColumnType;
            this.destinationColumnName = destinationColumnName;
            this.variableType = variableType;
            this.destinationColumnType = GetDestinationColumnType(variableType);
            this.recodeInputTable = recodeInputTable;
            this.shouldMaintainSortOrder = maintainSortOrder;
            this.shouldUseWildcards = shouldUseWildcards;

            if (this.variableType.Equals(DashboardVariableType.YesNo))
            {
                string val = elseValue.ToLowerInvariant();

                if(val == "true" || val == "(+)" || val == dashboardHelper.Config.Settings.RepresentationOfYes.ToLowerInvariant())
                {
                    this.elseValue = "true";
                }
                else if (val == "false" || val == "(-)" || val == dashboardHelper.Config.Settings.RepresentationOfNo.ToLowerInvariant())
                {
                    this.elseValue = "false";
                }
                else
                {
                    this.elseValue = string.Empty;
                }
            }
            else
            {
                this.elseValue = elseValue;
            }
            Construct();
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets the name of the source column from which to read values
        /// </summary>
        public string SourceColumnName
        {
            get
            {
                return sourceColumnName;
            }
        }

        /// <summary>
        /// Gets the type of the source column from which to read values
        /// </summary>
        public string SourceColumnType
        {
            get
            {
                return sourceColumnType;
            }
        }

        /// <summary>
        /// Gets the recode table for this rule
        /// </summary>
        public DataTable RecodeTable
        {
            get
            {
                return this.recodeTable;
            }
        }

        /// <summary>
        /// Gets the original recode table for this rule, as created in the recode dialog. Used for editing the rule.
        /// </summary>
        public DataTable RecodeInputTable
        {
            get
            {
                return this.recodeInputTable;
            }
        }

        /// <summary>
        /// Gets whether or not to maintain the sort order of this rule
        /// </summary>
        public bool ShouldMaintainSortOrder
        {
            get
            {
                return this.shouldMaintainSortOrder;
            }
        }

        /// <summary>
        /// Gets whether or not to use wildcards in the 'from' column
        /// </summary>
        public bool ShouldUseWildcards
        {
            get
            {
                return this.shouldUseWildcards;
            }
        }

        /// <summary>
        /// Gets the 'else' value
        /// </summary>
        public string ElseValue
        {
            get
            {
                return this.elseValue;
            }
        }
        #endregion // Public Properties

        #region Public Methods
        /// <summary>
        /// Gets a list of field names that this rule cannot be run without
        /// </summary>
        /// <returns>List of strings</returns>
        public override List<string> GetDependencies()
        {
            List<string> dependencies = new List<string>();

            dependencies.Add(DestinationColumnName);
            dependencies.Add(SourceColumnName);

            return dependencies;
        }

        /// <summary>
        /// Converts the value of the current EpiDashboard.Rule_Format object to its equivalent string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.FriendlyRule;
        }

        /// <summary>
        /// Gets all the 'To' values in the rule as a list
        /// </summary>
        /// <returns></returns>
        public List<string> GetToValues()
        {
            List<string> toValues = new List<string>();
            int column = 1;
            if (this.RecodeTable.Columns.Count == 3)
            {
                column = 2;
            }
            foreach (DataRow row in this.RecodeTable.Rows)
            {
                if(!toValues.Contains(row[column].ToString())) 
                {
                    toValues.Add(row[column].ToString());
                }
            }
            return toValues;
        }
        #endregion // Public Methods

        #region Private Methods
        /// <summary>
        /// Constructs the recode rule
        /// </summary>
        private void Construct()
        {
            if (SourceColumnType == null)
            {
                // To give a friendlier error message to users when they try to load canvas files made with older versions of Epi Info 7
                throw new NullReferenceException("The SourceColumnType property is null for the recoded variable '" + this.DestinationColumnName + "'. If loading this canvas from a canvas file, please make sure the <sourceColumnType> tag is present and has valid data.");
            }

            this.recodeTable = new DataTable("recode");
            DataColumn col1 = new DataColumn("From (lower)", typeof(string));
            DataColumn col2 = new DataColumn("From (upper)", typeof(string));
            DataColumn col3 = new DataColumn("To", typeof(string));

            this.recodeTable.Columns.Add(col1);
            this.recodeTable.Columns.Add(col2);
            this.recodeTable.Columns.Add(col3);

            if (recodeInputTable.Columns.Count == 2)
            {
                foreach (DataRow row in recodeInputTable.Rows)
                {
                    string fromValue = row[0].ToString();
                    string toValue = row[1].ToString();

                    if (!(string.IsNullOrEmpty(fromValue) && string.IsNullOrEmpty(toValue)))
                    {
                        this.recodeTable.Rows.Add(fromValue, toValue);
                    }
                }
            }
            else if (recodeInputTable.Columns.Count == 3)
            {
                if (SourceColumnType.Equals("System.DateTime"))
                {
                    // 3 columns, we know it's date
                    foreach (DataRow row in recodeInputTable.Rows)
                    {
                        DateTime lowerBound;
                        DateTime upperBound;
                        bool firstSuccess = false;
                        bool secondSuccess = false;

                        if (row[0].ToString().Equals("LOVALUE"))
                        {
                            lowerBound = DateTime.MinValue;
                            firstSuccess = true;
                        }
                        else
                        {
                            firstSuccess = DateTime.TryParse(row[0].ToString(), out lowerBound);
                        }

                        if (row[1].ToString().Equals("HIVALUE"))
                        {
                            upperBound = DateTime.MaxValue;
                            secondSuccess = true;
                        }

                        else
                        {
                            secondSuccess = DateTime.TryParse(row[1].ToString(), out upperBound);
                        }

                        string lower = string.Empty;
                        string upper = string.Empty;

                        if (lowerBound != null)
                        {
                            lower = lowerBound.ToShortDateString();
                        }
                        if (upperBound != null)
                        {
                            upper = upperBound.ToShortDateString();
                        }

                        if (!firstSuccess)
                        {
                            lower = string.Empty;
                        }
                        if (!secondSuccess)
                        {
                            upper = string.Empty;
                        }

                        this.recodeTable.Rows.Add(lower, upper, row[2].ToString());
                    }
                }
                else
                {
                    // 3 columns, we know it's numeric
                    foreach (DataRow row in recodeInputTable.Rows)
                    {
                        double lowerBound = 0;
                        double upperBound = 0;
                        bool firstSuccess = false;
                        bool secondSuccess = false;

                        if (row[0].ToString().Equals("LOVALUE"))
                        {
                            switch (SourceColumnType)
                            {                                
                                case "System.Single":
                                    lowerBound = Single.MinValue;
                                    break;
                                case "System.Int32":
                                    lowerBound = Int32.MinValue;
                                    break;
                                case "System.Int16":
                                    lowerBound = Int32.MinValue;
                                    break;
                                case "System.Byte":
                                    lowerBound = Byte.MinValue;
                                    break;
                                case "System.SByte":
                                    lowerBound = SByte.MinValue;
                                    break;
                                case "System.Double":
                                default:
                                    lowerBound = Double.MinValue;
                                    break;
                            }
                            
                            firstSuccess = true;
                        }
                        else
                        {
                            firstSuccess = Double.TryParse(row[0].ToString(), out lowerBound);
                        }

                        if (row[1].ToString().Equals("HIVALUE"))
                        {
                            //upperBound = Double.MaxValue;

                            switch (SourceColumnType)
                            {
                                case "System.Single":
                                    upperBound = Single.MaxValue;
                                    break;
                                case "System.Int32":
                                    upperBound = Int32.MaxValue;
                                    break;
                                case "System.Int16":
                                    upperBound = Int32.MaxValue;
                                    break;
                                case "System.Byte":
                                    upperBound = Byte.MaxValue;
                                    break;
                                case "System.SByte":
                                    upperBound = SByte.MaxValue;
                                    break;
                                case "System.Double":
                                default:
                                    upperBound = Double.MaxValue;
                                    break;
                            }

                            secondSuccess = true;
                        }
                        
                        else
                        {
                            secondSuccess = Double.TryParse(row[1].ToString(), out upperBound);
                        }

                        string lower = lowerBound.ToString("R");
                        string upper = upperBound.ToString("R");

                        if (!firstSuccess)
                        {
                            lower = string.Empty;
                        }
                        if (!secondSuccess)
                        {
                            upper = string.Empty;
                        }

                        this.recodeTable.Rows.Add(lower, upper, row[2].ToString());
                    }
                }
            }

            Configuration config = dashboardHelper.Config;
            configYesValue = config.Settings.RepresentationOfYes;
            configNoValue = config.Settings.RepresentationOfNo;
            configMissingValue = config.Settings.RepresentationOfMissing;
        }

        /// <summary>
        /// Gets a column type appropriate for a .NET data table based off of the dashboard variable type selected by the user
        /// </summary>
        /// <param name="dashboardVariableType">The type of variable that is storing the recoded values</param>
        /// <returns>A string representing the type of a .NET DataColumn</returns>
        private string GetDestinationColumnType(DashboardVariableType dashboardVariableType)
        {
            switch (dashboardVariableType)
            {
                case DashboardVariableType.Numeric:
                    return "System.Decimal";
                case DashboardVariableType.Date:
                    return "System.DateTime";
                case DashboardVariableType.Text:
                    return "System.String";
                case DashboardVariableType.YesNo:
                    return "System.Boolean";
                case DashboardVariableType.None:
                    throw new ApplicationException(SharedStrings.DASHBOARD_ERROR_INVALID_COLUMN_TYPE);
                default:
                    return "System.String";
            }
        }

        /// <summary>
        /// Converts a wildcard string to a regular expression
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private string WildcardToRegex(string pattern)
        {
            //return "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            string s = "^" + Regex.Escape(pattern) + "$";
            s = Regex.Replace(s, @"(?<!\\)\\\*", @".*"); // Negative Lookbehind
            s = Regex.Replace(s, @"\\\\\\\*", @"\*");
            s = Regex.Replace(s, @"(?<!\\)\\\?", @".");  // Negative Lookbehind
            s = Regex.Replace(s, @"\\\\\\\?", @"\?");
            return Regex.Replace(s, @"\\\\\\\\", @"\\"); 
        }
        #endregion // Private Methods

        #region IDashboardRule Members
        /// <summary>
        /// Generates an Xml element for this rule
        /// </summary>
        /// <param name="doc">The parent Xml document</param>
        /// <returns>XmlNode representing this rule</returns>
        public override System.Xml.XmlNode Serialize(System.Xml.XmlDocument doc)
        {
            string xmlString =
            "<friendlyRule>" + friendlyRule + "</friendlyRule>" +
            "<sourceColumnName>" + sourceColumnName + "</sourceColumnName>" +
            "<sourceColumnType>" + sourceColumnType + "</sourceColumnType>" +
            "<destinationColumnName>" + destinationColumnName + "</destinationColumnName>" +
            "<destinationColumnType>" + destinationColumnType + "</destinationColumnType>" +
            "<tableColumns>" + recodeInputTable.Columns.Count.ToString() + "</tableColumns>" +
            "<elseValue>" + ElseValue + "</elseValue>" +
            "<shouldUseWildcards>" + shouldUseWildcards + "</shouldUseWildcards>" +
            "<shouldMaintainSortOrder>" + shouldMaintainSortOrder + "</shouldMaintainSortOrder>";

            xmlString = xmlString + "<recodeTable>";

            foreach (DataRow row in recodeInputTable.Rows)
            {
                xmlString = xmlString + "<recodeTableRow>";
                object[] objects = row.ItemArray;
                foreach (object obj in objects)
                {
                    xmlString = xmlString + "<recodeTableData>";
                    xmlString = xmlString + obj.ToString().Replace("<", "&lt;").Replace(">", "&gt;").Replace("&", "&amp;");
                    xmlString = xmlString + "</recodeTableData>";
                }
                xmlString = xmlString + "</recodeTableRow>";
            }

            xmlString = xmlString + "</recodeTable>";

            System.Xml.XmlElement element = doc.CreateElement("rule");
            element.InnerXml = xmlString;

            System.Xml.XmlAttribute type = doc.CreateAttribute("ruleType");

            type.Value = "EpiDashboard.Rules.Rule_Recode";

            element.Attributes.Append(type);

            return element;
        }

        /// <summary>
        /// Creates the rule from an Xml element
        /// </summary>
        /// <param name="element">The XmlElement from which to create the rule</param>
        public override void CreateFromXml(System.Xml.XmlElement element)
        {
            this.recodeInputTable = new DataTable();
            int columns = 3;

            foreach (XmlElement child in element.ChildNodes)
            {
                if (child.Name.Equals("friendlyRule"))
                {
                    this.friendlyRule = child.InnerText;
                }
                else if (child.Name.Equals("sourceColumnName"))
                {
                    this.sourceColumnName = child.InnerText;
                }
                else if (child.Name.Equals("sourceColumnType"))
                {
                    this.sourceColumnType = child.InnerText;
                }
                else if (child.Name.Equals("destinationColumnName"))
                {
                    this.destinationColumnName = child.InnerText;
                }
                else if (child.Name.Equals("destinationColumnType"))
                {
                    this.destinationColumnType = child.InnerText;
                }
                else if (child.Name.Equals("elseValue"))
                {
                    this.elseValue = child.InnerText;
                }
                if (child.Name.Equals("tableColumns"))
                {
                    columns = int.Parse(child.InnerText);
                    for (int i = 0; i < columns; i++)
                    {
                        DataColumn column = new DataColumn(i.ToString(), typeof(string));
                        recodeInputTable.Columns.Add(column);
                    }
                }
                else if (child.Name.Equals("shouldMaintainSortOrder"))
                {
                    this.shouldMaintainSortOrder = bool.Parse(child.InnerText);
                }
                else if (child.Name.Equals("shouldUseWildcards"))
                {
                    this.shouldUseWildcards = bool.Parse(child.InnerText);
                }
                else if (child.Name.Equals("recodeTable"))
                {
                    foreach (XmlElement recodeRow in child.ChildNodes)
                    {
                        if (recodeRow.Name.ToLowerInvariant().Equals("recodetablerow"))
                        {
                            string[] itemArray = new string[columns];
                            int count = 0;
                            foreach (XmlElement recodeCell in recodeRow.ChildNodes)
                            {
                                if (recodeCell.Name.ToLowerInvariant().Equals("recodetabledata"))
                                {
                                    itemArray[count] = recodeCell.InnerText.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");
                                    count++;
                                }
                            }
                            RecodeInputTable.Rows.Add(itemArray);
                        }
                    }
                }
            }

            if (destinationColumnType.Equals("System.String"))
            {
                this.variableType = DashboardVariableType.Text;
            }
            else if (destinationColumnType.Equals("System.Single") || destinationColumnType.Equals("System.Double") || destinationColumnType.Equals("System.Decimal") || destinationColumnType.Equals("System.Int16") || destinationColumnType.Equals("System.Int32"))
            {
                this.variableType = DashboardVariableType.Numeric;
            }

            Construct();
        }

        /// <summary>
        /// Sets up the rule
        /// </summary>
        /// <param name="table">The table in which to apply the rule</param>
        public override void SetupRule(DataTable table)
        {
            string destinationColumnType = this.DestinationColumnType;
            sourceColumnType = DashboardHelper.GetColumnType(this.SourceColumnName);

            DataColumn dc;

            switch (destinationColumnType)
            {
                case "System.Byte":
                case "System.SByte":
                    dc = new DataColumn(this.DestinationColumnName, typeof(byte));
                    break;
                case "System.Boolean":
                    dc = new DataColumn(this.DestinationColumnName, typeof(bool));
                    dc.AllowDBNull = true;
                    break;
                case "System.Single":
                case "System.Double":
                    dc = new DataColumn(this.DestinationColumnName, typeof(double));
                    break;
                case "System.Decimal":
                    dc = new DataColumn(this.DestinationColumnName, typeof(decimal));
                    break;
                case "System.String":
                default:
                    dc = new DataColumn(this.DestinationColumnName, typeof(string));
                    break;
            }

            if (!table.Columns.Contains(dc.ColumnName))
            {
                table.Columns.Add(dc);
            }
            else
            {
                foreach (DataRow row in table.Rows)
                {
                    row[dc.ColumnName] = DBNull.Value;
                }
            }

            //table.Columns.Add(dc);
        }

        /// <summary>
        /// Applies the rule
        /// </summary>
        /// <param name="row">The row on which to apply the rule</param>
        public override void ApplyRule(DataRow row)
        {
            if (!row.Table.Columns.Contains(this.DestinationColumnName) || !row.Table.Columns.Contains(this.SourceColumnName))
            {
                return;
            }

            //string sourceColumnType = DashboardHelper.GetColumnType(this.SourceColumnName);

            if (!string.IsNullOrEmpty(ElseValue))
            {
                row[this.DestinationColumnName] = ElseValue;
            }

            if (sourceColumnType.Equals("System.Boolean"))
            {
            }
            else if (sourceColumnType.Equals("System.Single") || sourceColumnType.Equals("System.Double") || sourceColumnType.Equals("System.Decimal"))
            {
                string recodedValue = string.Empty;
                string value = row[this.SourceColumnName].ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    double numericValue;
                    bool outerSuccess = Double.TryParse(value, out numericValue);

                    try
                    {
                        if (outerSuccess)
                        {
                            foreach (DataRow recodeRow in this.RecodeTable.Rows)
                            {
                                if (string.IsNullOrEmpty(recodeRow[0].ToString()) && string.IsNullOrEmpty(recodeRow[1].ToString()))
                                {
                                    continue;
                                }

                                double lowerBound;
                                double upperBound;

                                if (recodeRow[0].ToString().Equals("LOVALUE"))
                                {
                                    lowerBound = Double.MinValue;
                                }
                                if (recodeRow[1].ToString().Equals("HIVALUE"))
                                {
                                    upperBound = Double.MaxValue;
                                }

                                bool innerSuccess = false;
                                innerSuccess = double.TryParse(recodeRow[0].ToString(), out lowerBound);

                                if (!innerSuccess)
                                {
                                    continue;
                                }

                                innerSuccess = double.TryParse(recodeRow[1].ToString(), out upperBound);

                                if (!innerSuccess && numericValue != lowerBound)
                                {
                                    continue;
                                }
                                else if (numericValue == lowerBound)
                                {
                                    recodedValue = recodeRow[2].ToString();
                                }
                                else if (numericValue >= lowerBound && numericValue < upperBound)
                                {
                                    recodedValue = recodeRow[2].ToString();
                                }

                                if (destinationColumnType.Equals("System.Boolean") || (dashboardHelper.IsUsingEpiProject && dashboardHelper.View.Fields.Contains(destinationColumnName) && dashboardHelper.View.Fields[destinationColumnName] is Epi.Fields.YesNoField))
                                {
                                    if (recodedValue.Equals(configYesValue))
                                    {
                                        if (string.IsNullOrEmpty(ElseValue))
                                        {
                                            row[this.DestinationColumnName] = true;
                                        }
                                    }
                                    else if (recodedValue.Equals(configNoValue))
                                    {
                                        row[this.DestinationColumnName] = false;
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(recodedValue))
                                    {
                                        if (string.IsNullOrEmpty(ElseValue))
                                        {
                                            row[this.DestinationColumnName] = DBNull.Value;
                                        }
                                    }
                                    else
                                    {
                                        row[this.DestinationColumnName] = recodedValue;
                                    }
                                }
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        //this.IsDefective = true;
                        throw new DefectiveRuleException(DefectiveRuleException.RuleIssue.RecodedValueFormat, "The recoded values in " + this.DestinationColumnName + " are text, but the rule has been created as a number field. The number field cannot accept text data.");
                    }                
                }
            }
            else if (sourceColumnType.Equals("System.Int32") || sourceColumnType.Equals("System.Int16") || sourceColumnType.Equals("System.Byte") || sourceColumnType.Equals("System.SByte"))
            {
                string recodedValue = string.Empty;
                string value = row[this.SourceColumnName].ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    int numericValue;
                    bool outerSuccess = Int32.TryParse(value, out numericValue);

                    if (outerSuccess)
                    {
                        foreach (DataRow recodeRow in this.RecodeTable.Rows)
                        {
                            if (string.IsNullOrEmpty(recodeRow[0].ToString()) && string.IsNullOrEmpty(recodeRow[1].ToString()))
                            {
                                continue;
                            }

                            int lowerBound;
                            int upperBound;
                            
                            bool innerSuccess = false;
                            
                            innerSuccess = Int32.TryParse(recodeRow[0].ToString(), out lowerBound);                            

                            if (!innerSuccess)
                            {
                                continue;
                            }

                            innerSuccess = Int32.TryParse(recodeRow[1].ToString(), out upperBound);

                            if (!innerSuccess && numericValue != lowerBound)
                            {
                                continue;
                            }

                            if ((numericValue == lowerBound) || (numericValue >= lowerBound && numericValue < upperBound))
                            {
                                recodedValue = recodeRow[1].ToString();
                            }

                            if (destinationColumnType.Equals("System.Boolean") || (dashboardHelper.IsUsingEpiProject && dashboardHelper.View.Fields.Contains(destinationColumnName) && dashboardHelper.View.Fields[destinationColumnName] is Epi.Fields.YesNoField))
                            {
                                if (recodedValue.Equals(configYesValue))
                                {
                                    row[this.DestinationColumnName] = true;
                                }
                                else if (recodedValue.Equals(configNoValue))
                                {
                                    row[this.DestinationColumnName] = false;
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(recodedValue))
                                {
                                    if (string.IsNullOrEmpty(ElseValue))
                                    {
                                        row[this.DestinationColumnName] = DBNull.Value;
                                    }
                                }
                                else
                                {
                                    row[this.DestinationColumnName] = recodedValue;
                                }
                            }
                        }
                    }
                }
            }
            else if (sourceColumnType.Equals("System.DateTime"))
            {
                string recodedValue = string.Empty;
                string value = row[this.SourceColumnName].ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    DateTime dateValue;
                    bool outerSuccess = DateTime.TryParse(value, out dateValue);

                    if (outerSuccess)
                    {
                        foreach (DataRow recodeRow in this.RecodeTable.Rows)
                        {
                            if (string.IsNullOrEmpty(recodeRow[0].ToString()) && string.IsNullOrEmpty(recodeRow[1].ToString()))
                            {
                                continue;
                            }

                            DateTime lowerBound;
                            DateTime upperBound;

                            if (recodeRow[0].ToString().Equals("LOVALUE"))
                            {
                                lowerBound = DateTime.MinValue;
                            }
                            if (recodeRow[1].ToString().Equals("HIVALUE"))
                            {
                                upperBound = DateTime.MaxValue;
                            }

                            bool innerSuccess = false;
                            innerSuccess = DateTime.TryParse(recodeRow[0].ToString(), out lowerBound);

                            if (!innerSuccess)
                            {
                                continue;
                            }

                            innerSuccess = DateTime.TryParse(recodeRow[1].ToString(), out upperBound);

                            if (!innerSuccess && dateValue != lowerBound)
                            {
                                continue;
                            }
                            else if (dateValue == lowerBound)
                            {
                                recodedValue = recodeRow[2].ToString();
                            }
                            else if (dateValue >= lowerBound && dateValue < upperBound)
                            {
                                recodedValue = recodeRow[2].ToString();
                            }

                            if (destinationColumnType.Equals("System.Boolean") || (dashboardHelper.IsUsingEpiProject && dashboardHelper.View.Fields.Contains(destinationColumnName) && dashboardHelper.View.Fields[destinationColumnName] is Epi.Fields.YesNoField))
                            {
                                if (recodedValue.Equals(configYesValue))
                                {
                                    row[this.DestinationColumnName] = true;
                                }
                                else if (recodedValue.Equals(configNoValue))
                                {
                                    row[this.DestinationColumnName] = false;
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(recodedValue))
                                {
                                    if (string.IsNullOrEmpty(ElseValue))
                                    {
                                        row[this.DestinationColumnName] = DBNull.Value;
                                    }
                                }
                                else
                                {
                                    row[this.DestinationColumnName] = recodedValue;
                                }
                            }
                        }
                    }
                }
            }
            else if (sourceColumnType.Equals("System.String"))
            {
                string recodedValue = string.Empty;
                string value = row[this.SourceColumnName].ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    foreach (DataRow recodeRow in this.RecodeTable.Rows)
                    {
                        if (string.IsNullOrEmpty(recodeRow[0].ToString()))
                        {
                            continue;
                        }

                        if (recodeRow[0].ToString().Contains("*") && this.ShouldUseWildcards)
                        {
                            string reg = WildcardToRegex(recodeRow[0].ToString());
                            Match match = Regex.Match(value.Replace("\n", string.Empty), reg);
                            if (match.Success)
                            {
                                string sourceValue = recodeRow[0].ToString();
                                recodedValue = recodeRow[1].ToString();

                                if (destinationColumnType.Equals("System.Boolean") || (dashboardHelper.IsUsingEpiProject && dashboardHelper.View.Fields.Contains(destinationColumnName) && dashboardHelper.View.Fields[destinationColumnName] is Epi.Fields.YesNoField))
                                {
                                    if (recodedValue.Equals(configYesValue))
                                    {
                                        row[this.DestinationColumnName] = true;
                                    }
                                    else if (recodedValue.Equals(configNoValue))
                                    {
                                        row[this.DestinationColumnName] = false;
                                    }
                                }
                                else
                                {
                                    row[this.DestinationColumnName] = recodedValue;
                                }
                            }
                        }
                        else
                        {
                            string sourceValue = recodeRow[0].ToString();
                            recodedValue = recodeRow[1].ToString();

                            if (destinationColumnType.Equals("System.Boolean") || (dashboardHelper.IsUsingEpiProject && dashboardHelper.View.Fields.Contains(destinationColumnName) && dashboardHelper.View.Fields[destinationColumnName] is Epi.Fields.YesNoField))
                            {
                                if (value.Equals(sourceValue))
                                {
                                    if (recodedValue.Equals(configYesValue))
                                    {
                                        row[this.DestinationColumnName] = true;
                                    }
                                    else if (recodedValue.Equals(configNoValue))
                                    {
                                        row[this.DestinationColumnName] = false;
                                    }
                                }
                            }
                            else
                            {
                                if (value.Equals(sourceValue))
                                {
                                    row[this.DestinationColumnName] = recodedValue;
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion // IDashboardRule Members
    }
}
