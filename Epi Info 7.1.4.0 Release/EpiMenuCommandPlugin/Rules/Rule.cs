using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using com.calitha.goldparser;


namespace EpiMenu.CommandPlugin
{
    public abstract class Rule
    {
        public Rule_Context Context;

        #region Constructors

        public Rule() 
        {
            this.Context = new Rule_Context();
        }

        public Rule(Rule_Context pContext)
        {
            this.Context = pContext;
        }

        /*
        public Rule(IMemoryRegion currentModule)
        {
            this.Context = new Rule_Context(currentModule);
        }*/


        #endregion Constructors

        public abstract object Execute();

        protected string BoolVal(bool isTrue)
        {
            if (isTrue)
            {
                return "true";
            }
            else
            {
                return "false";
            }
        }

        protected bool IsNumeric(string value)
        {

            System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex(@"^[-+]?\d+\.?\d*$|^[-+]?\d*\.?\d+$");

            return re.IsMatch(value);
        }
        /*
        /// <summary>
        /// Returns the DataType enumeration for the data type name passed 
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns>DataType</returns>
        protected DataType GetDataType(string typeName)
        {
            DataType type = DataType.Unknown;
            try
            {
                foreach (Epi.DataSets.AppDataSet.DataTypesRow row in AppData.Instance.DataTypesDataTable.Rows)
                {
                    // save a dereference
                    string expression = row.Expression;

                    if (!string.IsNullOrEmpty(expression) && (string.Compare(typeName, expression, true) == 0))
                    {
                        return (type = ((DataType)row.DataTypeId));
                    }
                }
            }
            catch (Exception)
            {
            }
            return type;
        }*/

        // zack 1/10/2008, for getting command parameter element without parsing strings
        /// <summary>
        /// Developer can get Command parameter element without parsing tokens
        /// </summary>
        /// <param name="tokens">tokens parameter from the parser</param>
        /// <param name="index">Element Index in the command</param>
        /// <returns></returns>
        protected string GetCommandElement(Token[] tokens, int index)
        {
            if (tokens[0] is NonterminalToken)
            {
                if (((NonterminalToken)tokens[0]).Tokens[index] is NonterminalToken)
                {
                    return ExtractTokens(((NonterminalToken)((NonterminalToken)tokens[0]).Tokens[index]).Tokens).Trim();
                }
                else
                {
                    return (((NonterminalToken)tokens[0]).Tokens[index]).ToString();
                }
            }
            else
            {
                if (tokens[index] is NonterminalToken)
                {
                    return ExtractTokens(((NonterminalToken)tokens[index]).Tokens).Trim();
                }
                else
                {
                    return (tokens[index]).ToString();
                }
            }
        }

        /// <summary>
        /// Build a string composed of all tokens in tree.
        /// </summary>
        /// <param name="tokens">tokens</param>
        /// <returns>string</returns>
        protected string ExtractTokens(Token[] tokens)
        {
            int max = tokens.GetUpperBound(0);
            string tokensInTree = "";
            for (int i = tokens.GetLowerBound(0); i <= max; i++)
            {
                if (tokens[i] is NonterminalToken)
                {
                    tokensInTree += ExtractTokens(((NonterminalToken)tokens[i]).Tokens);
                }
                else
                {
                    tokensInTree += tokens[i].ToString() + " ";
                }
            }
            return tokensInTree;
        }


        private static bool FoundName(string[] varNames, string name)
        {
            foreach (string variableName in varNames)
            {
                if (string.Compare(variableName, name, true) == 0)
                {
                    return true;
                }
            }
            return false;
        }


    }
}
