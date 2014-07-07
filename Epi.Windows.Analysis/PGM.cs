using System;
using System.IO;
using System.Data;
using Epi;
// dcs0 refactoring
namespace Epi.Analysis
{
    /// <summary>
    /// Abstract base class Pgm
    /// </summary>
	public abstract class Pgm
	{
        #region Constructors

        /// <summary>
        /// Constructor for Pgm
        /// </summary>
        /// <param name="proj"></param>
        protected Pgm(Project proj )
        {
            pgmRow = CreatePgmRow( proj );
        }
        #endregion Constructors

        #region Private Attributes
        #endregion Private Attributes

        #region Private Methods
        #endregion Private Methods

        #region Protected Attributes
        /// <summary>
        /// The DataRow that contains all of attributes of the current pgm
        /// </summary>
        protected DataRow pgmRow;
        #endregion Protected Attributes

        #region Protected Methods

        /// <summary>
        /// Loads the Pgm as a DataRow
        /// </summary>
        /// <param name="name"></param>
        /// <returns>DataRow</returns>
        public abstract DataRow PgmLoad(string name);

        /// <summary>
        /// Saves the Pgm passed as a DataRow
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public abstract bool PgmSave(DataRow row);

 
        /// <summary>
        /// Deletes the PGM by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract bool PgmDelete(string name);

        /// <summary>
        /// Creates an instance of a DataRow that is cloned after the MetaPrograms table
        /// </summary>
        /// <param name="proj">Reference to the current project</param>
        /// <returns>DataRow</returns>
        protected DataRow CreatePgmRow(Project proj)
        {
            try
            {
                DataTable tbl = proj.GetPgms();
                DataTable clone = tbl.Clone();
                DataRow row = clone.NewRow();
                clone.Dispose();
                tbl.Dispose();
                return row;
            }
            catch
            {
                throw new ApplicationException("You do not have a current project.");
            }
           
        }

        /// <summary>
        /// Finds the program in MetaPrograms table and returns it in a DataRow
        /// </summary>
        /// <param name="proj">Reference to the current project</param>
        /// <param name="name">Name of the pgm</param>
        /// <returns>DataRow</returns>
        protected DataRow FindPgmByName(Project proj, string name)
        {
            DataRow row = null;
            DataTable programs = proj.GetPgms();
            if (programs.Rows.Count > 0)
            {
                DataRow[] rows;
                rows = programs.Select("[" + ColumnNames.PGM_NAME + "] = '" + name + "'");
                if (rows.GetUpperBound( 0 ) >= 0)      // Existing pgm
                {
                    row = rows[0];
                }
            }
            return row;
        }

        #endregion Protected Methods

        #region Public Properties

        /// <summary>
        /// Return the PGM script
        /// </summary>
        public string Content
        {
            get
            {
                return pgmRow[ColumnNames.PGM_CONTENT].ToString();
            }
        }

        /// <summary>
        /// Return the name of the PGM
        /// </summary>
        public string Name
        {
            get
            {
                    return pgmRow[ColumnNames.PGM_NAME].ToString();
            }
        }
        #endregion Public Properties

    }
}
