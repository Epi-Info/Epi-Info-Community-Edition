using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Xml;

namespace EpiDashboard.Rules
{
    public enum FormatTypes
    {
        EpiWeek = 0,
        RegularDate = 1,
        Hours = 2,
        HoursMinutes = 3,
        HoursMinutesSeconds = 4,
        SortableDateTime = 5,
        MonthYear = 6,
        DayMonth = 7,
        RFC1123 = 8,
        Day = 9,
        ShortDayName = 10,
        FullDayName = 11,
        Month = 12,
        ShortMonthName = 13,
        FullMonthName = 14,
        TwoDigitYear = 15,
        FourDigitYear = 16,
        NumericInteger = 17,
        NumericDecimal1 = 18,
        NumericDecimal2 = 19,
        NumericDecimal3 = 20,
        NumericDecimal4 = 21,
        NumericDecimal5 = 22,
        MonthAndFourDigitYear = 23,
        LongDate = 24,
        NumericYear = 25,
        NumericMonth = 26,
        NumericDay = 27
    }
    
    public enum RuleExecutionLocation
    {
        /// <summary>
        /// Execute the rule before sending the formatted data to the statistics engine for processing
        /// </summary>
        ExecuteBefore,

        /// <summary>
        /// Execute the rule after sending the formatted data to the statistics engine for processing
        /// </summary>
        ExecuteAfter
    }

    /// <summary>
    /// A class designed to format data for display purposes based on a rule defined by the user
    /// </summary>
    public class Rule_Format : DataAssignmentRule
    {
        #region Private Members
        private string sourceColumnName;
        private string suffix;
        private string formatString;
        //private int decimalDigits; // TODO: Implement later
        private FormatTypes formatType;
        #endregion // Private Members

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public Rule_Format(DashboardHelper dashboardHelper)
        {
            this.dashboardHelper = dashboardHelper;
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourceColumnName">The name of the source column</param>
        /// <param name="formatString"></param>
        /// <param name="formatType">The format type to use when processing this rule</param>
        public Rule_Format(DashboardHelper dashboardHelper, string friendlyRule, string sourceColumnName, string destinationColumnName, string formatString, FormatTypes formatType)
        {            
            this.friendlyRule = friendlyRule;
            this.formatString = formatString;
            this.sourceColumnName = sourceColumnName;
            this.destinationColumnName = destinationColumnName;
            this.formatType = formatType;
            this.suffix = string.Empty;
            this.dashboardHelper = dashboardHelper;
            this.destinationColumnType = "System.String";
            if (formatType == FormatTypes.NumericYear || formatType == FormatTypes.NumericMonth || formatType == FormatTypes.NumericDay)
                this.destinationColumnType = "System.Int32";
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourceColumnName">The name of the source column</param>
        /// <param name="formatString"></param>
        /// <param name="formatType">The format type to use when processing this rule</param>
        /// <param name="customSuffix">The customized suffix to append to the end of the reformatted data</param>
        public Rule_Format(DashboardHelper dashboardHelper, string friendlyRule, string sourceColumnName, string destinationColumnName, string formatString, FormatTypes formatType, string customSuffix)
        {
            this.friendlyRule = friendlyRule;
            this.formatString = formatString;
            this.sourceColumnName = sourceColumnName;
            this.destinationColumnName = destinationColumnName;
            this.formatType = formatType;
            this.suffix = customSuffix;
            this.dashboardHelper = dashboardHelper;
            this.destinationColumnType = "System.String";
            Construct();
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets the name of the source column to be formatted
        /// </summary>
        public string SourceColumnName
        {
            get
            {
                return sourceColumnName;
            }
        }

        /// <summary>
        /// Gets the human-readable version of the format type option
        /// </summary>
        public string FormatString
        {
            get
            {
                return formatString;
            }
        }

        /// <summary>
        /// Gets the formatting type for this rule
        /// </summary>
        public FormatTypes FormatType
        {
            get
            {
                return this.formatType;
            }
        }

        /// <summary>
        /// Gets the customized suffix for the reformatted data
        /// </summary>
        public string Suffix
        {
            get
            {
                return this.suffix;
            }
        }

        /// <summary>
        /// Gets the location for which the formatting rule should be exected. 'Before' implies before any statistics have been run, while 'After' specifies the formatting applies to the output after it has been processed.
        /// </summary>
        public RuleExecutionLocation ExecutionLocation
        {
            get
            {
                return this.GetRuleExecutionLocation();
            }
        }
        #endregion // Public Properties

        #region Public Methods
        /// <summary>
        /// Converts the value of the current EpiDashboard.Rule_Format object to its equivalent string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.FriendlyRule;
        }

        /// <summary>
        /// Gets a list of field names that this rule cannot be run without
        /// </summary>
        /// <returns>List of strings</returns>
        public override List<string> GetDependencies()
        {
            List<string> dependencies = new List<string>();

            dependencies.Add(DestinationColumnName);
            if (!dependencies.Contains(SourceColumnName))
            {
                dependencies.Add(SourceColumnName);
            }

            return dependencies;
        }

        /// <summary>
        /// Gets the format string for this rule
        /// </summary>
        public string GetFormatString()
        {
            string formatString = string.Empty; 
            switch (FormatType)
            {
                case FormatTypes.EpiWeek:
                    formatString = "epiweek"; // note: Special case scenario
                    break;
                case FormatTypes.NumericDay:
                    formatString = "{0:dd}";
                    break;
                case FormatTypes.Day:
                    formatString = "{0:dd}";
                    break;
                case FormatTypes.ShortDayName:
                    formatString = "{0:ddd}";
                    break;
                case FormatTypes.FullDayName:
                    formatString = "{0:dddd}";
                    break;
                case FormatTypes.NumericYear:
                    formatString = "{0:yyyy}";
                    break;
                case FormatTypes.FourDigitYear:
                    formatString = "{0:yyyy}";
                    break;
                case FormatTypes.TwoDigitYear:
                    formatString = "{0:yy}";
                    break;
                case FormatTypes.DayMonth:
                    formatString = "{0:M}";
                    break;
                case FormatTypes.NumericMonth:
                    formatString = "{0:MM}";
                    break;
                case FormatTypes.Month:
                    formatString = "{0:MM}";
                    break;
                case FormatTypes.ShortMonthName:
                    formatString = "{0:MMM}";
                    break;
                case FormatTypes.FullMonthName:
                    formatString = "{0:MMMM}";
                    break;
                case FormatTypes.RFC1123:
                    formatString = "{0:r}";
                    break;
                case FormatTypes.RegularDate:
                    formatString = "{0:d}";
                    break;
                case FormatTypes.LongDate:
                    formatString = "{0:D}";
                    break;
                case FormatTypes.SortableDateTime:
                    formatString = "{0:s}";
                    break;
                case FormatTypes.MonthYear:
                    formatString = "{0:y}";
                    break;
                case FormatTypes.Hours:
                    formatString = "{0:HH}";
                    break;
                case FormatTypes.HoursMinutes:
                    formatString = "{0:t}";
                    break;
                case FormatTypes.HoursMinutesSeconds:
                    formatString = "{0:T}";
                    break;
                case FormatTypes.NumericInteger:
                    formatString = "{0:0}";
                    break;
                case FormatTypes.NumericDecimal1:
                    formatString = "{0:0.0}";
                    break;
                case FormatTypes.NumericDecimal2:
                    formatString = "{0:0.00}";
                    break;
                case FormatTypes.NumericDecimal3:
                    formatString = "{0:0.000}";
                    break;
                case FormatTypes.NumericDecimal4:
                    formatString = "{0:0.0000}";
                    break;
                case FormatTypes.NumericDecimal5:
                    formatString = "{0:0.00000}";
                    break;
                case FormatTypes.MonthAndFourDigitYear:
                    formatString = "{0:y}";
                    break;
            }

            return formatString;
        }
        #endregion // Public Methods

        #region Private Methods
        /// <summary>
        /// Constructs the rule
        /// </summary>
        private void Construct()
        {
            if (this.FormatType.Equals(FormatTypes.Hours))
            {
                this.destinationColumnType = "System.Decimal";
                this.variableType = DashboardVariableType.Numeric;
            }
            else if (this.FormatType.Equals(FormatTypes.EpiWeek))
            {
                this.destinationColumnType = "System.Decimal";
                this.variableType = DashboardVariableType.Numeric;
            }
        }

        /// <summary>
        /// Gets the location for which the rule should be exected. 'Before' implies before any statistics have been run, while 'After' specifies the formatting applies to the output after it has been processed.
        /// </summary>
        /// <returns>RuleExecutionLocation</returns>
        private RuleExecutionLocation GetRuleExecutionLocation()
        {
            RuleExecutionLocation location = RuleExecutionLocation.ExecuteBefore;
            switch (FormatType)
            {
                case FormatTypes.EpiWeek:
                    location = RuleExecutionLocation.ExecuteBefore;
                    break;
                case FormatTypes.Day:
                    location = RuleExecutionLocation.ExecuteBefore;
                    break;
                case FormatTypes.ShortDayName:
                    location = RuleExecutionLocation.ExecuteBefore;
                    break;
                case FormatTypes.FullDayName:
                    location = RuleExecutionLocation.ExecuteBefore;
                    break;
                case FormatTypes.FourDigitYear:
                    location = RuleExecutionLocation.ExecuteBefore;
                    break;
                case FormatTypes.TwoDigitYear:
                    location = RuleExecutionLocation.ExecuteBefore;
                    break;
                case FormatTypes.DayMonth:
                    location = RuleExecutionLocation.ExecuteBefore;
                    break;
                case FormatTypes.Month:
                    location = RuleExecutionLocation.ExecuteBefore;
                    break;
                case FormatTypes.ShortMonthName:
                    location = RuleExecutionLocation.ExecuteBefore;
                    break;
                case FormatTypes.FullMonthName:
                    location = RuleExecutionLocation.ExecuteBefore;
                    break;
                case FormatTypes.RFC1123:
                    location = RuleExecutionLocation.ExecuteAfter;
                    break;
                case FormatTypes.RegularDate:
                    location = RuleExecutionLocation.ExecuteAfter;
                    break;
                case FormatTypes.SortableDateTime:
                    location = RuleExecutionLocation.ExecuteAfter;
                    break;
                case FormatTypes.MonthYear:
                    location = RuleExecutionLocation.ExecuteBefore;
                    break;
                case FormatTypes.HoursMinutes:
                    location = RuleExecutionLocation.ExecuteAfter;
                    break;
                case FormatTypes.HoursMinutesSeconds:
                    location = RuleExecutionLocation.ExecuteAfter;
                    break;
                case FormatTypes.NumericInteger:
                    location = RuleExecutionLocation.ExecuteAfter;
                    break;
                case FormatTypes.NumericDecimal1:
                    location = RuleExecutionLocation.ExecuteAfter;
                    break;
                case FormatTypes.NumericDecimal2:
                    location = RuleExecutionLocation.ExecuteAfter;
                    break;
                case FormatTypes.NumericDecimal3:
                    location = RuleExecutionLocation.ExecuteAfter;
                    break;
                case FormatTypes.NumericDecimal4:
                    location = RuleExecutionLocation.ExecuteAfter;
                    break;
                case FormatTypes.NumericDecimal5:
                    location = RuleExecutionLocation.ExecuteAfter;
                    break;
            }

            return location;   
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
            "<destinationColumnName>" + destinationColumnName + "</destinationColumnName>" +
            "<destinationColumnType>" + destinationColumnType + "</destinationColumnType>" +
            "<formatString>" + formatString + "</formatString>" +
            "<formatType>" + ((int)formatType).ToString() + "</formatType>";

            System.Xml.XmlElement element = doc.CreateElement("rule");
            element.InnerXml = xmlString;

            System.Xml.XmlAttribute order = doc.CreateAttribute("order");
            System.Xml.XmlAttribute type = doc.CreateAttribute("ruleType");

            type.Value = "EpiDashboard.Rules.Rule_Format";

            element.Attributes.Append(type);

            return element;
        }

        /// <summary>
        /// Creates the rule from an Xml element
        /// </summary>
        /// <param name="element">The XmlElement from which to create the rule</param>
        public override void CreateFromXml(System.Xml.XmlElement element)
        {
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
                else if (child.Name.Equals("destinationColumnName"))
                {
                    this.destinationColumnName = child.InnerText;
                }
                else if (child.Name.Equals("destinationColumnType"))
                {
                    this.destinationColumnType = child.InnerText;
                }
                else if (child.Name.Equals("formatString"))
                {
                    this.formatString = child.InnerText;
                }
                else if (child.Name.Equals("formatType"))
                {
                    this.formatType = ((FormatTypes)Int32.Parse(child.InnerText));
                }
            }

            if (this.FormatType.Equals(FormatTypes.Hours))
            {
                this.destinationColumnType = "System.Decimal";
                this.variableType = DashboardVariableType.Numeric;
            }
            else if (this.FormatType.Equals(FormatTypes.EpiWeek))
            {
                this.destinationColumnType = "System.Decimal";
                this.variableType = DashboardVariableType.Numeric;
            }
            else if (destinationColumnType.Equals("System.String"))
            {
                this.variableType = DashboardVariableType.Text;
            }
            else if (destinationColumnType.Equals("System.Single") || destinationColumnType.Equals("System.Double") || destinationColumnType.Equals("System.Decimal") || destinationColumnType.Equals("System.Int16") || destinationColumnType.Equals("System.Int32") || destinationColumnType.Equals("System.SByte"))
            {
                this.variableType = DashboardVariableType.Numeric;
            }
        }

        /// <summary>
        /// Sets up the rule
        /// </summary>
        public override void SetupRule(DataTable table)
        {
            string destinationColumnType = this.DestinationColumnType;
            string sourceColumnType = DashboardHelper.GetColumnType(this.SourceColumnName);

            DataColumn dc;

            switch (destinationColumnType)
            {
                case "System.Byte":
                case "System.SByte":
                    dc = new DataColumn(this.DestinationColumnName, typeof(byte));
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
        }

        /// <summary>
        /// Applies the rule
        /// </summary>
        public override void ApplyRule(DataRow row)
        {
            if (!row.Table.Columns.Contains(this.DestinationColumnName) || !row.Table.Columns.Contains(this.SourceColumnName))
            {
                return;
            }

            string value = string.Empty;

            if (this.FormatType != FormatTypes.EpiWeek)
            {
                value = string.Format(System.Globalization.CultureInfo.CurrentCulture, this.GetFormatString(), row[this.SourceColumnName]) + this.Suffix;
                value = value.Trim();
                if ((this.formatType == FormatTypes.NumericDay || this.formatType == FormatTypes.NumericMonth) && value[0] == '0')
                    value = value.Trim('0');
            }
            else
            {
                StatisticsRepository.EpiWeek epiWeek = new StatisticsRepository.EpiWeek();
                if(row[this.SourceColumnName] != null && !string.IsNullOrEmpty(row[this.SourceColumnName].ToString())) 
                {
                    string datestring = row[this.SourceColumnName].ToString();
                    DateTime? dt = Convert.ToDateTime(datestring);
                    value = epiWeek.GetEpiWeek(dt).ToString();
                    value = value.Trim();
                }
                
            }

            if (string.IsNullOrEmpty(value))
            {
                row[this.DestinationColumnName] = DBNull.Value;
            }
            else
            {
                row[this.DestinationColumnName] = value;
            }
        }
        #endregion // IDashboardRule Members

    }
}
