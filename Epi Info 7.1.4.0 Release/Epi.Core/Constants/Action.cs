namespace Epi
{
    /// <summary>
    /// Enumeration of Command Actions.
    /// </summary>
    public enum Action
    {
        /// <summary>
        /// Beep
        /// </summary>
        Beep,

        /// <summary>
        /// Quit
        /// </summary>
        /// <remarks>Causes an exit from the application</remarks>
        Quit,

        /// <summary>
        /// Cancel
        /// </summary>
        Cancel,

        /// <summary>
        /// Back
        /// </summary>
        Back,

        /// <summary>
        /// next
        /// </summary>
        Next,

        /// <summary>
        /// User Dialog
        /// </summary>
        /// <remarks>Action to display a user defined dialog</remarks>
        UserDialog,

        /// <summary>
        /// Simple Dialog
        /// </summary>
        /// <remarks>Displays the UserDialog but doesn't assign the results</remarks>
        SimpleDialog,

        /// <summary>
        /// File Open Dialog
        /// </summary>
        /// <remarks>Display the File Open Dialog</remarks>
        FileOpenDialog,

        /// <summary>
        /// File Save Dialog
        /// </summary>
        /// <remarks>Display the File Save Dialog</remarks>
        FileSaveDialog,

        // TODO: This must be more rigorous to allow for other types of databases
        /// <summary>
        /// Databases Dialog
        /// </summary>
        /// <remarks>Display a Dialog to list databases</remarks>
        DatabasesDialog,

        //Actions for Enter Commands
        
        ///<summary>
        /// Always
        ///</summary>
        Always,
        
        /// <summary>
        /// Assign
        /// </summary>
        Assign,

        /// <summary>
        /// Autosearch
        /// </summary>
        AutoSearch,

        /// <summary>
        /// Clear
        /// </summary>
        Clear,

        /// <summary>
        /// Define Global Variable
        /// </summary>
        DefineGlobalVar,

        /// <summary>
        /// Define Permanent Variable
        /// </summary>
        DefinePermanentVar,

        /// <summary>
        /// Define Standard variable
        /// </summary>
        DefineStandardVar,

        /// <summary>
        /// Dialog
        /// </summary>
        Dialog,

        /// <summary>
        /// Execute String
        /// </summary>
        ExecuteString,

        /// <summary>
        /// Execute File
        /// </summary>
        ExecuteFile,

        /// <summary>
        /// Execute Url
        /// </summary>
        ExecuteUrl,

        /// <summary>
        /// Go To
        /// </summary>
        GoTo,

        /// <summary>
        /// Help
        /// </summary>
        Help,

        /// <summary>
        /// Hide
        /// </summary>
        Hide,

        /// <summary>
        /// Hide Except
        /// </summary>
        HideExcept,

        /// <summary>
        /// If Statement
        /// </summary>
        IfStatement,

        /// <summary>
        /// If Else Statement
        /// </summary>
        IfElseStatement,

        /// <summary>
        /// New Record
        /// </summary>
        NewRecord,

        /// <summary>
        /// Send a File to Printer.
        /// </summary>
        Print,

        /// <summary>
        /// Unhide
        /// </summary>
        Unhide,

        /// <summary>
        /// Unhide Except
        /// </summary>
        UnhideExcept,

        //Actions for Analysis Commands
        /// <summary>
        /// Display XML in a Grid
        /// </summary>
        GridTable,

        /// <summary>
        /// Update
        /// </summary>
        /// <remarks>Display XML in a Grid and allow update</remarks>
        Update,

        /// <summary>
        /// Produce an output table in the current collected data
        /// </summary>
        OutTable,

        /// <summary>
        /// Change the name of the output file.
        /// </summary>
        OutputFileName
    }
}