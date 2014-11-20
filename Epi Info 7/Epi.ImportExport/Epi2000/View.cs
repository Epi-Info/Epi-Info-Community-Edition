using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using Epi.Collections;

namespace Epi.Epi2000
{
	/// <summary>
	/// A view in Epi2000 format
	/// </summary>
    public class View
	{
        #region Fields
		private string name = string.Empty;
		private bool isRelatedView = false;
        private bool isWideTableView = false;
		private string checkCodeVariableDefinitions = string.Empty;
		private string checkCodeBefore = string.Empty;
		private string checkCodeAfter = string.Empty;
		private string recordCheckCodeBefore = string.Empty;
		private string recordCheckCodeAfter = string.Empty;
		//private bool mustRefreshFieldCollection = false;
        //private string dataTableName = string.Empty;
        private readonly List<string> dataTableNames = new List<string>();
		private int id = 0;
        private Epi2000.View parentView;
        
		/// <summary>
		/// Collection of all pages of the view
		/// </summary>
        protected List<Page> pages = null;
        /// <summary>
        /// Project this view is part of.
        /// </summary>
        protected Project project;
		#endregion Fields
        
		#region Constructors     
        
        /// <summary>
        /// Default constructor for a View
        /// </summary>
		public View()
		{
		}

        /// <summary>
        /// Constructor to build a view from a Epi 2000 project
        /// </summary>
        /// <param name="proj"></param>
        public View(Project proj)
        {
            project = proj;
        }

        /// <summary>
        /// Copy a view
        /// </summary>
        /// <param name="other"></param>
        public void CopyTo(Epi.View other)
        {
            other.Name = this.NameWithoutPrefix;
            other.IsRelatedView = this.IsRelatedView;
            /*
            other.CheckCodeVariableDefinitions = this.CheckCodeVariableDefinitions;
            other.CheckCodeBefore = this.CheckCodeBefore;
            other.WebSurveyId = this.CheckCodeAfter;
            other.RecordCheckCodeBefore = this.RecordCheckCodeBefore;
            other.RecordCheckCodeAfter = this.RecordCheckCodeAfter;
            other.IsRelatedView = this.IsRelatedView;*/            
        }


        /// <summary>
        /// Constructs a new view from a data row
        /// </summary>
        /// <param name="row">Data row containing view information</param>
        /// <param name="proj"></param>
        public View(DataRow row, Project proj)            
        {
            project = proj;

            Name = row[ColumnNames.NAME].ToString();
            System.Text.StringBuilder CheckCode = new System.Text.StringBuilder();


            // DefineVariable CheckCode Block
            if (!string.IsNullOrEmpty(row[ColumnNames.CHECK_CODE_VARIABLE_DEFINITIONS].ToString()))
            {
                CheckCode.Append("\nDefineVariables\n\t");
                CheckCode.Append(row[ColumnNames.CHECK_CODE_VARIABLE_DEFINITIONS].ToString().Replace("\n","\n\t\t"));
                CheckCode.Append("\nEnd-DefineVariables");
            }

            // View CheckCode Block
            if (
                   (!string.IsNullOrEmpty(row[ColumnNames.CHECK_CODE_BEFORE].ToString()))
                ||
                   (!string.IsNullOrEmpty(row[ColumnNames.CHECK_CODE_AFTER].ToString()))
                )
            {

                CheckCode.Append("\nView\n\t");
                if (!string.IsNullOrEmpty(row[ColumnNames.CHECK_CODE_BEFORE].ToString()))
                {
                    CheckCode.Append("Before\n\t\t");
                    //CheckCodeBefore = row[ColumnNames.CHECK_CODE_BEFORE].ToString().Replace("\n", "\n\t\t\t");
                    CheckCode.Append(row[ColumnNames.CHECK_CODE_BEFORE].ToString().Replace("\n", "\n\t\t\t"));
                    CheckCode.Append("\n\tEnd-Before\n");
                }

                if (!string.IsNullOrEmpty(row[ColumnNames.CHECK_CODE_AFTER].ToString()))
                {
                    CheckCode.Append("After\n\t\t");
                    //CheckCodeAfter = row[ColumnNames.CHECK_CODE_AFTER].ToString().Replace("\n", "\n\t\t\t");
                    CheckCode.Append(row[ColumnNames.CHECK_CODE_AFTER].ToString().Replace("\n", "\n\t\t\t"));
                    CheckCode.Append("\n\tEnd-After\n\t\t");
                }
                CheckCode.Append("\nEnd-View");
            }


            // Record CheckCode Block
            if (
                   (!string.IsNullOrEmpty(row[ColumnNames.RECORD_CHECK_CODE_BEFORE].ToString()))
                ||
                   (!string.IsNullOrEmpty(row[ColumnNames.RECORD_CHECK_CODE_AFTER].ToString()))
                )
            {

                CheckCode.Append("\nRecord\n\t");
                if (!string.IsNullOrEmpty(row[ColumnNames.RECORD_CHECK_CODE_BEFORE].ToString()))
                {
                    CheckCode.Append("Before\n\t\t");
                    //CheckCodeBefore = row[ColumnNames.RECORD_CHECK_CODE_BEFORE].ToString().Replace("\n", "\n\t\t\t");
                    CheckCode.Append(row[ColumnNames.RECORD_CHECK_CODE_BEFORE].ToString().Replace("\n", "\n\t\t\t"));
                    CheckCode.Append("\n\tEnd-Before\n");
                }

                if (!string.IsNullOrEmpty(row[ColumnNames.RECORD_CHECK_CODE_AFTER].ToString()))
                {
                    CheckCode.Append("After\n\t\t");
                    //CheckCodeAfter = row[ColumnNames.RECORD_CHECK_CODE_AFTER].ToString().Replace("\n", "\n\t\t\t");
                    CheckCode.Append(row[ColumnNames.RECORD_CHECK_CODE_AFTER].ToString().Replace("\n", "\n\t\t\t"));
                    CheckCode.Append("\n\tEnd-After\n\t\t");
                }
                CheckCode.Append("\nEnd-Record");
            }

            CheckCodeBefore = CheckCode.ToString();
            /*
            CheckCodeVariableDefinitions = row[ColumnNames.CHECK_CODE_VARIABLE_DEFINITIONS].ToString();
            CheckCodeBefore = row[ColumnNames.CHECK_CODE_BEFORE].ToString();
            CheckCodeAfter = row[ColumnNames.CHECK_CODE_AFTER].ToString();
            RecordCheckCodeBefore = row[ColumnNames.RECORD_CHECK_CODE_BEFORE].ToString();
            RecordCheckCodeAfter = row[ColumnNames.RECORD_CHECK_CODE_AFTER].ToString();*/
            
            // dataTableName = row[ColumnNames.DATA_TABLE_NAME].ToString();
            dataTableNames.Add(row[ColumnNames.DATA_TABLE_NAME].ToString());
            if (dataTableNames[0].Contains(";"))
            {
                IsWideTableView = true;
                string[] tableNames = dataTableNames[0].Split(';');
                
                dataTableNames.Clear();

                foreach (string s in tableNames)
                {
                    dataTableNames.Add(s);
                }                
            }
        }

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Returns the name of the view
		/// </summary>
		public string Name        
		{
			get
			{
				return (name);
			}
			set
			{
				name = value;
			}
		}

        /// <summary>
        /// Gets the name of the view's data table
        /// </summary>
        public string TableName
        {
            get
            {
                if (TableNames.Count <= 0)
                {
                    return string.Empty;
                }
                else
                {
                    return TableNames[0];
                }
            }
        }

        /// <summary>
        /// Gets the names of all the view's data tables
        /// </summary>
        public List<string> TableNames
        {
            get
            {
                return dataTableNames;
            }
        }

		/// <summary>
		/// /// Returns the name of the view with the prefix
		/// </summary>
		public string NameWithPrefix
		{
			get
			{
				if (Name.ToLower().StartsWith("view"))
				{
					return Name;
				}
				else
				{
					return ("view" + Name);
				}
			}
		}

		/// <summary>
		/// /// Returns the name of the view without the prefix
		/// </summary>
		public string NameWithoutPrefix
		{
			get
			{
				if (Name.ToLower().StartsWith("view"))
				{
					return Name.Substring(4);
				}
				else
				{
					return Name;
				}
			}
		}

        /// <summary>
        /// Gets or sets the IsRelatedView attribute
        /// </summary>
		public bool IsRelatedView
		{
			get
			{
				return (isRelatedView);
			}
			set
			{
				isRelatedView = value;
			}
		}

        /// <summary>
        /// Gets or sets the IsWideTableView attribute
        /// </summary>
        public bool IsWideTableView
        {
            get
            {
                return (isWideTableView);
            }
            set
            {
                isWideTableView = value;
            }
        }

        /// <summary>
        /// Gets or sets the CheckCodeVariableDefinitions attribute
        /// </summary>
		public string CheckCodeVariableDefinitions
		{
			get
			{
				return (checkCodeVariableDefinitions);
			}
			set
			{
				checkCodeVariableDefinitions = value;
			}
		}

        /// <summary>
        /// Gets or sets the CheckCodeAfter attribute
        /// </summary>
		public string CheckCodeAfter
		{
			get
			{
				return (checkCodeAfter);
			}
			set
			{
				checkCodeAfter = value;
			}
		}

        /// <summary>
        /// Gets or sets the CheckCodeBefore attribute
        /// </summary>
		public string CheckCodeBefore
		{
			get
			{
				return (checkCodeBefore);
			}
			set
			{
				checkCodeBefore = value;
			}
		}

        /// <summary>
        /// Gets or sets the CheckCodeAfter attribute
        /// </summary>
		public string RecordCheckCodeAfter
		{
			get
			{
				return (recordCheckCodeAfter);
			}
			set
			{
				recordCheckCodeAfter = value;
			}
		}

        /// <summary>
        /// Gets or sets the RecordCheckCodeBefore attribute
        /// </summary>
		public string RecordCheckCodeBefore
		{
			get
			{
				return (recordCheckCodeBefore);
			}
			set
			{
				recordCheckCodeBefore = value;
			}
		}
		

        //public string FullName
        //{
        //    get
        //    {
        //        return (Project.FullName + StringLiterals.COLON + this.Name);
        //    }
        //}

        ///// <summary>
        ///// Overrides TableName property value
        ///// </summary>
        ///// <param name="tableName"></param>
        //public void SetTableName(string tableName)
        //{
        //    this.TableName = tableName;
        //}		

		/// <summary>
        /// Gets or sets the Id attribute
		/// </summary>
		public virtual int Id
		{
			get
			{
				return id;
			}
			set
			{
				id = value;
			}
		}

        /// <summary>
        /// View that this view is related to if IsRelatedView = true.
        /// </summary>
        public View ParentView
        {
            get { return parentView; }
            set { parentView = value; }
        }


        /// <summary>
        /// Returns a collection of all pages of the view
        /// </summary>
        public List<Page> Pages
        {
            get
            {
                if (pages == null)
                {
//                    pages = GetMetadata().GetViewPages(this);
                    pages = this.project.Metadata.GetViewPages(this);
                }
                return (pages);
            }
        }

		#endregion Public Methods			

        #region Public Methods
        /// <summary>
        /// Returns the project object
        /// </summary>
        /// <returns></returns>
        public Epi2000.Project GetProject()
        {
            return (Epi2000.Project)project;
        }

        /// <summary>
        /// Gets the record count for the current view
        /// </summary>
        /// <returns>Record count</returns>
        public int GetRecordCount()
        {
            return (this.GetProject().CollectedData.GetRecordCount(this));
        }

        /// <summary>
        /// Strips the view name of it's prefix.
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public static string StripViewNameOfPrefix(string viewName)
        {
            if (viewName.ToLower().StartsWith("view"))
            {
                return viewName.Substring(4);
            }
            else
            {
                return (viewName);
            }
        }
        #endregion Public Methods
    }
}