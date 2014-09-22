using System;
using System.Collections.Generic;
using Epi.Fields;

namespace Epi.ImportExport.ProjectPackagers
{
    public struct PackageFieldData
    {
        public string RecordGUID;
        public string FieldName;
        public object FieldValue { get; set; }
        public Page Page;
        public Dictionary<Field, object> KeyValues;
    }
}
