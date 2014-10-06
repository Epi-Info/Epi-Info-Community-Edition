using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.Enter
{
    public class EnterUIConfig
    {
        public Dictionary<View, bool> ShowFileMenu = new Dictionary<View, bool>();
        public Dictionary<View, bool> ShowToolbar = new Dictionary<View, bool>();
        public Dictionary<View, bool> ShowOpenFormButton = new Dictionary<View, bool>();
        public Dictionary<View, bool> ShowSaveRecordButton = new Dictionary<View, bool>();
        public Dictionary<View, bool> ShowPrintButton = new Dictionary<View, bool>();
        public Dictionary<View, bool> ShowFindButton = new Dictionary<View, bool>();
        public Dictionary<View, bool> ShowNewRecordButton = new Dictionary<View, bool>();
        public Dictionary<View, bool> ShowNavButtons = new Dictionary<View, bool>();
        public Dictionary<View, bool> ShowRecordCounter = new Dictionary<View, bool>();
        public Dictionary<View, bool> ShowDeleteButtons = new Dictionary<View, bool>();

        public Dictionary<View, bool> ShowLineListButton = new Dictionary<View, bool>();
        public Dictionary<View, bool> ShowDashboardButton = new Dictionary<View, bool>();
        public Dictionary<View, bool> ShowMapButton = new Dictionary<View, bool>();
        public Dictionary<View, bool> ShowEditFormButton = new Dictionary<View, bool>();
        public bool ShowLinkedRecordsViewer = true;
        public Dictionary<View, bool> AllowOneRecordOnly = new Dictionary<View, bool>();
        public Dictionary<View, bool> DisableCodeTableCache = new Dictionary<View, bool>();
    }
}
