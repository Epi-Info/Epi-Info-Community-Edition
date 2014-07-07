
namespace Epi
{
    /// <summary>
    /// Valid Epi Info file extensions
    /// </summary>
    public static class FileExtensions
    {
        /// <summary>
        /// Config file extension
        /// </summary>
        public const string CONFIG = ".config";
        /// <summary>
        /// Data  file extension
        /// </summary>
        public const string DATA = ".data";
        /// <summary>
        /// Project file extension
        /// </summary>
        public const string EPI_PROJ = ".prj";
        /// <summary>
        /// Menu file extension
        /// </summary>
        public const string MNU = ".mnu";
        /// <summary>
        /// PGM (analysis program)  file extension
        /// </summary>
        public const string Pgm = ".pgm7";
        /// <summary>
        /// DLL  ile extension
        /// </summary>
        public const string DLL = ".dll";
        /// <summary>
        /// Access file extension
        /// </summary>
        public const string MDB = ".mdb";
        /// <summary>
        /// Access 2007 file extension
        /// </summary>
        public const string MDB2007 = ".accdb";
        /// <summary>
        /// Excel Spreadsheet extension
        /// </summary>
        public const string XLS = ".xls";
        /// <summary>
        /// Excel 2007 Spreadsheet extension
        /// </summary>
        public const string XLS2007 = ".xlsx";
        /// <summary>
        /// eXtensible Markup Language extension
        /// </summary>
        public const string XML = ".xml";

        /// <summary>
        /// Case-insensitive comparison of extension of fileName to fileExtension.
        /// </summary>
        /// <param name="fileName">Fully-qualified name of the new file, or the relative file name.</param>
        /// <param name="fileExtension">File name extension.</param>
        /// <returns>True/False</returns>
        public static bool HasExtension(string fileName, string fileExtension)
        {
            System.IO.FileInfo fileNameInfo = new System.IO.FileInfo(fileName);

            return (string.Compare(fileNameInfo.Extension, fileExtension, true) == 0);
        }
    } // Class FileExtensions	
}