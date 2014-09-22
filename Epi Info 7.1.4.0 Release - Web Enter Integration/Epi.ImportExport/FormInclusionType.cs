using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.ImportExport
{
    /// <summary>
    /// For data imports and exports that are based on a form, the program often times needs to know whether
    /// to include data in related forms or not. This enum is designed to assist in how descendant forms are
    /// selected for these operations.
    /// </summary>
    public enum FormInclusionType
    {
        /// <summary>
        /// Will include the current form and all descendant forms, e.g. child forms and grandchild forms.
        /// </summary>
        AllDescendants = 0,

        /// <summary>
        /// Will include the current form and only direct descendant forms, e.g. child forms, but NOT grandchild forms.
        /// </summary>
        DirectDescendants = 1,

        /// <summary>
        /// Will include the current form only and skip ALL descendant forms.
        /// </summary>
        CurrentFormOnly = 2
    }
}
