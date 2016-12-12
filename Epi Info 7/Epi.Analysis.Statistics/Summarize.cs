using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using EpiInfo.Plugin;
using Epi.Data;

namespace Epi.Analysis.Statistics
{
    public class Summarize : IAnalysisStatistic
    {
        string[] aggregateList = null;
        Dictionary<string, string> aggregateElement = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string[] stratvarList = null;
        string outTableName = null;
        string weightVar = null;
        string commandText = string.Empty;
        string currentRead_Identifier = string.Empty;
        IAnalysisStatisticContext Context;
        List<string> participatingVariableList = new List<string>();

        static private System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<object>> PermutationList;
        static private System.Collections.Generic.List<string> SelectClauses;

        public Summarize(IAnalysisStatisticContext AnalysisStatisticContext) 
        {
            Construct(AnalysisStatisticContext);
        }

        public string Name { get { return "Epi.Analysis.Statistics.Summarize"; } }
        public void Construct(IAnalysisStatisticContext AnalysisStatisticContext)
        {
            Context = AnalysisStatisticContext;
            aggregateList = Context.InputVariableList["AggregateList"].Split(',');
            
            if (Context.InputVariableList.ContainsKey("StratvarList"))
            {
                stratvarList = Context.InputVariableList["StratvarList"].Split(',');
            }
            string[]table = Context.InputVariableList["AggregateElementList"].Split(',');
            
            for(int i = 0; i < table.Length; i++)
            {
                string[] row = table[i].Split('=');
                aggregateElement.Add(row[0].Trim(), row[1].Replace(" ", "").Trim());
            }

            if (Context.InputVariableList.ContainsKey("ParticipatingVariableList"))
            {
                participatingVariableList.AddRange(Context.InputVariableList["ParticipatingVariableList"].Split(','));
            }

            outTableName = Context.InputVariableList["OutTable"];
            weightVar = Context.InputVariableList["WeightVar"];
            commandText = Context.InputVariableList["commandText"];
            currentRead_Identifier = Context.InputVariableList["TableName"];
            commandText = Context.InputVariableList["CommandText"];
        }

        public void Execute()
        {
            StringBuilder columnList = new StringBuilder();
            StringBuilder groupByList = new StringBuilder();
            cDataSetHelper dsHelper = new cDataSetHelper();
            System.Data.DataTable dataTable = null;

            if (stratvarList == null || stratvarList.Length == 0)
            {
                dataTable = new DataTable(outTableName);
                object[] a = new object[aggregateList.Length];

                foreach (string aggregate in aggregateList)
                {
                    if (aggregateElement.ContainsKey(aggregate))
                    {
                        columnList.Append(aggregateElement[aggregate]);
                        columnList.Append(" ");
                        columnList.Append(aggregate);
                        columnList.Append(", ");
                    }
                    else
                    {
                        columnList.Append(aggregate);
                        columnList.Append(", ");
                    }
                }

                columnList.Length = columnList.Length - 2;

                dataTable = dsHelper.CreateGroupByTable(
                    outTableName,
                    Context.Columns,
                    Context.GetDataRows(null),
                    columnList.ToString(),
                    true
                    );

                dsHelper.InsertGroupByInto(
                    dataTable,
                    Context.Columns,
                    Context.GetDataRows(participatingVariableList),
                    columnList.ToString(),
                    null,
                    groupByList.ToString());

                // Dec. 9, 2016
                // Replace the values in the dataTable.Rows created by the above method,
                // which limits the working dataset to rows where all participating variables
                // are non-missing, with values from a dataset that does not impose this limit.
                for (int e = 0; e < participatingVariableList.Count; e++)
                {
                    // Compute the summary statistics one at a time.
                    // Working dataset is limited to rows with non-missing values
                    // for just the one variable being summarized
                    List<string> subList = new List<string>();
                    subList.Add(participatingVariableList[e]);
                    dsHelper.InsertGroupByInto(
                        dataTable,
                        Context.Columns,
                        Context.GetDataRows(subList),
                        columnList.ToString().Split(',')[e],
                        null,
                        groupByList.ToString());

                    dataTable.Rows[0][e] = dataTable.Rows[dataTable.Rows.Count - 1][e];
                    dataTable.Rows.RemoveAt(dataTable.Rows.Count - 1);
                }
                // End of Dec. 9, 2016 change.
            }
            else
            {
                participatingVariableList.AddRange(stratvarList);

                foreach (string stratvar in stratvarList)
                {
                    groupByList.Append(stratvar);
                    groupByList.Append(", ");
                }

                columnList.Append(groupByList);
                groupByList.Length = groupByList.Length - 2;

                foreach (string aggregate in aggregateList)
                {
                    if (aggregateElement.ContainsKey(aggregate))
                    {
                        columnList.Append(aggregateElement[aggregate]);
                        columnList.Append(" ");
                        columnList.Append(aggregate);
                        columnList.Append(", ");
                    }
                    else
                    {
                        columnList.Append(aggregate);
                        columnList.Append(", ");
                    }
                }

                columnList.Length = columnList.Length - 2;
                
                dataTable = dsHelper.CreateGroupByTable(
                    outTableName, 
                    Context.Columns, 
                    Context.GetDataRows(null), 
                    columnList.ToString(),
                    true
                    );

                // Dec. 9, 2016
                // Replace the values in the dataTable.Rows created by the above method,
                // which limits the working dataset to rows where all participating variables
                // are non-missing, with values from a dataset that does not impose this limit.

                // First, allow missing values for the by-variables to be included
                List<string> participatingVariablesMinusStrataVariablesList = new List<string>();
                for (int e = 0; e < participatingVariableList.Count - stratvarList.Length; e++)
                    participatingVariablesMinusStrataVariablesList.Add(participatingVariableList[e]);

                dsHelper.InsertGroupByInto(
                    dataTable, 
                    Context.Columns, 
                    Context.GetDataRows(participatingVariablesMinusStrataVariablesList), 
                    columnList.ToString(), 
                    null, 
                    groupByList.ToString());

                // Start a column list string with the by-variables
                int numberOfRows = dataTable.Rows.Count;
                string[] columnArray = columnList.ToString().Split(',');
                string newColumnList = columnArray[0];
                for (int e = 1; e < stratvarList.Length; e++)
                    newColumnList = newColumnList + ", " + columnArray[e];

                for (int e = 0; e < participatingVariableList.Count - stratvarList.Length; e++)
                {
                    // Similar process to non-stratified section but with more than one row per variable
                    List<string> subList = new List<string>();
                    subList.Add(participatingVariableList[e]);
                    dsHelper.InsertGroupByInto(
                        dataTable,
                        Context.Columns,
                        Context.GetDataRows(subList),
                        newColumnList + ", " + columnList.ToString().Split(',')[e + stratvarList.Length],
                        null,
                        groupByList.ToString());

                    for (int z = numberOfRows - 1; z >= 0; z--)
                    {
                        dataTable.Rows[z][e + stratvarList.Length] = dataTable.Rows[z + numberOfRows][e + stratvarList.Length];
                        dataTable.Rows.RemoveAt(z + numberOfRows);
                    }
                }
                // End of Dec. 9, 2016 change.
            }

            Context.OutTable(dataTable);

            StringBuilder HTMLString = new StringBuilder();
            HTMLString.Append("<table><tr>");
            foreach (DataColumn c in dataTable.Columns)
            {
                //System.Console.WriteLine("{0}\t{1}\t{2}", r[0], r[1], r[2]);
                //System.Console.WriteLine("{0}\t{1}\t{2}\t{3}", r[0], r[1], r[2], r[3]);
                System.Console.Write("{0}\t", c.ColumnName);
                HTMLString.Append("<th>");
                HTMLString.Append(c.ColumnName);
                HTMLString.Append("</th>");
            }
            
            System.Console.Write("\n");
            HTMLString.Append("</tr>");

            foreach (DataRow r in dataTable.Rows)
            {
                HTMLString.Append("<tr>");
                foreach (DataColumn c in dataTable.Columns)
                {
                    //System.Console.WriteLine("{0}\t{1}\t{2}", r[0], r[1], r[2]);
                    //System.Console.WriteLine("{0}\t{1}\t{2}\t{3}", r[0], r[1], r[2], r[3]);
                    System.Console.Write("{0}\t", r[c.ColumnName]);
                    HTMLString.Append("<td>");
                    HTMLString.Append(r[c.ColumnName]);
                    HTMLString.Append("</td>");
                }
                System.Console.Write("\n");
                HTMLString.Append("</tr>");
            }
            
            HTMLString.Append("</table>");

            Dictionary<string, string> config = Context.SetProperties;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("COMMANDNAME", "SUMMARIZE");
            args.Add("COMMANDTEXT", commandText.Trim());
            args.Add("HTMLRESULTS", HTMLString.ToString());

            Context.Display(args);
        }
    }
}
