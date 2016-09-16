using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epi.Windows.MakeView.Excel
{
    public static class FieldType
    {
        public enum Types
        {
            Text = 1,
            Numeric = 5,
            YesNo = 11,
            Checkbox = 10,
            Options = 12,
            Dropdown = 17,
            Date = 7,
        };
    }
}
