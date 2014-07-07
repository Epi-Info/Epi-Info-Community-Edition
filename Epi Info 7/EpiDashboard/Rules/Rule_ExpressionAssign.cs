using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using Epi;
using Epi.Core;
using Epi.Fields;

namespace EpiDashboard.Rules
{   

    /// <summary>
    /// A class designed to assign data to another variable using a mathematical expression created by the user
    /// </summary>
    public class Rule_ExpressionAssign : DataAssignmentRule
    {
        #region Private Members
        private string expression;        
        #endregion // Private Members

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Rule_ExpressionAssign()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Rule_ExpressionAssign(DashboardHelper dashboardHelper)
        {
            this.dashboardHelper = dashboardHelper;
        }

        /// <summary>
        /// Constructor for simple assignment
        /// </summary>
        public Rule_ExpressionAssign(DashboardHelper dashboardHelper, string friendlyRule, string destinationColumnName, string expression)
        {
            this.friendlyRule = friendlyRule;
            this.destinationColumnName = destinationColumnName;
            this.destinationColumnType = "System.Decimal";
            this.DashboardHelper = dashboardHelper;
            this.variableType = DashboardVariableType.Numeric;
            this.expression = expression;
        }

        /// <summary>
        /// Constructor for simple assignment with custom column type
        /// </summary>
        public Rule_ExpressionAssign(DashboardHelper dashboardHelper, string friendlyRule, string destinationColumnName, string destinationColumnType, string expression)
        {
            this.friendlyRule = friendlyRule;
            this.destinationColumnName = destinationColumnName;
            this.destinationColumnType = destinationColumnType;
            this.DashboardHelper = dashboardHelper;

            if (DestinationColumnType.Equals("System.String"))
            {
                this.variableType = DashboardVariableType.Text;
            }
            else if (DestinationColumnType.Equals("System.DateTime"))
            {
                this.variableType = DashboardVariableType.Date;
            }
            else
            {
                this.variableType = DashboardVariableType.Numeric;
            }
            this.expression = expression;
        }
        #endregion // Constructors

        #region Public Methods
        /// <summary>
        /// Gets a list of field names that this rule cannot be run without
        /// </summary>
        /// <returns>List of strings</returns>
        public override List<string> GetDependencies()
        {
            List<string> dependencies = new List<string>();

            dependencies.Add(DestinationColumnName);

            foreach (KeyValuePair<string, string> kvp in dashboardHelper.TableColumnNames)
            {
                if (this.Expression.Contains(kvp.Key))
                {
                    dependencies.Add(kvp.Key);
                }
            }

            return dependencies;
        }

        public override void SetupRule(DataTable table)
        {
            string destinationColumnType = this.DestinationColumnType;

            DataColumn dc = new DataColumn(destinationColumnName);

            if (!table.Columns.Contains(dc.ColumnName))
            {
                switch (destinationColumnType)
                {
                    case "System.SByte":
                    case "System.Byte":
                        dc = new DataColumn(this.DestinationColumnName, typeof(byte));
                        break;
                    case "System.Single":
                    case "System.Double":
                        dc = new DataColumn(this.DestinationColumnName, typeof(double));
                        break;
                    case "System.Decimal":
                        dc = new DataColumn(this.DestinationColumnName, typeof(decimal));
                        break;
                    case "System.DateTime":
                        dc = new DataColumn(this.DestinationColumnName, typeof(DateTime));
                        break;
                    case "System.String":
                    default:
                        dc = new DataColumn(this.DestinationColumnName, typeof(string));
                        break;
                }

                dc.Expression = this.expression;

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
                dc.Expression = this.expression;
            }
        }
        #endregion //Public Methods

        #region Public Properties
        /// <summary>
        /// Gets the rule's expression
        /// </summary>
        public string Expression
        {
            get
            {
                return this.expression;
            }
        }
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
        /// Generates an Xml element for this rule
        /// </summary>
        /// <param name="doc">The parent Xml document</param>
        /// <returns>XmlNode representing this rule</returns>
        public override System.Xml.XmlNode Serialize(System.Xml.XmlDocument doc)
        {
            string xmlString =
            "<friendlyRule>" + friendlyRule + "</friendlyRule>" +
            "<expression>" + expression + "</expression>" +
            "<destinationColumnName>" + destinationColumnName + "</destinationColumnName>" +
            "<destinationColumnType>" + destinationColumnType + "</destinationColumnType>";

            System.Xml.XmlElement element = doc.CreateElement("rule");
            element.InnerXml = xmlString;

            System.Xml.XmlAttribute order = doc.CreateAttribute("order");
            System.Xml.XmlAttribute type = doc.CreateAttribute("ruleType");
            
            type.Value = "EpiDashboard.Rules.Rule_ExpressionAssign";
            
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
                else if (child.Name.Equals("expression"))
                {
                    this.expression = child.InnerText;
                }
                else if (child.Name.Equals("destinationColumnName"))
                {
                    this.destinationColumnName = child.InnerText;
                }
                else if (child.Name.Equals("destinationColumnType"))
                {
                    this.destinationColumnType = child.InnerText;
                }
            }

            if (DestinationColumnType.Equals("System.String"))
            {
                this.variableType = DashboardVariableType.Text;
            }
            else if (DestinationColumnType.Equals("System.DateTime"))
            {
                this.variableType = DashboardVariableType.Date;
            }
            else
            {
                this.variableType = DashboardVariableType.Numeric;
            }
        }

        /// <summary>
        /// Applies the rule
        /// </summary>
        public override void ApplyRule(DataRow row)
        {   
        }
        #endregion // IDashboardRule Members
    }
}
