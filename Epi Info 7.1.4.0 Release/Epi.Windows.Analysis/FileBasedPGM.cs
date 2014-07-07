using System;
using System.IO;
using System.Data;


namespace Epi.Analysis
{

    /// <summary>
    /// File based PGM class
    /// </summary>
	public class FileBasedPgm : Pgm
	{
		#region Private Class Members
		private string filePath = string.Empty;
        private Project _project;
		#endregion Private Class Members

		#region Constructor
        /// <summary>
        /// Constructor for the file-based PGM class
        /// </summary>
        /// <param name="proj"></param>
        /// <param name="filepath"></param>
        public FileBasedPgm(Project proj, string filepath) : base(proj)
		{
			#region Input Validation
			if (string.IsNullOrEmpty(filepath))
			{
				throw new ArgumentNullException("filepath");
			}
			#endregion Input Validation
            this._project = proj;
			this.filePath = filepath;
		}
		#endregion Constructors

        #region Public Properties
        /// <summary>
		/// Read-only accessor for file path
		/// </summary>
		public string FilePath
		{
			get
			{
				return (filePath);
			}
            set
            {
                filePath = value;
            }
		}


		#endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Loads the selected program by filepath and returns it in a DataRow
        /// </summary>
        /// <param name="filepath">Name of the pgm</param>
        /// <returns>DataRow</returns>
        public override DataRow PgmLoad(String filepath)
        {
            string text;
            try
            {
                using (StreamReader sr = File.OpenText(filepath))
                {
                    //Read the entire file
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                    text = sr.ReadToEnd();
                }
                pgmRow[ColumnNames.PGM_CREATE_DATE] = File.GetCreationTime(filePath);
                pgmRow[ColumnNames.PGM_MODIFY_DATE] = File.GetLastWriteTime(filePath);
                pgmRow[ColumnNames.PGM_CONTENT] = text;
                pgmRow[ColumnNames.PGM_COMMENT] = text;
                pgmRow[ColumnNames.PGM_NAME] = Path.GetFileNameWithoutExtension(filePath);
                this.filePath = filepath;
                return pgmRow;
            }
            catch (Exception ex)
            {
                throw new GeneralException(SharedStrings.UNABLE_READ_PGM, ex);
            }
        }

        /// <summary>
        /// Saves the selected program
        /// </summary>
        /// <param name="row">Complete information of the pgm</param>
        /// <returns>bool</returns>

        public override bool PgmSave(DataRow row)
        {
            #region Preconditions
            if ((row == null) || (row[ColumnNames.PGM_CONTENT].ToString() == string.Empty))
            {
                throw new GeneralException("Save called with no File Path");
            }
            #endregion Preconditions
            try
            {
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    //Write the entire file
                    sw.BaseStream.Seek(0, SeekOrigin.Begin);
                    string[] program = row[ColumnNames.PGM_CONTENT].ToString().Split('\n');
                    foreach (string line in program)
                    {
                        sw.WriteLine(line);
                    }
                    sw.Close();
                    sw.Dispose();
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(SharedStrings.UNABLE_WRITE_PGM, ex);  
            }
        }



        /// <summary>
        /// Saves the Pgm passed as a string
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="PGMContent"></param>
        /// <returns>bool</returns>
        public  static bool PgmSave(string filePath, string PGMContent)
        {

            try
            {
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    
                    sw.BaseStream.Seek(0, SeekOrigin.Begin);
                    sw.WriteLine(PGMContent);
                 
                    sw.Close();
                    sw.Dispose();
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(SharedStrings.UNABLE_WRITE_PGM, ex);
            }

           
        }
        /// <summary>
        /// deletes the .PGM
        /// </summary>
        /// <param name="filepath">Complet path to the pgm</param>
        /// <returns>bool</returns>
        public override bool PgmDelete( string filepath )
        {
            File.Delete(filepath);
            this.filePath = string.Empty;
            return true;
        }
        #endregion Public Methods
	}
}
