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
using Epi.Core.AnalysisInterpreter.Rules;

namespace Epi.Core.AnalysisInterpreter
{
    public abstract class AnalysisRule
    {
        protected List<string> NumericTypeList = new List<string>(new string[] { "INT", "FLOAT", "INT16", "INT32", "INT64", "SINGLE", "DOUBLE", "BYTE", "DECIMAL" });
        protected Rule_Context Context;

        public AnalysisRule()
        {
            Context = new Rule_Context();
        }

        public AnalysisRule(Rule_Context context)
        {
            Context = context;
        }

        public AnalysisRule(IMemoryRegion currentModule)
        {
            Context = new Rule_Context(currentModule);
        }

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
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^[-+]?\d+\.?\d*$|^[-+]?\d*\.?\d+$");
            return regex.IsMatch(value);
        }

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
                    string expression = row.Expression;

                    if (!string.IsNullOrEmpty(expression) && (string.Compare(typeName, expression, true) == 0))
                    {
                        return (type = ((DataType)row.DataTypeId));
                    }
                }
            }
            catch
            {
            }
            
            return type;
        }

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

        /// <summary>
        /// creates a negateexp from a token
        /// </summary>
        /// <returns>Rule_NegateExp</returns>
        public string CreateNegateRecode(Token token)
        {
            NonterminalToken nonterminal = (NonterminalToken)((NonterminalToken)token).Tokens[0];
            Rule_NegateExp result = new Rule_NegateExp(Context, nonterminal);
            return result.Execute().ToString();
        }

        static public AnalysisRule BuildStatments(Rule_Context context, Token token)
        {
            AnalysisRule result = null;
            if (token is NonterminalToken)
            {
                NonterminalToken nonterminal = (NonterminalToken)token;
                switch (nonterminal.Symbol.ToString())
                {
                    case "<Statements>":
                        result = new Rule_Statements(context, nonterminal);
                        break;
                    case "<Statement>":
                        result = new Rule_Statement(context, nonterminal);
                        break;
                    case "<Always_Statement>":
                        result = new Rule_Always(context, nonterminal);
                        break;
                    case "<Simple_Assign_Statement>":
                    case "<Let_Statement>":
                    case "<Assign_Statement>":
                        result = new Rule_Assign(context, nonterminal);
                        break;
                    case "<Assign_DLL_Statement>":
                        result = new Rule_Assign_DLL_Statement(context, nonterminal);
                        break;
                    case "<If_Statement>":
                    case "<If_Else_Statement>":
                        result = new Rule_If_Then_Else_End(context, nonterminal);
                        break;
                    case "<Define_Variable_Statement>":
                        result = new Rule_Define(context, nonterminal);
                        break;
                    case "<FuncName2>":
                    case "<FunctionCall>":
                        result = new Rule_FunctionCall(context, nonterminal);
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
                    case "<Dialog_Date_Mask_Statement>":
                        result = new Rule_Dialog(context, nonterminal);
                        break;
                    case "<Read_Epi_Statement>":
                    case "<Simple_Read_Statement>":
                    case "<Read_Epi_File_Spec_Statement>":
                    case "<Read_Sql_Statement>":
                    case "<Read_Excel_File_Statement>":
                    case "<Read_Db_Table_Statement>":
                        result = new Rule_Read(context, nonterminal);
                        break;
                    case "<Merge_Table_Statement>":
                    case "<Merge_Db_Table_Statement>":
                    case "<Merge_File_Statement>":
                    case "<Merge_Excel_File_Statement>":
                        result = new Rule_Merge(context, nonterminal);
                        break;
                    case "<Write_All_Statement>":
                    case "<Write_Some_Statement>":
                    case "<Write_Except_Statement>":
                        result = new Rule_Write(context, nonterminal);
                        break;
                    case "<Select_Statement>":
                    case "<Cancel_Select_By_Selecting_Statement>":
                    case "<Cancel_Select_Statement>":
                        result = new Rule_Select(context, nonterminal);
                        break;
                    case "<Recode_Statement>":
                        result = new Rule_Recode(context, nonterminal);
                        break;
                    case "<Comment_Line>":
                        result = new Rule_CommentLine(context, nonterminal);
                        break;
                    case "<Execute_Statement>":
                        result = new Rule_Execute(context, nonterminal);
                        break;
                    case "<Report_Display_Statement>":
                    case "<Report_File_Statement>":
                    case "<Report_Print_Statement>":
                        result = new Rule_Report(context, nonterminal);
                        break;
                    case "<List_Statement>":
                        result = new Rule_List(context, nonterminal);
                        break;
                    case "<Simple_Tables_Statement>":
                    case "<Tables_One_Variable_Statement>":
                    case "<Row_All_Tables_Statement>":
                    case "<Row_Except_Tables_Statement>":
                    case "<Column_All_Tables_Statement>":
                    case "<Column_Except_Tables_Statement>":
                    case "<Row_Column_Tables_Statement>":
                        result = new Rule_Tables(context, nonterminal);
                        break;
                    case "<Freq_All_Statement>":
                    case "<Freq_Columns_Statement>":
                    case "<Freq_All_Except_Statement>":
                        result = new Rule_Freq(context, nonterminal);
                        break;
                    case "<Cancel_Sort_By_Sorting_Statement>":
                    case "<Cancel_Sort_Statement>":
                    case "<Sort_Statement>":
                        result = new Rule_Sort(context, nonterminal);
                        break;
                    case "<Means_Statement>":
                        result = new Rule_Means(context, nonterminal);
                        break;
                    case "<Relate_Epi_Table_Statement>":
                    case "<Relate_Table_Statement>":
                    case "<Relate_Db_Table_With_Identifier_Statement>":
                    case "<Relate_Db_Table_Statement>":
                    case "<Relate_File_Statement>":
                    case "<Relate_Excel_File_Statement>":
                        result = new Rule_Relate(context, nonterminal);
                        break;
                    case "<Beep_Statement>":
                        result = new Rule_Beep(context, nonterminal);
                        break;
                    case "<Quit_Statement>":
                        result = new Rule_Quit(context, nonterminal);
                        break;
                    case "<Delete_Table_Long_Statement>":
                        result = new Rule_Delete_Table_Long(context, nonterminal);
                        break;
                    case "<Delete_File_Statement>":
                        result = new Rule_Delete_File(context, nonterminal);
                        break;
                    case "<Delete_Table_Statement>":
                        result = new Rule_Delete_Table(context, nonterminal);
                        break;
                    case "<Type_Out_String_Statement>":
                    case "<Type_Out_File_Statement>":
                    case "<Type_Out_String_With_Font_Statement>":
                    case "<Type_Out_File_With_Font_Statement>":
                        result = new Rule_Typeout(context, nonterminal);
                        break;
                    case "<Simple_Routeout_Statement>":
                    case "<Replace_Routeout_Statement>":
                    case "<Append_Routeout_Statement>":
                        result = new Rule_RouteOut(context, nonterminal);
                        break;
                    case "<Close_Out_Statement>":
                        result = new Rule_CloseOut(context, nonterminal);
                        break;
                    case "<Variables_Display_Statement>":
                    case "<Views_Display_Statement>":
                    case "<Tables_Display_Statement>":
                        result = new Rule_Display(context, nonterminal);
                        break;
                    case "<Header_Title_String_Statement>":
                    case "<Header_Title_Font_Statement>":
                    case "<Header_Title_String_And_Font_Statement>":
                        result = new Rule_Header(context, nonterminal);
                        break;
                    case "<Simple_Undefine_Statement>":
                        result = new Rule_Undefine(context, nonterminal);
                        break;
                    case "<Run_File_PGM_Statement>":
                        result = new Rule_Run_File_PGM_Statement(context, nonterminal);
                        break;
                    case "<Run_String_Statement>":
                        result = new Rule_Run_String_Statement(context, nonterminal);
                        break;
                    case "<Run_PGM_In_Db_Statement>":
                        result = new Rule_Run_PGM_In_Db_Statement(context, nonterminal);
                        break;
                    case "<Delete_Records_All_Statement>":
                        result = new Rule_Delete_Records_All_Statement(context, nonterminal);
                        break;
                    case "<Delete_Records_Selected_Statement>":
                        result = new Rule_Delete_Records_Selected_Statement(context, nonterminal);
                        break;
                    case "<Simple_Print_Out_Statement>":
                    case "<File_Print_Out_Statement>":
                        result = new Rule_Printout(context, nonterminal);
                        break;
                    case "<SQL_Execute_Command>":
                        result = new Rule_SQLExec(context, nonterminal);
                        break;
                    case "<RecordSet_Command>":
                        result = new Rule_RecordSet(context, nonterminal);
                        break;
                    case "<Define_Connection_Command>":
                        result = new Rule_Define_Connection(context, nonterminal);
                        break;
                    case "<Regress_Statement>":
                        result = new Rule_LinearRegression(context, nonterminal);
                        break;
                    case "<Logistic_Statement>":
                        result = new Rule_LogisticRegression(context, nonterminal);
                        break;
                    case "<CoxPH_Statement>":
                        result = new Rule_CoxPH(context, nonterminal);
                        break;
                    case "<KM_Survival_Statement>":
                        result = new Rule_KMSurvival(context, nonterminal);
                        break;
                    case "<Define_Dll_Statement>":
                        result = new Rule_DLL_Statement(context, nonterminal);
                        break;
                    case "<Define_Group_Statement>":
                        result = new Rule_Define_Group(context, nonterminal);
                        break;
                    case "<Summarize_Statement>":
                        result = new Rule_Summarize(context, nonterminal);
                        break;
                    case "<Set_Statement>":
                        result = new Rule_Set(context, nonterminal);
                        break;
                    case "<Match_Row_All_Statement>":
                    case "<Match_Row_Except_Statement>":
                    case "<Match_Column_All_Statement>":
                    case "<Match_Column_Except_Statement>":
                    case "<Match_Row_Column_Statement>":
                        result = new Rule_Match(context, nonterminal);
                        break;
                    case "<Graph_Statement>":
                    case "<Simple_Graph_Statement>":
                    case "<Strata_Var_Graph_Statement>":
                    case "<Weight_Var_Graph_Statement>":
                    case "<Stra_Wei_Var_Graph_Statement>":
                    case "<Graph_Opt_1_Statement>":
                    case "<Graph_Opt_2_Statement>":
                    case "<Graph_Opt_3_Statement>":
                    case "<Graph_Opt_4_Statement>":
                    case "<Graph_Opt_5_Statement>":
                    case "<Graph_Generic_Opt_Statement>":
                        result = new Rule_Graph(context, nonterminal);
                        break;
                    case "<Expr List>":
                        result = new Rule_ExprList(context, nonterminal);
                        break;
                    case "<Expression>":
                        result = new Rule_Expression(context, nonterminal);
                        break;
                    case "<And Exp>":
                        result = new Rule_AndExp(context, nonterminal);
                        break;
                    case "<Not Exp>":
                        result = new Rule_NotExp(context, nonterminal);
                        break;
                    case "<Compare Exp>":
                        result = new Rule_CompareExp(context, nonterminal);
                        break;
                    case "<Concat Exp>":
                        result = new Rule_ConcatExp(context, nonterminal);
                        break;
                    case "<Add Exp>":
                        result = new Rule_AddExp(context, nonterminal);
                        break;
                    case "<Mult Exp>":
                        result = new Rule_MultExp(context, nonterminal);
                        break;
                    case "<Pow Exp>":
                        result = new Rule_PowExp(context, nonterminal);
                        break;
                    case "<Negate Exp>":
                        result = new Rule_NegateExp(context, nonterminal);
                        break;
                    default:
                    case "<Value>":
                        result = new Rule_Value(context, nonterminal);
                        break;
                    case "<Undelete_All_Statement>":
                        result = new Rule_UnDelete(context, nonterminal);
                        break;
                    case "<QualifiedIdList>":
                        result = new Rule_QualifiedIdList(context, nonterminal);
                        break;
                    case "<Subroutine_Statement>":
                        result = new Rule_Subroutine_Statement(context, nonterminal);
                        break;

                    //**these are not yet implemented; move up when completed    
                    //**these are not yet implemented; move up when completed    
                    //**these are not yet implemented; move up when completed    
                    //**these are not yet implemented; move up when completed    
                    //**these are not yet implemented; move up when completed    

                    case "<Simple_Run_Statement>":
                    case "<About_Statement>":
                    case "<Browser_Statement>":
                    case "<Browser_Size_Statement>":
                    case "<Button_Offset_Size_1_Statement>":
                    case "<Button_Offset_Size_2_Statement>":
                    case "<Button_Offset_1_Statement>":
                    case "<Button_Offset_2_Statement>":
                    case "<Call_Statement>":
                    case "<Simple_CMD_Statement>":
                    case "<CMD_Line_Statement>":
                    case "<Delete_Table_Short_Statement>":
                    case "<Exit_Statement>":
                    case "<File_Dialog_Statement>":
                    case "<Get_Path_Statement>":
                    case "<Help_File_Statement>":
                    case "<Simple_Help_Statement>":
                    case "<Link_Statement>":
                    case "<Link_Remove_Statement>":
                    case "<Map_AVG_Statement>":
                    case "<Map_Case_Statement>":
                    case "<Map_Sum_Statement>":
                    case "<Map_Count_Statement>":
                    case "<Map_Min_Statement>":
                    case "<Map_Max_Statement>":
                    case "<Map_Opt_1_Statement>":
                    case "<Map_Opt_2_Statement>":
                    case "<Map_Opt_3_Statement>":
                    case "<Map_Opt_4_Statement>":
                    case "<Map_Opt_5_Statement>":
                    case "<Menu_Statement>":
                    case "<Menu_Command_Statement>":
                    case "<Menu_Dialog_Statement>":
                    case "<Menu_Execute_Statement>":
                    case "<Menu_Item_Block_Name_Statement>":
                    case "<Menu_Item_Separator_Statement>":
                    case "<Menu_Replace_Statement>":
                    case "<Move_Buttons_Statement>":
                    case "<New_Page_Statement>":
                    case "<On_Browser_Exit_Block>":
                    case "<Picture_Statement>":
                    case "<Picture_Size_Statement>":
                    case "<Popup_Statement>":
                    case "<Repeat_Statement>":
                    case "<Screen_Text_Statement>":
                    case "<Set_Buttons_Statement>":
                    case "<Set_DB_Version_Statement>":
                    case "<Set_DOS_Win_Statement>":
                    case "<Set_Import_Year_Statement>":
                    case "<Set_Ini_Dir_Statement>":
                    case "<Set_Language_Statement>":
                    case "<Set_Picture_Statement>":
                    case "<Set_Work_Dir_Statement>":
                    case "<ShutDown_Block>":
                    case "<Startup_Block>":
                    case "<Sys_Info_Statement>":
                    case "<All_Standard_Undefine_Statement>":
                    case "<All_Global_Undefine_Statement>":
                    case "<Wait_For_Statement>":
                    case "<Wait_For_Exit_Statement>":
                    case "<Wait_For_File_Exists_Statement>":
                    case "<Command_Block>":
                    case "<On_Browser_Exit_Empty_Block>":
                    case "<Startup_Empty_Block>":
                    case "<ShutDown_Empty_Block>":
                    case "<Menu_Empty_Block>":
                    case "<Popup_Empty_Block>":
                    case "<Empty_Command_Block>":
                        break;
                }
            }
            else // terminal token
            {
                TerminalToken terminal = (TerminalToken)token;

                switch (terminal.Symbol.ToString())
                {
                    case "<Value>":
                    default:
                        result = new Rule_Value(context, terminal);
                        break;
                }
            }

            return result;
        }

        static public List<AnalysisRule> GetFunctionParameters(Rule_Context context, Token token)
        {
            List<AnalysisRule> result = new List<AnalysisRule>();

            if (token is NonterminalToken)
            {
                NonterminalToken nonterminal = (NonterminalToken)token;

                switch (nonterminal.Symbol.ToString())
                {
                    case "<NonEmptyFunctionParameterList>":
                        result.AddRange(AnalysisRule.GetFunctionParameters(context, nonterminal));
                        break;
                    case "<SingleFunctionParameterList>":
                        result.AddRange(AnalysisRule.GetFunctionParameters(context, nonterminal));
                        break;
                    case "<EmptyFunctionParameterList>":
                        break;
                    case "<MultipleFunctionParameterList>":
                        result.AddRange(AnalysisRule.GetFunctionParameters(context, nonterminal.Tokens[0]));
                        result.Add(AnalysisRule.BuildStatments(context, nonterminal.Tokens[2]));
                        break;
                    case "<FuncName2>":
                    case "<Expression>":
                    case "<expression>":
                    case "<FunctionCall>":
                    default:
                        result.Add(AnalysisRule.BuildStatments(context, nonterminal));
                        break;
                }
            }
            else
            {
                TerminalToken terminal = (TerminalToken)token;
                if (terminal.Text != ",")
                {
                    result.Add(new Rule_Value(context, token));
                }
            }

            return result;
        }

        /// <summary>
        /// Splits IdentefierList and escapes square brackets ie "[Poverty Rate], age"
        /// </summary>
        /// <param name="value"></param>
        /// <returns>string[]</returns>
        static public string[] SpliIdentifierList(string value)
        {
            List<string> result = new List<string>();
            StringBuilder temp = new StringBuilder();

            int Level = 0;

            foreach (char c in value)
            {
                if (c == ' ' && Level == 0)
                {
                    result.Add(temp.ToString().ToUpper());
                    temp.Length = 0;
                }
                else
                {
                    if (c == '[')
                    {
                        Level++;
                    }
                    else if (c == ']')
                    {
                        Level--;
                    }
                    else
                    {
                        temp.Append(c);
                    }
                }
            }
            result.Add(temp.ToString().ToUpper());
            return result.ToArray();
        }

        public string SetQualifiedId(Token token)
        {
            string result = null;

            if (token is NonterminalToken)
            {
                NonterminalToken nonterminal = (NonterminalToken)token;
                switch (nonterminal.Symbol.ToString())
                {
                    case "<Fully_Qualified_Id>":
                        result = this.SetFully_Qualified_Id(nonterminal);
                        break;
                    default:
                        result = this.GetCommandElement(nonterminal.Tokens, 0).Trim(new char[] { '[', ']' });
                        break;
                }
            }
            else
            {
                TerminalToken terminal = (TerminalToken)token;
                result = terminal.Text.Trim(new char[] { '[', ']' });
            }

            return result;
        }


        public string SetFully_Qualified_Id(NonterminalToken pToken)
        {
            string result = null;
            result = this.GetCommandElement(pToken.Tokens, 0).Trim(new char[] { '[', ']' }) + "." + this.SetQualifiedId(pToken.Tokens[2]);
            return result;
        }

        protected object ConvertStringToBoolean(string pValue)
        {
            object result = null;
            switch (pValue.ToUpper())
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
    }
}
