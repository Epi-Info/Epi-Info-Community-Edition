using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using com.calitha.goldparser;

namespace EpiMenu.CommandPlugin
{
    public class Rule_Context
    {

        public enum eRunMode
        {
            Enter,
            Analysis,
            Menu
        }

        public eRunMode RunMode;

        public System.Collections.Specialized.NameValueCollection PermanantVariables = new System.Collections.Specialized.NameValueCollection();
        public System.Collections.Specialized.NameValueCollection GlobalVariables = new System.Collections.Specialized.NameValueCollection();
        public System.Collections.Generic.Dictionary<string, string> StandardVariables = new Dictionary<string, string>();

        public StringBuilder SelectString = new StringBuilder();

        
        public StringBuilder SortExpression = new StringBuilder();

        public System.Data.DataSet DataSet = new System.Data.DataSet();
        public System.Data.DataRow CurrentDataRow;

        public bool RunOneCommand;
        public bool OneCommandHasRun;
        public StringBuilder ProgramText = new StringBuilder();

        public bool IsFirstStatement = false;
        public System.Collections.Generic.Dictionary<int, System.Collections.DictionaryEntry> RecodeList;


        public Rule_Context()
        {
            this.Initialize();
        }

        /*
        public Rule_Context(IMemoryRegion currentModule)
        {
            this.Initialize();
        }*/

        private void Initialize()
        {
            this.RunMode = Rule_Context.eRunMode.Enter;
            this.PermanantVariables = new System.Collections.Specialized.NameValueCollection();
            this.GlobalVariables = new System.Collections.Specialized.NameValueCollection();
            this.StandardVariables = new Dictionary<string, string>();
            this.SelectString = new StringBuilder();

            this.SortExpression = new StringBuilder();
            this.DataSet = new System.Data.DataSet();
            this.ProgramText = new StringBuilder();
            this.IsFirstStatement = false;

        }


        public object GetVariable(string name)
        {
            object result = null;
            //result = StandardVariables[name];
            //result = MemoryRegion.GetVariable(name);
            return result;
        }

        public bool SetVariable(string name, object setValue)//, Epi.VariableType pType)
        {
            bool result = false;
            string value = setValue.ToString();
            if (StandardVariables.ContainsKey(name))
            {
                StandardVariables[name] = value;
            }
            else
            {
                StandardVariables.Add(name, setValue.ToString());
            }

            return result;
        }

        public delegate void MapDataDelegate();


        public void Reset()
        {
            this.SortExpression.Length = 0;
            RunOneCommand = false;
            OneCommandHasRun = false;
            this.CurrentDataRow = null;
        }

        public void SyncVariableAndOutputTable()
        {
            if (!this.DataSet.Tables.Contains("Output"))
            {
                this.DataSet.Tables.Add("Output");
            }
            DataTable outputTable = this.DataSet.Tables["Output"];
            DataTable variableTable;

            if (!this.DataSet.Tables.Contains("variables"))
            {
                this.DataSet.Tables.Add(new DataTable("variables"));
            }

            variableTable = this.DataSet.Tables["variables"];

            // will only add current variables to the output table
            foreach (DataColumn column in variableTable.Columns)
            {
                DataColumn newColumn = new DataColumn(column.ColumnName);
                newColumn.DataType = column.DataType;
                if (!outputTable.Columns.Contains(column.ColumnName))
                {
                    outputTable.Columns.Add(newColumn);
                }
            }
        }


        /// <summary>
        /// Clears the session state
        /// </summary>
        public void ClearState()
        {
            //this.MemoryRegion.RemoveVariablesInScope(VariableType.Standard);

            //this.MemoryRegion.RemoveVariablesInScope(VariableType.DataSource);
            //this.MemoryRegion.RemoveVariablesInScope(VariableType.DataSourceRedefined);

            //VariableCollection vars = module.Processor.GetVariablesInScope(VariableType.DataSource | VariableType.Standard | VariableType.DataSourceRedefined);
            //foreach (IVariable var in vars)
            //{
            //    module.Processor.UndefineVariable(var.Name);
            //}

            // Remove the current DataSourceInfo object and create a new one.
            this.IsFirstStatement = false;
            this.ProgramText.Length = 0;
        }

        public void SetOneCommandMode()
        {
            RunOneCommand = true;
            OneCommandHasRun = false;
            //OneStatement = null;
            this.IsFirstStatement = false;
            this.ProgramText.Length = 0;
        }




    }
}
