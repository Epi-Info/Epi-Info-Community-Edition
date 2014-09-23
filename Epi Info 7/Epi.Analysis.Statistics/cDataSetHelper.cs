using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Epi.Data
{
    // http://support.microsoft.com/kb/326145 DataSet GROUP BY 
    //dt = dsHelper.CreateGroupByTable("OrderSummary", ds.Tables["Orders"], "EmployeeID,sum(Amount) Total,min(Amount) Min,max(Amount) Max");
    //dsHelper.InsertGroupByInto(ds.Tables["OrderSummary"], ds.Tables["Orders"], "EmployeeID,sum(Amount) Total,min(Amount) Min,max(Amount) Max", "EmployeeID<5", "EmployeeID");
    //dt = dsHelper.SelectGroupByInto("OrderSummary", ds.Tables["Employees"], "EmployeeID,sum(Amount) Total,min(Amount) Min,max(Amount) Max", "EmployeeID<5", "EmployeeID");




    // http://support.microsoft.com/kb/326009/en-us  DataSet SELECT INTO 
    //dt = dsHelper.CreateTable("TestTable", ds.Tables["Employees"], "FirstName FName,LastName LName,BirthDate");
    //dsHelper.InsertInto(ds.Tables["TestTable"], ds.Tables["Employees"], "FirstName FName,LastName LName,BirthDate", "EmployeeID<5", "BirthDate") ;
    //dt = dsHelper.SelectInto("TestTable", ds.Tables["Employees"], "FirstName FName,LastName LName,BirthDate", "EmployeeID<5", "BirthDate") ;




    // http://support.microsoft.com/kb/326080/en-us DataSet JOIN 
    //http://support.microsoft.com/kb/326080
    //dt = dsHelper.CreateJoinTable("TestTable", ds.Tables["Employees"], "FirstName FName,LastName LName,DepartmentEmployee.DepartmentName Department");
    //dsHelper.InsertJoinInto(ds.Tables["TestTable"], ds.Tables["Employees"], "FirstName FName,LastName LName,DepartmentEmployee.DepartmentName Department", "EmployeeID<5", "BirthDate");
    // dt = dsHelper.SelectInto("TestTable", ds.Tables["Employees"], "FirstName FName,LastName LName,DepartmentEmployee.DepartmentName Department", "EmployeeID<5", "BirthDate");


    // http://support.microsoft.com/kb/326176/en-us Select DISTINCT
    //dsHelper.SelectDistinct("DistinctEmployees", ds.Tables["Orders"], "EmployeeID");

    // http://msdn.microsoft.com/en-us/library/system.data.datatable.compute.aspx DataTable.Compute Method
    /*
        private void ComputeBySalesSalesID(DataSet dataSet)
        {
            // Presumes a DataTable named "Orders" that has a column named "Total."
            DataTable table;
            table = dataSet.Tables["Orders"];

            // Declare an object variable.
            object sumObject;
            sumObject = table.Compute("Sum(Total)", "EmpID = 5");
        }
    */



    public class cDataSetHelper
    {
            public DataSet ds;
            private System.Collections.ArrayList m_FieldInfo;
            private string m_FieldList;
            private System.Collections.ArrayList GroupByFieldInfo;
            private string GroupByFieldList;

            public System.Collections.Generic.Dictionary<string, int> ValueCount;
            public double Median;
            public string Mode;
            public double m25;
            public double m75;
            public string Main_Variable;
            public List<object> GroupMedian;
            public List<object> Group25;
            public List<object> Group75;
            public List<string> GroupMode;

            private class FieldInfo
            {
                public string RelationName;
                public string FieldName;	//source table field name
                public string FieldAlias;	//destination table field name
                public string Aggregate;
            }


            public cDataSetHelper(ref DataSet DataSet)
            {
                ds = DataSet;
            }

            public cDataSetHelper()
            {
                ds = null;
            }

            private void ParseFieldList(string FieldList, bool AllowRelation)
            {
                /*
                 * This code parses FieldList into FieldInfo objects  and then 
                 * adds them to the m_FieldInfo private member
                 * 
                 * FieldList systax:  [relationname.]fieldname[ alias], ...
                */
                if (m_FieldList == FieldList) return;
                m_FieldInfo = new System.Collections.ArrayList();
                m_FieldList = FieldList;
                FieldInfo Field; string[] FieldParts; string[] Fields = FieldList.Split(',');
                int i;
                for (i = 0; i <= Fields.Length - 1; i++)
                {
                    Field = new FieldInfo();
                    //parse FieldAlias
                    FieldParts = Fields[i].Trim().Split(' ');
                    switch (FieldParts.Length)
                    {
                        case 1:
                            //to be set at the end of the loop
                            break;
                        case 2:
                            Field.FieldAlias = FieldParts[1];
                            break;
                        default:
                            throw new Exception("Too many spaces in field definition: '" + Fields[i] + "'.");
                    }
                    //parse FieldName and RelationName
                    FieldParts = FieldParts[0].Split('.');
                    switch (FieldParts.Length)
                    {
                        case 1:
                            Field.FieldName = FieldParts[0];
                            break;
                        case 2:
                            if (AllowRelation == false)
                                throw new Exception("Relation specifiers not permitted in field list: '" + Fields[i] + "'.");
                            Field.RelationName = FieldParts[0].Trim();
                            Field.FieldName = FieldParts[1].Trim();
                            break;
                        default:
                            throw new Exception("Invalid field definition: " + Fields[i] + "'.");
                    }
                    if (Field.FieldAlias == null)
                        Field.FieldAlias = Field.FieldName;
                    m_FieldInfo.Add(Field);
                }
            }

            private void ParseGroupByFieldList(string FieldList)
            {
                /*
                * Parses FieldList into FieldInfo objects and adds them to the GroupByFieldInfo private member
                * 
                * FieldList syntax: fieldname[ alias]|operatorname(fieldname)[ alias],...
                * 
                * Supported Operators: count,sum,max,min,first,last
                */
                if (GroupByFieldList == FieldList) return;
                GroupByFieldInfo = new System.Collections.ArrayList();
                FieldInfo Field; string[] FieldParts; string[] Fields = FieldList.Split(',');
                for (int i = 0; i <= Fields.Length - 1; i++)
                {
                    Field = new FieldInfo();
                    //Parse FieldAlias
                    FieldParts = Fields[i].Trim().Split(' ');
                    switch (FieldParts.Length)
                    {
                        case 1:
                            //to be set at the end of the loop
                            break;
                        case 2:
                            Field.FieldAlias = FieldParts[1];
                            break;
                        default:
                            throw new ArgumentException("Too many spaces in field definition: '" + Fields[i] + "'.");
                    }
                    //Parse FieldName and Aggregate
                    FieldParts = FieldParts[0].Split('(');
                    switch (FieldParts.Length)
                    {
                        case 1:
                            Field.FieldName = FieldParts[0];
                            break;
                        case 2:
                            Field.Aggregate = FieldParts[0].Trim().ToLower();    //we're doing a case-sensitive comparison later
                            Field.FieldName = FieldParts[1].Trim(' ', ')');
                            break;
                        default:
                            throw new ArgumentException("Invalid field definition: '" + Fields[i] + "'.");
                    }
                    if (Field.FieldAlias == null)
                    {
                        if (Field.Aggregate == null)
                            Field.FieldAlias = Field.FieldName;
                        else
                            Field.FieldAlias = Field.Aggregate + "of" + Field.FieldName;
                    }
                    GroupByFieldInfo.Add(Field);
                }
                GroupByFieldList = FieldList;
            }

            public DataTable CreateGroupByTable(string tableName, DataColumnCollection columns, List<DataRow> sourceTable, string fieldList)
            {
                /*
                 * Creates a table based on aggregates of fields of another table
                 * 
                 * RowFilter affects rows before GroupBy operation. No "Having" support
                 * though this can be emulated by subsequent filtering of the table that results
                 * 
                 *  FieldList syntax: fieldname[ alias]|aggregatefunction(fieldname)[ alias], ...
                */
                if (fieldList == null)
                {
                    throw new ArgumentException("You must specify at least one field in the field list.");
                }
                else
                {
                    DataTable table = new DataTable(tableName);
                    ParseGroupByFieldList(fieldList);
                    
                    foreach (FieldInfo Field in GroupByFieldInfo)
                    {
                        DataColumn column = columns[Field.FieldName];
                        if (column != null && Field.Aggregate == null)
                        {
                            table.Columns.Add(Field.FieldAlias, column.DataType, column.Expression);
                        }
                        else
                        {
                            if (column == null)
                            {
                                table.Columns.Add(Field.FieldAlias, typeof(double));
                            }
                            else
                            {
                                if (Field.Aggregate != null)
                                {
                                    table.Columns.Add(Field.FieldAlias, typeof(double));
                                }
                                else
                                {
                                    table.Columns.Add(Field.FieldAlias, column.DataType);
                                }
                            }
                        }
                    }

                    if (ds != null)
                    {
                        ds.Tables.Add(table);
                    }

                    return table;
                }
            }

            public DataTable CreateGroupByTable(string TableName, DataTable SourceTable, string FieldList)
            {
                /*
                 * Creates a table based on aggregates of fields of another table
                 * 
                 * RowFilter affects rows before GroupBy operation. No "Having" support
                 * though this can be emulated by subsequent filtering of the table that results
                 * 
                 *  FieldList syntax: fieldname[ alias]|aggregatefunction(fieldname)[ alias], ...
                */
                if (FieldList == null)
                {
                    throw new ArgumentException("You must specify at least one field in the field list.");
                    //return CreateTable(TableName, SourceTable);
                }
                else
                {
                    DataTable dt = new DataTable(TableName);
                    ParseGroupByFieldList(FieldList);
                    foreach (FieldInfo Field in GroupByFieldInfo)
                    {
                        DataColumn dc = SourceTable.Columns[Field.FieldName];
                        if (dc != null && Field.Aggregate == null)
                        {
                            dt.Columns.Add(Field.FieldAlias, dc.DataType, dc.Expression);
                        }
                        else
                        {
                            if (dc == null)
                            {
                                //dt.Columns.Add(Field.FieldAlias, typeof(double));
                                dt.Columns.Add(Field.FieldAlias, typeof(object));
                            }
                            else
                            {
                                if (Field.Aggregate != null)
                                {
                                    //dt.Columns.Add(Field.FieldAlias, typeof(double));
                                    dt.Columns.Add(Field.FieldAlias, typeof(object));
                                }
                                else
                                {
                                    dt.Columns.Add(Field.FieldAlias, dc.DataType);
                                }
                            }
                        }
                    }                    

                    if (ds != null)
                        ds.Tables.Add(dt);
                    return dt;
                }
            }

            public void InsertGroupByInto
                (
                DataTable DestTable,
                DataColumnCollection Columns,
                List<DataRow> SourceRows,
                string FieldList,
                string RowFilter,
                string GroupBy
                )
            {
                /*
                 * Copies the selected rows and columns from SourceTable and inserts them into DestTable
                 * FieldList has same format as CreateGroupByTable
                */
                if (FieldList == null)
                    throw new ArgumentException("You must specify at least one field in the field list.");
                ParseGroupByFieldList(FieldList);	//parse field list
                ParseFieldList(GroupBy, false);			//parse field names to Group By into an arraylist
                List<DataRow> DestRows = new List<DataRow>();
                if (string.IsNullOrEmpty(GroupBy))
                {
                    if (Main_Variable != null)
                    {
                        DestRows = SortRows(SourceRows, Columns, Main_Variable);
                        SourceRows.Clear();
                        foreach (DataRow row in DestRows)
                        {
                            SourceRows.Add(row);
                        }
                    }                
                }
                else
                {
                    if (!string.IsNullOrEmpty(GroupBy.Split(',')[0]))
                    {
                        Main_Variable = GroupBy.Split(',')[0];
                    }
                    if (string.IsNullOrEmpty(this.Main_Variable))
                    {
                        String[] valuelist;
                        valuelist = GroupBy.Split(',');
                        foreach (string value in valuelist)
                        {
                            DestRows = SortRows(SourceRows, Columns, value);
                        }
                        SourceRows.Clear();
                        foreach (DataRow row in DestRows)
                        {
                            SourceRows.Add(row);
                        }       
                    }
                    else
                    {
                        SourceRows.Sort(new cDataRowComparer(this.Main_Variable + ", " + GroupBy));
                        String[] valuelist;
                        valuelist = GroupBy.Split(',');
                        foreach (string value in valuelist)
                        {
                            DestRows = SortRows(SourceRows, Columns, value);
                        }
                        DestRows = SortRows(SourceRows, Columns, Main_Variable);
                        SourceRows.Clear();
                        foreach (DataRow row in DestRows)
                        {
                            SourceRows.Add(row);
                        }    
                    }
                }

                if (SourceRows.Count == 0)
                {
                    return;
                }
                this.ValueCount = new Dictionary<string, int>();
                // ******** Statistics - Start               
                if (Main_Variable != null)
                {
                    if (Columns[Main_Variable].DataType == typeof(double) || Columns[Main_Variable].DataType == typeof(int) || Columns[Main_Variable].DataType == typeof(byte) || Columns[Main_Variable].DataType == typeof(long) || Columns[Main_Variable].DataType == typeof(float) || Columns[Main_Variable].DataType == typeof(decimal))
                    {
                        this.Median = this.GetMedian(SourceRows, 0, SourceRows.Count);
                        int half = SourceRows.Count / 2;

                        if (SourceRows.Count % 2 == 0)
                        {
                            this.m25 = this.GetMedian(SourceRows, 0, half);
                            this.m75 = this.GetMedian(SourceRows, SourceRows.Count - half, SourceRows.Count);
                        }
                        else
                        {
                            this.m25 = this.GetMedian(SourceRows, 0, half + 1);
                            this.m75 = this.GetMedian(SourceRows, SourceRows.Count - half - 1, SourceRows.Count);
                        }
                    }
                }
                // ******** Statistics - End

                DataRow LastSourceRow = null, DestRow = null; bool SameRow; int RowCount = 0; int GroupStart = 0;

                Dictionary<string, object> AccumulatedSum = new Dictionary<string, object>();
                Dictionary<string, object> AccumulatedSquareOfSum = new Dictionary<string, object>();
                Dictionary<string, object> AccumulatedMean = new Dictionary<string, object>();

                this.GroupMedian = new List<object>();
                this.Group25 = new List<object>();
                this.Group75 = new List<object>();
                this.GroupMode = new List<string>();
                Dictionary<string, int> GroupValueCount = new Dictionary<string, int>();


                Dictionary<string, Dictionary<string, object>> AccumlatorSet = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
                AccumlatorSet.Add("AccumulatedSum", AccumulatedSum);
                AccumlatorSet.Add("AccumulatedSquareOfSum", AccumulatedSquareOfSum);
                AccumlatorSet.Add("AccumulatedMean",AccumulatedMean);
                foreach (FieldInfo Field in GroupByFieldInfo)
                {
                    AccumlatorSet["AccumulatedSum"].Add(Field.FieldAlias, 0.0);
                    AccumlatorSet["AccumulatedSquareOfSum"].Add(Field.FieldAlias, 0.0);
                    AccumlatorSet["AccumulatedMean"].Add(Field.FieldAlias, 0.0);
                }


                foreach (DataRow SourceRow in SourceRows)
                {

                    // Calculations for eventually determining the mode
                    if (Main_Variable != null)
                    {
                        if (SourceRow[Main_Variable] != DBNull.Value)
                        {
                            string val = SourceRow[Main_Variable].ToString();
                            if (this.ValueCount.ContainsKey(val))
                            {
                                this.ValueCount[val]++;
                            }
                            else
                            {
                                this.ValueCount.Add(val, 1);
                            }


                            if (GroupValueCount.ContainsKey(val))
                            {
                                GroupValueCount[val]++;
                            }
                            else
                            {
                                GroupValueCount.Add(val, 1);
                            }

                        }
                    }
                    

                    SameRow = false;
                    if (LastSourceRow != null)
                    {
                        SameRow = true;
                        
                        foreach (FieldInfo Field in m_FieldInfo)
                        {
                            if (String.IsNullOrEmpty(Field.Aggregate) && !string.IsNullOrEmpty(Field.FieldName))
                            {
                                if (!ColumnEqual(LastSourceRow[Field.FieldName], SourceRow[Field.FieldName]))
                                {
                                    SameRow = false;
                                    break;
                                }
                            }
                        }
                        // start - finalize
                        if (!SameRow)
                        {
                            if (Main_Variable != null)
                            {
                                if (Columns[Main_Variable].DataType == typeof(double) || Columns[Main_Variable].DataType == typeof(int) || Columns[Main_Variable].DataType == typeof(byte) || Columns[Main_Variable].DataType == typeof(long) || Columns[Main_Variable].DataType == typeof(float) || Columns[Main_Variable].DataType == typeof(decimal))
                                {
                                    finalizeGroup(GroupStart, RowCount, SourceRows, GroupMedian, Group25, Group75, GroupMode, GroupValueCount);

                                    GroupStart = RowCount;
                                    GroupValueCount.Clear();
                                }
                            }

                            this.finalizeRow(RowCount, DestRow, SourceRow, AccumlatorSet);
                            DestTable.Rows.Add(DestRow);
                        }
                        // end - finalize
                        
                    }
                    // start - initialize
                    if (!SameRow)
                    {
                        

                        DestRow = DestTable.NewRow();
                        RowCount = 0;
                        foreach (FieldInfo Field in GroupByFieldInfo)
                        {
                            AccumlatorSet["AccumulatedSum"][Field.FieldAlias] = 0.0;
                            AccumlatorSet["AccumulatedSquareOfSum"][Field.FieldAlias] = 0.0;
                            AccumlatorSet["AccumulatedMean"][Field.FieldAlias] = 0.0;
                        }


                    }
                    RowCount += 1;
                    // end - initialize

                    
                    this.ProcessRow(RowCount, DestRow, SourceRow, AccumlatorSet);

                    LastSourceRow = SourceRow;
                    
                }
                if (DestRow != null)
                {
                     this.finalizeRow(RowCount, DestRow, LastSourceRow, AccumlatorSet);
                    DestTable.Rows.Add(DestRow);
                }


                // finish Mode calculation
                if (Main_Variable != null)
                {
                    KeyValuePair<string, int> kvp = this.ValueCount.ElementAt(0);
                    for (int i = 0; i < this.ValueCount.Count; i++)
                    {
                        if (this.ValueCount.ElementAt(i).Value > kvp.Value)
                        {
                            kvp = this.ValueCount.ElementAt(i);
                        }
                    }
                    this.Mode = kvp.Key;
                }         
            }

            private List<DataRow> SortRows(List<DataRow> Sourcerows, DataColumnCollection Columns, string groupby)
            {
                List<DataRow> DestRows = new List<DataRow>();
                DataTable datatable = new DataTable();
                foreach (DataColumn column in Columns)
                {
                    DataColumn newColumn = new DataColumn(column.ColumnName);
                    newColumn.DataType = column.DataType;
                    datatable.Columns.Add(newColumn);
                }
                foreach (DataRow row in Sourcerows)
                {
                    datatable.ImportRow(row);
                }
                DataRow[] Sortedrows = null;
                Sortedrows = datatable.Select("", groupby);
                foreach (DataRow r in Sortedrows)
                {
                    DestRows.Add(r);
                }
                return DestRows;
            }

        private FieldInfo LocateFieldInfoByName(System.Collections.ArrayList FieldList, string Name)
        {
            //Looks up a FieldInfo record based on FieldName
            foreach (FieldInfo Field in FieldList)
            {
                if (Field.FieldName==Name)
                    return Field;
            }
            return null;
        }

        private bool ColumnEqual(object a, object b)
        {
            /*
             * Compares two values to see if they are equal. Also compares DBNULL.Value.
             * 
             * Note: If your DataTable contains object fields, you must extend this
             * function to handle them in a meaningful way if you intend to group on them.
            */ 
            if ((a is DBNull) && (b is DBNull))
                return true;    //both are null
            if ((a is DBNull) || (b is DBNull))
                return false;    //only one is null
            //return (a==b);    //value type standard comparison
            return (a.Equals(b));    //value type standard comparison
        }   

        private object Min(object a, object b)
        {
            //Returns MIN of two values - DBNull is less than all others
            

            if ((a is DBNull) || (b is DBNull))
                return DBNull.Value;

            if (a is Double || b is Double)
            {
                if (((IComparable)Convert.ToDouble(a)).CompareTo(Convert.ToDouble(b)) == -1)
                    return a;
                else
                    return b;
            }
            else
            {
                if (((IComparable)a).CompareTo(b) == -1)
                    return a;
                else
                    return b;
            }
        }
        private double calcVariance(System.Data.DataTable pDT)
        {
            int n = 0;
            double Sum = 0.0;
            double Sum_Sqr = 0.0;

            foreach (System.Data.DataRow R in pDT.Rows)
            {
                n++;
                double x = 0.0;
                Double.TryParse(R["value"].ToString(), out x);

                Sum += x;
                Sum_Sqr += x * x;
            }

            double mean = Sum / n;
            double variance = (Sum_Sqr - Sum * mean) / (n - 1);
            return variance;
        }

        public double StandardDeviation(List<double> num)
        {
            double SumOfSqrs = 0;
            double avg = num.Average();
            for (int i = 0; i < num.Count; i++)
            {
                SumOfSqrs += Math.Pow(((double)num[i] - avg), 2);
            }
            double n = (double)num.Count;
            return Math.Sqrt(SumOfSqrs / (n - 1));
        }

        private double calcStd_Dev(System.Data.DataRow[] pRows, double pMean)
        {
            double result = 0.0;

            //sqrt(sum(((obs.value - mean)^2))/numberOfObs)
            int n = 0;
            double Sum = 0.0;

            foreach (System.Data.DataRow R in pRows)
            {
                n++;
                double x = 0.0;
                Double.TryParse(R["value"].ToString(), out x);

                Sum += Math.Pow(x - pMean, 2);
            }

            result = Math.Sqrt(Sum / n);

            return result;
        }



        public object STDev(object pSumOfSqrs, object pSum, object pCount)
        {
            if ((pSumOfSqrs is DBNull) || (pSum is DBNull) || (pCount is DBNull))
            {
                return DBNull.Value;
            }

            double sum = (double)pSum;
            double count = Convert.ToDouble(pCount);
            double Sum_Sqr = (double)pSumOfSqrs;
            double mean = sum / count;
            double result;
            if (count > 1)
            {
                result = Math.Sqrt((Sum_Sqr - sum * mean) / (count - 1));
            }
            else
            {
                result = Math.Sqrt(Sum_Sqr - sum * mean);
            }

            return result;

        }
        private object STDev(object pSum, object pCount)
        {
            if ((pSum is DBNull) || (pCount is DBNull))
            {
                return DBNull.Value;
            }
           
            double sum = (double) pSum;
            double count = Convert.ToDouble(pCount);
            //double mean = sum / 2.0;
            double result = Math.Sqrt(sum / 2.0);


            //result = Math.Sqrt(((Math.Pow(sum, 2.0)) - ((1.0 / count) * (Math.Pow(sum, 2.0)))) / (count - 1.0));

            return result;
        }



        public object VARiance(object pSumOfSqrs, object pSum, object pCount)
        {
            if ((pSumOfSqrs is DBNull) || (pSum is DBNull) || (pCount is DBNull))
            {
                return DBNull.Value;
            }

            double sum = (double)pSum;
            double count = Convert.ToDouble(pCount);
            double Sum_Sqr = (double)pSumOfSqrs;
            double mean = sum / count;

            double result;
            if (count > 1)
            {
                result = (Sum_Sqr - sum * mean) / (count - 1);
            }
            else
            {
                result = (Sum_Sqr - sum * mean);
            }

            return result;

        }
           

        //private object VARiance(object pSumOfSqrs, object pSum, object pCount)
        private object VARiance(object pSum, object pCount)
        {
            //if ((pSumOfSqrs is DBNull) || (pSum is DBNull) || (pCount is DBNull))
            if ((pSum is DBNull) || (pCount is DBNull))
            {
                return DBNull.Value;
            }

            double sum = (double)pSum;
            double count = Convert.ToDouble(pCount);
            //double Sum_Sqr = (double)pSumOfSqrs;
            double mean = sum / count;
            //double result = (Sum_Sqr - sum * mean) / (count - 1);
            //result = sum / (count - 1);
            //double result = (Sum_Sqr - sum * mean);
            double result = sum / count;

            //result = Math.Sqrt(((Math.Pow(sum, 2.0)) - ((1.0 / count) * (Math.Pow(sum, 2.0)))) / (count - 1.0));

            return result;
        }

        private object Avg(object a, int pCount)
        {
            if ((a is DBNull) || pCount == 0)
            {
                return DBNull.Value;
            }
            else
            {
                return (double)a / (double)pCount;
            }
        }   

        private object Max(object a, object b)
        {
            //Returns Max of two values - DBNull is less than all others
            if (a is DBNull)
                return b;
            if (b is DBNull)
                return a;

            if (a is Double || b is Double)
            {
                if (((IComparable)Convert.ToDouble(a)).CompareTo(Convert.ToDouble(b)) == 1)
                    return a;
                else
                    return b;
            }
            else
            {
                if (((IComparable)a).CompareTo(b) == 1)
                    return a;
                else
                    return b;
            }
        }   

        private object Add(object a, object b)
        {
            //Adds two values - if one is DBNull, then returns the other
            if (a is DBNull)
                return b;
            if (b is DBNull)
                return a;
            //return ((decimal)a + (decimal)b);
            return Convert.ToDouble(a) + Convert.ToDouble(b);
        }

        public DataTable SelectGroupByInto
            (
                string TableName,
                DataTable SourceTable,
                string FieldList,
                string RowFilter,
                string GroupBy
            )
        {
            /*
             * Selects data from one DataTable to another and performs various aggregate functions
             * along the way. See InsertGroupByInto and ParseGroupByFieldList for supported aggregate functions.
             */
            ValueCount = new Dictionary<string, int>();
            DataTable dt = CreateGroupByTable(TableName, SourceTable, FieldList);
            List<DataRow> Rows =  new List<DataRow>(SourceTable.Select(""));
            InsertGroupByInto(dt, SourceTable.Columns, Rows, FieldList, RowFilter, GroupBy);
            return dt;
        }


        public DataTable CreateTable(string TableName, DataTable SourceTable, string FieldList)
        {
            /*
             * This code creates a DataTable by using the SourceTable as a template and creates the fields in the
             * order that is specified in the FieldList. If the FieldList is blank, the code uses DataTable.Clone().
            */
            DataTable dt;
            if (FieldList.Trim() == "")
            {
                dt = SourceTable.Clone();
                dt.TableName = TableName;
            }
            else
            {
                dt = new DataTable(TableName);
                ParseFieldList(FieldList, false);
                DataColumn dc;
                foreach (FieldInfo Field in m_FieldInfo)
                {
                    dc = SourceTable.Columns[Field.FieldName];
                    dt.Columns.Add(Field.FieldAlias, dc.DataType);
                }
            }
            if (ds != null)
                ds.Tables.Add(dt);
            return dt;
        }

        public void InsertInto
            (
                DataTable DestTable,
                DataTable SourceTable,
                string FieldList,
                string RowFilter,
                string Sort
            )
        {
            // 
            // This code copies the selected rows and columns from SourceTable and inserts them into DestTable.
            // 
            ParseFieldList(FieldList, false);
            DataRow[] Rows = SourceTable.Select(RowFilter, Sort);
            DataRow DestRow;
            foreach (DataRow SourceRow in Rows)
            {
                DestRow = DestTable.NewRow();
                if (FieldList == "")
                {
                    foreach (DataColumn dc in DestRow.Table.Columns)
                    {
                        if (dc.Expression == "")
                            DestRow[dc] = SourceRow[dc.ColumnName];
                    }
                }
                else
                {
                    foreach (FieldInfo Field in m_FieldInfo)
                    {
                        DestRow[Field.FieldAlias] = SourceRow[Field.FieldName];
                    }
                }
                DestTable.Rows.Add(DestRow);
            }
        }


        public DataTable SelectInto
            (
                string TableName,
                DataTable SourceTable,
                string FieldList,
                string RowFilter,
                string Sort
            )
        {
            /*
             *  This code selects values that are sorted and filtered from one DataTable into another.
             *  The FieldList specifies which fields are to be copied.
            */
            DataTable dt = CreateTable(TableName, SourceTable, FieldList);
            InsertInto(dt, SourceTable, FieldList, RowFilter, Sort);
            return dt;
        }


        public DataTable CreateJoinTable(string TableName, DataTable SourceTable, string FieldList)
        {
            /*
             * Creates a table based on fields of another table and related parent tables
             * 
             * FieldList syntax: [relationname.]fieldname[ alias][,[relationname.]fieldname[ alias]]...
            */
            if (FieldList == null)
            {
                throw new ArgumentException("You must specify at least one field in the field list.");
                //return CreateTable(TableName, SourceTable);
            }
            else
            {
                DataTable dt = new DataTable(TableName);
                ParseFieldList(FieldList, true);
                foreach (FieldInfo Field in m_FieldInfo)
                {
                    if (Field.RelationName == null)
                    {
                        DataColumn dc = SourceTable.Columns[Field.FieldName];
                        dt.Columns.Add(dc.ColumnName, dc.DataType, dc.Expression);
                    }
                    else
                    {
                        DataColumn dc = SourceTable.ParentRelations[Field.RelationName].ParentTable.Columns[Field.FieldName];
                        dt.Columns.Add(dc.ColumnName, dc.DataType, dc.Expression);
                    }
                }
                if (ds != null)
                    ds.Tables.Add(dt);
                return dt;
            }
        }


        public DataTable SelectJoinInto(string TableName, DataTable SourceTable, string FieldList, string RowFilter, string Sort)
        {
            /*
             * Selects sorted, filtered values from one DataTable to another.
             * Allows you to specify relationname.fieldname in the FieldList to include fields from
             *  a parent table. The Sort and Filter only apply to the base table and not to related tables.
            */
            DataTable dt = CreateJoinTable(TableName, SourceTable, FieldList);
            InsertJoinInto(dt, SourceTable, FieldList, RowFilter, Sort);
            return dt;
        }

        public void InsertJoinInto
            (
                DataTable DestTable,
                DataTable SourceTable,
                string FieldList,
                string RowFilter,
                string Sort
            )
        {
            /*
            * Copies the selected rows and columns from SourceTable and inserts them into DestTable
            * FieldList has same format as CreatejoinTable
            */
            if (FieldList == null)
            {
                throw new ArgumentException("You must specify at least one field in the field list.");
                //InsertInto(DestTable, SourceTable, RowFilter, Sort);
            }
            else
            {
                ParseFieldList(FieldList, true);
                DataRow[] Rows = SourceTable.Select(RowFilter, Sort);
                foreach (DataRow SourceRow in Rows)
                {
                    DataRow DestRow = DestTable.NewRow();
                    foreach (FieldInfo Field in m_FieldInfo)
                    {
                        if (Field.RelationName == null)
                        {
                            DestRow[Field.FieldName] = SourceRow[Field.FieldName];
                        }
                        else
                        {
                            DataRow ParentRow = SourceRow.GetParentRow(Field.RelationName);
                            DestRow[Field.FieldName] = ParentRow[Field.FieldName];
                        }
                    }
                    DestTable.Rows.Add(DestRow);
                }
            }
        }


        public DataTable SelectDistinct(string TableName, DataTable SourceTable, string FieldName)
        {
            DataTable dt = new DataTable(TableName);
            dt.Columns.Add(FieldName, SourceTable.Columns[FieldName].DataType);

            object LastValue = null;
            foreach (DataRow dr in SourceTable.Select("", "[" + FieldName + "]"))
            {
                if (LastValue == null || !(ColumnEqual(LastValue, dr[FieldName])))
                {
                    LastValue = dr[FieldName];
                    dt.Rows.Add(new object[] { LastValue });
                }
            }
            if (ds != null)
                ds.Tables.Add(dt);
            return dt;
        }

        private void ProcessRow(int RowCount, DataRow DestRow, DataRow SourceRow, Dictionary<string, Dictionary<string, object>> AccumlatorSet)
        {
            foreach (FieldInfo Field in GroupByFieldInfo)
            {
                switch (Field.Aggregate)    //this test is case-sensitive
                {
                    case null:        //implicit last
                    case "":        //implicit last
                    case "last":
                        if (Field.FieldName != "''" && Field.FieldName != "")
                        {
                            DestRow[Field.FieldAlias] = SourceRow[Field.FieldName];
                        }
                        break;
                    case "first":
                        if (RowCount == 1)
                            DestRow[Field.FieldAlias] = SourceRow[Field.FieldName];
                        break;
                    case "count":
                        DestRow[Field.FieldAlias] = RowCount;
                        break;
                    case "sum":
                        DestRow[Field.FieldAlias] = Add(DestRow[Field.FieldAlias], SourceRow[Field.FieldName]);
                        break;
                    case "max":
                        DestRow[Field.FieldAlias] = Max(DestRow[Field.FieldAlias], SourceRow[Field.FieldName]);
                        break;
                    case "min":
                        if (RowCount == 1)
                            DestRow[Field.FieldAlias] = SourceRow[Field.FieldName];
                        else
                            DestRow[Field.FieldAlias] = Min(DestRow[Field.FieldAlias], SourceRow[Field.FieldName]);
                        break;
                    case "avg":
                    case "stdev":
                    case "var":
                          AccumlatorSet["AccumulatedSum"][Field.FieldAlias] = Add(AccumlatorSet["AccumulatedSum"][Field.FieldAlias], SourceRow[Field.FieldName]);
                        if (SourceRow[Field.FieldName] != DBNull.Value && AccumlatorSet["AccumulatedSquareOfSum"][Field.FieldAlias] != DBNull.Value)
                        {
                            if (RowCount == 1)
                            {
                                AccumlatorSet["AccumulatedSquareOfSum"][Field.FieldAlias] = Math.Pow(Convert.ToDouble(SourceRow[Field.FieldName]), 2);
                            }
                            else
                            {
                                AccumlatorSet["AccumulatedSquareOfSum"][Field.FieldAlias] = Add(AccumlatorSet["AccumulatedSquareOfSum"][Field.FieldAlias], Math.Pow(Convert.ToDouble(SourceRow[Field.FieldName]) - Convert.ToDouble(AccumlatorSet["AccumulatedMean"][Field.FieldAlias]), 2));
                            }
                        }
                        else
                        {
                            AccumlatorSet["AccumulatedSquareOfSum"][Field.FieldAlias] = DBNull.Value;
                        }

                        break;
                }
            }
        }


        private void finalizeGroup(int pGroupStart, int pRowCount, List<DataRow> pRows, List<object> pGroupMedian, List<object> pGroup25, List<object> pGroup75, List<string> pGroupMode, Dictionary<string,int> pGroupValueCount)
        {
            
            
            pGroupMedian.Add(this.GetMedian(pRows, pGroupStart, pRowCount - pGroupStart));
            int index = pGroupMedian.Count - 1;
            int Length = pRowCount - pGroupStart;
            int half = Length / 2;

            if (Length % 2 == 0)
            {
                pGroup25.Add(this.GetMedian(pRows, pGroupStart, half));
                pGroup75.Add(this.GetMedian(pRows, pRowCount - half, pRowCount));
            }
            else
            {
                pGroup25.Add(this.GetMedian(pRows, pGroupStart, half + 1));
                pGroup75.Add(this.GetMedian(pRows, pRowCount - half - 1, pRowCount));
            }



            KeyValuePair<string, int> kvp = pGroupValueCount.ElementAt(0);
            for (int i = 0; i < pGroupValueCount.Count; i++)
            {
                if (pGroupValueCount.ElementAt(i).Value > kvp.Value)
                {
                    kvp = pGroupValueCount.ElementAt(i);
                }
            }
            pGroupMode.Add(kvp.Key);
        }


        private void finalizeRow(int RowCount, DataRow DestRow, DataRow SourceRow, Dictionary<string, Dictionary<string, object>> AccumlatorSet)
        {
            foreach (FieldInfo Field in GroupByFieldInfo)
            {
                switch (Field.Aggregate)    //this test is case-sensitive
                {
                    case null:        //implicit last
                    case "":        //implicit last
                    case "last":
                    case "first":
                    case "count":
                    case "sum":
                    case "max":
                    case "min":
                        break;
                    case "avg":
                        DestRow[Field.FieldAlias] = Avg(AccumlatorSet["AccumulatedSum"][Field.FieldAlias], RowCount);
                        break;
                    case "stdev":
                        DestRow[Field.FieldAlias] = STDev(AccumlatorSet["AccumulatedSquareOfSum"][Field.FieldAlias], AccumlatorSet["AccumulatedSum"][Field.FieldAlias], RowCount);
                        break;
                    case "var":
                        DestRow[Field.FieldAlias] = VARiance(AccumlatorSet["AccumulatedSquareOfSum"][Field.FieldAlias], AccumlatorSet["AccumulatedSum"][Field.FieldAlias], RowCount);
                        break;
                }
            }
        }



        public double GetMedian(List<DataRow> pRows, int pStart, int pEnd)
        {
            double result = 0;

            int Length = pEnd - pStart;

            double MiddleNumber = Length / 2.0;
            if (Length <= pRows.Count && Length > 0)
            {
                if (Length == 1)
                {
                    result = Convert.ToDouble(pRows[1][Main_Variable]);
                }
                else if (Length % 2 == 0)
                {
                    result = (Convert.ToDouble(pRows[Convert.ToInt32(pStart + MiddleNumber - 1)][Main_Variable]) + Convert.ToDouble(pRows[Convert.ToInt32(pStart + MiddleNumber)][Main_Variable])) / 2.0;
                }
                else
                {
                    result = Convert.ToDouble(pRows[Convert.ToInt32(pStart + MiddleNumber)][Main_Variable]);
                }
            }

            return result;
        }

        private class cDataRowComparer : IComparer<DataRow>
        {
            string[] ValueSet;
            //System.Data.DataColumnCollection Columns;

            public cDataRowComparer(string pValueList)
            {
                ValueSet = pValueList.Split(',');
                //Columns = pColumns;
            }
            public int Compare(DataRow x, DataRow y)
            {
                int result = 0;

                int i = 0;
                
                while(i < ValueSet.Length)
                {
                    if (x[ValueSet[i].Trim()] != DBNull.Value && y[ValueSet[i].Trim()] != DBNull.Value)
                    {
                        result = ((IComparable)x[ValueSet[i].Trim()]).CompareTo((IComparable)y[ValueSet[i].Trim()]);
                    }
                    else if (x[ValueSet[i].Trim()] == DBNull.Value && y[ValueSet[i].Trim()] == DBNull.Value)
                    {
                        result = 0;
                    }
                    else if (x[ValueSet[i].Trim()] == DBNull.Value)
                    {
                        result = -1;
                    }
                    else
                    {
                        result = 1;
                    }


                    if (result != 0)
                    {
                        break;
                    }
                    i++;
                }
                return result;
            }
        }

    }





}
