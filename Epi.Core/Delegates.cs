using System;

namespace Epi
{    
    #region Custom EventArgs

    /// <summary>
    /// MessageEvent arguments class
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        #region Private Attributes
        private string message;
        private bool createLogEntry;        
        #endregion Private Attributes

        #region Constructor
        /// <summary>
        /// Constructor for the MessageEventArgs 
        /// </summary>
        /// <param name="msg"></param>
        public MessageEventArgs(string msg)
        {
            this.message = msg;
            createLogEntry = true;
        }

        /// <summary>
        /// Constructor for the MessageEventArgs 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="createLogEntry"></param>
        public MessageEventArgs(string msg, bool createLogEntry)
        {
            this.message = msg;
            this.createLogEntry = createLogEntry;
        }

        #endregion Constructor

        #region Public Properties

        /// <summary>
        /// Public Property get for the Message
        /// </summary>
        public string Message
        {
            get { return message; }
        }

        /// <summary>
        /// Gets whether or not to create a log entry for this event
        /// </summary>
        public bool CreateLogEntry
        {
            get { return this.createLogEntry; }
        }
        #endregion Public Properties
    }


    /// <summary>
    /// Event parameters for the ImportStarted event
    /// </summary>
    public class ImportStartedEventArgs : EventArgs
    {
        #region Private Members
        private int objectCount;
        #endregion

        #region Constructors

        /// <summary>
        /// Constructs and initializes a new Import Event argument
        /// </summary>
        /// <param name="count">Total number of views in a EI 3.x project</param>
        public ImportStartedEventArgs(int count)
        {
            objectCount = count;
        }

        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Gets the total number of views in a EI 3.x project
        /// </summary>
        public int ObjectCount
        {
            get
            {
                return objectCount;
            }
        }

        #endregion Public Properties
    }

    ///// <summary>
    ///// class ImportStatusEventArgs
    ///// </summary>
    //public class ImportStatusEventArgs : EventArgs
    //{
    //    #region Private Members
    //    private string message = string.Empty;
    //    #endregion Private Members

    //    #region Constructors

    //    /// <summary>
    //    /// Constructor for ImportStatusEventArgs
    //    /// </summary>
    //    /// <param name="message"></param>
    //    public ImportStatusEventArgs(string message)
    //    {
    //        this.message = message;
    //    }
    //    #endregion Constructors

    //    #region Public Properties

    //    /// <summary>
    //    /// Returns the message
    //    /// </summary>
    //    public string Message
    //    {
    //        get
    //        {
    //            return message;
    //        }
    //    }
    //    #endregion Public Properties
    //}

    /// <summary>
    /// TableCopyStatusEventArgs
    /// </summary>
    public class TableCopyStatusEventArgs : EventArgs
    {
        #region Private Attributes

        private int recordCount = 0;
        private string tableName = string.Empty;
        private int totalRecordCount = 0;

        #endregion Private Attributes

        #region Constructors
        /// <summary>
        /// Constructor for the TableCopyStatusEventArgs
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="recCount"></param>
        public TableCopyStatusEventArgs(string tableName, int recCount)
        {
            this.tableName = tableName;
            this.recordCount = recCount;
        }
        /// <summary>
        /// Constructor for the TableCopyStatusEventArgs
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="recCount">Record Count</param>
        /// <param name="totalRecords">Total number of records</param>
        public TableCopyStatusEventArgs(string tableName, int recCount, int totalRecords)
        {
            this.tableName = tableName;
            this.recordCount = recCount;
            this.totalRecordCount = totalRecords;
        }
        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// Property get for RecordCount
        /// </summary>
        public int RecordCount
        {
            get { return this.recordCount; }
        }

        /// <summary>
        /// Property get for TableName
        /// </summary>
        public string TableName
        {
            get { return this.tableName; }
        }

        /// <summary>
        /// Property get for TotalRecords
        /// </summary>
        public int TotalRecords
        {
            get { return this.totalRecordCount; }
        }

        #endregion Public Properties
    }

    public class SaveRecordEventArgs : EventArgs
    {
        private View _form;
        private string _recordGuid;
        private string _fKey;

        public SaveRecordEventArgs()
        {
        }

        public SaveRecordEventArgs(View form, string recordGuid, string fKey = "")
        {
            this._form = form;
            this._recordGuid = recordGuid;
            this._fKey = fKey;
        }

        public View Form { get { return this._form; } }
        public string RecordGuid { get { return this._recordGuid; } }
        public string RecordFkey { get { return this._fKey; } }
    }
    
    #endregion CustomEventArgs

    #region Delegates

    /// <summary>
    /// The delegate to be invoked at the start of "busy"
    /// </summary>
    /// <param name="message"></param>
    public delegate void BeginBusyEventHandler(string message);

    /// <summary>
    /// The delegate to be invoked at the end of "busy"
    /// </summary>
    public delegate void EndBusyEventHandler();

    /// <summary>
    /// The delegate that handles updating a dashboard gadget's status message
    /// </summary>
    /// <param name="statusMessage">The status message to display</param>
    /// <param name="progress">The amount of progress by which to increment</param>
    public delegate void SetGadgetStatusHandler(string statusMessage, double progress = 0);

    /// <summary>
    /// The delegate that handles updating both a progress bar value and a text status message
    /// </summary>
    /// <param name="statusMessage">The status message to display</param>
    /// <param name="progress">The amount of progress by which to increment</param>
    public delegate void SetProgressAndStatusHandler(string statusMessage, double progress = 0);

    /// <summary>
    /// The delegate that handles updating a progress bar.
    /// </summary>
    /// <param name="progress">The amount of progress</param>
    public delegate void SetProgressBarDelegate(double progress);

    /// <summary>
    /// The delegate that handles setting a progress bar's max value
    /// </summary>
    /// <param name="progress">The maximum attainable progress</param>
    public delegate void SetMaxProgressBarValueDelegate(double maxProgress);

    /// <summary>
    /// The delegate that handles checking whether a dashboard gadget has been cancelled
    /// </summary>
    public delegate bool CheckForCancellationHandler();
        
    /// <summary>
    /// Event handler that delivers a message
    /// </summary>
    /// <param name="sender">Object that fired the event</param>
    /// <param name="e">.NET supplied event parameters</param>
    public delegate void MessageEventHandler(object sender, MessageEventArgs e);

    /// <summary>
    /// the delegate to be invoked when the ProgressReportBegin Event arises
    /// </summary>
    /// <param name="min">the minimum value of the progress bar</param>
    /// <param name="max">the maximum value of the progress bar</param>
    /// <param name="step">how much to step the progress bar for each event</param>
    public delegate void ProgressReportBeginEventHandler(int min, int max, int step);

    /// <summary>
    /// the delegate to be invoked when the ProgressReportUpdate event occurs
    /// </summary>
    public delegate void ProgressReportUpdateEventHandler();

    /// <summary>
    /// Event handler with no parameters
    /// </summary>
    public delegate void SimpleEventHandler();

    /// <summary>
    /// Event handler to report table copy status
    /// </summary>
    /// <param name="sender">The object that requested this service</param>
    /// <param name="e">TableCopyStatusEventArgs</param>
    public delegate void TableCopyStatusEventHandler(object sender, TableCopyStatusEventArgs e);

    /// <summary>
    /// The delegate to be invoked when the UpdateStatusEvent arises
    /// </summary>
    /// <param name="message"></param>
    public delegate void UpdateStatusEventHandler(string message);

    public delegate void SaveRecordEventHandler(object sender, SaveRecordEventArgs e); 

    #endregion Delegates
}