using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace EpiMenu.CommandPlugin
{
    public class Rule_Statement : Rule
    {
        private Rule reduction = null;

        public Rule_Statement(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {

            NonterminalToken T = (NonterminalToken) pToken.Tokens[0];

            if (!this.Context.RunOneCommand)
            {
                this.Context.ProgramText.Append(pToken.Tokens[0].ToString());

            }
            else
            {
                if (!this.Context.IsFirstStatement)
                {
                    this.Context.ProgramText.Length = 0;
                    this.Context.ProgramText.Append(this.ExtractTokens(pToken.Tokens));
                    this.Context.IsFirstStatement = true;
                }
            }

            /*
            switch (pToken.Rule.Rhs[0].ToString())
            {
                // H1N1
                case "<Always_Statement>":
                    this.reduction = new Rule_Always(pContext, T);
                    break;

                case "<Simple_Assign_Statement>":
                case "<Let_Statement>":
                case "<Assign_Statement>":
                    this.reduction = new Rule_Assign(pContext, T);
                    break;

                case "<If_Statement>":
                case "<If_Else_Statement>":
                    this.reduction = new Rule_If_Then_Else_End(pContext, T);
                    break;

                case "<Define_Variable_Statement>":
                    this.reduction = new Rule_Define(pContext, T);
                    break;

                case "<FunctionCall>":
                    this.reduction = new Rule_FunctionCall(pContext, T);
                    break;

                case "<Hide_Some_Statement>":
                case "<Hide_Except_Statement>":
                    this.reduction = new Rule_Hide(pContext, T);
                    break;

                case "<Unhide_Some_Statement>":
                case "<Unhide_Except_Statement>":
                    this.reduction = new Rule_UnHide(pContext, T);
                    break;

                case "<Go_To_Variable_Statement>":
                case "<Go_To_Page_Statement>":
                    this.reduction = new Rule_GoTo(pContext, T);
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
                    this.reduction = new Rule_Dialog(pContext, T);
                    break;
                
                case "<Read_Epi_Statement>":
                case "<Simple_Read_Statement>":
                case "<Read_Epi_File_Spec_Statement>":
                case "<Read_Sql_Statement>":
                case "<Read_Excel_File_Statement>":
                case "<Read_Db_Table_Statement>":
                    this.reduction = new Rule_Read_Epi_Statement(pContext, T);
                    break;

                case "<Merge_Table_Statement>":
                case "<Merge_Db_Table_Statement>":
                case "<Merge_File_Statement>":
                case "<Merge_Excel_File_Statement>":
                    this.reduction = new Rule_Merge(pContext, T);
                    break;

                case "<Write_All_Statement>":
                case "<Write_Some_Statement>":
                case "<Write_Except_Statement>":
                    this.reduction = new Rule_Write(pContext, T);
                    break;

                case "<Select_Statement>":
                case "<Cancel_Select_By_Selecting_Statement>":
                case "<Cancel_Select_Statement>":
                    this.reduction = new Rule_Select(pContext, T);
                    break;

                case "<Recode_Statement>":
                    this.reduction = new Rule_Recode(pContext, T);
                    break;
                case "<Comment_Line>":
                    this.reduction = new Rule_CommentLine(pContext, T);
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
                    this.reduction = new Rule_Execute(pContext, T);
                    break;

                case "<List_All_Except_Statement>":
                case "<List_All_Statement>":
                case "<List_Columns_Statement>":
                //case "<Simple_List_Statement>":
                    this.reduction = new Rule_List(pContext, T);
                    break;

                case "<Simple_Tables_Statement>":
                case "<Tables_One_Variable_Statement>":
                case "<Row_All_Tables_Statement>":
                case "<Row_Except_Tables_Statement>":
                case "<Column_All_Tables_Statement>":
                case "<Column_Except_Tables_Statement>":
                case "<Row_Column_Tables_Statement>":
                    this.reduction = new Rule_Tables(pContext, T);
                    break;

                case "<Freq_All_Statement>":
                case "<Freq_Columns_Statement>":
                case "<Freq_All_Except_Statement>":
                    this.reduction = new Rule_Freq(pContext, T);
                    break;

                case "<Cancel_Sort_By_Sorting_Statement>":
                case "<Cancel_Sort_Statement>":
                case "<Sort_Statement>":
                    this.reduction = new Rule_Sort(pContext, T);
                    break;

                case "<Simple_Means_Statement>":
                case "<Row_All_Means_Statement>":
                case "<Row_All_Column_Means_Statement>":
                case "<Row_Column_All_Means_Statement>":
                case "<Row_Column_Means_Statement>":
                    this.reduction = new Rule_Means(pContext, T);
                    break;

                case "<Relate_Table_Statement>":
                    this.reduction = new Rule_Relate_Table(pContext, T);
                    break;
                case "<Relate_Db_Table_With_Identifier_Statement>":
                    this.reduction = new Rule_Relate_Db_Table_Identifier(pContext, T);
                    break;
                case "<Relate_Epi_Table_Statement>":
                    this.reduction = new Rule_Relate_Epi_Table(pContext, T);
                    break;

                case "<Beep_Statement>":
                    this.reduction = new Rule_Beep(pContext, T);
                    break;
                
                case "<Auto_Search_Statement>":
                    this.reduction = new Rule_AutoSearch(pContext, T);
                    break;

                case "<Quit_Statement>":
                    this.reduction = new Rule_Quit();
                    break;

                case "<Clear_Statement>":
                    this.reduction = new Rule_Clear(pContext, T);
                    break;

                case "<Delete_Table_Long_Statement>":
                    this.reduction = new Rule_Delete_Table_Long(pContext, T);
                    break;
                case "<Delete_File_Statement>":
                    this.reduction = new Rule_Delete_File(pContext, T);
                    break;
                case "<Delete_Table_Statement>":
                    this.reduction = new Rule_Delete_Table(pContext, T);
                    break;

                case "<Type_Out_String_Statement>":
                case "<Type_Out_File_Statement>":
                case "<Type_Out_String_With_Font_Statement>":
                case "<Type_Out_File_With_Font_Statement>":
                    this.reduction = new Rule_Typeout(pContext, T);
                    break;

                case "<New_Record_Statement>":
                    this.reduction = new Rule_NewRecord(pContext, T);
                    break;

                case "<Simple_Routeout_Statement>":
                case "<Replace_Routeout_Statement>":
                case "<Append_Routeout_Statement>":
                    this.reduction = new Rule_RouteOut(pContext, T);
                    break;

                case "<Close_Out_Statement>":
                    this.reduction = new Rule_CloseOut(pContext, T);
                    break;

                case "<Variables_Display_Statement>":
                case "<Views_Display_Statement>":
                case "<Tables_Display_Statement>":
                    this.reduction = new Rule_Display(pContext, T);
                    break;

                case "<Header_Title_String_Statement>":
                case "<Header_Title_Font_Statement>":
                case "<Header_Title_String_And_Font_Statement>":
                    this.reduction = new Rule_Header(pContext, T);
                    break;


                case "<Simple_Undefine_Statement>":
                    this.reduction = new Rule_Undefine(pContext, T);
                    break;

                case "<Run_File_PGM_Statement>":
                    this.reduction = new Rule_Run_File_PGM_Statement(pContext, T);
                    break;
                case "<Run_String_Statement>":
                    this.reduction = new Rule_Run_String_Statement(pContext, T);
                    break;
                case "<Run_PGM_In_Db_Statement>":
                    this.reduction = new Rule_Run_PGM_In_Db_Statement(pContext, T);
                    break;


                case "<Delete_Records_All_Statement>":
                    this.reduction = new Rule_Delete_Records_All_Statement(pContext, T);
                    break;
                case "<Delete_Records_Selected_Statement>":
                    this.reduction = new Rule_Delete_Records_Selected_Statement(pContext, T);
                    break;


                case "<Simple_Print_Out_Statement>":
                case "<File_Print_Out_Statement>":
                    this.reduction = new Rule_Printout(pContext, T);
                    break;

                case "<Simple_Run_Statement>":

                //**these are not yet implemented; move up when completed    
                //**these are not yet implemented; move up when completed    
                //**these are not yet implemented; move up when completed    
                //**these are not yet implemented; move up when completed    
                //**these are not yet implemented; move up when completed    
                
                case "<Undelete_All_Statement>":
                case "<Undelete_Selected_Statement>":
                // Build 6

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
//!                case "<Simple_Cox_Statement>":
//!                case "<Boolean_Cox_Statement>":
                case "<Define_Dll_Statement>":
                case "<Define_Group_Statement>":
                case "<Delete_Table_Short_Statement>":

                case "<Exit_Statement>":
                case "<File_Dialog_Statement>":
                case "<Get_Path_Statement>":
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

                case "<Help_File_Statement>":
                case "<Simple_Help_Statement>":
                case "<Simple_KM_Survival_Statement>":
                case "<KM_Survival_Boolean_Statement>":
                case "<Logistic_Statement>":
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
                case "<Match_Row_All_Statement>":
                case "<Match_Row_Except_Statement>":
                case "<Match_Column_All_Statement>":
                case "<Match_Column_Except_Statement>":
                case "<Match_Row_Column_Statement>":

                case "<Menu_Statement>":
                case "<Menu_Command_Statement>":
                case "<Menu_Dialog_Statement>":
                case "<Menu_Execute_Statement>":
//!                case "<Menu_Help_Statement>":
//!                case "<Menu_If_Statement>":
                case "<Menu_Item_Block_Name_Statement>":
                case "<Menu_Item_Separator_Statement>":
                case "<Menu_Replace_Statement>":
                case "<Move_Buttons_Statement>":
                case "<New_Page_Statement>":
                case "<On_Browser_Exit_Block>":
                case "<Picture_Statement>":
                case "<Picture_Size_Statement>":
                case "<Popup_Statement>":

                case "<Regress_Statement>":
                case "<Relate_Db_Table_Statement>":
                case "<Relate_File_Statement>":
                case "<Relate_Excel_File_Statement>":
                case "<Repeat_Statement>":
//!                case "<Replace_Statement>":
                case "<Screen_Text_Statement>":
 
                case "<Set_Statement>":
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
                case "<Summarize_Statement>":
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
                default:
                    break;
            }*/
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
