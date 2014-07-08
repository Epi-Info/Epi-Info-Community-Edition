using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using com.calitha.goldparser;
using Epi;
using Epi.Collections;
using Epi.Data;
using Epi.Data.Services;
using Epi.Fields;
using VariableCollection = Epi.Collections.NamedObjectCollection<Epi.IVariable>;
using Epi.Core.AnalysisInterpreter.Rules;
using EpiInfo.Plugin;

namespace Epi.Core.AnalysisInterpreter
{
    public class Rule_Context
    {
        Dictionary<string, System.Reflection.Assembly> StatisticModuleList;
        Dictionary<string, string> CommandToAssemblyMap;
        Dictionary<string, string> OverriddenSetOptions;
        Dictionary<string, string> StandardVariables = new Dictionary<string, string>();
        Dictionary<string, string> ParticipatingVariableList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        NameValueCollection PermanantVariables = new System.Collections.Specialized.NameValueCollection();
        NameValueCollection GlobalVariables = new System.Collections.Specialized.NameValueCollection();
        List<DataRow> CurrentRowSet;
        IDataReader CurrentDataTableReader;
        IMemoryRegion memoryRegion;

        public IAnalysisInterpreterHost AnalysisInterpreterHost;
        public bool DataTableRefreshNeeded;
        public bool NewSortNeeded = false;
        public System.Collections.Generic.Dictionary<string, IDLLClass> DLLClassList;
        public System.Collections.Generic.Dictionary<string, string> AssignVariableCheck;
        public Dictionary<string, List<string>> GroupVariableList;
        public System.Collections.Generic.Dictionary<string, AnalysisRule> Subroutine;
        public StringBuilder SelectString = new StringBuilder();
        public StringBuilder CurrentSelect = null;
        public Dictionary<string, IDataSource> ConnectionList;
        public string CurrrentConnection = null;
        public Dictionary<string, AnalysisRule> VariableExpressionList;
        public Dictionary<string, object> VariableValueList;
        public bool isCancelRequest = false;
        public AnalysisOutput AnalysisOut;
        public List<Epi.Core.AnalysisInterpreter.AnalysisRule> SelectExpression = new List<Epi.Core.AnalysisInterpreter.AnalysisRule>();
        public StringBuilder SortExpression = new StringBuilder();
        public DataSet DataSet = new System.Data.DataSet();
        public DataRow CurrentDataRow;
        public IAnalysisCheckCode AnalysisCheckCodeInterface;
        public Epi.Project CurrentProject;
        public DataSourceInfo DataInfo = new DataSourceInfo();
        public bool RunOneCommand;
        public bool OneCommandHasRun;
        public bool isReadMode;
        public StringBuilder ProgramText = new StringBuilder();
        public Epi.Core.AnalysisInterpreter.AnalysisRule NextRule;
        public bool IsFirstStatement = false;
        public Epi.Core.AnalysisInterpreter.Rules.Rule_Read CurrentRead;
        public Dictionary<int, System.Collections.DictionaryEntry> RecodeList;
        public Dictionary<string, Epi.Core.AnalysisInterpreter.Rules.RecodeList> Recodes = new Dictionary<string, Epi.Core.AnalysisInterpreter.Rules.RecodeList>();
        public List<String> SelectCommandList = new List<String>();
        public List<String> DefineVarList = new List<String>();
        public void ClearParticipatingVariableList()
        {
            ParticipatingVariableList.Clear();
        }

        public IMemoryRegion MemoryRegion
        {
            get
            {
                if (memoryRegion == null)
                {
                    memoryRegion = new MemoryRegion();
                }
                return memoryRegion;
            }

            set { memoryRegion = value; }
        }

        public Rule_Context()
        {
            Initialize();
        }

        public Rule_Context(IMemoryRegion currentModule)
        {
            Initialize();
            memoryRegion = currentModule;
        }

        public IAnalysisStatistic GetStatistic(string pName, EpiInfo.Plugin.IAnalysisStatisticContext pHost)
        {
            IAnalysisStatistic result = null;

            string StatisticsAssembly = CommandToAssemblyMap[pName];
            System.Reflection.Assembly a = StatisticModuleList[StatisticsAssembly];

            foreach (Type type in a.GetTypes())
            {
                if (type.IsPublic && type.IsClass && type.Name.Equals(pName, StringComparison.OrdinalIgnoreCase))
                {
                    result = (EpiInfo.Plugin.IAnalysisStatistic)Activator.CreateInstance(type, new object[] { pHost });
                    break;
                }
            }
            
            return result;
        }

        private void Initialize()
        {
            GroupVariableList = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            StatisticModuleList = new Dictionary<string, System.Reflection.Assembly>(StringComparer.OrdinalIgnoreCase);
            CommandToAssemblyMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            DLLClassList = new Dictionary<string, IDLLClass>();

            foreach (string s in System.Configuration.ConfigurationSettings.AppSettings)
            {
                switch (s.ToUpper())
                {
                    case "FREQUENCY":
                    case "TABLES":
                    case "SUMMARIZE":
                    case "GRAPH":
                    case "MEANS":
                    case "MATCH":
                    case "LINEARREGRESSION":
                    case "LOGISTICREGRESSION":
                    case "COMPLEXSAMPLEMEANS":
                    case "COMPLEXSAMPLETABLES":
                    case "COMPLEXSAMPLEFREQUENCY":
                    case "KAPLANMEIERSURVIVAL":
                    case "COXPROPORTIONALHAZARDS":
                        string StatisticsAssembly = System.Configuration.ConfigurationSettings.AppSettings[s].ToString();
                        if (! StatisticModuleList.ContainsKey(StatisticsAssembly))
                        {
                            System.Reflection.Assembly a = System.Reflection.Assembly.Load(StatisticsAssembly);
                            StatisticModuleList.Add(StatisticsAssembly, a);
                        }
                        CommandToAssemblyMap.Add(s, StatisticsAssembly);
                        break;
                }
            }
            
            ConnectionList = new Dictionary<string, IDataSource>(StringComparer.OrdinalIgnoreCase);
            DataTableRefreshNeeded = true;
            CurrentSelect = new StringBuilder();
            
            PermanantVariables = new System.Collections.Specialized.NameValueCollection();
            GlobalVariables = new System.Collections.Specialized.NameValueCollection();
            StandardVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            SelectString = new StringBuilder();
            SelectExpression = new List<Epi.Core.AnalysisInterpreter.AnalysisRule>();
            SortExpression = new StringBuilder();
            DataSet = new System.Data.DataSet();
            DataInfo = new DataSourceInfo();
            ProgramText = new StringBuilder();
            IsFirstStatement = false;
            Recodes = new Dictionary<string, Epi.Core.AnalysisInterpreter.Rules.RecodeList>(StringComparer.OrdinalIgnoreCase);
            VariableExpressionList = new Dictionary<string, AnalysisRule>(StringComparer.OrdinalIgnoreCase);
            VariableValueList = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            AnalysisOut = new AnalysisOutput(this);
            OverriddenSetOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);


            SelectCommandList.Add(CommandNames.ABS);
            SelectCommandList.Add(CommandNames.AND);
            SelectCommandList.Add(CommandNames.ALWAYS);
            SelectCommandList.Add(CommandNames.APPEND);
            SelectCommandList.Add(CommandNames.MOD);
            SelectCommandList.Add(CommandNames.LIKE);
            SelectCommandList.Add(CommandNames.OR);
            SelectCommandList.Add(CommandNames.XOR);
            SelectCommandList.Add(CommandNames.NOT);
            SelectCommandList.Add(CommandNames.EXP);
            SelectCommandList.Add(CommandNames.LN);
            SelectCommandList.Add(CommandNames.LOG);
            SelectCommandList.Add(CommandNames.NUMTODATE);
            SelectCommandList.Add(CommandNames.NUMTOTIME);
            SelectCommandList.Add(CommandNames.RECORDCOUNT);
            SelectCommandList.Add(CommandNames.RND);
            SelectCommandList.Add(CommandNames.ROUND);
            SelectCommandList.Add(CommandNames.STEP);
            SelectCommandList.Add(CommandNames.SIN);
            SelectCommandList.Add(CommandNames.COS);
            SelectCommandList.Add(CommandNames.TAN);
            SelectCommandList.Add(CommandNames.TRUNC);
            SelectCommandList.Add(CommandNames.PFROMZ);
            SelectCommandList.Add(CommandNames.ZSCORE);
            SelectCommandList.Add(CommandNames.YEARS);
            SelectCommandList.Add(CommandNames.MONTHS);
            SelectCommandList.Add(CommandNames.DAYS);
            SelectCommandList.Add(CommandNames.YEAR);
            SelectCommandList.Add(CommandNames.MONTH);
            SelectCommandList.Add(CommandNames.DAY);
            SelectCommandList.Add(CommandNames.CURRENTUSER);
            SelectCommandList.Add(CommandNames.EXISTS);
            SelectCommandList.Add(CommandNames.FILEDATE);
            SelectCommandList.Add(CommandNames.SYSTEMDATE);
            SelectCommandList.Add(CommandNames.SYSTEMTIME);
            SelectCommandList.Add(CommandNames.HOURS);
            SelectCommandList.Add(CommandNames.MINUTES);
            SelectCommandList.Add(CommandNames.SECONDS);
            SelectCommandList.Add(CommandNames.HOUR);
            SelectCommandList.Add(CommandNames.MINUTE);
            SelectCommandList.Add(CommandNames.SECOND);
            SelectCommandList.Add(CommandNames.FINDTEXT);
            SelectCommandList.Add(CommandNames.FORMAT);
            SelectCommandList.Add(CommandNames.LINEBREAK);
            SelectCommandList.Add(CommandNames.STRLEN);
            SelectCommandList.Add(CommandNames.SUBSTRING);
            SelectCommandList.Add(CommandNames.TXTTONUM);
            SelectCommandList.Add(CommandNames.TXTTODATE);
            SelectCommandList.Add(CommandNames.UPPERCASE);    
        }
        
        public object GetVariable(string name)
        {
            object result = null;
            result = MemoryRegion.GetVariable(name);
            return result;
        }

        public bool SetVariable(string name, object setValue)
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

        /// <summary>
        /// applys select and returns the Default IDataSource
        /// </summary>
        /// <returns></returns>
        public IDataSource GetDefaultIDataSource()
        {
            return ConnectionList["_DB"];
        }

        /// <summary>
        /// applys select and returns the 'Output' data table
        /// </summary>
        /// <returns></returns>
        public List<DataRow> GetOutput(List<string> pVariableList = null)
        {
            string dataAccessLock = "thread lock for multithreaded scenario";
            lock (dataAccessLock)
            {
                List<DataRow> result = null;

                string pSQL;
                IDataReader newReader;
                System.Data.IDataReader EpiDataReader;

                if(CurrentRead != null)
                {
                    if (DataTableRefreshNeeded && ConnectionList.ContainsKey("_DB"))
                    {
                        System.Data.DataTable Output = new DataTable();

                        if (CurrentRead.IsEpi7ProjectRead && CurrentProject != null)
                        {
                            if (CurrentProject.Views.Exists(CurrentRead.Identifier))
                            {
                                Configuration config = Configuration.GetNewInstance();
                                ApplyOverridenConfigSettings(config);
                                pSQL = "Select * From [" + CurrentProject.Views[CurrentRead.Identifier].TableName + "]";
                                switch (config.Settings.RecordProcessingScope)
                                {
                                    case (int)RecordProcessingScope.Undeleted:
                                        pSQL += " Where RecStatus = 1";
                                        break;
                                    case (int)RecordProcessingScope.Deleted:
                                        pSQL += " Where RecStatus = 0";
                                        break;
                                    case 3:
                                    default:
                                        break;
                                }

                                Output = DBReadExecute.GetDataTable(CurrentRead.File, pSQL);

                                foreach (Page page in CurrentProject.Views[CurrentRead.Identifier].Pages)
                                {
                                    Output = CurrentRead.JoinTables(Output, DBReadExecute.GetDataTable(CurrentRead.File, "Select * From [" + page.TableName + "]"));
                                }
                            }
                            else
                            {
                                pSQL = CreateCurrentSQL();
                                newReader = ConnectionList["_DB"].GetDataTableReader(pSQL);
                                EpiDataReader = new EpiDataReader(this, newReader);
                                Output.Load(EpiDataReader);
                            }
                        }
                        else
                        {
                            pSQL = CreateCurrentSQL();
                            newReader = ConnectionList["_DB"].GetDataTableReader(pSQL);
                            EpiDataReader = new EpiDataReader(this, newReader);
                            Output.Load(EpiDataReader);
                        }

                        Output.TableName = "Output";

                        if (DataSet.Tables.Contains("Output"))
                        {
                            DataSet.Tables.Remove("Output");
                        }

                        DataSet.Tables.Add(Output);
                    }

                    SyncVariableAndOutputTable();

                    if (CurrentRead != null && CurrentRead.IsEpi7ProjectRead && CurrentProject != null && CurrentProject.Views.Exists(CurrentRead.Identifier) && DataTableRefreshNeeded)
                    {
                        View view = CurrentProject.Views[CurrentRead.Identifier];

                        OptionNameProvider OptionNames = new OptionNameProvider(CurrentProject.Metadata, CurrentRead.Identifier);
                        foreach (Epi.Fields.Field field in view.Fields)
                        {
                            if (field is Epi.Fields.OptionField)
                            {
                                string fieldName = ((Epi.INamedObject)field).Name;
                                DataSet.Tables["output"].Columns[fieldName].ColumnName = "__temp__";
                                DataSet.Tables["output"].Columns.Add(new DataColumn(fieldName, typeof(System.String)));

                                for (int i = 0; i < DataSet.Tables["output"].Rows.Count; i++)
                                {
                                    if (DataSet.Tables["output"].Rows[i]["__temp__"] != DBNull.Value)
                                    {
                                        int test;
                                        if (int.TryParse(DataSet.Tables["output"].Rows[i]["__temp__"].ToString(), out test))
                                        {

                                            DataSet.Tables["output"].Rows[i][fieldName] = OptionNames.Name(fieldName, int.Parse(DataSet.Tables["output"].Rows[i]["__temp__"].ToString()));
                                        }
                                        else
                                        {
                                            DataSet.Tables["output"].Rows[i][fieldName] = DataSet.Tables["output"].Rows[i]["__temp__"].ToString();
                                        }
                                    }
                                    else
                                    {
                                        DataSet.Tables["output"].Rows[i][fieldName] = DBNull.Value;
                                    }
                                }

                                DataSet.Tables["output"].Columns.Remove("__temp__");
                            }
                        }
                    }

                    DataTableRefreshNeeded = false;

                }
                else if (!ConnectionList.ContainsKey("_DB") && DataSet.Tables["Output"] == null)
                {
                    System.Data.DataTable Output = new DataTable();
                    Output.TableName = "Output";

                    if (DataSet.Tables.Contains("Output"))
                    {
                        DataSet.Tables.Remove("Output");
                    }

                    DataSet.Tables.Add(Output);
                    SyncVariableAndOutputTable();
                    DataRow row = Output.NewRow();
                    Output.Rows.Add(row);
                }
                else if (DataTableRefreshNeeded && ConnectionList.ContainsKey("_DB") && !string.IsNullOrEmpty(this.CurrentSelect.ToString()))
                {
                    System.Data.DataTable Output = new DataTable();
                    Output.TableName = "Output";

                    if (DataSet.Tables.Contains("Output"))
                    {
                        DataSet.Tables.Remove("Output");
                    }

                    newReader = ConnectionList["_DB"].GetDataTableReader(CurrentSelect.ToString());
                    EpiDataReader = new EpiDataReader(this, newReader);
                    Output.Load(EpiDataReader);

                    DataSet.Tables.Add(Output);
                    SyncVariableAndOutputTable();
                    DataTableRefreshNeeded = false;
                }

                result = new List<DataRow>();

                if (CurrentDataRow == null)
                {
                    DataRow[] SortedDataRows;
                    SortedDataRows = DataSet.Tables["Output"].Select(PrepareSelectExclusionString(pVariableList), this.SortExpression.ToString());

                    for (int i = 0; i < SortedDataRows.Length; i++)
                    {
                        CurrentDataRow = SortedDataRows[i];
                        bool isTrue = true;

                        for (int j = 0; j < SelectExpression.Count; j++)
                        {
                            try
                            {
                                object selected = SelectExpression[j].Execute();
                                if (selected != null)
                                {
                                    if (selected.ToString().Equals("false", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        isTrue = false;
                                        break;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                string exceptionmessage;
                                exceptionmessage = ex.Message;
                                if (ex.Message.Contains("Output"))
                                {
                                    exceptionmessage = ex.Message.Replace("Output", CurrentRead.Identifier);
                                }
                                Exception e = new Exception(exceptionmessage);
                                throw e;
                            }
                        }

                        if (isTrue)
                        {
                            result.Add(SortedDataRows[i]);
                        }
                    }
                    
                    CurrentRowSet = result;
                    CurrentDataRow = null;
                }
                else
                {
                    if (isReadMode)
                    {
                        result = CurrentRowSet;
                    }
                    else
                    {
                        result.AddRange(DataSet.Tables["Output"].Select());
                    }
                }
                return result;
            }
        }

        private string PrepareSelectExclusionString(List<string> pVariableList)
        {
            StringBuilder result = new StringBuilder();
            Configuration config = Configuration.GetNewInstance();
            ApplyOverridenConfigSettings(config);

            if (!config.Settings.IncludeMissingValues)
            {
                foreach (KeyValuePair<string, string> kvp in ParticipatingVariableList)
                {
                    result.Append("[");
                    result.Append(kvp.Key);
                    result.Append("]");
                    result.Append(" NOT is Null AND ");
                }

                if (result.Length > 0)
                {
                    result.Length = result.Length - 4;
                }

                if (pVariableList != null)
                {
                    foreach (string s in pVariableList)
                    {
                        if (DataSet.Tables["Output"].Columns.Contains(s))
                        {
                            result.Append("[");
                            result.Append(s);
                            result.Append("] NOT is Null AND ");
                        }
                    }

                    if (result.Length > 0)
                    {
                        result.Length = result.Length - 4;
                    }
                }
            }

            return result.ToString();
        }

        private string CreateCurrentSQL()
        {
            if (CurrentRead != null && isReadMode)
            {
                CurrentSelect.Length = 0;
                string[] Identifiers = CurrentRead.Identifier.Split('.');
                StringBuilder Identifier = new StringBuilder();

                for (int i = 0; i < Identifiers.Length; i++)
                {
                    Identifier.Append("[");
                    Identifier.Append(Identifiers[i]);
                    Identifier.Append("].");
                }
                
                Identifier.Length = Identifier.Length - 1;

                if (CurrentRead.IsEpi7ProjectRead && CurrentProject != null)
                {
                    
                    if (CurrentProject.Views.Exists(CurrentRead.Identifier))
                    {
                        View view = CurrentProject.GetViewByName(CurrentRead.Identifier);

                        CurrentSelect.Append(Rule_Read.GetEpi7ProjectViewSQL(view));
                        Configuration config = Configuration.GetNewInstance();
                        switch (config.Settings.RecordProcessingScope)
                        {
                            case (int)RecordProcessingScope.Undeleted: 
                                CurrentSelect.Append(" Where RecStatus = 1");
                                break;
                            case (int)RecordProcessingScope.Deleted: 
                                CurrentSelect.Append(" Where RecStatus = 0");
                                break;
                            case 3: 
                            default:
                                break;
                        }
                    }
                    else
                    {
                        CurrentSelect.Append("Select * ");
                        CurrentSelect.Append(" FROM ");
                        CurrentSelect.Append(Identifier.ToString());
                    }
                }
                else
                {
                    CurrentSelect.Append("Select * From ");
                    CurrentSelect.Append(Identifier.ToString());
                }
            }

            return CurrentSelect.ToString();
        }

        private string ConvertToSQL(string pValue)
        {
            string result = pValue.Replace("\"", "\'").Replace("(.)", "NULL").Replace("(+)", "true").Replace("(-)", "false");
            return result;
        }

        public System.Data.IDataReader GetCurrentDataTableReader()
        {
            GetOutput();
            CurrentDataTableReader = DataSet.Tables["output"].CreateDataReader();
            return CurrentDataTableReader;
        }

        public List<DataRow> GetOutput(MapDataDelegate mapDataDelegate)
        {
            List<DataRow> result = null;
            int affectedCount = 0;

            if (CurrentDataRow == null)
            {
                result = GetOutput();

                for (int i = result.Count - 1; i > -1; i--)
                {
                    CurrentDataRow = result[i];
                    mapDataDelegate();
                    affectedCount++;
                }

                CurrentDataRow = null;
            }
            else
            {
                mapDataDelegate();
            }
            return result;
        }

        public List<DataRow> GetOutput(Epi.Core.AnalysisInterpreter.AnalysisRule ifClause, MapDataDelegate trueDelegate, MapDataDelegate falseDelegate)
        {
            List<DataRow> result = GetOutput();
            int affectedCount = 0;

            if (DataSet.Tables["Output"] == null)
            {
                SyncVariableAndOutputTable();
            }

            if (CurrentDataRow == null)
            {
                List<System.Data.DataRow> rows = GetOutput();

                foreach (DataRow row in rows)
                {
                    CurrentDataRow = row;

                    if (ifClause.Execute().ToString().Equals("true", StringComparison.CurrentCultureIgnoreCase))
                    {
                        trueDelegate();
                        affectedCount++;
                    }
                    else
                    {
                        falseDelegate();
                        affectedCount++;
                    }
                }

                CurrentDataRow = null;
            }
            else
            {
                if (ifClause.Execute().ToString().Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    trueDelegate();
                    affectedCount++;
                }
                else
                {
                    falseDelegate();
                    affectedCount++;
                }
            }
            return result;
        }

        public void Reset()
        {
            SelectExpression.Clear();
            SelectString.Length = 0;

            if (DataInfo != null)
            {
                DataInfo.SqlStatementPartWhere = String.Empty;
            }
            
            SortExpression.Length = 0;
            RunOneCommand = false;
            OneCommandHasRun = false;
            CurrentDataRow = null;
        }

        /// <summary>
        /// Reset the Current Datarow to null while check commands are selected from the program window, Issue 937
        /// </summary>
        public void ResetWhileSelected()
        {
            CurrentDataRow = null;
        }

        public void SyncVariableAndOutputTable()
        {
            if (!DataSet.Tables.Contains("Output"))
            {
                DataSet.Tables.Add("Output");
            }
            
            DataTable outputTable = DataSet.Tables["Output"];
            DataTable variableTable;

            if (!DataSet.Tables.Contains("variables"))
            {
                DataSet.Tables.Add(new DataTable("variables"));
            }

            variableTable = DataSet.Tables["variables"];

            foreach (DataColumn column in variableTable.Columns)
            {
                DataColumn newColumn = new DataColumn(column.ColumnName);
                newColumn.DataType = column.DataType;
                if (!outputTable.Columns.Contains(column.ColumnName))
                {
                    outputTable.Columns.Add(newColumn);
                }
            }

            if (memoryRegion != null)
            { 
                foreach (IVariable var in memoryRegion.GetVariablesInScope(VariableType.Global | VariableType.Permanent))
                {
                    DataColumn newColumn = new DataColumn(var.Name);
                    newColumn.DataType = typeof(string);
                    newColumn.DefaultValue = var.Expression;
                    if (!outputTable.Columns.Contains(var.Name))
                    {
                        outputTable.Columns.Add(newColumn);
                    }
                }
            }

            if (CurrentRead != null && CurrentRead.IsEpi7ProjectRead && CurrentProject != null && CurrentProject.Views.Exists(CurrentRead.Identifier))
            {
                View view = CurrentProject.Views[CurrentRead.Identifier];

                foreach (IField field in view.Fields)
                {
                    if (field is GroupField)
                    {
                        GroupField groupField = (GroupField)field;

                        if (GroupVariableList.ContainsKey(groupField.Name))
                        {
                            GroupVariableList[groupField.Name].Clear();
                        }
                        else
                        {
                            GroupVariableList.Add(groupField.Name, new List<string>());
                        }

                        foreach (string fieldName in groupField.ChildFieldNameArray)
                        {
                            if (view.Fields.Contains(fieldName))
                            {
                                if (view.Fields[fieldName].FieldType != MetaFieldType.LabelTitle)
                                {
                                    GroupVariableList[groupField.Name].Add(fieldName);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ReadDataSource(DataTable table)
        {
            if (table != null)
            {
                IMemoryRegion region = MemoryRegion;
                FieldCollectionMaster dataFields = new FieldCollectionMaster();
                Project tempProject = new Project();
                View tempView = new View(tempProject);
                Page tempPage = new Page(tempView);

                DataInfo.PrimaryTable = table;

                List<IField> listOfFields = new List<IField>();

                foreach (DataColumn column in table.Columns)
                {
                    IField field = (IField)tempPage.CreateField(MetaFieldType.Text);
                    field.Name = column.ColumnName;
                    RenderableField tempField = (RenderableField)field;
                    tempField.PromptText = column.ColumnName;
                    tempField.SourceTable = table.TableName;
                    listOfFields.Add(tempField);
                }

                foreach (IField field in listOfFields)
                {
                    region.DefineVariable((IDataField)field);
                }

                if (CurrentRead != null && CurrentRead.IsEpi7ProjectRead && CurrentProject != null && CurrentProject.Views.Exists(CurrentRead.Identifier))
                {
                    View view = CurrentProject.Views[CurrentRead.Identifier];

                    foreach (IField field in view.Fields)
                    {
                        if (field is GroupField)
                        {
                            IVariable v = new DataSourceVariable(field.Name, DataType.Unknown);
                            region.DefineVariable(v);
                        }
                    }
                }

                foreach (KeyValuePair<string, List<string>> kvp in GroupVariableList)
                {
                    IVariable v = new DataSourceVariable(kvp.Key, DataType.Unknown);
                    region.DefineVariable(v);
                }
            }
        }

        /// <summary>
        /// Clears the session state
        /// </summary>
        public void ClearState()
        {
            MemoryRegion.RemoveVariablesInScope(VariableType.Standard);

            if (DataInfo != null)
            {
                DataInfo.Dispose();
            }

            OverriddenSetOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            DataInfo = new DataSourceInfo();
            Recodes.Clear();
            Reset();
            CurrentRead = null;
            IsFirstStatement = false;
            ProgramText.Length = 0;
            DataTableRefreshNeeded = true;
            AnalysisOut = new AnalysisOutput(this);
            isCancelRequest = false;
            GroupVariableList = new Dictionary<string, List<string>>();
        }

        public void SetOneCommandMode()
        {
            RunOneCommand = false;
            OneCommandHasRun = false;
            IsFirstStatement = false;
            ProgramText.Length = 0;
        }

        public void SetOutputTable(DataTable dataTable)
        {
            if (DataSet != null && CurrentRead != null)
            {
                if (DataSet.Tables.Contains("Output"))
                {
                    DataSet.Tables.Remove("Output");
                }
                dataTable.TableName = "Output";
                DataSet.Tables.Add(dataTable);
            }
            else
            {
                throw new ApplicationException("No table has been read.");
            }
        }

        public Dictionary<string, string> GetGlobalSettingProperties(Configuration config = null)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (config == null)
            {
                config = Configuration.GetNewInstance();
            }

            ApplyOverridenConfigSettings(config);
            
            result.Add("RepresentationOfMissing", config.Settings.RepresentationOfMissing);
            result.Add("RepresentationOfNo", config.Settings.RepresentationOfNo);
            result.Add("RepresentationOfYes", config.Settings.RepresentationOfYes);
            result.Add("STATISTICS",config.Settings.StatisticsLevel.ToString());
            result.Add("RECORD-PROCESSING-SCOPE",config.Settings.RecordProcessingScope.ToString());
            result.Add("DELETED-RECORD-INCLUSION-LEVEL", config.Settings.RecordProcessingScope.ToString());
            result.Add("ShOW-PERCENTS",config.Settings.ShowPercents.ToString());
            result.Add("INCLUDE-MISSING",config.Settings.IncludeMissingValues.ToString());
            result.Add("SHOW-SELECT",config.Settings.ShowSelection.ToString());
            result.Add("SHOW-FREQGRAPH",config.Settings.ShowGraphics.ToString());
            result.Add("SHOW-HYPERLINKS",config.Settings.ShowHyperlinks.ToString());
            result.Add("SHOW-PROMPTS", config.Settings.ShowCompletePrompt.ToString());
            result.Add("SHOW-TABLES", config.Settings.ShowTables.ToString());

            return result;
        }

         public void ApplyOverridenConfigSettings(Configuration config)
        {
            foreach (KeyValuePair<string, string> kvp in OverriddenSetOptions)
            {
                switch (kvp.Key.ToUpper())
                {
                    
                    case "STATISTICS":// '=' <StatisticsOption>	!These options could be set in FREQ,MATCH, and MEANS commands also
                        /*<StatisticsOption>	::= NONE
                           | MINIMAL
                           | INTERMEDIATE
                           | COMPLETE*/
                        switch(kvp.Value.ToUpper())
                        {
                            case "NONE":
                                config.Settings.StatisticsLevel = 0;
                                break;
                            case "MINIMAL":
                                config.Settings.StatisticsLevel = 1;
                                break;
                            case "COMPLETE":
                                config.Settings.StatisticsLevel = 3;
                                break;
                            case "INTERMEDIATE":
                            default:
                                config.Settings.StatisticsLevel = 2;
                                break;
                        }
                        break;
                    case "PROCESS":// '=' <ProcessOption>
                        /*<ProcessOption> ::= UNDELETED
                        | DELETED
                        | BOTH*/
                        switch(kvp.Value.ToUpper())
                        {
                            case "UNDELETED":
                            case "NORMAL": // deleted records are NOT incuded
                                config.Settings.RecordProcessingScope = 1;
                                break;

                            case "DELETED": // only deleted records are included
                                config.Settings.RecordProcessingScope = 2;
                                break;
                             case "BOTH": // Deleted records are included
                            default:
                                config.Settings.RecordProcessingScope = 3;
                                break;
                        }
                        break;
                    case "DELETED":// '=' <DeletedOption>
                        switch(kvp.Value.ToUpper())
                        {
                            case "ONLY": 
                                config.Settings.RecordProcessingScope = 1;
                                break;
                            case "YES": 
                            case "(+)":
                                config.Settings.RecordProcessingScope = 2;
                                break;
                            case "NO": 
                            case "(-)":
                            default:
                                config.Settings.RecordProcessingScope = 0;
                                break;
                        }
                        break;

                    //case "PROCESS":// '=' Identifier
                    case "Boolean":// '=' String
                        //config.Settings = true;
                        break;
                    case "(+)":// '=' String
                        config.Settings.RepresentationOfYes = kvp.Value;
                        break;
                    case "(-)":// '=' String
                        config.Settings.RepresentationOfNo = kvp.Value;
                        break;
                    case "(.)":// '=' String
                        config.Settings.RepresentationOfMissing = kvp.Value;
                        break;
                    case "YN"://	'=' String ',' String ',' String
                        string[] yn = kvp.Value.Split(',');
                        if (yn.Length > 0)
                        {
                            config.Settings.RepresentationOfYes = yn[0];
                        }

                        if (yn.Length > 1)
                        {
                            config.Settings.RepresentationOfNo = yn[1];
                        }

                        if (yn.Length > 2)
                        {
                            config.Settings.RepresentationOfMissing = yn[2];
                        }
                        break;
                    case "PERCENTS":// '=' <OnOff>
                        switch (kvp.Value.ToUpper())
                        {
                            // <OnOff> ::= ON | OFF  | Boolean
                            case "ON":
                            case"TRUE":
                            case "(+)":
                                config.Settings.ShowPercents = true;
                                break;
                            case "OFF":
                            case "FALSE":
                            case "(-)":
                                config.Settings.ShowPercents = false;
                                break;
                        }
                        break;
                    case "MISSING":// '=' <OnOff>
                        switch (kvp.Value.ToUpper())
                        {
                            // <OnOff> ::= ON | OFF  | Boolean
                            case "ON":
                            case"TRUE":
                            case "(+)":
                                config.Settings.IncludeMissingValues = true;
                                break;
                            case "OFF":
                            case "FALSE":
                            case "(-)":
                                config.Settings.IncludeMissingValues = false;
                                break;
                        }
                        break;

                    case "IGNORE":// '=' <OnOff>
                        switch (kvp.Value.ToUpper())
                        {
                            // <OnOff> ::= ON | OFF  | Boolean
                            case "ON":
                            case "TRUE":
                            case "(+)":
                                config.Settings.IncludeMissingValues = false;
                                break;
                            case "OFF":
                            case "FALSE":
                            case "(-)":
                                config.Settings.IncludeMissingValues = true;
                                break;
                        }
                        break;

                    case "SELECT":// '=' <OnOff>
                        switch (kvp.Value.ToUpper())
                        {
                            // <OnOff> ::= ON | OFF  | Boolean
                            case "ON":
                            case"TRUE":
                            case "(+)":
                                config.Settings.ShowSelection = true;
                                break;
                            case "OFF":
                            case "FALSE":
                            case "(-)":
                                config.Settings.ShowSelection = false;
                                break;
                        }
                        break;

                    case "FREQGRAPH":// '=' <OnOff>
                        switch (kvp.Value.ToUpper())
                        {
                            // <OnOff> ::= ON | OFF  | Boolean
                            case "ON":
                            case"TRUE":
                            case "(+)":
                                config.Settings.ShowGraphics = true;
                                break;
                            case "OFF":
                            case "FALSE":
                            case "(-)":
                                config.Settings.ShowGraphics = false;
                                break;
                        }
                        break;

                    case "HYPERLINKS":// '=' <OnOff>
                        switch (kvp.Value.ToUpper())
                        {
                            // <OnOff> ::= ON | OFF  | Boolean
                            case "ON":
                            case"TRUE":
                            case "(+)":
                                config.Settings.ShowHyperlinks = true;
                                break;
                            case "OFF":
                            case "FALSE":
                            case "(-)":
                                config.Settings.ShowHyperlinks = false;
                                break;
                        }
                        break;

                    case "SHOWPROMPTS":// '=' <OnOff>
                        switch (kvp.Value.ToUpper())
                        {
                            // <OnOff> ::= ON | OFF  | Boolean
                            case "ON":
                            case"TRUE":
                            case "(+)":
                                config.Settings.ShowCompletePrompt = true;
                                break;
                            case "OFF":
                            case "FALSE":
                            case "(-)":
                                config.Settings.ShowCompletePrompt = false;
                                break;
                        }
                        break;

                    case "TABLES":// '=' <OnOff>
                        switch (kvp.Value.ToUpper())
                        {
                            // <OnOff> ::= ON | OFF  | Boolean
                            case "ON":
                            case"TRUE":
                            case "(+)":
                                config.Settings.ShowTables = true;
                                break;
                            case "OFF":
                            case "FALSE":
                            case "(-)":
                                config.Settings.ShowTables = false;
                                break;
                        }
                        break;

                    case "USEBROWSER":// '=' <OnOff>*/
                        /*
                        switch (kvp.Value.ToUpper())
                        {
                            // <OnOff> ::= ON | OFF  | Boolean
                            case "ON":
                            case"TRUE":
                                config.Settings. = true;
                                break;
                            case "OFF":
                            case "FALSE":
                                config.Settings. = false;
                                break;
                        }*/
                        break;
                }
            }
            Configuration.Save(config);
        }

        public void AddConfigSettings(Dictionary<string, string> pSetOptions)
        {
            foreach (KeyValuePair<string, string> kvp in pSetOptions)
            {
                if (OverriddenSetOptions.ContainsKey(kvp.Key))
                {
                    OverriddenSetOptions[kvp.Key] = kvp.Value;
                }
                else
                {
                    OverriddenSetOptions.Add(kvp.Key, kvp.Value);
                }
            }          
        }

        public void AddConnection(string identifier, string pathOrConnectionString)
        {
            object driver = DBReadExecute.GetDataDriver(pathOrConnectionString);
            IDataSource dataSource = (IDataSource)driver;

            if (ConnectionList.ContainsKey(identifier))
            {
                ConnectionList.Remove(identifier);
            }

            ConnectionList.Add(identifier, dataSource);
        }

        public void ExpandGroupVariables(List<string> identifierList, ref bool isExceptionList)
        {
            List<string> result = new List<string>();

            for (int i = 0; i < identifierList.Count; i++)
            {
                identifierList[i] = identifierList[i].ToUpper();
            }

            if (identifierList.Count == 1 && identifierList[0] == "*")
            {
                identifierList.Clear();

                foreach (DataColumn column in DataSet.Tables["Output"].Columns)
                {
                    identifierList.Add(column.ColumnName.ToUpper());
                }
            }

            if (isExceptionList)
            {
                List<string> List2 = new List<string>();

                foreach (DataColumn column in DataSet.Tables["Output"].Columns)
                {
                    List2.Add(column.ColumnName.ToUpper());
                }

                foreach (string identifier in identifierList)
                {
                    if (GroupVariableList.ContainsKey(identifier))
                    {
                        List<string> GroupVarList = GroupVariableList[identifier];
                        foreach (string Variable in GroupVarList)
                        {
                            if (List2.Contains(Variable.ToUpper()))
                            {
                                List2.Remove(Variable.ToUpper());
                            }
                        }
                    }
                    else
                    {
                        if (List2.Contains(identifier.ToUpper()))
                        {

                            List2.Remove(identifier.ToUpper());
                        }
                    }
                }
                identifierList.Clear();

                foreach (string s in List2)
                {
                    identifierList.Add(s);
                }

                isExceptionList = false;
            }
            else
            {
                foreach (KeyValuePair<string, List<string>> GroupVarList in GroupVariableList)
                {
                    if (identifierList.Contains(GroupVarList.Key.ToUpper()))
                    {
                        identifierList.Remove(GroupVarList.Key.ToUpper());

                        foreach (string Variable in GroupVarList.Value)
                        {
                            if (!identifierList.Contains(Variable.ToUpper()))
                            {
                                identifierList.Add(Variable.ToUpper());
                            }
                        }
                    }
                }
            }
        }
    }
}
