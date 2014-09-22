using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Epi.Windows.Dialogs;

namespace Epi.Windows.MakeView.Dialogs
{
    public partial class SupportedFieldTypeAndroidDialog : DialogBase
    {
        #region Constructors

        public SupportedFieldTypeAndroidDialog()
        {
            InitializeComponent();
        }

        public SupportedFieldTypeAndroidDialog(string fieldList)
        {
            InitializeComponent();
            Construct(fieldList);
        }

        #endregion //Constructors

        #region Private Methods

        /// <summary>
        /// Loads the dialog
        /// </summary>
        private void Construct(string fieldList)
        {
            string[] listOfFields = fieldList.Split(';');
            foreach (string fieldNameType in listOfFields)
            {
                if (fieldNameType.Length > 0) lbxFields.Items.Add(fieldNameType);
            }
        }
        #endregion  //Private Methods
    }
}
