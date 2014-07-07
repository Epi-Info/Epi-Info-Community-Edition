using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using EpiInfo.Plugin;
using Epi.Data;


namespace Epi.Analysis.Statistics
{
    public class Match : IAnalysisStatistic
    {


        string[] AggregateList = null;
        Dictionary<string, string> AggregateElement = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string[] StratvarList = null;
        string OutTable = null;
        string WeightVar = null;
        string commandText = string.Empty;
        string CurrentRead_Identifier = string.Empty;
        IAnalysisStatisticContext Context;


        static private System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<object>> PermutationList;
        static private System.Collections.Generic.List<string> SelectClauses;

        public Match(IAnalysisStatisticContext AnalysisStatisticContext) 
        {
            this.Construct(AnalysisStatisticContext);
        
        }

        public string Name { get { return "Epi.Analysis.Statistics.Summarize"; } }
        public void Construct(IAnalysisStatisticContext AnalysisStatisticContext)
        {
            this.Context = AnalysisStatisticContext;
            //AggregateElementList
            this.AggregateList = this.Context.InputVariableList["AggregateList"].Split(',');
            if (this.Context.InputVariableList.ContainsKey("StratvarList"))
            {
                this.StratvarList = this.Context.InputVariableList["StratvarList"].Split(',');
            }
            string[]table = this.Context.InputVariableList["AggregateElementList"].Split(',');
            for(int i = 0; i < table.Length; i++)
            {
                string[] row = table[i].Split('=');
                this.AggregateElement.Add(row[0].Trim(), row[1].Replace(" ","").Trim());
            }

            
            this.OutTable = this.Context.InputVariableList["OutTable"];
            this.WeightVar = this.Context.InputVariableList["WeightVar"];
            this.commandText = this.Context.InputVariableList["commandText"];
            this.CurrentRead_Identifier = this.Context.InputVariableList["TableName"];
            this.commandText = this.Context.InputVariableList["CommandText"];

        }

        public void Execute()
        {
            // READ 'C:\temp\Sample.mdb':Oswego
            // SUMMARIZE FirstVisit::MIN(VisitDate), LastVisit::MAX(VisitDate), NVisit::COUNT() TO PatSumm STRATAVAR=PatientID

            //dt = dsHelper.CreateGroupByTable("OrderSummary", ds.Tables["Orders"], "EmployeeID,sum(Amount) Total,min(Amount) Min,max(Amount) Max");

            //READ {Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\temp\Sample.mdb}:Addicts 
            //SUMMARIZE MinTime::MIN(Survival_Time_Days), MaxTime::MAX(Survival_Time_Days) TO PatSumm STRATAVAR=clinic
            // OutTable Structure Clinic, MinTime, MaxTime
            StringBuilder columnList = new StringBuilder();
            StringBuilder groupByList = new StringBuilder();
            cDataSetHelper dsHelper = new cDataSetHelper();
            System.Data.DataTable dt = null;
            List<DataRow> DataRows = this.Context.GetDataRows(null);

            if (this.StratvarList == null || this.StratvarList.Length == 0)
            {
                dt = new DataTable(this.OutTable);
                object[] a = new object[this.AggregateList.Length];
                for (int i = 0; i < this.AggregateList.Length; i++)
                {
                    string s = this.AggregateList[i];

                    DataColumn c = new DataColumn(s, typeof(double), this.AggregateElement[s]);
                    DataColumn c2 = new DataColumn(s, typeof(double));
                    this.Context.Columns.Add(c);
                    dt.Columns.Add(c2);
                    a[i] = DataRows[0][c.ColumnName];
                }

                //dt.Rows.Add(new object[] { "Sam", 5, 25.00 });
                dt.Rows.Add(a);
            }
            else
            {
                foreach (string s in this.StratvarList)
                {
                    groupByList.Append(s);
                    groupByList.Append(", ");
                }

                columnList.Append(groupByList);
                groupByList.Length = groupByList.Length - 2;
                
                
                foreach (string s in this.AggregateList)
                {
                    if(this.AggregateElement.ContainsKey(s))
                    {
                        columnList.Append(this.AggregateElement[s]);
                        columnList.Append(" ");
                        columnList.Append(s);
                        columnList.Append(", ");
                    }
                    else
                    {
                        columnList.Append(s);
                        columnList.Append(", ");
                    }

                    //groupByList.Append(s);
                    //groupByList.Append(" ");
                }

                columnList.Length = columnList.Length - 2;
                

                
                //READ {Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\temp\Sample.mdb}:Addicts 
                //SUMMARIZE MinTime::MIN(Survival_Time_Days), MaxTime::MAX(Survival_Time_Days) TO PatSumm 
    
                
                //System.Data.DataTable dt = dsHelper.CreateGroupByTable(this.OutTable, this.Context.CurrentDataTable, columnList.ToString());
                //dsHelper.InsertGroupByInto(dt, this.Context.CurrentDataTable, columnList.ToString(), null, "clinic");


                //System.Data.DataTable dt = dsHelper.CreateGroupByTable(this.OutTable, this.Context.CurrentDataTable, "MIN(Survival_Time_Days) MinTime, MAX(Survival_Time_Days) MaxTime");
                //dsHelper.InsertGroupByInto(dt, this.Context.CurrentDataTable, "MIN(Survival_Time_Days) MinTime, MAX(Survival_Time_Days) MaxTime", null, "Survival_Time_Days");


                /*
                READ {Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\temp\Sample.mdb}:Oswego 
                SUMMARIZE Age::Count() TO PatSumm STRATAVAR=Age BakedHam Ill
                */
                //"Age, Count(Age) Freq"
                dt = dsHelper.CreateGroupByTable(this.OutTable, this.Context.Columns, DataRows, columnList.ToString());

                dsHelper.InsertGroupByInto(dt, this.Context.Columns, DataRows, columnList.ToString(), null, groupByList.ToString());

                /*
                System.Data.DataTable dt = dsHelper.CreateGroupByTable(this.OutTable, this.Context.CurrentDataTable, "Age");
                dsHelper.InsertGroupByInto(dt, this.Context.CurrentDataTable, "Age", null, "Age");

                dt.Columns.Add("Freq", typeof(double), "Count(Age)");*/
                //dt.Columns["ageCount"].Expression = ;

                //


/*                
READ {Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\temp\Sample.mdb}:Oswego 
SUMMARIZE Age :: Avg(AGE) TO patsumm STRATAVAR=ILL 



SUMMARIZE Age :: Avg(AGE), AgeCount :: Count(Age) TO patsumm STRATAVAR=ILL 
 */

            }


            this.Context.OutTable(dt);

            StringBuilder HTMLString = new StringBuilder();
            HTMLString.Append("<table><tr>");
            foreach (DataColumn c in dt.Columns)
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
            foreach (DataRow r in dt.Rows)
            {
                HTMLString.Append("<tr>");
                foreach (DataColumn c in dt.Columns)
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

            Dictionary<string,string> config = this.Context.SetProperties;

            

            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("COMMANDNAME", "SUMMARIZE");
            args.Add("COMMANDTEXT", commandText.Trim());
            //args.Add("FREQVARIABLE", this.Identifier);
            args.Add("HTMLRESULTS", HTMLString.ToString());


            this.Context.Display(args);

        }

    }

 



}
