using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using com.calitha.goldparser;
using Epi;
using Epi.Collections;
using Epi.Data;
using Epi.Data.Services;
using Epi.Fields;
using VariableCollection = Epi.Collections.NamedObjectCollection<Epi.IVariable>;
using Epi.Core.EnterInterpreter.Rules;
using EpiInfo.Plugin;

namespace Epi.Core.EnterInterpreter
{
    public abstract class EnterRule : ICommand
    {
        protected List<string> NumericTypeList = new List<string>(new string[] { "INT", "FLOAT", "INT16", "INT32", "INT64", "SINGLE", "DOUBLE", "BYTE", "DECIMAL" });

        public Rule_Context Context;

        #region Constructors

        public EnterRule() 
        {
            //this.Context = new Rule_Context();
        }

        public EnterRule(Rule_Context pContext)
        {
            this.Context = pContext;
        }


        #endregion Constructors

        public abstract object Execute();


        public override string ToString()
        {
            return "";
        }

        public virtual bool IsNull() 
        { 
            return false; 
        }

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

        protected object ConvertStringToBoolean(string pValue)
        {
            object result = null;

            switch (pValue.ToUpperInvariant())
            {
                case "(+)":
                case "YES":
                case "Y":
                case "TRUE":
                case "T":
                    result = true;
                    break;

                case "(-)":
                case "NO":
                case "N":
                case "FALSE":
                case "F":
                    result = false;
                    break;
            }


            return result;
        }

        protected bool IsNumeric(string value)
        {

            System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex(@"^[-+]?\d+\.?\d*$|^[-+]?\d*\.?\d+$");

            return re.IsMatch(value);
        }

        /// <summary>
        /// Returns the DataType enumeration for the data type name passed 
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns>DataType</returns>
        protected EpiInfo.Plugin.DataType GetDataType(string typeName)
        {
            EpiInfo.Plugin.DataType type = EpiInfo.Plugin.DataType.Unknown;
            try
            {
                foreach (Epi.DataSets.AppDataSet.DataTypesRow row in AppData.Instance.DataTypesDataTable.Rows)
                {
                    // save a dereference
                    string expression = row.Expression;

                    if (!string.IsNullOrEmpty(expression) && (string.Compare(typeName, expression, true) == 0))
                    {
                        return (type = ((EpiInfo.Plugin.DataType)row.DataTypeId));
                    }
                }
            }
            catch (Exception)
            {
            }
            return type;
        }

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

        /// <summary>
        /// Build a string composed of all tokens in tree.
        /// </summary>
        /// <param name="tokens">tokens</param>
        /// <returns>string</returns>
        protected string ExtractTokensWithFormat(Token[] tokens)
        {
            int max = tokens.GetUpperBound(0);
            string tokensInTree = "";
            for (int i = tokens.GetLowerBound(0); i <= max; i++)
            {
                if (tokens[i] is NonterminalToken)
                {
                    tokensInTree += "\t" + ExtractTokensWithFormat(((NonterminalToken)tokens[i]).Tokens);
                }
                else
                {
                    tokensInTree += tokens[i].ToString() + " ";
                }
            }
            return tokensInTree + "\n";
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

        /// <summary>
        /// creates a negateexp from a token
        /// </summary>
        /// <returns>Rule_NegateExp</returns>
        public string CreateNegateRecode(Token pToken)
        {
            NonterminalToken T = (NonterminalToken)((NonterminalToken)pToken).Tokens[0];
            Rule_NegateExp result = new Rule_NegateExp(this.Context, T);

            return result.Execute().ToString();
        }


        static public EnterRule BuildStatments(Rule_Context pContext, Token pToken)
        {
            EnterRule result = null;
            if (pToken is NonterminalToken)
            {
                NonterminalToken NT = (NonterminalToken)pToken;

                switch (NT.Symbol.ToString())
                {
                    // H1N1
                    case "<Program>":
                        result = new Rule_Program(pContext, NT);
                        break;
                    case "<Always_Statement>":
                        result  = new Rule_Always(pContext, NT);
                        break;

                    case "<Simple_Assign_Statement>":
                    case "<Let_Statement>":
                    case "<Assign_Statement>":
                        result  = new Rule_Assign(pContext, NT);
                        break;
                    case "<Assign_DLL_Statement>":
                        result  = new Rule_Assign_DLL_Statement(pContext, NT);
                        break;
                    case "<If_Statement>":
                    case "<If_Else_Statement>":
                        result  = new Rule_If_Then_Else_End(pContext, NT);
                        break;
                    case "<Else_If_Statement>":
                        result = new Rule_Else_If_Statement(pContext, NT);
                        break;
                    case "<Define_Variable_Statement>":
                        result  = new Rule_Define(pContext, NT);
                        break;
                    case "<Define_Dll_Statement>":
                        result = new Rule_DLL_Statement(pContext, NT);
                        break;
                    case "<FuncName2>":
                    case "<FunctionCall>":
                        result = new Rule_FunctionCall(pContext, NT);
                        break;

                    case "<Hide_Some_Statement>":
                    case "<Hide_Except_Statement>":
                        result  = new Rule_Hide(pContext, NT);
                        break;

                    case "<Unhide_Some_Statement>":
                    case "<Unhide_Except_Statement>":
                        result  = new Rule_UnHide(pContext, NT);
                        break;

                    case "<Go_To_Variable_Statement>":
                    case "<Go_To_Page_Statement>":
                    case "<Go_To_Form_Statement>":
                        result  = new Rule_GoTo(pContext, NT);
                        break;

                    case "<Simple_Dialog_Statement>":
                    case "<Numeric_Dialog_Implicit_Statement>":
                    case "<Numeric_Dialog_Explicit_Statement>":
                    case "<TextBox_Dialog_Statement>":
                    case "<Db_Values_Dialog_Statement>":
                    case "<YN_Dialog_Statement>":
                    case "<Db_Views_Dialog_Statement>":
                    case "<Databases_Dialog_Statement>":
                    case "<Db_Variables_Dialog_Statement>":
                    case "<Multiple_Choice_Dialog_Statement>":
                    case "<Dialog_Read_Statement>":
                    case "<Dialog_Write_Statement>":
                    case "<Dialog_Read_Filter_Statement>":
                    case "<Dialog_Write_Filter_Statement>":
                    case "<Dialog_Date_Statement>":
                    case "<Dialog_Time_Statement>":
                    case "<Dialog_DateTime_Statement>":
                    case "<Dialog_Date_Mask_Statement>":
                        result  = new Rule_Dialog(pContext, NT);
                        break;

                    case "<Comment_Line>":
                        result  = new Rule_CommentLine(pContext, NT);
                        break;

                    case "<Simple_Execute_Statement>":
                    case "<Execute_File_Statement>":
                    case "<Execute_Url_Statement>":
                    case "<Execute_Wait_For_Exit_File_Statement>":
                    case "<Execute_Wait_For_Exit_String_Statement>":
                    case "<Execute_Wait_For_Exit_Url_Statement>":
                    case "<Execute_No_Wait_For_Exit_File_Statement>":
                    case "<Execute_No_Wait_For_Exit_String_Statement>":
                    case "<Execute_No_Wait_For_Exit_Url_Statement>":
                        result  = new Rule_Execute(pContext, NT);
                        break;
                    case "<Beep_Statement>":
                        result  = new Rule_Beep(pContext, NT);
                        break;
                    case "<Auto_Search_Statement>":
                        result  = new Rule_AutoSearch(pContext, NT);
                        break;
                    case "<Quit_Statement>":
                        result  = new Rule_Quit(pContext);
                        break;
                    case "<Clear_Statement>":
                        result  = new Rule_Clear(pContext, NT);
                        break;
                    case "<New_Record_Statement>":
                        result  = new Rule_NewRecord(pContext, NT);
                        break;
                    case "<Save_Record_Statement>":
                        result = new Rule_SaveRecord(pContext, NT);
                        break;
                    case "<Simple_Undefine_Statement>":
                        result  = new Rule_Undefine(pContext, NT);
                        break;
                    case "<Geocode_Statement>":
                        result  = new Rule_Geocode(pContext, NT);
                        break;
                    case "<DefineVariables_Statement>":
                        result  = new Rule_DefineVariables_Statement(pContext, NT);
                        break;
                    case "<Field_Checkcode_Statement>":
                        result  = new Rule_Field_Checkcode_Statement(pContext, NT);
                        break;
                    case "<View_Checkcode_Statement>":
                        result  = new Rule_View_Checkcode_Statement(pContext, NT);
                        break;
                    case "<Record_Checkcode_Statement>":
                        result  = new Rule_Record_Checkcode_Statement(pContext, NT);
                        break;
                    case "<Page_Checkcode_Statement>":
                        result  = new Rule_Page_Checkcode_Statement(pContext, NT);
                        break;
                    case "<Subroutine_Statement>":
                        result  = new Rule_Subroutine_Statement(pContext, NT);
                        break;
                    case "<Call_Statement>":
                        result  = new Rule_Call(pContext, NT);
                        break;
                    case "<Expr List>":
                        result = new Rule_ExprList(pContext, NT);
                        break;
                    case "<Expression>":
                        result = new Rule_Expression(pContext, NT);
                        break;
                    case "<And Exp>":
                        result = new Rule_AndExp(pContext, NT);
                        break;
                    case "<Not Exp>":
                        result = new Rule_NotExp(pContext, NT);
                        break;
                    case "<Compare Exp>":
                        result = new Rule_CompareExp(pContext, NT);
                        break;
                    case "<Concat Exp>":
                        result = new Rule_ConcatExp(pContext, NT);
                        break;
                    case "<Add Exp>":
                        result = new Rule_AddExp(pContext, NT);
                        break;
                    case "<Mult Exp>":
                        result = new Rule_MultExp(pContext, NT);
                        break;
                    case "<Pow Exp>":
                        result = new Rule_PowExp(pContext, NT);
                        break;
                    case "<Negate Exp>":
                        result = new Rule_NegateExp(pContext, NT);
                        break;
                    case "<Begin_Before_statement>":
                        result = new Rule_Begin_Before_Statement(pContext, NT); 
                        break;
                    case "<Begin_After_statement>":
                        result = new Rule_Begin_After_Statement(pContext, NT);
                        break;
                    case "<Begin_Click_statement>":
                        result = new Rule_Begin_Click_Statement(pContext, NT);
                        break;
                    case "<CheckCodeBlock>":
                        result = new Rule_CheckCodeBlock(pContext, NT);
                        break;
                    case "<CheckCodeBlocks>":
                        result = new Rule_CheckCodeBlocks(pContext, NT);
                        break;
                    case "<Simple_Run_Statement>":
                        break;
                    case "<Statements>":
                        result = new Rule_Statements(pContext, NT);
                        break;
                    case "<Statement>":
                        result = new Rule_Statement(pContext, NT);
                        break;
                    case "<Define_Statement_Group>":
                        result = new Rule_Define_Statement_Group(pContext, NT);
                        break;
                    case "<Define_Statement_Type>":
                        result = new Rule_Define_Statement_Type(pContext, NT);
                        break;
                    case "<Highlight_Statement>":
                        result = new Rule_Highlight(pContext, NT);
                        break;
                    case "<UnHighlight_Statement>":
                        result = new Rule_UnHighlight(pContext, NT);
                        break;
                    case "<Enable_Statement>":
                        result = new Rule_Enable(pContext, NT);
                        break;
                    case "<Disable_Statement>":
                        result = new Rule_Disable(pContext, NT);
                        break;
                    case "<Set_Required_Statement>":
                        result = new Rule_SetRequired(pContext, NT);
                        break;
                    case "<Set_Not_Required_Statement>":
                        result = new Rule_SetNOTRequired(pContext, NT);
                        break;
                    case "<Value>":
                    default:
                        result = new Rule_Value(pContext, NT);
                        break;
                    
                        //result = new Rule_Value(pContext, NT);
                        //throw new Exception("Missing rule in EnterRule.BuildStatments " + NT.Symbol.ToString());
                        
                }


            }
            else // terminal token
            {
                TerminalToken TT = (TerminalToken)pToken;

                switch (TT.Symbol.ToString())
                {
                    case "<Value>":
                    default:
                        result = new Rule_Value(pContext, TT);
                        break;
                }
            }

            return result;
        }

        static public List<EnterRule> GetFunctionParameters(Rule_Context pContext, Token pToken)
        {
            List<EnterRule> result = new List<EnterRule>();

            if (pToken is NonterminalToken)
            {

                NonterminalToken NT = (NonterminalToken)pToken;

                switch (NT.Symbol.ToString())
                {
                    case "<NonEmptyFunctionParameterList>":


                        //this.paramList.Push(new Rule_NonEmptyFunctionParameterList(T, this.paramList));
                        result.AddRange(EnterRule.GetFunctionParameters(pContext, NT));
                        break;
                    case "<SingleFunctionParameterList>":

                        result.AddRange(EnterRule.GetFunctionParameters(pContext, NT));
                        break;
                    case "<EmptyFunctionParameterList>":
                        //this.paramList = new Rule_EmptyFunctionParameterList(T);
                        // do nothing the parameterlist is empty
                        break;
                    case "<MultipleFunctionParameterList>":

                        //this.MultipleParameterList = new Rule_MultipleFunctionParameterList(pToken);
                        //<NonEmptyFunctionParameterList> ',' <Expression>
                        //result.Add(AnalysisRule.BuildStatments(pContext, NT.Tokens[0]));
                        result.AddRange(EnterRule.GetFunctionParameters(pContext, NT.Tokens[0]));
                        result.Add(EnterRule.BuildStatments(pContext, NT.Tokens[2]));
                        break;
                    case "<FuncName2>":
                    case "<Expression>":
                    case "<expression>":
                    case "<FunctionCall>":
                    default:
                        result.Add(EnterRule.BuildStatments(pContext, NT));
                        break;
                }
            }
            else
            {
                TerminalToken TT = (TerminalToken)pToken;
                if (TT.Text != ",")
                {
                    result.Add(new Rule_Value(pContext, pToken));
                }
            }


            /*
                <FunctionCall> ::= Identifier '(' <FunctionParameterList> ')'
                   | FORMAT '(' <FunctionParameterList> ')'
                    | <FuncName2>
                !           | <FuncName1> '(' <FunctionCall> ')'
                <FunctionParameterList> ::= <EmptyFunctionParameterList> | <NonEmptyFunctionParameterList>
                <NonEmptyFunctionParameterList> ::= <MultipleFunctionParameterList> | <SingleFunctionParameterList>

                <MultipleFunctionParameterList> ::= <NonEmptyFunctionParameterList> ',' <Expression>
                <SingleFunctionParameterList> ::= <expression>
                <EmptyFunctionParameterList> ::=
             */
            /*
            foreach (Token T in pToken.Tokens)
            {
                if (T is NonterminalToken)
                {

                    NonterminalToken NT = (NonterminalToken)T;

                    switch (NT.Rule.Lhs.ToString())
                    {
                        case "<NonEmptyFunctionParameterList>":


                            //this.paramList.Push(new Rule_NonEmptyFunctionParameterList(T, this.paramList));
                            result.AddRange(EnterRule.GetFunctionParameters(pContext, NT));
                            break;
                        case "<SingleFunctionParameterList>":

                            result.AddRange(EnterRule.GetFunctionParameters(pContext, NT));
                            break;
                        case "<EmptyFunctionParameterList>":
                            //this.paramList = new Rule_EmptyFunctionParameterList(T);
                            // do nothing the parameterlist is empty
                            break;
                        case "<MultipleFunctionParameterList>":

                            //this.MultipleParameterList = new Rule_MultipleFunctionParameterList(pToken);
                            result.AddRange(EnterRule.GetFunctionParameters(pContext, NT));
                            break;
                        case "<FuncName2>":
                        case "<Expression>":
                        case "<expression>":
                        case "<FunctionCall>":

                            result.Add(EnterRule.BuildStatments(pContext, NT));
                            break;
                        default:
                            result.Add(new Rule_Value(pContext, T));
                            break;
                    }
                }
                else
                {
                    TerminalToken TT = (TerminalToken)T;
                    if (TT.Text != ",")
                    {
                        result.Add(new Rule_Value(pContext, TT));
                    }
                }
            }*/




            return result;
        }

        public void AddCommandVariableCheckValue(List<EnterRule> ParameterList, string commandVariableCheckValue)
        {
            if (ParameterList == null) return;
            if (commandVariableCheckValue == null) return;
            if (this.Context == null) return;
            if (this.Context.CommandVariableCheck == null) return;
            if (this.Context.IsVariableValidationEnable == false) return;
            try
            {
                if (ParameterList.Count > 0)
                {
                    foreach (var item in ParameterList)
                    {
                        if (item is Rule_Value)
                        {
                            var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
                            if (id != null && !this.Context.CommandVariableCheck.ContainsKey(id.ToLowerInvariant()))
                            {
                                this.Context.CommandVariableCheck.Add(id.ToLowerInvariant(), commandVariableCheckValue);
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }
}
