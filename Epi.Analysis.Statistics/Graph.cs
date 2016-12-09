using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using EpiInfo.Plugin;
using Epi.Data;


namespace Epi.Analysis.Statistics
{
    public class Graph : IAnalysisStatistic
    {
        IAnalysisStatisticContext Context;
        public StatisticsRepository.cTable.SingleTableResults singleTableResults;
        string _commandText = string.Empty;
        string _independentVariableList = string.Empty;
        string[] _independentVariableArray = null;
        string _graphType = string.Empty;

        string _graphCrossTab = string.Empty;
        List<string> _graphCrossTabList = new List<string>();

        string _strataVar = string.Empty;
        string _graphTitle = string.Empty;
        string _graphDateFormat = string.Empty;
        string _graphIndependentAxisLabel = string.Empty;
        string _graphDependentAxisLabel = string.Empty;
        string _weightVar = string.Empty;
        string _aggregateFunction = string.Empty;

        string _graphInterval = string.Empty;
        string _graphIntervalUnits = string.Empty;
        DateTime _graphStartFrom = DateTime.MinValue;

        bool _independentValueTypesSame = true; 
        bool _independentValuesAllBool = true;        
        
        DataTable _regressionTable = new DataTable();

        public Graph(IAnalysisStatisticContext AnalysisStatisticContext)
        {
            Construct(AnalysisStatisticContext);
        }

        public string Name { get { return "Epi.Analysis.Statistics.Graph"; } }

        public void Construct(IAnalysisStatisticContext AnalysisStatisticContext)
        {
            Context = AnalysisStatisticContext;

            if (Context.InputVariableList.ContainsKey("Command_Text"))
            {
                _commandText = Context.InputVariableList["Command_Text"];
            }
            if (Context.InputVariableList.ContainsKey("Independent_Variable_List"))
            {
                _independentVariableList = Context.InputVariableList["Independent_Variable_List"];
                _independentVariableArray = Context.InputVariableList["Independent_Variable_List"].Split(',');
            }
            if (Context.InputVariableList.ContainsKey("Cross_Tabulation_Variable"))
            {
                _graphCrossTab = Context.InputVariableList["Cross_Tabulation_Variable"];
            }
            if (this.Context.InputVariableList.ContainsKey("StrataVar"))
            {
                _strataVar = this.Context.InputVariableList["StrataVar"];
            }
            if (Context.InputVariableList.ContainsKey("Graph_Type"))
            {
                _graphType = Context.InputVariableList["Graph_Type"].ToUpperInvariant().Replace(" ", "").Replace("\"", "");
            }
            if (Context.InputVariableList.ContainsKey("Graph_Title"))
            {
                _graphTitle = Context.InputVariableList["Graph_Title"];
            }
            if (Context.InputVariableList.ContainsKey("Graph_Independent_Value_Axis_Label"))
            {
                _graphIndependentAxisLabel = Context.InputVariableList["Graph_Independent_Value_Axis_Label"];
            }
            if (Context.InputVariableList.ContainsKey("Graph_Dependent_Value_Axis_Label"))
            {
                _graphDependentAxisLabel = Context.InputVariableList["Graph_Dependent_Value_Axis_Label"];
            }
            if (Context.InputVariableList.ContainsKey("Weight_Variable"))
            {
                _weightVar = Context.InputVariableList["Weight_Variable"];
            }
            if (Context.InputVariableList.ContainsKey("Aggregate_Function"))
            {
                _aggregateFunction = Context.InputVariableList["Aggregate_Function"];
            }
            if (Context.InputVariableList.ContainsKey("Date_Format"))
            {
                _graphDateFormat = Context.InputVariableList["Date_Format"];
            }
            if (Context.InputVariableList.ContainsKey("Interval"))
            {
                _graphInterval = Context.InputVariableList["Interval"];
            }
            if (Context.InputVariableList.ContainsKey("Interval_Units"))
            {
                _graphIntervalUnits = Context.InputVariableList["Interval_Units"];
            }
            if (Context.InputVariableList.ContainsKey("Start_From"))
            {
                _graphStartFrom = DateTime.MinValue;
                string startFromDate = Context.InputVariableList["START_FROM"].Trim(new char[]{'"'});
                DateTime.TryParse(startFromDate, out _graphStartFrom);
            }
        }

        public void Execute()
        {
            _regressionTable = new DataTable();
            _regressionTable.Columns.Add("SeriesName", typeof(string));
            _regressionTable.Columns.Add("Predictor", typeof(object));
            _regressionTable.Columns.Add("Response", typeof(double));
            
            Dictionary<string, string> config = Context.SetProperties;
            StringBuilder HTMLString = new StringBuilder();

            System.Data.DataTable sourceTable = BuildTempContextTable();
            cDataSetHelper dsHelper = new cDataSetHelper();

            Dictionary<string, string> args = new Dictionary<string, string>();

            string strataVarValue = string.Empty;
            string objectElement = string.Empty;

            SilverlightMarkup silverlight = new SilverlightMarkup();

            DataTable Strata_ValueList;

            if (string.IsNullOrEmpty(_strataVar))
            {
                Strata_ValueList = sourceTable.Clone();
                Strata_ValueList.Rows.Add(Strata_ValueList.NewRow());
            }
            else
            {
                Strata_ValueList = dsHelper.SelectDistinct("", sourceTable, _strataVar);
            }

            FilterStruct filter = new FilterStruct();
            filter.strataVarName = _strataVar;
            
            _independentValueTypesSame = true;
            if(_independentVariableArray.Length > 1)
            {
                for(int i = 0; (i + 1) < _independentVariableArray.Length ; i++)
                {
                    if (sourceTable.Columns[_independentVariableArray[i]].DataType != sourceTable.Columns[_independentVariableArray[i+1]].DataType )
                    {
                        _independentValueTypesSame = false;
                        break;
                    }
                }
            }

            _independentValuesAllBool = true;
            for (int i = 0; i < _independentVariableArray.Length; i++)
            {
                string indepVar = _independentVariableArray[i];

                if (sourceTable.Columns.Contains(indepVar))
                {
                    if (sourceTable.Columns[indepVar].DataType.Name != "Byte")
                    {
                        _independentValuesAllBool = false;
                        break;
                    }
                }
                else
                {
                    return;
                }
            }

            foreach (DataRow strataRow in Strata_ValueList.Rows)
            {
                if (string.IsNullOrEmpty(_strataVar) || strataRow[_strataVar] == DBNull.Value)
                {
                    filter.strataVarValue = "null";
                }
                else
                {
                    filter.strataVarValue = strataRow[_strataVar].ToString();
                }

                if (_graphType == SilverlightStatics.Scatter)
                {
                    if (_independentVariableArray.Length < 2)
                    {
                        throw new Exception("Scatter graphs must contain two (2) main variables.");
                    }
                    
                    string categoryName = string.Empty;
                    Dictionary<object, double> indDepValuePairCollection = new Dictionary<object, double>();

                    List<string> StrataVarNameList = new List<string>();
                    List<string> StrataVarValueList = new List<string>();

                    string regressor = _independentVariableArray[0];
                    string regressand = _independentVariableArray[1];

                    StrataVarNameList.Add(filter.strataVarName);
                    StrataVarValueList.Add(filter.strataVarValue);

                    DataTable table = new DataTable();
                    table.Columns.Add(new DataColumn(sourceTable.Columns[regressor].ColumnName, sourceTable.Columns[regressor].DataType));
                    table.Columns.Add(new DataColumn(sourceTable.Columns[regressand].ColumnName, sourceTable.Columns[regressand].DataType));

                    foreach (DataRow row in sourceTable.Rows)
                    {
                        table.Rows.Add(row[regressor], row[regressand]);
                    }

                    foreach (DataRow row in table.Rows)
                    {
                        if (row[regressor] != DBNull.Value)
                        {
                            string seriesName = string.Empty;
                            object independentValue = new object();

                            if (_independentValuesAllBool)
                            {
                                independentValue = regressor;
                                byte value = (byte)(row[regressor]);
                            }
                            else
                            {
                                independentValue = GetValue(regressor, row, config);
                            }

                            seriesName = string.Format("{0} x {1}", sourceTable.Columns[regressor].ColumnName, sourceTable.Columns[regressand].ColumnName);

                            if (string.IsNullOrEmpty(_weightVar))
                            {
                                double dependentVal;

                                if (double.TryParse(row[regressand].ToString(), out dependentVal))
                                {
                                    _regressionTable.Rows.Add(seriesName, independentValue, dependentVal);
                                }
                            }
                            else
                            {
                                filter.independentVarValue = row[regressor];
                                filter.independentVarName = regressor;
                                filter.weightVarName = _weightVar;
                                double dependentVal = GetAggregateValue(sourceTable, filter);

                                _regressionTable.Rows.Add(seriesName, independentValue, dependentVal);
                            }
                        }
                    }
                }
                else if (sourceTable.Columns.Contains(_graphCrossTab))
                {
                    string categoryName = string.Empty;
                    Dictionary<object, double> indDepValuePairCollection = new Dictionary<object, double>();

                    List<string> StrataVarNameList = new List<string>();
                    List<string> StrataVarValueList = new List<string>();

                    foreach (string independentVariableName in _independentVariableArray)
                    {
                        StrataVarNameList.Add(filter.strataVarName);
                        StrataVarValueList.Add(filter.strataVarValue);

                        DataTable workingTable = cWorkingTable.CreateWorkingTable(
                            independentVariableName, 
                            _graphCrossTab, 
                            config,
                            sourceTable,
                            StrataVarNameList,
                            StrataVarValueList,
                            _weightVar);

                        foreach (DataColumn crossTabCandidate in workingTable.Columns)
                        {
                            if (crossTabCandidate.ColumnName != "__Values__" && crossTabCandidate.ColumnName != "__Count__")
                            {
                                Type crossTabDataType = sourceTable.Columns[_graphCrossTab].DataType;

                                string crossTabValue = crossTabCandidate.ColumnName;

                                if (crossTabDataType.Name == "Byte")
                                {
                                    if (crossTabCandidate.ColumnName == "0")
                                    {
                                        crossTabValue = config["RepresentationOfNo"];
                                    }
                                    else if (crossTabCandidate.ColumnName == "1")
                                    {
                                        crossTabValue = config["RepresentationOfYes"];
                                    }
                                }

                                string seriesName = string.Format("{0}={1}", _graphCrossTab, crossTabValue);

                                foreach (DataRow row in workingTable.Rows)
                                {
                                    double dependentVal;
                                    
                                    if (double.TryParse(row[crossTabCandidate.ColumnName].ToString(), out dependentVal))
                                    {
                                        if (row["__Values__"] != DBNull.Value)
                                        {
                                            string independentVariableTypeName = string.Empty;

                                            if (sourceTable.Columns.Contains(independentVariableName))
                                            {
                                                independentVariableTypeName = sourceTable.Columns[independentVariableName].DataType.Name;
                                            }

                                            categoryName = BuildCategoryName(independentVariableName, independentVariableTypeName, row, config);

                                            object independentValue = row["__Values__"];

                                            if (string.IsNullOrEmpty(_weightVar))
                                            {
                                                if (double.TryParse(row[crossTabCandidate.ColumnName].ToString(), out dependentVal))
                                                {
                                                    _regressionTable.Rows.Add(seriesName, independentValue, dependentVal);
                                                }
                                            }
                                            else
                                            {
                                                object independentVariableValue = row["__Values__"];

                                                bool isValue = false;

                                                if (independentVariableValue != null)
                                                {
                                                    isValue = true;

                                                    if (independentVariableValue is string)
                                                    {
                                                        string candidate = ((string)independentVariableValue).Trim();
                                                        isValue = candidate != string.Empty;
                                                    }
                                                }

                                                if (isValue)
                                                { 
                                                    filter.crossTabName = _graphCrossTab;
                                                    filter.crossTabValue = crossTabCandidate.ColumnName;
                                                    filter.independentVarName = independentVariableName;
                                                    filter.independentVarValue = independentVariableValue;
                                                    filter.weightVarName = _weightVar;
                                                    dependentVal = GetAggregateValue(sourceTable, filter);
                                                    _regressionTable.Rows.Add(seriesName, independentValue, dependentVal);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (_graphType == "EPICURVE" && _graphIntervalUnits != "")
                {
                    string categoryName = string.Empty;
                    Dictionary<object, double> indDepValuePairCollection = new Dictionary<object, double>();

                    List<string> StrataVarNameList = new List<string>();
                    List<string> StrataVarValueList = new List<string>();

                    foreach (string independentVariable in _independentVariableArray)
                    {
                        StrataVarNameList.Add(filter.strataVarName);
                        StrataVarValueList.Add(filter.strataVarValue);

                        DataTable workingTable = cWorkingTable.CreateWorkingTable(
                            independentVariable, 
                            "", 
                            config, 
                            sourceTable,
                            StrataVarNameList,
                            StrataVarValueList,
                            _weightVar);

                        DateTime intervalStart = DateTime.MinValue;
                        DateTime intervalEnd = DateTime.MinValue;
                        DateTime dateTimeValue = DateTime.MinValue;
                        intervalStart = _graphStartFrom;

                        double givenInterval = 1;
                        int days = 0, hours = 0, minutes = 0, seconds = 0; 
                        TimeSpan period = new TimeSpan(days, hours, minutes, seconds);

                        if (_graphInterval != "")
                        {
                            double.TryParse(_graphInterval, out givenInterval);
                        }

                        if (_graphIntervalUnits == "")
                        {
                            _graphIntervalUnits = "Hours";
                        }

                        foreach (DataRow row in workingTable.Rows)
                        {
                            if (row["__Values__"] != DBNull.Value)
                            {
                                dateTimeValue = (DateTime)row["__Values__"];

                                //if (intervalStart == DateTime.MinValue)
                                //{
                                //    intervalStart = dateTimeValue;
                                //    _graphStartFrom = dateTimeValue;
                                //}

                                while ( dateTimeValue >= intervalEnd)
                                {
                                    if (intervalEnd != DateTime.MinValue)
                                    { 
                                        intervalStart = intervalEnd;
                                    }

                                    switch (_graphIntervalUnits)
                                    {
                                        case "Years":
                                            intervalEnd = intervalStart.AddYears((int)givenInterval);
                                            break;
                                        case "Quarters":
                                            intervalEnd = intervalStart.AddDays(givenInterval * 365.25 / 4.0);
                                            break;
                                        case "Weeks":
                                            intervalEnd = intervalStart.AddDays(givenInterval * 7);
                                            break;
                                        case "Days":
                                            intervalEnd = intervalStart.AddDays(givenInterval);
                                            break;
                                        case "Hours":
                                            intervalEnd = intervalStart.AddHours(givenInterval);
                                            break;
                                        case "Minutes":
                                            intervalEnd = intervalStart.AddMinutes(givenInterval);
                                            break;
                                        case "Seconds":
                                            intervalEnd = intervalStart.AddSeconds(givenInterval);
                                            break;
                                    }
                                }

                                string seriesName = string.Empty;
                                object independentValue = new object();

                                independentValue = BuildIndependentValue(independentVariable, row, config);

                                if (string.IsNullOrEmpty(seriesName))
                                {
                                    seriesName = SilverlightStatics.COUNT;

                                    if (_independentVariableArray.Length > 1)
                                    {
                                        seriesName = independentVariable;
                                    }

                                    if(string.IsNullOrEmpty(_aggregateFunction) == false)
                                    {
                                        seriesName = _aggregateFunction;
                                    }
                                }

                                if (string.IsNullOrEmpty(_weightVar))
                                {
                                    double dependentVal;

                                    if (double.TryParse(row["__Count__"].ToString(), out dependentVal))
                                    {
                                        if ((dateTimeValue >= intervalStart) && (dateTimeValue < intervalEnd))
                                        {
                                            string expression = "Predictor = #" + intervalStart.ToString() + "#";
                                            DataRow[] foundRows = _regressionTable.Select(expression);

                                            if (foundRows.Length == 0)
                                            {
                                                _regressionTable.Rows.Add(seriesName, intervalStart, dependentVal);
                                            }
                                            else
                                            {
                                                foundRows[0]["Response"] = (double)foundRows[0]["Response"] + dependentVal;
                                                _regressionTable.AcceptChanges();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    filter.independentVarValue = row["__Values__"];
                                    filter.independentVarName = independentVariable;
                                    filter.weightVarName = _weightVar;
                                    double dependentVal = GetAggregateValue(sourceTable, filter);
                                    _regressionTable.Rows.Add(seriesName, independentValue, dependentVal);
                                }
                            }
                        }
                    }
                }
                else
                {
                    string categoryName = string.Empty;
                    Dictionary<object, double> indDepValuePairCollection = new Dictionary<object, double>();

                    List<string> StrataVarNameList = new List<string>();
                    List<string> StrataVarValueList = new List<string>();

                    foreach (string independentVariable in _independentVariableArray)
                    {
                        StrataVarNameList.Add(filter.strataVarName);
                        StrataVarValueList.Add(filter.strataVarValue);

                        DataTable workingTable = cWorkingTable.CreateWorkingTable(
                            independentVariable, 
                            "", 
                            config, 
                            sourceTable,
                            StrataVarNameList,
                            StrataVarValueList,
                            _weightVar);

                        foreach (DataRow row in workingTable.Rows)
                        {
                            if (row["__Values__"] != DBNull.Value)
                            {
                                string seriesName = string.Empty;
                                object independentValue = new object();

                                if (!_independentValueTypesSame || _graphType == SilverlightStatics.Pie )
                                {
                                    independentValue = BuildIndependentValue(independentVariable, row, config);
                                }
                                else if (_independentValuesAllBool)
                                {
                                    independentValue = independentVariable;
                                    byte value = (byte)(row["__Values__"]);

                                    seriesName = row["__Values__"].ToString();

                                    if (value == 0)
                                    {
                                        seriesName = config["RepresentationOfNo"];
                                    }
                                    else if (value == 1)
                                    {
                                        seriesName = config["RepresentationOfYes"];
                                    }
                                }
                                else
                                {
                                    independentValue = BuildIndependentValue(independentVariable, row, config);
                                }

                                if (string.IsNullOrEmpty(seriesName))
                                {
                                    seriesName = SilverlightStatics.COUNT;

                                    if (_independentVariableArray.Length > 1)
                                    {
                                        seriesName = independentVariable;
                                    }

                                    if(string.IsNullOrEmpty(_aggregateFunction) == false)
                                    {
                                        seriesName = _aggregateFunction;
                                    }
                                }

                                if (string.IsNullOrEmpty(_weightVar))
                                {
                                    //
                                    if (independentValue.ToString().ToUpperInvariant() == "TRUE")
                                    {
                                        independentValue = config["RepresentationOfYes"];
                                    }
                                    if (independentValue.ToString().ToUpperInvariant() == "FALSE")
                                    {

                                        independentValue = config["RepresentationOfNo"]; 
                                    }
                                    //
                                    double dependentVal;

                                    if (double.TryParse(row["__Count__"].ToString(), out dependentVal))
                                    {
                                        if ((_graphType == "EPICURVE" && independentValue is DateTime && ((DateTime)independentValue) <= _graphStartFrom) == false)
                                        {
                                            _regressionTable.Rows.Add(seriesName, independentValue, dependentVal);
                                        }
                                    }
                                }
                                else
                                {
                                    //
                                    if (independentValue.ToString().ToUpperInvariant() == "TRUE")
                                    {
                                        independentValue = config["RepresentationOfYes"];
                                    }
                                    if (independentValue.ToString().ToUpperInvariant() == "FALSE")
                                    {

                                        independentValue = config["RepresentationOfNo"]; 
                                    }
                                    //
                                    filter.independentVarValue = row["__Values__"];
                                    filter.independentVarName = independentVariable;
                                    filter.weightVarName = _weightVar;
                                    double dependentVal = GetAggregateValue(sourceTable, filter);
                                    _regressionTable.Rows.Add(seriesName, independentValue, dependentVal);
                                }
                            }
                        }
                    }
                }

                string graphTitle = _graphTitle;

                if (sourceTable.Columns.Contains(filter.strataVarName))
                {
                    Type _strataVarDataType = sourceTable.Columns[filter.strataVarName].DataType;
                    string strataVarText = filter.strataVarValue;

                    strataVarText = GetPrintValue(filter.strataVarName, _strataVarDataType.Name, filter.strataVarValue, config);

                    if (string.IsNullOrEmpty(_strataVar) == false)
                    {
                        graphTitle = string.Format("{0} {1}={2}", _graphTitle, filter.strataVarName, strataVarText);
                    }
                }

                DataView view = new DataView(_regressionTable);
                DataTable distinctValues = new DataTable();
                distinctValues = view.ToTable(true, "SeriesName");

                bool hideLegend = false;

                if (distinctValues.Rows.Count == 1 && distinctValues.Rows[0][0].ToString() == "COUNT")
                {
                    hideLegend = true;
                }

                objectElement = objectElement
                    + silverlight.Graph
                    (
                        _graphType,
                        graphTitle,
                        _graphIndependentAxisLabel,
                        _graphDependentAxisLabel,
                        "",
                        "",
                        _graphInterval,
                        _graphIntervalUnits,
                        _graphStartFrom,
                        _regressionTable,
                        hideLegend
                    );

                _regressionTable.Rows.Clear();
            }

            string data = string.Empty;

            if (objectElement != "")
            {
                data = objectElement + @"<hr/>";
            }
            else
            {
                data = SharedStrings.UNABLE_CREATE_GRAPH;
            }
            
            args.Add("COMMANDNAME", CommandNames.GRAPH);
            args.Add("DATA", data);
            args.Add("COMMANDTEXT", _commandText.Trim());
            Context.Display(args);
        }

        private double GetAggregateValue(System.Data.DataTable sourceTable, FilterStruct filter)
        {
            AggregateStruct statStruct = GetStatistics(sourceTable, filter);

            double dependentVal;

            switch (_aggregateFunction.ToUpperInvariant())
            {
                case SilverlightStatics.AVG:
                    dependentVal = statStruct.Average;
                    break;
                case SilverlightStatics.COUNT:
                    dependentVal = statStruct.Observations;
                    break;
                case SilverlightStatics.MAX:
                    if (statStruct.Maximun is double)
                    {
                        dependentVal = (double)statStruct.Maximun;
                    }
                    else
                    {
                        dependentVal = (int)statStruct.Maximun;
                    }
                    break;
                case SilverlightStatics.MIN:
                    if (statStruct.Minimum is double)
                    {
                        dependentVal = (double)statStruct.Minimum;
                    }
                    else
                    {
                        dependentVal = (int)statStruct.Minimum;
                    }
                    break;
                case SilverlightStatics.PERCENT:
                    dependentVal = (double)statStruct.Percent;
                    break;
                case SilverlightStatics.SUM:
                    dependentVal = (double)statStruct.Sum;
                    break;
                case SilverlightStatics.SUMPCT:
                    dependentVal = (double)statStruct.SumPercent;
                    break;
                default:
                    dependentVal = statStruct.Sum;
                    break;
            }
            return dependentVal;
        }

        private string BuildCategoryName(string independentVariable, string independentVariableTypeName, DataRow row, Dictionary<string, string> config)
        {
            string valuesItem = string.Empty;
            string categoryName = string.Empty;

            valuesItem = GetPrintValue(independentVariable, independentVariableTypeName, row["__Values__"], config);

            if (_independentVariableArray.Length > 1 && !(row["__Values__"] is System.DateTime))
            {
                categoryName = string.Format("{0}={1}", independentVariable, valuesItem);
            }
            else if (!string.IsNullOrEmpty(_graphCrossTab) && !(row["__Values__"] is System.DateTime))
            {
                categoryName = string.Format("{1}", independentVariable, valuesItem);
            }
            else
            {
                categoryName = valuesItem;
            }
            return categoryName;
        }

        private string GetPrintValue(string pFieldName, string pDataType, object pValue, Dictionary<string, string> pConfig)
        {
            string result = null;

            if (pValue == DBNull.Value)
            {
                result = pConfig["RepresentationOfMissing"];
            }
            else switch (pDataType.Replace("System.", ""))
                {
                    case "Byte":
                        if (this.Context.EpiViewVariableList.ContainsKey(pFieldName))
                        {
                            int val = 0;
                            if (int.TryParse(pValue.ToString(), out val))
                            {
                                result = (val != 0 ? pConfig["RepresentationOfYes"] : pConfig["RepresentationOfNo"]);
                            }
                            else
                            {

                                result = pValue.ToString();
                            }
                        }
                        else
                        {
                            result = pValue.ToString();
                        }

                        break;
                    case "Boolean":
                        result = (Convert.ToBoolean(pValue) ? pConfig["RepresentationOfYes"] : pConfig["RepresentationOfNo"]);
                        break;
                    case "Double":
                        result = string.Format("{0:0.##}", pValue);
                        break;
                    default:
                        result = pValue.ToString();
                        break;
                }

            return result;
        }

        private object BuildIndependentValue(string independentVariable, DataRow row, Dictionary<string, string> config)
        {
            string valuesItem = string.Empty;
            string categoryName = string.Empty;

            if (row["__Values__"] is System.DateTime)
            {
                return row["__Values__"];
            }
            else if (row["__Values__"] is System.Double)
            {
                return row["__Values__"];
            }
            else
            {
                valuesItem = row["__Values__"].ToString();
            }

            if (_independentValuesAllBool)
            {
                byte value = (byte)(row["__Values__"]);
                valuesItem = row["__Values__"].ToString();

                if (value == 0)
                {
                    valuesItem = config["RepresentationOfNo"];
                }
                else if (value == 1)
                {
                    valuesItem = config["RepresentationOfYes"];
                }
            }

            if (_independentVariableArray.Length > 1 && !(row["__Values__"] is System.DateTime))
            {
                categoryName = string.Format("{0}={1}", independentVariable, valuesItem);
            }
            else if (!string.IsNullOrEmpty(_graphCrossTab) && !(row["__Values__"] is System.DateTime))
            {
                categoryName = string.Format("{0}={1}", independentVariable, valuesItem);
            }
            else
            {
                categoryName = valuesItem;
            }
            return categoryName;
        }

        private object GetValue(string columnName, DataRow row, Dictionary<string, string> config)
        {
            string valuesItem = string.Empty;
            string categoryName = string.Empty;

            if (row[columnName] is System.DateTime)
            {
                return row[columnName];
            }
            else if (row[columnName] is System.Double)
            {
                return row[columnName];
            }
            else
            {
                valuesItem = row[columnName].ToString();
            }

            if (_independentValuesAllBool)
            {
                byte value = (byte)(row[columnName]);
                valuesItem = row[columnName].ToString();

                if (value == 0)
                {
                    valuesItem = config["RepresentationOfNo"];
                }
                else if (value == 1)
                {
                    valuesItem = config["RepresentationOfYes"];
                }
            }

            if (_independentVariableArray.Length > 1 && !(row["__Values__"] is System.DateTime))
            {
                categoryName = string.Format("{0}={1}", columnName, valuesItem);
            }
            else if (!string.IsNullOrEmpty(_graphCrossTab) && !(row["__Values__"] is System.DateTime))
            {
                categoryName = string.Format("{0}={1}", columnName, valuesItem);
            }
            else
            {
                categoryName = valuesItem;
            }
            return categoryName;
        }

        private void AddDataPoint(ref Dictionary<string, Dictionary<object, double>> IndDepValueDictionary, string seriesName, object independentValue, double dependentValue)
        {
            if (IndDepValueDictionary.ContainsKey(seriesName) == false)
            {
                IndDepValueDictionary.Add(seriesName, new Dictionary<object, double>());
            }

            IndDepValueDictionary[seriesName].Add(independentValue, dependentValue);
        }

        private System.Data.DataTable BuildTempContextTable()
        {
            System.Data.DataTable tempTable = new DataTable();
            foreach (DataColumn column in Context.Columns)
            {
                DataColumn newColumn = new DataColumn(column.ColumnName);
                newColumn.DataType = column.DataType;
                tempTable.Columns.Add(newColumn);
            }

            foreach (DataRow row in Context.GetDataRows(null))
            {
                tempTable.ImportRow(row);
            }
            return tempTable;
        }

        public static AggregateStruct GetStatistics(DataTable sourceTable, FilterStruct filter)
        {
            AggregateStruct result;

            result.Observations = 0;
            result.Sum = 0;
            result.Average = 0;
            result.Variance = 0;
            result.Std_Dev = 0;
            result.Minimum = 0;
            result.Median_25 = 0;
            result.Median = 0;
            result.Median_75 = 0;
            result.Maximun = 0;
            result.Percent = 0;
            result.SumPercent = 0;

            DataRow[] filteredRows;

            string indCompare = string.Empty;

            if (filter.independentVarValue is string || filter.independentVarValue is DateTime)
            {
                filter.independentVarValue = "'" + filter.independentVarValue + "'";
            }

            string filterSelect = string.Format("{0} = {1}", filter.independentVarName, filter.independentVarValue);

            if (filter.crossTabValue is string || filter.crossTabValue is DateTime)
            {
                filter.crossTabValue = "'" + filter.crossTabValue + "'";
            }

            if (string.IsNullOrEmpty(filter.crossTabName) == false)
            {
                filterSelect = string.Format("{0} AND {1} = {2}", filterSelect, filter.crossTabName, filter.crossTabValue);
            }

            if (string.IsNullOrEmpty(filter.strataVarName) == false)
            {
                double value;
                if (Double.TryParse(filter.strataVarValue, out value))
                {
                    filterSelect = string.Format("{0} AND [{1}] = {2}", filterSelect, filter.strataVarName, filter.strataVarValue);
                }
                else
                {
                    filterSelect = string.Format("{0} AND [{1}] = '{2}'", filterSelect, filter.strataVarName, filter.strataVarValue);
                }


            }

            filteredRows = sourceTable.Select(filterSelect);

            int rowCount = sourceTable.Rows.Count;

            if (filteredRows.Length < 1) return result;

            double observations = 0;
            double sum = 0;
            string independentVarString = "";
            string weightVarString = "";

            foreach (System.Data.DataRow row in sourceTable.Rows)
            {
                double fromParse;
                independentVarString = row[filter.independentVarName].ToString();
                weightVarString = row[filter.weightVarName].ToString();

                if (independentVarString != "" && weightVarString != "")
                {
                    double.TryParse(row[filter.weightVarName].ToString(), out fromParse);
                    sum += fromParse;
                    observations++;
                }
            }

            int i = 0;
            foreach (System.Data.DataRow row in filteredRows)
            {
                double weightValue;

                string candidate = row[filter.weightVarName].ToString();
                result.Observations += 1;
                
                if (double.TryParse(candidate, out weightValue))
                {
                    result.Sum += weightValue;

                    if (i == 0)
                    {
                        result.Maximun = result.Minimum = weightValue;
                    }
                    else
                    {
                        int compare = ((IComparable)weightValue).CompareTo(result.Maximun);
                        if (compare > 0)
                        {
                            result.Maximun = weightValue;
                        }

                        compare = ((IComparable)weightValue).CompareTo(result.Minimum);
                        if (compare < 0)
                        {
                            result.Minimum = weightValue;
                        }
                    }
                    i++;
                }
            }

            result.Average = result.Sum / result.Observations;
            result.Std_Dev = Math.Sqrt(result.Variance);

            result.Percent = (result.Observations / observations) * 100;
            result.SumPercent = (result.Sum / sum) * 100;

            return result;
        }

        public struct AggregateStruct
        {
            public double Observations;
            public double Sum;
            public double Average;
            public double Variance;
            public double Std_Dev;
            public object Minimum;
            public double Median;
            public double Median_25;
            public double Median_75;
            public object Maximun;
            public double Percent;
            public double SumPercent;
        }

        public struct FilterStruct
        {
            public string independentVarName;
            public object independentVarValue;
            public string strataVarName;
            public string strataVarValue;
            public string crossTabName;
            public string crossTabValue;
            public string weightVarName;
        }
    }
}
