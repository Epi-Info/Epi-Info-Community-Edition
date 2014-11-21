using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.ImportExport.ProjectPackagers
{
    public class ImportInfo
    {
        private Dictionary<View, List<string>> _recordIdsUpdated = new Dictionary<View, List<string>>();
        private Dictionary<View, List<string>> _recordIdsAppended = new Dictionary<View, List<string>>();

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

        /// <summary>
        /// Adds a record GUID to the list of record GUIDs that were updated during the import operation.
        /// </summary>
        /// <param name="form">The Epi Info 7 form that the updated record belongs to</param>
        /// <param name="id">The GUID of the updated record</param>
        public void AddRecordIdAsUpdated(View form, string id)
        {
            if (_recordIdsUpdated.ContainsKey(form))
            {
                List<string> recordIds = _recordIdsUpdated[form];
                if (!recordIds.Contains(id))
                {
                    recordIds.Add(id);
                }
            }
            else
            {
                _recordIdsUpdated.Add(form, new List<string>() { id });
            }
        }

        /// <summary>
        /// Adds a record GUID to the list of record GUIDs that were appended (inserted) during the import operation.
        /// </summary>
        /// <param name="form">The Epi Info 7 form that the appended record belongs to</param>
        /// <param name="id">The GUID of the appended record</param>
        public void AddRecordIdAsAppended(View form, string id)
        {
            if (_recordIdsAppended.ContainsKey(form))
            {
                List<string> recordIds = _recordIdsAppended[form];
                if (!recordIds.Contains(id))
                {
                    recordIds.Add(id);
                }
            }
            else
            {
                _recordIdsAppended.Add(form, new List<string>() { id });
            }
        }

        /// <summary>
        /// Gets a list of all of the records that were appended (inserted) during the import operation for the specified form
        /// </summary>
        /// <param name="form">The Epi Info 7 form for which the records were appended</param>
        /// <returns>IEnumerable; a collection of strings that represent the GUIDs for each of the appended records</returns>
        public IEnumerable<string> GetAppendedRecordIdsForForm(View form)
        {
            if (_recordIdsAppended.ContainsKey(form))
            {
                return _recordIdsAppended[form].AsEnumerable();
            }

            else
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Gets a list of all of the records that were updated during the import operation for the specified form
        /// </summary>
        /// <param name="form">The Epi Info 7 form for which the records were updated</param>
        /// <returns>IEnumerable; a collection of strings that represent the GUIDs for each of the updated records</returns>
        public IEnumerable<string> GetUpdatedRecordIdsForForm(View form)
        {
            if (_recordIdsUpdated.ContainsKey(form))
            {
                return _recordIdsUpdated[form].AsEnumerable();
            }

            else
            {
                return new List<string>();
            }
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
