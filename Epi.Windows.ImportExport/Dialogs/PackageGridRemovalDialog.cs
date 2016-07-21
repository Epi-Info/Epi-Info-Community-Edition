using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Epi;
using Epi.Fields;
using Epi.ImportExport;
using Epi.Windows;

namespace Epi.Windows.ImportExport.Dialogs
{
    public partial class PackageGridRemovalDialog : Form
    {
        #region Private Members
        private Dictionary<string, List<string>> fieldsToNull;
        private Project sourceProject;
        private string formName;
        #endregion // Private Members

        public PackageGridRemovalDialog()
        {
            InitializeComponent();
        }

        public PackageGridRemovalDialog(Project sourceProject, string formName)
        {
            InitializeComponent();
            this.sourceProject = sourceProject;
            this.formName = formName;

            FillGrids();

            fieldsToNull = new Dictionary<string, List<string>>();
            DeselectFields();

            if (fieldsToNull.Count == 0)
            {
                foreach (View currentView in sourceProject.Views)
                {
                    foreach (GridField gridField in currentView.Fields.GridFields)
                    {
                        fieldsToNull.Add(currentView.Name + ":" + gridField.Name, new List<string>());                        
                    }
                }
            }
        }

        public PackageGridRemovalDialog(Project sourceProject, string formName, Dictionary<string, List<string>> gridFieldsToNull)
        {
            InitializeComponent();
            this.sourceProject = sourceProject;
            this.formName = formName;

            FillGrids();

            if (gridFieldsToNull != null)
            {
                this.fieldsToNull = gridFieldsToNull;
            }
            else
            {
                this.fieldsToNull = new Dictionary<string, List<string>>();
            }

            DeselectFields();

            if (fieldsToNull.Count == 0)
            {
                foreach (View currentView in sourceProject.Views)
                {
                    foreach (GridField gridField in currentView.Fields.GridFields)
                    {
                        fieldsToNull.Add(currentView.Name + ":" + gridField.Name, new List<string>());                        
                    }
                }
            }
        }

        public Dictionary<string, List<string>> FieldsToNull 
        {
            get
            {
                return this.fieldsToNull;
            }
        }

        private void FillGrids()
        {
            string selectedForm = formName;

            if (sourceProject.Views.Contains(selectedForm))
            {
                cmbGridSelector.Items.Clear();

                View parentView = sourceProject.Views[selectedForm];

                foreach (GridField gridField in parentView.Fields.GridFields)
                {
                    cmbGridSelector.Items.Add(parentView.Name + ":" + gridField.Name);
                }

                foreach (View view in sourceProject.Views)
                {
                    if (ImportExportHelper.IsFormDescendant(view, parentView))
                    {
                        foreach (GridField gridField in view.Fields.GridFields)
                        {
                            cmbGridSelector.Items.Add(view.Name + ":" + gridField.Name);
                        }
                    }
                }

                cmbGridSelector.SelectedIndex = -1;
            }            
        }

        private void cmbFormSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillFields();
        }

        private void lbxFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbGridSelector.SelectedItem != null)
            {
                string[] gridFieldInfo = cmbGridSelector.SelectedItem.ToString().Split(':');

                View view = sourceProject.Views[gridFieldInfo[0]];
                GridField gridField = view.Fields.GridFields[gridFieldInfo[1]];

                List<string> nullList = new List<string>();
                foreach (string s in lbxGridColumns.SelectedItems)
                {
                    nullList.Add(s);
                }
                fieldsToNull[view.Name + ":" + gridField.Name] = nullList;                
            }
            else
            {
                ClearFields();
            }
        }

        private void ClearFields()
        {
            lbxGridColumns.DataSource = null;
            lbxGridColumns.Items.Clear();
        }

        private void DeselectFields()
        {
            lbxGridColumns.SelectedItems.Clear();
        }

        private void FillFields()
        {
            if (cmbGridSelector.SelectedIndex >= 0)
            {
                ClearFields();

                Project selectedProject = sourceProject;

                string[] gridFieldInfo = cmbGridSelector.SelectedItem.ToString().Split(':');

                View selectedView = selectedProject.Views[gridFieldInfo[0]];
                GridField gridField = selectedView.Fields.GridFields[gridFieldInfo[1]];

                foreach (GridColumnBase gc in gridField.Columns)
                {
                    if(!(gc.Name.ToLowerInvariant().Equals("recstatus") ||
                        gc.Name.ToLowerInvariant().Equals("fkey") ||
                        gc.Name.ToLowerInvariant().Equals("uniquekey") ||
                        gc.Name.ToLowerInvariant().Equals("globalrecordid")
                        ))
                    lbxGridColumns.Items.Add(gc.Name);
                }

                List<string> selectedFields = new List<string>();

                if (lbxGridColumns.Items.Count > 0 && cmbGridSelector.SelectedIndex > -1)
                {
                    foreach (KeyValuePair<string, List<string>> kvp in fieldsToNull)
                    {
                        if (kvp.Key.Equals(cmbGridSelector.SelectedItem.ToString()))
                        {
                            foreach (string s in kvp.Value)
                            {
                                selectedFields.Add(s);
                            }
                        }
                    }
                }

                foreach (string s in selectedFields)
                {
                    lbxGridColumns.SelectedItems.Add(s);
                }
            }
        }
    }
}
