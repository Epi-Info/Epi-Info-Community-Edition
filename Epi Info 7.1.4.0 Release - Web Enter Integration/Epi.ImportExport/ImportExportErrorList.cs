using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Epi.ImportExport
{
    /// <summary>
    /// A class used to manage lists of errors, warnings, and notifications generated during various types of operations in import/export.
    /// </summary>
    public class ImportExportErrorList : IEnumerable<ImportExportMessage>
    {
        #region Members
        /// <summary>
        /// This list will contain all the errors, warnings, and other notices.
        /// </summary>
        private List<ImportExportMessage> errorList;
        #endregion // Members

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public ImportExportErrorList()
        {
            errorList = new List<ImportExportMessage>();
        }
        #endregion // Constructors

        #region Properties
        /// <summary>
        /// Gets the number of errors in the list
        /// </summary>
        public int Errors
        {
            get
            {
                int errorCount = 0;
                foreach (ImportExportMessage problem in errorList)
                {
                    if (problem.MessageType == ImportExportMessageType.Error) errorCount++;
                }
                return errorCount;
            }
        }

        /// <summary>
        /// Gets the number of errors in the list
        /// </summary>
        public int Warnings
        {
            get
            {
                int warningCount = 0;
                foreach (ImportExportMessage problem in errorList)
                {
                    if (problem.MessageType == ImportExportMessageType.Warning) warningCount++;
                }
                return warningCount;
            }
        }

        /// <summary>
        /// Gets the number of notifications in the list
        /// </summary>
        public int Notifications
        {
            get
            {
                int notificationCount = 0;
                foreach (ImportExportMessage problem in errorList)
                {
                    if (problem.MessageType == ImportExportMessageType.Notification) notificationCount++;
                }
                return notificationCount;
            }
        }
        #endregion // Properties

        #region Methods
        /// <summary>
        /// Adds a new entry to the list of problems
        /// </summary>
        /// <param name="pProblemType">The type of problem</param>
        /// <param name="pCode">The code associated with the problem</param>
        /// <param name="pMessage">The short description of the problem</param>
        /// <param name="pDescription">The long description of the problem</param>
        public void Add(ImportExportMessageType pProblemType, string pCode, string pMessage, string pDescription)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(pCode))
            {
                throw new ArgumentNullException(pCode);
            }
            else if (string.IsNullOrEmpty(pMessage))
            {
                throw new ArgumentNullException(pMessage);
            }
            #endregion // Input Validation

            ImportExportMessage problem = new ImportExportMessage();
            problem.ID = GetNextID();
            problem.MessageType = pProblemType;
            problem.Code = pCode;
            problem.Message = pMessage;
            problem.Description = pDescription;
            errorList.Add(problem);
        }

        /// <summary>
        /// Adds a new entry to the list of problems
        /// </summary>
        /// <param name="pProblemType">The type of problem</param>
        /// <param name="pCode">The code associated with the problem</param>
        /// <param name="pMessage">The short description of the problem</param>
        public void Add(ImportExportMessageType pProblemType, string pCode, string pMessage)
        {
            this.Add(pProblemType, pCode, pMessage, string.Empty);
        }

        /// <summary>
        /// Gets the next ID for the list of problems.
        /// </summary>
        /// <returns></returns>
        private int GetNextID()
        {
            return this.errorList.Count + 1;
        }
        #endregion // Methods

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<ImportExportMessage> GetEnumerator()
        {
            return new ImportExportErrorListEnumerator(errorList);
        }

        public void Dispose() { }

        #endregion        

        #region Classes
        /// <summary>
        /// Enumerator class for the data filter class
        /// </summary>
        protected class ImportExportErrorListEnumerator : IEnumerator<ImportExportMessage>
        {
            #region Private Members
            private List<ImportExportMessage> errorList;
            private int currentIndex;
            #endregion Private Members

            public ImportExportErrorListEnumerator(List<ImportExportMessage> pErrorList)
            {
                errorList = pErrorList;
                Reset();
            }

            public void Reset()
            {
                currentIndex = -1;
            }

            public ImportExportMessage Current
            {
                get
                {
                    return (ImportExportMessage)errorList[currentIndex];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public bool MoveNext()
            {
                currentIndex++;
                return currentIndex < errorList.Count;
            }

            public void Dispose() { }
        }
        #endregion // Classes
    }
}
