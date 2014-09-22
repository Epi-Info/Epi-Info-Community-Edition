#region Namespaces

using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Collections;

#endregion  //Namespaces

namespace Epi.Windows.Controls
{

    /// <summary>
    /// A group box to be used in MakeView's questionnaire designer
    /// </summary>
    public class FieldGroupBox : GroupBox
    {
        /// <summary>
        /// The Default Constructor
        /// </summary>
        public FieldGroupBox()
            : base()
        {
            FlatStyle = FlatStyle.Standard;
        }
    }
}
