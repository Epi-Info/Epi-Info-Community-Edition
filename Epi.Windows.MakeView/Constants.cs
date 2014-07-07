using System;

namespace Epi.Windows.MakeView
{	
	/// <summary>
	/// Category for wizard type
	/// </summary>
	public enum WizardType
	{
		/// <summary>
		/// Access New project wizard
		/// </summary>
		NewProject = 1,
		/// <summary>
		/// Access Import wizard
		/// </summary>
		Import = 2,
	}

	/// <summary>
	/// Category for panels of the project wizard
	/// </summary>
	public enum WizardPanel
	{
		/// <summary>
		/// Access the Access Meta Data panel
		/// </summary>
		AccessMetadata,
		/// <summary>
		/// Access the Database Format panel
		/// </summary>
		Database_Format,
		/// <summary>
		/// Access the SQL Meta Data panel
		/// </summary>
		Sql_Meta_Data,
		/// <summary>
		/// Access the SQL Project Data panel
		/// </summary>
		Sql_Project_Data,
		/// <summary>
		/// Access the Project Setup panel
		/// </summary>
		Setup,
		/// <summary>
		/// Access the Create Project panel
		/// </summary>
		Create_Project,
		/// <summary>
		/// Access the Access Project Data panel
		/// </summary>
		Access_Project_Data,
        /// <summary>
        /// 
        /// </summary>
        ViewInfo
	}
}
