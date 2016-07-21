#region Namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
#endregion  //Namespaces

namespace Epi
{
	/// <summary>
	/// Handles the arguments received in a command line
	/// </summary>
	public sealed class CommandLine : ICommandLine
    {
        #region Private Data Members
        string[] arguments;
        private IDictionary dictionary;
        #endregion  //Private Data Members

        #region Constructors        
        
        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="arguments">An array of command line arguments</param>
        public CommandLine(string[] arguments)
		{
            this.arguments = arguments;
			this.dictionary = new Hashtable();

            string commandString = string.Empty;
            IEnumerator argumentsEnum = arguments.GetEnumerator();
            while (argumentsEnum.MoveNext())
            {
                commandString += argumentsEnum.Current + " ";
            }

            string[] commands = commandString.Split(new char[]{'/'}, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < commands.Length; i++)
            {
                string argument = commands[i];
                string key = string.Empty;
                string value = string.Empty;
                
                //if ((argument[0] != '/') && (argument[0] != '-'))
                //{
                //    value = argument;
                //}
                //else
                //{
                //    int seperatorPos = argument.IndexOf(':');
                //    if (seperatorPos == -1)
                //    {
                //        key = argument.Substring(1).ToLowerInvariant(CultureInfo.InvariantCulture);
                //        if (key == "?")
                //        {
                //            key = "help";
                //        }
                //    }
                //    else
                //    {
                //        key = argument.Substring(1, seperatorPos - 1).ToLowerInvariant(CultureInfo.InvariantCulture);
                //        value = argument.Substring(seperatorPos + 1);
                //    }
                //}

                int seperatorPos = argument.IndexOf(':');
                if (seperatorPos == -1)
                {
                    key = argument.Substring(0).ToLowerInvariant();
                    if (key == "?")
                    {
                        key = "help";
                    }
                }
                else
                {
                    key = argument.Substring(0, seperatorPos).ToLowerInvariant();
                    value = argument.Substring(seperatorPos + 1);
                }

                List<string> list = (List<string>)this.dictionary[key];
                if (list == null)
                {
                    list = new List<string>();
                    this.dictionary.Add(key, list);
                }

                list.Add(value.Trim());
            }
        }

        #endregion  //Constructors

        #region Public Methods
        /// <summary>
        /// Retrieves argument by argument name
        /// </summary>
        /// <param name="name">The name of the argument</param>
        /// <returns>The argument</returns>
        public string GetArgument(string name)
		{
            List<string> list = (List<string>)this.dictionary[name];
			if (list == null)
			{
				return null;
			}
			if (list.Count != 1)
			{
				throw new InvalidOperationException();
			}
			return (string) list[0];
		}
 
        /// <summary>
        /// Retrieves arguments by argument name
        /// </summary>
        /// <param name="name">The name of the argument</param>
        /// <returns>The argument</returns>
		public string[] GetArguments(string name)
		{
            List<string> list = (List<string>)this.dictionary[name];
			if (list != null)
			{
				string[] textArray1 = new string[list.Count];
				list.CopyTo(textArray1, 0);
				return textArray1;
			}
			return null;
        }

        #endregion  //Public Methods

        #region Public Properties
        /// <summary>
        /// The array of arguments
        /// </summary>
        public string[] ArgumentStrings
        {
            get
            {
                return this.arguments;
            }
        }
        #endregion //Public Properties
    }
}
