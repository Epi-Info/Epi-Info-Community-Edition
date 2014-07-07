using System;
using System.Data;
using System.Collections;


namespace Epi.Analysis
{

    /// <summary>
    /// Project Based Pgm class
    /// </summary>
	public class ProjectBasedPgm : Pgm
    {
        #region Private Attributes
        private Project _project;
        private string _name;
        #endregion Private Attributes

        #region Private Methods
        #endregion Private Methods

        #region Constructor

        /// <summary>
        /// Constructor for the Project based PGM
        /// </summary>
        /// <param name="proj"></param>
        /// <param name="name"></param>
        public ProjectBasedPgm(Project proj, string name) : base(proj)
        {
			_project = proj;
            _name = name;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the selected program by name and returns it in a DataRow
        /// </summary>
        /// <param name="name">Name of the pgm</param>
        /// <returns>DataRow</returns>
        public override DataRow PgmLoad( string name )
        {
            pgmRow = FindPgmByName(_project, _name);
            return pgmRow;
        }

        /// <summary>
        /// Saves the program.  If the program exists, it is updated.
        /// </summary>
        /// <param name="pgmRow">DataRow with all of the pgm information</param>
        /// <returns>bool</returns>
        public override bool PgmSave(DataRow pgmRow)
        {
            if (FindPgmByName(_project, pgmRow[ColumnNames.PGM_NAME].ToString())== null)
            {
                _project.InsertPgm(pgmRow[ColumnNames.PGM_NAME].ToString(), 
                                   pgmRow[ColumnNames.PGM_CONTENT].ToString(),
                                   pgmRow[ColumnNames.PGM_COMMENT].ToString(), 
                                   pgmRow[ColumnNames.PGM_AUTHOR].ToString());
            }
            else
            {
                _project.UpdatePgm( (int)pgmRow[ColumnNames.PROGRAM_ID], 
                                        pgmRow[ColumnNames.PGM_NAME].ToString(), 
                                        pgmRow[ColumnNames.PGM_CONTENT].ToString(),
                                        pgmRow[ColumnNames.PGM_COMMENT].ToString(), 
                                        pgmRow[ColumnNames.PGM_AUTHOR].ToString());
            }
            return true;
        }
    
        /// <summary>
        /// Deletes the program by name.
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>bool</returns>
        public override bool PgmDelete( string name )
        {
            pgmRow = FindPgmByName(_project, name);
            if (pgmRow != null)
            {
                //if (_project.DataFormat == Epi2000)
                _project.DeletePgm(name, (int)pgmRow[ColumnNames.PROGRAM_ID]);
            }
            return true;
        }
        #endregion Public Methods

    }
}
