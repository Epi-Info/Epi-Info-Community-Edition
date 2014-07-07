//KKM4 added this comment for testing
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using Epi.Collections;
using Epi.DataSets;
using Epi.Fields;
using VariableCollection = Epi.Collections.NamedObjectCollection<Epi.IVariable>;
using VariableCollectionStack = System.Collections.Generic.Stack<Epi.Collections.NamedObjectCollection<Epi.IVariable>>;

namespace Epi
{
    /// <summary>
    /// Class MemoryRegion
    /// </summary>
    public class MemoryRegion : IMemoryRegion
    {
        private class LocalBlockVariableCollection : VariableCollection
        {

        }

        /// <summary>
        /// syncLock
        /// </summary>
        protected static object syncLock = new object();
        private static bool staticInitalized = false;
        private static VariableCollection systemVariables;
        private static VariableCollection permanentVariables;
        /// <summary>
        /// Global Variables
        /// </summary>
        protected VariableCollection globalVariables;
        /// <summary>
        /// Local Variable Stack
        /// </summary>
        protected Stack<VariableCollection> localVariableStack;
        /// <summary>
        /// Configuration Updated EventHandler
        /// </summary>
        protected EventHandler configurationUpdated;


        /// <summary>
        /// Constructor
        /// </summary>
        public MemoryRegion()
        {
            lock (syncLock)
            {
                if (!staticInitalized)
                {
                    LoadPermanentVariables();
                    //LoadSystemVariables();                    

                    configurationUpdated = new EventHandler(MemoryRegion.ConfigurationUpdated);
                    staticInitalized = true;
                }
            }

            localVariableStack = new Stack<VariableCollection>();
            globalVariables = new VariableCollection();

            PushLocalRegion();

        }

        private static void DeletePermanentVariable(string variableName)
        {

            Configuration config = Configuration.GetNewInstance();
            DataRow[] result = config.PermanentVariables.Select("Name='" + variableName + "'");
            if (result.Length != 1)
            {
                throw new ConfigurationException(ConfigurationException.ConfigurationIssue.ContentsInvalid);
            }
            result[0].Delete();
            Configuration.Save(config);
        }

        public static void UpdatePermanentVariable(IVariable variable)
        {

            Configuration config = Configuration.GetNewInstance();
            DataRow[] result = config.PermanentVariables.Select("Name='" + variable.Name + "'");
            if (result.Length < 1)
            {
                config.PermanentVariables.AddPermanentVariableRow(
                   variable.Name,
                   variable.Expression ?? "",
                    (int)variable.DataType,
                   config.ParentRowPermanentVariables);
            }
            else if (result.Length == 1)
            {
                ((DataSets.Config.PermanentVariableRow)result[0]).DataValue = variable.Expression ?? "";
                ((DataSets.Config.PermanentVariableRow)result[0]).DataType = (int)variable.DataType;
            }
            else
            {
                throw new ConfigurationException(ConfigurationException.ConfigurationIssue.ContentsInvalid, "Duplicate permanent variable rows encountered.");
            }

            Configuration.Save(config);
        }

        /// <summary>
        /// GetVariablesInScope()
        /// </summary>
        /// <param name="scopeCombination">The logically ORed scopes to be included</param>
        /// <returns>VariableCollection</returns>
        public VariableCollection GetVariablesInScope(VariableType scopeCombination)
        {
            VariableCollection shortList = new VariableCollection();
            VariableCollection masterList = GetVariablesInScope();
            foreach (IVariable variable in masterList)
            {
                if ((variable.VarType & scopeCombination) > 0)
                {
                    shortList.Add(variable);
                }
            }
            return shortList;
        }

        /// <summary>
        /// Returns a list of all standard variables.
        /// </summary>
        /// <returns></returns>
        public List<ISetBasedVariable> GetStandardVariables()
        {
            List<ISetBasedVariable> vars = new List<ISetBasedVariable>();
            VariableCollection varsInScope = GetVariablesInScope(VariableType.Standard);
            foreach (IVariable var in varsInScope)
            {
                vars.Add(var as ISetBasedVariable);
            }
            return vars;
        }

        /// <summary>
        /// Returns a list of all data source variables.
        /// </summary>
        /// <returns></returns>
        public List<ISetBasedVariable> GetDataSourceVariables()
        {
            List<ISetBasedVariable> vars = new List<ISetBasedVariable>();
            VariableCollection varsInScope = GetVariablesInScope(VariableType.DataSource);
            foreach (IVariable var in varsInScope)
            {
                vars.Add(var as ISetBasedVariable);
            }
            return vars;
        }

        /// <summary>
        /// Returns a list of all Data source redefined variables.
        /// </summary>
        /// <returns></returns>
        public List<ISetBasedVariable> GetDataSourceRedefinedVariables()
        {
            List<ISetBasedVariable> vars = new List<ISetBasedVariable>();
            VariableCollection varsInScope = GetVariablesInScope(VariableType.DataSourceRedefined);
            foreach (IVariable var in varsInScope)
            {
                vars.Add(var as ISetBasedVariable);
            }
            return vars;
        }

        /// <summary>
        /// Returns a list of all standard variables.
        /// </summary>
        /// <returns></returns>
        public List<IScalarVariable> GetGlobalVariables()
        {
            List<IScalarVariable> vars = new List<IScalarVariable>();
            VariableCollection varsInScope = GetVariablesInScope(VariableType.Global);
            foreach (IVariable var in varsInScope)
            {
                vars.Add(var as IScalarVariable);
            }
            return vars;
        }

        /// <summary>
        /// Returns a list of all standard variables.
        /// </summary>
        /// <returns></returns>
        public List<IScalarVariable> GetPermanentVariables()
        {
            List<IScalarVariable> vars = new List<IScalarVariable>();
            VariableCollection varsInScope = GetVariablesInScope(VariableType.Permanent);
            foreach (IVariable var in varsInScope)
            {
                vars.Add(var as IScalarVariable);
            }
            return vars;
        }

        /// <summary>
        /// GetVariablesInScope()
        /// </summary>
        /// <remarks> Returns all variables, regardless of scope or type</remarks>
        /// <returns>VariableCollection</returns>
        public VariableCollection GetVariablesInScope()
        {
            VariableCollection result;
            lock (syncLock)
            {
                VariableCollection localVariables = new VariableCollection();
                if (localVariableStack.Peek() is LocalBlockVariableCollection)
                {
                    VariableCollection[] locals = new VariableCollection[localVariableStack.Count];
                    localVariableStack.CopyTo(locals, 0);

                    for (int i = 0; i < locals.Length; i++)
                    {
                        localVariables = CombineCollections(localVariables, locals[i]);
                        if (!(locals[i] is LocalBlockVariableCollection))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    localVariables = localVariableStack.Peek();
                }

                result = CombineCollections(systemVariables, permanentVariables, globalVariables, localVariables);
            }
            return result;
        }

        /// <summary>
        /// dcs0 8/9/2007
        /// Gets all the variables as a datatable
        /// </summary>
        /// <returns>Newly constructed DataTable with appropriate columns</returns>
        private DataTable CreateAllVariablesTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(ColumnNames.NAME, typeof(string));
            dt.Columns.Add(ColumnNames.VARIABLE_SCOPE, typeof(Int16));
            dt.Columns.Add(ColumnNames.DATA_TABLE_NAME, typeof(string));
            dt.Columns.Add(ColumnNames.FIELD_TYPE_ID, typeof(Int16));
            dt.Columns.Add(ColumnNames.DATA_TYPE, typeof(Int16));
            dt.Columns.Add(ColumnNames.VARIABLE_VALUE, typeof(string));
            dt.Columns.Add(ColumnNames.ADDITIONAL_INFO, typeof(string));
            dt.Columns.Add(ColumnNames.PROMPT, typeof(string));
            return dt;
        }

        private DataRow AddRow(DataTable table, IVariable var)
        {
            DataRow row = table.NewRow();
            row[ColumnNames.NAME] = var.Name;
            row[ColumnNames.DATA_TYPE] = var.DataType;
            row[ColumnNames.VARIABLE_SCOPE] = var.VarType;
            row[ColumnNames.VARIABLE_VALUE] = var.Expression;
            row[ColumnNames.ADDITIONAL_INFO] = string.Empty;
            row[ColumnNames.PROMPT] = string.Empty;

            // Data source variables have an extra piece of information: Table name
            if (var is IDataSourceVariable)
            {
                IDataSourceVariable dataSourceVar = var as IDataSourceVariable;
                row[ColumnNames.DATA_TABLE_NAME] = dataSourceVar.TableName;

            }

            // Data field variables know the field type.
            if (var is IDataField)
            {
                IDataField dataField = var as IDataField;
                row[ColumnNames.FIELD_TYPE_ID] = dataField.FieldType;
            }

            table.Rows.Add(row);
            return row;
        }

        /// <summary>
        /// GetVariablesAsDataTable()
        /// </summary>
        /// <param name="scopeCombination"></param>
        /// <returns>DataTable</returns>
        public DataTable GetVariablesAsDataTable(VariableType scopeCombination)
        {
            //TODO: HERE
            VariableCollection allVariables = GetVariablesInScope(scopeCombination);

            DataTable variablesDataTable = CreateAllVariablesTable();
            DataRow newRow = null;

            foreach (IVariable variable in allVariables)
            {

                if (variable.IsVarType(scopeCombination))
                {
                    newRow = AddRow(variablesDataTable, variable);
                    if (variable.IsVarType(VariableType.DataSource))
                    {
                        string tableName = ((IDataSourceVariable)variable).TableName;
                        tableName = (tableName == null) ? string.Empty : tableName;
                        newRow[ColumnNames.DATA_TABLE_NAME] = tableName;
                    }
                    else
                    {
                        newRow[ColumnNames.DATA_TABLE_NAME] = SharedStrings.DEFINED_VARIABLE;
                    }
                }
            }

            return variablesDataTable;
        }

        /// <summary>
        /// IsVariableInScope()
        /// </summary>
        /// <param name="varName"></param>
        /// <returns>bool</returns>
        public bool IsVariableInScope(string varName)
        {
            return GetVariablesInScope().Contains(varName);
        }

        /// <summary>
        /// UndefineVariable()
        /// </summary>
        /// <param name="varName"></param>
        public void UndefineVariable(string varName)
        {
            if (localVariableStack.Peek().Contains(varName))
            {
                localVariableStack.Peek().Remove(varName);
            }
            else if (globalVariables.Contains(varName))
            {
                globalVariables.Remove(varName);
            }
            else
            {
                lock (syncLock)
                {
                    if (permanentVariables.Contains(varName))
                    {
                        DeletePermanentVariable(varName);
                        permanentVariables.Remove(varName);
                    }
                    //else if (systemVariables.Contains(varName))
                    //{
                    //    throw new GeneralException(string.Format("System variable '{0}' cannot be undefined.", varName));
                    //}
                    //else
                    //{
                    //    throw new GeneralException(string.Format("Variable '{0}' is not defined.", varName));
                    //}
                }
            }
        }

        /// <summary>
        /// DefineVariable()
        /// </summary>
        /// <param name="variable"></param>
        public void DefineVariable(IVariable variable)
        {
            if (IsVariableInScope(variable.Name))
            {
                //throw new GeneralException(string.Format("Variable '{0}' is already defined.", variable.Name));
            }
            else
            {
                if (variable.VarType == VariableType.Permanent)
                {
                    DefinePermanentVariable(variable);
                    LoadPermanentVariables();
                }
                else if (variable.VarType == VariableType.System)
                {
                    DefineSystemVariable(variable);
                }
                else if (variable.VarType == VariableType.Global)
                {
                    globalVariables.Add(variable);
                }
                else // Standard, DataSource or DataSourceRedefined variables
                {
                    VariableCollection localVariables = localVariableStack.Peek();
                    localVariables.Add(variable);
                }
            }
        }

        private static void DefinePermanentVariable(IVariable variable)
        {
            lock (syncLock)
            {
                UpdatePermanentVariable(variable);
                permanentVariables.Add(variable);
            }
        }

        private static void DefineSystemVariable(IVariable variable)
        {
            lock (syncLock)
            {
                systemVariables.Add(variable);
            }
        }

        /// <summary>
        /// Removes all variables of the given types.
        /// </summary>
        /// <param name="varTypes"></param>
        public void RemoveVariablesInScope(VariableType varTypes)
        {
            VariableCollection vars = GetVariablesInScope(varTypes);
            foreach (IVariable var in vars)
            {
                UndefineVariable(var.Name);
            }
        }

        /// <summary>
        /// Try to get a given variable
        /// </summary>
        /// <param name="varName">The name of the variable</param>
        /// <param name="var">The variable</param>
        /// <returns>bool</returns>
        public bool TryGetVariable(string varName, out IVariable var)
        {
            var = null;
            try
            {
                if (localVariableStack.Peek().Contains(varName))
                {
                    var = localVariableStack.Peek()[varName];
                }
                else if (globalVariables.Contains(varName))
                {
                    var = globalVariables[varName];
                }
                else
                {
                    lock (syncLock)
                    {
                        if (permanentVariables.Contains(varName))
                        {
                            var = permanentVariables[varName];
                        }
                        else /*if (systemVariables.Contains(varName))
                        {
                            var = systemVariables[varName];
                        }
                        else*/
                        {
                            return false;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// Get a given variable
        /// </summary>
        /// <param name="varName">The name of the variable</param>
        /// <returns>IVariable</returns>
        public IVariable GetVariable(string varName)
        {
            IVariable var = null;

            if (localVariableStack.Peek().Contains(varName))
            {
                return localVariableStack.Peek()[varName];
            }
            else if (globalVariables.Contains(varName))
            {
                var = globalVariables[varName];
            }
            else
            {
                lock (syncLock)
                {
                    if (permanentVariables.Contains(varName))
                    {
                        var = permanentVariables[varName];
                    }
                    else if (systemVariables != null && systemVariables.Contains(varName))
                    {
                        var = systemVariables[varName];
                    }
                    else
                    {
                       // throw new GeneralException(string.Format("Variable '{0}' is not defined.", varName));
                    }
                }
            }
            return var;
        }

        /// <summary>
        /// CombineCollections
        /// </summary>
        /// <remarks>
        /// Null parameters are allowed
        /// </remarks>
        /// <param name="collections">Collections</param>
        /// <returns>VariableCollection</returns>
        private static VariableCollection CombineCollections(params VariableCollection[] collections)
        {
            VariableCollection combinedCollection = new VariableCollection();
            foreach (VariableCollection col in collections)
            {
                if (col != null)
                {
                    combinedCollection.Add(col);
                }
            }

            return combinedCollection;
        }

        /// <summary>
        /// Push the Local Region onto the stack
        /// </summary>
        public void PushLocalRegion()
        {
            localVariableStack.Push(new VariableCollection());
        }

        /// <summary>
        /// Push the Local Block Region onto the stack
        /// </summary>
        public void PushLocalBlockRegion()
        {
            localVariableStack.Push(new LocalBlockVariableCollection());
        }

        /// <summary>
        /// PopRegion()
        /// </summary>
        /// <remarks>
        /// Pop the region off the stack
        /// </remarks>
        public void PopRegion()
        {
            if (localVariableStack.Count > 1)
            {
                localVariableStack.Pop();
            }
            else
            {
                throw new GeneralException("Root local variable stack cannot be popped off the local memory stack.");
            }
        }

        private static void ConfigurationUpdated(object sender, EventArgs e)
        {
            LoadPermanentVariables();
        }

        private static void LoadPermanentVariables()
        {
            lock (syncLock)
            {
                MemoryRegion.permanentVariables = new VariableCollection();
                Configuration config = Configuration.GetNewInstance();
                foreach (Config.PermanentVariableRow row in config.PermanentVariables)
                {
                    DefinePermanentVariable(new PermanentVariable(row));
                }
            }
        }

        private static void LoadSystemVariables()
        {
            lock (syncLock)
            {
                MemoryRegion.systemVariables = new VariableCollection();
                foreach (IVariable variable in GetSystemVariables())
                {
                    DefineSystemVariable(variable);
                }
            }
        }

        private static VariableCollection GetSystemVariables()
        {
            VariableCollection list = new VariableCollection();
            string[] names = Enum.GetNames(typeof(SystemVariables));
            foreach (string variableName in names)
            {
                list.Add(GetSystemVariable(variableName));
            }
            return list;
        }

        private static IVariable GetSystemVariable(string variableName)
        {
            IVariable target = null;
            if (Enum.IsDefined(typeof(SystemVariables), variableName))
            {
                SystemVariables targetType = (SystemVariables)Enum.Parse(typeof(SystemVariables), variableName);
                Type type = typeof(MemoryRegion).GetNestedType(variableName, System.Reflection.BindingFlags.NonPublic);
                target = (IVariable)Activator.CreateInstance(type);
            }
            else
            {
                // look up from plugin =)
            }
            return target;
        }

        /*
        #region SYSTEMDATE
        private class SYSTEMDATE : VariableBase, ISystemVariable
        {
            public SYSTEMDATE()
                : base("SYSTEMDATE", DataType.Date, VariableType.System)
            {

            }
            public string Value
            {
                get
                {
                    return DateTime.Now.ToShortDateString();
                }
            }
        }
        #endregion

        #region SYSTEMTIME
        private class SYSTEMTIME : VariableBase, ISystemVariable
        {
            public SYSTEMTIME()
                : base("SYSTEMTIME", DataType.Date, VariableType.System)
            {

            }
            public override string Expression
            {
                get
                {
                    return DateTime.Now.ToShortTimeString();
                }
                set
                {
                    throw new NotSupportedException();
                }
            }
            public string Value
            {
                get
                {
                    return Expression;
                }
            }

        }
        #endregion*/

    }
}
