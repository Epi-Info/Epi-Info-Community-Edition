using System;
using System.Collections;

namespace Epi
{
	/// <summary>
	/// Broker to open and close project files. Opened projects should be observed by 
	/// the creation factory implementor.
	/// </summary>
	public interface IProjectManager
	{
		/// <summary>
		/// Factory method for all project instances. Loads project instance.
		/// </summary>
		/// <param name="filePath">The location of the project file</param>
		/// <returns>An object implementing the Project interface</returns>
		Project OpenProject(string filePath);
		
		/// <summary>
		/// Creates new empty project 
		/// </summary>
		/// <returns>Instance of project</returns>
		Project CreateNewProject();

	}
}
