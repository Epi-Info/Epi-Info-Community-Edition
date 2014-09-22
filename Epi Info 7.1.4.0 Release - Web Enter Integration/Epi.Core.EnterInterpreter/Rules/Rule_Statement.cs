using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Statement : EnterRule 
    {
        private EnterRule reduction = null;

        public Rule_Statement(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            if (pToken.Tokens[0] is TerminalToken)
            {
                return;
            }
            NonterminalToken T = (NonterminalToken) pToken.Tokens[0];

            this.Context.ProgramText.Append(pToken.Tokens[0].ToString());

            this.reduction = EnterRule.BuildStatments(pContext, T);

            //switch (pToken.Rule.Rhs[0].ToString())
            //{
            //    // H1N1
            //    case "<Always_Statement>":
            //        this.reduction = new Rule_Always(pContext, T);
            //        break;

            //    case "<Simple_Assign_Statement>":
            //    case "<Let_Statement>":
            //    case "<Assign_Statement>":
            //        this.reduction = new Rule_Assign(pContext, T);
            //        break;
            //    case "<Assign_DLL_Statement>":
            //        this.reduction = new Rule_Assign_DLL_Statement(pContext, T);
            //        break;
            //    case "<If_Statement>":
            //    case "<If_Else_Statement>":
            //        this.reduction = new Rule_If_Then_Else_End(pContext, T);
            //        break;

            //    case "<Define_Variable_Statement>":
            //        this.reduction = new Rule_Define(pContext, T);
            //        break;

            //    case "<FunctionCall>":
            //        this.reduction = new Rule_FunctionCall(pContext, T);
            //        break;

            //    case "<Hide_Some_Statement>":
            //    case "<Hide_Except_Statement>":
            //        this.reduction = new Rule_Hide(pContext, T);
            //        break;

            //    case "<Unhide_Some_Statement>":
            //    case "<Unhide_Except_Statement>":
            //        this.reduction = new Rule_UnHide(pContext, T);
            //        break;

            //    case "<Go_To_Variable_Statement>":
            //    case "<Go_To_Page_Statement>":
            //        this.reduction = new Rule_GoTo(pContext, T);
            //        break;

            //    case "<Simple_Dialog_Statement>":
            //    case "<Numeric_Dialog_Implicit_Statement>":
            //    case "<Numeric_Dialog_Explicit_Statement>":
            //    case "<TextBox_Dialog_Statement>":
            //    case "<Db_Values_Dialog_Statement>":
            //    case "<YN_Dialog_Statement>":
            //    case "<Db_Views_Dialog_Statement>":
            //    case "<Databases_Dialog_Statement>":
            //    case "<Db_Variables_Dialog_Statement>":
            //    case "<Multiple_Choice_Dialog_Statement>":
            //    case "<Dialog_Read_Statement>":
            //    case "<Dialog_Write_Statement>":
            //    case "<Dialog_Read_Filter_Statement>":
            //    case "<Dialog_Write_Filter_Statement>":
            //    case "<Dialog_Date_Statement>":
            //    case "<Dialog_Date_Mask_Statement>":
            //        this.reduction = new Rule_Dialog(pContext, T);
            //        break;
            //    //case "<Comment_Line>":
            //    //    this.reduction = new Rule_CommentLine(pContext, T);
            //    //    break;
            //    case "<Simple_Execute_Statement>":
            //    case "<Execute_File_Statement>":
            //    case "<Execute_Url_Statement>":
            //    case "<Execute_Wait_For_Exit_File_Statement>":
            //    case "<Execute_Wait_For_Exit_String_Statement>":
            //    case "<Execute_Wait_For_Exit_Url_Statement>":
            //    case "<Execute_No_Wait_For_Exit_File_Statement>":
            //    case "<Execute_No_Wait_For_Exit_String_Statement>":
            //    case "<Execute_No_Wait_For_Exit_Url_Statement>":
            //        this.reduction = new Rule_Execute(pContext, T);
            //        break;
            //    case "<Beep_Statement>":
            //        this.reduction = new Rule_Beep(pContext, T);
            //        break;
            //    case "<Auto_Search_Statement>":
            //        this.reduction = new Rule_AutoSearch(pContext, T);
            //        break;
            //    case "<Quit_Statement>":
            //        this.reduction = new Rule_Quit(pContext);
            //        break;
            //    case "<Clear_Statement>":
            //        this.reduction = new Rule_Clear(pContext, T);
            //        break;
            //    case "<New_Record_Statement>":
            //        this.reduction = new Rule_NewRecord(pContext, T);
            //        break;
            //    case "<Simple_Undefine_Statement>":
            //        this.reduction = new Rule_Undefine(pContext, T);
            //        break;
            //    case "<Geocode_Statement>":
            //        this.reduction = new Rule_Geocode(pContext, T);
            //        break;
            //    case "<DefineVariables_Statement>":
            //        this.reduction = new Rule_DefineVariables_Statement(pContext, T);
            //        break;
            //    case "<Field_Checkcode_Statement>":
            //        this.reduction = new Rule_Field_Checkcode_Statement(pContext, T);
            //        break;
            //    case "<View_Checkcode_Statement>":
            //        this.reduction = new Rule_View_Checkcode_Statement(pContext, T);
            //        break;
            //    case "<Record_Checkcode_Statement>":
            //        this.reduction = new Rule_Record_Checkcode_Statement(pContext, T);
            //        break;
            //    case "<Page_Checkcode_Statement>":
            //        this.reduction = new Rule_Page_Checkcode_Statement(pContext, T);
            //        break;
            //    case "<Subroutine_Statement>":
            //        this.reduction = new Rule_Subroutine_Statement(pContext, T);
            //        break;
            //    case "<Call_Statement>":
            //        this.reduction = new Rule_Call(pContext, T);
            //        break;
            //    case "<Simple_Run_Statement>":
            //    default:
            //        break;
            //}
        }
        /// <summary>
        /// performs execution of and Epi rule statement
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            //System.Console.WriteLine("...executing Statement...");
            object result = null;
            if (this.reduction != null)
            {
                result = this.reduction.Execute();
            }
            return result;
        }
    }
}
