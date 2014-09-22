using System;

namespace Epi.Analysis
{
	/// <summary>
	/// Command categories for Analysis
	/// </summary>
	public enum CommandGroups
	{
		/// <summary>
		/// Data command category
		/// </summary>
		Data = 1,
		/// <summary>
		/// Variables command category
		/// </summary>
		Variables = 2,
		/// <summary>
		///  SelectIf command category
		/// </summary>
		SelectIf = 3,
		/// <summary>
		/// Statistics command category
		/// </summary>
		Statistics = 4,
		/// <summary>
		/// Advanced Statistics command category
		/// </summary>
		AdvancedStatistics = 5,
		/// <summary>
		/// Output command category
		/// </summary>
		Output = 6,
		/// <summary>
		/// UserDefined command category
		/// </summary>
		UserDefined = 7,
		/// <summary>
		/// UserInteraction command category
		/// </summary>
		UserInteraction = 8,
		/// <summary>
		/// Options command category
		/// </summary>
		Options =9
	}
	/// <summary>
	/// Command category for Data commands
	/// </summary>
	public enum DataCommands
	{
		/// <summary>
		/// Read command category
		/// </summary>
		Read = 1,
		/// <summary>
		/// Relate command category
		/// </summary>
		Relate = 2,
		/// <summary>
		/// Write command category
		/// </summary>
		Write = 3,
		/// <summary>
		/// Merge command category
		/// </summary>
		Merge = 4,
		/// <summary>
		/// DeleteFile command category
		/// </summary>
		DeleteFile = 5,
		/// <summary>
		/// DeleteRecord command category
		/// </summary>
		DeleteRecord = 6,
		/// <summary>
		/// UndeleteRecord command category
		/// </summary>
		UndeleteRecord = 7
	}
	/// <summary>
	/// Command category for Variable commands
	/// </summary>
	public enum VariableCommands
	{
		/// <summary>
		/// Define command category
		/// </summary>
		Define = 1,
        /// <summary>
        /// Define group command category
        /// </summary>
        DefineGroup = 2,
        /// <summary>
		/// Undefine command category
		/// </summary>
		Undefine = 3,
		/// <summary>
		/// Assign command category
		/// </summary>
		Assign = 4,
		/// <summary>
		/// Recode command category
		/// </summary>
		Recode = 5,
		/// <summary>
		/// Display command category
		/// </summary>
		Display = 6
	}
	/// <summary>
	/// Command category for SelectIf commands
	/// </summary>
	public enum SelectIfCommands
	{
		/// <summary>
		/// Select command category
		/// </summary>
		Select = 1,
		/// <summary>
		/// CancelSelect command category
		/// </summary>
		CancelSelect = 2,
		/// <summary>
		/// If command category
		/// </summary>
		If = 3,
		/// <summary>
		/// Sort command category
		/// </summary>
		Sort = 4,
		/// <summary>
		/// CancelSort command category
		/// </summary>
		CancelSort =5
	}
	/// <summary>
	/// Command categories for Statistics commands
	/// </summary>
	public enum StatisticsCommands
	{
		/// <summary>
		/// List command category
		/// </summary>
		List = 1,
		/// <summary>
		/// Frequencies command category
		/// </summary>
		Frequencies = 2,
		/// <summary>
		/// Tables command category
		/// </summary>
		Tables = 3,
		/// <summary>
		/// Match command category
		/// </summary>
		Match = 4,
		/// <summary>
		/// Means command category
		/// </summary>
		Means = 5,
		/// <summary>
		/// Summarize command category
		/// </summary>
		Summarize = 6,
		/// <summary>
		/// Graph command category
		/// </summary>
		Graph = 7,
		/// <summary>
		/// Map command category
		/// </summary>
		Map = 8
	}
	/// <summary>
	/// Command categories for Advanced Statistics commands
	/// </summary>
	public enum AdvancedStatisticsCommands
	{
		/// <summary>
		/// Linear Regression command category
		/// </summary>
		LinearRegression = 1,
		/// <summary>
		/// Logistic Regression command category
		/// </summary>
		LogisticRegression = 2,
		/// <summary>
		/// Kaplan Meier Survival command category
		/// </summary>
		KaplanMeierSurvival = 3,
		/// <summary>
		/// Cox Proportional Hazards command category
		/// </summary>
		CoxProportionalHazards = 4,
		/// <summary>
		/// Complex Sample Frequencies command category
		/// </summary>
		ComplexSampleFrequencies = 5,
		/// <summary>
		/// Complex Sample Tables command category
		/// </summary>
		ComplexSampleTables = 6,
		/// <summary>
		/// Complex Sample Means command category
		/// </summary>
		ComplexSampleMeans = 7
	}
	/// <summary>
	/// Command categories for Output commands
	/// </summary>
	public enum OutputCommands
	{
		/// <summary>
		/// Header command category
		/// </summary>
		Header = 1,
		/// <summary>
		/// Type command category
		/// </summary>
		Type = 2,
		/// <summary>
		/// Routeout command category
		/// </summary>
		Routeout = 3,
		/// <summary>
		/// Closeout command category
		/// </summary>
		Closeout = 4,
		/// <summary>
		/// PrintOut command category
		/// </summary>
		Printout = 5,
		/// <summary>
		/// Reports command category
		/// </summary>
		Reports = 6,
		/// <summary>
		/// StoreOutput command category
		/// </summary>
		StoreOutput = 7
	}
	/// <summary>
	/// Command categories for User Defined commands
	/// </summary>
	public enum UserDefinedCommands
	{
		/// <summary>
		/// Define Command command category
		/// </summary>
		DefineCommand = 1,
		/// <summary>
		/// User Command command category
		/// </summary>
		UserCommand = 2,
		/// <summary>
		/// Run Saved Program command category
		/// </summary>
		RunSavedProgram = 3,
		/// <summary>
		/// Execute File command category
		/// </summary>
		ExecuteFile = 4
	}
	/// <summary>
	/// Command categories for User Interaction commands
	/// </summary>
	public enum UserInteractionCommands
	{
		/// <summary>
		/// Dialog command category
		/// </summary>
		Dialog = 1,
		/// <summary>
		/// Beep command category
		/// </summary>
		Beep = 2,
		/// <summary>
		/// Help command category
		/// </summary>
		Help = 3,
		/// <summary>
		/// Quit command category
		/// </summary>
		Quit = 4
	}
	/// <summary>
	/// Command categories for Options command
	/// </summary>
	public enum OptionCommands
	{
		/// <summary>
		/// Set command category
		/// </summary>
		Set = 1
	}

}
