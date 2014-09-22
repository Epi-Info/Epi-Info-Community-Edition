#region Namespaces

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

#endregion Namespaces

namespace Epi.Windows.Controls
{
    /// <summary>
    /// A label to be used in MakeView's questionnaire designer
    /// </summary>
    public class TransparentLabel : Label
    {

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="TransparentLabel"/> instance.
        /// </summary>
        public TransparentLabel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
            
            BackColor = Color.Transparent;
            TabStop = false;
        }
        #endregion  //Constructors

        #region Protected Methods
        /// <summary>
        /// Gets the creation parameters.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20;
                return cp;
            }
        }

        /// <summary>
        /// Paints the background.
        /// </summary>
        /// <param name="e">Paint Event argument</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // do nothing
        }

        #endregion  //Protected Methods
    }
}
