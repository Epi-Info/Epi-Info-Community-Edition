using System;
using System.Collections.Generic;
using System.Text;

namespace Epi
{
    /// <summary>
    /// Used by the main Epi Info modules to check and see what UI elements (or other actions) should
    /// be enabled or disabled depending on what rights the current user has in the database.
    /// </summary>
    public class ProjectPermissions
    {
        #region Public Members
        public bool CanUpdateRowsInMetaTables;
        public bool CanDeleteRowsInMetaTables;
        public bool CanInsertRowsInMetaTables;
        public bool CanSelectRowsInMetaTables;

        public bool CanCreateTablesInDatabase;
        public bool CanDeleteTablesInDatabase;
        public bool CanOpenDatabase;
        public bool FullPermissions;
        #endregion // Public Members

        #region Private Members
        public List<string> TablesWithUpdatePermissions;
        public List<string> TablesWithSelectPermissions;
        public List<string> TablesWithInsertPermissions;
        public List<string> TablesWithDeletePermissions;
        #endregion // Private Members

        #region Constructors
        public ProjectPermissions()
        {
            FullPermissions = true;

            TablesWithUpdatePermissions = new List<string>();
            TablesWithSelectPermissions = new List<string>();
            TablesWithInsertPermissions = new List<string>();
            TablesWithDeletePermissions = new List<string>();
        }
        #endregion // Constructors
    }
}
