using System;
using System.Collections.Generic;
using System.Text;

namespace Epi
{
    /// <summary>
    /// Named Subroutine class
    /// </summary>
    public class SubRoutine : INamedObject
    {
        #region Private Members
        private string name = string.Empty;
        private List<string> body = null;
        #endregion Private Members
        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of subroutine</param>
        public SubRoutine(string name)
        {
            this.name = name;
            body = new List<string>();
        }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Gets/sets the name of the subroutine
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                name = value;
            }
        }
        /// <summary>
        /// Gets a list of subroutines.
        /// </summary>
        public List<string> Body
        {
            get
            {
                return body;
            }
        }
        #endregion Public Properties
    }
}
