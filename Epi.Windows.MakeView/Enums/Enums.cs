namespace Epi.Windows.MakeView.Enums
{
	/// <summary>
	/// Command categories for MakeView Check Code
	/// </summary>
	public enum CommandGroups
	{
		/// <summary>
		/// Field command category
		/// </summary>
		Fields,
		/// <summary>
		/// Program command category
		/// </summary>
		Programs,
		/// <summary>
		/// Record command category
		/// </summary>
		Records,
		/// <summary>
		/// User Interaction command category
		/// </summary>
		UserInteraction,
		/// <summary>
		/// Variable command category
		/// </summary>
		Variables
	}

	/// <summary>
	/// Commands in the Field category
	/// </summary>
	public enum FieldCommands
	{
		/// <summary>
		/// Clear a field
		/// </summary>
		Clear,
		/// <summary>
		/// Go to a field
		/// </summary>
		GoTo,
        /// <summary>
        /// Go to a relatefield
        /// </summary>
        GoToForm,
		/// <summary>
		/// Hide a field
		/// </summary>
		Hide,
		/// <summary>
		/// Unhide a field
		/// </summary>
		Unhide,
        /// <summary>
        /// Enables a field
        /// </summary>
        Enable,
        /// <summary>
        /// Disables a field
        /// </summary>
        Disable,
        /// <summary>
        /// Highlights a field
        /// </summary>
        Highlight,
        /// <summary>
        /// Unhighlights a field
        /// </summary>
        Unhighlight,
        /// <summary>
        /// Geocode a field
        /// </summary>
        Geocode,
        /// <summary>
        /// Sets a field required
        /// </summary>
        Set_Required,
        /// <summary>
        /// Sets a field not required
        /// </summary>
        Set_Not_Required,

         /// <summary>
        /// Select FieldSelector
        /// </summary>
        FieldSelector,
        /// <summary>
        /// IOCode a field
        /// </summary>
        IOCode
    }

    /// <summary>
    /// Commands in the Program category
    /// </summary>
    public enum ProgramCommands
	{
        /// <summary>
        /// calls a subroutine
        /// </summary>
        Call,
		/// <summary>
		/// Execute a program
		/// </summary>
		Execute,        
        /// <summary>
        /// Exits a program
        /// </summary>
        Quit
	}

	/// <summary>
	/// Commands in the Record category
	/// </summary>
	public enum RecordCommands
	{
		/// <summary>
		/// Auto search for a record
		/// </summary>
		AutoSearch,
		/// <summary>
		/// If-then block
		/// </summary>
		If,
        /// <summary>
        /// Creates a new record
        /// </summary>
        NewRecord
	}

	/// <summary>
	/// Commands in the User Interaction category
	/// </summary>
	public enum UserInteractionCommands
	{
		/// <summary>
		/// Show a dialog
		/// </summary>
		Dialog,
		/// <summary>
		/// Show help
		/// </summary>
		Help
	}

	/// <summary>
	/// Commands in the Variable category
	/// </summary>
	public enum VariableCommands
	{
		/// <summary>
		/// Assign a variable
		/// </summary>
		Assign,
		/// <summary>
		/// Define a variable
		/// </summary>
		Define
	}
    
    public enum TemplateLevel 
    { 
        Project, 
        Form, 
        Page, 
        Field, 
        Unknown 
    }

    public enum OptionLayout
    {
        Horizontal,
        Vertical,
        Left,
        Right
    }

    public enum MakeSame
    {
        Width_Use_Maximum,
        Width_Use_Minimum,
        Height_Use_Maximum,
        Height_Use_Minimum,
        Size_Use_Maximum,
        Size_Use_Minimum
    }
}
