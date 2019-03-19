using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using Epi.Data.MongoDB;
using System.Data.CData.MongoDB;

namespace Epi.Data.MongoDB
{
    /// <summary>
    /// MongoDB statement and related parameters.
    /// </summary>
    public class MongoDBQuery : Query
    {
        #region Constructors

        /// <summary>
        /// Constructor with parameter queryStatement
        /// </summary>
        /// <param name="queryStatement">Query</param>
        public MongoDBQuery(string queryStatement)
            : base(queryStatement)
        {
            //catch non-ansi statements here
            //Query calls need to be more uniform in application

            // replace 'identity' with 'auto_increment'
            sqlStatement = sqlStatement.Replace(" identity (1,1) ", " auto_increment ");

            // Replace escape chars
            sqlStatement = sqlStatement.Replace(CharLiterals.LEFT_SQUARE_BRACKET, CharLiterals.BACK_TICK);
            sqlStatement = sqlStatement.Replace(CharLiterals.RIGHT_SQUARE_BRACKET, CharLiterals.BACK_TICK);
            
            // Correct syntax for default
            sqlStatement = sqlStatement.Replace("default 1", "default '1'");

            // End the statement with a semicolon
            if (!sqlStatement.EndsWith(StringLiterals.SEMI_COLON))
            {
                sqlStatement += StringLiterals.SEMI_COLON;
            }
        }
        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Get MongoDB Syntax Insert Into Query Value part
        /// </summary>
        /// <param name="pvalues"> Semicolon delaminated string groups (value1, value2, ... ,valueN)</param>
        /// <returns>Insert Into Query Value part : VALUES (v11,v12,...,v1n), (v21,v22,...,v2n), ... (vm1,vm2,...,vmn)</returns>
        public override string GetInsertValue(string pvalues)
        {
            StringBuilder qResult = new StringBuilder();
            qResult.Append("VALUES "); 
            string[] ss = pvalues.Split(';');
            for (int i = 0; i < ss.Length; i++)
            {
                qResult.Append(" (" + ss[i] + "),");
            }
            qResult.Remove(qResult.Length - 1, 1);
            return qResult.ToString();
        }

        #endregion

        #region Deprecated Code
        //// <summary>
        //// Return MongoDB syntax query for Recode
        //// </summary>
        //// <param name="variable">Source variable</param>
        //// <param name="assignToVariable">Destination variable</param>
        //// <param name="rangeNames">Recode group ranges</param>
        //// <returns></returns>
        /*public override string GetRecodeCaseSubQuery(IVariable variable, IVariable assignToVariable, List<Epi.Core.Interpreter.Rules.RecodeRange> recodeRanges)
        {
            StringBuilder queryResult = new StringBuilder();
            StringBuilder queryPart1 = new StringBuilder();
            StringBuilder queryPart2 = new StringBuilder();
            bool bacs = false;
            queryResult.Append(" CASE ");
           // string compareOperator1 = string.Empty;
           // string compareOperator2 = string.Empty;
           // //string compareOperator0 = string.Empty;
           // if (string.Equals(rangeNames[0].Low, CommandNames.LOVALUE))
           // {
           //     bacs = true;
           // }
           // else if (string.Equals(rangeNames[rangeNames.Count - 1].Low, CommandNames.LOVALUE))
           // {
           //     bacs = false;
           // }
           // else if (int.Parse(rangeNames[0].Low) < int.Parse(rangeNames[rangeNames.Count - 1].Low))
           // {
           //     bacs = true;
           // }

           // if (bacs)
           // {
           //     compareOperator1 = " <= ";
           //     compareOperator2 = " > ";
           //     //compareOperator0 = " >= ";
           // }
           // else
           // {
           //     //compareOperator0 = " <= ";
           //     compareOperator1 = " < ";
           //     compareOperator2 = " >= ";
           // }

           // for (int i = 0; i < rangeNames.Count; i++)
           // {
           //     if (queryPart1.Length > 0) queryPart1.Remove(0, queryPart1.Length);
           //     if (queryPart2.Length > 0) queryPart2.Remove(0, queryPart2.Length);
           //     queryResult.Append(" WHEN ");
           //     if (rangeNames[i].High != CommandNames.HIVALUE)
           //     {
           //         queryPart1.Append(variable.Name);
           //         queryPart1.Append(compareOperator1);
           //         queryPart1.Append(rangeNames[i].High);
           //     }
           //     if (rangeNames[i].Low != CommandNames.LOVALUE)
           //     {
           //         queryPart2.Append(variable.Name);
           //         queryPart2.Append(compareOperator2);
           //         //queryPart2.Append(i == 0 ? compareOperator0 : compareOperator2);
           //         queryPart2.Append(rangeNames[i].Low);
           //     }
           //     if (queryPart1.Length != 0 && queryPart2.Length != 0)
           //     {
           //         queryResult.Append(queryPart1.ToString());
           //         queryResult.Append(" AND ");
           //         queryResult.Append(queryPart2.ToString());
           //     }
           //     else if (queryPart2.Length == 0)
           //     {
           //         queryResult.Append(queryPart1.ToString());
           //     }
           //     else if (queryPart1.Length == 0)
           //     {
           //         queryResult.Append(queryPart2.ToString());
           //     }
           //     queryResult.Append(" THEN ");
           //     queryResult.Append("'" + rangeNames[i].Name.Substring(1, rangeNames[i].Name.Length - 2) + "'");

           //     //sbswitch.Append(st.Substring(1, st.Length - 2));
           //}            

            queryResult.Append(" END ");
            return queryResult.ToString();
        }*/
        #endregion
    }
}
