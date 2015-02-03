
namespace Epi
{
    /// <summary>
    /// String constants for various commands
    /// </summary>
    public static class CommandNames
    {
        /// <summary>
        /// ABS absolute value numeric function
        /// </summary>
        public const string ABS = "ABS";
        /// <summary>
        /// AND logical operator
        /// </summary>
        public const string AND = "AND";
        /// <summary>
        /// ALWAYS key word used with AUTOSEARCH command to run search on existing records as well as new.
        /// </summary>
        public const string ALWAYS = "ALWAYS";
        /// <summary>
        /// Append data when exporting
        /// </summary>
        public const string APPEND = "APPEND";
        /// <summary>
        /// Sort data in descending order
        /// </summary>
        public const string ASCENDING = "ASCENDING";
        /// <summary>
        /// Assign a value or the result of an expression to a defined variable
        /// </summary>        
        /// 
        public const string ASSIGN = "ASSIGN";
        /// <summary>
        /// Autosearches particular field(s)
        /// </summary>
        public const string AUTOSEARCH = "AUTOSEARCH";
        /// <summary>
        /// Generate a beep
        /// </summary>
        public const string BEEP = "BEEP";
        /// <summary>
        /// Calls a definded sub-routine
        /// </summary>
        public const string CALL = "CALL";
        /// <summary>
        /// Cancels the current select command
        /// </summary>
        public const string CANCELSELECT = "SELECT";
        /// <summary>
        /// Cancels the current sort command
        /// </summary>
        public const string CANCELSORT = "SORT";
        /// <summary>
        /// Clears a field(s)
        /// </summary>
        public const string CLEAR = "CLEAR";
        /// <summary>
        /// Stops routing the current output to the specified output file
        /// </summary>
        public const string CLOSEOUT = "CLOSEOUT";
        /// <summary>
        /// Column size for output of analysis command tables.
        /// </summary>
        public const string COLUMNSIZE = "COLUMNSIZE";
        /// <summary>
        /// COS numeric function
        /// </summary>
        public const string COS = "COS";
        /// <summary>
        /// Cox Proportional Hazards
        /// </summary>
        public const string COXPH = "COXPH";
        /// <summary>
        /// Currency parameter of FORMAT function
        /// </summary>
        public const string CURRENCY_FORMAT = "\"Currency\"";
        /// <summary>
        /// System function returning the Current User signed in
        /// </summary>
        public const string CURRENTUSER = "CURRENTUSER";
        /// <summary>
        /// Dialog format Database list
        /// </summary>
        public const string DATABASES = "DATABASES";
        /// <summary>
        /// DAY date function
        /// </summary>
        public const string DAY = "DAY";
        /// <summary>
        /// DAYS date function
        /// </summary>
        public const string DAYS = "DAYS";
        /// <summary>
        /// Used in Dialog command
        /// </summary>
        public const string DBVALUES = "DBVALUES";
        /// <summary>
        /// Display database variables
        /// </summary>
        public const string DBVARIABLES = "DBVARIABLES";
        /// <summary>
        /// Dialog format Views list and Display Views
        /// </summary>
        public const string DBVIEWS = "DBVIEWS";
        /// <summary>
        /// Sort data in descending order
        /// </summary>
        public const string DESCENDING = "DESCENDING";
        /// <summary>
        /// Define a variable of standard, global or permanent scope
        /// </summary>
        public const string DEFINE = "DEFINE";
        /// <summary>
        /// Delete table, file or records
        /// </summary>
        public const string DELETE = "DELETE";
        /// <summary>
        /// Creates a dialog box
        /// </summary>
        public const string DIALOG = "DIALOG";
        /// <summary>
        /// Command to disable a field
        /// </summary>
        public const string DISABLE = "DISABLE";
        /// <summary>
        /// Command to send the current output file to the default Display
        /// </summary>
        public const string DISPLAY = "DISPLAY";
        /// <summary>
        /// Command to display only selected fields in AUTOSEARCH results; Used with AUTOSEARCH command
        /// </summary>
        public const string DISPLAYLIST = "DISPLAYLIST";
        /// <summary>
        /// Else of If command
        /// </summary>
		public const string ELSE = "ELSE";
        /// <summary>
        /// Command to enable a field
        /// </summary>
        public const string ENABLE = "ENABLE";
        /// <summary>
        /// End of If command
        /// </summary>
        public const string END = "END";
        /// <summary>
        /// ENVIRON system function
        /// </summary>
        public const string ENVIRON = "ENVIRON";
        /// <summary>
        /// EPIWEEK date function
        /// </summary>
        public const string EPIWEEK = "EPIWEEK";
        /// <summary>
        /// Columns excluded from the List command
        /// </summary>
        public const string EXCEPT = "EXCEPT";
        /// <summary>
        /// Executes a program
        /// </summary>
        public const string EXECUTE = "EXECUTE";
        /// <summary>
        /// EXISTS system function
        /// </summary>
        public const string EXISTS = "EXISTS";
        /// <summary>
        /// Exits Analysis
        /// </summary>
        public const string EXIT = "EXIT";
        /// <summary>
        /// EXP exponentiation numeric function
        /// </summary>
        public const string EXP = "EXP";
        /// <summary>
        /// FIELDVAR, used is DISPLAY DBVARIABLES
        /// </summary>
        public const string FIELDVAR = "FIELDVAR";
        /// <summary>
        /// FILEDATE system function
        /// </summary>
        public const string FILEDATE = "FILEDATE";
        /// <summary>
        /// FINDTEXT text function
        /// </summary>
        public const string FINDTEXT = "FINDTEXT";
        /// <summary>
        /// Fixed parameter of FORMAT function
        /// </summary>
        public const string FIXED_FORMAT = "\"Fixed\"";
        /// <summary>
        /// Computes the frequency of any column(s) in the current dataset or a user defined standard variable(s)
        /// </summary>
        public const string FREQ = "FREQ";
        /// <summary>
        /// Frequency Graph Command
        /// </summary>
		public const string FREQGRAPH = "FREQGRAPH";
        /// <summary>
        /// FORMAT text function
        /// </summary>
        public const string FORMAT = "FORMAT";
        /// <summary>
        /// General Date parameter of FORMAT function
        /// </summary>
        public const string GENERAL_DATE_FORMAT = "\"General Date\"";
        /// <summary>
        /// General Number parameter of FORMAT function
        /// </summary>
        public const string GENERAL_NUMBER_FORMAT = "\"General Number\"";
        /// <summary>
        /// Sets the focus on a page, field or new record
        /// </summary>
        public const string GOTO = "GOTO";
        /// <summary>
        /// Goes to Relate Form
        /// </summary>
        public const string GOTOFORM = "GOTOFORM";
        /// <summary>
        /// <summary>
        /// </summary>
        public const string GRAPH = "GRAPH";
		/// <summary>
		/// Type of graph to be displayed following CoxPH, KMSurvival, etc.
		/// </summary>
		public const string GRAPHTYPE = "GRAPHTYPE";
		/// <summary>
        /// Data is displayed as a grid in the List command
        /// </summary>
        public const string GRIDTABLE = "GRIDTABLE";
        /// <summary>
        /// Constant for "Define Group" Command
        /// </summary>
        public const string GROUPVAR = "GROUPVAR";
        /// <summary>
        /// Creates a header in the output .HTM document
        /// </summary>
        public const string HEADER = "HEADER";
        /// <summary>
        /// Opens a .HTM or .CHM help document
        /// </summary>
        public const string HELP = "HELP";
        /// <summary>
        /// Hides field(s)
        /// </summary>
        public const string HIDE = "HIDE";
        /// <summary>
        /// Highlight a field
        /// </summary>
        public const string HIGHLIGHT = "HIGHLIGHT";
        /// <summary>
        /// Highest value
        /// </summary>
        public const string HIVALUE = "HIVALUE";
        /// <summary>
        /// HOUR time function
        /// </summary>
        public const string HOUR = "HOUR";
        /// <summary>
        /// HOURS time function
        /// </summary>
        public const string HOURS = "HOURS";
        /// <summary>
        /// Hyperlinks Command
        /// </summary>
		public const string HYPERLINKS = "HYPERLINKS";
        /// <summary>
        /// Perform a command or a set of commands conditionally
        /// </summary>
        public const string IF = "IF";
        /// <summary>
        /// Perform a command or a set of commands conditionally
        /// </summary>
        public const string ISUNIQUE = "ISUNIQUE";
        /// <summary>
        /// Kaplan-Meier Survival 
        /// </summary>
        public const string KMSURVIVAL = "KMSURVIVAL";
        /// <summary>
        /// String comparison operator LIKE that compares strings that have LIKE wildcard characters (*).
        /// </summary>
        public const string LIKE = "LIKE";
        /// <summary>
        /// Line Break text function
        /// </summary>
        public const string LINEBREAK = "LINEBREAK";
        /// <summary>
        /// Lists values of column(s) or user defined standard variable(s) in the current dataset
        /// </summary>
        public const string LIST = "LIST";
        /// <summary>
        /// LN (natural log) numeric function
        /// </summary>
        public const string LN = "LN";
        /// <summary>
        /// LOG (decimal log) numeric function
        /// </summary>
        public const string LOG = "LOG";
        /// <summary>
        /// Logistic regression command
        /// </summary>
        public const string LOGISTIC = "LOGISTIC";
        /// <summary>
        /// Long Date parameter of FORMAT function
        /// </summary>
        public const string LONG_DATE_FORMAT = "\"Long Date\"";
        /// <summary>
        /// Long Time parameter of FORMAT function
        /// </summary>
        public const string LONG_TIME_FORMAT = "\"Long Time\"";
        /// <summary>
        /// Lowest value
        /// </summary>
        public const string LOVALUE = "LOVALUE";
        /// <summary>
        /// Performs a matched analysis of specified exposure and outcome variables
        /// </summary>
        public const string MATCH = "MATCH";
        /// <summary>
        /// Matched analysis
        /// </summary>
        public const string MATCHVAR = "MATCHVAR";
        /// <summary>
        ///  Means command
        /// </summary>
        public const string MEANS = "MEANS";
        /// <summary>
        /// Merge command
        /// </summary>
        public const string MERGE = "MERGE";
        /// <summary>
        /// MINUTE time function
        /// </summary>
        public const string MINUTE = "MINUTE";
        /// <summary>
        /// MINUTES time function
        /// </summary>
        public const string MINUTES = "MINUTES";
        /// <summary>
        /// Missing command
        /// </summary>
        public const string MISSING = "MISSING";
        /// <summary>
        /// MOD function (modulus or remainder)
        /// </summary>
        public const string MOD = "MOD";
        /// <summary>
        /// MONTH date function 
        /// </summary>
        public const string MONTH = "MONTH";
        /// <summary>
        /// MONTHS date function 
        /// </summary>
        public const string MONTHS = "MONTHS";
        /// <summary>
        /// Creates a new record
        /// </summary>
        public const string NEWRECORD = "NEWRECORD";
        /// <summary>
        /// No Intercept in Logistic Regression
        /// </summary>
        public const string NOINTERCEPT = "NOINTERCEPT";
        /// <summary>
        /// NOT logical operator
        /// </summary>
        public const string NOT = "NOT";
        /// <summary>
        /// Do not wait for program to exit
        /// </summary>
        public const string NOWAITFOREXIT = "NOWAITFOREXIT";
        /// <summary>
        /// Prevents line breaks from being inserted in Analysis output so columns as wide as longest line.
        /// </summary>
        public const string NOWRAP = "NOWRAP";
        /// <summary>
        /// NUMTODATE numeric function
        /// </summary>
        public const string NUMTODATE = "NUMTODATE";
        /// <summary>
        /// NUMTOTIME numeric function
        /// </summary>
        public const string NUMTOTIME = "NUMTOTIME";
        /// <summary>
        /// On/Off parameter of FORMAT function
        /// </summary>
        public const string ON_OFF_FORMAT = "\"On/Off\"";
        /// <summary>
        /// OR logical operator
        /// </summary>
        public const string OR = "OR";
        /// <summary>
        /// for specifying the output 'table'
        /// </summary>
        public const string OUTTABLE = "OUTTABLE";
        /// <summary>
        /// Percent number parameter of FORMAT function
        /// </summary>
        public const string PERCENT_FORMAT = "\"Percent\"";
        /// <summary>
        /// Percents command
        /// </summary>
        public const string PERCENTS = "PERCENTS";
        /// <summary>
        /// Scope of the defined variable is permanent
        /// </summary>
        public const string PERMANENT = "PERMANENT";
        /// <summary>
        /// Command to calculate the P value from the Z-score
        /// </summary>
        public const string PFROMZ = "PFROMZ";
        /// <summary>
        /// Command to send the current output file to the default printer
        /// </summary>
        public const string PRINTOUT = "PRINTOUT";
        /// <summary>
        /// For SET PROCESS = (undeleted, deleted, both) records.
        /// </summary>
		public const string PROCESS = "PROCESS";
        /// <summary>
        /// Variable for the Primary Sampling Unit (PSU) in Complex Sample commands.
        /// </summary>
        public const string PSUVAR = "PSUVAR";
		/// <summary>
		/// Variable for the Primary Sampling Unit (PSU) in Complex Sample commands.
		/// </summary>
		public const string PVALUE = "PVALUE";
		/// <summary>
        /// Command to exit Analysis module
        /// </summary>
        public const string QUIT = "QUIT";
        /// <summary>
        /// Command to read project data from an Access 97, Access 2000, SQL Server 2000 or MSDE 2000 database
        /// </summary>
        public const string READ = "READ";
        /// <summary>
        /// Translates data in a column or a standard variable to another column or standard variable 
        /// </summary>
        public const string RECODE = "RECODE";
        /// <summary>
        /// Command to return the current count of saved records (not counting * new record)
        /// </summary>
        public const string RECORDCOUNT = "RECORDCOUNT";
        /// <summary>
        /// Linear regression command
        /// </summary>
        public const string REGRESS = "REGRESS";
        /// <summary>
        /// Replaces an existing output file or an output table while exporting data
        /// </summary>
        public const string REPLACE = "REPLACE";
        /// <summary>
        /// Join tables.
        /// </summary>        
        public const string RELATE = "RELATE";
        /// <summary>
        /// RND (random) numeric function
        /// </summary>
        public const string RND = "RND";
        /// <summary>
        /// Report command keyword
        /// </summary>
        public const string REPORT = "REPORT";
       
        /// <summary>
        /// ROUND numeric function
        /// </summary>
        public const string ROUND = "ROUND";

        /// <remarks>If no output is selected Epi Info will create a new file with a sequential number.</remarks>
        /// <example>ROUTEOUT 'D:\EPIInfo\Monthly_Report.htm' REPLACE</example>
        /// </summary>
        public const string ROUTEOUT = "ROUTEOUT";
        /// <summary>
        /// Runs an existing program
        /// </summary>
        public const string RUNPGM = "RUNPGM";
        /// <summary>
        /// No confirmatory messages will be displayed
        /// </summary>
        public const string RUNSILENT = "RUNSILENT";
        /// <summary>
        /// Data tables will not be deleted
        /// </summary>
        public const string SAVEDATA = "SAVEDATA";
        /// <summary>
        /// Scientific number parameter of FORMAT function
        /// </summary>
        public const string SCIENTIFIC_FORMAT = "\"Scientific\"";
        /// <summary>
        /// SECOND time function
        /// </summary>
        public const string SECOND = "SECOND";
        /// <summary>
        /// SECONDS time function
        /// </summary>
        public const string SECONDS = "SECONDS";
        /// <summary>
        /// Selects only the records that match the specified expression
        /// </summary>
        public const string SELECT = "SELECT";
        /// <summary>
        /// Sets various options that affect the performance and output of Analysis
        /// </summary>
        public const string SET = "SET";
        /// <summary>
        /// Sets the attribute required = true for one or more fields in Check Code.
        /// </summary>
        public const string SET_REQUIRED = "SET-REQUIRED";
        /// <summary>
        /// Sets the attribute required = false for one or more fields in Check Code.
        /// </summary>
        public const string SET_NOT_REQUIRED = "SET-NOT-REQUIRED";
        /// <summary>
        /// Short Date parameter of FORMAT function
        /// </summary>
        public const string SHORT_DATE_FORMAT = "\"Short Date\"";
        /// <summary>
        /// Short Time parameter of FORMAT function
        /// </summary>
        public const string SHORT_TIME_FORMAT = "\"Short Time\"";
        /// <summary>
        /// Show prompts command
        /// </summary>
		public const string SHOWPROMPTS = "SHOWPROMPTS";
        /// <summary>
        /// SIN numeric function
        /// </summary>
        public const string SIN = "SIN";
        /// <summary>
        /// Sorts the data in the current dataset by columns or variables and sort order specified
        /// </summary>
        public const string SORT = "SORT";
        /// <summary>
        /// Standard number parameter of FORMAT function
        /// </summary>
        public const string STANDARD_FORMAT = "\"Standard\"";
        /// <summary>
        /// Statistics Command
        /// </summary>
		public const string STATISTICS = "STATISTICS";
        /// <summary>
        /// Step Command
        /// </summary>
        public const string STEP = "STEP";
        /// <summary>
        /// Stratify by the variable
        /// </summary>
        public const string STRATAVAR = "STRATAVAR";
        ///<summary>
        /// STRLEN text function
        ///</summary>
        public const string STRLEN = "STRLEN";
        ///<summary>
        /// SUBSTRING text function
        ///</summary>
        public const string SUBSTRING = "SUBSTRING";
        /// <summary>
        /// Computes the aggregates of a column in the current dataset or a standard variable
        /// </summary>
        public const string SUMMARIZE = "SUMMARIZE";
        /// <summary>
        /// SYSTEMDATE system function
        /// </summary>
        public const string SYSTEMDATE = "SYSTEMDATE";
        /// <summary>
        /// SYSTEMTIME system function
        /// </summary>
        public const string SYSTEMTIME = "SYSTEMTIME";
        /// FIRSTSAVETIME system function
        /// </summary>
        /// --123
        public const string FIRSTSAVETIME = "FIRSTSAVETIME";
        ///LASTSAVETIME system function
        /// </summary>
        public const string LASTSAVETIME = "LASTSAVETIME";
        //---
        /// <summary>
        /// Performs a cross-tabulation  of the specified exposure and outcome variables
        /// </summary>
        public const string TABLES = "TABLES";
        /// <summary>
        /// TAN numeric function
        /// </summary>
        public const string TAN = "TAN";
        /// <summary>
        /// A dialogs title text
        /// </summary>
        public const string TITLETEXT = "TITLETEXT";
        /// <summary>
        /// TYPEOUT Command font modifier.
        /// </summary>
        public const string TEXTFONT = "TEXTFONT";
        /// <summary>
        /// The THEN Command
        /// </summary>
		public const string THEN = "THEN";
        /// <summary>
		/// Unit of time used for graphs following CoxPH, KMSurvival, etc.
        /// </summary>
        public const string TIMEUNIT = "TIMEUNIT";
		/// <summary>
		/// In Summarize command, the table to which output is written
		/// </summary>
		public const string TO = "TO";
        /// <summary>
        /// True/False parameter of FORMAT function
        /// </summary>
        public const string TRUE_FALSE_FORMAT = "\"True/False\"";
        /// <summary>
        /// TRUNC (truncate) numeric function
        /// </summary>
        public const string TRUNC = "TRUNC";
        /// <summary>
        /// The TYPEOUT Command inserts text, either a string or the
        /// contents of a file, into the output.
        /// <remarks>Typical uses might include comments or boilerplate.</remarks>
        /// <example>TYPEOUT 'My Fancy Logo.htm'</example>
        /// </summary>
        public const string TYPEOUT = "TYPEOUT";
        /// <summary>
        /// TXTTODATE text function
        /// </summary>
        public const string TXTTODATE = "TXTTODATE";
        /// <summary>
        /// TXTTONUM text function
        /// </summary>
        public const string TXTTONUM = "TXTTONUM";
        /// <summary>
        /// Undefine a defined variable
        /// </summary>
        public const string UNDEFINE = "UNDEFINE";
        /// <summary>
        /// Undelete records that were marked as deleted
        /// </summary>
        public const string UNDELETE = "UNDELETE";
        /// <summary>
        /// Unhide field(s)
        /// </summary>
        public const string UNHIDE = "UNHIDE";
        /// <summary>
        /// Unhighlight a field
        /// </summary>
        public const string UNHIGHLIGHT = "UNHIGHLIGHT";
        /// <summary>
        /// Allows update of data in the List command
        /// </summary>
        public const string UPDATE = "UPDATE";
        /// <summary>
        /// UPPERCASE text function 
        /// </summary>
        public const string UPPERCASE = "UPPERCASE";
        /// <summary>
        /// Wait for program to exit
        /// </summary>
        public const string WAITFOREXIT = "WAITFOREXIT";
        /// <summary>
        /// Weight variable
        /// </summary>
        public const string WEIGHTVAR = "WEIGHTVAR";
        /// <summary>
        /// Export the data to an output table in the current project or a different project
        /// </summary>
        public const string WRITE = "WRITE";
        /// <summary>
        /// XOR--exclusive OR logical operator
        /// </summary>
        public const string XOR = "XOR";
        /// <summary>
        /// Yes/No number parameter of FORMAT function
        /// </summary>
        public const string YESNO_FORMAT = "\"Yes/No\"";
        /// <summary>
        /// A dialog format in Dialog command
        /// </summary>
        public const string YN = "YN";
        /// <summary>
        /// YEAR date function
        /// </summary>
        public const string YEAR = "YEAR";
        /// <summary>
        /// YEARS date function
        /// </summary>
        public const string YEARS = "YEARS";
        /// <summary>
        /// ZSCORE numeric function
        /// </summary>
        public const string ZSCORE = "ZSCORE";
    }
}
