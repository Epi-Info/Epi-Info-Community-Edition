using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Xml;
using System.Text;
using System.IO;

using System.Collections.Generic;

namespace Epi
{
    /// <summary>
    /// Process Command Results event handler delegate.
    /// </summary>
    /// <param name="results">Command Processor Results</param>
    public delegate void ProcessCommandResultsHandler(CommandProcessorResults results);
    /// <summary>
    /// Command Processor Results class
    /// </summary>
	public class CommandProcessorResults
	{
		#region Private Attributes
		private ArrayList actions = new ArrayList();
		private bool resetOutput = false;
        private string htmlOutput = string.Empty;
        private XmlDocument xmlOutput = null;
        private DataSet dsOutput = null;
        private string fileNameOutput;
        private string outTableName;
        private ArrayList commandBlock = new ArrayList();
        private Dictionary<Epi.Action, Collection<string>> checkCodeList;        
		
        #endregion Private Attributes

		#region Constructors
		/// <summary>
		/// command processor results
		/// </summary>
		public CommandProcessorResults()
		{
		}
		#endregion Constructors

        #region Public Methods
        /// <summary>
        /// Reset Command Processor
        /// </summary>
        public void Clear()
        {
            if (dsOutput != null) // DEFECT: 200
            {
                dsOutput.Dispose();
            }
            else
            {
                dsOutput = null;
            }
            htmlOutput = string.Empty;
            xmlOutput = null;
            actions.Clear();
        }

        #endregion Public Methods

        #region Public Properties

        /// <summary>
		/// List of actions to perform after the command has executed.
		/// </summary>
		public ArrayList Actions
		{
			get
			{
				return actions;
			}
		}

		/// <summary>
		/// Indicates if the output stream should be reset before adding results.
		/// </summary>
		public bool ResetOutput
		{
			get
			{
				return this.resetOutput;
			}
			set
			{
				resetOutput = value;
			}
		}

		/// <summary>
		/// Gets/sets the <see cref="System.Data.DataSet"/>  output for command processor
		/// </summary>
		public DataSet DsOutput
		{
			get
			{
                if (this.dsOutput == null)
                {
                    this.dsOutput = new DataSet();
                }

				return this.dsOutput;
			}
			set
			{
				this.dsOutput = value;
			}
		}

        /// <summary>
        /// Gets/sets the HTML output for command processor
        /// </summary>
        public string HtmlOutput
        {
            get
            {
                return this.htmlOutput;
            }
            set
            {
                this.htmlOutput = value;
            }
        }

		/// <summary>
		/// Gets/sets the Xml output for command processor
		/// </summary>
		public  XmlDocument XmlOutput 
		{
			get
			{
				return this.xmlOutput;
			}
			set
			{
				this.xmlOutput = value;
			}

        }

        /// <summary>
        /// Gets/sets the name of a file for command processor
        /// </summary>
        public string FileNameOutput
        {
            get { return fileNameOutput; }
            set { fileNameOutput = value; }
        }

        /// <summary>
        /// Gets/sets the name of a file for command processor
        /// </summary>
        public string OutTableName
        {
            get { return outTableName; }
            set { outTableName = value; }
        }

        /// <summary>
        /// Gets/sets the Menu command block for command processor
        /// </summary>
        public ArrayList MenuCommandBlock
        {
            get
            {
                return this.commandBlock;
            }
            set
            {
                this.commandBlock = value;
            }

        }

        /// <summary>
        /// Gets/sets the list which houses items in which check code actions will be executed upon
        /// </summary>
        //public ArrayList CheckCodeList
        public Dictionary<Epi.Action, Collection<string>> CheckCodeList
        {
            get
            {
                if (this.checkCodeList == null)
                {
                    checkCodeList = new Dictionary<Epi.Action, Collection<string>>();
                }                
                return this.checkCodeList;
            }
            set
            {
                this.checkCodeList = value;
            }
        }

        #endregion Public Properties

    }
}