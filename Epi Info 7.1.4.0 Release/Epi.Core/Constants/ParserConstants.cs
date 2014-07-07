using System;
namespace Epi.Parser
{
    /// <summary>
    /// Parser Symbols enumeration.
    /// </summary>
    public enum Symbols
    {
        /// <summary>EOF Symbol</summary>
        SYMBOL_EOF,
        /// <summary>ERROR Symbol</summary>
        SYMBOL_ERROR,
        /// <summary>WHITESPACE Symbol</summary>
        SYMBOL_WHITESPACE,
        /// <summary>MINUS Symbol</summary>
        SYMBOL_MINUS,
        /// <summary>QUOTEEPI2000QUOTE Symbol</summary>
        SYMBOL_QUOTEEPI2000QUOTE,
        /// <summary>QUOTEEPI7QUOTE Symbol</summary>
        SYMBOL_QUOTEEPI7QUOTE,
        /// <summary>QUOTEEXCEL8DOT0QUOTE Symbol</summary>
        SYMBOL_QUOTEEXCEL8DOT0QUOTE,
        /// <summary>QUOTEODBCQUOTE Symbol</summary>
        SYMBOL_QUOTEODBCQUOTE,
        /// <summary>QUOTESQLQUOTE Symbol</summary>
        SYMBOL_QUOTESQLQUOTE,
        /// <summary>PERCENT Symbol</summary>
        SYMBOL_PERCENT,
        /// <summary>AMP Symbol</summary>
        SYMBOL_AMP,
        /// <summary>LPARAN Symbol</summary>
        SYMBOL_LPARAN,
        /// <summary>RPARAN Symbol</summary>
        SYMBOL_RPARAN,
        /// <summary>TIMES Symbol</summary>
        SYMBOL_TIMES,
        /// <summary>COMMA Symbol</summary>
        SYMBOL_COMMA,
        /// <summary>DOT Symbol</summary>
        SYMBOL_DOT,
        /// <summary>DOTDOTBACKSLASH Symbol</summary>
        SYMBOL_DOTDOTBACKSLASH,
        /// <summary>DOTDOTBACKSLASHATAT Symbol</summary>
        SYMBOL_DOTDOTBACKSLASHATAT,
        /// <summary>DOTBACKSLASH Symbol</summary>
        SYMBOL_DOTBACKSLASH,
        /// <summary>DOTBACKSLASHATAT Symbol</summary>
        SYMBOL_DOTBACKSLASHATAT,
        /// <summary>DIV Symbol</summary>
        SYMBOL_DIV,
        /// <summary>COLON Symbol</summary>
        SYMBOL_COLON,
        /// <summary>COLONDIVDIV Symbol</summary>
        SYMBOL_COLONDIVDIV,
        /// <summary>COLONCOLON Symbol</summary>
        SYMBOL_COLONCOLON,
        /// <summary>SEMI Symbol</summary>
        SYMBOL_SEMI,
        /// <summary>ATAT Symbol</summary>
        SYMBOL_ATAT,
        /// <summary>BACKSLASH Symbol</summary>
        SYMBOL_BACKSLASH,
        /// <summary>CARET Symbol</summary>
        SYMBOL_CARET,
        /// <summary>PLUS Symbol</summary>
        SYMBOL_PLUS,
        /// <summary>LT Symbol</summary>
        SYMBOL_LT,
        /// <summary>LTEQ Symbol</summary>
        SYMBOL_LTEQ,
        /// <summary>LTGT Symbol</summary>
        SYMBOL_LTGT,
        /// <summary>EQ Symbol</summary>
        SYMBOL_EQ,
        /// <summary>GT Symbol</summary>
        SYMBOL_GT,
        /// <summary>GTEQ Symbol</summary>
        SYMBOL_GTEQ,
        /// <summary>ABOUT Symbol</summary>
        SYMBOL_ABOUT,
        /// <summary>ABS Symbol</summary>
        SYMBOL_ABS,
        /// <summary>ALL Symbol</summary>
        SYMBOL_ALL,
        /// <summary>ALWAYS Symbol</summary>
        SYMBOL_ALWAYS,
        /// <summary>AND Symbol</summary>
        SYMBOL_AND,
        /// <summary>APPEND Symbol</summary>
        SYMBOL_APPEND,
        /// <summary>ASCENDING Symbol</summary>
        SYMBOL_ASCENDING,
        /// <summary>ASSIGN Symbol</summary>
        SYMBOL_ASSIGN,
        /// <summary>AUTOSEARCH Symbol</summary>
        SYMBOL_AUTOSEARCH,
        /// <summary>AVG Symbol</summary>
        SYMBOL_AVG,
        /// <summary>BEEP Symbol</summary>
        SYMBOL_BEEP,
        /// <summary>BEGIN Symbol</summary>
        SYMBOL_BEGIN,
        /// <summary>BOOLEAN Symbol</summary>
        SYMBOL_BOOLEAN,
        /// <summary>BOTH Symbol</summary>
        SYMBOL_BOTH,
        /// <summary>BROWSER Symbol</summary>
        SYMBOL_BROWSER,
        /// <summary>BROWSERSIZE Symbol</summary>
        SYMBOL_BROWSERSIZE,
        /// <summary>BUTTON Symbol</summary>
        SYMBOL_BUTTON,
        /// <summary>BUTTONS Symbol</summary>
        SYMBOL_BUTTONS,
        /// <summary>CALL Symbol</summary>
        SYMBOL_CALL,
        /// <summary>CANCEL Symbol</summary>
        SYMBOL_CANCEL,
        /// <summary>CASE BASED Symbol</summary>
        SYMBOL_CASE_BASED,
        /// <summary>CHARLITERAL Symbol</summary>
        SYMBOL_CHARLITERAL,
        /// <summary>CLEAR Symbol</summary>
        SYMBOL_CLEAR,
        /// <summary>CLOSEOUT Symbol</summary>
        SYMBOL_CLOSEOUT,
        /// <summary>CMD Symbol</summary>
        SYMBOL_CMD,
        /// <summary>COLUMNSIZE Symbol</summary>
        SYMBOL_COLUMNSIZE,
        /// <summary>COMMANDLINE Symbol</summary>
        SYMBOL_COMMANDLINE,
        /// <summary>COMPLETE Symbol</summary>
        SYMBOL_COMPLETE,
        /// <summary>COS Symbol</summary>
        SYMBOL_COS,
        /// <summary>COULMNSIZE Symbol</summary>
        SYMBOL_COULMNSIZE,
        /// <summary>COUNTLPARAN Symbol</summary>
        SYMBOL_COUNTLPARAN,
        /// <summary>DATABASES Symbol</summary>
        SYMBOL_DATABASES,
        /// <summary>DATE Symbol</summary>
        SYMBOL_DATE,
        /// <summary>DATEFORMAT Symbol</summary>
        SYMBOL_DATEFORMAT,
        /// <summary>DAY Symbol</summary>
        SYMBOL_DAY,
        /// <summary>DAYS Symbol</summary>
        SYMBOL_DAYS,
        /// <summary>DBVALUES Symbol</summary>
        SYMBOL_DBVALUES,
        /// <summary>DBVARIABLES Symbol</summary>
        SYMBOL_DBVARIABLES,
        /// <summary>DBVIEWS Symbol</summary>
        SYMBOL_DBVIEWS,
        /// <summary>DECLITERAL Symbol</summary>
        SYMBOL_DECLITERAL,
        /// <summary>DEFINE Symbol</summary>
        SYMBOL_DEFINE,
        /// <summary>DELETE Symbol</summary>
        SYMBOL_DELETE,
        /// <summary>DELETED Symbol</summary>
        SYMBOL_DELETED,
        /// <summary>DENOMINATOR Symbol</summary>
        SYMBOL_DENOMINATOR,
        /// <summary>DESCENDING Symbol</summary>
        SYMBOL_DESCENDING,
        /// <summary>DIALOG Symbol</summary>
        SYMBOL_DIALOG,
        /// <summary>DISPLAY Symbol</summary>
        SYMBOL_DISPLAY,
        /// <summary>DLLOBJECT Symbol</summary>
        SYMBOL_DLLOBJECT,
        /// <summary>ELSE Symbol</summary>
        SYMBOL_ELSE,
        /// <summary>END Symbol</summary>
        SYMBOL_END,
        /// <summary>ENVIRON Symbol</summary>
        SYMBOL_ENVIRON,
        /// <summary>EXCELRANGE Symbol</summary>
        SYMBOL_EXCELRANGE,
        /// <summary>EXCEPT Symbol</summary>
        SYMBOL_EXCEPT,
        /// <summary>EXECUTE Symbol</summary>
        SYMBOL_EXECUTE,
        /// <summary>EXISTS Symbol</summary>
        SYMBOL_EXISTS,
        /// <summary>EXIT Symbol</summary>
        SYMBOL_EXIT,
        /// <summary>EXITBUTTON Symbol</summary>
        SYMBOL_EXITBUTTON,
        /// <summary>EXP Symbol</summary>
        SYMBOL_EXP,
        /// <summary>FIELDVAR Symbol</summary>
        SYMBOL_FIELDVAR,
        /// <summary>FILE Symbol</summary>
        SYMBOL_FILE,
        /// <summary>FILEDATE Symbol</summary>
        SYMBOL_FILEDATE,
        /// <summary>FILEDIALOG Symbol</summary>
        SYMBOL_FILEDIALOG,
        /// <summary>FILESPEC Symbol</summary>
        SYMBOL_FILESPEC,
        /// <summary>FINDTEXT Symbol</summary>
        SYMBOL_FINDTEXT,
        /// <summary>FORMAT Symbol</summary>
        SYMBOL_FORMAT,
        /// <summary>FREQ Symbol</summary>
        SYMBOL_FREQ,
        /// <summary>FREQGRAPH Symbol</summary>
        SYMBOL_FREQGRAPH,
        /// <summary>FROM Symbol</summary>
        SYMBOL_FROM,
        /// <summary>FTP Symbol</summary>
        SYMBOL_FTP,
        /// <summary>GETPATH Symbol</summary>
        SYMBOL_GETPATH,
        /// <summary>GLOBAL Symbol</summary>
        SYMBOL_GLOBAL,
        /// <summary>GOTO Symbol</summary>
        SYMBOL_GOTO,
        /// <summary>GRAPH Symbol</summary>
        SYMBOL_GRAPH,
        /// <summary>GRAPHTYPE Symbol</summary>
        SYMBOL_GRAPHTYPE,
        /// <summary>GRIDTABLE Symbol</summary>
        SYMBOL_GRIDTABLE,
        /// <summary>GROUPVAR Symbol</summary>
        SYMBOL_GROUPVAR,
        /// <summary>HEADER Symbol</summary>
        SYMBOL_HEADER,
        /// <summary>HELP Symbol</summary>
        SYMBOL_HELP,
        /// <summary>HEXLITERAL Symbol</summary>
        SYMBOL_HEXLITERAL,
        /// <summary>HIDE Symbol</summary>
        SYMBOL_HIDE,
        /// <summary>HIVALUE Symbol</summary>
        SYMBOL_HIVALUE,
        /// <summary>HOUR Symbol</summary>
        SYMBOL_HOUR,
        /// <summary>HOURS Symbol</summary>
        SYMBOL_HOURS,
        /// <summary>HTTP Symbol</summary>
        SYMBOL_HTTP,
        /// <summary>HTTPS Symbol</summary>
        SYMBOL_HTTPS,
        /// <summary>HYPERLINKS Symbol</summary>
        SYMBOL_HYPERLINKS,
        /// <summary>IDENTIFER Symbol</summary>
        SYMBOL_IDENTIFER,
        /// <summary>IDENTIFIER Symbol</summary>
        SYMBOL_IDENTIFIER,
        /// <summary>IF Symbol</summary>
        SYMBOL_IF,
        /// <summary>IGNORE Symbol</summary>
        SYMBOL_IGNORE,
        /// <summary>INDENTIFIER Symbol</summary>
        SYMBOL_INDENTIFIER,
        /// <summary>INTERMEDIATE Symbol</summary>
        SYMBOL_INTERMEDIATE,
        /// <summary>INTERVAL Symbol</summary>
        SYMBOL_INTERVAL,
        /// <summary>KEYVARS Symbol</summary>
        SYMBOL_KEYVARS,
        /// <summary>KMSURVIVAL Symbol</summary>
        SYMBOL_KMSURVIVAL,
        /// <summary>LET Symbol</summary>
        SYMBOL_LET,
        /// <summary>LIKE Symbol</summary>
        SYMBOL_LIKE,
        /// <summary>LINENUMBERS Symbol</summary>
        SYMBOL_LINENUMBERS,
        /// <summary>LINK Symbol</summary>
        SYMBOL_LINK,
        /// <summary>LINKNAME Symbol</summary>
        SYMBOL_LINKNAME,
        /// <summary>LINKREMOVE Symbol</summary>
        SYMBOL_LINKREMOVE,
        /// <summary>LIST Symbol</summary>
        SYMBOL_LIST,
        /// <summary>LITERAL Symbol</summary>
        SYMBOL_LITERAL,
        /// <summary>LN Symbol</summary>
        SYMBOL_LN,
        /// <summary>LOG Symbol</summary>
        SYMBOL_LOG,
        /// <summary>LOGISTIC Symbol</summary>
        SYMBOL_LOGISTIC,
        /// <summary>LOVALUE Symbol</summary>
        SYMBOL_LOVALUE,
        /// <summary>MAP Symbol</summary>
        SYMBOL_MAP,
        /// <summary>MATCH Symbol</summary>
        SYMBOL_MATCH,
        /// <summary>MATCHING Symbol</summary>
        SYMBOL_MATCHING,
        /// <summary>MATCHVAR Symbol</summary>
        SYMBOL_MATCHVAR,
        /// <summary>MAX Symbol</summary>
        SYMBOL_MAX,
        /// <summary>MEANS Symbol</summary>
        SYMBOL_MEANS,
        /// <summary>MENU Symbol</summary>
        SYMBOL_MENU,
        /// <summary>MENUITEM Symbol</summary>
        SYMBOL_MENUITEM,
        /// <summary>MERGE Symbol</summary>
        SYMBOL_MERGE,
        /// <summary>MIN Symbol</summary>
        SYMBOL_MIN,
        /// <summary>MINIMAL Symbol</summary>
        SYMBOL_MINIMAL,
        /// <summary>MINUTES Symbol</summary>
        SYMBOL_MINUTES,
        /// <summary>MISSING Symbol</summary>
        SYMBOL_MISSING,
        /// <summary>MNU Symbol</summary>
        SYMBOL_MNU,
        /// <summary>MOD Symbol</summary>
        SYMBOL_MOD,
        /// <summary>MONTH Symbol</summary>
        SYMBOL_MONTH,
        /// <summary>MONTHS Symbol</summary>
        SYMBOL_MONTHS,
        /// <summary>MOVEBUTTONS Symbol</summary>
        SYMBOL_MOVEBUTTONS,
        /// <summary>NEWLINE Symbol</summary>
        SYMBOL_NEWLINE,
        /// <summary>NEWPAGE Symbol</summary>
        SYMBOL_NEWPAGE,
        /// <summary>NEWRECORD Symbol</summary>
        SYMBOL_NEWRECORD,
        /// <summary>NOIMAGE Symbol</summary>
        SYMBOL_NOIMAGE,
        /// <summary>NOINTERCEPT Symbol</summary>
        SYMBOL_NOINTERCEPT,
        /// <summary>NONE Symbol</summary>
        SYMBOL_NONE,
        /// <summary>NOT Symbol</summary>
        SYMBOL_NOT,
        /// <summary>NOWAITFOREXIT Symbol</summary>
        SYMBOL_NOWAITFOREXIT,
        /// <summary>NOWRAP Symbol</summary>
        SYMBOL_NOWRAP,
        /// <summary>NUMERIC Symbol</summary>
        SYMBOL_NUMERIC,
        /// <summary>NUMTODATE Symbol</summary>
        SYMBOL_NUMTODATE,
        /// <summary>NUMTOTIME Symbol</summary>
        SYMBOL_NUMTOTIME,
        /// <summary>OFF Symbol</summary>
        SYMBOL_OFF,
        /// <summary>ON Symbol</summary>
        SYMBOL_ON,
        /// <summary>ONBROWSEREXIT Symbol</summary>
        SYMBOL_ONBROWSEREXIT,
        /// <summary>OR Symbol</summary>
        SYMBOL_OR,
        /// <summary>OUTTABLE Symbol</summary>
        SYMBOL_OUTTABLE,
        /// <summary>PERCENT2 Symbol</summary>
        SYMBOL_PERCENT2,
        /// <summary>PERCENTS Symbol</summary>
        SYMBOL_PERCENTS,
        /// <summary>PERMANENT Symbol</summary>
        SYMBOL_PERMANENT,
        /// <summary>PGMNAME Symbol</summary>
        SYMBOL_PGMNAME,
        /// <summary>PICTURE Symbol</summary>
        SYMBOL_PICTURE,
        /// <summary>PICTURESIZE Symbol</summary>
        SYMBOL_PICTURESIZE,
        /// <summary>POPUP Symbol</summary>
        SYMBOL_POPUP,
        /// <summary>PRINTOUT Symbol</summary>
        SYMBOL_PRINTOUT,
        /// <summary>PROCESS Symbol</summary>
        SYMBOL_PROCESS,
        /// <summary>PSUVAR Symbol</summary>
        SYMBOL_PSUVAR,
        /// <summary>PVALUE Symbol</summary>
        SYMBOL_PVALUE,
        /// <summary>QUIT Symbol</summary>
        SYMBOL_QUIT,
        /// <summary>READ Symbol</summary>
        SYMBOL_READ,
        /// <summary>REALLITERAL Symbol</summary>
        SYMBOL_REALLITERAL,
        /// <summary>RECODE Symbol</summary>
        SYMBOL_RECODE,
        /// <summary>RECORDCOUNT Symbol</summary>
        SYMBOL_RECORDCOUNT,
        /// <summary>REGRESS Symbol</summary>
        SYMBOL_REGRESS,
        /// <summary>RELATE Symbol</summary>
        SYMBOL_RELATE,
        /// <summary>REPEAT Symbol</summary>
        SYMBOL_REPEAT,
        /// <summary>REPLACE Symbol</summary>
        SYMBOL_REPLACE,
        /// <summary>RND Symbol</summary>
        SYMBOL_RND,
        /// <summary>ROUND Symbol</summary>
        SYMBOL_ROUND,
        /// <summary>ROUTEOUT Symbol</summary>
        SYMBOL_ROUTEOUT,
        /// <summary>RUNPGM Symbol</summary>
        SYMBOL_RUNPGM,
        /// <summary>RUNSILENT Symbol</summary>
        SYMBOL_RUNSILENT,
        /// <summary>SAVEDATA Symbol</summary>
        SYMBOL_SAVEDATA,
        /// <summary>SCREENTEXT Symbol</summary>
        SYMBOL_SCREENTEXT,
        /// <summary>SECOND Symbol</summary>
        SYMBOL_SECOND,
        /// <summary>SECONDS Symbol</summary>
        SYMBOL_SECONDS,
        /// <summary>SELECT Symbol</summary>
        SYMBOL_SELECT,
        /// <summary>SEPARATOR Symbol</summary>
        SYMBOL_SEPARATOR,
        /// <summary>SET Symbol</summary>
        SYMBOL_SET,
        /// <summary>SETBUTTONS Symbol</summary>
        SYMBOL_SETBUTTONS,
        /// <summary>SETDBVERSION Symbol</summary>
        SYMBOL_SETDBVERSION,
        /// <summary>SETDOSWIN Symbol</summary>
        SYMBOL_SETDOSWIN,
        /// <summary>SETIMPORTYEAR Symbol</summary>
        SYMBOL_SETIMPORTYEAR,
        /// <summary>SETINIDIR Symbol</summary>
        SYMBOL_SETINIDIR,
        /// <summary>SETLANGUAGE Symbol</summary>
        SYMBOL_SETLANGUAGE,
        /// <summary>SETPICTURE Symbol</summary>
        SYMBOL_SETPICTURE,
        /// <summary>SETWORKDIR Symbol</summary>
        SYMBOL_SETWORKDIR,
        /// <summary>SHOWPROMPTS Symbol</summary>
        SYMBOL_SHOWPROMPTS,
        /// <summary>SHUTDOWN Symbol</summary>
        SYMBOL_SHUTDOWN,
        /// <summary>SIN Symbol</summary>
        SYMBOL_SIN,
        /// <summary>SORT Symbol</summary>
        SYMBOL_SORT,
        /// <summary>STANDARD Symbol</summary>
        SYMBOL_STANDARD,
        /// <summary>STARTUP Symbol</summary>
        SYMBOL_STARTUP,
        /// <summary>STATISTICS Symbol</summary>
        SYMBOL_STATISTICS,
        /// <summary>STEP Symbol</summary>
        SYMBOL_STEP,
        /// <summary>STRATAVAR Symbol</summary>
        SYMBOL_STRATAVAR,
        /// <summary>STRING Symbol</summary>
        SYMBOL_STRING,
        /// <summary>SUBSTRING Symbol</summary>
        SYMBOL_SUBSTRING,
        /// <summary>SUM Symbol</summary>
        SYMBOL_SUM,
        /// <summary>SUMMARIZE Symbol</summary>
        SYMBOL_SUMMARIZE,
        /// <summary>SYSINFO Symbol</summary>
        SYMBOL_SYSINFO,
        /// <summary>SYSTEMDATE Symbol</summary>
        SYMBOL_SYSTEMDATE,
        /// <summary>SYSTEMTIME Symbol</summary>
        SYMBOL_SYSTEMTIME,
        /// <summary>TABLES Symbol</summary>
        SYMBOL_TABLES,
        /// <summary>TAN Symbol</summary>
        SYMBOL_TAN,
        /// <summary>TEMPLATE Symbol</summary>
        SYMBOL_TEMPLATE,
        /// <summary>TEXTFONT Symbol</summary>
        SYMBOL_TEXTFONT,
        /// <summary>TEXTINPUT Symbol</summary>
        SYMBOL_TEXTINPUT,
        /// <summary>THEN Symbol</summary>
        SYMBOL_THEN,
        /// <summary>THREED Symbol</summary>
        SYMBOL_THREED,
        /// <summary>TIME Symbol</summary>
        SYMBOL_TIME,
        /// <summary>TIMEUNIT Symbol</summary>
        SYMBOL_TIMEUNIT,
        /// <summary>TITLETEXT Symbol</summary>
        SYMBOL_TITLETEXT,
        /// <summary>TO Symbol</summary>
        SYMBOL_TO,
        /// <summary>TRUNC Symbol</summary>
        SYMBOL_TRUNC,
        /// <summary>TXTTODATE Symbol</summary>
        SYMBOL_TXTTODATE,
        /// <summary>TXTTONUM Symbol</summary>
        SYMBOL_TXTTONUM,
        /// <summary>TYPEOUT Symbol</summary>
        SYMBOL_TYPEOUT,
        /// <summary>UNDEFINE Symbol</summary>
        SYMBOL_UNDEFINE,
        /// <summary>UNDELETE Symbol</summary>
        SYMBOL_UNDELETE,
        /// <summary>UNDELETED Symbol</summary>
        SYMBOL_UNDELETED,
        /// <summary>UNHIDE Symbol</summary>
        SYMBOL_UNHIDE,
        /// <summary>UNTIL Symbol</summary>
        SYMBOL_UNTIL,
        /// <summary>UPDATE Symbol</summary>
        SYMBOL_UPDATE,
        /// <summary>UPPERCASE Symbol</summary>
        SYMBOL_UPPERCASE,
        /// <summary>URL Symbol</summary>
        SYMBOL_URL,
        /// <summary>USEBROWSER Symbol</summary>
        SYMBOL_USEBROWSER,
        /// <summary>VIEWNAME Symbol</summary>
        SYMBOL_VIEWNAME,
        /// <summary>WAITFOR Symbol</summary>
        SYMBOL_WAITFOR,
        /// <summary>WAITFOREXIT Symbol</summary>
        SYMBOL_WAITFOREXIT,
        /// <summary>WAITFORFILEEXISTS Symbol</summary>
        SYMBOL_WAITFORFILEEXISTS,
        /// <summary>WEIGHTVAR Symbol</summary>
        SYMBOL_WEIGHTVAR,
        /// <summary>WRITE Symbol</summary>
        SYMBOL_WRITE,
        /// <summary>XOR Symbol</summary>
        SYMBOL_XOR,
        /// <summary>XTITLE Symbol</summary>
        SYMBOL_XTITLE,
        /// <summary>YEAR Symbol</summary>
        SYMBOL_YEAR,
        /// <summary>YEARS Symbol</summary>
        SYMBOL_YEARS,
        /// <summary>YN Symbol</summary>
        SYMBOL_YN,
        /// <summary>YTITLE Symbol</summary>
        SYMBOL_YTITLE,
        /// <summary>ABOUT STATEMENT Symbol</summary>
        SYMBOL_ABOUT_STATEMENT,
        /// <summary>ADDEXP Symbol</summary>
        SYMBOL_ADDEXP,
        /// <summary>AGGREGATEELEMENT Symbol</summary>
        SYMBOL_AGGREGATEELEMENT,
        /// <summary>AGGREGATELIST Symbol</summary>
        SYMBOL_AGGREGATELIST,
        /// <summary>AGGREGATEVARIABLEELEMENT Symbol</summary>
        SYMBOL_AGGREGATEVARIABLEELEMENT,
        /// <summary>ALL GLOBAL UNDEFINE STATEMENT Symbol</summary>
        SYMBOL_ALL_GLOBAL_UNDEFINE_STATEMENT,
        /// <summary>ALL STANDARD UNDEFINE STATEMENT Symbol</summary>
        SYMBOL_ALL_STANDARD_UNDEFINE_STATEMENT,
        /// <summary>ALWAYS STATEMENT Symbol</summary>
        SYMBOL_ALWAYS_STATEMENT,
        /// <summary>ANDEXP Symbol</summary>
        SYMBOL_ANDEXP,
        /// <summary>APPEND ROUTEOUT STATEMENT Symbol</summary>
        SYMBOL_APPEND_ROUTEOUT_STATEMENT,
        /// <summary>ASSIGN STATEMENT Symbol</summary>
        SYMBOL_ASSIGN_STATEMENT,
        /// <summary>AUTO SEARCH STATEMENT Symbol</summary>
        SYMBOL_AUTO_SEARCH_STATEMENT,
        /// <summary>BASETERM Symbol</summary>
        SYMBOL_BASETERM,
        /// <summary>BEEP STATEMENT Symbol</summary>
        SYMBOL_BEEP_STATEMENT,
        /// <summary>BROWSER SIZE STATEMENT Symbol</summary>
        SYMBOL_BROWSER_SIZE_STATEMENT,
        /// <summary>BROWSER STATEMENT Symbol</summary>
        SYMBOL_BROWSER_STATEMENT,
        /// <summary>BROWSER1STATEMENT Symbol</summary>
        SYMBOL_BROWSER1STATEMENT,
        /// <summary>BROWSER2STATEMENT Symbol</summary>
        SYMBOL_BROWSER2STATEMENT,
        /// <summary>BROWSER3STATEMENT Symbol</summary>
        SYMBOL_BROWSER3STATEMENT,
        /// <summary>BROWSERSIZE1STATEMENT Symbol</summary>
        SYMBOL_BROWSERSIZE1STATEMENT,
        /// <summary>BROWSERSIZE2STATEMENT Symbol</summary>
        SYMBOL_BROWSERSIZE2STATEMENT,
        /// <summary>BUTTON OFFSET 1 STATEMENT Symbol</summary>
        SYMBOL_BUTTON_OFFSET_1_STATEMENT,
        /// <summary>BUTTON OFFSET 2 STATEMENT Symbol</summary>
        SYMBOL_BUTTON_OFFSET_2_STATEMENT,
        /// <summary>BUTTON OFFSET SIZE 1 STATEMENT Symbol</summary>
        SYMBOL_BUTTON_OFFSET_SIZE_1_STATEMENT,
        /// <summary>BUTTON OFFSET SIZE 2 STATEMENT Symbol</summary>
        SYMBOL_BUTTON_OFFSET_SIZE_2_STATEMENT,
        /// <summary>CALL STATEMENT Symbol</summary>
        SYMBOL_CALL_STATEMENT,
        /// <summary>CANCEL SELECT BY SELECTING STATEMENT Symbol</summary>
        SYMBOL_CANCEL_SELECT_BY_SELECTING_STATEMENT,
        /// <summary>CANCEL SELECT STATEMENT Symbol</summary>
        SYMBOL_CANCEL_SELECT_STATEMENT,
        /// <summary>CANCEL SORT BY SORTING STATEMENT Symbol</summary>
        SYMBOL_CANCEL_SORT_BY_SORTING_STATEMENT,
        /// <summary>CANCEL SORT STATEMENT Symbol</summary>
        SYMBOL_CANCEL_SORT_STATEMENT,
        /// <summary>CAPTIONLINKSTATEMENT Symbol</summary>
        SYMBOL_CAPTIONLINKSTATEMENT,
        /// <summary>CLEAR STATEMENT Symbol</summary>
        SYMBOL_CLEAR_STATEMENT,
        /// <summary>CLOSE OUT STATEMENT Symbol</summary>
        SYMBOL_CLOSE_OUT_STATEMENT,
        /// <summary>CMD LINE STATEMENT Symbol</summary>
        SYMBOL_CMD_LINE_STATEMENT,
        /// <summary>CMDLINEOPTFILE Symbol</summary>
        SYMBOL_CMDLINEOPTFILE,
        /// <summary>CMDLINEOPTFILEPGM Symbol</summary>
        SYMBOL_CMDLINEOPTFILEPGM,
        /// <summary>CMDLINEOPTPGM Symbol</summary>
        SYMBOL_CMDLINEOPTPGM,
        /// <summary>CMDLINEOPTPROJECTPGM Symbol</summary>
        SYMBOL_CMDLINEOPTPROJECTPGM,
        /// <summary>CMDLINEOPTSTRING Symbol</summary>
        SYMBOL_CMDLINEOPTSTRING,
        /// <summary>CMDLINEOPTVIEW Symbol</summary>
        SYMBOL_CMDLINEOPTVIEW,
        /// <summary>CMDLISTOPT Symbol</summary>
        SYMBOL_CMDLISTOPT,
        /// <summary>COLOR Symbol</summary>
        SYMBOL_COLOR,
        /// <summary>COLUMN ALL TABLES STATEMENT Symbol</summary>
        SYMBOL_COLUMN_ALL_TABLES_STATEMENT,
        /// <summary>COLUMN EXCEPT TABLES STATEMENT Symbol</summary>
        SYMBOL_COLUMN_EXCEPT_TABLES_STATEMENT,
        /// <summary>COMMA DELIMITED STRINGS Symbol</summary>
        SYMBOL_COMMA_DELIMITED_STRINGS,
        /// <summary>COMMAND BLOCK Symbol</summary>
        SYMBOL_COMMAND_BLOCK,
        /// <summary>COMPAREEXP Symbol</summary>
        SYMBOL_COMPAREEXP,
        /// <summary>CONCATEXP Symbol</summary>
        SYMBOL_CONCATEXP,
        /// <summary>DATABASES DIALOG STATEMENT Symbol</summary>
        SYMBOL_DATABASES_DIALOG_STATEMENT,
        /// <summary>DATEFORMATOPT Symbol</summary>
        SYMBOL_DATEFORMATOPT,
        /// <summary>DB VALUES DIALOG STATEMENT Symbol</summary>
        SYMBOL_DB_VALUES_DIALOG_STATEMENT,
        /// <summary>DB VARIABLES DIALOG STATEMENT Symbol</summary>
        SYMBOL_DB_VARIABLES_DIALOG_STATEMENT,
        /// <summary>DB VIEWS DIALOG STATEMENT Symbol</summary>
        SYMBOL_DB_VIEWS_DIALOG_STATEMENT,
        /// <summary>DBDATAFORMAT Symbol</summary>
        SYMBOL_DBDATAFORMAT,
        /// <summary>DBVARIABLESOPT Symbol</summary>
        SYMBOL_DBVARIABLESOPT,
        /// <summary>DBVIEWSOPT Symbol</summary>
        SYMBOL_DBVIEWSOPT,
        /// <summary>DECIMAL NUMBER Symbol</summary>
        SYMBOL_DECIMAL_NUMBER,
        /// <summary>DEFAULTDATEFORMATOPT Symbol</summary>
        SYMBOL_DEFAULTDATEFORMATOPT,
        /// <summary>DEFAULTDBVIEWSOPT Symbol</summary>
        SYMBOL_DEFAULTDBVIEWSOPT,
        /// <summary>DEFAULTDISPLAYOPT Symbol</summary>
        SYMBOL_DEFAULTDISPLAYOPT,
        /// <summary>DEFAULTGENERICOPT Symbol</summary>
        SYMBOL_DEFAULTGENERICOPT,
        /// <summary>DEFAULTINTERVALOPT Symbol</summary>
        SYMBOL_DEFAULTINTERVALOPT,
        /// <summary>DEFAULTTEMPLATEOPT Symbol</summary>
        SYMBOL_DEFAULTTEMPLATEOPT,
        /// <summary>DEFAULTTITLESTRINGPART Symbol</summary>
        SYMBOL_DEFAULTTITLESTRINGPART,
        /// <summary>DEFAULTXYTITLEOPT Symbol</summary>
        SYMBOL_DEFAULTXYTITLEOPT,
        /// <summary>DEFINE DLL STATEMENT Symbol</summary>
        SYMBOL_DEFINE_DLL_STATEMENT,
        /// <summary>DEFINE GROUP STATEMENT Symbol</summary>
        SYMBOL_DEFINE_GROUP_STATEMENT,
        /// <summary>DEFINE PROMPT Symbol</summary>
        SYMBOL_DEFINE_PROMPT,
        /// <summary>DEFINE VARIABLE STATEMENT Symbol</summary>
        SYMBOL_DEFINE_VARIABLE_STATEMENT,
        /// <summary>DELETE FILE STATEMENT Symbol</summary>
        SYMBOL_DELETE_FILE_STATEMENT,
        /// <summary>DELETE RECORDS ALL STATEMENT Symbol</summary>
        SYMBOL_DELETE_RECORDS_ALL_STATEMENT,
        /// <summary>DELETE RECORDS SELECTED STATEMENT Symbol</summary>
        SYMBOL_DELETE_RECORDS_SELECTED_STATEMENT,
        /// <summary>DELETE TABLE LONG STATEMENT Symbol</summary>
        SYMBOL_DELETE_TABLE_LONG_STATEMENT,
        /// <summary>DELETE TABLE SHORT STATEMENT Symbol</summary>
        SYMBOL_DELETE_TABLE_SHORT_STATEMENT,
        /// <summary>DELETE TABLE STATEMENT Symbol</summary>
        SYMBOL_DELETE_TABLE_STATEMENT,
        /// <summary>DELETEDOPTION Symbol</summary>
        SYMBOL_DELETEDOPTION,
        /// <summary>DELETEOPT Symbol</summary>
        SYMBOL_DELETEOPT,
        /// <summary>DELETEOPTS Symbol</summary>
        SYMBOL_DELETEOPTS,
        /// <summary>DIALOG DATE MASK STATEMENT Symbol</summary>
        SYMBOL_DIALOG_DATE_MASK_STATEMENT,
        /// <summary>DIALOG DATE STATEMENT Symbol</summary>
        SYMBOL_DIALOG_DATE_STATEMENT,
        /// <summary>DIALOG READ FILTER STATEMENT Symbol</summary>
        SYMBOL_DIALOG_READ_FILTER_STATEMENT,
        /// <summary>DIALOG READ STATEMENT Symbol</summary>
        SYMBOL_DIALOG_READ_STATEMENT,
        /// <summary>DIALOG WRITE FILTER STATEMENT Symbol</summary>
        SYMBOL_DIALOG_WRITE_FILTER_STATEMENT,
        /// <summary>DIALOG WRITE STATEMENT Symbol</summary>
        SYMBOL_DIALOG_WRITE_STATEMENT,
        /// <summary>DIALOGBUTTONSSTATEMENT Symbol</summary>
        SYMBOL_DIALOGBUTTONSSTATEMENT,
        /// <summary>DIALOGDATEFORMATSTATEMENT Symbol</summary>
        SYMBOL_DIALOGDATEFORMATSTATEMENT,
        /// <summary>DIALOGFORMATSTATEMENT Symbol</summary>
        SYMBOL_DIALOGFORMATSTATEMENT,
        /// <summary>DIALOGLISTSTATEMENT Symbol</summary>
        SYMBOL_DIALOGLISTSTATEMENT,
        /// <summary>DISPLAYOPT Symbol</summary>
        SYMBOL_DISPLAYOPT,
        /// <summary>EFFECTLEFT Symbol</summary>
        SYMBOL_EFFECTLEFT,
        /// <summary>EFFECTLIST Symbol</summary>
        SYMBOL_EFFECTLIST,
        /// <summary>EMPTY COMMAND BLOCK Symbol</summary>
        SYMBOL_EMPTY_COMMAND_BLOCK,
        /// <summary>EMPTYFUNCTIONPARAMETERLIST Symbol</summary>
        SYMBOL_EMPTYFUNCTIONPARAMETERLIST,
        /// <summary>EXECUTE FILE STATEMENT Symbol</summary>
        SYMBOL_EXECUTE_FILE_STATEMENT,
        /// <summary>EXECUTE NO WAIT FOR EXIT FILE STATEMENT Symbol</summary>
        SYMBOL_EXECUTE_NO_WAIT_FOR_EXIT_FILE_STATEMENT,
        /// <summary>EXECUTE NO WAIT FOR EXIT STRING STATEMENT Symbol</summary>
        SYMBOL_EXECUTE_NO_WAIT_FOR_EXIT_STRING_STATEMENT,
        /// <summary>EXECUTE NO WAIT FOR EXIT URL STATEMENT Symbol</summary>
        SYMBOL_EXECUTE_NO_WAIT_FOR_EXIT_URL_STATEMENT,
        /// <summary>EXECUTE URL STATEMENT Symbol</summary>
        SYMBOL_EXECUTE_URL_STATEMENT,
        /// <summary>EXECUTE WAIT FOR EXIT FILE STATEMENT Symbol</summary>
        SYMBOL_EXECUTE_WAIT_FOR_EXIT_FILE_STATEMENT,
        /// <summary>EXECUTE WAIT FOR EXIT STRING STATEMENT Symbol</summary>
        SYMBOL_EXECUTE_WAIT_FOR_EXIT_STRING_STATEMENT,
        /// <summary>EXECUTE WAIT FOR EXIT URL STATEMENT Symbol</summary>
        SYMBOL_EXECUTE_WAIT_FOR_EXIT_URL_STATEMENT,
        /// <summary>EXECUTEFILE1STATEMENT Symbol</summary>
        SYMBOL_EXECUTEFILE1STATEMENT,
        /// <summary>EXECUTEFILE2STATEMENT Symbol</summary>
        SYMBOL_EXECUTEFILE2STATEMENT,
        /// <summary>EXECUTEFILE3STATEMENT Symbol</summary>
        SYMBOL_EXECUTEFILE3STATEMENT,
        /// <summary>EXECUTEFILESTRINGSTATEMENT Symbol</summary>
        SYMBOL_EXECUTEFILESTRINGSTATEMENT,
        /// <summary>EXECUTEIDENTIFIEROPTSTATEMENT Symbol</summary>
        SYMBOL_EXECUTEIDENTIFIEROPTSTATEMENT,
        /// <summary>EXECUTETWOFILES1STATEMENT Symbol</summary>
        SYMBOL_EXECUTETWOFILES1STATEMENT,
        /// <summary>EXECUTETWOFILES2STATEMENT Symbol</summary>
        SYMBOL_EXECUTETWOFILES2STATEMENT,
        /// <summary>EXECUTEWEBSTATEMENT Symbol</summary>
        SYMBOL_EXECUTEWEBSTATEMENT,
        /// <summary>EXIT STATEMENT Symbol</summary>
        SYMBOL_EXIT_STATEMENT,
        /// <summary>EXPRLIST Symbol</summary>
        SYMBOL_EXPRLIST,
        /// <summary>EXPRESSION Symbol</summary>
        SYMBOL_EXPRESSION,
        /// <summary>FILE DIALOG STATEMENT Symbol</summary>
        SYMBOL_FILE_DIALOG_STATEMENT,
        /// <summary>FILE PRINT OUT STATEMENT Symbol</summary>
        SYMBOL_FILE_PRINT_OUT_STATEMENT,
        /// <summary>FILECOMMAFILE Symbol</summary>
        SYMBOL_FILECOMMAFILE,
        /// <summary>FILEDATAFORMAT Symbol</summary>
        SYMBOL_FILEDATAFORMAT,
        /// <summary>FILEDIALOGSTRSTATEMENT Symbol</summary>
        SYMBOL_FILEDIALOGSTRSTATEMENT,
        /// <summary>FILEDIALOGVARSTATEMENT Symbol</summary>
        SYMBOL_FILEDIALOGVARSTATEMENT,
        /// <summary>FILENAME Symbol</summary>
        SYMBOL_FILENAME,
        /// <summary>FILEPATH Symbol</summary>
        SYMBOL_FILEPATH,
        /// <summary>FILESPEC2 Symbol</summary>
        SYMBOL_FILESPEC2,
        /// <summary>FILESPECFIELD Symbol</summary>
        SYMBOL_FILESPECFIELD,
        /// <summary>FILESPECFIELDDEF Symbol</summary>
        SYMBOL_FILESPECFIELDDEF,
        /// <summary>FILESPECFIELDLIST Symbol</summary>
        SYMBOL_FILESPECFIELDLIST,
        /// <summary>FILESPECFIELDLISTEND Symbol</summary>
        SYMBOL_FILESPECFIELDLISTEND,
        /// <summary>FILESPECFIELDLITERAL Symbol</summary>
        SYMBOL_FILESPECFIELDLITERAL,
        /// <summary>FILESPECFIELDSINGLE Symbol</summary>
        SYMBOL_FILESPECFIELDSINGLE,
        /// <summary>FILESPECKEY Symbol</summary>
        SYMBOL_FILESPECKEY,
        /// <summary>FILESPECKEYANDFIELD Symbol</summary>
        SYMBOL_FILESPECKEYANDFIELD,
        /// <summary>FILESPECKEYDEF Symbol</summary>
        SYMBOL_FILESPECKEYDEF,
        /// <summary>FILESPECKEYLIST Symbol</summary>
        SYMBOL_FILESPECKEYLIST,
        /// <summary>FILESPECNUMERICKEY Symbol</summary>
        SYMBOL_FILESPECNUMERICKEY,
        /// <summary>FILESPECSTRINGKEY Symbol</summary>
        SYMBOL_FILESPECSTRINGKEY,
        /// <summary>FONTSTYLE Symbol</summary>
        SYMBOL_FONTSTYLE,
        /// <summary>FREQ ALL EXCEPT STATEMENT Symbol</summary>
        SYMBOL_FREQ_ALL_EXCEPT_STATEMENT,
        /// <summary>FREQ ALL STATEMENT Symbol</summary>
        SYMBOL_FREQ_ALL_STATEMENT,
        /// <summary>FREQ COLUMNS STATEMENT Symbol</summary>
        SYMBOL_FREQ_COLUMNS_STATEMENT,
        /// <summary>FREQOPT Symbol</summary>
        SYMBOL_FREQOPT,
        /// <summary>FREQOPTCOLUMNSIZE Symbol</summary>
        SYMBOL_FREQOPTCOLUMNSIZE,
        /// <summary>FREQOPTNOWRAP Symbol</summary>
        SYMBOL_FREQOPTNOWRAP,
        /// <summary>FREQOPTPSUVAR Symbol</summary>
        SYMBOL_FREQOPTPSUVAR,
        /// <summary>FREQOPTS Symbol</summary>
        SYMBOL_FREQOPTS,
        /// <summary>FREQOPTSTRATA Symbol</summary>
        SYMBOL_FREQOPTSTRATA,
        /// <summary>FROMFILETOFILE Symbol</summary>
        SYMBOL_FROMFILETOFILE,
        /// <summary>FULLY QUALIFIED ID Symbol</summary>
        SYMBOL_FULLY_QUALIFIED_ID,
        /// <summary>FUNCNAME1 Symbol</summary>
        SYMBOL_FUNCNAME1,
        /// <summary>FUNCNAME2 Symbol</summary>
        SYMBOL_FUNCNAME2,
        /// <summary>FUNCTIONCALL Symbol</summary>
        SYMBOL_FUNCTIONCALL,
        /// <summary>FUNCTIONPARAMETERLIST Symbol</summary>
        SYMBOL_FUNCTIONPARAMETERLIST,
        /// <summary>GENERIC STRING Symbol</summary>
        SYMBOL_GENERIC_STRING,
        /// <summary>GENERICOPT Symbol</summary>
        SYMBOL_GENERICOPT,
        /// <summary>GET PATH STATEMENT Symbol</summary>
        SYMBOL_GET_PATH_STATEMENT,
        /// <summary>GETPATHSTRSTATEMENT Symbol</summary>
        SYMBOL_GETPATHSTRSTATEMENT,
        /// <summary>GETPATHVARSTATEMENT Symbol</summary>
        SYMBOL_GETPATHVARSTATEMENT,
        /// <summary>GO TO PAGE STATEMENT Symbol</summary>
        SYMBOL_GO_TO_PAGE_STATEMENT,
        /// <summary>GO TO VARIABLE STATEMENT Symbol</summary>
        SYMBOL_GO_TO_VARIABLE_STATEMENT,
        /// <summary>GRAPH GENERIC OPT STATEMENT Symbol</summary>
        SYMBOL_GRAPH_GENERIC_OPT_STATEMENT,
        /// <summary>GRAPH OPT 1 STATEMENT Symbol</summary>
        SYMBOL_GRAPH_OPT_1_STATEMENT,
        /// <summary>GRAPH OPT 2 STATEMENT Symbol</summary>
        SYMBOL_GRAPH_OPT_2_STATEMENT,
        /// <summary>GRAPH OPT 3 STATEMENT Symbol</summary>
        SYMBOL_GRAPH_OPT_3_STATEMENT,
        /// <summary>GRAPH OPT 4 STATEMENT Symbol</summary>
        SYMBOL_GRAPH_OPT_4_STATEMENT,
        /// <summary>GRAPH OPT 5 STATEMENT Symbol</summary>
        SYMBOL_GRAPH_OPT_5_STATEMENT,
        /// <summary>HEADER TITLE FONT STATEMENT Symbol</summary>
        SYMBOL_HEADER_TITLE_FONT_STATEMENT,
        /// <summary>HEADER TITLE STRING AND FONT STATEMENT Symbol</summary>
        SYMBOL_HEADER_TITLE_STRING_AND_FONT_STATEMENT,
        /// <summary>HEADER TITLE STRING STATEMENT Symbol</summary>
        SYMBOL_HEADER_TITLE_STRING_STATEMENT,
        /// <summary>HELP FILE STATEMENT Symbol</summary>
        SYMBOL_HELP_FILE_STATEMENT,
        /// <summary>HEX NUMBER Symbol</summary>
        SYMBOL_HEX_NUMBER,
        /// <summary>HIDE EXCEPT STATEMENT Symbol</summary>
        SYMBOL_HIDE_EXCEPT_STATEMENT,
        /// <summary>HIDE SOME STATEMENT Symbol</summary>
        SYMBOL_HIDE_SOME_STATEMENT,
        /// <summary>IDENTIFIERLIST Symbol</summary>
        SYMBOL_IDENTIFIERLIST,
        /// <summary>IDENTIFIEROPT Symbol</summary>
        SYMBOL_IDENTIFIEROPT,
        /// <summary>IF ELSE STATEMENT Symbol</summary>
        SYMBOL_IF_ELSE_STATEMENT,
        /// <summary>IF STATEMENT Symbol</summary>
        SYMBOL_IF_STATEMENT,
        /// <summary>INTERVALOPT Symbol</summary>
        SYMBOL_INTERVALOPT,
        /// <summary>JOINOPT Symbol</summary>
        SYMBOL_JOINOPT,
        /// <summary>KEYDEF Symbol</summary>
        SYMBOL_KEYDEF,
        /// <summary>KEYEXPR Symbol</summary>
        SYMBOL_KEYEXPR,
        /// <summary>KEYEXPRIDENTIFIER Symbol</summary>
        SYMBOL_KEYEXPRIDENTIFIER,
        /// <summary>KEYEXPRSIMPLE Symbol</summary>
        SYMBOL_KEYEXPRSIMPLE,
        /// <summary>KEYVARLIST Symbol</summary>
        SYMBOL_KEYVARLIST,
        /// <summary>KM SURVIVAL BOOLEAN STATEMENT Symbol</summary>
        SYMBOL_KM_SURVIVAL_BOOLEAN_STATEMENT,
        /// <summary>KMSURVGRAPHOPT Symbol</summary>
        SYMBOL_KMSURVGRAPHOPT,
        /// <summary>KMSURVOPT Symbol</summary>
        SYMBOL_KMSURVOPT,
        /// <summary>KMSURVOPTS Symbol</summary>
        SYMBOL_KMSURVOPTS,
        /// <summary>KMSURVOUTOPT Symbol</summary>
        SYMBOL_KMSURVOUTOPT,
        /// <summary>KMSURVTIMEOPT Symbol</summary>
        SYMBOL_KMSURVTIMEOPT,
        /// <summary>KMSURVTITLEOPT Symbol</summary>
        SYMBOL_KMSURVTITLEOPT,
        /// <summary>KMSURVWEIGHTOPT Symbol</summary>
        SYMBOL_KMSURVWEIGHTOPT,
        /// <summary>LET STATEMENT Symbol</summary>
        SYMBOL_LET_STATEMENT,
        /// <summary>LINK REMOVE STATEMENT Symbol</summary>
        SYMBOL_LINK_REMOVE_STATEMENT,
        /// <summary>LINK STATEMENT Symbol</summary>
        SYMBOL_LINK_STATEMENT,
        /// <summary>LINKNAME2 Symbol</summary>
        SYMBOL_LINKNAME2,
        /// <summary>LINKREMOVEALLSTATEMENT Symbol</summary>
        SYMBOL_LINKREMOVEALLSTATEMENT,
        /// <summary>LINKREMOVESTRINGSTATEMENT Symbol</summary>
        SYMBOL_LINKREMOVESTRINGSTATEMENT,
        /// <summary>LIST2 Symbol</summary>
        SYMBOL_LIST2,
        /// <summary>LIST ALL EXCEPT STATEMENT Symbol</summary>
        SYMBOL_LIST_ALL_EXCEPT_STATEMENT,
        /// <summary>LIST ALL STATEMENT Symbol</summary>
        SYMBOL_LIST_ALL_STATEMENT,
        /// <summary>LIST COLUMNS STATEMENT Symbol</summary>
        SYMBOL_LIST_COLUMNS_STATEMENT,
        /// <summary>LISTGRIDOPT Symbol</summary>
        SYMBOL_LISTGRIDOPT,
        /// <summary>LISTHTMLOPT Symbol</summary>
        SYMBOL_LISTHTMLOPT,
        /// <summary>LISTHTMLOPTLINE Symbol</summary>
        SYMBOL_LISTHTMLOPTLINE,
        /// <summary>LISTHTMLOPTNOIMAGE Symbol</summary>
        SYMBOL_LISTHTMLOPTNOIMAGE,
        /// <summary>LISTHTMLOPTNOWRAP Symbol</summary>
        SYMBOL_LISTHTMLOPTNOWRAP,
        /// <summary>LISTHTMLOPTONECOLUMN Symbol</summary>
        SYMBOL_LISTHTMLOPTONECOLUMN,
        /// <summary>LISTHTMLOPTTHREECOLUMNS Symbol</summary>
        SYMBOL_LISTHTMLOPTTHREECOLUMNS,
        /// <summary>LISTHTMLOPTTWOCOLUMNS Symbol</summary>
        SYMBOL_LISTHTMLOPTTWOCOLUMNS,
        /// <summary>LISTOPT Symbol</summary>
        SYMBOL_LISTOPT,
        /// <summary>LISTUPDATEOPT Symbol</summary>
        SYMBOL_LISTUPDATEOPT,
        /// <summary>LITERAL2 Symbol</summary>
        SYMBOL_LITERAL2,
        /// <summary>LITERAL CHAR Symbol</summary>
        SYMBOL_LITERAL_CHAR,
        /// <summary>LITERAL DATE Symbol</summary>
        SYMBOL_LITERAL_DATE,
        /// <summary>LITERAL DATE TIME Symbol</summary>
        SYMBOL_LITERAL_DATE_TIME,
        /// <summary>LITERAL STRING Symbol</summary>
        SYMBOL_LITERAL_STRING,
        /// <summary>LITERAL TIME Symbol</summary>
        SYMBOL_LITERAL_TIME,
        /// <summary>LITERALINC Symbol</summary>
        SYMBOL_LITERALINC,
        /// <summary>LOGISTIC STATEMENT Symbol</summary>
        SYMBOL_LOGISTIC_STATEMENT,
        /// <summary>LOGISTICMATCHOPT Symbol</summary>
        SYMBOL_LOGISTICMATCHOPT,
        /// <summary>LOGISTICNOINTERCEPTOPT Symbol</summary>
        SYMBOL_LOGISTICNOINTERCEPTOPT,
        /// <summary>LOGISTICOPT Symbol</summary>
        SYMBOL_LOGISTICOPT,
        /// <summary>LOGISTICOPTS Symbol</summary>
        SYMBOL_LOGISTICOPTS,
        /// <summary>LOGISTICPVALPERCENTOPT Symbol</summary>
        SYMBOL_LOGISTICPVALPERCENTOPT,
        /// <summary>LOGISTICPVALREALOPT Symbol</summary>
        SYMBOL_LOGISTICPVALREALOPT,
        /// <summary>LOGISTICTITLEOPT Symbol</summary>
        SYMBOL_LOGISTICTITLEOPT,
        /// <summary>MAP AVG STATEMENT Symbol</summary>
        SYMBOL_MAP_AVG_STATEMENT,
        /// <summary>MAP CASE STATEMENT Symbol</summary>
        SYMBOL_MAP_CASE_STATEMENT,
        /// <summary>MAP COUNT STATEMENT Symbol</summary>
        SYMBOL_MAP_COUNT_STATEMENT,
        /// <summary>MAP MAX STATEMENT Symbol</summary>
        SYMBOL_MAP_MAX_STATEMENT,
        /// <summary>MAP MIN STATEMENT Symbol</summary>
        SYMBOL_MAP_MIN_STATEMENT,
        /// <summary>MAP OPT 1 STATEMENT Symbol</summary>
        SYMBOL_MAP_OPT_1_STATEMENT,
        /// <summary>MAP OPT 2 STATEMENT Symbol</summary>
        SYMBOL_MAP_OPT_2_STATEMENT,
        /// <summary>MAP OPT 3 STATEMENT Symbol</summary>
        SYMBOL_MAP_OPT_3_STATEMENT,
        /// <summary>MAP OPT 4 STATEMENT Symbol</summary>
        SYMBOL_MAP_OPT_4_STATEMENT,
        /// <summary>MAP OPT 5 STATEMENT Symbol</summary>
        SYMBOL_MAP_OPT_5_STATEMENT,
        /// <summary>MAP SUM STATEMENT Symbol</summary>
        SYMBOL_MAP_SUM_STATEMENT,
        /// <summary>MASKOPT Symbol</summary>
        SYMBOL_MASKOPT,
        /// <summary>MATCH COLUMN ALL STATEMENT Symbol</summary>
        SYMBOL_MATCH_COLUMN_ALL_STATEMENT,
        /// <summary>MATCH COLUMN EXCEPT STATEMENT Symbol</summary>
        SYMBOL_MATCH_COLUMN_EXCEPT_STATEMENT,
        /// <summary>MATCH ROW ALL STATEMENT Symbol</summary>
        SYMBOL_MATCH_ROW_ALL_STATEMENT,
        /// <summary>MATCH ROW COLUMN STATEMENT Symbol</summary>
        SYMBOL_MATCH_ROW_COLUMN_STATEMENT,
        /// <summary>MATCH ROW EXCEPT STATEMENT Symbol</summary>
        SYMBOL_MATCH_ROW_EXCEPT_STATEMENT,
        /// <summary>MATCHOPT Symbol</summary>
        SYMBOL_MATCHOPT,
        /// <summary>MATCHOPTS Symbol</summary>
        SYMBOL_MATCHOPTS,
        /// <summary>MENU BLOCK Symbol</summary>
        SYMBOL_MENU_BLOCK,
        /// <summary>MENU COMMAND STATEMENT Symbol</summary>
        SYMBOL_MENU_COMMAND_STATEMENT,
        /// <summary>MENU DIALOG STATEMENT Symbol</summary>
        SYMBOL_MENU_DIALOG_STATEMENT,
        /// <summary>MENU EMPTY BLOCK Symbol</summary>
        SYMBOL_MENU_EMPTY_BLOCK,
        /// <summary>MENU EXECUTE STATEMENT Symbol</summary>
        SYMBOL_MENU_EXECUTE_STATEMENT,
        /// <summary>MENU ITEM BLOCK NAME STATEMENT Symbol</summary>
        SYMBOL_MENU_ITEM_BLOCK_NAME_STATEMENT,
        /// <summary>MENU ITEM SEPARATOR STATEMENT Symbol</summary>
        SYMBOL_MENU_ITEM_SEPARATOR_STATEMENT,
        /// <summary>MENU REPLACE STATEMENT Symbol</summary>
        SYMBOL_MENU_REPLACE_STATEMENT,
        /// <summary>MENU SINGLE STATEMENT Symbol</summary>
        SYMBOL_MENU_SINGLE_STATEMENT,
        /// <summary>MENU STATEMENT Symbol</summary>
        SYMBOL_MENU_STATEMENT,
        /// <summary>MENUCOMMANDSTATEMENT1 Symbol</summary>
        SYMBOL_MENUCOMMANDSTATEMENT1,
        /// <summary>MENUCOMMANDSTATEMENT2 Symbol</summary>
        SYMBOL_MENUCOMMANDSTATEMENT2,
        /// <summary>MENUCOMMANDSTATEMENT3 Symbol</summary>
        SYMBOL_MENUCOMMANDSTATEMENT3,
        /// <summary>MENUCOMMANDSTATEMENT4 Symbol</summary>
        SYMBOL_MENUCOMMANDSTATEMENT4,
        /// <summary>MERGE DB TABLE STATEMENT Symbol</summary>
        SYMBOL_MERGE_DB_TABLE_STATEMENT,
        /// <summary>MERGE EXCEL FILE STATEMENT Symbol</summary>
        SYMBOL_MERGE_EXCEL_FILE_STATEMENT,
        /// <summary>MERGE FILE STATEMENT Symbol</summary>
        SYMBOL_MERGE_FILE_STATEMENT,
        /// <summary>MERGE TABLE STATEMENT Symbol</summary>
        SYMBOL_MERGE_TABLE_STATEMENT,
        /// <summary>MERGEOPT Symbol</summary>
        SYMBOL_MERGEOPT,
        /// <summary>MOVE BUTTONS STATEMENT Symbol</summary>
        SYMBOL_MOVE_BUTTONS_STATEMENT,
        /// <summary>MULTEXP Symbol</summary>
        SYMBOL_MULTEXP,
        /// <summary>MULTIPLE CHOICE DIALOG STATEMENT Symbol</summary>
        SYMBOL_MULTIPLE_CHOICE_DIALOG_STATEMENT,
        /// <summary>MULTIPLEFUNCTIONPARAMETERLIST Symbol</summary>
        SYMBOL_MULTIPLEFUNCTIONPARAMETERLIST,
        /// <summary>NEGATEEXP Symbol</summary>
        SYMBOL_NEGATEEXP,
        /// <summary>NEW PAGE STATEMENT Symbol</summary>
        SYMBOL_NEW_PAGE_STATEMENT,
        /// <summary>NEW RECORD STATEMENT Symbol</summary>
        SYMBOL_NEW_RECORD_STATEMENT,
        /// <summary>NONEMPTYFUNCTIONPARAMETERLIST Symbol</summary>
        SYMBOL_NONEMPTYFUNCTIONPARAMETERLIST,
        /// <summary>NOTEXP Symbol</summary>
        SYMBOL_NOTEXP,
        /// <summary>NUMBER Symbol</summary>
        SYMBOL_NUMBER,
        /// <summary>NUMERIC DIALOG EXPLICIT STATEMENT Symbol</summary>
        SYMBOL_NUMERIC_DIALOG_EXPLICIT_STATEMENT,
        /// <summary>NUMERIC DIALOG IMPLICIT STATEMENT Symbol</summary>
        SYMBOL_NUMERIC_DIALOG_IMPLICIT_STATEMENT,
        /// <summary>OFFSET Symbol</summary>
        SYMBOL_OFFSET,
        /// <summary>ON BROWSER EXIT BLOCK Symbol</summary>
        SYMBOL_ON_BROWSER_EXIT_BLOCK,
        /// <summary>ON BROWSER EXIT EMPTY BLOCK Symbol</summary>
        SYMBOL_ON_BROWSER_EXIT_EMPTY_BLOCK,
        /// <summary>ONOFF Symbol</summary>
        SYMBOL_ONOFF,
        /// <summary>OUTTABLEOPT Symbol</summary>
        SYMBOL_OUTTABLEOPT,
        /// <summary>OUTTARGET Symbol</summary>
        SYMBOL_OUTTARGET,
        /// <summary>PATHSYMBOL Symbol</summary>
        SYMBOL_PATHSYMBOL,
        /// <summary>PICTURE SIZE STATEMENT Symbol</summary>
        SYMBOL_PICTURE_SIZE_STATEMENT,
        /// <summary>PICTURE STATEMENT Symbol</summary>
        SYMBOL_PICTURE_STATEMENT,
        /// <summary>PICTUREFILESTATEMENT Symbol</summary>
        SYMBOL_PICTUREFILESTATEMENT,
        /// <summary>PICTUREOFFSETSTATEMENT Symbol</summary>
        SYMBOL_PICTUREOFFSETSTATEMENT,
        /// <summary>PICTUREOFFSTATEMENT Symbol</summary>
        SYMBOL_PICTUREOFFSTATEMENT,
        /// <summary>PICTUREONSTATEMENT Symbol</summary>
        SYMBOL_PICTUREONSTATEMENT,
        /// <summary>POPUP BLOCK Symbol</summary>
        SYMBOL_POPUP_BLOCK,
        /// <summary>POPUP EMPTY BLOCK Symbol</summary>
        SYMBOL_POPUP_EMPTY_BLOCK,
        /// <summary>POPUP SINGLE STATEMENT Symbol</summary>
        SYMBOL_POPUP_SINGLE_STATEMENT,
        /// <summary>POPUP STATEMENT Symbol</summary>
        SYMBOL_POPUP_STATEMENT,
        /// <summary>POWEXP Symbol</summary>
        SYMBOL_POWEXP,
        /// <summary>PROCESSOPTION Symbol</summary>
        SYMBOL_PROCESSOPTION,
        /// <summary>QUALIFIEDID Symbol</summary>
        SYMBOL_QUALIFIEDID,
        /// <summary>QUIT STATEMENT Symbol</summary>
        SYMBOL_QUIT_STATEMENT,
        /// <summary>READ DB TABLE STATEMENT Symbol</summary>
        SYMBOL_READ_DB_TABLE_STATEMENT,
        /// <summary>READ EPI FILE SPEC STATEMENT Symbol</summary>
        SYMBOL_READ_EPI_FILE_SPEC_STATEMENT,
        /// <summary>READ EPI STATEMENT Symbol</summary>
        SYMBOL_READ_EPI_STATEMENT,
        /// <summary>READ EXCEL FILE STATEMENT Symbol</summary>
        SYMBOL_READ_EXCEL_FILE_STATEMENT,
        /// <summary>READ SQL STATEMENT Symbol</summary>
        SYMBOL_READ_SQL_STATEMENT,
        /// <summary>READOPT Symbol</summary>
        SYMBOL_READOPT,
        /// <summary>REAL NUMBER Symbol</summary>
        SYMBOL_REAL_NUMBER,
        /// <summary>RECODE2 Symbol</summary>
        SYMBOL_RECODE2,
        /// <summary>RECODE A Symbol</summary>
        SYMBOL_RECODE_A,
        /// <summary>RECODE B Symbol</summary>
        SYMBOL_RECODE_B,
        /// <summary>RECODE C Symbol</summary>
        SYMBOL_RECODE_C,
        /// <summary>RECODE D Symbol</summary>
        SYMBOL_RECODE_D,
        /// <summary>RECODE E Symbol</summary>
        SYMBOL_RECODE_E,
        /// <summary>RECODE F Symbol</summary>
        SYMBOL_RECODE_F,
        /// <summary>RECODE G Symbol</summary>
        SYMBOL_RECODE_G,
        /// <summary>RECODE H Symbol</summary>
        SYMBOL_RECODE_H,
        /// <summary>RECODE I Symbol</summary>
        SYMBOL_RECODE_I,
        /// <summary>RECODE J Symbol</summary>
        SYMBOL_RECODE_J,
        /// <summary>RECODE K Symbol</summary>
        SYMBOL_RECODE_K,
        /// <summary>RECODE L Symbol</summary>
        SYMBOL_RECODE_L,
        /// <summary>RECODE M Symbol</summary>
        SYMBOL_RECODE_M,
        /// <summary>RECODE N Symbol</summary>
        SYMBOL_RECODE_N,
        /// <summary>RECODE STATEMENT Symbol</summary>
        SYMBOL_RECODE_STATEMENT,
        /// <summary>RECODELIST Symbol</summary>
        SYMBOL_RECODELIST,
        /// <summary>REGRESS STATEMENT Symbol</summary>
        SYMBOL_REGRESS_STATEMENT,
        /// <summary>RELATE DB TABLE STATEMENT Symbol</summary>
        SYMBOL_RELATE_DB_TABLE_STATEMENT,
        /// <summary>RELATE DB TABLE WITH IDENTIFIER STATEMENT Symbol</summary>
        SYMBOL_RELATE_DB_TABLE_WITH_IDENTIFIER_STATEMENT,
        /// <summary>RELATE EPI TABLE STATEMENT Symbol</summary>
        SYMBOL_RELATE_EPI_TABLE_STATEMENT,
        /// <summary>RELATE EXCEL FILE STATEMENT Symbol</summary>
        SYMBOL_RELATE_EXCEL_FILE_STATEMENT,
        /// <summary>RELATE FILE STATEMENT Symbol</summary>
        SYMBOL_RELATE_FILE_STATEMENT,
        /// <summary>RELATE TABLE STATEMENT Symbol</summary>
        SYMBOL_RELATE_TABLE_STATEMENT,
        /// <summary>REPEAT STATEMENT Symbol</summary>
        SYMBOL_REPEAT_STATEMENT,
        /// <summary>REPLACE ROUTEOUT STATEMENT Symbol</summary>
        SYMBOL_REPLACE_ROUTEOUT_STATEMENT,
        /// <summary>REPLACEWITHCOMMASTATEMENT Symbol</summary>
        SYMBOL_REPLACEWITHCOMMASTATEMENT,
        /// <summary>REPLACEWITHCOMMATIMESSTATEMENT Symbol</summary>
        SYMBOL_REPLACEWITHCOMMATIMESSTATEMENT,
        /// <summary>REPLACEWITHFROMTOSTATEMENT Symbol</summary>
        SYMBOL_REPLACEWITHFROMTOSTATEMENT,
        /// <summary>REPLACEWITHFROMTOTIMESSTATEMENT Symbol</summary>
        SYMBOL_REPLACEWITHFROMTOTIMESSTATEMENT,
        /// <summary>ROW ALL COLUMN MEANS STATEMENT Symbol</summary>
        SYMBOL_ROW_ALL_COLUMN_MEANS_STATEMENT,
        /// <summary>ROW ALL MEANS STATEMENT Symbol</summary>
        SYMBOL_ROW_ALL_MEANS_STATEMENT,
        /// <summary>ROW ALL TABLES STATEMENT Symbol</summary>
        SYMBOL_ROW_ALL_TABLES_STATEMENT,
        /// <summary>ROW COLUMN ALL MEANS STATEMENT Symbol</summary>
        SYMBOL_ROW_COLUMN_ALL_MEANS_STATEMENT,
        /// <summary>ROW COLUMN MEANS STATEMENT Symbol</summary>
        SYMBOL_ROW_COLUMN_MEANS_STATEMENT,
        /// <summary>ROW COLUMN TABLES STATEMENT Symbol</summary>
        SYMBOL_ROW_COLUMN_TABLES_STATEMENT,
        /// <summary>ROW EXCEPT TABLES STATEMENT Symbol</summary>
        SYMBOL_ROW_EXCEPT_TABLES_STATEMENT,
        /// <summary>RUN FILE PGM STATEMENT Symbol</summary>
        SYMBOL_RUN_FILE_PGM_STATEMENT,
        /// <summary>RUN PGM IN DB STATEMENT Symbol</summary>
        SYMBOL_RUN_PGM_IN_DB_STATEMENT,
        /// <summary>RUN STRING STATEMENT Symbol</summary>
        SYMBOL_RUN_STRING_STATEMENT,
        /// <summary>SCREEN TEXT STATEMENT Symbol</summary>
        SYMBOL_SCREEN_TEXT_STATEMENT,
        /// <summary>SELECT STATEMENT Symbol</summary>
        SYMBOL_SELECT_STATEMENT,
        /// <summary>SET BUTTONS STATEMENT Symbol</summary>
        SYMBOL_SET_BUTTONS_STATEMENT,
        /// <summary>SET DB VERSION STATEMENT Symbol</summary>
        SYMBOL_SET_DB_VERSION_STATEMENT,
        /// <summary>SET DOS WIN STATEMENT Symbol</summary>
        SYMBOL_SET_DOS_WIN_STATEMENT,
        /// <summary>SET IMPORT YEAR STATEMENT Symbol</summary>
        SYMBOL_SET_IMPORT_YEAR_STATEMENT,
        /// <summary>SET INI DIR 1 STATEMENT Symbol</summary>
        SYMBOL_SET_INI_DIR_1_STATEMENT,
        /// <summary>SET INI DIR 2 STATEMENT Symbol</summary>
        SYMBOL_SET_INI_DIR_2_STATEMENT,
        /// <summary>SET INI DIR STATEMENT Symbol</summary>
        SYMBOL_SET_INI_DIR_STATEMENT,
        /// <summary>SET LANGUAGE STATEMENT Symbol</summary>
        SYMBOL_SET_LANGUAGE_STATEMENT,
        /// <summary>SET PICTURE STATEMENT Symbol</summary>
        SYMBOL_SET_PICTURE_STATEMENT,
        /// <summary>SET STATEMENT Symbol</summary>
        SYMBOL_SET_STATEMENT,
        /// <summary>SET WORK DIR STATEMENT Symbol</summary>
        SYMBOL_SET_WORK_DIR_STATEMENT,
        /// <summary>SETCLAUSE Symbol</summary>
        SYMBOL_SETCLAUSE,
        /// <summary>SETLIST Symbol</summary>
        SYMBOL_SETLIST,
        /// <summary>SETWORKDIR1STATEMENT Symbol</summary>
        SYMBOL_SETWORKDIR1STATEMENT,
        /// <summary>SETWORKDIR2STATEMENT Symbol</summary>
        SYMBOL_SETWORKDIR2STATEMENT,
        /// <summary>SHUTDOWN BLOCK Symbol</summary>
        SYMBOL_SHUTDOWN_BLOCK,
        /// <summary>SHUTDOWN EMPTY BLOCK Symbol</summary>
        SYMBOL_SHUTDOWN_EMPTY_BLOCK,
        /// <summary>SIMPLE ASSIGN STATEMENT Symbol</summary>
        SYMBOL_SIMPLE_ASSIGN_STATEMENT,
        /// <summary>SIMPLE CMD STATEMENT Symbol</summary>
        SYMBOL_SIMPLE_CMD_STATEMENT,
        /// <summary>SIMPLE DIALOG STATEMENT Symbol</summary>
        SYMBOL_SIMPLE_DIALOG_STATEMENT,
        /// <summary>SIMPLE EXECUTE STATEMENT Symbol</summary>
        SYMBOL_SIMPLE_EXECUTE_STATEMENT,
        /// <summary>SIMPLE GRAPH STATEMENT Symbol</summary>
        SYMBOL_SIMPLE_GRAPH_STATEMENT,
        /// <summary>SIMPLE HELP STATEMENT Symbol</summary>
        SYMBOL_SIMPLE_HELP_STATEMENT,
        /// <summary>SIMPLE KM SURVIVAL STATEMENT Symbol</summary>
        SYMBOL_SIMPLE_KM_SURVIVAL_STATEMENT,
        /// <summary>SIMPLE LIST STATEMENT Symbol</summary>
        SYMBOL_SIMPLE_LIST_STATEMENT,
        /// <summary>SIMPLE MEANS STATEMENT Symbol</summary>
        SYMBOL_SIMPLE_MEANS_STATEMENT,
        /// <summary>SIMPLE PRINT OUT STATEMENT Symbol</summary>
        SYMBOL_SIMPLE_PRINT_OUT_STATEMENT,
        /// <summary>SIMPLE READ STATEMENT Symbol</summary>
        SYMBOL_SIMPLE_READ_STATEMENT,
        /// <summary>SIMPLE ROUTEOUT STATEMENT Symbol</summary>
        SYMBOL_SIMPLE_ROUTEOUT_STATEMENT,
        /// <summary>SIMPLE RUN STATEMENT Symbol</summary>
        SYMBOL_SIMPLE_RUN_STATEMENT,
        /// <summary>SIMPLE TABLES STATEMENT Symbol</summary>
        SYMBOL_SIMPLE_TABLES_STATEMENT,
        /// <summary>SIMPLE UNDEFINE STATEMENT Symbol</summary>
        SYMBOL_SIMPLE_UNDEFINE_STATEMENT,
        /// <summary>SIMPLEFILE Symbol</summary>
        SYMBOL_SIMPLEFILE,
        /// <summary>SIMPLELINKSTATEMENT Symbol</summary>
        SYMBOL_SIMPLELINKSTATEMENT,
        /// <summary>SIMPLETITLEOPT Symbol</summary>
        SYMBOL_SIMPLETITLEOPT,
        /// <summary>SINGLEFUNCTIONPARAMETERLIST Symbol</summary>
        SYMBOL_SINGLEFUNCTIONPARAMETERLIST,
        /// <summary>SORT2 Symbol</summary>
        SYMBOL_SORT2,
        /// <summary>SORT STATEMENT Symbol</summary>
        SYMBOL_SORT_STATEMENT,
        /// <summary>SORTLIST Symbol</summary>
        SYMBOL_SORTLIST,
        /// <summary>SORTOPT Symbol</summary>
        SYMBOL_SORTOPT,
        /// <summary>STARTUP BLOCK Symbol</summary>
        SYMBOL_STARTUP_BLOCK,
        /// <summary>STARTUP EMPTY BLOCK Symbol</summary>
        SYMBOL_STARTUP_EMPTY_BLOCK,
        /// <summary>STATEMENT Symbol</summary>
        SYMBOL_STATEMENT,
        /// <summary>STATEMENTS Symbol</summary>
        SYMBOL_STATEMENTS,
        /// <summary>STATISTICSOPTION Symbol</summary>
        SYMBOL_STATISTICSOPTION,
        /// <summary>STRA WEI VAR GRAPH STATEMENT Symbol</summary>
        SYMBOL_STRA_WEI_VAR_GRAPH_STATEMENT,
        /// <summary>STRATA VAR GRAPH STATEMENT Symbol</summary>
        SYMBOL_STRATA_VAR_GRAPH_STATEMENT,
        /// <summary>STRINGLIST Symbol</summary>
        SYMBOL_STRINGLIST,
        /// <summary>STRINGPAIR Symbol</summary>
        SYMBOL_STRINGPAIR,
        /// <summary>SUMMARIZE STATEMENT Symbol</summary>
        SYMBOL_SUMMARIZE_STATEMENT,
        /// <summary>SUMMARIZEOPT Symbol</summary>
        SYMBOL_SUMMARIZEOPT,
        /// <summary>SYS INFO STATEMENT Symbol</summary>
        SYMBOL_SYS_INFO_STATEMENT,
        /// <summary>TABLES DISPLAY STATEMENT Symbol</summary>
        SYMBOL_TABLES_DISPLAY_STATEMENT,
        /// <summary>TABLES ONE VARIABLE STATEMENT Symbol</summary>
        SYMBOL_TABLES_ONE_VARIABLE_STATEMENT,
        /// <summary>TEMPLATEOPT Symbol</summary>
        SYMBOL_TEMPLATEOPT,
        /// <summary>TERM Symbol</summary>
        SYMBOL_TERM,
        /// <summary>TERMLIST Symbol</summary>
        SYMBOL_TERMLIST,
        /// <summary>TEXTBOX DIALOG STATEMENT Symbol</summary>
        SYMBOL_TEXTBOX_DIALOG_STATEMENT,
        /// <summary>TITLEFONTPART Symbol</summary>
        SYMBOL_TITLEFONTPART,
        /// <summary>TITLEOPT Symbol</summary>
        SYMBOL_TITLEOPT,
        /// <summary>TITLESTRINGPART Symbol</summary>
        SYMBOL_TITLESTRINGPART,
        /// <summary>TITLESTRINGPARTWITHAPPEND Symbol</summary>
        SYMBOL_TITLESTRINGPARTWITHAPPEND,
        /// <summary>TYPE OUT FILE STATEMENT Symbol</summary>
        SYMBOL_TYPE_OUT_FILE_STATEMENT,
        /// <summary>TYPE OUT FILE WITH FONT STATEMENT Symbol</summary>
        SYMBOL_TYPE_OUT_FILE_WITH_FONT_STATEMENT,
        /// <summary>TYPE OUT STRING STATEMENT Symbol</summary>
        SYMBOL_TYPE_OUT_STRING_STATEMENT,
        /// <summary>TYPE OUT STRING WITH FONT STATEMENT Symbol</summary>
        SYMBOL_TYPE_OUT_STRING_WITH_FONT_STATEMENT,
        /// <summary>UNDEFEXPRESSION Symbol</summary>
        SYMBOL_UNDEFEXPRESSION,
        /// <summary>UNDELETE ALL STATEMENT Symbol</summary>
        SYMBOL_UNDELETE_ALL_STATEMENT,
        /// <summary>UNDELETE OPTION Symbol</summary>
        SYMBOL_UNDELETE_OPTION,
        /// <summary>UNDELETE SELECTED STATEMENT Symbol</summary>
        SYMBOL_UNDELETE_SELECTED_STATEMENT,
        /// <summary>UNHIDE EXCEPT STATEMENT Symbol</summary>
        SYMBOL_UNHIDE_EXCEPT_STATEMENT,
        /// <summary>UNHIDE SOME STATEMENT Symbol</summary>
        SYMBOL_UNHIDE_SOME_STATEMENT,
        /// <summary>VALUE Symbol</summary>
        SYMBOL_VALUE,
        /// <summary>VAR TYPE DATE Symbol</summary>
        SYMBOL_VAR_TYPE_DATE,
        /// <summary>VAR TYPE NUMERIC Symbol</summary>
        SYMBOL_VAR_TYPE_NUMERIC,
        /// <summary>VAR TYPE TEXT INPUT Symbol</summary>
        SYMBOL_VAR_TYPE_TEXT_INPUT,
        /// <summary>VAR TYPE YN Symbol</summary>
        SYMBOL_VAR_TYPE_YN,
        /// <summary>VARIABLE SCOPE Symbol</summary>
        SYMBOL_VARIABLE_SCOPE,
        /// <summary>VARIABLES DISPLAY STATEMENT Symbol</summary>
        SYMBOL_VARIABLES_DISPLAY_STATEMENT,
        /// <summary>VARIABLETYPEINDICATOR Symbol</summary>
        SYMBOL_VARIABLETYPEINDICATOR,
        /// <summary>VIEWS DISPLAY STATEMENT Symbol</summary>
        SYMBOL_VIEWS_DISPLAY_STATEMENT,
        /// <summary>WAIT FOR EXIT STATEMENT Symbol</summary>
        SYMBOL_WAIT_FOR_EXIT_STATEMENT,
        /// <summary>WAIT FOR FILE EXISTS STATEMENT Symbol</summary>
        SYMBOL_WAIT_FOR_FILE_EXISTS_STATEMENT,
        /// <summary>WAIT FOR STATEMENT Symbol</summary>
        SYMBOL_WAIT_FOR_STATEMENT,
        /// <summary>WAITFORFILEEXISTSSTATEMENT1 Symbol</summary>
        SYMBOL_WAITFORFILEEXISTSSTATEMENT1,
        /// <summary>WAITFORFILEEXISTSSTATEMENT2 Symbol</summary>
        SYMBOL_WAITFORFILEEXISTSSTATEMENT2,
        /// <summary>WEBFILE Symbol</summary>
        SYMBOL_WEBFILE,
        /// <summary>WEBHEADER Symbol</summary>
        SYMBOL_WEBHEADER,
        /// <summary>WEBLINK Symbol</summary>
        SYMBOL_WEBLINK,
        /// <summary>WEBPATH Symbol</summary>
        SYMBOL_WEBPATH,
        /// <summary>WEIGHT VAR GRAPH STATEMENT Symbol</summary>
        SYMBOL_WEIGHT_VAR_GRAPH_STATEMENT,
        /// <summary>WEIGHTOPT Symbol</summary>
        SYMBOL_WEIGHTOPT,
        /// <summary>WORDS Symbol</summary>
        SYMBOL_WORDS,
        /// <summary>WRITE ALL STATEMENT Symbol</summary>
        SYMBOL_WRITE_ALL_STATEMENT,
        /// <summary>WRITE EXCEPT STATEMENT Symbol</summary>
        SYMBOL_WRITE_EXCEPT_STATEMENT,
        /// <summary>WRITE SOME STATEMENT Symbol</summary>
        SYMBOL_WRITE_SOME_STATEMENT,
        /// <summary>WRITEMODE Symbol</summary>
        SYMBOL_WRITEMODE,
        /// <summary>XYTITLEOPT Symbol</summary>
        SYMBOL_XYTITLEOPT,
        /// <summary>YN DIALOG STATEMENT Symbol</summary>
        SYMBOL_YN_DIALOG_STATEMENT,
    }

    /// <summary>
    /// Rules
    /// </summary>
    public enum Rules
    {
        /// <summary>&lt;Statements&gt; ::= &lt;Statements&gt; NewLine &lt;Statement&gt;</summary>
        Statements,
        /// <summary>&lt;Statements&gt; ::= &lt;Statement&gt;</summary>
        Statements2,
        /// <summary>&lt;Statement&gt; ::= &lt;About_Statement&gt;</summary>
        Statement,
        /// <summary>&lt;Statement&gt; ::= &lt;Always_Statement&gt;</summary>
        Statement2,
        /// <summary>&lt;Statement&gt; ::= &lt;Assign_Statement&gt;</summary>
        Statement3,
        /// <summary>&lt;Statement&gt; ::= &lt;Simple_Assign_Statement&gt;</summary>
        Statement4,
        /// <summary>&lt;Statement&gt; ::= &lt;Let_Statement&gt;</summary>
        Statement5,
        /// <summary>&lt;Statement&gt; ::= &lt;Auto_Search_Statement&gt;</summary>
        Statement6,
        /// <summary>&lt;Statement&gt; ::= &lt;Beep_Statement&gt;</summary>
        Statement7,
        /// <summary>&lt;Statement&gt; ::= &lt;Browser_Statement&gt;</summary>
        Statement8,
        /// <summary>&lt;Statement&gt; ::= &lt;Browser_Size_Statement&gt;</summary>
        Statement9,
        /// <summary>&lt;Statement&gt; ::= &lt;Button_Offset_Size_1_Statement&gt;</summary>
        Statement10,
        /// <summary>&lt;Statement&gt; ::= &lt;Button_Offset_Size_2_Statement&gt;</summary>
        Statement11,
        /// <summary>&lt;Statement&gt; ::= &lt;Button_Offset_1_Statement&gt;</summary>
        Statement12,
        /// <summary>&lt;Statement&gt; ::= &lt;Button_Offset_2_Statement&gt;</summary>
        Statement13,
        /// <summary>&lt;Statement&gt; ::= &lt;Call_Statement&gt;</summary>
        Statement14,
        /// <summary>&lt;Statement&gt; ::= &lt;Cancel_Select_By_Selecting_Statement&gt;</summary>
        Statement15,
        /// <summary>&lt;Statement&gt; ::= &lt;Cancel_Select_Statement&gt;</summary>
        Statement16,
        /// <summary>&lt;Statement&gt; ::= &lt;Cancel_Sort_By_Sorting_Statement&gt;</summary>
        Statement17,
        /// <summary>&lt;Statement&gt; ::= &lt;Cancel_Sort_Statement&gt;</summary>
        Statement18,
        /// <summary>&lt;Statement&gt; ::= &lt;Clear_Statement&gt;</summary>
        Statement19,
        /// <summary>&lt;Statement&gt; ::= &lt;Close_Out_Statement&gt;</summary>
        Statement20,
        /// <summary>&lt;Statement&gt; ::= &lt;Simple_CMD_Statement&gt;</summary>
        Statement21,
        /// <summary>&lt;Statement&gt; ::= &lt;CMD_Line_Statement&gt;</summary>
        Statement22,
        /// <summary>&lt;Statement&gt; ::= &lt;Define_Variable_Statement&gt;</summary>
        Statement23,
        /// <summary>&lt;Statement&gt; ::= &lt;Define_Dll_Statement&gt;</summary>
        Statement24,
        /// <summary>&lt;Statement&gt; ::= &lt;Define_Group_Statement&gt;</summary>
        Statement25,
        /// <summary>&lt;Statement&gt; ::= &lt;Delete_Records_All_Statement&gt;</summary>
        Statement26,
        /// <summary>&lt;Statement&gt; ::= &lt;Delete_Records_Selected_Statement&gt;</summary>
        Statement27,
        /// <summary>&lt;Statement&gt; ::= &lt;Delete_Table_Statement&gt;</summary>
        Statement28,
        /// <summary>&lt;Statement&gt; ::= &lt;Delete_Table_Short_Statement&gt;</summary>
        Statement29,
        /// <summary>&lt;Statement&gt; ::= &lt;Delete_Table_Long_Statement&gt;</summary>
        Statement30,
        /// <summary>&lt;Statement&gt; ::= &lt;Delete_File_Statement&gt;</summary>
        Statement31,
        /// <summary>&lt;Statement&gt; ::= &lt;Simple_Dialog_Statement&gt;</summary>
        Statement32,
        /// <summary>&lt;Statement&gt; ::= &lt;Numeric_Dialog_Implicit_Statement&gt;</summary>
        Statement33,
        /// <summary>&lt;Statement&gt; ::= &lt;Numeric_Dialog_Explicit_Statement&gt;</summary>
        Statement34,
        /// <summary>&lt;Statement&gt; ::= &lt;TextBox_Dialog_Statement&gt;</summary>
        Statement35,
        /// <summary>&lt;Statement&gt; ::= &lt;Db_Values_Dialog_Statement&gt;</summary>
        Statement36,
        /// <summary>&lt;Statement&gt; ::= &lt;YN_Dialog_Statement&gt;</summary>
        Statement37,
        /// <summary>&lt;Statement&gt; ::= &lt;Db_Views_Dialog_Statement&gt;</summary>
        Statement38,
        /// <summary>&lt;Statement&gt; ::= &lt;Databases_Dialog_Statement&gt;</summary>
        Statement39,
        /// <summary>&lt;Statement&gt; ::= &lt;Db_Variables_Dialog_Statement&gt;</summary>
        Statement40,
        /// <summary>&lt;Statement&gt; ::= &lt;Multiple_Choice_Dialog_Statement&gt;</summary>
        Statement41,
        /// <summary>&lt;Statement&gt; ::= &lt;Dialog_Read_Statement&gt;</summary>
        Statement42,
        /// <summary>&lt;Statement&gt; ::= &lt;Dialog_Write_Statement&gt;</summary>
        Statement43,
        /// <summary>&lt;Statement&gt; ::= &lt;Dialog_Read_Filter_Statement&gt;</summary>
        Statement44,
        /// <summary>&lt;Statement&gt; ::= &lt;Dialog_Write_Filter_Statement&gt;</summary>
        Statement45,
        /// <summary>&lt;Statement&gt; ::= &lt;Dialog_Date_Statement&gt;</summary>
        Statement46,
        /// <summary>&lt;Statement&gt; ::= &lt;Dialog_Date_Mask_Statement&gt;</summary>
        Statement47,
        /// <summary>&lt;Statement&gt; ::= &lt;Variables_Display_Statement&gt;</summary>
        Statement48,
        /// <summary>&lt;Statement&gt; ::= &lt;Views_Display_Statement&gt;</summary>
        Statement49,
        /// <summary>&lt;Statement&gt; ::= &lt;Tables_Display_Statement&gt;</summary>
        Statement50,
        /// <summary>&lt;Statement&gt; ::= &lt;New_Record_Statement&gt;</summary>
        Statement51,
        /// <summary>&lt;Statement&gt; ::= &lt;Simple_Execute_Statement&gt;</summary>
        Statement52,
        /// <summary>&lt;Statement&gt; ::= &lt;Execute_File_Statement&gt;</summary>
        Statement53,
        /// <summary>&lt;Statement&gt; ::= &lt;Execute_Url_Statement&gt;</summary>
        Statement54,
        /// <summary>&lt;Statement&gt; ::= &lt;Execute_Wait_For_Exit_File_Statement&gt;</summary>
        Statement55,
        /// <summary>&lt;Statement&gt; ::= &lt;Execute_Wait_For_Exit_String_Statement&gt;</summary>
        Statement56,
        /// <summary>&lt;Statement&gt; ::= &lt;Execute_Wait_For_Exit_Url_Statement&gt;</summary>
        Statement57,
        /// <summary>&lt;Statement&gt; ::= &lt;Execute_No_Wait_For_Exit_File_Statement&gt;</summary>
        Statement58,
        /// <summary>&lt;Statement&gt; ::= &lt;Execute_No_Wait_For_Exit_String_Statement&gt;</summary>
        Statement59,
        /// <summary>&lt;Statement&gt; ::= &lt;Execute_No_Wait_For_Exit_Url_Statement&gt;</summary>
        Statement60,
        /// <summary>&lt;Statement&gt; ::= &lt;Exit_Statement&gt;</summary>
        Statement61,
        /// <summary>&lt;Statement&gt; ::= &lt;File_Dialog_Statement&gt;</summary>
        Statement62,
        /// <summary>&lt;Statement&gt; ::= &lt;Freq_All_Statement&gt;</summary>
        Statement63,
        /// <summary>&lt;Statement&gt; ::= &lt;Freq_Columns_Statement&gt;</summary>
        Statement64,
        /// <summary>&lt;Statement&gt; ::= &lt;Freq_All_Except_Statement&gt;</summary>
        Statement65,
        /// <summary>&lt;Statement&gt; ::= &lt;Get_Path_Statement&gt;</summary>
        Statement66,
        /// <summary>&lt;Statement&gt; ::= &lt;Go_To_Variable_Statement&gt;</summary>
        Statement67,
        /// <summary>&lt;Statement&gt; ::= &lt;Go_To_Page_Statement&gt;</summary>
        Statement68,
        /// <summary>&lt;Statement&gt; ::= &lt;Simple_Graph_Statement&gt;</summary>
        Statement69,
        /// <summary>&lt;Statement&gt; ::= &lt;Strata_Var_Graph_Statement&gt;</summary>
        Statement70,
        /// <summary>&lt;Statement&gt; ::= &lt;Weight_Var_Graph_Statement&gt;</summary>
        Statement71,
        /// <summary>&lt;Statement&gt; ::= &lt;Stra_Wei_Var_Graph_Statement&gt;</summary>
        Statement72,
        /// <summary>&lt;Statement&gt; ::= &lt;Graph_Opt_1_Statement&gt;</summary>
        Statement73,
        /// <summary>&lt;Statement&gt; ::= &lt;Graph_Opt_2_Statement&gt;</summary>
        Statement74,
        /// <summary>&lt;Statement&gt; ::= &lt;Graph_Opt_3_Statement&gt;</summary>
        Statement75,
        /// <summary>&lt;Statement&gt; ::= &lt;Graph_Opt_4_Statement&gt;</summary>
        Statement76,
        /// <summary>&lt;Statement&gt; ::= &lt;Graph_Opt_5_Statement&gt;</summary>
        Statement77,
        /// <summary>&lt;Statement&gt; ::= &lt;Graph_Generic_Opt_Statement&gt;</summary>
        Statement78,
        /// <summary>&lt;Statement&gt; ::= &lt;Header_Title_String_Statement&gt;</summary>
        Statement79,
        /// <summary>&lt;Statement&gt; ::= &lt;Header_Title_Font_Statement&gt;</summary>
        Statement80,
        /// <summary>&lt;Statement&gt; ::= &lt;Header_Title_String_And_Font_Statement&gt;</summary>
        Statement81,
        /// <summary>&lt;Statement&gt; ::= &lt;Help_File_Statement&gt;</summary>
        Statement82,
        /// <summary>&lt;Statement&gt; ::= &lt;Simple_Help_Statement&gt;</summary>
        Statement83,
        /// <summary>&lt;Statement&gt; ::= &lt;Hide_Some_Statement&gt;</summary>
        Statement84,
        /// <summary>&lt;Statement&gt; ::= &lt;Hide_Except_Statement&gt;</summary>
        Statement85,
        /// <summary>&lt;Statement&gt; ::= &lt;If_Statement&gt;</summary>
        Statement86,
        /// <summary>&lt;Statement&gt; ::= &lt;If_Else_Statement&gt;</summary>
        Statement87,
        /// <summary>&lt;Statement&gt; ::= &lt;Simple_KM_Survival_Statement&gt;</summary>
        Statement88,
        /// <summary>&lt;Statement&gt; ::= &lt;KM_Survival_Boolean_Statement&gt;</summary>
        Statement89,
        /// <summary>&lt;Statement&gt; ::= &lt;Simple_List_Statement&gt;</summary>
        Statement90,
        /// <summary>&lt;Statement&gt; ::= &lt;List_All_Statement&gt;</summary>
        Statement91,
        /// <summary>&lt;Statement&gt; ::= &lt;List_Columns_Statement&gt;</summary>
        Statement92,
        /// <summary>&lt;Statement&gt; ::= &lt;List_All_Except_Statement&gt;</summary>
        Statement93,
        /// <summary>&lt;Statement&gt; ::= &lt;Logistic_Statement&gt;</summary>
        Statement94,
        /// <summary>&lt;Statement&gt; ::= &lt;Link_Statement&gt;</summary>
        Statement95,
        /// <summary>&lt;Statement&gt; ::= &lt;Link_Remove_Statement&gt;</summary>
        Statement96,
        /// <summary>&lt;Statement&gt; ::= &lt;Map_AVG_Statement&gt;</summary>
        Statement97,
        /// <summary>&lt;Statement&gt; ::= &lt;Map_Case_Statement&gt;</summary>
        Statement98,
        /// <summary>&lt;Statement&gt; ::= &lt;Map_Sum_Statement&gt;</summary>
        Statement99,
        /// <summary>&lt;Statement&gt; ::= &lt;Map_Count_Statement&gt;</summary>
        Statement100,
        /// <summary>&lt;Statement&gt; ::= &lt;Map_Min_Statement&gt;</summary>
        Statement101,
        /// <summary>&lt;Statement&gt; ::= &lt;Map_Max_Statement&gt;</summary>
        Statement102,
        /// <summary>&lt;Statement&gt; ::= &lt;Map_Opt_1_Statement&gt;</summary>
        Statement103,
        /// <summary>&lt;Statement&gt; ::= &lt;Map_Opt_2_Statement&gt;</summary>
        Statement104,
        /// <summary>&lt;Statement&gt; ::= &lt;Map_Opt_3_Statement&gt;</summary>
        Statement105,
        /// <summary>&lt;Statement&gt; ::= &lt;Map_Opt_4_Statement&gt;</summary>
        Statement106,
        /// <summary>&lt;Statement&gt; ::= &lt;Map_Opt_5_Statement&gt;</summary>
        Statement107,
        /// <summary>&lt;Statement&gt; ::= &lt;Match_Row_All_Statement&gt;</summary>
        Statement108,
        /// <summary>&lt;Statement&gt; ::= &lt;Match_Row_Except_Statement&gt;</summary>
        Statement109,
        /// <summary>&lt;Statement&gt; ::= &lt;Match_Column_All_Statement&gt;</summary>
        Statement110,
        /// <summary>&lt;Statement&gt; ::= &lt;Match_Column_Except_Statement&gt;</summary>
        Statement111,
        /// <summary>&lt;Statement&gt; ::= &lt;Match_Row_Column_Statement&gt;</summary>
        Statement112,
        /// <summary>&lt;Statement&gt; ::= &lt;Simple_Means_Statement&gt;</summary>
        Statement113,
        /// <summary>&lt;Statement&gt; ::= &lt;Row_All_Means_Statement&gt;</summary>
        Statement114,
        /// <summary>&lt;Statement&gt; ::= &lt;Row_All_Column_Means_Statement&gt;</summary>
        Statement115,
        /// <summary>&lt;Statement&gt; ::= &lt;Row_Column_All_Means_Statement&gt;</summary>
        Statement116,
        /// <summary>&lt;Statement&gt; ::= &lt;Row_Column_Means_Statement&gt;</summary>
        Statement117,
        /// <summary>&lt;Statement&gt; ::= &lt;Menu_Statement&gt;</summary>
        Statement118,
        /// <summary>&lt;Statement&gt; ::= &lt;Menu_Command_Statement&gt;</summary>
        Statement119,
        /// <summary>&lt;Statement&gt; ::= &lt;Menu_Dialog_Statement&gt;</summary>
        Statement120,
        /// <summary>&lt;Statement&gt; ::= &lt;Menu_Execute_Statement&gt;</summary>
        Statement121,
        /// <summary>&lt;Statement&gt; ::= &lt;Menu_Item_Block_Name_Statement&gt;</summary>
        Statement122,
        /// <summary>&lt;Statement&gt; ::= &lt;Menu_Item_Separator_Statement&gt;</summary>
        Statement123,
        /// <summary>&lt;Statement&gt; ::= &lt;Menu_Replace_Statement&gt;</summary>
        Statement124,
        /// <summary>&lt;Statement&gt; ::= &lt;Merge_Table_Statement&gt;</summary>
        Statement125,
        /// <summary>&lt;Statement&gt; ::= &lt;Merge_Db_Table_Statement&gt;</summary>
        Statement126,
        /// <summary>&lt;Statement&gt; ::= &lt;Merge_File_Statement&gt;</summary>
        Statement127,
        /// <summary>&lt;Statement&gt; ::= &lt;Merge_Excel_File_Statement&gt;</summary>
        Statement128,
        /// <summary>&lt;Statement&gt; ::= &lt;Move_Buttons_Statement&gt;</summary>
        Statement129,
        /// <summary>&lt;Statement&gt; ::= &lt;New_Page_Statement&gt;</summary>
        Statement130,
        /// <summary>&lt;Statement&gt; ::= &lt;On_Browser_Exit_Block&gt;</summary>
        Statement131,
        /// <summary>&lt;Statement&gt; ::= &lt;Picture_Statement&gt;</summary>
        Statement132,
        /// <summary>&lt;Statement&gt; ::= &lt;Picture_Size_Statement&gt;</summary>
        Statement133,
        /// <summary>&lt;Statement&gt; ::= &lt;Popup_Statement&gt;</summary>
        Statement134,
        /// <summary>&lt;Statement&gt; ::= &lt;Simple_Print_Out_Statement&gt;</summary>
        Statement135,
        /// <summary>&lt;Statement&gt; ::= &lt;File_Print_Out_Statement&gt;</summary>
        Statement136,
        /// <summary>&lt;Statement&gt; ::= &lt;Quit_Statement&gt;</summary>
        Statement137,
        /// <summary>&lt;Statement&gt; ::= &lt;Simple_Read_Statement&gt;</summary>
        Statement138,
        /// <summary>&lt;Statement&gt; ::= &lt;Read_Epi_Statement&gt;</summary>
        Statement139,
        /// <summary>&lt;Statement&gt; ::= &lt;Read_Epi_File_Spec_Statement&gt;</summary>
        Statement140,
        /// <summary>&lt;Statement&gt; ::= &lt;Read_Sql_Statement&gt;</summary>
        Statement141,
        /// <summary>&lt;Statement&gt; ::= &lt;Read_Excel_File_Statement&gt;</summary>
        Statement142,
        /// <summary>&lt;Statement&gt; ::= &lt;Read_Db_Table_Statement&gt;</summary>
        Statement143,
        /// <summary>&lt;Statement&gt; ::= &lt;Recode_Statement&gt;</summary>
        Statement144,
        /// <summary>&lt;Statement&gt; ::= &lt;Regress_Statement&gt;</summary>
        Statement145,
        /// <summary>&lt;Statement&gt; ::= &lt;Relate_Epi_Table_Statement&gt;</summary>
        Statement146,
        /// <summary>&lt;Statement&gt; ::= &lt;Relate_Table_Statement&gt;</summary>
        Statement147,
        /// <summary>&lt;Statement&gt; ::= &lt;Relate_Db_Table_Statement&gt;</summary>
        Statement148,
        /// <summary>&lt;Statement&gt; ::= &lt;Relate_Db_Table_With_Identifier_Statement&gt;</summary>
        Statement149,
        /// <summary>&lt;Statement&gt; ::= &lt;Relate_File_Statement&gt;</summary>
        Statement150,
        /// <summary>&lt;Statement&gt; ::= &lt;Relate_Excel_File_Statement&gt;</summary>
        Statement151,
        /// <summary>&lt;Statement&gt; ::= &lt;Repeat_Statement&gt;</summary>
        Statement152,
        /// <summary>&lt;Statement&gt; ::= &lt;Simple_Routeout_Statement&gt;</summary>
        Statement153,
        /// <summary>&lt;Statement&gt; ::= &lt;Replace_Routeout_Statement&gt;</summary>
        Statement154,
        /// <summary>&lt;Statement&gt; ::= &lt;Append_Routeout_Statement&gt;</summary>
        Statement155,
        /// <summary>&lt;Statement&gt; ::= &lt;Simple_Run_Statement&gt;</summary>
        Statement156,
        /// <summary>&lt;Statement&gt; ::= &lt;Run_String_Statement&gt;</summary>
        Statement157,
        /// <summary>&lt;Statement&gt; ::= &lt;Run_File_PGM_Statement&gt;</summary>
        Statement158,
        /// <summary>&lt;Statement&gt; ::= &lt;Run_PGM_In_Db_Statement&gt;</summary>
        Statement159,
        /// <summary>&lt;Statement&gt; ::= &lt;Screen_Text_Statement&gt;</summary>
        Statement160,
        /// <summary>&lt;Statement&gt; ::= &lt;Select_Statement&gt;</summary>
        Statement161,
        /// <summary>&lt;Statement&gt; ::= &lt;Set_Statement&gt;</summary>
        Statement162,
        /// <summary>&lt;Statement&gt; ::= &lt;Set_Buttons_Statement&gt;</summary>
        Statement163,
        /// <summary>&lt;Statement&gt; ::= &lt;Set_DB_Version_Statement&gt;</summary>
        Statement164,
        /// <summary>&lt;Statement&gt; ::= &lt;Set_DOS_Win_Statement&gt;</summary>
        Statement165,
        /// <summary>&lt;Statement&gt; ::= &lt;Set_Import_Year_Statement&gt;</summary>
        Statement166,
        /// <summary>&lt;Statement&gt; ::= &lt;Set_Ini_Dir_Statement&gt;</summary>
        Statement167,
        /// <summary>&lt;Statement&gt; ::= &lt;Set_Language_Statement&gt;</summary>
        Statement168,
        /// <summary>&lt;Statement&gt; ::= &lt;Set_Picture_Statement&gt;</summary>
        Statement169,
        /// <summary>&lt;Statement&gt; ::= &lt;Set_Work_Dir_Statement&gt;</summary>
        Statement170,
        /// <summary>&lt;Statement&gt; ::= &lt;ShutDown_Block&gt;</summary>
        Statement171,
        /// <summary>&lt;Statement&gt; ::= &lt;Sort_Statement&gt;</summary>
        Statement172,
        /// <summary>&lt;Statement&gt; ::= &lt;Startup_Block&gt;</summary>
        Statement173,
        /// <summary>&lt;Statement&gt; ::= &lt;Summarize_Statement&gt;</summary>
        Statement174,
        /// <summary>&lt;Statement&gt; ::= &lt;Sys_Info_Statement&gt;</summary>
        Statement175,
        /// <summary>&lt;Statement&gt; ::= &lt;Simple_Tables_Statement&gt;</summary>
        Statement176,
        /// <summary>&lt;Statement&gt; ::= &lt;Tables_One_Variable_Statement&gt;</summary>
        Statement177,
        /// <summary>&lt;Statement&gt; ::= &lt;Row_All_Tables_Statement&gt;</summary>
        Statement178,
        /// <summary>&lt;Statement&gt; ::= &lt;Row_Except_Tables_Statement&gt;</summary>
        Statement179,
        /// <summary>&lt;Statement&gt; ::= &lt;Column_All_Tables_Statement&gt;</summary>
        Statement180,
        /// <summary>&lt;Statement&gt; ::= &lt;Column_Except_Tables_Statement&gt;</summary>
        Statement181,
        /// <summary>&lt;Statement&gt; ::= &lt;Row_Column_Tables_Statement&gt;</summary>
        Statement182,
        /// <summary>&lt;Statement&gt; ::= &lt;Type_Out_String_Statement&gt;</summary>
        Statement183,
        /// <summary>&lt;Statement&gt; ::= &lt;Type_Out_File_Statement&gt;</summary>
        Statement184,
        /// <summary>&lt;Statement&gt; ::= &lt;Type_Out_String_With_Font_Statement&gt;</summary>
        Statement185,
        /// <summary>&lt;Statement&gt; ::= &lt;Type_Out_File_With_Font_Statement&gt;</summary>
        Statement186,
        /// <summary>&lt;Statement&gt; ::= &lt;Simple_Undefine_Statement&gt;</summary>
        Statement187,
        /// <summary>&lt;Statement&gt; ::= &lt;All_Standard_Undefine_Statement&gt;</summary>
        Statement188,
        /// <summary>&lt;Statement&gt; ::= &lt;All_Global_Undefine_Statement&gt;</summary>
        Statement189,
        /// <summary>&lt;Statement&gt; ::= &lt;Undelete_All_Statement&gt;</summary>
        Statement190,
        /// <summary>&lt;Statement&gt; ::= &lt;Undelete_Selected_Statement&gt;</summary>
        Statement191,
        /// <summary>&lt;Statement&gt; ::= &lt;Unhide_Some_Statement&gt;</summary>
        Statement192,
        /// <summary>&lt;Statement&gt; ::= &lt;Unhide_Except_Statement&gt;</summary>
        Statement193,
        /// <summary>&lt;Statement&gt; ::= &lt;Wait_For_Statement&gt;</summary>
        Statement194,
        /// <summary>&lt;Statement&gt; ::= &lt;Wait_For_Exit_Statement&gt;</summary>
        Statement195,
        /// <summary>&lt;Statement&gt; ::= &lt;Wait_For_File_Exists_Statement&gt;</summary>
        Statement196,
        /// <summary>&lt;Statement&gt; ::= &lt;Write_All_Statement&gt;</summary>
        Statement197,
        /// <summary>&lt;Statement&gt; ::= &lt;Write_Some_Statement&gt;</summary>
        Statement198,
        /// <summary>&lt;Statement&gt; ::= &lt;Write_Except_Statement&gt;</summary>
        Statement199,
        /// <summary>&lt;Statement&gt; ::= &lt;Command_Block&gt;</summary>
        Statement200,
        /// <summary>&lt;Statement&gt; ::= &lt;On_Browser_Exit_Empty_Block&gt;</summary>
        Statement201,
        /// <summary>&lt;Statement&gt; ::= &lt;Startup_Empty_Block&gt;</summary>
        Statement202,
        /// <summary>&lt;Statement&gt; ::= &lt;ShutDown_Empty_Block&gt;</summary>
        Statement203,
        /// <summary>&lt;Statement&gt; ::= &lt;Menu_Empty_Block&gt;</summary>
        Statement204,
        /// <summary>&lt;Statement&gt; ::= &lt;Popup_Empty_Block&gt;</summary>
        Statement205,
        /// <summary>&lt;Statement&gt; ::= &lt;Empty_Command_Block&gt;</summary>
        Statement206,
        /// <summary>&lt;Statement&gt; ::= NewLine</summary>
        Statement207,
        /// <summary>&lt;Literal&gt; ::= &lt;Number&gt;</summary>
        Literal,
        /// <summary>&lt;Literal&gt; ::= &lt;Literal_Char&gt;</summary>
        Literal2,
        /// <summary>&lt;Literal&gt; ::= &lt;Literal_String&gt;</summary>
        Literal3,
        /// <summary>&lt;Literal&gt; ::= &lt;Literal_Date&gt;</summary>
        Literal4,
        /// <summary>&lt;Literal&gt; ::= &lt;Literal_Time&gt;</summary>
        Literal5,
        /// <summary>&lt;Literal&gt; ::= &lt;Literal_Date_Time&gt;</summary>
        Literal6,
        /// <summary>&lt;Literal_Char&gt; ::= CharLiteral</summary>
        Literal_Char,
        /// <summary>&lt;Literal_String&gt; ::= String</summary>
        Literal_String,
        /// <summary>&lt;Literal_Date&gt; ::= Date</summary>
        Literal_Date,
        /// <summary>&lt;Literal_Time&gt; ::= Time</summary>
        Literal_Time,
        /// <summary>&lt;Literal_Date_Time&gt; ::= Date Time</summary>
        Literal_Date_Time,
        /// <summary>&lt;Number&gt; ::= &lt;Real_Number&gt;</summary>
        Number,
        /// <summary>&lt;Number&gt; ::= &lt;Decimal_Number&gt;</summary>
        Number2,
        /// <summary>&lt;Number&gt; ::= &lt;Hex_Number&gt;</summary>
        Number3,
        /// <summary>&lt;Real_Number&gt; ::= RealLiteral</summary>
        Real_Number,
        /// <summary>&lt;Decimal_Number&gt; ::= DecLiteral</summary>
        Decimal_Number,
        /// <summary>&lt;Hex_Number&gt; ::= HexLiteral</summary>
        Hex_Number,
        /// <summary>&lt;IdentifierList&gt; ::= &lt;IdentifierList&gt; Identifier</summary>
        IdentifierList,
        /// <summary>&lt;IdentifierList&gt; ::= Identifier</summary>
        IdentifierList2,
        /// <summary>&lt;Qualified ID&gt; ::= Identifier</summary>
        Qualified_ID,
        /// <summary>&lt;Qualified ID&gt; ::= &lt;Fully_Qualified_Id&gt;</summary>
        Qualified_ID2,
        /// <summary>&lt;Fully_Qualified_Id&gt; ::= Identifier '.' &lt;Qualified ID&gt;</summary>
        Fully_Qualified_Id,
        /// <summary>&lt;Generic_String&gt; ::= String</summary>
        Generic_String,
        /// <summary>&lt;StringList&gt; ::= &lt;Generic_String&gt;</summary>
        StringList,
        /// <summary>&lt;StringList&gt; ::= &lt;Comma_Delimited_Strings&gt;</summary>
        StringList2,
        /// <summary>&lt;Comma_Delimited_Strings&gt; ::= &lt;StringList&gt; ',' &lt;Generic_String&gt;</summary>
        Comma_Delimited_Strings,
        /// <summary>&lt;VariableTypeIndicator&gt; ::= &lt;Var_Type_Numeric&gt;</summary>
        VariableTypeIndicator,
        /// <summary>&lt;VariableTypeIndicator&gt; ::= &lt;Var_Type_Text_Input&gt;</summary>
        VariableTypeIndicator2,
        /// <summary>&lt;VariableTypeIndicator&gt; ::= &lt;Var_Type_YN&gt;</summary>
        VariableTypeIndicator3,
        /// <summary>&lt;VariableTypeIndicator&gt; ::= &lt;Var_Type_Date&gt;</summary>
        VariableTypeIndicator4,
        /// <summary>&lt;VariableTypeIndicator&gt; ::= </summary>
        VariableTypeIndicator5,
        /// <summary>&lt;Var_Type_Numeric&gt; ::= NUMERIC</summary>
        Var_Type_Numeric,
        /// <summary>&lt;Var_Type_Text_Input&gt; ::= TEXTINPUT</summary>
        Var_Type_Text_Input,
        /// <summary>&lt;Var_Type_YN&gt; ::= YN</summary>
        Var_Type_YN,
        /// <summary>&lt;Var_Type_Date&gt; ::= DATEFORMAT</summary>
        Var_Type_Date,
        /// <summary>&lt;FuncName1&gt; ::= ABS</summary>
        FuncName1,
        /// <summary>&lt;FuncName1&gt; ::= COS</summary>
        FuncName12,
        /// <summary>&lt;FuncName1&gt; ::= DAY</summary>
        FuncName13,
        /// <summary>&lt;FuncName1&gt; ::= DAYS</summary>
        FuncName14,
        /// <summary>&lt;FuncName1&gt; ::= ENVIRON</summary>
        FuncName15,
        /// <summary>&lt;FuncName1&gt; ::= EXISTS</summary>
        FuncName16,
        /// <summary>&lt;FuncName1&gt; ::= EXP</summary>
        FuncName17,
        /// <summary>&lt;FuncName1&gt; ::= FILEDATE</summary>
        FuncName18,
        /// <summary>&lt;FuncName1&gt; ::= FINDTEXT</summary>
        FuncName19,
        /// <summary>&lt;FuncName1&gt; ::= FORMAT</summary>
        FuncName110,
        /// <summary>&lt;FuncName1&gt; ::= HOUR</summary>
        FuncName111,
        /// <summary>&lt;FuncName1&gt; ::= HOURS</summary>
        FuncName112,
        /// <summary>&lt;FuncName1&gt; ::= LN</summary>
        FuncName113,
        /// <summary>&lt;FuncName1&gt; ::= LOG</summary>
        FuncName114,
        /// <summary>&lt;FuncName1&gt; ::= MINUTES</summary>
        FuncName115,
        /// <summary>&lt;FuncName1&gt; ::= Month</summary>
        FuncName116,
        /// <summary>&lt;FuncName1&gt; ::= MONTHS</summary>
        FuncName117,
        /// <summary>&lt;FuncName1&gt; ::= NUMTODATE</summary>
        FuncName118,
        /// <summary>&lt;FuncName1&gt; ::= NUMTOTIME</summary>
        FuncName119,
        /// <summary>&lt;FuncName1&gt; ::= RECORDCOUNT</summary>
        FuncName120,
        /// <summary>&lt;FuncName1&gt; ::= RND</summary>
        FuncName121,
        /// <summary>&lt;FuncName1&gt; ::= ROUND</summary>
        FuncName122,
        /// <summary>&lt;FuncName1&gt; ::= SECOND</summary>
        FuncName123,
        /// <summary>&lt;FuncName1&gt; ::= SECONDS</summary>
        FuncName124,
        /// <summary>&lt;FuncName1&gt; ::= STEP</summary>
        FuncName125,
        /// <summary>&lt;FuncName1&gt; ::= SUBSTRING</summary>
        FuncName126,
        /// <summary>&lt;FuncName1&gt; ::= SIN</summary>
        FuncName127,
        /// <summary>&lt;FuncName1&gt; ::= TRUNC</summary>
        FuncName128,
        /// <summary>&lt;FuncName1&gt; ::= TXTTODATE</summary>
        FuncName129,
        /// <summary>&lt;FuncName1&gt; ::= TXTTONUM</summary>
        FuncName130,
        /// <summary>&lt;FuncName1&gt; ::= TAN</summary>
        FuncName131,
        /// <summary>&lt;FuncName1&gt; ::= UPPERCASE</summary>
        FuncName132,
        /// <summary>&lt;FuncName1&gt; ::= YEAR</summary>
        FuncName133,
        /// <summary>&lt;FuncName1&gt; ::= YEARS</summary>
        FuncName134,
        /// <summary>&lt;FuncName2&gt; ::= SYSTEMTIME</summary>
        FuncName2,
        /// <summary>&lt;FuncName2&gt; ::= SYSTEMDATE</summary>
        FuncName22,
        /// <summary>&lt;FunctionCall&gt; ::= &lt;FuncName1&gt; '(' &lt;FunctionParameterList&gt; ')'</summary>
        FunctionCall,
        /// <summary>&lt;FunctionCall&gt; ::= &lt;FuncName1&gt; '(' &lt;FunctionCall&gt; ')'</summary>
        FunctionCall2,
        /// <summary>&lt;FunctionCall&gt; ::= &lt;FuncName2&gt;</summary>
        FunctionCall3,
        /// <summary>&lt;FunctionParameterList&gt; ::= &lt;EmptyFunctionParameterList&gt;</summary>
        FunctionParameterList,
        /// <summary>&lt;FunctionParameterList&gt; ::= &lt;NonEmptyFunctionParameterList&gt;</summary>
        FunctionParameterList2,
        /// <summary>&lt;NonEmptyFunctionParameterList&gt; ::= &lt;MultipleFunctionParameterList&gt;</summary>
        NonEmptyFunctionParameterList,
        /// <summary>&lt;NonEmptyFunctionParameterList&gt; ::= &lt;SingleFunctionParameterList&gt;</summary>
        NonEmptyFunctionParameterList2,
        /// <summary>&lt;MultipleFunctionParameterList&gt; ::= &lt;NonEmptyFunctionParameterList&gt; ',' &lt;Literal&gt;</summary>
        MultipleFunctionParameterList,
        /// <summary>&lt;SingleFunctionParameterList&gt; ::= &lt;Literal&gt;</summary>
        SingleFunctionParameterList,
        /// <summary>&lt;EmptyFunctionParameterList&gt; ::= </summary>
        EmptyFunctionParameterList,
        /// <summary>&lt;Always_Statement&gt; ::= ALWAYS NewLine &lt;Statements&gt; NewLine END</summary>
        Always_Statement,
        /// <summary>&lt;Assign_Statement&gt; ::= ASSIGN &lt;Qualified ID&gt; '=' &lt;Expression&gt;</summary>
        Assign_Statement,
        /// <summary>&lt;Assign_Statement&gt; ::= ASSIGN &lt;Qualified ID&gt; '=' &lt;FunctionCall&gt;</summary>
        Assign_Statement2,
        /// <summary>&lt;Simple_Assign_Statement&gt; ::= Identifier '=' &lt;Expression&gt;</summary>
        Simple_Assign_Statement,
        /// <summary>&lt;Simple_Assign_Statement&gt; ::= Identifier '=' &lt;FunctionCall&gt;</summary>
        Simple_Assign_Statement2,
        /// <summary>&lt;Let_Statement&gt; ::= LET Identifier '=' &lt;Expression&gt;</summary>
        Let_Statement,
        /// <summary>&lt;Let_Statement&gt; ::= LET Identifier '=' &lt;FunctionCall&gt;</summary>
        Let_Statement2,
        /// <summary>&lt;Auto_Search_Statement&gt; ::= AUTOSEARCH &lt;IdentifierList&gt;</summary>
        Auto_Search_Statement,
        /// <summary>&lt;Beep_Statement&gt; ::= BEEP</summary>
        Beep_Statement,
        /// <summary>&lt;Cancel_Select_By_Selecting_Statement&gt; ::= SELECT</summary>
        Cancel_Select_By_Selecting_Statement,
        /// <summary>&lt;Cancel_Select_Statement&gt; ::= CANCEL SELECT</summary>
        Cancel_Select_Statement,
        /// <summary>&lt;Cancel_Sort_By_Sorting_Statement&gt; ::= SORT</summary>
        Cancel_Sort_By_Sorting_Statement,
        /// <summary>&lt;Cancel_Sort_Statement&gt; ::= CANCEL SORT</summary>
        Cancel_Sort_Statement,
        /// <summary>&lt;Clear_Statement&gt; ::= CLEAR &lt;IdentifierList&gt;</summary>
        Clear_Statement,
        /// <summary>&lt;Close_Out_Statement&gt; ::= CLOSEOUT</summary>
        Close_Out_Statement,
        /// <summary>&lt;Simple_CMD_Statement&gt; ::= CMD Identifier NewLine &lt;Statements&gt; END</summary>
        Simple_CMD_Statement,
        /// <summary>&lt;CMD_Line_Statement&gt; ::= COMMANDLINE &lt;CMDListOpt&gt;</summary>
        CMD_Line_Statement,
        /// <summary>&lt;CMDListOpt&gt; ::= &lt;CMDLineOptString&gt;</summary>
        CMDListOpt,
        /// <summary>&lt;CMDListOpt&gt; ::= &lt;CMDLineOptFile&gt;</summary>
        CMDListOpt2,
        /// <summary>&lt;CMDListOpt&gt; ::= &lt;CMDLineOptPGM&gt;</summary>
        CMDListOpt3,
        /// <summary>&lt;CMDListOpt&gt; ::= &lt;CMDLineOptView&gt;</summary>
        CMDListOpt4,
        /// <summary>&lt;CMDListOpt&gt; ::= &lt;CMDLineOptPGM&gt; &lt;CMDLineOptView&gt;</summary>
        CMDListOpt5,
        /// <summary>&lt;CMDListOpt&gt; ::= &lt;CMDLineOptView&gt; &lt;CMDLineOptPGM&gt;</summary>
        CMDListOpt6,
        /// <summary>&lt;CMDListOpt&gt; ::= </summary>
        CMDListOpt7,
        /// <summary>&lt;CMDLineOptFile&gt; ::= File</summary>
        CMDLineOptFile,
        /// <summary>&lt;CMDLineOptString&gt; ::= File ':' String</summary>
        CMDLineOptString,
        /// <summary>&lt;CMDLineOptPGM&gt; ::= &lt;CMDLineOptProjectPGM&gt;</summary>
        CMDLineOptPGM,
        /// <summary>&lt;CMDLineOptPGM&gt; ::= &lt;CMDLineOptFilePGM&gt;</summary>
        CMDLineOptPGM2,
        /// <summary>&lt;CMDLineOptProjectPGM&gt; ::= PGMNAME '=' File ':' String</summary>
        CMDLineOptProjectPGM,
        /// <summary>&lt;CMDLineOptFilePGM&gt; ::= PGMNAME '=' File</summary>
        CMDLineOptFilePGM,
        /// <summary>&lt;CMDLineOptView&gt; ::= VIEWNAME '=' File ':' String</summary>
        CMDLineOptView,
        /// <summary>&lt;Define_Variable_Statement&gt; ::= DEFINE Identifier &lt;Variable_Scope&gt; &lt;VariableTypeIndicator&gt; &lt;Define_Prompt&gt;</summary>
        Define_Variable_Statement,
        /// <summary>&lt;Define_Dll_Statement&gt; ::= DEFINE Identifier DLLOBJECT String</summary>
        Define_Dll_Statement,
        /// <summary>&lt;Define_Group_Statement&gt; ::= DEFINE Identifier GROUPVAR &lt;IdentifierList&gt;</summary>
        Define_Group_Statement,
        /// <summary>&lt;Define_Prompt&gt; ::= '(' String ')'</summary>
        Define_Prompt,
        /// <summary>&lt;Define_Prompt&gt; ::= String</summary>
        Define_Prompt2,
        /// <summary>&lt;Define_Prompt&gt; ::= </summary>
        Define_Prompt3,
        /// <summary>&lt;Variable_Scope&gt; ::= STANDARD</summary>
        Variable_Scope,
        /// <summary>&lt;Variable_Scope&gt; ::= GLOBAL</summary>
        Variable_Scope2,
        /// <summary>&lt;Variable_Scope&gt; ::= PERMANENT</summary>
        Variable_Scope3,
        /// <summary>&lt;Variable_Scope&gt; ::= </summary>
        Variable_Scope4,
        /// <summary>&lt;Delete_File_Statement&gt; ::= DELETE File &lt;DeleteOpts&gt;</summary>
        Delete_File_Statement,
        /// <summary>&lt;Delete_Records_All_Statement&gt; ::= DELETE '*' &lt;DeleteOpts&gt;</summary>
        Delete_Records_All_Statement,
        /// <summary>&lt;Delete_Records_Selected_Statement&gt; ::= DELETE &lt;Compare Exp&gt; &lt;DeleteOpts&gt;</summary>
        Delete_Records_Selected_Statement,
        /// <summary>&lt;Delete_Table_Statement&gt; ::= DELETE TABLES Identifier &lt;DeleteOpts&gt;</summary>
        Delete_Table_Statement,
        /// <summary>&lt;Delete_Table_Short_Statement&gt; ::= DELETE TABLES File &lt;DeleteOpts&gt;</summary>
        Delete_Table_Short_Statement,
        /// <summary>&lt;Delete_Table_Long_Statement&gt; ::= DELETE TABLES File ':' Identifier &lt;DeleteOpts&gt;</summary>
        Delete_Table_Long_Statement,
        /// <summary>&lt;DeleteOpts&gt; ::= &lt;DeleteOpts&gt; &lt;DeleteOpt&gt;</summary>
        DeleteOpts,
        /// <summary>&lt;DeleteOpts&gt; ::= &lt;DeleteOpt&gt;</summary>
        DeleteOpts2,
        /// <summary>&lt;DeleteOpts&gt; ::= </summary>
        DeleteOpts3,
        /// <summary>&lt;DeleteOpt&gt; ::= PERMANENT</summary>
        DeleteOpt,
        /// <summary>&lt;DeleteOpt&gt; ::= SAVEDATA</summary>
        DeleteOpt2,
        /// <summary>&lt;DeleteOpt&gt; ::= RUNSILENT</summary>
        DeleteOpt3,
        /// <summary>&lt;Simple_Dialog_Statement&gt; ::= DIALOG String &lt;TitleOpt&gt;</summary>
        Simple_Dialog_Statement,
        /// <summary>&lt;Numeric_Dialog_Implicit_Statement&gt; ::= DIALOG String Identifier &lt;TitleOpt&gt;</summary>
        Numeric_Dialog_Implicit_Statement,
        /// <summary>&lt;TextBox_Dialog_Statement&gt; ::= DIALOG String Identifier TEXTINPUT &lt;MaskOpt&gt; &lt;TitleOpt&gt;</summary>
        TextBox_Dialog_Statement,
        /// <summary>&lt;Numeric_Dialog_Explicit_Statement&gt; ::= DIALOG String Identifier NUMERIC &lt;MaskOpt&gt; &lt;TitleOpt&gt;</summary>
        Numeric_Dialog_Explicit_Statement,
        /// <summary>&lt;Db_Values_Dialog_Statement&gt; ::= DIALOG String Identifier DBVALUES Identifier Identifier &lt;TitleOpt&gt;</summary>
        Db_Values_Dialog_Statement,
        /// <summary>&lt;YN_Dialog_Statement&gt; ::= DIALOG String Identifier YN &lt;TitleOpt&gt;</summary>
        YN_Dialog_Statement,
        /// <summary>&lt;Db_Views_Dialog_Statement&gt; ::= DIALOG String Identifier DBVIEWS &lt;TitleOpt&gt;</summary>
        Db_Views_Dialog_Statement,
        /// <summary>&lt;Databases_Dialog_Statement&gt; ::= DIALOG String Identifier DATABASES &lt;TitleOpt&gt;</summary>
        Databases_Dialog_Statement,
        /// <summary>&lt;Db_Variables_Dialog_Statement&gt; ::= DIALOG String Identifier DBVARIABLES &lt;TitleOpt&gt;</summary>
        Db_Variables_Dialog_Statement,
        /// <summary>&lt;Multiple_Choice_Dialog_Statement&gt; ::= DIALOG String Identifier &lt;StringList&gt; &lt;TitleOpt&gt;</summary>
        Multiple_Choice_Dialog_Statement,
        /// <summary>&lt;Dialog_Read_Statement&gt; ::= DIALOG String Identifier READ &lt;TitleOpt&gt;</summary>
        Dialog_Read_Statement,
        /// <summary>&lt;Dialog_Write_Statement&gt; ::= DIALOG String Identifier WRITE &lt;TitleOpt&gt;</summary>
        Dialog_Write_Statement,
        /// <summary>&lt;Dialog_Read_Filter_Statement&gt; ::= DIALOG String Identifier READ String &lt;TitleOpt&gt;</summary>
        Dialog_Read_Filter_Statement,
        /// <summary>&lt;Dialog_Write_Filter_Statement&gt; ::= DIALOG String Identifier WRITE String &lt;TitleOpt&gt;</summary>
        Dialog_Write_Filter_Statement,
        /// <summary>&lt;Dialog_Date_Statement&gt; ::= DIALOG String Identifier DATEFORMAT &lt;TitleOpt&gt;</summary>
        Dialog_Date_Statement,
        /// <summary>&lt;Dialog_Date_Mask_Statement&gt; ::= DIALOG String Identifier DATEFORMAT String &lt;TitleOpt&gt;</summary>
        Dialog_Date_Mask_Statement,
        /// <summary>&lt;TitleOpt&gt; ::= &lt;SimpleTitleOpt&gt;</summary>
        TitleOpt,
        /// <summary>&lt;TitleOpt&gt; ::= </summary>
        TitleOpt2,
        /// <summary>&lt;SimpleTitleOpt&gt; ::= TITLETEXT '=' String</summary>
        SimpleTitleOpt,
        /// <summary>&lt;MaskOpt&gt; ::= String</summary>
        MaskOpt,
        /// <summary>&lt;MaskOpt&gt; ::= </summary>
        MaskOpt2,
        /// <summary>&lt;Variables_Display_Statement&gt; ::= DISPLAY DBVARIABLES &lt;DbVariablesOpt&gt; &lt;DisplayOpt&gt;</summary>
        Variables_Display_Statement,
        /// <summary>&lt;Views_Display_Statement&gt; ::= DISPLAY DBVIEWS &lt;DbViewsOpt&gt; &lt;DisplayOpt&gt;</summary>
        Views_Display_Statement,
        /// <summary>&lt;Tables_Display_Statement&gt; ::= DISPLAY TABLES &lt;DbViewsOpt&gt; &lt;DisplayOpt&gt;</summary>
        Tables_Display_Statement,
        /// <summary>&lt;DbVariablesOpt&gt; ::= DEFINE</summary>
        DbVariablesOpt,
        /// <summary>&lt;DbVariablesOpt&gt; ::= FIELDVAR</summary>
        DbVariablesOpt2,
        /// <summary>&lt;DbVariablesOpt&gt; ::= LIST &lt;IdentifierList&gt;</summary>
        DbVariablesOpt3,
        /// <summary>&lt;DbVariablesOpt&gt; ::= &lt;IdentifierList&gt;</summary>
        DbVariablesOpt4,
        /// <summary>&lt;DbVariablesOpt&gt; ::= </summary>
        DbVariablesOpt22,
        /// <summary>&lt;DisplayOpt&gt; ::= &lt;DefaultDisplayOpt&gt;</summary>
        DisplayOpt,
        /// <summary>&lt;DisplayOpt&gt; ::= </summary>
        DisplayOpt2,
        /// <summary>&lt;DefaultDisplayOpt&gt; ::= OUTTABLE '=' Identifier</summary>
        DefaultDisplayOpt,
        /// <summary>&lt;DbViewsOpt&gt; ::= &lt;DefaultDbViewsOpt&gt;</summary>
        DbViewsOpt,
        /// <summary>&lt;DbViewsOpt&gt; ::= </summary>
        DbViewsOpt2,
        /// <summary>&lt;DefaultDbViewsOpt&gt; ::= File</summary>
        DefaultDbViewsOpt,
        /// <summary>&lt;Simple_Execute_Statement&gt; ::= EXECUTE String</summary>
        Simple_Execute_Statement,
        /// <summary>&lt;Execute_File_Statement&gt; ::= EXECUTE File</summary>
        Execute_File_Statement,
        /// <summary>&lt;Execute_Url_Statement&gt; ::= EXECUTE Url</summary>
        Execute_Url_Statement,
        /// <summary>&lt;Execute_Wait_For_Exit_File_Statement&gt; ::= EXECUTE WAITFOREXIT File</summary>
        Execute_Wait_For_Exit_File_Statement,
        /// <summary>&lt;Execute_Wait_For_Exit_String_Statement&gt; ::= EXECUTE WAITFOREXIT String</summary>
        Execute_Wait_For_Exit_String_Statement,
        /// <summary>&lt;Execute_Wait_For_Exit_Url_Statement&gt; ::= EXECUTE WAITFOREXIT Url</summary>
        Execute_Wait_For_Exit_Url_Statement,
        /// <summary>&lt;Execute_No_Wait_For_Exit_File_Statement&gt; ::= EXECUTE NOWAITFOREXIT File</summary>
        Execute_No_Wait_For_Exit_File_Statement,
        /// <summary>&lt;Execute_No_Wait_For_Exit_String_Statement&gt; ::= EXECUTE NOWAITFOREXIT String</summary>
        Execute_No_Wait_For_Exit_String_Statement,
        /// <summary>&lt;Execute_No_Wait_For_Exit_Url_Statement&gt; ::= EXECUTE NOWAITFOREXIT Url</summary>
        Execute_No_Wait_For_Exit_Url_Statement,
        /// <summary>&lt;Exit_Statement&gt; ::= EXIT</summary>
        Exit_Statement,
        /// <summary>&lt;Freq_All_Statement&gt; ::= FREQ '*' &lt;FreqOpts&gt;</summary>
        Freq_All_Statement,
        /// <summary>&lt;Freq_Columns_Statement&gt; ::= FREQ &lt;IdentifierList&gt; &lt;FreqOpts&gt;</summary>
        Freq_Columns_Statement,
        /// <summary>&lt;Freq_All_Except_Statement&gt; ::= FREQ '*' EXCEPT &lt;IdentifierList&gt; &lt;FreqOpts&gt;</summary>
        Freq_All_Except_Statement,
        /// <summary>&lt;FreqOpts&gt; ::= &lt;FreqOpts&gt; &lt;FreqOpt&gt;</summary>
        FreqOpts,
        /// <summary>&lt;FreqOpts&gt; ::= &lt;FreqOpt&gt;</summary>
        FreqOpts2,
        /// <summary>&lt;FreqOpts&gt; ::= </summary>
        FreqOpts3,
        /// <summary>&lt;FreqOpt&gt; ::= &lt;WeightOpt&gt;</summary>
        FreqOpt,
        /// <summary>&lt;FreqOpt&gt; ::= &lt;FreqOptStrata&gt;</summary>
        FreqOpt2,
        /// <summary>&lt;FreqOpt&gt; ::= &lt;OutTableOpt&gt;</summary>
        FreqOpt3,
        /// <summary>&lt;FreqOpt&gt; ::= &lt;SetClause&gt;</summary>
        FreqOpt4,
        /// <summary>&lt;FreqOpt&gt; ::= &lt;FreqOptNoWrap&gt;</summary>
        FreqOpt5,
        /// <summary>&lt;FreqOpt&gt; ::= &lt;FreqOptColumnSize&gt;</summary>
        FreqOpt6,
        /// <summary>&lt;FreqOpt&gt; ::= &lt;FreqOptPsuvar&gt;</summary>
        FreqOpt7,
        /// <summary>&lt;WeightOpt&gt; ::= WEIGHTVAR '=' Identifier</summary>
        WeightOpt,
        /// <summary>&lt;FreqOptStrata&gt; ::= STRATAVAR '=' &lt;IdentifierList&gt;</summary>
        FreqOptStrata,
        /// <summary>&lt;FreqOptNoWrap&gt; ::= NOWRAP</summary>
        FreqOptNoWrap,
        /// <summary>&lt;FreqOptColumnSize&gt; ::= COLUMNSIZE '=' DecLiteral</summary>
        FreqOptColumnSize,
        /// <summary>&lt;FreqOptPsuvar&gt; ::= PSUVAR '=' Identifier</summary>
        FreqOptPsuvar,
        /// <summary>&lt;Go_To_Variable_Statement&gt; ::= GOTO Identifier</summary>
        Go_To_Variable_Statement,
        /// <summary>&lt;Go_To_Page_Statement&gt; ::= GOTO DecLiteral</summary>
        Go_To_Page_Statement,
        /// <summary>&lt;Simple_Graph_Statement&gt; ::= GRAPH Identifier GRAPHTYPE '=' String</summary>
        Simple_Graph_Statement,
        /// <summary>&lt;Strata_Var_Graph_Statement&gt; ::= GRAPH &lt;IdentifierList&gt; '*' Identifier GRAPHTYPE '=' String STRATAVAR '=' Identifier</summary>
        Strata_Var_Graph_Statement,
        /// <summary>&lt;Weight_Var_Graph_Statement&gt; ::= GRAPH Identifier '*' Identifier GRAPHTYPE '=' String WEIGHTVAR '=' Identifier</summary>
        Weight_Var_Graph_Statement,
        /// <summary>&lt;Stra_Wei_Var_Graph_Statement&gt; ::= GRAPH Identifier '*' &lt;IdentifierList&gt; GRAPHTYPE '=' String STRATAVAR '=' Identifier WEIGHTVAR '=' Identifier</summary>
        Stra_Wei_Var_Graph_Statement,
        /// <summary>&lt;Graph_Opt_1_Statement&gt; ::= GRAPH Identifier GRAPHTYPE '=' String &lt;XYTitleOpt&gt; TEMPLATE '=' File</summary>
        Graph_Opt_1_Statement,
        /// <summary>&lt;Graph_Opt_2_Statement&gt; ::= GRAPH Identifier GRAPHTYPE '=' String XTITLE '=' String YTITLE '=' String &lt;TemplateOpt&gt;</summary>
        Graph_Opt_2_Statement,
        /// <summary>&lt;Graph_Opt_3_Statement&gt; ::= GRAPH Identifier GRAPHTYPE '=' String &lt;DateFormatOpt&gt; INTERVAL '=' DecLiteral</summary>
        Graph_Opt_3_Statement,
        /// <summary>&lt;Graph_Opt_4_Statement&gt; ::= GRAPH Identifier GRAPHTYPE '=' String DATEFORMAT '=' String &lt;IntervalOpt&gt;</summary>
        Graph_Opt_4_Statement,
        /// <summary>&lt;Graph_Opt_5_Statement&gt; ::= GRAPH Identifier GRAPHTYPE '=' String THREED '=' Boolean</summary>
        Graph_Opt_5_Statement,
        /// <summary>&lt;Graph_Generic_Opt_Statement&gt; ::= GRAPH Identifier GRAPHTYPE '=' String &lt;GenericOpt&gt;</summary>
        Graph_Generic_Opt_Statement,
        /// <summary>&lt;DateFormatOpt&gt; ::= &lt;DefaultDateFormatOpt&gt;</summary>
        DateFormatOpt,
        /// <summary>&lt;DateFormatOpt&gt; ::= </summary>
        DateFormatOpt2,
        /// <summary>&lt;DefaultDateFormatOpt&gt; ::= DATEFORMAT '=' String</summary>
        DefaultDateFormatOpt,
        /// <summary>&lt;IntervalOpt&gt; ::= &lt;DefaultIntervalOpt&gt;</summary>
        IntervalOpt,
        /// <summary>&lt;IntervalOpt&gt; ::= </summary>
        IntervalOpt2,
        /// <summary>&lt;DefaultIntervalOpt&gt; ::= INTERVAL '=' DecLiteral</summary>
        DefaultIntervalOpt,
        /// <summary>&lt;XYTitleOpt&gt; ::= &lt;DefaultXYTitleOpt&gt;</summary>
        XYTitleOpt,
        /// <summary>&lt;XYTitleOpt&gt; ::= </summary>
        XYTitleOpt2,
        /// <summary>&lt;DefaultXYTitleOpt&gt; ::= XTITLE '=' String YTITLE '=' String</summary>
        DefaultXYTitleOpt,
        /// <summary>&lt;TemplateOpt&gt; ::= &lt;DefaultTemplateOpt&gt;</summary>
        TemplateOpt,
        /// <summary>&lt;TemplateOpt&gt; ::= </summary>
        TemplateOpt2,
        /// <summary>&lt;DefaultTemplateOpt&gt; ::= TEMPLATE '=' File</summary>
        DefaultTemplateOpt,
        /// <summary>&lt;GenericOpt&gt; ::= &lt;DefaultGenericOpt&gt;</summary>
        GenericOpt,
        /// <summary>&lt;GenericOpt&gt; ::= &lt;GenericOpt&gt; &lt;DefaultGenericOpt&gt;</summary>
        GenericOpt2,
        /// <summary>&lt;DefaultGenericOpt&gt; ::= Identifier '=' Identifier</summary>
        DefaultGenericOpt,
        /// <summary>&lt;Header_Title_String_Statement&gt; ::= HEADER DecLiteral &lt;TitleStringPart&gt;</summary>
        Header_Title_String_Statement,
        /// <summary>&lt;Header_Title_Font_Statement&gt; ::= HEADER DecLiteral &lt;TitleFontPart&gt;</summary>
        Header_Title_Font_Statement,
        /// <summary>&lt;Header_Title_String_And_Font_Statement&gt; ::= HEADER DecLiteral &lt;TitleStringPart&gt; &lt;TitleFontPart&gt;</summary>
        Header_Title_String_And_Font_Statement,
        /// <summary>&lt;TitleStringPart&gt; ::= &lt;DefaultTitleStringPart&gt;</summary>
        TitleStringPart,
        /// <summary>&lt;TitleStringPart&gt; ::= &lt;TitleStringPartWithAppend&gt;</summary>
        TitleStringPart2,
        /// <summary>&lt;DefaultTitleStringPart&gt; ::= String &lt;EffectList&gt;</summary>
        DefaultTitleStringPart,
        /// <summary>&lt;TitleStringPartWithAppend&gt; ::= String &lt;EffectList&gt; APPEND</summary>
        TitleStringPartWithAppend,
        /// <summary>&lt;Help_File_Statement&gt; ::= HELP File String</summary>
        Help_File_Statement,
        /// <summary>&lt;Simple_Help_Statement&gt; ::= HELP File Identifier</summary>
        Simple_Help_Statement,
        /// <summary>&lt;Hide_Some_Statement&gt; ::= HIDE &lt;IdentifierList&gt;</summary>
        Hide_Some_Statement,
        /// <summary>&lt;Hide_Except_Statement&gt; ::= HIDE '*' EXCEPT &lt;IdentifierList&gt;</summary>
        Hide_Except_Statement,
        /// <summary>&lt;If_Statement&gt; ::= IF &lt;Expression&gt; THEN NewLine &lt;Statements&gt; NewLine END</summary>
        If_Statement,
        /// <summary>&lt;If_Else_Statement&gt; ::= IF &lt;Expression&gt; THEN NewLine &lt;Statements&gt; NewLine ELSE NewLine &lt;Statements&gt; NewLine END</summary>
        If_Else_Statement,
        /// <summary>&lt;Simple_KM_Survival_Statement&gt; ::= KMSURVIVAL Identifier '=' Identifier '*' Identifier '(' Literal ')' &lt;KMSurvOpts&gt;</summary>
        Simple_KM_Survival_Statement,
        /// <summary>&lt;KM_Survival_Boolean_Statement&gt; ::= KMSURVIVAL Identifier '=' Identifier '*' Identifier '(' Boolean ')' &lt;KMSurvOpts&gt;</summary>
        KM_Survival_Boolean_Statement,
        /// <summary>&lt;KMSurvOpts&gt; ::= &lt;KMSurvOpt&gt;</summary>
        KMSurvOpts,
        /// <summary>&lt;KMSurvOpts&gt; ::= &lt;KMSurvOpt&gt; &lt;KMSurvOpts&gt;</summary>
        KMSurvOpts2,
        /// <summary>&lt;KMSurvOpt&gt; ::= &lt;KMSurvTimeOpt&gt;</summary>
        KMSurvOpt,
        /// <summary>&lt;KMSurvOpt&gt; ::= &lt;KMSurvGraphOpt&gt;</summary>
        KMSurvOpt2,
        /// <summary>&lt;KMSurvOpt&gt; ::= &lt;KMSurvOutOpt&gt;</summary>
        KMSurvOpt3,
        /// <summary>&lt;KMSurvOpt&gt; ::= &lt;KMSurvWeightOpt&gt;</summary>
        KMSurvOpt4,
        /// <summary>&lt;KMSurvOpt&gt; ::= &lt;KMSurvTitleOpt&gt;</summary>
        KMSurvOpt5,
        /// <summary>&lt;KMSurvTimeOpt&gt; ::= TIMEUNIT '=' String</summary>
        KMSurvTimeOpt,
        /// <summary>&lt;KMSurvGraphOpt&gt; ::= GRAPHTYPE '=' String</summary>
        KMSurvGraphOpt,
        /// <summary>&lt;KMSurvOutOpt&gt; ::= OUTTABLE '=' Identifier</summary>
        KMSurvOutOpt,
        /// <summary>&lt;KMSurvWeightOpt&gt; ::= WEIGHTVAR '=' Identifier</summary>
        KMSurvWeightOpt,
        /// <summary>&lt;KMSurvTitleOpt&gt; ::= TITLETEXT '=' String</summary>
        KMSurvTitleOpt,
        /// <summary>&lt;Simple_List_Statement&gt; ::= LIST &lt;ListOpt&gt;</summary>
        Simple_List_Statement,
        /// <summary>&lt;List_All_Statement&gt; ::= LIST '*' &lt;ListOpt&gt;</summary>
        List_All_Statement,
        /// <summary>&lt;List_Columns_Statement&gt; ::= LIST &lt;IdentifierList&gt; &lt;ListOpt&gt;</summary>
        List_Columns_Statement,
        /// <summary>&lt;List_All_Except_Statement&gt; ::= LIST '*' EXCEPT &lt;IdentifierList&gt; &lt;ListOpt&gt;</summary>
        List_All_Except_Statement,
        /// <summary>&lt;ListOpt&gt; ::= &lt;ListGridOpt&gt;</summary>
        ListOpt,
        /// <summary>&lt;ListOpt&gt; ::= &lt;ListUpdateOpt&gt;</summary>
        ListOpt2,
        /// <summary>&lt;ListOpt&gt; ::= &lt;ListHTMLOpt&gt;</summary>
        ListOpt3,
        /// <summary>&lt;ListGridOpt&gt; ::= GRIDTABLE</summary>
        ListGridOpt,
        /// <summary>&lt;ListUpdateOpt&gt; ::= UPDATE</summary>
        ListUpdateOpt,
        /// <summary>&lt;ListHTMLOpt&gt; ::= &lt;ListHTMLOptOneColumn&gt;</summary>
        ListHTMLOpt,
        /// <summary>&lt;ListHTMLOpt&gt; ::= &lt;ListHTMLOptTwoColumns&gt;</summary>
        ListHTMLOpt2,
        /// <summary>&lt;ListHTMLOpt&gt; ::= &lt;ListHTMLOptThreeColumns&gt;</summary>
        ListHTMLOpt3,
        /// <summary>&lt;ListHTMLOpt&gt; ::= &lt;ListHTMLOptNoImage&gt;</summary>
        ListHTMLOpt4,
        /// <summary>&lt;ListHTMLOpt&gt; ::= &lt;ListHTMLOptNowrap&gt;</summary>
        ListHTMLOpt5,
        /// <summary>&lt;ListHTMLOpt&gt; ::= &lt;ListHTMLOptLine&gt;</summary>
        ListHTMLOpt6,
        /// <summary>&lt;ListHTMLOpt&gt; ::= </summary>
        ListHTMLOpt7,
        /// <summary>&lt;ListHTMLOptOneColumn&gt; ::= &lt;ListHTMLOpt&gt; COULMNSIZE '=' DecLiteral</summary>
        ListHTMLOptOneColumn,
        /// <summary>&lt;ListHTMLOptTwoColumns&gt; ::= &lt;ListHTMLOpt&gt; COULMNSIZE '=' DecLiteral ',' DecLiteral</summary>
        ListHTMLOptTwoColumns,
        /// <summary>&lt;ListHTMLOptThreeColumns&gt; ::= &lt;ListHTMLOpt&gt; COULMNSIZE '=' DecLiteral ',' DecLiteral ',' DecLiteral</summary>
        ListHTMLOptThreeColumns,
        /// <summary>&lt;ListHTMLOptNoImage&gt; ::= &lt;ListHTMLOpt&gt; NOIMAGE</summary>
        ListHTMLOptNoImage,
        /// <summary>&lt;ListHTMLOptNowrap&gt; ::= &lt;ListHTMLOpt&gt; NOWRAP</summary>
        ListHTMLOptNowrap,
        /// <summary>&lt;ListHTMLOptLine&gt; ::= &lt;ListHTMLOpt&gt; LINENUMBERS</summary>
        ListHTMLOptLine,
        /// <summary>&lt;Logistic_Statement&gt; ::= LOGISTIC Identifier '=' &lt;TermList&gt; &lt;LogisticOpts&gt;</summary>
        Logistic_Statement,
        /// <summary>&lt;LogisticOpts&gt; ::= &lt;LogisticOpts&gt; &lt;LogisticOpt&gt;</summary>
        LogisticOpts,
        /// <summary>&lt;LogisticOpts&gt; ::= &lt;LogisticOpt&gt;</summary>
        LogisticOpts2,
        /// <summary>&lt;LogisticOpts&gt; ::= </summary>
        LogisticOpts3,
        /// <summary>&lt;LogisticOpt&gt; ::= &lt;LogisticTitleOpt&gt;</summary>
        LogisticOpt,
        /// <summary>&lt;LogisticOpt&gt; ::= &lt;WeightOpt&gt;</summary>
        LogisticOpt2,
        /// <summary>&lt;LogisticOpt&gt; ::= &lt;LogisticMatchOpt&gt;</summary>
        LogisticOpt3,
        /// <summary>&lt;LogisticOpt&gt; ::= &lt;LogisticPValPercentOpt&gt;</summary>
        LogisticOpt4,
        /// <summary>&lt;LogisticOpt&gt; ::= &lt;LogisticPValRealOpt&gt;</summary>
        LogisticOpt5,
        /// <summary>&lt;LogisticOpt&gt; ::= &lt;OutTableOpt&gt;</summary>
        LogisticOpt6,
        /// <summary>&lt;LogisticOpt&gt; ::= &lt;LogisticNoInterceptOpt&gt;</summary>
        LogisticOpt7,
        /// <summary>&lt;LogisticTitleOpt&gt; ::= TITLETEXT '=' String</summary>
        LogisticTitleOpt,
        /// <summary>&lt;LogisticMatchOpt&gt; ::= MATCHVAR '=' Identifier</summary>
        LogisticMatchOpt,
        /// <summary>&lt;LogisticPValPercentOpt&gt; ::= PVALUE '=' Percent</summary>
        LogisticPValPercentOpt,
        /// <summary>&lt;LogisticPValRealOpt&gt; ::= PVALUE '=' RealLiteral</summary>
        LogisticPValRealOpt,
        /// <summary>&lt;OutTableOpt&gt; ::= OUTTABLE '=' Identifier &lt;KeyVarList&gt;</summary>
        OutTableOpt,
        /// <summary>&lt;LogisticNoInterceptOpt&gt; ::= NOINTERCEPT</summary>
        LogisticNoInterceptOpt,
        /// <summary>&lt;BaseTerm&gt; ::= Identifier</summary>
        BaseTerm,
        /// <summary>&lt;BaseTerm&gt; ::= '(' Identifier ')'</summary>
        BaseTerm2,
        /// <summary>&lt;Term&gt; ::= &lt;BaseTerm&gt;</summary>
        Term,
        /// <summary>&lt;Term&gt; ::= &lt;Term&gt; '*' &lt;BaseTerm&gt;</summary>
        Term2,
        /// <summary>&lt;TermList&gt; ::= &lt;Term&gt;</summary>
        TermList,
        /// <summary>&lt;TermList&gt; ::= &lt;TermList&gt; &lt;Term&gt;</summary>
        TermList2,
        /// <summary>&lt;Map_AVG_Statement&gt; ::= MAP AVG '(' Identifier ')' Identifier '::' &lt;ReadOpt&gt; ':' Identifier</summary>
        Map_AVG_Statement,
        /// <summary>&lt;Map_Case_Statement&gt; ::= MAP 'CASE_BASED' '(' Identifier ')' Identifier '::' &lt;ReadOpt&gt; ':' Identifier</summary>
        Map_Case_Statement,
        /// <summary>&lt;Map_Sum_Statement&gt; ::= MAP SUM '(' Identifier ')' Identifier '::' &lt;ReadOpt&gt; ':' Identifier</summary>
        Map_Sum_Statement,
        /// <summary>&lt;Map_Count_Statement&gt; ::= MAP 'COUNT(' Identifier ')' Identifier '::' &lt;ReadOpt&gt; ':' Identifier</summary>
        Map_Count_Statement,
        /// <summary>&lt;Map_Min_Statement&gt; ::= MAP MIN '(' Identifier ')' Identifier '::' &lt;ReadOpt&gt; ':' Identifier</summary>
        Map_Min_Statement,
        /// <summary>&lt;Map_Max_Statement&gt; ::= MAP MAX '(' Identifier ')' Identifier '::' &lt;ReadOpt&gt; ':' Identifier</summary>
        Map_Max_Statement,
        /// <summary>&lt;Map_Opt_1_Statement&gt; ::= MAP SUM '(' Identifier ')' Identifier '::' &lt;ReadOpt&gt; ':' Identifier DENOMINATOR '=' Identifier</summary>
        Map_Opt_1_Statement,
        /// <summary>&lt;Map_Opt_2_Statement&gt; ::= MAP SUM '(' Identifier ')' Identifier '::' &lt;ReadOpt&gt; ':' Identifier OUTTABLE '=' Identifier DENOMINATOR '=' Identifier</summary>
        Map_Opt_2_Statement,
        /// <summary>&lt;Map_Opt_3_Statement&gt; ::= MAP SUM '(' Identifier ')' Identifier '::' &lt;ReadOpt&gt; ':' Identifier TITLETEXT '=' String</summary>
        Map_Opt_3_Statement,
        /// <summary>&lt;Map_Opt_4_Statement&gt; ::= MAP SUM '(' Identifier ')' Identifier '::' &lt;ReadOpt&gt; ':' Identifier TEMPLATE '=' String</summary>
        Map_Opt_4_Statement,
        /// <summary>&lt;Map_Opt_5_Statement&gt; ::= MAP SUM '(' Identifier ')' Identifier '::' &lt;ReadOpt&gt; ':' Identifier RUNSILENT</summary>
        Map_Opt_5_Statement,
        /// <summary>&lt;Match_Row_All_Statement&gt; ::= MATCH '*' Identifier &lt;MatchOpts&gt;</summary>
        Match_Row_All_Statement,
        /// <summary>&lt;Match_Row_Except_Statement&gt; ::= MATCH '*' EXCEPT &lt;IdentifierList&gt; Identifier &lt;MatchOpts&gt;</summary>
        Match_Row_Except_Statement,
        /// <summary>&lt;Match_Column_All_Statement&gt; ::= MATCH Identifier '*' &lt;MatchOpts&gt;</summary>
        Match_Column_All_Statement,
        /// <summary>&lt;Match_Column_Except_Statement&gt; ::= MATCH Identifier '*' EXCEPT &lt;IdentifierList&gt; &lt;MatchOpts&gt;</summary>
        Match_Column_Except_Statement,
        /// <summary>&lt;Match_Row_Column_Statement&gt; ::= MATCH Identifier Identifier &lt;MatchOpts&gt;</summary>
        Match_Row_Column_Statement,
        /// <summary>&lt;MatchOpts&gt; ::= &lt;MatchOpts&gt; &lt;MatchOpt&gt;</summary>
        MatchOpts,
        /// <summary>&lt;MatchOpts&gt; ::= &lt;MatchOpt&gt;</summary>
        MatchOpts2,
        /// <summary>&lt;MatchOpts&gt; ::= </summary>
        MatchOpts3,
        /// <summary>&lt;MatchOpt&gt; ::= WEIGHTVAR '=' Identifier</summary>
        MatchOpt,
        /// <summary>&lt;MatchOpt&gt; ::= MATCHVAR '=' &lt;IdentifierList&gt;</summary>
        MatchOpt2,
        /// <summary>&lt;MatchOpt&gt; ::= &lt;SetClause&gt;</summary>
        MatchOpt3,
        /// <summary>&lt;Simple_Means_Statement&gt; ::= MEANS Identifier &lt;FreqOpts&gt;</summary>
        Simple_Means_Statement,
        /// <summary>&lt;Row_All_Means_Statement&gt; ::= MEANS '*' &lt;FreqOpts&gt;</summary>
        Row_All_Means_Statement,
        /// <summary>&lt;Row_All_Column_Means_Statement&gt; ::= MEANS '*' Identifer &lt;FreqOpts&gt;</summary>
        Row_All_Column_Means_Statement,
        /// <summary>&lt;Row_Column_All_Means_Statement&gt; ::= MEANS Identifier '*' &lt;FreqOpts&gt;</summary>
        Row_Column_All_Means_Statement,
        /// <summary>&lt;Row_Column_Means_Statement&gt; ::= MEANS Identifier Identifier &lt;FreqOpts&gt;</summary>
        Row_Column_Means_Statement,
        /// <summary>&lt;Merge_Table_Statement&gt; ::= MERGE Identifier &lt;KeyDef&gt; &lt;MergeOpt&gt;</summary>
        Merge_Table_Statement,
        /// <summary>&lt;Merge_Db_Table_Statement&gt; ::= MERGE &lt;ReadOpt&gt; File ':' Identifier &lt;LinkName&gt; &lt;KeyDef&gt; &lt;MergeOpt&gt; NewLine &lt;FileSpec&gt;</summary>
        Merge_Db_Table_Statement,
        /// <summary>&lt;Merge_File_Statement&gt; ::= MERGE &lt;ReadOpt&gt; File &lt;LinkName&gt; &lt;KeyDef&gt; &lt;MergeOpt&gt; NewLine &lt;FileSpec&gt;</summary>
        Merge_File_Statement,
        /// <summary>&lt;Merge_Excel_File_Statement&gt; ::= MERGE &lt;ReadOpt&gt; File ExcelRange &lt;LinkName&gt; &lt;KeyDef&gt; &lt;MergeOpt&gt; NewLine &lt;FileSpec&gt;</summary>
        Merge_Excel_File_Statement,
        /// <summary>&lt;MergeOpt&gt; ::= APPEND</summary>
        MergeOpt,
        /// <summary>&lt;MergeOpt&gt; ::= UPDATE</summary>
        MergeOpt2,
        /// <summary>&lt;MergeOpt&gt; ::= RELATE</summary>
        MergeOpt3,
        /// <summary>&lt;MergeOpt&gt; ::= </summary>
        MergeOpt4,
        /// <summary>&lt;New_Page_Statement&gt; ::= NEWPAGE</summary>
        New_Page_Statement,
        /// <summary>&lt;New_Record_Statement&gt; ::= NewRecord</summary>
        New_Record_Statement,
        /// <summary>&lt;Simple_Print_Out_Statement&gt; ::= PRINTOUT</summary>
        Simple_Print_Out_Statement,
        /// <summary>&lt;File_Print_Out_Statement&gt; ::= PRINTOUT File</summary>
        File_Print_Out_Statement,
        /// <summary>&lt;Quit_Statement&gt; ::= QUIT</summary>
        Quit_Statement,
        /// <summary>&lt;Simple_Read_Statement&gt; ::= READ &lt;ReadOpt&gt; Identifier</summary>
        Simple_Read_Statement,
        /// <summary>&lt;Read_Epi_Statement&gt; ::= READ File ':' Identifier</summary>
        Read_Epi_Statement,
        /// <summary>&lt;Read_Epi_File_Spec_Statement&gt; ::= READ &lt;FileDataFormat&gt; File ':' Identifier NewLine &lt;FileSpec&gt;</summary>
        Read_Epi_File_Spec_Statement,
        /// <summary>&lt;Read_Sql_Statement&gt; ::= READ &lt;DbDataformat&gt; String ':' Identifier</summary>
        Read_Sql_Statement,
        /// <summary>&lt;Read_Excel_File_Statement&gt; ::= READ &lt;FileDataFormat&gt; File ExcelRange &lt;LinkName&gt; NewLine &lt;FileSpec&gt;</summary>
        Read_Excel_File_Statement,
        /// <summary>&lt;Read_Db_Table_Statement&gt; ::= READ &lt;FileDataFormat&gt; File ':' Identifier &lt;LinkName&gt; NewLine &lt;FileSpec&gt;</summary>
        Read_Db_Table_Statement,
        /// <summary>&lt;FileDataFormat&gt; ::= '"Epi7"'</summary>
        FileDataFormat,
        /// <summary>&lt;FileDataFormat&gt; ::= '"Epi2000"'</summary>
        FileDataFormat2,
        /// <summary>&lt;FileDataFormat&gt; ::= '"Excel 8.0"'</summary>
        FileDataFormat3,
        /// <summary>&lt;DbDataformat&gt; ::= '"SQL"'</summary>
        DbDataformat,
        /// <summary>&lt;DbDataformat&gt; ::= '"ODBC"'</summary>
        DbDataformat2,
        /// <summary>&lt;ReadOpt&gt; ::= String</summary>
        ReadOpt,
        /// <summary>&lt;ReadOpt&gt; ::= </summary>
        ReadOpt2,
        /// <summary>&lt;Recode_Statement&gt; ::= RECODE Identifier TO Identifier NewLine &lt;RecodeList&gt; END</summary>
        Recode_Statement,
        /// <summary>&lt;RecodeList&gt; ::= &lt;Recode&gt; NewLine</summary>
        RecodeList,
        /// <summary>&lt;RecodeList&gt; ::= &lt;RecodeList&gt; &lt;Recode&gt; NewLine</summary>
        RecodeList2,
        /// <summary>&lt;Recode&gt; ::= &lt;Recode_A&gt;</summary>
        Recode,
        /// <summary>&lt;Recode&gt; ::= &lt;Recode_B&gt;</summary>
        Recode2,
        /// <summary>&lt;Recode&gt; ::= &lt;Recode_C&gt;</summary>
        Recode3,
        /// <summary>&lt;Recode&gt; ::= &lt;Recode_D&gt;</summary>
        Recode4,
        /// <summary>&lt;Recode&gt; ::= &lt;Recode_E&gt;</summary>
        Recode5,
        /// <summary>&lt;Recode&gt; ::= &lt;Recode_F&gt;</summary>
        Recode6,
        /// <summary>&lt;Recode&gt; ::= &lt;Recode_G&gt;</summary>
        Recode7,
        /// <summary>&lt;Recode&gt; ::= &lt;Recode_H&gt;</summary>
        Recode8,
        /// <summary>&lt;Recode&gt; ::= &lt;Recode_I&gt;</summary>
        Recode9,
        /// <summary>&lt;Recode&gt; ::= &lt;Recode_J&gt;</summary>
        Recode10,
        /// <summary>&lt;Recode&gt; ::= &lt;Recode_K&gt;</summary>
        Recode11,
        /// <summary>&lt;Recode&gt; ::= &lt;Recode_L&gt;</summary>
        Recode12,
        /// <summary>&lt;Recode&gt; ::= &lt;Recode_M&gt;</summary>
        Recode13,
        /// <summary>&lt;Recode&gt; ::= &lt;Recode_N&gt;</summary>
        Recode14,
        /// <summary>&lt;Recode_A&gt; ::= &lt;Literal&gt; '-' &lt;Literal&gt; '=' &lt;Literal&gt;</summary>
        Recode_A,
        /// <summary>&lt;Recode_B&gt; ::= &lt;Literal&gt; '=' &lt;Literal&gt;</summary>
        Recode_B,
        /// <summary>&lt;Recode_C&gt; ::= Boolean '=' &lt;Literal&gt;</summary>
        Recode_C,
        /// <summary>&lt;Recode_D&gt; ::= LOVALUE '-' &lt;Literal&gt; '=' &lt;Literal&gt;</summary>
        Recode_D,
        /// <summary>&lt;Recode_E&gt; ::= &lt;Literal&gt; '-' HIVALUE '=' &lt;Literal&gt;</summary>
        Recode_E,
        /// <summary>&lt;Recode_F&gt; ::= LOVALUE '-' HIVALUE '=' &lt;Literal&gt;</summary>
        Recode_F,
        /// <summary>&lt;Recode_G&gt; ::= ELSE '=' &lt;Literal&gt;</summary>
        Recode_G,
        /// <summary>&lt;Recode_H&gt; ::= &lt;Literal&gt; '-' &lt;Literal&gt; '=' Boolean</summary>
        Recode_H,
        /// <summary>&lt;Recode_I&gt; ::= &lt;Literal&gt; '=' Boolean</summary>
        Recode_I,
        /// <summary>&lt;Recode_J&gt; ::= Boolean '=' Boolean</summary>
        Recode_J,
        /// <summary>&lt;Recode_K&gt; ::= LOVALUE '-' &lt;Literal&gt; '=' Boolean</summary>
        Recode_K,
        /// <summary>&lt;Recode_L&gt; ::= &lt;Literal&gt; '-' HIVALUE '=' Boolean</summary>
        Recode_L,
        /// <summary>&lt;Recode_M&gt; ::= LOVALUE '-' HIVALUE '=' Boolean</summary>
        Recode_M,
        /// <summary>&lt;Recode_N&gt; ::= ELSE '=' Boolean</summary>
        Recode_N,
        /// <summary>&lt;Regress_Statement&gt; ::= REGRESS Identifier '=' &lt;TermList&gt; &lt;LogisticOpts&gt;</summary>
        Regress_Statement,
        /// <summary>&lt;Relate_Epi_Table_Statement&gt; ::= RELATE Identifier &lt;JoinOpt&gt;</summary>
        Relate_Epi_Table_Statement,
        /// <summary>&lt;Relate_Table_Statement&gt; ::= RELATE Identifier &lt;KeyDef&gt; &lt;JoinOpt&gt;</summary>
        Relate_Table_Statement,
        /// <summary>&lt;Relate_Db_Table_Statement&gt; ::= RELATE &lt;ReadOpt&gt; File &lt;LinkName&gt; &lt;KeyDef&gt; &lt;JoinOpt&gt;</summary>
        Relate_Db_Table_Statement,
        /// <summary>&lt;Relate_Db_Table_With_Identifier_Statement&gt; ::= RELATE &lt;ReadOpt&gt; File ':' Identifier &lt;LinkName&gt; &lt;KeyDef&gt; &lt;JoinOpt&gt;</summary>
        Relate_Db_Table_With_Identifier_Statement,
        /// <summary>&lt;Relate_File_Statement&gt; ::= RELATE &lt;ReadOpt&gt; File &lt;LinkName&gt; &lt;KeyDef&gt; &lt;JoinOpt&gt; NewLine &lt;FileSpec&gt;</summary>
        Relate_File_Statement,
        /// <summary>&lt;Relate_Excel_File_Statement&gt; ::= RELATE &lt;ReadOpt&gt; File ExcelRange &lt;LinkName&gt; &lt;KeyDef&gt; &lt;JoinOpt&gt; NewLine &lt;FileSpec&gt;</summary>
        Relate_Excel_File_Statement,
        /// <summary>&lt;JoinOpt&gt; ::= MATCHING</summary>
        JoinOpt,
        /// <summary>&lt;JoinOpt&gt; ::= ALL</summary>
        JoinOpt2,
        /// <summary>&lt;JoinOpt&gt; ::= </summary>
        JoinOpt3,
        /// <summary>&lt;Simple_Routeout_Statement&gt; ::= ROUTEOUT File</summary>
        Simple_Routeout_Statement,
        /// <summary>&lt;Replace_Routeout_Statement&gt; ::= ROUTEOUT File REPLACE</summary>
        Replace_Routeout_Statement,
        /// <summary>&lt;Append_Routeout_Statement&gt; ::= ROUTEOUT File APPEND</summary>
        Append_Routeout_Statement,
        /// <summary>&lt;Simple_Run_Statement&gt; ::= RUNPGM Identifier</summary>
        Simple_Run_Statement,
        /// <summary>&lt;Run_String_Statement&gt; ::= RUNPGM String</summary>
        Run_String_Statement,
        /// <summary>&lt;Run_File_PGM_Statement&gt; ::= RUNPGM File</summary>
        Run_File_PGM_Statement,
        /// <summary>&lt;Run_PGM_In_Db_Statement&gt; ::= RUNPGM File ':' Identifier</summary>
        Run_PGM_In_Db_Statement,
        /// <summary>&lt;Select_Statement&gt; ::= SELECT &lt;Expression&gt;</summary>
        Select_Statement,
        /// <summary>&lt;Set_Statement&gt; ::= SET &lt;SetList&gt;</summary>
        Set_Statement,
        /// <summary>&lt;SetClause&gt; ::= STATISTICS '=' &lt;StatisticsOption&gt;</summary>
        SetClause,
        /// <summary>&lt;SetClause&gt; ::= PROCESS '=' &lt;ProcessOption&gt;</summary>
        SetClause2,
        /// <summary>&lt;SetClause&gt; ::= PROCESS '=' Identifier</summary>
        SetClause3,
        /// <summary>&lt;SetClause&gt; ::= Boolean '=' String</summary>
        SetClause4,
        /// <summary>&lt;SetClause&gt; ::= YN '=' String ',' String ',' String</summary>
        SetClause5,
        /// <summary>&lt;SetClause&gt; ::= DELETED '=' &lt;DeletedOption&gt;</summary>
        SetClause6,
        /// <summary>&lt;SetClause&gt; ::= PERCENTS '=' &lt;OnOff&gt;</summary>
        SetClause7,
        /// <summary>&lt;SetClause&gt; ::= MISSING '=' &lt;OnOff&gt;</summary>
        SetClause8,
        /// <summary>&lt;SetClause&gt; ::= IGNORE '=' &lt;OnOff&gt;</summary>
        SetClause9,
        /// <summary>&lt;SetClause&gt; ::= SELECT '=' &lt;OnOff&gt;</summary>
        SetClause10,
        /// <summary>&lt;SetClause&gt; ::= FREQGRAPH '=' &lt;OnOff&gt;</summary>
        SetClause11,
        /// <summary>&lt;SetClause&gt; ::= HYPERLINKS '=' &lt;OnOff&gt;</summary>
        SetClause12,
        /// <summary>&lt;SetClause&gt; ::= SHOWPROMPTS '=' &lt;OnOff&gt;</summary>
        SetClause13,
        /// <summary>&lt;SetClause&gt; ::= TABLES '=' &lt;OnOff&gt;</summary>
        SetClause14,
        /// <summary>&lt;SetClause&gt; ::= USEBROWSER '=' &lt;OnOff&gt;</summary>
        SetClause15,
        /// <summary>&lt;SetList&gt; ::= &lt;SetList&gt; &lt;SetClause&gt;</summary>
        SetList,
        /// <summary>&lt;SetList&gt; ::= &lt;SetClause&gt;</summary>
        SetList2,
        /// <summary>&lt;OnOff&gt; ::= ON</summary>
        OnOff,
        /// <summary>&lt;OnOff&gt; ::= OFF</summary>
        OnOff2,
        /// <summary>&lt;OnOff&gt; ::= Boolean</summary>
        OnOff3,
        /// <summary>&lt;DeletedOption&gt; ::= &lt;OnOff&gt;</summary>
        DeletedOption,
        /// <summary>&lt;DeletedOption&gt; ::= Identifier</summary>
        DeletedOption2,
        /// <summary>&lt;StatisticsOption&gt; ::= NONE</summary>
        StatisticsOption,
        /// <summary>&lt;StatisticsOption&gt; ::= MINIMAL</summary>
        StatisticsOption2,
        /// <summary>&lt;StatisticsOption&gt; ::= INTERMEDIATE</summary>
        StatisticsOption3,
        /// <summary>&lt;StatisticsOption&gt; ::= COMPLETE</summary>
        StatisticsOption4,
        /// <summary>&lt;ProcessOption&gt; ::= UNDELETED</summary>
        ProcessOption,
        /// <summary>&lt;ProcessOption&gt; ::= DELETED</summary>
        ProcessOption2,
        /// <summary>&lt;ProcessOption&gt; ::= BOTH</summary>
        ProcessOption3,
        /// <summary>&lt;Sort_Statement&gt; ::= SORT &lt;SortList&gt;</summary>
        Sort_Statement,
        /// <summary>&lt;SortOpt&gt; ::= ASCENDING</summary>
        SortOpt,
        /// <summary>&lt;SortOpt&gt; ::= DESCENDING</summary>
        SortOpt2,
        /// <summary>&lt;SortOpt&gt; ::= </summary>
        SortOpt3,
        /// <summary>&lt;Sort&gt; ::= Identifier &lt;SortOpt&gt;</summary>
        Sort,
        /// <summary>&lt;SortList&gt; ::= &lt;SortList&gt; &lt;Sort&gt;</summary>
        SortList,
        /// <summary>&lt;SortList&gt; ::= &lt;Sort&gt;</summary>
        SortList2,
        /// <summary>&lt;Summarize_Statement&gt; ::= SUMMARIZE &lt;AggregateList&gt; TO Identifier &lt;SummarizeOpt&gt;</summary>
        Summarize_Statement,
        /// <summary>&lt;SummarizeOpt&gt; ::= &lt;SummarizeOpt&gt; STRATAVAR '=' &lt;IdentifierList&gt;</summary>
        SummarizeOpt,
        /// <summary>&lt;SummarizeOpt&gt; ::= &lt;SummarizeOpt&gt; WEIGHTVAR '=' Identifier</summary>
        SummarizeOpt2,
        /// <summary>&lt;SummarizeOpt&gt; ::= </summary>
        SummarizeOpt3,
        /// <summary>&lt;AggregateVariableElement&gt; ::= Identifier '::' &lt;AggregateElement&gt;</summary>
        AggregateVariableElement,
        /// <summary>&lt;AggregateElement&gt; ::= Identifier '(' Identifier ')'</summary>
        AggregateElement,
        /// <summary>&lt;AggregateElement&gt; ::= Identifier '(' ')'</summary>
        AggregateElement2,
        /// <summary>&lt;AggregateList&gt; ::= &lt;AggregateVariableElement&gt;</summary>
        AggregateList,
        /// <summary>&lt;AggregateList&gt; ::= &lt;AggregateList&gt; &lt;AggregateVariableElement&gt;</summary>
        AggregateList2,
        /// <summary>&lt;AggregateList&gt; ::= &lt;AggregateList&gt; ',' &lt;AggregateVariableElement&gt;</summary>
        AggregateList3,
        /// <summary>&lt;Simple_Tables_Statement&gt; ::= TABLES '*' &lt;FreqOpts&gt;</summary>
        Simple_Tables_Statement,
        /// <summary>&lt;Tables_One_Variable_Statement&gt; ::= TABLES Identifier &lt;FreqOpts&gt;</summary>
        Tables_One_Variable_Statement,
        /// <summary>&lt;Row_All_Tables_Statement&gt; ::= TABLES '*' Identifier &lt;FreqOpts&gt;</summary>
        Row_All_Tables_Statement,
        /// <summary>&lt;Row_Except_Tables_Statement&gt; ::= TABLES '*' EXCEPT &lt;IdentifierList&gt; Indentifier &lt;FreqOpts&gt;</summary>
        Row_Except_Tables_Statement,
        /// <summary>&lt;Column_All_Tables_Statement&gt; ::= TABLES Identifier '*' &lt;FreqOpts&gt;</summary>
        Column_All_Tables_Statement,
        /// <summary>&lt;Column_Except_Tables_Statement&gt; ::= TABLES Identifier '*' EXCEPT &lt;IdentifierList&gt; &lt;FreqOpts&gt;</summary>
        Column_Except_Tables_Statement,
        /// <summary>&lt;Row_Column_Tables_Statement&gt; ::= TABLES Identifier Identifier &lt;FreqOpts&gt;</summary>
        Row_Column_Tables_Statement,
        /// <summary>&lt;Type_Out_String_Statement&gt; ::= TYPEOUT String &lt;EffectList&gt;</summary>
        Type_Out_String_Statement,
        /// <summary>&lt;Type_Out_File_Statement&gt; ::= TYPEOUT File</summary>
        Type_Out_File_Statement,
        /// <summary>&lt;Type_Out_String_With_Font_Statement&gt; ::= TYPEOUT String &lt;EffectList&gt; &lt;TitleFontPart&gt;</summary>
        Type_Out_String_With_Font_Statement,
        /// <summary>&lt;Type_Out_File_With_Font_Statement&gt; ::= TYPEOUT File &lt;EffectList&gt; &lt;TitleFontPart&gt;</summary>
        Type_Out_File_With_Font_Statement,
        /// <summary>&lt;Simple_Undefine_Statement&gt; ::= UNDEFINE Identifier</summary>
        Simple_Undefine_Statement,
        /// <summary>&lt;All_Standard_Undefine_Statement&gt; ::= UNDEFINE '*'</summary>
        All_Standard_Undefine_Statement,
        /// <summary>&lt;All_Global_Undefine_Statement&gt; ::= UNDEFINE '*' GLOBAL</summary>
        All_Global_Undefine_Statement,
        /// <summary>&lt;Undelete_All_Statement&gt; ::= UNDELETE '*' &lt;Undelete_Option&gt;</summary>
        Undelete_All_Statement,
        /// <summary>&lt;Undelete_Selected_Statement&gt; ::= UNDELETE &lt;Expression&gt; &lt;Undelete_Option&gt;</summary>
        Undelete_Selected_Statement,
        /// <summary>&lt;Undelete_Option&gt; ::= RUNSILENT</summary>
        Undelete_Option,
        /// <summary>&lt;Undelete_Option&gt; ::= </summary>
        Undelete_Option2,
        /// <summary>&lt;Unhide_Some_Statement&gt; ::= UNHIDE &lt;IdentifierList&gt;</summary>
        Unhide_Some_Statement,
        /// <summary>&lt;Unhide_Except_Statement&gt; ::= UNHIDE '*' EXCEPT &lt;IdentifierList&gt;</summary>
        Unhide_Except_Statement,
        /// <summary>&lt;Write_All_Statement&gt; ::= WRITE &lt;WriteMode&gt; &lt;FileDataFormat&gt; &lt;OutTarget&gt; '*'</summary>
        Write_All_Statement,
        /// <summary>&lt;Write_Some_Statement&gt; ::= WRITE &lt;WriteMode&gt; &lt;FileDataFormat&gt; &lt;OutTarget&gt; &lt;IdentifierList&gt;</summary>
        Write_Some_Statement,
        /// <summary>&lt;Write_Except_Statement&gt; ::= WRITE &lt;WriteMode&gt; &lt;FileDataFormat&gt; &lt;OutTarget&gt; '*' EXCEPT &lt;IdentifierList&gt;</summary>
        Write_Except_Statement,
        /// <summary>&lt;WriteMode&gt; ::= APPEND</summary>
        WriteMode,
        /// <summary>&lt;WriteMode&gt; ::= REPLACE</summary>
        WriteMode2,
        /// <summary>&lt;WriteMode&gt; ::= </summary>
        WriteMode3,
        /// <summary>&lt;OutTarget&gt; ::= Identifier</summary>
        OutTarget,
        /// <summary>&lt;OutTarget&gt; ::= File ':' Identifier</summary>
        OutTarget2,
        /// <summary>&lt;OutTarget&gt; ::= File</summary>
        OutTarget3,
        /// <summary>&lt;Expr List&gt; ::= &lt;Expression&gt; ',' &lt;Expr List&gt;</summary>
        Expr_List,
        /// <summary>&lt;Expr List&gt; ::= &lt;Expression&gt;</summary>
        Expr_List2,
        /// <summary>&lt;Expression&gt; ::= &lt;And Exp&gt; OR &lt;Expression&gt;</summary>
        Expression,
        /// <summary>&lt;Expression&gt; ::= &lt;And Exp&gt; XOR &lt;Expression&gt;</summary>
        Expression2,
        /// <summary>&lt;Expression&gt; ::= &lt;And Exp&gt;</summary>
        Expression3,
        /// <summary>&lt;And Exp&gt; ::= &lt;Not Exp&gt; AND &lt;And Exp&gt;</summary>
        And_Exp,
        /// <summary>&lt;And Exp&gt; ::= &lt;Not Exp&gt;</summary>
        And_Exp2,
        /// <summary>&lt;Not Exp&gt; ::= NOT &lt;Compare Exp&gt;</summary>
        Not_Exp,
        /// <summary>&lt;Not Exp&gt; ::= &lt;Compare Exp&gt;</summary>
        Not_Exp2,
        /// <summary>&lt;Compare Exp&gt; ::= &lt;Concat Exp&gt; LIKE String</summary>
        Compare_Exp,
        /// <summary>&lt;Compare Exp&gt; ::= &lt;Concat Exp&gt; '=' &lt;Compare Exp&gt;</summary>
        Compare_Exp2,
        /// <summary>&lt;Compare Exp&gt; ::= &lt;Concat Exp&gt; '&lt;&gt;' &lt;Compare Exp&gt;</summary>
        Compare_Exp3,
        /// <summary>&lt;Compare Exp&gt; ::= &lt;Concat Exp&gt; '&gt;' &lt;Compare Exp&gt;</summary>
        Compare_Exp4,
        /// <summary>&lt;Compare Exp&gt; ::= &lt;Concat Exp&gt; '&gt;=' &lt;Compare Exp&gt;</summary>
        Compare_Exp5,
        /// <summary>&lt;Compare Exp&gt; ::= &lt;Concat Exp&gt; '&lt;' &lt;Compare Exp&gt;</summary>
        Compare_Exp6,
        /// <summary>&lt;Compare Exp&gt; ::= &lt;Concat Exp&gt; '&lt;=' &lt;Compare Exp&gt;</summary>
        Compare_Exp7,
        /// <summary>&lt;Compare Exp&gt; ::= &lt;Concat Exp&gt;</summary>
        Compare_Exp8,
        /// <summary>&lt;Concat Exp&gt; ::= &lt;Add Exp&gt; '&amp;' &lt;Concat Exp&gt;</summary>
        Concat_Exp,
        /// <summary>&lt;Concat Exp&gt; ::= &lt;Add Exp&gt;</summary>
        Concat_Exp2,
        /// <summary>&lt;Add Exp&gt; ::= &lt;Mult Exp&gt; '+' &lt;Add Exp&gt;</summary>
        Add_Exp,
        /// <summary>&lt;Add Exp&gt; ::= &lt;Mult Exp&gt; '-' &lt;Add Exp&gt;</summary>
        Add_Exp2,
        /// <summary>&lt;Add Exp&gt; ::= &lt;Mult Exp&gt;</summary>
        Add_Exp3,
        /// <summary>&lt;Mult Exp&gt; ::= &lt;Pow Exp&gt; '*' &lt;Mult Exp&gt;</summary>
        Mult_Exp,
        /// <summary>&lt;Mult Exp&gt; ::= &lt;Pow Exp&gt; '/' &lt;Mult Exp&gt;</summary>
        Mult_Exp2,
        /// <summary>&lt;Mult Exp&gt; ::= &lt;Pow Exp&gt; MOD &lt;Mult Exp&gt;</summary>
        Mult_Exp3,
        /// <summary>&lt;Mult Exp&gt; ::= &lt;Pow Exp&gt; '%' &lt;Mult Exp&gt;</summary>
        Mult_Exp4,
        /// <summary>&lt;Mult Exp&gt; ::= &lt;Pow Exp&gt;</summary>
        Mult_Exp5,
        /// <summary>&lt;Pow Exp&gt; ::= &lt;Negate Exp&gt; '^' &lt;Negate Exp&gt;</summary>
        Pow_Exp,
        /// <summary>&lt;Pow Exp&gt; ::= &lt;Negate Exp&gt;</summary>
        Pow_Exp2,
        /// <summary>&lt;Negate Exp&gt; ::= '-' &lt;Value&gt;</summary>
        Negate_Exp,
        /// <summary>&lt;Negate Exp&gt; ::= &lt;Value&gt;</summary>
        Negate_Exp2,
        /// <summary>&lt;Value&gt; ::= Identifier</summary>
        Value,
        /// <summary>&lt;Value&gt; ::= &lt;Literal&gt;</summary>
        Value2,
        /// <summary>&lt;Value&gt; ::= Boolean</summary>
        Value3,
        /// <summary>&lt;Value&gt; ::= '(' &lt;Expr List&gt; ')'</summary>
        Value4,
        /// <summary>&lt;LinkName&gt; ::= LINKNAME '=' Identifier</summary>
        LinkName,
        /// <summary>&lt;LinkName&gt; ::= </summary>
        LinkName2,
        /// <summary>&lt;FileSpec&gt; ::= &lt;FilespecKey&gt;</summary>
        FileSpec,
        /// <summary>&lt;FileSpec&gt; ::= &lt;FileSpecField&gt;</summary>
        FileSpec2,
        /// <summary>&lt;FileSpec&gt; ::= &lt;FileSpecKeyAndField&gt;</summary>
        FileSpec3,
        /// <summary>&lt;FileSpec&gt; ::= </summary>
        FileSpec4,
        /// <summary>&lt;FilespecKey&gt; ::= FILESPEC &lt;FileSpecKeyList&gt; END</summary>
        FilespecKey,
        /// <summary>&lt;FileSpecField&gt; ::= &lt;FileSpecFieldListEnd&gt;</summary>
        FileSpecField,
        /// <summary>&lt;FileSpecField&gt; ::= &lt;FileSpecFieldLiteral&gt;</summary>
        FileSpecField2,
        /// <summary>&lt;FileSpecFieldListEnd&gt; ::= FILESPEC NewLine &lt;FileSpecFieldList&gt; END</summary>
        FileSpecFieldListEnd,
        /// <summary>&lt;FileSpecFieldLiteral&gt; ::= Identifier DecLiteral '-' DecLiteral Identifier</summary>
        FileSpecFieldLiteral,
        /// <summary>&lt;FileSpecKeyAndField&gt; ::= FILESPEC &lt;FileSpecKeyList&gt; &lt;FileSpecFieldList&gt; END</summary>
        FileSpecKeyAndField,
        /// <summary>&lt;FileSpecKeyList&gt; ::= &lt;FileSpecKeyDef&gt;</summary>
        FileSpecKeyList,
        /// <summary>&lt;FileSpecKeyList&gt; ::= &lt;FileSpecKeyDef&gt; &lt;FileSpecKeyList&gt;</summary>
        FileSpecKeyList2,
        /// <summary>&lt;FileSpecKeyDef&gt; ::= &lt;FileSpecNumericKey&gt;</summary>
        FileSpecKeyDef,
        /// <summary>&lt;FileSpecKeyDef&gt; ::= &lt;FileSpecStringKey&gt;</summary>
        FileSpecKeyDef2,
        /// <summary>&lt;FileSpecNumericKey&gt; ::= Identifier '=' &lt;Number&gt;</summary>
        FileSpecNumericKey,
        /// <summary>&lt;FileSpecStringKey&gt; ::= Identifier '=' &lt;StringList&gt;</summary>
        FileSpecStringKey,
        /// <summary>&lt;FileSpecFieldSingle&gt; ::= Identifier DecLiteral Identifier</summary>
        FileSpecFieldSingle,
        /// <summary>&lt;FileSpecFieldDef&gt; ::= &lt;FileSpecField&gt;</summary>
        FileSpecFieldDef,
        /// <summary>&lt;FileSpecFieldDef&gt; ::= &lt;FileSpecFieldSingle&gt;</summary>
        FileSpecFieldDef2,
        /// <summary>&lt;FileSpecFieldList&gt; ::= &lt;FileSpecFieldDef&gt;</summary>
        FileSpecFieldList,
        /// <summary>&lt;FileSpecFieldList&gt; ::= &lt;FileSpecFieldDef&gt; &lt;FileSpecFieldList&gt;</summary>
        FileSpecFieldList2,
        /// <summary>&lt;UndefExpression&gt; ::= Literal</summary>
        UndefExpression,
        /// <summary>&lt;UndefExpression&gt; ::= Identifier</summary>
        UndefExpression2,
        /// <summary>&lt;UndefExpression&gt; ::= '-' '(' &lt;UndefExpression&gt; ')'</summary>
        UndefExpression3,
        /// <summary>&lt;UndefExpression&gt; ::= '-' Identifier</summary>
        UndefExpression4,
        /// <summary>&lt;UndefExpression&gt; ::= '(' &lt;UndefExpression&gt; ')'</summary>
        UndefExpression5,
        /// <summary>&lt;EffectLeft&gt; ::= Identifier</summary>
        EffectLeft,
        /// <summary>&lt;EffectLeft&gt; ::= &lt;EffectLeft&gt; ',' Identifier</summary>
        EffectLeft2,
        /// <summary>&lt;EffectList&gt; ::= '(' &lt;EffectLeft&gt; ')'</summary>
        EffectList,
        /// <summary>&lt;EffectList&gt; ::= </summary>
        EffectList2,
        /// <summary>&lt;TitleFontPart&gt; ::= TEXTFONT &lt;Color&gt; DecLiteral</summary>
        TitleFontPart,
        /// <summary>&lt;TitleFontPart&gt; ::= TEXTFONT &lt;Color&gt;</summary>
        TitleFontPart2,
        /// <summary>&lt;TitleFontPart&gt; ::= TEXTFONT DecLiteral</summary>
        TitleFontPart22,
        /// <summary>&lt;Color&gt; ::= Identifier</summary>
        Color,
        /// <summary>&lt;Color&gt; ::= HexLiteral</summary>
        Color2,
        /// <summary>&lt;KeyExpr&gt; ::= &lt;KeyExprSimple&gt;</summary>
        KeyExpr,
        /// <summary>&lt;KeyExpr&gt; ::= &lt;KeyExprIdentifier&gt;</summary>
        KeyExpr2,
        /// <summary>&lt;KeyExprSimple&gt; ::= &lt;Expression&gt; '::' &lt;UndefExpression&gt;</summary>
        KeyExprSimple,
        /// <summary>&lt;KeyExprIdentifier&gt; ::= Identifier '::' &lt;UndefExpression&gt;</summary>
        KeyExprIdentifier,
        /// <summary>&lt;KeyDef&gt; ::= &lt;KeyExpr&gt;</summary>
        KeyDef,
        /// <summary>&lt;KeyDef&gt; ::= &lt;KeyDef&gt; AND &lt;KeyExpr&gt;</summary>
        KeyDef2,
        /// <summary>&lt;KeyVarList&gt; ::= KEYVARS '=' &lt;IdentifierList&gt;</summary>
        KeyVarList,
        /// <summary>&lt;KeyVarList&gt; ::= </summary>
        KeyVarList2,
        /// <summary>&lt;Menu_Statement&gt; ::= &lt;Menu_Block&gt;</summary>
        Menu_Statement,
        /// <summary>&lt;Menu_Statement&gt; ::= &lt;Menu_Single_Statement&gt;</summary>
        Menu_Statement2,
        /// <summary>&lt;Menu_Block&gt; ::= MENU &lt;Words&gt; NewLine BEGIN NewLine &lt;Statements&gt; NewLine END</summary>
        Menu_Block,
        /// <summary>&lt;Menu_Single_Statement&gt; ::= MENU &lt;Words&gt;</summary>
        Menu_Single_Statement,
        /// <summary>&lt;Menu_Empty_Block&gt; ::= MENU &lt;Words&gt; NewLine BEGIN NewLine END</summary>
        Menu_Empty_Block,
        /// <summary>&lt;Menu_Item_Block_Name_Statement&gt; ::= MENUITEM String ',' Identifier</summary>
        Menu_Item_Block_Name_Statement,
        /// <summary>&lt;Menu_Item_Separator_Statement&gt; ::= MENUITEM SEPARATOR</summary>
        Menu_Item_Separator_Statement,
        /// <summary>&lt;Button_Offset_Size_1_Statement&gt; ::= BUTTON String ',' Identifier ',' DecLiteral ',' DecLiteral ',' DecLiteral ',' DecLiteral ',' String</summary>
        Button_Offset_Size_1_Statement,
        /// <summary>&lt;Button_Offset_Size_2_Statement&gt; ::= BUTTON String ',' Identifier ',' DecLiteral ',' DecLiteral ',' DecLiteral ',' DecLiteral</summary>
        Button_Offset_Size_2_Statement,
        /// <summary>&lt;Button_Offset_1_Statement&gt; ::= BUTTON String ',' Identifier ',' DecLiteral ',' DecLiteral ',' String</summary>
        Button_Offset_1_Statement,
        /// <summary>&lt;Button_Offset_2_Statement&gt; ::= BUTTON String ',' Identifier ',' DecLiteral ',' DecLiteral</summary>
        Button_Offset_2_Statement,
        /// <summary>&lt;Offset&gt; ::= DecLiteral ',' DecLiteral</summary>
        Offset,
        /// <summary>&lt;Popup_Statement&gt; ::= &lt;Popup_Block&gt;</summary>
        Popup_Statement,
        /// <summary>&lt;Popup_Statement&gt; ::= &lt;Popup_Single_Statement&gt;</summary>
        Popup_Statement2,
        /// <summary>&lt;Popup_Block&gt; ::= POPUP String NewLine BEGIN NewLine &lt;Statements&gt; NewLine END</summary>
        Popup_Block,
        /// <summary>&lt;Popup_Single_Statement&gt; ::= POPUP String</summary>
        Popup_Single_Statement,
        /// <summary>&lt;Popup_Empty_Block&gt; ::= POPUP String NewLine BEGIN NewLine END</summary>
        Popup_Empty_Block,
        /// <summary>&lt;Command_Block&gt; ::= Identifier NewLine BEGIN NewLine &lt;Statements&gt; NewLine END</summary>
        Command_Block,
        /// <summary>&lt;Empty_Command_Block&gt; ::= Identifier NewLine BEGIN NewLine END</summary>
        Empty_Command_Block,
        /// <summary>&lt;Screen_Text_Statement&gt; ::= SCREENTEXT String ',' &lt;Offset&gt; ',' DecLiteral ',' &lt;Color&gt; ',' &lt;FontStyle&gt;</summary>
        Screen_Text_Statement,
        /// <summary>&lt;FontStyle&gt; ::= Identifier ',' Identifier</summary>
        FontStyle,
        /// <summary>&lt;FontStyle&gt; ::= Identifier</summary>
        FontStyle2,
        /// <summary>&lt;Words&gt; ::= &lt;LiteralInc&gt;</summary>
        Words,
        /// <summary>&lt;Words&gt; ::= &lt;LiteralInc&gt; &lt;Words&gt;</summary>
        Words2,
        /// <summary>&lt;LiteralInc&gt; ::= DecLiteral</summary>
        LiteralInc,
        /// <summary>&lt;LiteralInc&gt; ::= Date</summary>
        LiteralInc2,
        /// <summary>&lt;LiteralInc&gt; ::= Identifier</summary>
        LiteralInc3,
        /// <summary>&lt;About_Statement&gt; ::= ABOUT</summary>
        About_Statement,
        /// <summary>&lt;Browser_Statement&gt; ::= &lt;Browser1Statement&gt;</summary>
        Browser_Statement,
        /// <summary>&lt;Browser_Statement&gt; ::= &lt;Browser2Statement&gt;</summary>
        Browser_Statement2,
        /// <summary>&lt;Browser_Statement&gt; ::= &lt;Browser3Statement&gt;</summary>
        Browser_Statement3,
        /// <summary>&lt;Browser1Statement&gt; ::= BROWSER &lt;FileName&gt; ',' DecLiteral ',' DecLiteral ',' DecLiteral ',' DecLiteral ',' EXITBUTTON</summary>
        Browser1Statement,
        /// <summary>&lt;Browser2Statement&gt; ::= BROWSER &lt;FileName&gt; ',' DecLiteral ',' DecLiteral ',' DecLiteral ',' DecLiteral</summary>
        Browser2Statement,
        /// <summary>&lt;Browser3Statement&gt; ::= BROWSER &lt;FileName&gt;</summary>
        Browser3Statement,
        /// <summary>&lt;Browser_Size_Statement&gt; ::= &lt;BrowserSize1Statement&gt;</summary>
        Browser_Size_Statement,
        /// <summary>&lt;Browser_Size_Statement&gt; ::= &lt;BrowserSize2Statement&gt;</summary>
        Browser_Size_Statement2,
        /// <summary>&lt;BrowserSize1Statement&gt; ::= BROWSERSIZE DecLiteral ',' DecLiteral ',' DecLiteral ',' DecLiteral ',' EXITBUTTON</summary>
        BrowserSize1Statement,
        /// <summary>&lt;BrowserSize2Statement&gt; ::= BROWSERSIZE DecLiteral ',' DecLiteral ',' DecLiteral ',' DecLiteral</summary>
        BrowserSize2Statement,
        /// <summary>&lt;Call_Statement&gt; ::= CALL Identifier</summary>
        Call_Statement,
        /// <summary>&lt;Menu_Dialog_Statement&gt; ::= &lt;DialogFormatStatement&gt;</summary>
        Menu_Dialog_Statement,
        /// <summary>&lt;Menu_Dialog_Statement&gt; ::= &lt;DialogListStatement&gt;</summary>
        Menu_Dialog_Statement2,
        /// <summary>&lt;Menu_Dialog_Statement&gt; ::= &lt;DialogButtonsStatement&gt;</summary>
        Menu_Dialog_Statement3,
        /// <summary>&lt;Menu_Dialog_Statement&gt; ::= &lt;DialogDateFormatStatement&gt;</summary>
        Menu_Dialog_Statement4,
        /// <summary>&lt;DialogFormatStatement&gt; ::= DIALOG String Identifier FORMAT '=' String ',' &lt;TitleOpt&gt;</summary>
        DialogFormatStatement,
        /// <summary>&lt;DialogListStatement&gt; ::= DIALOG String Identifier LIST '=' &lt;List&gt; &lt;TitleOpt&gt;</summary>
        DialogListStatement,
        /// <summary>&lt;DialogButtonsStatement&gt; ::= DIALOG String Identifier BUTTONS '=' &lt;List&gt; &lt;TitleOpt&gt;</summary>
        DialogButtonsStatement,
        /// <summary>&lt;DialogDateFormatStatement&gt; ::= DIALOG String Identifier DATEFORMAT '=' String ',' &lt;TitleOpt&gt;</summary>
        DialogDateFormatStatement,
        /// <summary>&lt;Menu_Execute_Statement&gt; ::= &lt;ExecuteIdentifierOptStatement&gt;</summary>
        Menu_Execute_Statement,
        /// <summary>&lt;Menu_Execute_Statement&gt; ::= &lt;ExecuteFile1Statement&gt;</summary>
        Menu_Execute_Statement2,
        /// <summary>&lt;Menu_Execute_Statement&gt; ::= &lt;ExecuteFile2Statement&gt;</summary>
        Menu_Execute_Statement3,
        /// <summary>&lt;Menu_Execute_Statement&gt; ::= &lt;ExecuteFile3Statement&gt;</summary>
        Menu_Execute_Statement4,
        /// <summary>&lt;Menu_Execute_Statement&gt; ::= &lt;ExecuteTwoFiles1Statement&gt;</summary>
        Menu_Execute_Statement5,
        /// <summary>&lt;Menu_Execute_Statement&gt; ::= &lt;ExecuteTwoFiles2Statement&gt;</summary>
        Menu_Execute_Statement6,
        /// <summary>&lt;Menu_Execute_Statement&gt; ::= &lt;ExecuteFileStringStatement&gt;</summary>
        Menu_Execute_Statement7,
        /// <summary>&lt;Menu_Execute_Statement&gt; ::= &lt;ExecuteWebStatement&gt;</summary>
        Menu_Execute_Statement8,
        /// <summary>&lt;ExecuteIdentifierOptStatement&gt; ::= EXECUTE &lt;IdentifierOpt&gt;</summary>
        ExecuteIdentifierOptStatement,
        /// <summary>&lt;ExecuteFile1Statement&gt; ::= EXECUTE &lt;PathSymbol&gt; Identifier &lt;IdentifierOpt&gt;</summary>
        ExecuteFile1Statement,
        /// <summary>&lt;ExecuteFile2Statement&gt; ::= EXECUTE &lt;PathSymbol&gt; &lt;FilePath&gt; Identifier &lt;IdentifierOpt&gt;</summary>
        ExecuteFile2Statement,
        /// <summary>&lt;ExecuteFile3Statement&gt; ::= EXECUTE '.\@@' Identifier '\' &lt;SimpleFile&gt;</summary>
        ExecuteFile3Statement,
        /// <summary>&lt;ExecuteTwoFiles1Statement&gt; ::= EXECUTE '.\@@' Identifier '\' &lt;SimpleFile&gt; ';' &lt;SimpleFile&gt;</summary>
        ExecuteTwoFiles1Statement,
        /// <summary>&lt;ExecuteTwoFiles2Statement&gt; ::= EXECUTE &lt;PathSymbol&gt; &lt;FilePath&gt; Identifier &lt;IdentifierOpt&gt; ';' &lt;PathSymbol&gt; Identifier '.' Identifier</summary>
        ExecuteTwoFiles2Statement,
        /// <summary>&lt;ExecuteFileStringStatement&gt; ::= EXECUTE &lt;IdentifierOpt&gt; String</summary>
        ExecuteFileStringStatement,
        /// <summary>&lt;ExecuteWebStatement&gt; ::= EXECUTE &lt;WebLink&gt;</summary>
        ExecuteWebStatement,
        /// <summary>&lt;List&gt; ::= String ';'</summary>
        List,
        /// <summary>&lt;List&gt; ::= String ';' &lt;List&gt;</summary>
        List2,
        /// <summary>&lt;File_Dialog_Statement&gt; ::= &lt;FileDialogStrStatement&gt;</summary>
        File_Dialog_Statement,
        /// <summary>&lt;File_Dialog_Statement&gt; ::= &lt;FileDialogVarStatement&gt;</summary>
        File_Dialog_Statement2,
        /// <summary>&lt;FileDialogStrStatement&gt; ::= FILEDIALOG Identifier ',' String</summary>
        FileDialogStrStatement,
        /// <summary>&lt;FileDialogVarStatement&gt; ::= FILEDIALOG Identifier</summary>
        FileDialogVarStatement,
        /// <summary>&lt;Get_Path_Statement&gt; ::= &lt;GetPathStrStatement&gt;</summary>
        Get_Path_Statement,
        /// <summary>&lt;Get_Path_Statement&gt; ::= &lt;GetPathVarStatement&gt;</summary>
        Get_Path_Statement2,
        /// <summary>&lt;GetPathStrStatement&gt; ::= GETPATH Identifier String</summary>
        GetPathStrStatement,
        /// <summary>&lt;GetPathVarStatement&gt; ::= GETPATH Identifier</summary>
        GetPathVarStatement,
        /// <summary>&lt;Link_Statement&gt; ::= &lt;SimpleLinkStatement&gt;</summary>
        Link_Statement,
        /// <summary>&lt;Link_Statement&gt; ::= &lt;CaptionLinkStatement&gt;</summary>
        Link_Statement2,
        /// <summary>&lt;SimpleLinkStatement&gt; ::= LINK &lt;FileCommaFile&gt;</summary>
        SimpleLinkStatement,
        /// <summary>&lt;CaptionLinkStatement&gt; ::= LINK &lt;FileName&gt; String ',' &lt;FileName&gt; String</summary>
        CaptionLinkStatement,
        /// <summary>&lt;Link_Remove_Statement&gt; ::= &lt;LinkRemoveStringStatement&gt;</summary>
        Link_Remove_Statement,
        /// <summary>&lt;Link_Remove_Statement&gt; ::= &lt;LinkRemoveAllStatement&gt;</summary>
        Link_Remove_Statement2,
        /// <summary>&lt;LinkRemoveStringStatement&gt; ::= LINKREMOVE String</summary>
        LinkRemoveStringStatement,
        /// <summary>&lt;LinkRemoveAllStatement&gt; ::= LINKREMOVE ALL</summary>
        LinkRemoveAllStatement,
        /// <summary>&lt;Menu_Command_Statement&gt; ::= &lt;MenuCommandStatement1&gt;</summary>
        Menu_Command_Statement,
        /// <summary>&lt;Menu_Command_Statement&gt; ::= &lt;MenuCommandStatement2&gt;</summary>
        Menu_Command_Statement2,
        /// <summary>&lt;Menu_Command_Statement&gt; ::= &lt;MenuCommandStatement3&gt;</summary>
        Menu_Command_Statement3,
        /// <summary>&lt;Menu_Command_Statement&gt; ::= &lt;MenuCommandStatement4&gt;</summary>
        Menu_Command_Statement4,
        /// <summary>&lt;MenuCommandStatement1&gt; ::= MENU Identifier '.' MNU &lt;FileName&gt; DecLiteral ',' DecLiteral ',' DecLiteral ',' DecLiteral</summary>
        MenuCommandStatement1,
        /// <summary>&lt;MenuCommandStatement2&gt; ::= MENU Identifier '.' MNU &lt;FileName&gt; DecLiteral ',' DecLiteral</summary>
        MenuCommandStatement2,
        /// <summary>&lt;MenuCommandStatement3&gt; ::= MENU Identifier '.' MNU &lt;FileName&gt;</summary>
        MenuCommandStatement3,
        /// <summary>&lt;MenuCommandStatement4&gt; ::= MENU Identifier '.' MNU</summary>
        MenuCommandStatement4,
        /// <summary>&lt;Move_Buttons_Statement&gt; ::= MOVEBUTTONS</summary>
        Move_Buttons_Statement,
        /// <summary>&lt;Picture_Statement&gt; ::= &lt;PictureOffsetStatement&gt;</summary>
        Picture_Statement,
        /// <summary>&lt;Picture_Statement&gt; ::= &lt;PictureFileStatement&gt;</summary>
        Picture_Statement2,
        /// <summary>&lt;Picture_Statement&gt; ::= &lt;PictureOnStatement&gt;</summary>
        Picture_Statement3,
        /// <summary>&lt;Picture_Statement&gt; ::= &lt;PictureOffStatement&gt;</summary>
        Picture_Statement4,
        /// <summary>&lt;PictureOffsetStatement&gt; ::= PICTURE &lt;FileName&gt; DecLiteral ',' DecLiteral ',' DecLiteral ',' DecLiteral</summary>
        PictureOffsetStatement,
        /// <summary>&lt;PictureFileStatement&gt; ::= PICTURE &lt;FileName&gt;</summary>
        PictureFileStatement,
        /// <summary>&lt;PictureOnStatement&gt; ::= PICTURE ON</summary>
        PictureOnStatement,
        /// <summary>&lt;PictureOffStatement&gt; ::= PICTURE OFF</summary>
        PictureOffStatement,
        /// <summary>&lt;Picture_Size_Statement&gt; ::= PICTURESIZE DecLiteral ',' DecLiteral ',' DecLiteral ',' DecLiteral</summary>
        Picture_Size_Statement,
        /// <summary>&lt;Repeat_Statement&gt; ::= REPEAT NewLine &lt;Statements&gt; NewLine UNTIL &lt;Expression&gt;</summary>
        Repeat_Statement,
        /// <summary>&lt;Menu_Replace_Statement&gt; ::= &lt;ReplaceWithCommaStatement&gt;</summary>
        Menu_Replace_Statement,
        /// <summary>&lt;Menu_Replace_Statement&gt; ::= &lt;ReplaceWithCommaTimesStatement&gt;</summary>
        Menu_Replace_Statement2,
        /// <summary>&lt;Menu_Replace_Statement&gt; ::= &lt;ReplaceWithFromToStatement&gt;</summary>
        Menu_Replace_Statement3,
        /// <summary>&lt;Menu_Replace_Statement&gt; ::= &lt;ReplaceWithFromToTimesStatement&gt;</summary>
        Menu_Replace_Statement4,
        /// <summary>&lt;ReplaceWithCommaStatement&gt; ::= REPLACE &lt;StringPair&gt; &lt;FileCommaFile&gt;</summary>
        ReplaceWithCommaStatement,
        /// <summary>&lt;ReplaceWithCommaTimesStatement&gt; ::= REPLACE &lt;StringPair&gt; &lt;FileCommaFile&gt; ',' DecLiteral</summary>
        ReplaceWithCommaTimesStatement,
        /// <summary>&lt;ReplaceWithFromToStatement&gt; ::= REPLACE &lt;StringPair&gt; &lt;FromFileToFile&gt;</summary>
        ReplaceWithFromToStatement,
        /// <summary>&lt;ReplaceWithFromToTimesStatement&gt; ::= REPLACE &lt;StringPair&gt; &lt;FromFileToFile&gt; ',' DecLiteral</summary>
        ReplaceWithFromToTimesStatement,
        /// <summary>&lt;FileCommaFile&gt; ::= &lt;FileName&gt; ',' &lt;FileName&gt;</summary>
        FileCommaFile,
        /// <summary>&lt;FromFileToFile&gt; ::= FROM &lt;FileName&gt; TO &lt;FileName&gt;</summary>
        FromFileToFile,
        /// <summary>&lt;StringPair&gt; ::= String ',' String ';' &lt;StringPair&gt;</summary>
        StringPair,
        /// <summary>&lt;StringPair&gt; ::= CharLiteral ',' CharLiteral ';' &lt;StringPair&gt;</summary>
        StringPair2,
        /// <summary>&lt;StringPair&gt; ::= String ',' String ';'</summary>
        StringPair22,
        /// <summary>&lt;StringPair&gt; ::= CharLiteral ',' CharLiteral ';'</summary>
        StringPair23,
        /// <summary>&lt;Set_Buttons_Statement&gt; ::= SETBUTTONS</summary>
        Set_Buttons_Statement,
        /// <summary>&lt;Set_DB_Version_Statement&gt; ::= SETDBVERSION</summary>
        Set_DB_Version_Statement,
        /// <summary>&lt;Set_DOS_Win_Statement&gt; ::= SETDOSWIN Identifier</summary>
        Set_DOS_Win_Statement,
        /// <summary>&lt;Set_Import_Year_Statement&gt; ::= SETIMPORTYEAR</summary>
        Set_Import_Year_Statement,
        /// <summary>&lt;Set_Language_Statement&gt; ::= SETLANGUAGE</summary>
        Set_Language_Statement,
        /// <summary>&lt;Set_Picture_Statement&gt; ::= SETPICTURE</summary>
        Set_Picture_Statement,
        /// <summary>&lt;Set_Work_Dir_Statement&gt; ::= &lt;SetWorkDir1Statement&gt;</summary>
        Set_Work_Dir_Statement,
        /// <summary>&lt;Set_Work_Dir_Statement&gt; ::= &lt;SetWorkDir2Statement&gt;</summary>
        Set_Work_Dir_Statement2,
        /// <summary>&lt;SetWorkDir1Statement&gt; ::= SETWORKDIR</summary>
        SetWorkDir1Statement,
        /// <summary>&lt;SetWorkDir2Statement&gt; ::= SETWORKDIR String</summary>
        SetWorkDir2Statement,
        /// <summary>&lt;Set_Ini_Dir_Statement&gt; ::= &lt;Set_Ini_Dir_1_Statement&gt;</summary>
        Set_Ini_Dir_Statement,
        /// <summary>&lt;Set_Ini_Dir_Statement&gt; ::= &lt;Set_Ini_Dir_2_Statement&gt;</summary>
        Set_Ini_Dir_Statement2,
        /// <summary>&lt;Set_Ini_Dir_1_Statement&gt; ::= SETINIDIR</summary>
        Set_Ini_Dir_1_Statement,
        /// <summary>&lt;Set_Ini_Dir_2_Statement&gt; ::= SETINIDIR String</summary>
        Set_Ini_Dir_2_Statement,
        /// <summary>&lt;Sys_Info_Statement&gt; ::= SYSINFO</summary>
        Sys_Info_Statement,
        /// <summary>&lt;Wait_For_Statement&gt; ::= WAITFOR &lt;FileName&gt; String</summary>
        Wait_For_Statement,
        /// <summary>&lt;Wait_For_Exit_Statement&gt; ::= WAITFOREXIT &lt;FileName&gt; String</summary>
        Wait_For_Exit_Statement,
        /// <summary>&lt;Wait_For_File_Exists_Statement&gt; ::= &lt;WaitForFileExistsStatement1&gt;</summary>
        Wait_For_File_Exists_Statement,
        /// <summary>&lt;Wait_For_File_Exists_Statement&gt; ::= &lt;WaitForFileExistsStatement2&gt;</summary>
        Wait_For_File_Exists_Statement2,
        /// <summary>&lt;WaitForFileExistsStatement1&gt; ::= WAITFORFILEEXISTS &lt;FileName&gt; ',' DecLiteral</summary>
        WaitForFileExistsStatement1,
        /// <summary>&lt;WaitForFileExistsStatement2&gt; ::= WAITFORFILEEXISTS &lt;FileName&gt;</summary>
        WaitForFileExistsStatement2,
        /// <summary>&lt;FileName&gt; ::= File</summary>
        FileName,
        /// <summary>&lt;FileName&gt; ::= String</summary>
        FileName2,
        /// <summary>&lt;FileName&gt; ::= &lt;Words&gt;</summary>
        FileName3,
        /// <summary>&lt;FileName&gt; ::= &lt;IdentifierOpt&gt;</summary>
        FileName22,
        /// <summary>&lt;SimpleFile&gt; ::= Identifier '.' Identifier</summary>
        SimpleFile,
        /// <summary>&lt;IdentifierOpt&gt; ::= &lt;SimpleFile&gt;</summary>
        IdentifierOpt,
        /// <summary>&lt;IdentifierOpt&gt; ::= &lt;IdentifierOpt&gt; '.' &lt;SimpleFile&gt;</summary>
        IdentifierOpt2,
        /// <summary>&lt;FilePath&gt; ::= '\' Identifier &lt;FilePath&gt;</summary>
        FilePath,
        /// <summary>&lt;FilePath&gt; ::= </summary>
        FilePath2,
        /// <summary>&lt;PathSymbol&gt; ::= '.\'</summary>
        PathSymbol,
        /// <summary>&lt;PathSymbol&gt; ::= '..\'</summary>
        PathSymbol2,
        /// <summary>&lt;PathSymbol&gt; ::= '\'</summary>
        PathSymbol3,
        /// <summary>&lt;PathSymbol&gt; ::= '@@'</summary>
        PathSymbol4,
        /// <summary>&lt;PathSymbol&gt; ::= '.\@@'</summary>
        PathSymbol5,
        /// <summary>&lt;PathSymbol&gt; ::= '..\@@'</summary>
        PathSymbol6,
        /// <summary>&lt;PathSymbol&gt; ::= </summary>
        PathSymbol7,
        /// <summary>&lt;WebLink&gt; ::= &lt;WebHeader&gt; &lt;IdentifierOpt&gt; &lt;WebPath&gt; &lt;WebFile&gt;</summary>
        WebLink,
        /// <summary>&lt;WebHeader&gt; ::= HTTP '://'</summary>
        WebHeader,
        /// <summary>&lt;WebHeader&gt; ::= HTTPS '://'</summary>
        WebHeader2,
        /// <summary>&lt;WebHeader&gt; ::= FTP '://'</summary>
        WebHeader3,
        /// <summary>&lt;WebPath&gt; ::= '/' Identifier &lt;WebPath&gt;</summary>
        WebPath,
        /// <summary>&lt;WebPath&gt; ::= </summary>
        WebPath2,
        /// <summary>&lt;WebFile&gt; ::= '/'</summary>
        WebFile,
        /// <summary>&lt;WebFile&gt; ::= '/' &lt;IdentifierOpt&gt;</summary>
        WebFile2,
        /// <summary>&lt;WebFile&gt; ::= </summary>
        WebFile3,
        /// <summary>&lt;On_Browser_Exit_Block&gt; ::= ONBROWSEREXIT NewLine BEGIN NewLine &lt;Statements&gt; NewLine END</summary>
        On_Browser_Exit_Block,
        /// <summary>&lt;Startup_Block&gt; ::= STARTUP NewLine BEGIN NewLine &lt;Statements&gt; NewLine END</summary>
        Startup_Block,
        /// <summary>&lt;ShutDown_Block&gt; ::= SHUTDOWN NewLine BEGIN NewLine &lt;Statements&gt; NewLine END</summary>
        ShutDown_Block,
        /// <summary>&lt;On_Browser_Exit_Empty_Block&gt; ::= ONBROWSEREXIT NewLine BEGIN NewLine END</summary>
        On_Browser_Exit_Empty_Block,
        /// <summary>&lt;Startup_Empty_Block&gt; ::= STARTUP NewLine BEGIN NewLine END</summary>
        Startup_Empty_Block,
        /// <summary>&lt;ShutDown_Empty_Block&gt; ::= SHUTDOWN NewLine BEGIN NewLine END</summary>
        ShutDown_Empty_Block,
    }
}