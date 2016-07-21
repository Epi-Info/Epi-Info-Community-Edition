using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Epi.Data;

namespace Epi.Analysis.Statistics
{
    public class cWorkingTable
    {
        public static DataTable CreateWorkingTable(string pNumericVariable, string pCrossTabVariable, Dictionary<string, string> config, DataTable DT, List<string> pStratavar, List<string> pStrataValue, string pWeightVariable)
        {
            DataTable result = new DataTable();
            result.CaseSensitive = true;


            DataTable RowValues = new DataTable();
            DataTable ColumnValues = new DataTable();

            /*
            Produce Table with rows = number of distinct values in main_variable
                if no cross _tab_variable then columns = 2
                else columns = 2 + number of values in cross_tab_variable

            For each strata 
                get distinct values of the strata
                for each distinct value
                    produce the following table
            */

            cDataSetHelper ds = new cDataSetHelper();

            ColumnValues = ds.SelectDistinct("ColumnValues", DT, pNumericVariable);
            
            result.Columns.Add(new DataColumn("__Values__", ColumnValues.Columns[pNumericVariable].DataType));
            result.Columns.Add(new DataColumn("__Count__", typeof(double)));


            if (!string.IsNullOrEmpty(pCrossTabVariable))
            {
                RowValues = ds.SelectDistinct("RowValues", DT, pCrossTabVariable);
                foreach (DataRow R in RowValues.Select("", pCrossTabVariable))
                {
                    if (R[0] == DBNull.Value || string.IsNullOrEmpty(R[0].ToString()))
                    {
                        if (config["include-missing"].ToUpperInvariant() != "FALSE")
                        {
                            DataColumn dataColumn = new DataColumn(config["RepresentationOfMissing"], typeof(double));
                            dataColumn.ExtendedProperties.Add("Value", R[0]);
                            bool isFound = false;

                            for (int i = 0; i < result.Columns.Count; i++)
                            {
                                if (dataColumn.ColumnName == result.Columns[i].ColumnName)
                                {
                                    isFound = true;
                                    break;
                                }
                            }

                            if (!isFound)
                            {
                                result.Columns.Add(dataColumn);
                            }
                            else
                            {

                            }
                        }
                    }
                    else
                    {
                        DataColumn dataColumn = new DataColumn(R[0].ToString(), typeof(double));
                        dataColumn.ExtendedProperties.Add("Value", R[0]);

                        bool isFound = false;

                        for (int i = 0; i < result.Columns.Count; i++)
                        {
                            if (dataColumn.ColumnName == result.Columns[i].ColumnName)
                            {
                                isFound = true;
                                break;
                            }
                        }

                        if (!isFound)
                        {
                            result.Columns.Add(dataColumn);
                        }
                        else
                        {

                        }
                    }
                }
            }

            Dictionary<string, DataRow> RowList = new Dictionary<string, DataRow>();
            DataRow newRow = null;
            // initialize table
            foreach (DataRow R in ColumnValues.Select("", "[" + pNumericVariable + "]"))
            {
                string RowKey = null;

                if (R[0] is DateTime)
                {
                    RowKey = ((DateTime)R[0]).ToString("MM/dd/yyyy hh:mm:ss.fff tt");
                }
                else
                {
                    RowKey  = R[0].ToString();
                    
                }

                if (!RowList.ContainsKey(RowKey))
                {
                    newRow = result.NewRow();
                    foreach (DataColumn C in result.Columns)
                    {
                        if (C.ColumnName.Equals("__Values__"))
                        {
                            newRow[C.ColumnName] = R[0];
                        }
                        else
                        {
                            newRow[C.ColumnName] = 0;
                        }
                    }
                    RowList.Add(RowKey, newRow);
                    result.Rows.Add(newRow);
                }
            }

            DataRow[] workingRows = null;
            if (pStratavar == null || pStratavar.Count == 0 || string.IsNullOrEmpty(pStratavar[0]))
            {
                workingRows = DT.Select("", "[" +  pNumericVariable + "]");
            }
            else
            {
                StringBuilder WhereClause = new StringBuilder();
                for(int i = 0; i < pStratavar.Count; i++)
                {
                    string CurrentStratavar = pStratavar[i];
                    string CurrentValue = pStrataValue[i];

                    DataColumn column = DT.Columns[CurrentStratavar];
                    switch (column.DataType.Name)
                    {
                        case "Byte":
                        case "Boolean":
                        case "Double":
                        case "Float":
                        case "Integer":
                        case "Int16":
                        case "Short":
                        case "Int32":
                        case "Int64":
                            if (CurrentValue.Equals("NULL"))
                                WhereClause.Append(string.Format("[{0}] IS {1} And ", CurrentStratavar, CurrentValue));
                            else
                                WhereClause.Append(string.Format("[{0}] = {1} And ", CurrentStratavar, CurrentValue));
                            break;
                        case "String":
                        default:
                            WhereClause.Append(string.Format("[{0}] = '{1}' And ", CurrentStratavar, CurrentValue));
                            break;
                    }
                }

                WhereClause.Length = WhereClause.Length - 4;
                workingRows = DT.Select(WhereClause.ToString(), "[" + pNumericVariable + "]");
            }

            if (string.IsNullOrEmpty(pCrossTabVariable))
            {
                foreach (DataRow R in workingRows)
                {
                    DataRow Dest;

                    if (R[pNumericVariable] is DateTime)
                    {
                        Dest = RowList[((DateTime)R[pNumericVariable]).ToString("MM/dd/yyyy hh:mm:ss.fff tt")];
                    }
                    else
                    {
                        Dest = RowList[R[pNumericVariable].ToString()];
                    }

                    if (string.IsNullOrEmpty(pWeightVariable))
                    {
                        Dest["__Count__"] = (double)Dest["__Count__"] + 1;
                    }
                    else
                    {
                        if (R[pWeightVariable] != DBNull.Value && ((R[pWeightVariable] is String) == false))
                        {
                            Dest["__Count__"] = (double)Dest["__Count__"] + Convert.ToDouble(R[pWeightVariable]);
                        }
                        else
                        {
                            if (config["include-missing"].ToUpperInvariant() != "FALSE")
                            {
                                Dest["__Count__"] = (double)Dest["__Count__"] + 1;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (DataRow R in workingRows)
                {
                    DataRow Dest;

                    if (R[pNumericVariable] is DateTime)
                    {
                        Dest = RowList[((DateTime)R[pNumericVariable]).ToString("MM/dd/yyyy hh:mm:ss.fff tt")];
                    }
                    else
                    {
                        Dest = RowList[R[pNumericVariable].ToString()];
                    }

                    double rowWeightValue = 0;
                    double rowCrossTabValue = 0;

                    if (string.IsNullOrEmpty(pCrossTabVariable) == false)
                    {
                        if (R[pCrossTabVariable] != System.DBNull.Value && !string.IsNullOrEmpty(R[pCrossTabVariable].ToString()))
                        {
                            rowCrossTabValue = (double)Dest[R[pCrossTabVariable].ToString()];
                        }
                        else
                        {
                            if (config["include-missing"].ToUpperInvariant() != "FALSE")
                            {
                               rowCrossTabValue =  (double) Dest[config["RepresentationOfMissing"]];
                            }
                        }
                    }

                    if(string.IsNullOrEmpty(pWeightVariable))
                    {
                        Dest["__Count__"] = (double)Dest["__Count__"] + 1;

                        if (R[pCrossTabVariable] != System.DBNull.Value && !string.IsNullOrEmpty(R[pCrossTabVariable].ToString()))
                        {
                            Dest[R[pCrossTabVariable].ToString()] = rowCrossTabValue + 1;
                        }
                        else
                        {
                            if (config["include-missing"].ToUpperInvariant() != "FALSE")
                            {
                                Dest[config["RepresentationOfMissing"]] = rowCrossTabValue + 1;
                            }
                        }
                    }
                    else
                    {
                        if (R[pWeightVariable] != System.DBNull.Value && !string.IsNullOrEmpty(R[pWeightVariable].ToString()))
                        {
                            rowWeightValue = Convert.ToDouble(R[pWeightVariable]);
                            Dest["__Count__"] = (double)Dest["__Count__"] + rowWeightValue;
                        }
                        else
                        {
                            if (config["include-missing"].ToUpperInvariant() != "FALSE")
                            {
                                Dest["__Count__"] = (double)Dest["__Count__"] + rowWeightValue;
                            }
                        }

                        if (R[pCrossTabVariable] != System.DBNull.Value && !string.IsNullOrEmpty(R[pCrossTabVariable].ToString()))
                        {
                            if (R[pNumericVariable] is DateTime)
                            {
                                Dest[((DateTime)R[pNumericVariable]).ToString("MM/dd/yyyy hh:mm:ss.fff tt")] = rowCrossTabValue + rowWeightValue;
                            }
                            else
                            {
                                Dest[R[pCrossTabVariable].ToString()] = rowCrossTabValue + rowWeightValue;
                            }
                        }
                        else
                        {
                            if (config["include-missing"].ToUpperInvariant() != "FALSE")
                            {
                                Dest[config["RepresentationOfMissing"]] = rowCrossTabValue + 1;
                            }
                        }
                    }
                }
            }

            return result;
        }

        static string GetPrintValue(object pValue, Dictionary<string, string> pConfig)
        {
            string result = null;

            if (pValue == DBNull.Value)
            {
                result = pConfig["RepresentationOfMissing"];
            }
            else switch (pValue.GetType().Name)
                {
                    case "Byte":
                        result = (Convert.ToBoolean(pValue) ? pConfig["RepresentationOfYes"] : pConfig["RepresentationOfNo"]);
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


        public static Statistic_Struct GetStatistics(DataTable pWorkingTable, string pColumnName)
        {
            Statistic_Struct result;

            result.Obs = 0;
            result.Sum = 0;
            result.Sum_Sqr = 0;
            result.AVG = 0;
            result.Variance = 0;
            result.Std_Dev = 0;
            result.Std_Error = 0;
            result.tZero = 0;
            result.tZeroP = 0;
            result.Min = null;
            result.Median_25 = null;
            result.Median = null;
            result.Median_75 = null;
            result.Max = null;
            result.Mode = null;

            double ModeCount = 0;

            DataRow[] ROWS = pWorkingTable.Select(string.Format("[{0}] > 0 ", pColumnName),"__Values__");

            int i = 0;
            foreach (System.Data.DataRow R in ROWS)
            {
                
                object temp = R["__Values__"];
                
                double currentCount;

                double.TryParse(R[pColumnName].ToString(), out currentCount);
                result.Obs += currentCount;

                if (currentCount > ModeCount)
                {
                    ModeCount = currentCount;
                    result.Mode = temp;
                }

                double temp2;

                if (temp != System.DBNull.Value)
                {

                    if (double.TryParse(R["__Values__"].ToString(), out temp2))
                    {
                        result.Sum += (temp2 * currentCount);
                    }
                    else
                    {

                    }

                    if (i == 0)
                    {
                        result.Max = result.Min = temp;
                    }
                    else
                    {
                        if (temp != System.DBNull.Value)
                        {
                            int compare;

                            if (result.Max == System.DBNull.Value)
                            {
                                result.Max = temp;
                            }
                            else
                            {
                                compare = ((IComparable)temp).CompareTo(result.Max);
                                if (compare > 0)
                                {
                                    result.Max = temp;
                                }
                            }

                            if (result.Min == System.DBNull.Value)
                            {
                                result.Min = temp;
                            }
                            else
                            {
                                compare = ((IComparable)temp).CompareTo(result.Min);
                                if (compare < 0)
                                {
                                    result.Min = temp;
                                }
                            }
                        }
                    }
                }
                i++;

            }

            result.AVG = result.Sum / result.Obs;

            result.Variance = 0.0;
            result.Sum_Sqr = 0.0;
            i = 0;
            foreach (System.Data.DataRow R in ROWS)
            {
                double temp;
                double.TryParse(R["__Values__"].ToString(), out temp);
                double currentCount;

                double.TryParse(R[pColumnName].ToString(), out currentCount);

                result.Sum_Sqr += Math.Pow(temp - result.AVG, 2) * currentCount;
                i += (int)currentCount;
            }

            result.Variance = result.Sum_Sqr / (i - 1);
            result.Std_Dev = Math.Sqrt(result.Variance);

            // Calculate 25 50 75 medians

            double m25 = result.Obs * .25;
            double m50 = result.Obs * .5;
            double m75 = result.Obs * .75;

            result.Obs = 0;
            object oldValue = null;
            for(int SortedRowIndex = 0; SortedRowIndex < ROWS.Length; SortedRowIndex++)
            {
                System.Data.DataRow R = ROWS[SortedRowIndex];

                object temp = R["__Values__"];
                double currentCount;

                double.TryParse(R[pColumnName].ToString(), out currentCount);

                double p1;
                double p2;

                if (m25 > result.Obs && m25 <= result.Obs + currentCount)
                {
                    if (m25 == result.Obs + currentCount)
                    {
                        if (SortedRowIndex + 1 < ROWS.Length)
                        {
                            oldValue = ROWS[SortedRowIndex + 1]["__Values__"];
                            if (temp != null && oldValue != null && double.TryParse(temp.ToString(), out p1) && double.TryParse(oldValue.ToString(), out p2))
                            {
                                result.Median_25 = (p1 + p2) / 2.0;
                            }
                        }
                    }
                    else
                    {
                        result.Median_25 = temp;
                    }
                }

                if (m50 > result.Obs && m50 <= result.Obs + currentCount)
                {
                    if (m50 == result.Obs + currentCount)
                        {
                        if (SortedRowIndex + 1 < ROWS.Length)
                        {
                            oldValue = ROWS[SortedRowIndex + 1]["__Values__"];
                            if (temp != null && oldValue != null && double.TryParse(temp.ToString(), out p1) && double.TryParse(oldValue.ToString(), out p2))
                            {
                                result.Median = (p1 + p2) / 2.0;
                            }
                        }
                    }
                    else
                    {
                        result.Median = temp;
                    }
                }

                if (m75 > result.Obs && m75 <= result.Obs + currentCount)
                {
                    if (m75 == result.Obs + currentCount)
                    {
                        
                        if(SortedRowIndex + 1 < ROWS.Length)
                        {
                            oldValue = ROWS[SortedRowIndex + 1]["__Values__"];
                            if (temp != null && oldValue != null && double.TryParse(temp.ToString(), out p1) && double.TryParse(oldValue.ToString(), out p2))
                            {
                                result.Median_75 = (p1 + p2) / 2.0;
                            }
                        }
                        else if (SortedRowIndex + 1 == ROWS.Length && ROWS.Length == 2)
                        {
                            oldValue = ROWS[0]["__Values__"];
                            if (temp != null && oldValue != null && double.TryParse(temp.ToString(), out p1) && double.TryParse(oldValue.ToString(), out p2))
                            {
                                result.Median_75 = (p1 + p2) / 2.0;
                            }
                        }
                    }
                    else
                    {
                        result.Median_75 = temp;
                    }
                }

                oldValue = temp;
                result.Obs += currentCount;
            }


            return result;
            
        }

        public static Statistic_Struct GetStatistics(DataTable pWorkingTable, string pColumnName, DataTable FullTable)
        {
            Statistic_Struct result;

            result.Obs = 0;
            result.Sum = 0;
            result.Sum_Sqr = 0;
            result.AVG = 0;
            result.Variance = 0;
            result.Std_Dev = 0;
            result.Std_Error = 0;
            result.tZero = 0;
            result.tZeroP = 0;
            result.Min = null;
            result.Median_25 = null;
            result.Median = null;
            result.Median_75 = null;
            result.Max = null;
            result.Mode = null;

            double ModeCount = 0;

            DataRow[] ROWS = pWorkingTable.Select(string.Format("[{0}] > 0 ", pColumnName), "__Values__");

            int i = 0;
            foreach (System.Data.DataRow R in ROWS)
            {

                object temp = R["__Values__"];

                double currentCount;

                double.TryParse(R[pColumnName].ToString(), out currentCount);
                result.Obs += currentCount;

                if (currentCount > ModeCount)
                {
                    ModeCount = currentCount;
                    result.Mode = temp;
                }

                double temp2;

                if (temp != System.DBNull.Value)
                {

                    if (double.TryParse(R["__Values__"].ToString(), out temp2))
                    {
                        result.Sum += (temp2 * currentCount);
                    }
                    else
                    {

                    }

                    if (i == 0)
                    {
                        result.Max = result.Min = temp;
                    }
                    else
                    {
                        if (temp != System.DBNull.Value)
                        {
                            int compare;

                            if (result.Max == System.DBNull.Value)
                            {
                                result.Max = temp;
                            }
                            else
                            {
                                compare = ((IComparable)temp).CompareTo(result.Max);
                                if (compare > 0)
                                {
                                    result.Max = temp;
                                }
                            }

                            if (result.Min == System.DBNull.Value)
                            {
                                result.Min = temp;
                            }
                            else
                            {
                                compare = ((IComparable)temp).CompareTo(result.Min);
                                if (compare < 0)
                                {
                                    result.Min = temp;
                                }
                            }
                        }
                    }
                }
                i++;

            }

            result.AVG = result.Sum / result.Obs;

            result.Variance = 0.0;
            result.Sum_Sqr = 0.0;
            i = 0;
            foreach (System.Data.DataRow R in ROWS)
            {
                double temp;
                double.TryParse(R["__Values__"].ToString(), out temp);
                double currentCount;

                double.TryParse(R[pColumnName].ToString(), out currentCount);

                result.Sum_Sqr += Math.Pow(temp - result.AVG, 2) * currentCount;
                i += (int)currentCount;
            }
            DataRow[] ALLROWS = FullTable.Select();
            i = 0;
            if (FullTable.Columns.Count > 1)
            {
                foreach (System.Data.DataRow R in ALLROWS)
                {
                    if (R[1].ToString().Equals(pColumnName) || pColumnName.Equals("__Count__"))
                        i++;
                }
            }
            else
            {
                foreach (System.Data.DataRow R in ALLROWS)
                {
                    if (R[0].ToString().Equals(pColumnName) || pColumnName.Equals("__Count__"))
                        i++;
                }
            }

            result.Variance = result.Sum_Sqr / (i - 1);
            result.Std_Dev = Math.Sqrt(result.Variance);

            // Calculate 25 50 75 medians

            double m25 = result.Obs * .25;
            double m50 = result.Obs * .5;
            double m75 = result.Obs * .75;

            result.Obs = 0;
            object oldValue = null;
            for (int SortedRowIndex = 0; SortedRowIndex < ROWS.Length; SortedRowIndex++)
            {
                System.Data.DataRow R = ROWS[SortedRowIndex];

                object temp = R["__Values__"];
                double currentCount;

                double.TryParse(R[pColumnName].ToString(), out currentCount);

                double p1;
                double p2;

                if (m25 > result.Obs && m25 <= result.Obs + currentCount)
                {
                    if ((result.Obs + currentCount) % 2.0 == 0)
                    {
                        if (SortedRowIndex + 1 < ROWS.Length)
                        {
                            oldValue = ROWS[SortedRowIndex + 1]["__Values__"];
                            if (temp != null && oldValue != null && double.TryParse(temp.ToString(), out p1) && double.TryParse(oldValue.ToString(), out p2))
                            {
                                result.Median_25 = (p1 + p2) / 2.0;
                            }
                        }
                    }
                    else
                    {
                        result.Median_25 = temp;
                    }
                }

                if (m50 > result.Obs && m50 <= result.Obs + currentCount)
                {
                    if (m50 == result.Obs + currentCount)
                    {
                        if (SortedRowIndex + 1 < ROWS.Length)
                        {
                            oldValue = ROWS[SortedRowIndex + 1]["__Values__"];
                            if (temp != null && oldValue != null && double.TryParse(temp.ToString(), out p1) && double.TryParse(oldValue.ToString(), out p2))
                            {
                                result.Median = (p1 + p2) / 2.0;
                            }
                        }
                    }
                    else
                    {
                        result.Median = temp;
                    }
                }

                if (m75 > result.Obs && m75 <= result.Obs + currentCount)
                {
                    if ((result.Obs + currentCount) % 2.0 == 0)
                    {

                        if (SortedRowIndex + 1 < ROWS.Length)
                        {
                            oldValue = ROWS[SortedRowIndex + 1]["__Values__"];
                            if (temp != null && oldValue != null && double.TryParse(temp.ToString(), out p1) && double.TryParse(oldValue.ToString(), out p2))
                            {
                                result.Median_75 = (p1 + p2) / 2.0;
                            }
                        }
                        else if (SortedRowIndex + 1 == ROWS.Length && ROWS.Length == 2)
                        {
                            oldValue = ROWS[0]["__Values__"];
                            if (temp != null && oldValue != null && double.TryParse(temp.ToString(), out p1) && double.TryParse(oldValue.ToString(), out p2))
                            {
                                result.Median_75 = (p1 + p2) / 2.0;
                            }
                        }
                    }
                    else
                    {
                        result.Median_75 = temp;
                    }
                }

                oldValue = temp;
                result.Obs += currentCount;
            }


            return result;

        }

        public static double GetMedian(string ColumnName, DataRow[] pRows, int pStart, int pEnd)
        {
            double result = 0;

            int Length = pEnd - pStart;

            double MiddleNumber = Length / 2.0;
            if (Length <= pRows.Length && Length > 0)
            {
                if (Length == 1)
                {
                    result = Convert.ToDouble(pRows[1][ColumnName]);
                }
                else if (Length % 2 == 0)
                {
                    result = (Convert.ToDouble(pRows[Convert.ToInt32(pStart + MiddleNumber - 1)][ColumnName]) + Convert.ToDouble(pRows[Convert.ToInt32(pStart + MiddleNumber)][ColumnName])) / 2.0;
                }
                else
                {
                    result = Convert.ToDouble(pRows[Convert.ToInt32(pStart + MiddleNumber)][ColumnName]);
                }
            }

            return result;
        }


        static void permut(int k, int n, int[] nums)
        {
            int i, j, tmp;

            /* when k > n we are done and should print */
            if (k <= n)
            {

                for (i = k; i <= n; i++)
                {
                    tmp = nums[i];
                    for (j = i; j > k; j--)
                    {
                        nums[j] = nums[j - 1];
                    }
                    nums[k] = tmp;

                    /* recurse on k+1 to n */
                    permut(k + 1, n, nums);

                    for (j = k; j < i; j++)
                    {
                        nums[j] = nums[j + 1];
                    }
                    nums[i] = tmp;
                }
            }
            else
            {
                for (i = 1; i <= n; i++)
                {
                    Console.Write("{0} ", nums[i]);
                }
                Console.WriteLine();
            }
        }

    }

    public struct Statistic_Struct
    {
        public double Obs;
        public double Sum;
        public double Sum_Sqr;
        public double AVG;
        public double Variance;
        public double Std_Dev;
        public double Std_Error;
        public double tZero;
        public double tZeroP;
        public object Min;
        public object Median;
        public object Median_25;
        public object Median_75;
        public object Max;
        public object Mode;
    }



}
