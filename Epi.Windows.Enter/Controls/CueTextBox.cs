#region Namespaces
using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
#endregion

namespace Epi.Windows.Enter.Controls
{
    /// <summary>
    /// Text box that provides cue
    /// </summary>
    class CueTextBox : System.Windows.Forms.TextBox
    {
        public CueTextBox()
        {

        }
        [Localizable(true)]
        public string Cue
        {
            get { return mCue; }
            set { mCue = value; updateCue(); }
        }

        private void updateCue()
        {
            if (this.IsHandleCreated && mCue != null)
            {
                SendMessage(this.Handle, 0x1501, (IntPtr)1, mCue);
            }
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            updateCue();
        }
        private string mCue;

        // PInvoke
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, string lp);
    }
}
