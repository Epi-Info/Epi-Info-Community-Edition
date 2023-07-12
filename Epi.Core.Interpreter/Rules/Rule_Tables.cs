using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

using com.calitha.goldparser;
using Epi.Data;
using Epi.Web;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    

    /*
<table_exposure> ::= '*' | Identifier
<table_outcome> ::= '*' | Identifier

<Simple_Tables_Statement>::= TABLE <table_exposure>
                      | TABLE <table_exposure> <FreqOpts>
                      | TABLE <table_exposure> <table_outcome>
                      | TABLE <table_exposure> <table_outcome> <FreqOpts>
     
    <FreqOpts>                          ::= <FreqOpts> <FreqOpt> | <FreqOpt> 

    <FreqOpt>                           ::= <WeightOpt>
                                    | <FreqOptStrata>
                                    | <OutTableOpt>
                                    | <SetClause>
                                    | <FreqOptNoWrap>
                                    | <FreqOptColumnSize>
                                    | <FreqOptPsuvar>

    <WeightOpt>                         ::= WEIGHTVAR '=' Identifier
    <FreqOptStrata>                     ::= STRATAVAR '=' <IdentifierList>
    <FreqOptNoWrap>                     ::= NOWRAP
    <FreqOptColumnSize>                     ::= COLUMNSIZE '=' DecLiteral
    <FreqOptPsuvar>                     ::= PSUVAR '=' Identifier
     */

    public class Rule_Tables : AnalysisRule
    {
        bool HasRun = false;

        string[] IdentifierList = null;
        string[] IdentifierList2 = null;
        Dictionary<string, string> inputVariableList;
        string[] Stratvar = null;
        string Exposure = null;
        string Outcome = null;
        string OutTable = null;
        string WeightVar = null;
        string PSUVar = null;
        //bool isExceptionList = false;
        string[] FrequencyOptions = null;
        string commandText = string.Empty;

        public bool tablesShowStatistics = true;
        public bool tablesDoFisher = false;

        string parameter1 = null;
        string parameter2 = null;
        string parameter3 = null;
        string parameter4 = null;
        //DataTable dt = null;

        EpiInfo.Plugin.IAnalysisStatistic TablesStatistic;

        public Rule_Tables(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //this.isExceptionList = false;

            commandText = this.ExtractTokens(pToken.Tokens);

            foreach (Token T in pToken.Tokens)
            {
                if (T is NonterminalToken)
                {
                    NonterminalToken NT = (NonterminalToken)T;
                    switch (NT.Symbol.ToString())
                    {
                        case"<table_exposure>":
                            this.SetTableExposure(NT);
                            break;
                        case "<table_outcome>":
                            this.SetTableOutcome(NT);
                            break;
                        case "<FreqOpts>":
                            //this.SetFreqOpts(NT);
                            this.SetFreqOpts(NT);
                            break;
                        case "<FreqOpt>":
                            this.SetFreqOpt(NT);
                            break;
                    }
                }
                else
                {
                    TerminalToken TT = (TerminalToken)T;
                    switch (TT.Symbol.ToString())
                    {
                        case"<table_exposure>":
                        case "<table_outcome>":
                        case "<FreqOpts>":
                            break;
                    }
                }
            }
        }


        private void SetTableExposure(NonterminalToken pT)
        {
            //inputVariableList.Add("EXPOSURE_VARIABLE", this.GetCommandElement(pT.Tokens, 0));
            this.Exposure = this.GetCommandElement(pT.Tokens, 0).Trim(new char[] {'[',']'});
        }


        private void SetTableOutcome(NonterminalToken pT)
        {
            //inputVariableList.Add("OUTCOME_VARIABLE", this.GetCommandElement(pT.Tokens, 0));
            this.Outcome = this.GetCommandElement(pT.Tokens, 0).Trim(new char[] { '[', ']' });
        }
        
        private void SetFreqOpts(NonterminalToken pT)
        {
            // <FreqOpts>                          ::= <FreqOpts> <FreqOpt> | <FreqOpt>

            foreach (Token T in pT.Tokens)
            {
                if (T is NonterminalToken)
                {
                    NonterminalToken NT = (NonterminalToken)T;
                    switch (NT.Symbol.ToString())
                    {
                        case "<FreqOpt>":
                            this.SetFreqOpt(NT);
                            break;
                        case "<FreqOpts>":
                            this.SetFreqOpt((NonterminalToken)NT.Tokens[0]);
                            this.SetFreqOpt((NonterminalToken)NT.Tokens[1]);
                            break;
                    }

                }

            }
        }

        private void SetFreqOpt(NonterminalToken pT)
        {
            /* <FreqOpt>                           ::= <WeightOpt>
                                    | <FreqOptStrata>
                                    | <OutTableOpt>
                                    | <SetClause>
                                    | <FreqOptNoWrap>
                                    | <FreqOptColumnSize>
                                    | <FreqOptPsuvar>

    <WeightOpt>                         ::= WEIGHTVAR '=' Identifier
    <FreqOptStrata>                     ::= STRATAVAR '=' <IdentifierList>
    <FreqOptNoWrap>                     ::= NOWRAP
    <FreqOptColumnSize>                     ::= COLUMNSIZE '=' DecLiteral
    <FreqOptPsuvar>                     ::= PSUVAR '=' Identifier*/

            switch (pT.Rule.Rhs[0].Name.ToString())
            {
                case "WEIGHTVAR":
                    this.WeightVar = this.GetCommandElement(pT.Tokens,2).Trim(new char[] { '[', ']' });
                    //inputVariableList.Add("WEIGHTVAR", this.GetCommandElement(pT.Tokens, 2).Trim(new char[] { '[', ']' }));
                    break;
                case "STRATAVAR":
                    //this.Stratvar = this.GetCommandElement(pT.Tokens, 2);
                    this.Stratvar = AnalysisRule.SpliIdentifierList(this.GetCommandElement(pT.Tokens, 2));
                    //StringBuilder temp2 = new StringBuilder();

                    
                    break;
                case "STATISTICS":
                    if (this.GetCommandElement(pT.Tokens, 2).Equals("NONE"))
                    {
                        this.tablesShowStatistics = false;
                    }
                    else if (this.GetCommandElement(pT.Tokens, 2).Equals("FISHER"))
                    {
                        this.tablesDoFisher = true;
                    }
                    break;
                case "OUTTABLE":
                    this.OutTable = this.GetCommandElement(pT.Tokens, 2).Trim(new char[] { '[', ']' });
                    //inputVariableList.Add("OUTTABLE", this.GetCommandElement(pT.Tokens, 2).Trim(new char[] { '[', ']' }));
                    break;
                case "<SetClause>":
                    break;
                case "<FreqOptNoWrap>":
                    //this.op
                    break;
                case "<FreqOptColumnSize>":
                    break;
                case "PSUVAR":
                    this.PSUVar = this.GetCommandElement(pT.Tokens, 2).Trim(new char[] { '[', ']' });
                    //inputVariableList.Add("PSUVAR", this.GetCommandElement(pT.Tokens, 2).Trim(new char[] { '[', ']' }));
                    break;
            }
        }

        public override object Execute()
        {
            object result = null;

            if (!this.HasRun)
            {
                //Configuration config = Configuration.GetNewInstance();


                Dictionary<string, string> setProperties = this.Context.GetGlobalSettingProperties();
                List<string> ExposureVariableList = new List<string>();
                ExposureVariableList.Add(this.Exposure);

                bool isExceptionList = false;
                this.Context.ExpandGroupVariables(ExposureVariableList, ref isExceptionList);


                foreach (string EXPOSURE_VARIABLE in ExposureVariableList)
                {
                    inputVariableList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    inputVariableList.Add("EXPOSURE_VARIABLE", EXPOSURE_VARIABLE);
                    inputVariableList.Add("OUTCOME_VARIABLE", this.Outcome);

                    if (this.Stratvar != null)
                    {
                        StringBuilder temp = new StringBuilder();
                        foreach (string s in Stratvar)
                        {
                            temp.Append(s);
                            temp.Append(',');
                        }
                        if (temp.Length > 0)
                        {
                            temp.Length = temp.Length - 1;
                        }
                        inputVariableList.Add("STRATAVAR", temp.ToString());
                    }

                    if (!string.IsNullOrEmpty(OutTable))
                    {
                        inputVariableList.Add("OutTable", OutTable);
                    }

                    if (!string.IsNullOrEmpty(WeightVar))
                    {
                        inputVariableList.Add("WeightVar", WeightVar);
                    }
                    if (!tablesShowStatistics)
                    {
                        inputVariableList.Add("STATISTICS", "NONE");
                    }
                    else if (tablesDoFisher)
                    {
                        inputVariableList.Add("STATISTICS", "FISHER");
                    }
                    inputVariableList.Add("commandText", commandText);
                    if (!string.IsNullOrEmpty(PSUVar))
                    {
                        inputVariableList.Add("PSUVar", PSUVar);
                    }
                    if (this.Context.CurrentRead == null)
                    {
                        inputVariableList.Add("TableName", "");
                    }
                    else
                    {
                        inputVariableList.Add("TableName", this.Context.CurrentRead.Identifier);
                    }

                    EpiInfo.Plugin.IDataSource DataSource = this.Context.GetDefaultIDataSource();

                    AnalysisStatisticExecuteHost statisticHost = new AnalysisStatisticExecuteHost(this.Context, setProperties, DataSource, inputVariableList, this.Context.CurrentSelect.ToString(), this.Context.AnalysisInterpreterHost);

                    if (!inputVariableList.ContainsKey("PSUVAR"))
                    {
                        this.TablesStatistic = this.Context.GetStatistic("Tables", statisticHost);
                    }
                    else
                    {
                        this.TablesStatistic = this.Context.GetStatistic("ComplexSampleTables", statisticHost);
                    }

                    this.TablesStatistic.Execute();

                    this.TablesStatistic = null;
                }

                this.HasRun = true;
            }

            return result;
        }

        private string ConvertToPercent(double pValue)
        {
            return string.Format("{0: ##0.0}%", (100.0 * pValue));
        }

        private string ConvertToPixelLength(double pValue)
        {
            return string.Format("{0: ##0}px", 1 * Math.Round((100.0 * pValue), MidpointRounding.AwayFromZero));
        }

        private void SetTableOptions(NonterminalToken pToken)
        {

            for (int i = 0; i < pToken.Tokens.Length; i++)
            {
                NonterminalToken T = (NonterminalToken)pToken.Tokens[i];
                switch (T.Rule.Rhs[0].ToString())
                {
                    case "<FreqOpts>":
                    case "<WeightOpt>":
                    case "<FreqOpt>":
                        for (int j = 0; j < T.Tokens.Length; j++)
                        {
                            string temp = this.ExtractTokens(((NonterminalToken)T.Tokens[j]).Tokens);
                            string[] tmp = temp.Split('=');

                            switch (tmp[0].ToUpperInvariant().Trim())
                            {
                                case "STRATAVAR":
                                    this.Stratvar = AnalysisRule.SpliIdentifierList(tmp[1].Trim());
                                    break;
                                case "WEIGHTVAR":
                                    this.WeightVar = tmp[1].Trim();
                                    break;
                            }
                        }
                        break;
                    case "<OutTableOpt>":
                        this.OutTable = this.GetCommandElement(T.Tokens, 2);
                        break;
                    case "<FreqOptPsuvar>":
                        this.PSUVar = this.GetCommandElement(T.Tokens, 2);
                        break;
                }
            }
        }


        /// <summary>
        /// performs execution of the TABLES command
        /// </summary>
        /// <returns>object</returns>
        public object Execute_old()
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("COMMANDNAME", CommandNames.TABLES);
            args.Add("parameter1", parameter1);
            if ((parameter2 != string.Empty))
            {
                int i = parameter2.Length;
                args.Add("parameter2", parameter2);
            }
            if ((parameter3 != string.Empty)&& (parameter4 != null))
            {
                args.Add("parameter3", parameter3);
            }
            if ((parameter4 != string.Empty) && (parameter4 != null))
            { 
                args.Add("parameter4", parameter4);
            }

            this.Context.AnalysisCheckCodeInterface.Display(args);

            return null;
        }
    }

    public static class HtmlRenderer
    {
        public static string RenderHtml(Rule_Context pContext, string commandName, string fileName, string tableName, int rowCount)
        {

            StringBuilder sb = new StringBuilder();
            Epi.DataSets.Config.SettingsRow settings = Configuration.GetNewInstance().Settings;
            sb.Append(HTML.Italics(SharedStrings.CURRENT_VIEW + ":&nbsp;"));
            sb.Append(HTML.Bold(String.Format("{0}:{1}", fileName.Trim(new char[] { '\'' }), tableName)));
            if (pContext.CurrentRead.RelatedTables != null)
            {
                foreach (string table in pContext.CurrentRead.RelatedTables)
                {
                    sb.Append(HTML.Tag("br"));
                    sb.Append(HTML.Italics("&nbsp&nbsp&nbsp&nbspRelate:&nbsp;"));
                    sb.Append(HTML.Bold(table));
                }
            }
            if (pContext.DataInfo.SelectCriteria != String.Empty)
            {
                sb.Append(HTML.Tag("br"));
                sb.Append(HTML.Italics("Selection:&nbsp;&nbsp;"));
                sb.Append("&nbsp;");
                sb.Append(HTML.Bold(EpiExpression(pContext, pContext.DataInfo.SelectCriteria)));
            }
            if (pContext.DataInfo.GetSqlStatementPartSortBy() != String.Empty)
            {
                sb.Append(HTML.Tag("br"));
                sb.Append(HTML.Italics("Sort By:&nbsp;&nbsp;"));
                sb.Append(HTML.Bold(EpiExpression(pContext, pContext.DataInfo.GetSqlStatementPartSortBy())));
            }
            sb.Append(HTML.Tag("br"));
            sb.Append(HTML.Italics(SharedStrings.RECORD_COUNT + ":&nbsp;&nbsp;"));
            sb.Append(HTML.Bold(rowCount.ToString()));
            string scope = string.Empty;
            switch (settings.RecordProcessingScope)
            {
                case 1:
                    scope = SharedStrings.DELETED_RECORDS_EXCLUDED;
                    break;
                case 2:
                    scope = SharedStrings.DELETED_RECORDS_ONLY;
                    break;
                default:
                    scope = SharedStrings.DELETED_RECORDS_INCLUDED;
                    break;
            }
            sb.Append("&nbsp;");
            sb.Append(HTML.Italics("(" + scope + ")&nbsp;&nbsp;&nbsp;"));
            sb.Append(HTML.Italics("Date:"));
            sb.Append("&nbsp;&nbsp;");
            sb.Append(HTML.Bold(DateTime.Now.ToString()));
            sb.Append(HTML.Tag("br"));
            sb.Append(HTML.Tag("br"));


            return sb.ToString();
        }

        private static bool IsBoolean(IVariable var)
        {
            return (var.DataType == DataType.Boolean || var.DataType == DataType.YesNo);
        }

        private static bool IsBoolean(Rule_Context pContext, string name)
        {
            return IsBoolean(pContext.MemoryRegion.GetVariable(name));
        }

        private static string RepresentationOfValue(string val, bool isBoolean)
        {
            Configuration config = Configuration.GetNewInstance();

            if (isBoolean)
            {
                return (val == "0") ? config.Settings.RepresentationOfNo : config.Settings.RepresentationOfYes;
            }
            else
            {
                return val;
            }
        }

        public static string TableHeadingHTML(Rule_Context pContext, DataTable distinct, string outcome, string exposure)
        {
            Configuration config = Configuration.GetNewInstance();
            StringBuilder sb = new StringBuilder();
            IMemoryRegion module = pContext.MemoryRegion;

            IVariable oVar = module.GetVariable(outcome);
            string outcomeWord = (config.Settings.ShowCompletePrompt) ?
                oVar.PromptText.ToString() : oVar.Name;
            IVariable eVar = module.GetVariable(exposure);
            string exposureWord = (config.Settings.ShowCompletePrompt) ?
                eVar.PromptText.ToString() : eVar.Name.ToString();

            sb.Append("<caption> <b>").Append(outcomeWord).Append("</b></caption>");

            sb.Append("<tr>");
            sb.Append("<th nowrap>").Append(exposureWord).Append("</th>");
            foreach (DataRow row in distinct.Rows)
            {
                foreach (DataColumn col in distinct.Columns)
                {
                    IVariable var = module.GetVariable(col.ColumnName);
                    DataType thisType = var.DataType;
                    bool isBoolean = (thisType == DataType.Boolean || thisType == DataType.YesNo);

                    sb.Append("<th>");
                    if (row[col.ColumnName] == null ||
                        string.IsNullOrEmpty(row[col.ColumnName].ToString()))
                    {
                        sb.Append(config.Settings.RepresentationOfMissing);
                    }
                    else
                    {
                        string val = RepresentationOfValue(row[col.ColumnName].ToString(), isBoolean);
                        sb.Append(val);
                    }
                    sb.Append("</th>");
                }
            }
            sb.Append("<th>TOTAL</th>");
            sb.Append("</tr>");

            return sb.ToString();
        }

        public static string TableRowHTML(string row, string col, int rowCount, int colCount, bool isBooleanField)     // exposure is going to have to be the word
        {
            StringBuilder sb = new StringBuilder();
            string rowWord = row;
            if (isBooleanField)
            {
                rowWord = RepresentationOfValue(row, true);
            }
            sb.Append("<tr>");
            sb.Append("<td align=right><b>");
            sb.Append(row);
            sb.Append("</b><br>row %<br>col %</td>");

            //foreach()
            //{
            //    sb.Append("<td align=right>");
            //    sb.Append(count).Append("<br>");
            //    double rowPct = ((double)count / (double)rowTotal) * 100;
            //    sb.Append(rowPct.ToString("##.#")).Append("<br>");
            //    double colPct = ((double)count / count) * 100;
            //    sb.Append(colPct.ToString("##.#"));
            //    sb.Append("</td>");
            //}
            sb.Append("<td align=right>").Append(rowCount).Append("<br>100.0<br>100.0").Append("</td>");
            sb.Append("</tr>");
            return sb.ToString();
        }

        // This is for the whole table.
        public static string TableDataHTML(string exposureField, string outcomeField, DataTable table2x2, int colCount)
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, Dictionary<string, int>> exposureList = new Dictionary<string, Dictionary<string, int>>();

            Dictionary<string, int> exposureTotals = new Dictionary<string, int>();
            Dictionary<string, int> outcomeTotals = new Dictionary<string, int>();

            int grandTotal = 0;
            //calulate totals
            foreach (DataRow r in table2x2.Rows)
            {
                string currExposure = r[ColumnNames.EXPOSURE].ToString();
                string currOutcome = r[ColumnNames.OUTCOME].ToString();
                int currCount = (int)r[ColumnNames.COUNT];
                grandTotal += currCount;

                bool isBoolOutcome = false;//(table2x2.Columns[colIndex].GetType() == typeof(bool));
                bool isBoolExposure = false;
                int outcomeCount = currCount;
                int exposureCount = currCount;

                if (isBoolExposure)
                {
                    if (exposureCount == 1)
                    {
                        currExposure += "#TRUE";
                        AddToTotal(exposureTotals, currExposure, exposureCount);
                    }
                    else
                    {
                        currExposure += "#FALSE";
                        exposureCount = 1;
                        AddToTotal(exposureTotals, currExposure, exposureCount);
                    }
                }
                else
                {
                    AddToTotal(exposureTotals, currExposure, exposureCount);
                }

                if (isBoolOutcome)
                {
                    if (outcomeCount == 1)
                    {
                        currOutcome += "#TRUE";
                        AddToTotal(outcomeTotals, currOutcome, outcomeCount);
                    }
                    else
                    {
                        currOutcome += "#FALSE";
                        outcomeCount = 1;
                        AddToTotal(outcomeTotals, currOutcome, outcomeCount);
                    }
                }
                else
                {
                    AddToTotal(outcomeTotals, currOutcome, outcomeCount);
                }
            }

            string[] keys = new string[outcomeTotals.Keys.Count];
            outcomeTotals.Keys.CopyTo(keys, 0);

            sb.Append("<table>");
            sb.AppendFormat("<tr><td>&nbsp;</td><td align=center colspan={0}><b>{1}</b></td></tr>", keys.Length, outcomeField);
            sb.Append(HeaderHtml(keys, exposureField));

            StringBuilder rowBuilder = new StringBuilder();
            int exposureTotal = 0;

            foreach (DataRow row in table2x2.Rows)
            {
                //create new html row
                string exposure = row[0].ToString();
                rowBuilder = new StringBuilder();
                exposureTotal = (int)row[1];
                rowBuilder.Append(RowHtml(exposure));

                for (int colIndex = 2; colIndex < table2x2.Columns.Count; colIndex++)
                {
                    string outcome = table2x2.Columns[colIndex].ColumnName;
                    int currCount = 0;
                    int.TryParse(row[colIndex].ToString(), out currCount);
                    bool isBoolField = (table2x2.Columns[colIndex].GetType() == typeof(bool));

                    if (isBoolField)
                    {
                        if (currCount == 1)
                        {
                            outcome += "#TRUE";
                            int outcomeTotal = outcomeTotals[outcome];
                            rowBuilder.Append(ColumnHtml(currCount, exposureTotal, outcomeTotal));
                        }
                        else
                        {
                            outcome += "#FALSE";
                            currCount = 1;
                            int outcomeTotal = outcomeTotals[outcome];
                            rowBuilder.Append(ColumnHtml(currCount, exposureTotal, outcomeTotal));
                        }
                    }
                    else
                    {
                        //add the next outcome
                        int outcomeTotal = outcomeTotals[outcome];
                        rowBuilder.Append(ColumnHtml(currCount, exposureTotal, outcomeTotal));
                    }
                }

                //add the last exposure info to the row
                rowBuilder.Append(ColumnHtml(exposureTotal, exposureTotal, grandTotal));
                rowBuilder.Append("</tr>");
                sb.Append(rowBuilder.ToString());
            }

            StringBuilder tr = new StringBuilder();
            tr.Append("<tr><td><b><i>Total</i></b><br/>Row %<br/>Col %</td>");
            for (int colIndex = 2; colIndex < table2x2.Columns.Count; colIndex++)
            {
                string currentOutcome = table2x2.Columns[colIndex].ColumnName;
                int colTotal = outcomeTotals[currentOutcome];
                tr.Append(ColumnHtml(colTotal, colTotal, grandTotal));
            }
            tr.Append(ColumnHtml(grandTotal, grandTotal, grandTotal));

            tr.Append("</tr>");

            sb.Append(tr.ToString());
            sb.Append("</table>");
            return sb.ToString();
        }

        private static void AddToTotal(Dictionary<string, int> totals, string key, int count)
        {
            if (totals.ContainsKey(key))
            {
                totals[key] += count;
            }
            else
            {
                totals.Add(key, count);
            }
        }

        public static string HeaderHtml(string[] columns, string exposureField)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<tr>");
            sb.AppendFormat("<td align=center><b>{0}</b></td>", exposureField);
            for (int i = 0; i < columns.Length; i++)
            {
                sb.AppendFormat("<td align=center><b>{0}</b></td>", columns[i]);
            }
            sb.Append("<td align=center><b>Total</b></td>");
            sb.Append("</tr>");
            return sb.ToString();
        }

        public static string ColumnHtml(int colCount, int rowTotal, int colTotal)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<td align=right>");
            sb.Append(colCount).Append("<br>");
            double rowPct = ((double)colCount / (double)rowTotal) * 100;
            sb.Append(rowPct.ToString("#0.0")).Append("<br>");
            double colPct = ((double)colCount / colTotal) * 100;
            sb.Append(colPct.ToString("#0.0"));
            sb.Append("</td>");
            return sb.ToString();
        }

        public static string RowHtml(string rowName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<tr><td align=right><b>");
            sb.Append(rowName);
            sb.Append("</b><br/>");
            sb.Append("Row %");
            sb.Append("<br/>");
            sb.Append("Col %");
            sb.Append("</td>");
            return sb.ToString();
        }

        public static string TotalsHTML(DataTable table2x2)
        {
            StringBuilder sb = new StringBuilder();
            return sb.ToString();
        }

        public static string TotalsHTML(int colCount, ulong rowTotal)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<tr>");
            sb.Append("<td align=right><b>").Append(SharedStrings.TOTAL);
            sb.Append("</b><br>Row %<br>Col %").Append("</td>");
            for (int i = 0; i < colCount; i++)
            {
                sb.Append("<td align=right>").Append("count").Append(i.ToString());
                sb.Append("<br>pct").Append(i.ToString());
                sb.Append("<br>").Append("100").Append("</td>");
            }
            sb.Append("<td align=right>").Append(rowTotal).Append("<br>100.0<br>100.0</td>");
            sb.Append("</tr>");
            return sb.ToString();
        }

        public static string TableHTML(Rule_Context pContext, string outcomeName, string exposureName)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                // Generate the table
                DataSet ds = pContext.DataInfo.GetDataSet2x2(pContext, outcomeName, exposureName);
                sb.Append("<table border align=left>");
                sb.Append(TableHeadingHTML(pContext, ds.Tables["DistinctOutcomes"], outcomeName, exposureName));
                sb.Append(TableDataHTML(outcomeName, exposureName, ds.Tables["Table2x2"], ds.Tables["DistinctOutcomes"].Rows.Count));
                sb.Append("</table>");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new GeneralException(SharedStrings.DATA_TABLE_NOT_CREATED, ex);
            }
        }

        private static string EpiExpression(Rule_Context pContext, string whereClause)
        {
            string expression = string.Empty;
            if (!string.IsNullOrEmpty(whereClause))
            {
                string where = whereClause.Replace("is null", " = (.)").Replace("is not null", " <> (.)");
                bool isBoolean = false;
                int result = 0;
                string s = string.Empty;
                string[] expStrings = where.Split(new char[] { ' ' });

                for (int i = 0; i < expStrings.Length; i++)
                {
                    if (string.IsNullOrEmpty(expStrings[i]))
                    {
                        s = string.Empty;
                    }
                    else if (Regex.IsMatch(expStrings[i], @"[()=]"))
                    {
                        s = expStrings[i];
                    }
                    else if (expStrings[i] == "<>")
                    {
                        s = expStrings[i];
                    }
                    else if (expStrings[i].Equals("and", StringComparison.CurrentCultureIgnoreCase) || expStrings[i].Equals("or", StringComparison.CurrentCultureIgnoreCase))
                    {
                        s = expStrings[i];
                    }
                    else if (!int.TryParse(expStrings[i], out result))        // Maybe it's a variable?
                    {
                        s = expStrings[i];
                        IVariable var = null;
                        try
                        {
                            var = pContext.MemoryRegion.GetVariable(s);
                        }
                        catch
                        {
                            var = null;
                        }
                        if (var != null)                    // It really is a variable
                        {
                            isBoolean = (var.DataType == DataType.Boolean || var.DataType == DataType.YesNo);
                        }
                        else
                        {
                            s = expStrings[i].ToString();
                        }
                    }
                    else if (isBoolean && int.TryParse(expStrings[i], out result))
                    {
                        if (result == 0)
                        {
                            s = "(-)";
                        }
                        if (result == 1)
                        {
                            s = "(+)";
                        }
                        isBoolean = false;
                    }
                    else
                    {
                        s = expStrings[i].ToString();
                    }
                    if (!string.IsNullOrEmpty(expression))
                    {
                        expression += " ";
                    }
                    expression += s;
                }

            }
            return expression;
        }

    }

}
