using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using Epi;
using Epi.Core;
using Epi.Fields;

namespace EpiDashboard.Rules
{

    /// <summary>
    /// A class designed to assign data to another variable using a 
    /// mathematical expression created by the user, but only when 
    /// certain conditions are met. Generally, this should be equivalent
    /// to an IF-THEN-ELSE in check code or Analysis PGMs.
    /// </summary>
    public class Rule_ConditionalAssign : DataAssignmentRule
    {
        #region Private Members
        //Dictionary<string, object> conditions = new Dictionary<string,object>();
        private string condition;
        private object elseValue;
        private DataView dvAssign;
        private object assignValue;
        #endregion // Private Members

        #region Public Members
        public DataFilters DataFilters;
        public bool UseElse;
        #endregion // Public Members

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Rule_ConditionalAssign()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Rule_ConditionalAssign(DashboardHelper dashboardHelper)
        {
            this.dashboardHelper = dashboardHelper;
        }

        /// <summary>
        /// Constructor for conditional assignment with custom column type
        /// </summary>
        public Rule_ConditionalAssign(DashboardHelper dashboardHelper, string friendlyRule, string destinationColumnName, string destinationColumnType, /*Dictionary<string, object> conditions*/object assignValue, object elseValue, string condition)
        {
            this.friendlyRule = friendlyRule;
            this.destinationColumnName = destinationColumnName;
            this.destinationColumnType = destinationColumnType;
            this.DashboardHelper = dashboardHelper;
            this.condition = condition;

            if (DestinationColumnType.Equals("System.String"))
            {
                this.variableType = DashboardVariableType.Text;
            }
            else if (DestinationColumnType.Equals("System.Boolean"))
            {
                this.variableType = DashboardVariableType.YesNo;
            }
            else
            {
                this.variableType = DashboardVariableType.Numeric;
            }
            //this.conditions = conditions;
            this.assignValue = assignValue;
            this.elseValue = elseValue;
            if (elseValue == null)
            {
                UseElse = false;
            }
            else
            {
                UseElse = true;
            }
        }
        #endregion // Constructors

        #region Public Methods

        public override void SetupRule(DataTable table)
        {
            string destinationColumnType = this.DestinationColumnType;

            DataColumn dc = new DataColumn(destinationColumnName);

            if (!table.Columns.Contains(dc.ColumnName))
            {
                switch (destinationColumnType)
                {
                    case "System.Boolean":
                        dc = new DataColumn(this.DestinationColumnName, typeof(bool));
                        break;
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

                try
                {
                    table.Columns.Add(dc);
                }
                catch (ArgumentException)
                {
                    dc = new DataColumn(DestinationColumnName);
                    table.Columns.Add(dc);
                }
            }
            else
            {
                dc = table.Columns[dc.ColumnName];
            }


            dvAssign = new DataView(table, DataFilters.GenerateDataFilterString(), string.Empty, DataViewRowState.CurrentRows);

            //foreach (KeyValuePair<string, object> kvp in this.conditions)
            //{
            //    dvAssign = new DataView(table, kvp.Key, string.Empty, DataViewRowState.CurrentRows);               

            //    assignValue = kvp.Value;
            //    // currently, we're supporting only one condition.
            //    // This KVP list may be useful later on if ELSE-IF is 
            //    // added as a feature. For now, just kill the loop.
            //    break; 
            //}
        }
        #endregion //Public Methods

        #region Public Properties
        public object ElseValue
        {
            get
            {
                return this.elseValue;
            }
        }

        public object AssignValue
        {
            get
            {
                return this.assignValue;
            }
        }

        public string Condition
        {
            get
            {
                return this.condition;
            }
        }

        //public Dictionary<string, object> Conditions
        //{
        //    get
        //    {
        //        return this.conditions;
        //    }
        //}
        #endregion // Public Properties

        #region Public Methods
        /// <summary>
        /// Converts the value of the current EpiDashboard.Rule_ExpressionAssign object to its equivalent string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.FriendlyRule;
        }
        #endregion // Public Methods

        #region Protected Methods
        /// <summary>
        /// Gets a column type appropriate for a .NET data table based off of the dashboard variable type selected by the user
        /// </summary>
        /// <param name="dashboardVariableType">The type of variable that is storing the recoded values</param>
        /// <returns>A string representing the type of a .NET DataColumn</returns>
        protected string GetDestinationColumnType(DashboardVariableType dashboardVariableType)
        {
            switch (dashboardVariableType)
            {
                case DashboardVariableType.Numeric:
                    return "System.Single";
                case DashboardVariableType.Text:
                    return "System.String";
                case DashboardVariableType.YesNo:
                    return "System.Byte";
                case DashboardVariableType.Date:
                    return "System.DateTime";
                case DashboardVariableType.None:
                    throw new ApplicationException(SharedStrings.DASHBOARD_ERROR_INVALID_COLUMN_TYPE);
                default:
                    return "System.String";
            }
        }
        #endregion // Protected Methods

        #region IDashboardRule Members
        /// <summary>
        /// Gets a list of field names that this rule cannot be run without
        /// </summary>
        /// <returns>List of strings</returns>
        public override List<string> GetDependencies()
        {
            List<string> dependencies = new List<string>();

            foreach (FilterCondition fc in this.DataFilters)
            {
                if (!dependencies.Contains(fc.RawColumnName))
                {
                    dependencies.Add(fc.RawColumnName);
                }
            }

            if (!dependencies.Contains(this.DestinationColumnName))
            {
                dependencies.Add(this.DestinationColumnName);
            }

            return dependencies;
        }

        /// <summary>
        /// Generates an Xml element for this rule
        /// </summary>
        /// <param name="doc">The parent Xml document</param>
        /// <returns>XmlNode representing this rule</returns>
        public override System.Xml.XmlNode Serialize(System.Xml.XmlDocument doc)
        {
            if (assignValue.ToString().Length > 0 && string.IsNullOrEmpty(assignValue.ToString().Trim()))
            {
                assignValue = "&#xA0;";
            }

            if (UseElse && elseValue != null && elseValue.ToString().Length > 0 && string.IsNullOrEmpty(elseValue.ToString().Trim()))
            {
                elseValue = "&#xA0;";
            }

            string xmlString =
            "<friendlyRule>" + friendlyRule.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "</friendlyRule>" +
            "<destinationColumnName>" + destinationColumnName + "</destinationColumnName>" +
            "<destinationColumnType>" + destinationColumnType + "</destinationColumnType>";

            if (DestinationColumnType == "System.Decimal" && assignValue is decimal)
            {
                decimal d = (decimal)assignValue;
                xmlString += "<assignValue>" + d.ToString(CultureInfo.InvariantCulture).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "</assignValue>";
            }
            else
            {
                xmlString += "<assignValue>" + assignValue.ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "</assignValue>";
            }

            xmlString += "<useElse>" + UseElse + "</useElse>";
            if (elseValue != null)
            {
                //xmlString = xmlString + "<elseValue>" + elseValue.ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "</elseValue>";
                if (DestinationColumnType == "System.Decimal" && elseValue is decimal)
                {
                    decimal d = (decimal)elseValue;
                    xmlString += "<elseValue>" + d.ToString(CultureInfo.InvariantCulture).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "</elseValue>";
                }
                else
                {
                    xmlString += "<elseValue>" + elseValue.ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "</elseValue>";
                }
            }

            System.Xml.XmlElement element = doc.CreateElement("rule");
            element.InnerXml = xmlString;

            element.AppendChild(DataFilters.Serialize(doc));

            System.Xml.XmlAttribute order = doc.CreateAttribute("order");
            System.Xml.XmlAttribute type = doc.CreateAttribute("ruleType");

            type.Value = "EpiDashboard.Rules.Rule_ConditionalAssign";

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
                if (child.Name.ToLower().Equals("friendlyrule"))
                {
                    this.friendlyRule = child.InnerText.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">");
                }
                else if (child.Name.ToLower().Equals("destinationcolumnname"))
                {
                    this.destinationColumnName = child.InnerText;
                }
                else if (child.Name.ToLower().Equals("destinationcolumntype"))
                {
                    this.destinationColumnType = child.InnerText;
                }
                else if (child.Name.ToLower().Equals("datafilters"))
                {
                    this.DataFilters = new DataFilters(this.dashboardHelper);
                    this.DataFilters.CreateFromXml(child);
                }
                else if (child.Name.ToLower().Equals("useelse"))
                {
                    this.UseElse = bool.Parse(child.InnerText);
                }
            }

            if (DestinationColumnType.Equals("System.String"))
            {
                this.variableType = DashboardVariableType.Text;
            }
            else if (destinationColumnType.Equals("System.Boolean"))
            {
                this.variableType = DashboardVariableType.YesNo;
            }
            else
            {
                this.variableType = DashboardVariableType.Numeric;
            }

            foreach (XmlElement child in element.ChildNodes)
            {
                if (child.Name.ToLower().Equals("assignvalue"))
                {
                    if (child.InnerText.Equals("&#xA0;"))
                    {
                        this.assignValue = " ";
                    }
                    else
                    {
                        //this.assignValue = child.InnerText.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">");
                        string strValue = child.InnerText.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">");

                        if (variableType == DashboardVariableType.Numeric)
                        {
                            decimal d;
                            bool success = Decimal.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out d);
                            if (success)
                            {
                                this.assignValue = d;
                            }
                            //this.elseValue = decimal.Parse()
                        }
                        else
                        {
                            this.assignValue = strValue;
                        }
                    }
                }
                else if (child.Name.ToLower().Equals("elsevalue"))
                {
                    if (child.InnerText.Equals("&#xA0;"))
                    {
                        this.elseValue = " ";
                    }
                    else
                    {
                        string strValue = child.InnerText.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">");

                        if (variableType == DashboardVariableType.Numeric)
                        {
                            decimal d;
                            bool success = Decimal.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out d);
                            if (success)
                            {
                                this.elseValue = d;
                            }
                            //this.elseValue = decimal.Parse()
                        }
                        else
                        {
                            this.elseValue = strValue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies the rule
        /// </summary>
        public override void ApplyRule(DataRow row)
        {
            // This method seems inefficient, but it's necessary to handle it in this manner so that the order of 
            // variable assignments is maintained across different types of created variables.

            bool assigned = false;

            row.AcceptChanges();

            foreach (DataRowView assignRowView in dvAssign)
            {
                DataRow assignRow = assignRowView.Row;
                if (assignRow == row)
                {
                    assignRow[destinationColumnName] = assignValue;
                    assigned = true;
                    break;
                }
            }

            if (elseValue != null && !assigned)
            {
                row[destinationColumnName] = elseValue;
            }
        }
        #endregion // IDashboardRule Members
    }
}
