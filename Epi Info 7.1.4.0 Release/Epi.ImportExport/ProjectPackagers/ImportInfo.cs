using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.ImportExport.ProjectPackagers
{
    public class ImportInfo
    {
        public int FormsProcessed;
        public int GridsProcessed;
        public int TotalRecordsUpdated;
        public int TotalRecordsAppended;
        public Dictionary<View, int> RecordsUpdated;
        public Dictionary<View, int> RecordsAppended;
        public ImportExportErrorList Messages;
        public TimeSpan TimeElapsed;
        public DateTime ImportInitiated;
        public DateTime ImportCompleted;
        public string UserID;
        public bool Succeeded;

        public ImportInfo()
        {
            FormsProcessed = 0;
            GridsProcessed = 0;
            TotalRecordsAppended = 0;
            TotalRecordsUpdated = 0;
            RecordsUpdated = new Dictionary<View, int>();
            RecordsAppended = new Dictionary<View, int>();
            Messages = new ImportExportErrorList();
            UserID = string.Empty;
            Succeeded = false;
        }

        public void AddNotification(string message, string code)
        {
            Messages.Add(ImportExportMessageType.Notification, code, message);
        }

        public void AddWarning(string message, string code)
        {
            Messages.Add(ImportExportMessageType.Warning, code, message);
        }

        public void AddError(string message, string code)
        {
            Messages.Add(ImportExportMessageType.Error, code, message);
        }
    }
}
