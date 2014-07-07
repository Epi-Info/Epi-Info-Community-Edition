using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard
{
    public struct FieldFlags
    {
        public bool IsDropDownListField;
        public bool IsCommentLegalField;
        public bool IsOptionField;
        public bool IsRecodedField;

        public FieldFlags(bool isDropDownListField, bool isCommentLegalField, bool isOptionField, bool isRecodedField)
        {
            IsDropDownListField = isDropDownListField;
            IsCommentLegalField = isCommentLegalField;
            IsOptionField = isOptionField;
            IsRecodedField = isRecodedField;
        }
    }
}
