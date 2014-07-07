using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Epi.Windows.Controls
{

    /// <summary>
    /// A list view item that holds a hidden value
    /// </summary>
    public class ListViewValuedItem : ListViewItem 
    {
        private object val;

        /// <summary>
        /// Gets or sets the list view item's value
        /// </summary>
        public object Value
        {
            get
            {
                return val;
            }
            set
            {
                val = value;
            }
        }

    }
}
