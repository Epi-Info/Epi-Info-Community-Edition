using System;

namespace Epi
{
	/// <summary>
	/// Interface class for Command Line.
	/// </summary>
    public interface ICommandLine
	{
		/// <summary>
		/// Gets a command line argument.
		/// </summary>
		/// <param name="name">Command line argument.</param>
		/// <returns>Command line argument value.</returns>
        string GetArgument(string name);
        /// <summary>
        /// Gets command line arguments.
        /// </summary>
        /// <param name="name">Command line arguments.</param>
        /// <returns>String array of command line arguments.</returns>
		string[] GetArguments(string name);
        /// <summary>
        /// Property get of command line argument strings.
        /// </summary>
        string[] ArgumentStrings { get;}
	}
}
