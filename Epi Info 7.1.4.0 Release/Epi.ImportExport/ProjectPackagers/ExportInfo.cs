using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.ImportExport.ProjectPackagers
{
    public class ExportInfo
    {
        public int FormsProcessed;
        public int GridsProcessed;
        public int TotalRecordsPackaged;
        public Dictionary<View, int> RecordsPackaged;
        public ImportExportErrorList Messages;
        public TimeSpan TimeElapsed;
        public DateTime ExportInitiated;
        public DateTime ExportCompleted;
        public string UserID;
        public bool Succeeded;

        public ExportInfo()
        {
            FormsProcessed = 0;
            GridsProcessed = 0;
            TotalRecordsPackaged = 0;
            RecordsPackaged = new Dictionary<View, int>();
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
