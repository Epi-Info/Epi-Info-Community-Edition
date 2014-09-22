using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Epi.Data.SqlServer
{
    /// <summary>
    /// Concrete Query class for Sql Server
    /// </summary>
    public class SqlQuery : Query
    {
        #region Constructor
        /// <summary>
        /// Constructor with parameter queryStatement
        /// </summary>
        /// <param name="queryStatement"></param>
        public SqlQuery(string queryStatement)
            : base(queryStatement)
        {
            this.Parameters = new List<QueryParameter>();
            if (!string.IsNullOrEmpty(this.sqlStatement))
                this.sqlStatement = this.sqlStatement.Replace('"', '\'');
        }
        #endregion
        #region Private Methods 
        #endregion
        #region Public Methods
        //// <summary>
        //// Return SQL Server syntax query for Recode
        //// </summary>
        //// <param name="variable">Source variable</param>
        //// <param name="assignToVariable">Destination variable</param>
        //// <param name="rangeNames">Recode groupe ranges</param>
        //// <returns></returns>
        ////
/*
        public override string GetRecodeCaseSubQuery(IVariable variable, IVariable assignToVariable, List<Epi.Core.Interpreter.Rules.RecodeRange> recodeRanges)
        {
            StringBuilder queryResult = new StringBuilder();
            string singlequote = string.Empty;
            if (variable.DataType == DataType.Text)
                singlequote = "'";

            queryResult.Append(assignToVariable.Name + " = ");
            queryResult.Append(" CASE ");

            for (int i = 0; i < recodeRanges.Count; i++)
            {
                switch (recodeRanges[i].RecodeType)
                {
                    case "RecodeA":
                        //<Recode_A> 							::= <Literal> '-' <Literal> '=' <Literal>
                        queryResult.Append(" WHEN " + variable.Name + " > " + singlequote + recodeRanges[i].Value1 + singlequote + " AND " + variable.Name + " <= " + singlequote + recodeRanges[i].Value2 + singlequote 
                            + " THEN '" + recodeRanges[i].EqualValue + "'");
                        break;
                    case "RecodeB":
                        //<Recode_B>							::= <Literal> '=' <Literal>
                        queryResult.Append(" WHEN " + variable.Name + " = " + singlequote + recodeRanges[i].Value1 + singlequote
                            + " THEN '" + recodeRanges[i].EqualValue.ToString() + "'");
                        break;
                    case "RecodeC":
                        //<Recode_C>							::= Boolean '=' <Literal> 	
                        queryResult.Append(" WHEN " + variable.Name + GetBooleanExpr(recodeRanges[i].Value1)
                            + " THEN '" + recodeRanges[i].EqualValue + "'");
                        break;
                    case "RecodeD":
                        //<Recode_D>							::= LOVALUE '-' <Literal> '=' <Literal>
                        queryResult.Append(" WHEN " + variable.Name + " <= " + singlequote + recodeRanges[i].Value2 + singlequote
                            + " THEN '" + recodeRanges[i].EqualValue + "'");
                        break;
                    case "RecodeE":
                        //<Recode_E>							::= <Literal> '-' HIVALUE '=' <Literal>
                        queryResult.Append(" WHEN " + variable.Name + " > " + singlequote + recodeRanges[i].Value1 + singlequote
                            + " THEN '" + recodeRanges[i].EqualValue + "'");
                        break;
                    case "RecodeF":
                        //<Recode_F>							::= LOVALUE '-' HIVALUE '=' <Literal>
                        queryResult.Append("");
                        break;
                    case "RecodeG":
                        //<Recode_G>							::= ELSE '=' <Literal>
                        queryResult.Append(" ELSE '" + recodeRanges[i] + "'");
                        break;
                    case "RecodeH":
                        //<Recode_H>							::= <Literal> '-' <Literal> '=' Boolean
                        queryResult.Append(" WHEN " + variable.Name + " > " + singlequote + recodeRanges[i].Value1 + singlequote + " AND " + variable.Name + "<=" + singlequote + recodeRanges[i].Value2 + singlequote
                            + " THEN " + Boolean.Parse(recodeRanges[i].EqualValue));
                        break;
                    case "RecodeI":
                        //<Recode_I>							::= <Literal> '=' Boolean
                        queryResult.Append(" WHEN " + variable.Name + " = " + singlequote + recodeRanges[i].Value1 + singlequote
                            + " THEN " + Boolean.Parse(recodeRanges[i].EqualValue)); 
                        break;
                    case "RecodeJ":
                        //<Recode_J>							::= Boolean '=' Boolean
                        queryResult.Append(" WHEN " + variable.Name + " = " + Boolean.Parse(recodeRanges[i].Value1)
                            + " THEN " + Boolean.Parse(recodeRanges[i].EqualValue));
                        break;
                    case "RecodeK":
                        //<Recode_K>							::= LOVALUE '-' <Literal> '=' Boolean
                        queryResult.Append(" WHEN " + variable.Name + " < " + singlequote + recodeRanges[i].Value2 + singlequote
                            + " THEN " + Boolean.Parse(recodeRanges[i].EqualValue));
                        break;
                    case "RecodeL":
                        //<Recode_L>							::= <Literal> '-' HIVALUE '=' Boolean
                        queryResult.Append(" WHEN " + variable.Name + " > " + singlequote + recodeRanges[i].Value1 + singlequote
                            + " THEN " + Boolean.Parse(recodeRanges[i].EqualValue));
                        break;
                    case "RecodeM":
                        //<Recode_M>							::= LOVALUE '-' HIVALUE '=' Boolean
                        queryResult.Append("");
                        break;
                    case "RecodeN":
                        //<Recode_N>							::= ELSE '=' Boolean
                        queryResult.Append(" ELSE " + Boolean.Parse(recodeRanges[i].EqualValue));
                        break;
                    default:
                        //Should not be here
                        break;
                }
            }
            queryResult.Append(" END ");
            return queryResult.ToString();

        }*/
/// <summary>
/// Get SQL Server Syntax Insert Into Query Value part
/// </summary>
/// <param name="pvalues">Semicolon delaminated string groups (value1, value2, ... ,valueN)</param>
/// <returns> Insert Into Query Value part : SELECT value1, value2, ... ,valueN UNION ALL ... </returns>
        public override string GetInsertValue(string pvalues)  
            {
                StringBuilder qResult = new StringBuilder();
                string[] ss = pvalues.Split(';');
                for (int i = 0; i < ss.Length; i++)
                {
                    if (!string.IsNullOrEmpty(ss[i]))
                    {
                        qResult.Append("SELECT " + ss[i] + " UNION All \n");
                    }
                }
                qResult.Remove(qResult.Length - 11, 11);   
                return qResult.ToString();
            }
    
        #endregion
    }
}
