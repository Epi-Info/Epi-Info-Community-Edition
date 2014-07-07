using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections; 

namespace Epi.Data.Office
{   
    /// <summary>
    /// Conrect Jet query class 
    /// </summary>
    public class JetQuery: Query 
    {

        #region Constructors
            /// <summary>
            /// Constructor for concret class JetQuery 
            /// </summary>
            /// <param name="queryStatement"></param>
            public JetQuery(string queryStatement)
                : base(queryStatement)
            {
                this.Parameters = new List<QueryParameter>();
            }
        #endregion
        #region Private Members
            private StringBuilder queryResult = new StringBuilder();
        #endregion
        #region Private Methods
        /*
        private string getSwitchStatement(IVariable variable, IVariable assignToVariable, List<Epi.Core.Interpreter.Rules.RecodeRange> recodeRanges)
            {
                StringBuilder queryResult = new StringBuilder();
                string singlequote = string.Empty;
                if (variable.DataType == DataType.Text)
                    singlequote = "'";
                queryResult.Append("SWITCH(");
                for (int i = 0; i < recodeRanges.Count ; i++)
                {
                    switch (recodeRanges[i].RecodeType)
                    {
                        case "RecodeA":
                            //<Recode_A> 							::= <Literal> '-' <Literal> '=' <Literal>
                            queryResult.Append( "(" + variable.Name + ") > " + singlequote + recodeRanges[i].Value1 + singlequote + " AND (" + variable.Name + ") <= " + singlequote + recodeRanges[i].Value2 + singlequote 
                                + " , '" + recodeRanges[i].EqualValue + "', ");
                            break;
                        case "RecodeB":
                            //<Recode_B>							::= <Literal> '=' <Literal>
                            queryResult.Append("(" + variable.Name + ") = " + singlequote + recodeRanges[i].Value1 + singlequote
                                + " , '" + recodeRanges[i].EqualValue + "', ");
                            break;
                        case "RecodeC":
                            //<Recode_C>							::= Boolean '=' <Literal> 	
                            queryResult.Append("(" + variable.Name + ") " + GetBooleanExpr(recodeRanges[i].Value1)
                                + " , '" + recodeRanges[i].EqualValue + "', ");
                            break;
                        case "RecodeD":
                            //<Recode_D>							::= LOVALUE '-' <Literal> '=' <Literal>
                            queryResult.Append("(" + variable.Name + ") <= " + singlequote + recodeRanges[i].Value2 + singlequote
                                + " , '" + recodeRanges[i].EqualValue + "', ");
                            break;
                        case "RecodeE":
                            //<Recode_E>							::= <Literal> '-' HIVALUE '=' <Literal>
                            queryResult.Append("(" + variable.Name + ") > " + singlequote + recodeRanges[i].Value1 + singlequote
                                + " , '" + recodeRanges[i].EqualValue + "', ");
                            break;
                        case "RecodeF":
                            //<Recode_F>							::= LOVALUE '-' HIVALUE '=' <Literal>
                            queryResult.Append("");
                            break;
                        case "RecodeG":
                            //<Recode_G>							::= ELSE '=' <Literal>
                            queryResult.Append(" true, '" + recodeRanges[i].EqualValue + "', ");
                            break;
                        case "RecodeH":
                            //<Recode_H>							::= <Literal> '-' <Literal> '=' Boolean
                            queryResult.Append("(" + variable.Name + ") > " + singlequote + recodeRanges[i].Value1 + singlequote + " AND (" + variable.Name + ")<=" + singlequote + recodeRanges[i].Value2 + singlequote
                                + " , " + GetBooleanExpr(recodeRanges[i].EqualValue));
                            break;
                        case "RecodeI":
                            //<Recode_I>							::= <Literal> '=' Boolean
                            queryResult.Append("(" + variable.Name + ") = " + singlequote + recodeRanges[i].Value1 + singlequote
                                + " , " + GetBooleanExpr(recodeRanges[i].EqualValue)); 
                            break;
                        case "RecodeJ":
                            //<Recode_J>							::= Boolean '=' Boolean
                            queryResult.Append("(" + variable.Name + ") = " + Boolean.Parse(recodeRanges[i].Value1)
                                + " , " + GetBooleanExpr(recodeRanges[i].EqualValue));
                            break;
                        case "RecodeK":
                            //<Recode_K>							::= LOVALUE '-' <Literal> '=' Boolean
                            queryResult.Append("(" + variable.Name + ") < " + singlequote + recodeRanges[i].Value2 + singlequote
                                + " , " + GetBooleanExpr(recodeRanges[i].EqualValue));
                            break;
                        case "RecodeL":
                            //<Recode_L>							::= <Literal> '-' HIVALUE '=' Boolean
                            queryResult.Append("(" + variable.Name + ") > " + singlequote + recodeRanges[i].Value1 + singlequote
                                + " , " + GetBooleanExpr(recodeRanges[i].EqualValue));
                            break;
                        case "RecodeM":
                            //<Recode_M>							::= LOVALUE '-' HIVALUE '=' Boolean
                            queryResult.Append("");
                            break;
                        case "RecodeN":
                            //<Recode_N>							::= ELSE '=' Boolean
                            queryResult.Append(" true, " + GetBooleanExpr(recodeRanges[i].EqualValue));
                            break;
                        default:
                            //Should not be here
                            break;
                    }
                }
                string s = queryResult.ToString();
                s = s.Substring(0, s.Length - 2) + ")";
                return s;
            }

        private string getExpressionStatement(IVariable variable, IVariable assignToVariable, List<Epi.Core.Interpreter.Rules.RecodeRange> recodeRanges)
        {
            StringBuilder queryResult = new StringBuilder();
            string singlequote = string.Empty;
            if (variable.DataType == DataType.Text)
                singlequote = "'";
            queryResult.Append("(");
            for (int i = 0; i < recodeRanges.Count; i++)
            {
                switch (recodeRanges[i].RecodeType)
                {
                    case "RecodeA":
                        //<Recode_A> 							::= <Literal> '-' <Literal> '=' <Literal>
                        queryResult.Append("((" + variable.Name + ") > " + singlequote + recodeRanges[i].Value1 + singlequote + " AND (" + variable.Name + ") <= " + singlequote + recodeRanges[i].Value2 + singlequote
                            + ") OR " );
                        break;
                    case "RecodeB":
                        //<Recode_B>							::= <Literal> '=' <Literal>
                        queryResult.Append("((" + variable.Name + ") = " + singlequote + recodeRanges[i].Value1 + singlequote
                            + ") OR ");
                        break;
                    case "RecodeC":
                        //<Recode_C>							::= Boolean '=' <Literal> 	
                        queryResult.Append("((" + variable.Name + ") " + GetBooleanExpr(recodeRanges[i].Value1)
                            + ") OR ");
                        break;
                    case "RecodeD":
                        //<Recode_D>							::= LOVALUE '-' <Literal> '=' <Literal>
                        queryResult.Append("((" + variable.Name + ") <= " + singlequote + recodeRanges[i].Value2 + singlequote
                            + ") OR ");
                        break;
                    case "RecodeE":
                        //<Recode_E>							::= <Literal> '-' HIVALUE '=' <Literal>
                        queryResult.Append("((" + variable.Name + ") > " + singlequote + recodeRanges[i].Value1 + singlequote
                            + ") OR ");
                        break;
                    case "RecodeF":
                        //<Recode_F>							::= LOVALUE '-' HIVALUE '=' <Literal>
                        queryResult.Append("");
                        break;
                    case "RecodeG":
                        //<Recode_G>							::= ELSE '=' <Literal>
                        //do nothing
                        break;
                    case "RecodeH":
                        //<Recode_H>							::= <Literal> '-' <Literal> '=' Boolean
                        queryResult.Append("((" + variable.Name + ") > " + singlequote + recodeRanges[i].Value1 + singlequote + " AND (" + variable.Name + ")<=" + singlequote + recodeRanges[i].Value2 + singlequote
                            + ") OR ");
                        break;
                    case "RecodeI":
                        //<Recode_I>							::= <Literal> '=' Boolean
                        queryResult.Append("((" + variable.Name + ") = " + singlequote + recodeRanges[i].Value1 + singlequote
                            + ") OR ");
                        break;
                    case "RecodeJ":
                        //<Recode_J>							::= Boolean '=' Boolean
                        queryResult.Append("((" + variable.Name + ") = " + Boolean.Parse(recodeRanges[i].Value1)
                            + ") OR ");
                        break;
                    case "RecodeK":
                        //<Recode_K>							::= LOVALUE '-' <Literal> '=' Boolean
                        queryResult.Append("((" + variable.Name + ") < " + singlequote + recodeRanges[i].Value2 + singlequote
                            + ") OR ");
                        break;
                    case "RecodeL":
                        //<Recode_L>							::= <Literal> '-' HIVALUE '=' Boolean
                        queryResult.Append("((" + variable.Name + ") > " + singlequote + recodeRanges[i].Value1 + singlequote
                            + ") OR ");
                        break;
                    case "RecodeM":
                        //<Recode_M>							::= LOVALUE '-' HIVALUE '=' Boolean
                        //queryResult.Append("");
                        break;
                    case "RecodeN":
                        //<Recode_N>							::= ELSE '=' Boolean
                        queryResult.Append("(True) OR ");
                        break;
                    default:
                        //Should not be here
                        break;
                }
            }
            string s = queryResult.ToString();
            s = s.Substring(1, s.Length - 4) ;   
            return s;
        }*/
        //private bool getBoolean(string pEpiBool)
        //{
        //    switch (pEpiBool.ToUpper() )
        //    {
        //        case "(+)":
        //            return true; 
        //        case "(-)":
        //            return false;
        //        case  "(.)":
        //            return false;
        //    }

        //}
        #endregion
        #region Public Methods
        /*    /// <summary>
            /// Hide the sytax differences in Jet SQL
            /// </summary>
            /// <param name="variable"></param>
            /// <param name="assignToVariable"></param>
            /// <param name="rangeNames"></param>
            /// <returns></returns>
        public override string GetRecodeCaseSubQuery(IVariable variable, IVariable assignToVariable, List<Epi.Core.Interpreter.Rules.RecodeRange> recodeRanges)
        {
            int iCount = recodeRanges.Count;
            int iGroup = iCount / 10;
            int iRemainder = iCount % 10;
            List<Epi.Core.Interpreter.Rules.RecodeRange> sublistRange = new List<Epi.Core.Interpreter.Rules.RecodeRange>();
            List < List<Epi.Core.Interpreter.Rules.RecodeRange>> listRange = new List<List<Epi.Core.Interpreter.Rules.RecodeRange>>();
            int j;
            for (int i = 0; i <= iCount-1; i++)
            {
                j = i % 10;
                if (j == 9)
                {
                    sublistRange.Add(recodeRanges[i]);
                    listRange.Add(sublistRange);
                    sublistRange = new List<Epi.Core.Interpreter.Rules.RecodeRange>();
                }
                else
                {
                    sublistRange.Add(recodeRanges[i]);
                    if (i == iCount-1)
                    {
                        listRange.Add(sublistRange);
                        //sublistRange.Clear();
                        break; 
                    }
                }
            }
            StringBuilder recodeQuery = new StringBuilder();
            queryResult.Append("SWITCH(");
            string subExpression;
            string subSwitchQuery; 
            for (int i = 0; i <= listRange.Count-1; i++)
            {
                //sublistRange.Clear();
                //sublistRange = listRange[i];
                subExpression = getExpressionStatement(variable, assignToVariable, listRange[i]);
                queryResult.Append(subExpression + ",");
                subSwitchQuery = getSwitchStatement(variable, assignToVariable, listRange[i]);
                queryResult.Append(subSwitchQuery + ",");
            }

            string s = queryResult.ToString();
            s = s.Substring(0, s.Length - 1) + ")";
            return s;
            }*/

        /// <summary>
        /// Get Insert Valud
        /// </summary>
        /// <param name="pvalues">values</param>
        /// <returns>string</returns>
            public override string GetInsertValue(string pvalues)
            {
                return string.Empty;
            }
        #endregion
    }
}
