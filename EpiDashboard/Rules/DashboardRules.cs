using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using Epi;
using EpiDashboard;
using EpiDashboard.Rules;

namespace EpiDashboard.Rules
{
    public enum VariableDependencyType
    {
        DestinationColumn = 0,
        SourceColumn = 1
    }

    public struct VariableDependency
    {
        public List<string> DependsOn;
        public string VariableName;
    }

    public class DashboardRules : IEnumerable<IDashboardRule>
    {
        #region Private Members
        private DataTable ruleTable;        
        private DashboardHelper dashboardHelper;
        #endregion // Private Members

        #region Constants
        public const string COLUMN_RUN_ORDER = "Run Order";   // the order in which the rule should be run
        public const string COLUMN_RULE = "Rule";
        #endregion // Constants

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public DashboardRules()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DashboardRules(DashboardHelper dashboardHelper)
        {
            this.dashboardHelper = dashboardHelper;

            DataColumn runOrderColumn = new DataColumn(COLUMN_RUN_ORDER, typeof(int));
            DataColumn ruleColumn = new DataColumn(COLUMN_RULE, typeof(IDashboardRule));

            runOrderColumn.AllowDBNull = false;
            ruleColumn.AllowDBNull = false;

            this.ruleTable = new DataTable("ruleTable");
            RuleTable.Columns.Add(runOrderColumn);            
            RuleTable.Columns.Add(ruleColumn);            
        }
        #endregion // Constructors

        #region Private Properties
        /// <summary>
        /// Gets the data table containing the dashboard rules
        /// </summary>
        private DataTable RuleTable
        {
            get
            {
                return this.ruleTable;
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

        #region Public Properties
        /// <summary>
        /// Gets the number of elements contained in the DataFilter
        /// </summary>
        public int Count
        {
            get
            {
                if (this.RuleTable != null)
                {
                    return this.RuleTable.Rows.Count;
                }
                else
                {
                    return 0;
                }
            }
        }
        #endregion // Public Properties

        #region Public Methods
        /// <summary>
        /// Adds a new rule to the set of dashboard rules
        /// </summary>        
        /// <param name="rule">The rule to add</param>
        /// <returns>Boolean; whether the addition was successful</returns>
        public bool AddRule(IDashboardRule rule)
        {
            #region Input Validation
            if (rule == null || string.IsNullOrEmpty(rule.FriendlyRule))
            {
                throw new ArgumentNullException("rule");
            }
            #endregion // Input Validation

            // check for duplicate rules
            foreach (DataRow row in RuleTable.Rows)
            {
                IDashboardRule rRule = (IDashboardRule)row[COLUMN_RULE];

                if (rRule.FriendlyRule.Equals(rule.FriendlyRule))
                {
                    // cannot add a duplicate rule, so return false
                    return false;
                }
            }

            // find the highest run order value of all the rules presently in the table
            int maxInt = int.MinValue;

            foreach (DataRow row in RuleTable.Rows)
            {
                int value = row.Field<int>(COLUMN_RUN_ORDER);
                maxInt = Math.Max(maxInt, value);
            }

            RuleTable.Rows.Add(maxInt + 1, rule);

            // sort by run order
            this.ruleTable = RuleTable.Select("", COLUMN_RUN_ORDER).CopyToDataTable().DefaultView.ToTable("RuleTable");

            if (rule is DataAssignmentRule)
            {
                DataAssignmentRule assignRule = rule as DataAssignmentRule;
                if (!dashboardHelper.TableColumnNames.ContainsKey(assignRule.DestinationColumnName))
                {
                    dashboardHelper.TableColumnNames.Add(assignRule.DestinationColumnName, assignRule.DestinationColumnType);
                }
            }

            return true;
        }

        /// <summary>
        /// Clears all dashboard rules
        /// </summary>
        public void ClearRules()
        {
            this.RuleTable.Rows.Clear();
        }

        /// <summary>
        /// Removes a rule
        /// </summary>
        /// <returns>Bool</returns>
        public bool RemoveRule(string friendlyRule)
        {
            int i;
            int rowIndexToRemove = -1;
            DataRow row = null;

            for (i = 0; i < RuleTable.Rows.Count; i++)
            {
                row = RuleTable.Rows[i];
                string str = row[COLUMN_RULE].ToString();

                if (str.Equals(friendlyRule))
                {
                    rowIndexToRemove = i;
                    break;
                }
            }

            if (rowIndexToRemove >= 0)
            {
                row = RuleTable.Rows[rowIndexToRemove];

                IDashboardRule rule = ((IDashboardRule)row[COLUMN_RULE]);

                if (rule is DataAssignmentRule && this.GetColumnAssignmentCount(rule.FriendlyRule) == 1)
                {
                    DataAssignmentRule assignRule = rule as DataAssignmentRule;
                    if (dashboardHelper.TableColumnNames.ContainsKey(assignRule.DestinationColumnName))
                    {
                        if (assignRule.OverwritesPermanentField == false)
                        {
                            dashboardHelper.TableColumnNames.Remove(assignRule.DestinationColumnName);
                        }
                    }
                }

                RuleTable.Rows.Remove(row);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the dependencies for a particular defined variable
        /// </summary>
        /// <returns>void</returns>
        //public void GetDependencies(string friendlyRule)
        //{
        //    IDashboardRule rule = GetRule(friendlyRule);

        //    if (rule is DataAssignmentRule)
        //    {
        //        DataAssignmentRule assignRule = rule as DataAssignmentRule;
        //        return GetRules(assignRule.DestinationColumnName).Count - 1;
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        /// <summary>
        /// Gets the number of dependencies for a particular defined variable
        /// </summary>
        /// <returns>Int; represents the number of variables dependent on the variable passed in as a parameter</returns>
        public int GetDependencyCount(string friendlyRule)
        {            
            return GetDependencyList(friendlyRule).Count;          
        }

        /// <summary>
        /// Gets the dependencies for a particular defined variable
        /// </summary>
        /// <returns>Int; represents the number of variables dependent on the variable passed in as a parameter</returns>
        public List<VariableDependency> GetDependencyList(string friendlyRule)
        {
            IDashboardRule rule = GetRule(friendlyRule);
            List<VariableDependency> dependencies = new List<VariableDependency>();

            if (rule is DataAssignmentRule)
            {
                // the one we're checking on for dependencies
                DataAssignmentRule assignRule = rule as DataAssignmentRule; 

                for (int i = 0; i < RuleTable.Rows.Count; i++)
                {
                    DataRow row = RuleTable.Rows[i];
                    IDashboardRule iRule = ((IDashboardRule)row[COLUMN_RULE]);

                    if (rule is DataAssignmentRule && rule.FriendlyRule != friendlyRule)
                    {
                        // a variable in the list, being checked to see if it's being depended on
                        //DataAssignmentRule iAssignRule = rule as DataAssignmentRule;

                        //VariableDependency vd = new VariableDependency();
                        //vd.VariableName = iAssignRule.DestinationColumnName;

                        //if (iAssignRule is Rule_Recode)
                        //{
                        //    Rule_Recode iRecodeRule = iAssignRule as Rule_Recode;
                        //    if(iRecodeRule.SourceColumnName
                        //}


                        //if (assignRule.DestinationColumnName.ToLower().Equals(destinationColumnName.ToLower()))
                        //{
                        //    rules.Add(assignRule);
                        //}
                    }
                }
            }

            return dependencies;
        }

        /// <summary>
        /// Gets the number of assignments for a given column
        /// </summary>
        /// <returns>Int; represents the number of variables dependent on the variable passed in as a parameter</returns>
        public int GetColumnAssignmentCount(string friendlyRule)
        {
            IDashboardRule rule = GetRule(friendlyRule);

            if (rule is DataAssignmentRule)
            {
                DataAssignmentRule assignRule = rule as DataAssignmentRule;
                return GetRules(assignRule.DestinationColumnName).Count;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Returns a rule from a given friendly rule
        /// </summary>
        /// <param name="friendlyRule">The friendly rule</param>
        /// <returns>Rule</returns>
        public IDashboardRule GetRule(string friendlyRule)
        {
            int i;            
            DataRow row = null;

            for (i = 0; i < RuleTable.Rows.Count; i++)
            {
                row = RuleTable.Rows[i];
                string str = row[COLUMN_RULE].ToString();

                if (str.Equals(friendlyRule))
                {
                    return ((IDashboardRule)row[COLUMN_RULE]);                    
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the run order for a given rule
        /// </summary>
        /// <param name="friendlyRule">The friendly rule</param>
        /// <returns>int; Represents the run order number for the rule</returns>
        public int GetRuleRunOrder(string friendlyRule)
        {
            int i;
            DataRow row = null;

            for (i = 0; i < RuleTable.Rows.Count; i++)
            {
                row = RuleTable.Rows[i];
                string str = row[COLUMN_RULE].ToString();

                if (str.Equals(friendlyRule))
                {
                    return ((int)row[COLUMN_RUN_ORDER]);
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns all user-defined variables with the given destination column
        /// </summary>
        /// <param name="destinationColumnName">The destination column name</param>
        /// <returns>Rule</returns>
        public List<IDashboardRule> GetRules(string destinationColumnName)
        {
            int i;
            DataRow row = null;
            List<IDashboardRule> rules = new List<IDashboardRule>();

            for (i = 0; i < RuleTable.Rows.Count; i++)
            {
                row = RuleTable.Rows[i];
                IDashboardRule rule = ((IDashboardRule)row[COLUMN_RULE]);
                if (rule is DataAssignmentRule)
                {
                    DataAssignmentRule assignRule = rule as DataAssignmentRule;
                    if (assignRule.DestinationColumnName.ToLower().Equals(destinationColumnName.ToLower()))
                    {
                        rules.Add(assignRule);
                    }
                }
            }

            return rules;
        }

        /// <summary>
        /// Gets a dictionary of all user-defined variables in this rule set
        /// </summary>
        /// <returns>Dictionary of user-defined variables</returns>
        public Dictionary<string, string> GetUserDefinedVariables()
        {
            Dictionary<string, string> userDefinedVariableNames = new Dictionary<string, string>();
            foreach (IDashboardRule rule in this)
            {
                if (rule is DataAssignmentRule)
                {
                    DataAssignmentRule assignRule = rule as DataAssignmentRule;
                    userDefinedVariableNames.Add(assignRule.DestinationColumnName, assignRule.DestinationColumnType);
                }
            }

            return userDefinedVariableNames;
        }

        /// <summary>
        /// Updates an existing rule
        /// </summary>        
        /// <param name="originalRule">The rule to be updated</param>
        /// <param name="updatedRule">The rule to update</param>
        /// <returns>Boolean; whether the addition was successful</returns>
        public bool UpdateRule(IDashboardRule originalRule, IDashboardRule updatedRule)
        {
            int i;
            int rowIndexToUpdate = -1;
            DataRow row = null;

            for (i = 0; i < RuleTable.Rows.Count; i++)
            {
                row = RuleTable.Rows[i];                

                IDashboardRule currentRowRule = ((IDashboardRule)row[COLUMN_RULE]);

                if (currentRowRule.Equals(originalRule))
                {
                    rowIndexToUpdate = i;
                    break;
                }
            }

            if (rowIndexToUpdate >= 0)
            {
                row = RuleTable.Rows[rowIndexToUpdate];
                row[COLUMN_RULE] = updatedRule;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Convert the set of rules into XML
        /// </summary>
        /// <returns>XmlElement</returns>
        public XmlElement Serialize(XmlDocument doc)
        {
            System.Xml.XmlElement root = doc.CreateElement("dashboardRules");

            foreach (IDashboardRule rule in this)
            {
                root.AppendChild(rule.Serialize(doc));
            }
            return root;
        }

        /// <summary>
        /// Create the set of dashboard rules from an XML element
        /// </summary>
        /// <param name="element">The XML element from which to create the rules</param>
        public void CreateFromXml(XmlElement element)
        {
            foreach (System.Xml.XmlElement iChild in element.ChildNodes)
            {
                if (iChild.Name.Equals("rule"))
                {
                    string type = string.Empty;
                    foreach (XmlAttribute attribute in iChild.Attributes)
                    {
                        switch (attribute.Name.ToLower())
                        {
                            case "ruletype":
                                type = attribute.Value;
                                break;                            
                        }
                    }

                    IDashboardRule rule = (IDashboardRule)Activator.CreateInstance(Type.GetType(type), new object[] { dashboardHelper });

                    try
                    {
                        rule.CreateFromXml(iChild);
                        AddRule(rule);
                    }
                    catch (NullReferenceException ex)
                    {
                        Epi.Windows.MsgBox.ShowWarning(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the type of a given defined variable
        /// </summary>
        /// <returns></returns>
        public string GetDefinedVariableType(string variableName)
        {
            foreach (IDashboardRule rule in this)
            {
                if (rule is DataAssignmentRule)
                {
                    DataAssignmentRule assignRule = rule as DataAssignmentRule;
                    return assignRule.DestinationColumnType;
                }
            }

            throw new ApplicationException(string.Format(SharedStrings.DASHBOARD_ERROR_DEFINED_VARIABLE_MISSING, variableName));
        }
        #endregion // Public Methods

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IDashboardRule> GetEnumerator()
        {
            return new DashboardRulesEnumerator(RuleTable);
        }

        public void Dispose() { }

        #endregion

        /// <summary>
        /// Enumerator class for the data filter class
        /// </summary>
        protected class DashboardRulesEnumerator : IEnumerator<IDashboardRule>
        {
            #region Private Members
            private DataTable ruleTable;
            private int currentIndex;
            #endregion Private Members

            public DashboardRulesEnumerator(DataTable table)
            {
                ruleTable = table;
                Reset();
            }

            public void Reset()
            {
                currentIndex = -1;
            }

            public IDashboardRule Current
            {
                get
                {
                    return (IDashboardRule)ruleTable.Rows[currentIndex][COLUMN_RULE];
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
                return currentIndex < ruleTable.Rows.Count;
            }

            public void Dispose() { }
        }
    }
}
