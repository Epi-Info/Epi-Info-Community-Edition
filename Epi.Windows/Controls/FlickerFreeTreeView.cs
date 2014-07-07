#region Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Windows.Controls;
using System.Runtime.InteropServices;

#endregion  //Namespaces

namespace Epi.Windows
{
   /// <summary>
   /// Eliminates flicker of the tree view
   /// </summary>
    public class FlickerFreeTreeView : TreeView
    {
      /*  #region //Constructors

        /// <summary>
        /// The default constructor
        /// </summary>
        public FlickerFreeTreeView()
            : base()
        {
        }

        #endregion  //Constructors

        #region Protected Methods

        /// <summary>
        /// Resets WndProc messages 
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            //prevent flicker
            if ((int)0x0014 == m.Msg) // erase background
            {
                m.Msg = (int)0x0000; // reset message to null
            }
            base.WndProc(ref m);
        }
        #endregion //Protected Methods*/
    

        protected override void OnHandleCreated(EventArgs e) 
        {
            SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            base.OnHandleCreated(e);
        }

        // Pinvoke:
        private const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
        private const int TVM_GETEXTENDEDSTYLE = 0x1100 + 45;
        private const int TVS_EX_DOUBLEBUFFER = 0x0004;
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
    }
}