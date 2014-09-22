#region Namespaces

using System;
using System.Reflection;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

#endregion  //Namespaces

namespace Epi.Windows.Enter.Dialogs
{
    /// <summary>
    /// This class handles a newly created Print Preview Dialog to allow for the addition of a new 
    /// print button which displays a print dialog in which its settings can be modified.  Currently, 
    /// VS 2005's print preview dialog is not modifiable.
    /// </summary>
    public partial class EnterPrintPreviewDialog : System.Windows.Forms.PrintPreviewDialog
    {
        #region Fields

        private ToolStripButton enterPrintButton;

        #endregion  //Fields

        #region Constructors
        /// <summary>
        /// Constructor for the Print Preview dialog
        /// </summary>        
        public EnterPrintPreviewDialog()
        {
            Type t = typeof(PrintPreviewDialog);
            FieldInfo fieldInfo = t.GetField("toolStrip1", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo fieldInfo2 = t.GetField("printToolStripButton", BindingFlags.Instance | BindingFlags.NonPublic);

            ToolStrip toolStrip1 = (ToolStrip)fieldInfo.GetValue(this);
            ToolStripButton printButton = (ToolStripButton)fieldInfo2.GetValue(this);

            printButton.Visible = false;

            enterPrintButton = new ToolStripButton();
            enterPrintButton.ToolTipText = printButton.ToolTipText;
            enterPrintButton.ImageIndex = 0;

            ToolStripItem[] oldButtons = new ToolStripItem[toolStrip1.Items.Count];

            for (int i = 0; i < oldButtons.Length; i++)
            {
                oldButtons[i] = toolStrip1.Items[i];            
            }

            toolStrip1.Items.Clear();
            toolStrip1.Items.Add(enterPrintButton);

            for (int i = 0; i < oldButtons.Length; i++)
            {
                toolStrip1.Items.Add(oldButtons[i]);             
            }

            toolStrip1.ItemClicked += new ToolStripItemClickedEventHandler(toolStrip1_ItemClicked);
        }

        #endregion  //Constructors

        #region Private Event Handlers
        /// <summary>
        /// Handles the Print Preview Toolstrip click event
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="eventargs">Tool Strip Items Clicked Event parameters</param>
        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs eventargs)
        {
            if (eventargs.ClickedItem == enterPrintButton)
            {
                PrintDialog printDialog = new PrintDialog();
                printDialog.AllowPrintToFile = true;
                printDialog.AllowSelection = false;
                printDialog.AllowSomePages = false;                
                printDialog.PrinterSettings.PrintRange = PrintRange.AllPages;
                printDialog.PrinterSettings.Collate = false;
                printDialog.Document = this.Document;                
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    this.Document.Print();
                }
            }
        }       
        #endregion       

        

    }
}
