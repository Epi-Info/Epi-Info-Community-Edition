using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace Epi.ImportExport
{
    public struct ColumnConversionInfo
    {
        public string SourceColumnName;
        public string DestinationColumnName;
        public string Prompt;
        public DbType SourceColumnType;
        public MetaFieldType FieldType;
        public int PageNumber;
        public int TabIndex;
        public bool IsTabStop;
        public bool IsReadOnly;
        public bool IsRequired;
        public bool IsRepeatLast;
        public bool HasRange;
        public object LowerBound;
        public object UpperBound;
        public string ListSourceTableName;
        public string ListSourceTextColumnName;
        public DataTable ListSourceTable;
        public Font ControlFont;
        public Font PromptFont;
        public double ControlLeftPosition;
        public double ControlTopPosition;
        public double PromptLeftPosition;
        public double PromptTopPosition;
    }
}
