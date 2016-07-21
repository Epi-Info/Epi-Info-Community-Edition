using System;
using System.Data;
using System.IO;
using Epi;
using Epi.Data;
using Epi.Data.Services;
using Epi.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Epi.Epi2000
{
    /// <summary>
    /// The Project class for Epi2000
    /// </summary>
    //	public class Project : Epi.Project //, IJetDatabase    
    public class Project
    {
        #region Public Events

        /// <summary>
        /// Declaration for the Table Copy Begin Event
        /// </summary>
        public event MessageEventHandler TableCopyBeginEvent;

        /// <summary>
        /// Declaration for the Table Copy Status Event
        /// </summary>
        public event TableCopyStatusEventHandler TableCopyStatusEvent;

        /// <summary>
        /// Declaration for the Table Copy End Event
        /// </summary>
        public event MessageEventHandler TableCopyEndEvent;

        #endregion Public Events

        #region Fields
        private string location;
        private string name;
        private bool useMetadataDbForCollectedData;
        private MetadataDbProvider metadata;
        private CollectedDataProvider collectedData;
        private Collection<View> views = null;
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public Project()
        {
            PreConstruct();
        }

        /// <summary>
        /// Constructs a project object from the database file path
        /// </summary>
        /// <param name="filePath"></param>
        public Project(string filePath)
        {
            Location = Path.GetDirectoryName(filePath);
            Name = Path.GetFileNameWithoutExtension(filePath);
            this.name = Name;
            this.location = Location;
            PreConstruct();
        }

        #endregion //Constructors

        #region Private Methods

        private void PreConstruct()
        {
            UseMetadataDbForCollectedData = true;
            metadata = new MetadataDbProvider(this);
            collectedData = new CollectedDataProvider(this);            
        }

        #endregion  //Private Methods

        #region Public Properties

        /// <summary>
        /// Gets or sets a reference to the collected data Provider
        /// </summary>
        public CollectedDataProvider CollectedData
        {
            get
            {
                return collectedData;
            }
            set
            {
                collectedData = value;
            }
        }

        /// <summary>
        /// Gets or sets a reference to the Metadata Provider
        /// </summary>
        public MetadataDbProvider Metadata
        {
            get
            {
                return metadata;
            }
            set
            {
                metadata = value;
            }
        }

        /// <summary>
        /// FileExtension property
        /// </summary>
        /// <remarks>
        /// just returns ".mdb" - should it have been a const?
        /// </remarks>
        //		protected override string FileExtension
        public string FileExtension
        {
            get
            {
                return ".mdb";
            }
        }

        /// <summary>
        /// Gets or sets the useMetadataDbForCollectedData property
        /// </summary>
        public bool UseMetadataDbForCollectedData
        {
            get
            {
                return useMetadataDbForCollectedData;
            }
            set
            {
                useMetadataDbForCollectedData = value;
            }
        }

        /// <summary>
        /// The location of the Epi 2000 database file
        /// </summary>
        public string Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
            }
        }

        /// <summary>
        /// The name of the Epi 2000 database file
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// The Epi 2000 database file path
        /// </summary>
        public string FilePath
        {
            get
            {
                if (string.IsNullOrEmpty(Location) || string.IsNullOrEmpty(FileName))
                {
                    return string.Empty;
                }
                else
                {
                    return Path.Combine(Location, FileName);
                }
            }
        }

        /// <summary>
        /// Returns the file name of the project
        /// </summary>
        public string FileName
        {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    return (Name + FileExtension);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Determines if this data source is actually an Epi Info (2000 or 7)collected data db
        /// </summary>
        //		public override bool IsEpiCollectedData
        public bool IsEpiCollectedData
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Views of the project
        /// </summary>
        public Collection<View> Views
        {
            get
            {
                if (views == null)
                {
                    LoadViews();
                }
                return views;
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// CopyCodeTablesTo()
        /// </summary>
        /// <param name="destination"></param>
        public void CopyCodeTablesTo(Epi.Project destination)
        {
            List<string> codeTableList = Metadata.GetCodeTableNames();

            foreach (string codeTableName in codeTableList)
            {
                RaiseEventTableCopyBegin(codeTableName + StringLiterals.ELLIPSIS);

                string[] columnNames = Metadata.GetCodeTableColumnNames(codeTableName);
                
                destination.CreateCodeTable(codeTableName, columnNames);
                
                DataTable CodeTable = this.Metadata.GetCodeTableData(codeTableName);
                int rowIndex = 0;

                foreach (DataRow CodeRow in CodeTable.Rows)
                {
                    RaiseEventTableCopyStatus(codeTableName, rowIndex, CodeTable.Rows.Count);

                    rowIndex++;
                    string[] columnData = new string[columnNames.Length];

                    for (int i = 0; i < columnNames.Length; i++)
                    {
                        columnData[i] = CodeRow[columnNames[i]].ToString();
                    }

                    destination.Metadata.CreateCodeTableRecord(codeTableName, columnNames, columnData);
                }
                
                RaiseEventTableCopyEnd(codeTableName + StringLiterals.ELLIPSIS);
            }
        }

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override DataTable GetCodeTables()
        {
            return null;
        }
        */

        /// <summary>
        /// CreateCodeTable() - not implemented
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames"></param>
        //public override void CreateCodeTable(string tableName, string[] columnNames)
        public void CreateCodeTable(string tableName, string[] columnNames)
        {
            throw new NotImplementedException("Not Implemented: CreateCodeTable");
        }

        /// <summary>
        /// CreateCodeTable() - not implemented
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        //public override void CreateCodeTable(string tableName, string columnName)
        public void CreateCodeTable(string tableName, string columnName)
        {
            throw new NotImplementedException("Not Implemented: CreateCodeTable");
        }

        /// <summary>
        /// Deletes a program from the database
        /// </summary>
        /// <param name="programName">Name of the program to be deleted</param>
        /// <param name="programId">Id of the program to be deleted</param>
        //public override void DeletePgm(string programName, int programId)
        public void DeletePgm(string programName, int programId)
        {
            Metadata.DeletePgm(programName);
        }

        /// <summary>
        /// Checks if the two projects are identical.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        //public override bool Equals(Project other)
        public bool Equals(Project other)
        {
            return (string.Compare(FilePath, other.FilePath, true) == 0);
        }

        /// <summary>
        /// Gets all tables that are not views
        /// </summary>
        /// <returns>DataTable</returns>
        //        public override DataTable GetNonViewTablesAsDataTable()
        public DataTable GetNonViewTablesAsDataTable()
        {
            return (Metadata.GetNonViewTablesAsDataTable());
        }

        /// <summary>
        ///  TODO: Need to implement this method
        /// </summary>
        /// <returns></returns>
        [System.Obsolete]
        //public override DataTable GetDataTableList()
        public DataTable GetDataTableList()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //public override DataTable GetViewsAsDataTable()
        public DataTable GetViewsAsDataTable()
        {
            return (Metadata.GetViewsAsDataTable());
        }

        ///// <summary>
        ///// Gets a list of pgms saved in the project
        ///// </summary>
        ///// <returns>DataTable containing a list of pgms</returns>
        //public override DataTable GetPgms()
        //{
        //    return (Metadata.GetPgms());
        //}

        /// <summary>
        /// Inserts a program into the database
        /// </summary>
        /// <param name="name">Name of the program</param>
        /// <param name="content">Content of the program</param>
        /// <param name="comment">Comment for the program</param>
        /// <param name="author">Author of the program</param>
        //public override void InsertPgm(string name, string content, string comment, string author)
        public void InsertPgm(string name, string content, string comment, string author)
        {
            Metadata.InsertPgm(name, content, comment, author);
        }

        /// <summary>
        /// Method LoadViews()
        /// </summary>
        //protected override void LoadViews()
        public void LoadViews()
        {
            this.views = new Collection<View>();
            System.Collections.Hashtable RelatedViews = new System.Collections.Hashtable();

            DataTable viewsTable = GetViewsAsDataTable();
            foreach (DataRow viewRow in viewsTable.Rows)
            {
                View V = new Epi2000.View(viewRow, this);
                
                // set the is related view attribute
                IDataReader R = this.collectedData.GetTableDataReader(V.Name);
                while(R.Read())
                {
                    
                    if (R["Name"].ToString().ToUpperInvariant().StartsWith("RELVIEW"))
                    {
                        if(! RelatedViews.ContainsKey(R["DataTable"].ToString()))
                        {
                            RelatedViews.Add(R["DataTable"].ToString(), R["DataTable"].ToString());
                        }
                    }
                }
                R.Close();

                this.views.Add(V);
            }


            foreach(Epi2000.View V in this.views)
            {
                if (RelatedViews.ContainsKey(V.Name))
                {
                    V.IsRelatedView = true;
                }
            }
        }
        
        /// <summary>
        /// Raise event indicating the copy has begun.
        /// </summary>
        /// <param name="tableName"></param>
        public void RaiseEventTableCopyBegin(string tableName)
        {
            if (TableCopyBeginEvent != null)
            {
                TableCopyBeginEvent(this, new MessageEventArgs(tableName + StringLiterals.ELLIPSIS));
            }
        }

        /// <summary>
        /// Raise event indicating the copy has ended.
        /// </summary>
        /// <param name="tableName"></param>
        public void RaiseEventTableCopyEnd(string tableName)
        {
            if (TableCopyEndEvent != null)
            {
                string msg = SharedStrings.END + " : " + tableName;
                TableCopyEndEvent(this, new MessageEventArgs(msg));
            }
        }

        /// <summary>
        /// Raises an event indicating the table copy has ended.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="recordCount"></param>
        public void RaiseEventTableCopyStatus(string tableName, int recordCount)
        {
            if (this.TableCopyStatusEvent != null)
            {
                this.TableCopyStatusEvent(this, new TableCopyStatusEventArgs(tableName, recordCount));
            }
        }
        /// <summary>
        /// Raises an event indicating the table copy has ended.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="recordCount">Record Count</param>
        /// <param name="totalRecords">Total Record count</param>
        public void RaiseEventTableCopyStatus(string tableName, int recordCount, int totalRecords)
        {
            if (this.TableCopyStatusEvent != null)
            {
                this.TableCopyStatusEvent(this, new TableCopyStatusEventArgs(tableName, recordCount, totalRecords));
            }
        }

        /// <summary>
        /// SaveCodeTableData() - not implemented
        /// </summary>
        /// <param name="dataTable">Data Table</param>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name</param>
        //public override void SaveCodeTableData(DataTable dataTable,string tableName, string columnName)
        public void SaveCodeTableData(DataTable dataTable, string tableName, string columnName)
        {

        }

        /// <summary>
        /// SaveCodeTableData() - not implemented
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="tablename"></param>
        /// <param name="columnNames"></param>
        //public override void SaveCodeTableData(DataTable dataTable, string tablename, string[] columnNames)
        public void SaveCodeTableData(DataTable dataTable, string tablename, string[] columnNames)
        {

        }

        /// <summary>
        /// Updates a program saved in the database
        /// </summary>
        /// <param name="programId">0 for Epi 2000 projects</param>
        /// <param name="name">Name of the program</param>
        /// <param name="content">Content of the program</param>
        /// <param name="comment">Comment for the program</param>
        /// <param name="author">Author of the program</param>
        //public override void UpdatePgm(int programId, string name, string content, string comment, string author)
        public void UpdatePgm(int programId, string name, string content, string comment, string author)
        {
            Metadata.UpdatePgm(name, content, comment, author);
        }

        /// <summary>
        /// Analyzed the project for any potential import errors. Logs these errors
        /// </summary>
        /// <returns></returns>
        public List<string> Validate()
        {
            // Analyze if any names used are reserved words.
            List<string> validationErrors = new List<string>();
            AppData appData = AppData.Instance;
            foreach (Epi2000.View view in this.Views)
            {
                if (appData.IsReservedWord(view.NameWithoutPrefix))
                {
                    string errorMsg = view.NameWithoutPrefix + " : " + SharedStrings.RESERVED_WORD_INVALID_USE;
                    validationErrors.Add(errorMsg);
                    //Logger.Log(DateTime.Now + ":  " + errorMsg);
                }
                foreach (Epi2000.Page page in view.Pages)
                {
                    // Check page name
                    if (appData.IsReservedWord(page.Name))
                    {
                        string errorMsg = view.Name + "\\" + page.Name + " : " + SharedStrings.RESERVED_WORD_INVALID_USE;
                        validationErrors.Add(errorMsg);
                        //Logger.Log(DateTime.Now + ":  " + errorMsg);
                    }
                    DataTable fieldsTable = Metadata.GetFieldsOnPageAsDataTable(view.NameWithPrefix, page.Position);
                    foreach (DataRow fieldRow in fieldsTable.Rows)
                    {
                        // Check Field Name
                        string fieldName = fieldRow[ColumnNames.NAME].ToString();
                        if (appData.IsReservedWord(fieldName))
                        {
                            string errorMsg = view.Name + "\\" + page.Name + "\\" + fieldName + " : " + SharedStrings.RESERVED_WORD_INVALID_USE;
                            validationErrors.Add(errorMsg);
                            //Logger.Log(DateTime.Now + ":  " + errorMsg);
                        }
                    }
                }
            }
            return (validationErrors);
        }

        #endregion Public Methods
    }
}