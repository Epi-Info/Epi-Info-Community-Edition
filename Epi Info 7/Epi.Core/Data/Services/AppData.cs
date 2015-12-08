using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;
using Epi;
using Epi.Data;
using Epi.DataSets;

namespace Epi.Data.Services
{
	/// <summary>
	/// Manages AppData database.
	/// </summary>
	public class AppData 
	{		
		#region Private Data
		private  ArrayList fieldTypes = null;
		//private  DataSets.AppDataSet.SettingsDataTable settingsDataTable;
		//private  DataSets.AppDataSet.ModulesDataTable modulesDataTable;
		//private  DataSets.AppDataSet.RecordProcessingScopesDataTable recordProcessingScopesDataTable;
		//private  DataSets.AppDataSet.RepresentationsOfMissingDataTable representationsOfMissingDataTable;
		//private  DataSets.AppDataSet.RepresentationsOfNoDataTable representationsOfNoDataTable;
		//private  DataSets.AppDataSet.RepresentationsOfYesDataTable representationsOfYesDataTable;
		//private  DataSets.AppDataSet.StatisticsLevelsDataTable statisticsLevelsDataTable;
		//private  DataSets.AppDataSet.DataPatternsDataTable dataPatternsDataTable;
		//private  DataSets.AppDataSet.DataTypesDataTable dataTypesDataTable;
		//private  DataSets.AppDataSet.FieldTypesDataTable fieldTypesDataTable;
		//private  DataSets.AppDataSet.FontStylesDataTable fontStylesDataTable;
		//private  DataSets.AppDataSet.ListTreatmentTypesDataTable listTreatmentTypesDataTable;
		//private  DataSets.AppDataSet.SourceControlTypesDataTable sourceControlTypesDataTable;
		//private  DataSets.AppDataSet.CommandGroupsDataTable commandGroupsDataTable;
		//private  DataSets.AppDataSet.CommandsDataTable commandsDataTable;
		//private  DataSets.AppDataSet.DialogFormatsDataTable dialogFormatsDataTable;
		//private  DataSets.AppDataSet.VariableScopesDataTable variableScopesDataTable;
		//private  DataSets.AppDataSet.SupportedAggregatesDataTable supportedAggregatesDataTable;
		//private  DataSets.AppDataSet.ReservedWordsDataTable reservedWordsDataTable;
		//private DataSets.AppDataSet.LayerRenderTypesDataTable layerRenderTypesDataTable;
		
		#endregion Private Data

		#region Constructors
		private static AppData instance;
		/// <summary>
		/// Reference to AppData object.
		/// </summary>
		public static AppData Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new AppData();
				}
				return instance;
			}
		}       
	 
		#endregion Constructors

		#region Public Properties
		#region Generated Code
		private DataSets.AppDataSet.CommandGroupsDataTable commandGroupsDataTable;
		/// <summary>
		/// Command Groups Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.CommandGroupsDataTable CommandGroupsDataTable
		{
			get
			{
				if (commandGroupsDataTable == null)
				{
					commandGroupsDataTable = new DataSets.AppDataSet.CommandGroupsDataTable();
					commandGroupsDataTable.AddCommandGroupsRow(1, @"Data", 1);
					commandGroupsDataTable.AddCommandGroupsRow(2, @"Variables", 2);
					commandGroupsDataTable.AddCommandGroupsRow(3, @"Select/If", 3);
					commandGroupsDataTable.AddCommandGroupsRow(4, @"Statistics", 4);
					commandGroupsDataTable.AddCommandGroupsRow(5, @"Advanced Statistics", 5);
					commandGroupsDataTable.AddCommandGroupsRow(6, @"Output", 6);
					commandGroupsDataTable.AddCommandGroupsRow(7, @"User-Defined Commands", 7);
					commandGroupsDataTable.AddCommandGroupsRow(8, @"User Interaction", 8);
					commandGroupsDataTable.AddCommandGroupsRow(9, @"Options", 9);
				}
				return (commandGroupsDataTable);
			}
		}

		private DataSets.AppDataSet.CommandsDataTable commandsDataTable;
		/// <summary>
		/// Commands Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.CommandsDataTable CommandsDataTable
		{
			get
			{
				if (commandsDataTable == null)
				{
					commandsDataTable = new DataSets.AppDataSet.CommandsDataTable();
					commandsDataTable.AddCommandsRow(1, @"Read", 1, 1);
					//commandsDataTable.AddCommandsRow(2, @"SQLExec", 2, 1);
					//commandsDataTable.AddCommandsRow(4, @"RecordSet", 3, 1);
					commandsDataTable.AddCommandsRow(2, @"Relate", 2, 1);
					commandsDataTable.AddCommandsRow(3, @"Write (Export)", 3, 1);
					commandsDataTable.AddCommandsRow(4, @"Merge", 4, 1);
					commandsDataTable.AddCommandsRow(5, @"Delete File/Table", 5, 1);
					commandsDataTable.AddCommandsRow(6, @"Delete Records", 6, 1);
					commandsDataTable.AddCommandsRow(7, @"Undelete Records", 7, 1);
					commandsDataTable.AddCommandsRow(1, @"Define", 1, 2);
					commandsDataTable.AddCommandsRow(2, @"DefineGroup", 2, 2);
					commandsDataTable.AddCommandsRow(3, @"Undefine", 3, 2);
					commandsDataTable.AddCommandsRow(4, @"Assign", 4, 2);
					commandsDataTable.AddCommandsRow(5, @"Recode", 5, 2);
					commandsDataTable.AddCommandsRow(6, @"Display", 6, 2);
					
					
					commandsDataTable.AddCommandsRow(1, @"Select", 1, 3);
					commandsDataTable.AddCommandsRow(2, @"Cancel Select", 2, 3);
					commandsDataTable.AddCommandsRow(3, @"If", 3, 3);
					commandsDataTable.AddCommandsRow(4, @"Sort", 4, 3);
					commandsDataTable.AddCommandsRow(5, @"Cancel Sort", 5, 3);
					commandsDataTable.AddCommandsRow(1, @"List", 1, 4);
					commandsDataTable.AddCommandsRow(2, @"Frequencies", 2, 4);
					commandsDataTable.AddCommandsRow(3, @"Tables", 3, 4);
					//commandsDataTable.AddCommandsRow(4, @"Match", 4, 4);
					commandsDataTable.AddCommandsRow(5, @"Means", 5, 4);
					commandsDataTable.AddCommandsRow(6, @"Summarize", 6, 4);
					commandsDataTable.AddCommandsRow(7, @"Graph", 7, 4);
					//commandsDataTable.AddCommandsRow(8, @"Map", 8, 4);
					commandsDataTable.AddCommandsRow(1, @"Linear Regression", 1, 5);
					commandsDataTable.AddCommandsRow(2, @"Logistic Regression", 2, 5);
					commandsDataTable.AddCommandsRow(3, @"Kaplan-Meier Survival", 3, 5);
					commandsDataTable.AddCommandsRow(4, @"Cox Proportional Hazards", 4, 5);
					commandsDataTable.AddCommandsRow(5, @"Complex Sample Frequencies", 5, 5);
					commandsDataTable.AddCommandsRow(6, @"Complex Sample Tables", 6, 5);
					commandsDataTable.AddCommandsRow(7, @"Complex Sample Means", 7, 5);
					commandsDataTable.AddCommandsRow(1, @"Header", 1, 6);
					commandsDataTable.AddCommandsRow(2, @"Type", 2, 6);
					commandsDataTable.AddCommandsRow(3, @"RouteOut", 3, 6);
					commandsDataTable.AddCommandsRow(4, @"CloseOut", 4, 6);
					commandsDataTable.AddCommandsRow(5, @"PrintOut", 5, 6);
					//commandsDataTable.AddCommandsRow(6, @"Reports", 6, 6);
					commandsDataTable.AddCommandsRow(7, @"Storing Output", 7, 6);

					commandsDataTable.AddCommandsRow(1, @"Define Command", 1, 7);
					commandsDataTable.AddCommandsRow(2, @"User Command", 2, 7);
					commandsDataTable.AddCommandsRow(3, @"Run Saved Program", 3, 7);
					commandsDataTable.AddCommandsRow(4, @"Execute File", 4, 7);
					
                    commandsDataTable.AddCommandsRow(1, @"Dialog", 1, 8);
					commandsDataTable.AddCommandsRow(2, @"Beep", 2, 8);
					//commandsDataTable.AddCommandsRow(3, @"Help", 3, 8);
					commandsDataTable.AddCommandsRow(4, @"Quit Program", 4, 8);
					commandsDataTable.AddCommandsRow(1, @"Set", 1, 9);
					//commandsDataTable.AddCommandsRow(2, @"Define Group", 2, 2);
				}
				return (commandsDataTable);
			}
		}

		private DataSets.AppDataSet.DataPatternsDataTable dataPatternsDataTable;
		/// <summary>
		/// Data Patterns Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.DataPatternsDataTable DataPatternsDataTable
		{
			get
			{
				if (dataPatternsDataTable == null)
				{
                    dataPatternsDataTable = new DataSets.AppDataSet.DataPatternsDataTable();
                    int patternId = 0;

                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 1, @"None", String.Empty, String.Empty);
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 1, @"#", @"#", @"#");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 1, @"##", @"##", @"##");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 1, @"###", @"###", @"###");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 1, @"####", @"####", @"####");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 1, @"##.##", @"##.##", @"##.##");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 1, @"##.###", @"##.###", @"##.###");

                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 3, @"MM-DD-YYYY", @"##-##-####", @"MM-dd-yyyy");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 3, @"DD-MM-YYYY", @"##-##-####", @"dd-MM-yyyy");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 3, @"YYYY-MM-DD", @"####-##-##", @"yyyy-MM-dd");

                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 4, @"HH:MM AMPM", @"##:## ??", @"hh:mm tt");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 4, @"HH:MM", @"##:##", @"HH:mm");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 4, @"HH:MM:SS AMPM", @"##:##:## ??", @"hh:mm:ss tt");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 4, @"HH:MM:SS", @"##:##:##", @"HH:mm:ss");

                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 5, @"MM-DD-YYYY HH:MM AMPM", @"##-##-#### ##:## ??", @"MM-dd-yyyy hh:mm tt");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 5, @"DD-MM-YYYY HH:MM AMPM", @"##-##-#### ##:## ??", @"dd-MM-yyyy hh:mm tt");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 5, @"YYYY-MM-DD HH:MM AMPM", @"####-##-## ##:## ??", @"yyyy-MM-dd hh:mm tt");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 5, @"MM-DD-YYYY HH:MM", @"##-##-#### ##:##", @"MM-dd-yyyy HH:mm");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 5, @"DD-MM-YYYY HH:MM", @"##-##-#### ##:##", @"dd-MM-yyyy HH:mm");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 5, @"YYYY-MM-DD HH:MM", @"####-##-## ##:##", @"yyyy-MM-dd HH:mm");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 5, @"MM-DD-YYYY HH:MM:SS AMPM", @"##-##-#### ##:##:## ??", @"MM-dd-yyyy hh:mm:ss tt");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 5, @"DD-MM-YYYY HH:MM:SS AMPM", @"##-##-#### ##:##:## ??", @"dd-MM-yyyy hh:mm:ss tt");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 5, @"YYYY-MM-DD HH:MM:SS AMPM", @"####-##-## ##:##:## ??", @"yyyy-MM-dd hh:mm:ss tt");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 5, @"MM-DD-YYYY HH:MM:SS", @"##-##-#### ##:##:##", @"MM-dd-yyyy HH:mm:ss");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 5, @"DD-MM-YYYY HH:MM:SS", @"##-##-#### ##:##:##", @"dd-MM-yyyy HH:mm:ss");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 5, @"YYYY-MM-DD HH:MM:SS", @"####-##-## ##:##:##", @"yyyy-MM-dd HH:mm:ss");

                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 7, @"None", @"CCCCCCCCCCCCCCCCCCCC", @"CCCCCCCCCCCCCCCCCCCC");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 7, @"Numeric", @"####################", @"####################");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 7, @"###-####", @"###-####", @"###-####");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 7, @"###-###-####", @"###-###-####", @"###-###-####");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 7, @"###-###-###-####", @"###-###-###-####", @"###-###-###-####");
                    dataPatternsDataTable.AddDataPatternsRow(++patternId, 7, @"#-###-###-###-####", @"#-###-###-###-####", @"#-###-###-###-####");
                }
				return (dataPatternsDataTable);
			}
		}

		private DataSets.AppDataSet.DataTypesDataTable dataTypesDataTable;
		/// <summary>
		/// Data Types Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.DataTypesDataTable DataTypesDataTable
		{
			get
			{
				if (dataTypesDataTable == null)
				{
					dataTypesDataTable = new DataSets.AppDataSet.DataTypesDataTable();
					dataTypesDataTable.AddDataTypesRow(1, true, false, true, @"Number", @"NUMERIC");
					dataTypesDataTable.AddDataTypesRow(2, false, true, false, @"Text", @"TEXTINPUT");
                    dataTypesDataTable.AddDataTypesRow(3, false, false, true, @"Date", @"DATEFORMAT");
                    dataTypesDataTable.AddDataTypesRow(4, false, false, false, @"Time", @"TIMEFORMAT");
                    dataTypesDataTable.AddDataTypesRow(5, false, false, false, @"DateTime", @"DATETIMEFORMAT");
					dataTypesDataTable.AddDataTypesRow(6, false, false, false, @"Boolean", @"YN");
					dataTypesDataTable.AddDataTypesRow(7, true, false, false, @"PhoneNumber", @"TEXTINPUT");
					dataTypesDataTable.AddDataTypesRow(8, false, false, false, @"YesNo", @"YN");
					dataTypesDataTable.AddDataTypesRow(9, false, false, false, @"Unknown", string.Empty);
					dataTypesDataTable.AddDataTypesRow(10, false, false, false, @"GUID", @"TEXTINPUT");
					dataTypesDataTable.AddDataTypesRow(0, false, false, false, @"Object", @"DLLOBJECT");
				}
				return (dataTypesDataTable);
			}
		}

		private DataSets.AppDataSet.DialogFormatsDataTable dialogFormatsDataTable;
		/// <summary>
		/// Dialog Formats Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.DialogFormatsDataTable DialogFormatsDataTable
		{
			get
			{
				if (dialogFormatsDataTable == null)
				{
					dialogFormatsDataTable = new DataSets.AppDataSet.DialogFormatsDataTable();
					dialogFormatsDataTable.AddDialogFormatsRow(1, @"Text Entry", 1, @"TEXTINPUT");
					dialogFormatsDataTable.AddDialogFormatsRow(2, @"Multiple Choice", 2, @"<StringList>");
					dialogFormatsDataTable.AddDialogFormatsRow(3, @"Variable List", 3, @"DBVARIABLES");
					dialogFormatsDataTable.AddDialogFormatsRow(4, @"View List", 4, @"DBVIEWS");
					dialogFormatsDataTable.AddDialogFormatsRow(5, @"Database List", 5, @"DATABASES");
					dialogFormatsDataTable.AddDialogFormatsRow(6, @"File Open", 6, @"READ");
					dialogFormatsDataTable.AddDialogFormatsRow(7, @"File Save", 7, @"WRITE");
				}
				return (dialogFormatsDataTable);
			}
		}

		private DataSets.AppDataSet.FieldTypesDataTable fieldTypesDataTable;
		/// <summary>
		/// Field Types Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.FieldTypesDataTable FieldTypesDataTable
		{
			get
			{
				if (fieldTypesDataTable == null)
				{
					fieldTypesDataTable = new DataSets.AppDataSet.FieldTypesDataTable();
					fieldTypesDataTable.AddFieldTypesRow(1, @"Text", false, true, true, true, true, false, false, false, true, true, true, 2, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(2, @"Label/Title", false, false, false, false, false, false, false, false, false, false, false, 9, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(3, @"Text (Uppercase)", false, true, true, true, true, false, false, false, true, false, false, 2, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(4, @"Multiline", false, false, true, true, true, false, false, false, true, false, false, 2, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(5, @"Number", true, false, true, true, true, false, false, true, true, false, true, 1, false, 2);
					fieldTypesDataTable.AddFieldTypesRow(6, @"Phone Number", true, false, true, true, true, false, false, false, true, false, true, 7, false, 7);
                    fieldTypesDataTable.AddFieldTypesRow(7, @"Date", true, false, true, true, true, false, false, true, true, false, true, 3, false, 9);
                    fieldTypesDataTable.AddFieldTypesRow(8, @"Time", true, false, true, true, true, false, false, false, true, false, true, 4, false, 14);
                    fieldTypesDataTable.AddFieldTypesRow(9, @"Date/Time", true, false, true, true, true, false, false, false, true, false, true, 5, false, 16);
					fieldTypesDataTable.AddFieldTypesRow(10, @"Checkbox", false, false, true, false, true, false, false, false, false, false, true, 6, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(11, @"Yes/No", false, false, true, true, true, false, false, false, true, false, true, 8, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(12, @"Option", false, false, false, false, false, false, false, false, false, false, false, 2, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(13, @"Command Button", false, false, false, false, false, false, false, false, true, false, false, 9, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(14, @"Image", false, false, false, false, false, false, true, false, false, false, false, 0, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(15, @"Mirror", false, false, false, false, false, false, false, false, true, false, false, 9, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(16, @"Grid", false, false, false, false, false, false, false, false, false, false, false, 9, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(17, @"Legal Values", false, false, true, true, true, false, false, false, true, true, true, 9, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(18, @"Codes", false, false, true, true, true, false, false, false, true, true, false, 9, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(19, @"Comment Legal", false, false, true, true, true, false, false, false, true, true, true, 9, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(20, @"Relate", false, false, false, false, false, false, false, false, true, false, false, 9, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(21, @"Group", false, false, false, false, false, false, false, false, false, false, false, 9, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(22, @"RecStatus", false, false, false, false, false, false, false, false, false, false, false, 1, true, 0);
					fieldTypesDataTable.AddFieldTypesRow(23, @"UniqueKey", false, false, false, false, false, false, false, false, false, false, false, 1, true, 0);
					fieldTypesDataTable.AddFieldTypesRow(24, @"ForeignKey", false, false, false, false, false, false, false, false, false, false, false, 1, true, 0);
					fieldTypesDataTable.AddFieldTypesRow(25, @"GUID", false, false, false, false, false, false, false, false, false, false, false, 10, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(26, @"GlobalRecordId", false, false, false, false, false, false, false, false, false, false, false, 2, false, 0);
					fieldTypesDataTable.AddFieldTypesRow(27, @"List", false, false, true, true, true, false, false, false, true, true, false, 9, false, 0);
                    //123---
                    fieldTypesDataTable.AddFieldTypesRow(29, @"FirstSaveTime", false, false, true, true, true, false, false, false, true, true, false, 9, false, 0);
                    fieldTypesDataTable.AddFieldTypesRow(30, @"LastSaveTime", false, false, true, true, true, false, false, false, true, true, false, 9, false, 0);
                    //---
					fieldTypesDataTable.AddFieldTypesRow(99, @"Unknown", false, false, false, false, false, false, false, false, false, false, false, 9, false, 0);
				}
				return (fieldTypesDataTable);
			}
		}

		private DataSets.AppDataSet.FontStylesDataTable fontStylesDataTable;
		/// <summary>
		/// Variable Scopes Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.FontStylesDataTable FontStylesDataTable
		{
			get
			{
				if (fontStylesDataTable == null)
				{
					fontStylesDataTable = new DataSets.AppDataSet.FontStylesDataTable();
					fontStylesDataTable.AddFontStylesRow(1, @"Regular");
					fontStylesDataTable.AddFontStylesRow(2, @"Italic");
					fontStylesDataTable.AddFontStylesRow(3, @"Bold");
					fontStylesDataTable.AddFontStylesRow(4, @"Bold Italic");
				}
				return (fontStylesDataTable);
			}
		}

		private DataSets.AppDataSet.LayerRenderTypesDataTable layerRenderTypesDataTable;
		/// <summary>
		/// Layer Render Types Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.LayerRenderTypesDataTable LayerRenderTypesDataTable
		{
			get
			{
				if (layerRenderTypesDataTable == null)
				{
					layerRenderTypesDataTable = new DataSets.AppDataSet.LayerRenderTypesDataTable();
					layerRenderTypesDataTable.AddLayerRenderTypesRow(1, @"Simple");
					layerRenderTypesDataTable.AddLayerRenderTypesRow(2, @"Choropleth");
					layerRenderTypesDataTable.AddLayerRenderTypesRow(3, @"Dot Density");
					layerRenderTypesDataTable.AddLayerRenderTypesRow(4, @"Unique Values");
				}
				return (layerRenderTypesDataTable);
			}
		}

		private DataSets.AppDataSet.ListTreatmentTypesDataTable listTreatmentTypesDataTable;
		/// <summary>
		/// List Treatment Types Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.ListTreatmentTypesDataTable ListTreatmentTypesDataTable
		{
			get
			{
				if (listTreatmentTypesDataTable == null)
				{
					listTreatmentTypesDataTable = new DataSets.AppDataSet.ListTreatmentTypesDataTable();
					listTreatmentTypesDataTable.AddListTreatmentTypesRow(1, @"Legal Values");
					listTreatmentTypesDataTable.AddListTreatmentTypesRow(2, @"Comment Legal");
				}
				return (listTreatmentTypesDataTable);
			}
		}

		private DataSets.AppDataSet.ModulesDataTable modulesDataTable;
		/// <summary>
		/// Modules Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.ModulesDataTable ModulesDataTable
		{
			get
			{
				if (modulesDataTable == null)
				{
					modulesDataTable = new DataSets.AppDataSet.ModulesDataTable();
					modulesDataTable.AddModulesRow(1, @"General", 1, @"EpiInfo", @"General", string.Empty, string.Empty);
					modulesDataTable.AddModulesRow(2, @"Menu", 2, @"Menu", @"Menu", string.Empty, string.Empty);
					modulesDataTable.AddModulesRow(3, @"Make View", 3, @"MakeView", @"Make View", @"Epi.MakeView", @"Epi.MakeView.Forms.MakeView");
					modulesDataTable.AddModulesRow(4, @"Enter", 4, @"Enter", @"Enter Data", @"Epi.Enter", @"Epi.Enter.Forms.Enter");
					modulesDataTable.AddModulesRow(5, @"Analysis", 5, @"Analysis", @"Analyze Data", @"Epi.Analysis", @"Epi.Analysis.Forms.Analysis");
					modulesDataTable.AddModulesRow(6, @"EpiMap", 6, @"EpiMap", @"Create Maps", @"Epi.Map", @"Epi.Map.Forms.EpiMap");
				}
				return (modulesDataTable);
			}
		}

		private DataSets.AppDataSet.RecordProcessingScopesDataTable recordProcessingScopesDataTable;
		/// <summary>
		/// Record Processing Scopes Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.RecordProcessingScopesDataTable RecordProcessingScopesDataTable
		{
			get
			{
				if (recordProcessingScopesDataTable == null)
				{
					recordProcessingScopesDataTable = new DataSets.AppDataSet.RecordProcessingScopesDataTable();
					recordProcessingScopesDataTable.AddRecordProcessingScopesRow(1, @"Normal (undeleted)", @"UNDELETED", 1);
					recordProcessingScopesDataTable.AddRecordProcessingScopesRow(2, @"Deleted", @"DELETED", 2);
					recordProcessingScopesDataTable.AddRecordProcessingScopesRow(3, @"Both", @"BOTH", 3);
				}
				return (recordProcessingScopesDataTable);
			}
		}

		private DataSets.AppDataSet.RepresentationsOfMissingDataTable representationsOfMissingDataTable;
		/// <summary>
		/// Representations Of Missing Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.RepresentationsOfMissingDataTable RepresentationsOfMissingDataTable
		{
			get
			{
				if (representationsOfMissingDataTable == null)
				{
					representationsOfMissingDataTable = new DataSets.AppDataSet.RepresentationsOfMissingDataTable();
					representationsOfMissingDataTable.AddRepresentationsOfMissingRow(1, @"Missing", 1);
					representationsOfMissingDataTable.AddRepresentationsOfMissingRow(2, @"Unknown", 2);
					representationsOfMissingDataTable.AddRepresentationsOfMissingRow(3, @"(.)", 3);
				}
				return (representationsOfMissingDataTable);
			}
		}



		private DataSets.AppDataSet.RepresentationsOfNoDataTable representationsOfNoDataTable;
		/// <summary>
		/// Representations Of No Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.RepresentationsOfNoDataTable RepresentationsOfNoDataTable
		{
			get
			{
				if (representationsOfNoDataTable == null)
				{
					representationsOfNoDataTable = new DataSets.AppDataSet.RepresentationsOfNoDataTable();
					representationsOfNoDataTable.AddRepresentationsOfNoRow(1, @"No", 1);
					representationsOfNoDataTable.AddRepresentationsOfNoRow(2, @"False", 2);
					representationsOfNoDataTable.AddRepresentationsOfNoRow(3, @"(-)", 3);
				}
				return (representationsOfNoDataTable);
			}
		}

		private DataSets.AppDataSet.RepresentationsOfYesDataTable representationsOfYesDataTable;
		/// <summary>
		/// Representations Of Yes Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.RepresentationsOfYesDataTable RepresentationsOfYesDataTable
		{
			get
			{
				if (representationsOfYesDataTable == null)
				{
					representationsOfYesDataTable = new DataSets.AppDataSet.RepresentationsOfYesDataTable();
					representationsOfYesDataTable.AddRepresentationsOfYesRow(1, @"Yes", 1);
					representationsOfYesDataTable.AddRepresentationsOfYesRow(2, @"True", 2);
					representationsOfYesDataTable.AddRepresentationsOfYesRow(3, @"(+)", 3);
				}
				return (representationsOfYesDataTable);
			}
		}

		private DataSets.AppDataSet.ReservedWordsDataTable reservedWordsDataTable;
		/// <summary>
		/// Reservered Words Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.ReservedWordsDataTable ReservedWordsDataTable
		{
			get
			{
				if (reservedWordsDataTable == null)
				{
                    int index = 1;
                    reservedWordsDataTable = new DataSets.AppDataSet.ReservedWordsDataTable();
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ABSOLUTE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ACTION", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ADA", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ADD", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ALL", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ALLOCATE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ALPHANUMERIC", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ALTER", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ALWAYS", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"and", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ANY", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"APPEND", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ARE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"AS", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"asc", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ASCENDING", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ASSERTION", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ASSIGN", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"AT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"AUTHORIZATION", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"AUTOINCREMENT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"AUTOSEARCH", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"AVG", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"BEEP", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"BEGIN", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"BETWEEN", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"BINARY", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"bit", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"BIT_LENGTH", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"BITMAP", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"BOOLEAN", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"BOTH", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"BY", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"byte", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CANCEL", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CASCADE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CASCADED", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CASE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CATALOG", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"char", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CHAR_LENGTH", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CHARACTER", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CHARACTER_LENGTH", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CHECK", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CLEAR", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CLIPBOARD", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CLOSE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CLOSEOUT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CMD", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"COALESCE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CODE       ", @"D");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"COLLATE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"COLLATION", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"COLUMN", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"COLUMNSIZE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"COMBINE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"COMMANDLINE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"COMMIT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"COMPRESS", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CONNECT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CONNECTION", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"constraint", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CONSTRAINTS", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CONTINUE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CONVERT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CORRESPONDING", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"count", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"COUNTER", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"COXPH", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CREATE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CROSS", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CURRENCY", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CURRENT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CURRENT_DATE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CURRENT_TIME", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CURRENT_TIMESTAMP", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CURRENT_USER", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CURSOR", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DATABASE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DATABASES", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"date", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DATEFORMAT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"datetime", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DAY", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DBVALUES", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DBVARIABLES", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DBVIEWS", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DEALLOCATE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DEC", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"decimal", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DECLARE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DECOMPRESS", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DEFAULT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DEFERRABLE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DEFERRED", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DEFINE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"delete", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DELETED", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DENOMINATOR", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"desc", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DESCENDING", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DESCRIBE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DESCRIPTOR", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DIALOG", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DISALLOW", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DISCONNECT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DISPLAY", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DISTINCT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DISTINCTROW", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DLLOBJECT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DOMAIN", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"double", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"DROP", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ELSE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ELSEIF", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"END", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ENDBEFORE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"END-EXEC", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"EQV", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ESCAPE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"EXCEPT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"EXCEPTION", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"EXEC", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"EXECUTE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"EXISTS", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"EXIT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"EXTERNAL", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"EXTRACT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"FALSE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"FETCH", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"FIELDVAR", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"FILESPEC", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"FIRST", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"FKEY", @"H");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"float", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"FLOAT4", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"FLOAT8", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"FOR", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"FOREIGN", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"FORTRAN", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"FOUND", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"FREQ", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"FREQGRAPH", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"from", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"FULL", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"GENERAL", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"GET", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"GLOBAL", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"GLOBALID   ", @"D");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"GO", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"GOTO", @"B");
                    //---2225
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"GOTOFORM", @"B");
                    //---
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"GRANT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"GRAPH", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"GRAPHTYPE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"GRIDLINES", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"GRIDTABLE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"GROUP", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"GROUPVAR", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"GUID", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"HAVING", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"HEADER", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"HELP", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"HIDE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"HIVALUE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"HOUR", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"HYPERLINKS", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"identity", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"IEEEDOUBLE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"IEEESINGLE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"IF", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"IGNORE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"IMMEDIATE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"IMP", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"in", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"INCLUDE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"INDEX", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"INDICATOR", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"INITIALLY", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"INNER", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"INPUT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"INSENSITIVE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"INSERT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"int", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"integer", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"INTEGER1", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"INTEGER2", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"INTEGER4", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"INTERSECT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"INTERVAL", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"INTO", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"IS", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ISOLATION", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"JOIN", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"KEY", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"KEYVARS", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"KMSURVIVAL", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LANGUAGE", @"D");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LAST", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LEADING", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LEFT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LET", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LEVEL", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"like", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LINENUMBERS", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LINKNAME", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LIST", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LOCAL", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LOGICAL", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LOGICAL1", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LOGISTIC", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LONG", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LONGBINARY", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LONGTEXT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LOVALUE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"LOWER", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"MAP", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"MATCH", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"MATCHING", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"MATCHVAR", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"MAX", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"MEANS", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"MEMO", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"MERGE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"min", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"MINUTE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"MISSING", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"MOD", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"MODDATE   ", @"D");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"MODULE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"MODUSER   ", @"D");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"money", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"MONTH", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"MULTIGRAPH", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"NAMES", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"NATURAL", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"nchar", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"NEWPAGE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"NEWRECORD", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"NEXT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"NO", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"NOIMAGE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"NOINTERCEPT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"NONE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"NOT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"NOWRAP", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"null", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"NULLIF", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"NUMBER", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"numeric", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"OCTET_LENGTH", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"OF", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"OFF", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"OLEOBJECT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"on", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ONLY", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"OPEN", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"OPTION", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"or", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ORDER", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"OUTER", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"OUTPUT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"OUTTABLE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"OVERLAPS", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"OVERLAYNEXT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"OWNERACCESS", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PAD", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PARAMETERS", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PARTIAL", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PASCAL", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PERCENT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PERCENTS", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PERMANENT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PGMNAME", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PIVOT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"POSITION", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PRECISION", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PREPARE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PRESERVE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PRIMARY", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PRINTOUT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PRIOR", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PRIVILEGES", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PROCEDURE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PROCESS", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PSUVAR", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PUBLIC", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PVALUE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"QUIT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"READ", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"real", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"RECDELETED", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"RECODE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"RECORDCOUNT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"RECSTATUS", @"H");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"references", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"REGRESS", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"RELATE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"RELATIVE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"REPEAT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"REPLACE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"REPORT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"REPORTDATA", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"RESPONSEVAR", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"RESTRICT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"REVOKE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"RIGHT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ROLLBACK", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ROUTEOUT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ROWS", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"RUNPGM", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"RUNSILENT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SCHEMA", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SCROLL", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SECOND", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SECTION", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SELECT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SESSION", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SESSION_USER", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"set", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SHORT", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SHOWOBSERVED", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SHOWPROMPTS", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SHOWSINGLECASES", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"single", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SIZE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"smallint", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SOME", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SORT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SPACE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SQL", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SQLCA", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SQLCODE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SQLERROR", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SQLNULL", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SQLREAL", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SQLSTATE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SQLSTRING", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SQLWARNING", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"STARTFROM", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"STATISTICS", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"STATUSBAR", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"STDEV", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"STDEVP", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"STRATAVAR", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SUBSTRING", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SUM", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SUMOF", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SYSTEM_USER", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SYSTEMDATE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"SYSTEMTIME", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TABLE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TABLEID", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TABLES", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TEMPLATE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TEMPORARY", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"text", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TEXTFONT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TEXTINPUT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"THEN", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"THREED", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TIME", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"timestamp", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TIMEUNIT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TIMEVAR", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TIMEZONE_HOUR", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TIMEZONE_MINUTE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TITLETEXT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TO", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TOP", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TRAILING", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TRANSACTION", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TRANSFORM", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TRANSLATE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TREE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TRIM", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"TYPEOUT", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"UNDEFINE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"UNDELETE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"UNHIDE", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"UNION", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"UNIQUE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"UNIQUEKEY", @"H");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"UNKNOWN", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"update", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"UPPER", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"USAGE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"USEBROWSER", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"USER", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"USING", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"VALUE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"values", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"VALUEVAR", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"VAR", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"varbinary", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"varchar", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"VARP", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"VARYING", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"VIEW", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"VIEWNAME", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"WEIGHTVAR", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"WHEN", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"WHENEVER", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"where", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"WITH", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"WORK", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"WRITE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"XOR", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"YEAR", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"YESNO", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"YN", @"A");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ZONE", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"CONTAINS", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"PROC", @"B");
                   

                    // JavaScript keywords
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"alert", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"isFinite", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"personalbar", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Anchor", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"isNan", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Plugin", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Area", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"java", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"print", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"arguments", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"JavaArray", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"prompt", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Array", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"JavaClass", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"prototype", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"assign", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"JavaObject", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Radio", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"blur", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"JavaPackage", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"ref", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Boolean", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"length", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"RegExp", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Button", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Link", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"releaseEvents", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"callee", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"location", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Reset", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"caller", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Location", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"resizeBy", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"captureEvents", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"locationbar", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"resizeTo", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Checkbox", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Math", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"routeEvent", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"clearInterval", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"menubar", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"scroll", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"clearTimeout", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"MimeType", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"scrollbars", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"close", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"moveBy", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"scrollBy", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"closed", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"moveTo", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"scrollTo", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"confirm", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"name", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Select", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"constructor", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"NaN", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"self", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Date", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"navigate", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"setInterval", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"defaultStatus", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"navigator", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"setTimeout", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"document", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Navigator", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"status", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Document", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"netscape", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"statusbar", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Element", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Number", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"stop", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"escape", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Object", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"String", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"eval", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"onBlur", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Submit", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"FileUpload", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"onError", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"sun", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"find", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"onFocus", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"taint", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"focus", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"onLoad", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Text", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Form", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"onUnload", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Textarea", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Frame", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"open", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"toolbar", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Frames", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"opener", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"top", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Function", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Option", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"toString", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"getClass", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"outerHeight", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"unescape", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Hidden", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"OuterWidth", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"untaint", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"history", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Packages", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"unwatch", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"History", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"pageXoffset", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"valueOf", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"home", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"pageYoffset", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"watch", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Image", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"parent", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"window", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Infinity", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"parseFloat", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Window", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"InnerHeight", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"parseInt", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"InnerWidth", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"Password", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"catch", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"enum", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"throw", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"class", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"extends", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"try", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"const", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"finally", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"debugger", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"super", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"abstract", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"implements", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"protected", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"boolean", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"instanceOf", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"public", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"byte", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"int", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"short", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"char", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"interface", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"static", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"double", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"long", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"synchronized", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"false", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"native", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"throws", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"final", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"null", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"transient", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"float", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"package", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"true", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"goto", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"private", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"break", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"export", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"return", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"case", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"for", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"switch", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"comment", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"function", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"this", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"continue", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"if", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"typeof", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"default", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"import", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"var", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"delete", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"in", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"void", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"do", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"label", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"while", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"else", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"new", @"B");
                    reservedWordsDataTable.AddReservedWordsRow(index++, @"with", @"B");

				}
				return (reservedWordsDataTable);
			}
		}

		private DataSets.AppDataSet.SettingsDataTable settingsDataTable;
		/// <summary>
		/// Settings Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.SettingsDataTable SettingsDataTable
		{
			get
			{
				if (settingsDataTable == null)
				{
					settingsDataTable = new DataSets.AppDataSet.SettingsDataTable();
					settingsDataTable.AddSettingsRow(1, @"BackgroundImage", @"C:\Epi_Info");
					settingsDataTable.AddSettingsRow(2, @"MRUProjectsCount", @"4");
					settingsDataTable.AddSettingsRow(3, @"Language", @"English");
					settingsDataTable.AddSettingsRow(4, @"RepresentationOfYes", @"Yes");
					settingsDataTable.AddSettingsRow(5, @"RepresentationOfNo", @"No");
					settingsDataTable.AddSettingsRow(6, @"RepresentationOfMissing", @"Missing");
					settingsDataTable.AddSettingsRow(7, @"StatisticsLevel", @"2");
					settingsDataTable.AddSettingsRow(8, @"RecordProcessingScope", @"2");
					settingsDataTable.AddSettingsRow(9, @"ShowCompletePrompt", @"true");
					settingsDataTable.AddSettingsRow(10, @"ShowSelection", @"true");
					settingsDataTable.AddSettingsRow(11, @"ShowGraphics", @"true");
					settingsDataTable.AddSettingsRow(12, @"ShowPercents", @"true");
					settingsDataTable.AddSettingsRow(13, @"ShowTables", @"true");
					settingsDataTable.AddSettingsRow(14, @"ShowHyperlinks", @"true");
					settingsDataTable.AddSettingsRow(15, @"IncludeMissingValues", @"false");
					settingsDataTable.AddSettingsRow(16, @"SnapToGrid", @"true");
					settingsDataTable.AddSettingsRow(17, @"EditorFontName", @"Verdana");
					settingsDataTable.AddSettingsRow(18, @"EditorFontSize", @"8.25");
					settingsDataTable.AddSettingsRow(17, @"ControlFontName", @"Arial");
					settingsDataTable.AddSettingsRow(18, @"ControlFontSize", @"11.25");
					settingsDataTable.AddSettingsRow(19, @"DefaultDatabaseFormat", @"2");
					settingsDataTable.AddSettingsRow(20, @"WorkingDirectory", @"C:\Temp");
					settingsDataTable.AddSettingsRow(21, @"ProjectDirectory", @"C:\Documents and Settings\kkm4\My Documents\Epi Info Projects");
					settingsDataTable.AddSettingsRow(22, @"MRUViewsCount", @"4");
					settingsDataTable.AddSettingsRow(22, @"MRUDataSourcesCount", @"5");
					settingsDataTable.AddSettingsRow(23, @"DefaultDataFormat", @"3");
				}
				return (settingsDataTable);
			}
		}



		private DataSets.AppDataSet.SourceControlTypesDataTable sourceControlTypesDataTable;
		/// <summary>
		/// Source Control Types Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.SourceControlTypesDataTable SourceControlTypesDataTable
		{
			get
			{
				if (sourceControlTypesDataTable == null)
				{
					sourceControlTypesDataTable = new DataSets.AppDataSet.SourceControlTypesDataTable();
					sourceControlTypesDataTable.AddSourceControlTypesRow(1, @"Codes");
					sourceControlTypesDataTable.AddSourceControlTypesRow(2, @"Mirror");
				}
				return (sourceControlTypesDataTable);
			}
		}

		private DataSets.AppDataSet.StatisticsLevelsDataTable statisticsLevelsDataTable;
		/// <summary>
		/// Statistics Levels Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.StatisticsLevelsDataTable StatisticsLevelsDataTable
		{
			get
			{
				if (statisticsLevelsDataTable == null)
				{
					statisticsLevelsDataTable = new DataSets.AppDataSet.StatisticsLevelsDataTable();
					statisticsLevelsDataTable.AddStatisticsLevelsRow(1, @"None", @"NONE", 1);
					statisticsLevelsDataTable.AddStatisticsLevelsRow(2, @"Minimal", @"MINIMAL", 2);
					statisticsLevelsDataTable.AddStatisticsLevelsRow(3, @"Intermediate", @"INTERMEDIATE", 3);
					statisticsLevelsDataTable.AddStatisticsLevelsRow(4, @"Advanced", @"COMPLETE", 4);
				}
				return (statisticsLevelsDataTable);
			}
		}



		private DataSets.AppDataSet.SupportedAggregatesDataTable supportedAggregatesDataTable;
		/// <summary>
		/// Supported Aggregates Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.SupportedAggregatesDataTable SupportedAggregatesDataTable
		{
			get
			{
				if (supportedAggregatesDataTable == null)
				{
					supportedAggregatesDataTable = new DataSets.AppDataSet.SupportedAggregatesDataTable();
					supportedAggregatesDataTable.AddSupportedAggregatesRow(1, @"Average", 1);
					supportedAggregatesDataTable.AddSupportedAggregatesRow(2, @"Count", 2);
					supportedAggregatesDataTable.AddSupportedAggregatesRow(3, @"First", 3);
					supportedAggregatesDataTable.AddSupportedAggregatesRow(4, @"Last", 4);
					supportedAggregatesDataTable.AddSupportedAggregatesRow(5, @"Maximum", 5);
					supportedAggregatesDataTable.AddSupportedAggregatesRow(6, @"Minimum", 6);
					supportedAggregatesDataTable.AddSupportedAggregatesRow(7, @"StdDev", 7);
					//supportedAggregatesDataTable.AddSupportedAggregatesRow(8, @"StdDev(Pop)", 8);
					supportedAggregatesDataTable.AddSupportedAggregatesRow(9, @"Sum", 9);
					supportedAggregatesDataTable.AddSupportedAggregatesRow(10, @"Var", 10);
					//supportedAggregatesDataTable.AddSupportedAggregatesRow(11, @"Var(Pop)", 11);
				}
				return (supportedAggregatesDataTable);
			}
		}



		private DataSets.AppDataSet.VariableScopesDataTable variableScopesDataTable;
		/// <summary>
		/// Variable Scopes Data Table in Application DataSet 
		/// </summary>
		public DataSets.AppDataSet.VariableScopesDataTable VariableScopesDataTable
		{
			get
			{
				if (variableScopesDataTable == null)
				{
					variableScopesDataTable = new DataSets.AppDataSet.VariableScopesDataTable();
					variableScopesDataTable.AddVariableScopesRow(4, @"Standard", 1, true);
					variableScopesDataTable.AddVariableScopesRow(2, @"Global", 2, false);
					variableScopesDataTable.AddVariableScopesRow(1, @"Permanent", 3, false);
					variableScopesDataTable.AddVariableScopesRow(0, @"Undefined", 0, false);
					variableScopesDataTable.AddVariableScopesRow(16, @"DataSource", 4, false);
					variableScopesDataTable.AddVariableScopesRow(32, @"DataSourceAssigned", 5, false);
				}
				return (variableScopesDataTable);
			}
		}
		#endregion Generated Code

		#endregion Public Properties

		#region Private Properties
		private  ArrayList FieldTypes
		{
			get
			{
				if (fieldTypes == null)
				{
					fieldTypes = new ArrayList();
					foreach (AppDataSet.FieldTypesRow row in FieldTypesDataTable.Rows)
					{
						FieldType fieldType = new FieldType(row);
						fieldTypes.Add(fieldType);
					}
				}
				return (fieldTypes);
			}
		}		
		#endregion Private Properties

		#region Public Methods

		/// <summary>
		/// Get Record Processessing Scope By Id
		/// </summary>
		/// <param name="id">The id of the Record ProcessingS cope</param>
		/// <returns>Record ProcessingS copes Row</returns>
		public AppDataSet.RecordProcessingScopesRow GetRecordProcessessingScopeById(RecordProcessingScope id)
		{
			foreach (AppDataSet.RecordProcessingScopesRow row in this.RecordProcessingScopesDataTable.Rows)
			{
				if (row.Id == (short) id)
				{
					return row;
				}
			}
			throw new GeneralException("Invalid Scope Id");
		}

		/// <summary>
		/// A row that contains the Statistics Levels and names
		/// </summary>
		/// <param name="id">Statistics Level enumeration.</param>
		/// <returns>Statistics Levels Row</returns>
		public AppDataSet.StatisticsLevelsRow  GetStatisticsLevelById(StatisticsLevel id)
		{
			foreach (AppDataSet.StatisticsLevelsRow row in this.StatisticsLevelsDataTable.Rows)
			{
				if (row.Id == (short) id)
				{
					return row;
				}
			}
			throw new System.ApplicationException("Invalid Statistics Level Id");
		}   

		/// <summary>
		/// Checks to see if a word already exists as a reserved word
		/// </summary>
		/// <param name="word">Possible reserved word</param>
		/// <returns>Returns status as to whether word already exists as a reserved word</returns>
		public bool IsReservedWord(string word)
		{
			#region Input Validation
			if (string.IsNullOrEmpty(word))			
			{
				throw new ArgumentNullException("word");
			}
			#endregion
			
			DataView dv = ReservedWordsDataTable.DefaultView;
			dv.RowFilter = ReservedWordsDataTable.NameColumn.ColumnName + " = '" + word + "'";
			return (dv.Count > 0);
		}
		#endregion Public Methods		
		
		#region Nested Types
		/// <summary>
		/// Field Type class
		/// </summary>
		public class FieldType
		{
			#region Constructors
			/// <summary>
			/// Default Constructor
			/// </summary>
			public FieldType()
			{
			}

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="row">Field Types row in application dataset.</param>
		public FieldType(AppDataSet.FieldTypesRow row)
			{
				inner = row;
			}
			#endregion Constructors

			#region Fields
			private AppDataSet.FieldTypesRow inner;
			#endregion Fields

			#region Public Properties
			/// <summary>
			/// Internal Field Types row.
			/// </summary>
			public AppDataSet.FieldTypesRow Inner
			{
				get
				{
					return inner;
				}
			}
			#endregion Public Properties
		}
		#endregion
	}
}
