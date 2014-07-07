
namespace Epi
{
    /// <summary>
    /// Often used string literals
    /// </summary>
    /// <remarks>Many of them should have been character literals</remarks>
	public static class StringLiterals
    {
        /// <summary>
        /// String Literal ASCENDING (++)
        /// </summary>
        public const string ASCENDING = "(++)";

        /// <summary>
        /// String Literal AMPERSAND
        /// </summary>
        public const string AMPERSAND = "&";

        /// <summary>
        /// String literal back tick
        /// </summary>
        public const string BACK_TICK = @"`";

        /// <summary>
        /// String Literal BACKWARD SLASH
        /// </summary>
        public const string BACKWARD_SLASH = "\\";

        /// <summary>
        /// String Literal CARET
        /// </summary>
        public const string CARET = "^";

        /// <summary>
        /// String Literal COLON
        /// </summary>
        public const string COLON = ":";

        /// <summary>
        /// String Literal COMMA
        /// </summary>
        public const string COMMA = ",";

        /// <summary>
        /// String Literal COMMERCIAL AT
        /// </summary>
        public const string COMMERCIAL_AT = "@";

        /// <summary>
        /// String Literal Left Curly Brace
        /// </summary>
        public const string CURLY_BRACE_LEFT = "{";

        /// <summary>
        /// String Literal Right Curly Brace
        /// </summary>
        public const string CURLY_BRACE_RIGHT = "}";

        /// <summary>
        /// String Literal DESCENDING
        /// </summary>
        public const string DESCENDING = "(--)";

        /// <summary>
        /// String Literal DOUBLE QUOTES
        /// </summary>
        public const string DOUBLEQUOTES = "\"";

        /// <summary>
        /// Ellipsis
        /// </summary>
        public const string ELLIPSIS = "...";
        
        /// <summary>
        /// String Literal EPI REPRESENTATION OF FALSE
        /// </summary>
        public const string EPI_REPRESENTATION_OF_FALSE = "(-)";

        /// <summary>
        /// String Literal EPI REPRESENTATION OF MISSING
        /// </summary>
        public const string EPI_REPRESENTATION_OF_MISSING = "(.)";

        /// <summary>
        /// String Literal EPI REPRESENTATION OF TRUE
        /// </summary>
        public const string EPI_REPRESENTATION_OF_TRUE = "(+)";

        /// <summary>
        /// String Literal EPI TABLE
        /// </summary>
        public const string EPI_TABLE = "epitable";

        /// <summary>
        /// String Literal EQUAL
        /// </summary>
        public const string EQUAL = "=";

        /// <summary>
        /// String Literal FORWARD SLASH
        /// </summary>
        public const string FORWARD_SLASH = "/";

        /// <summary>
        /// String Literal GREATER_THAN
        /// </summary>
        public const string GREATER_THAN = ">";

        /// <summary>
        /// String Literal GREATER_THAN_OR_EQUAL
        /// </summary>
        public const string GREATER_THAN_OR_EQUAL = ">=";

        /// <summary>
        /// String Literal HASH CLOSE
        /// </summary>
        public const string HASH = "#";

        /// <summary>
        /// String Literal HYPHEN
        /// </summary>
        public const string HYPHEN = "-";

        /// <summary>
        /// String Literal LEFT SQUARE BRACKET
        /// </summary>
        public const string LEFT_SQUARE_BRACKET = "[";

        /// <summary>
        /// String Literal LESS THAN
        /// </summary>
        public const string LESS_THAN = "<";


        /// <summary>
        /// String Literal LESS THAN_OR_EQUAL
        /// </summary>
        public const string LESS_THAN_OR_EQUAL = "<=";

        /// <summary>
        /// String Literal LESS THAN_OR_GREATER THAN (NOT EQUAL)
        /// </summary>
        public const string LESS_THAN_OR_GREATER_THAN = "<>";

        /// <summary>
        /// String Literal NEW LINE
        /// </summary>
        public const string NEW_LINE = "\r\n";

        /// <summary>
        /// String Literal NewValue
        /// </summary>
        public const string NEW_VALUE = "NewValue";

        /// <summary>
        /// String Literal 255
        /// </summary>
        public const string NUMBER_255 = "255";

        /// <summary>
        /// String Literal for Old Guid Code
        /// </summary>
        public const string OLD_GUID_CODE = "ALWAYS\r\nIF CDCUNIQUEID=(.) OR CDCUNIQUEID=\"\"  THEN\r\nASSIGN CDCUNIQUEID = GLOBAL_ID!GetGlobalUniqueID()\r\nASSIGN CDC_UNIQUE_ID=CDCUNIQUEID\r\nEND\r\nEND";
            
        /// <summary>
        /// String Literal OldValue
        /// </summary>
        public const string OLD_VALUE = "OldValue";
            
        /// <summary>
        /// String Literal PARENTHESES OPEN
        /// </summary>
		public const string PARANTHESES_OPEN = "(";

        /// <summary>
        /// String Literal PARENTHESES CLOSE
        /// </summary>
        public const string PARANTHESES_CLOSE = ")";

        /// <summary>
        /// String Literal PERCENT
        /// </summary>
        public const string PERCENT = "%";

        /// <summary>
        /// String Literal PERIOD
        /// </summary>
        public const string PERIOD = ".";

        /// <summary>
        /// String Literal PLUS
        /// </summary>
        public const string PLUS = "+";

        /// <summary>
        /// String Literal RIGHT SQUARE BRACKET
        /// </summary>
        public const string RIGHT_SQUARE_BRACKET = "]";

        /// <summary>
        /// Strlig literal semicolon
        /// </summary>
        public const string SEMI_COLON = @";";

        /// <summary>
        /// String Literal SINGLE QUOTES
        /// </summary>
		public const string SINGLEQUOTES = "'";

        /// <summary>
        /// String Literal SPACE
        /// </summary>
        public const string SPACE = " ";
                
        /// <summary>
        /// String Literal STAR
        /// </summary>
        public const string STAR = "*";
    
        /// <summary>
        /// String Literal TAB
        /// </summary>
        public const string TAB = "\t";

        /// <summary>
        /// String Literal for Table Import Status
        /// </summary>
        public const string TABLE_IMPORT_STATUS = "{0} ({1} {2}{3})";

        /// <summary>
        /// String literal UNDERSCORE
        /// </summary>
        public const string UNDER_SCORE = "_";
    } // class StringLiterals
}
