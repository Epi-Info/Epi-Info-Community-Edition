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
using Epi.Windows;

namespace Epi.Windows.ImportExport.Dialogs
{
    public partial class PackageColumnRemovalDialog : Form
    {
        #region Private Members
        private Dictionary<string, List<string>> fieldsToNull;
        private Project sourceProject;
        private string formName;
        #endregion // Private Members

        public PackageColumnRemovalDialog()
        {
            InitializeComponent();
        }

        public PackageColumnRemovalDialog(Project sourceProject, string formName)
        {
            InitializeComponent();
            this.sourceProject = sourceProject;
            this.formName = formName;

            FillForms();

            fieldsToNull = new Dictionary<string, List<string>>();
            DeselectFields();

            if (fieldsToNull.Count == 0)
            {
                foreach (View view in sourceProject.Views)
                {
                    fieldsToNull.Add(view.Name, new List<string>());
                }
            }
        }

        public PackageColumnRemovalDialog(Project sourceProject, string formName, Dictionary<string, List<string>> fieldsToNull)
        {
            InitializeComponent();
            this.sourceProject = sourceProject;
            this.formName = formName;

            if (fieldsToNull != null)
            {
                this.fieldsToNull = fieldsToNull;
            }
            else
            {
                this.fieldsToNull = new Dictionary<string, List<string>>();
            }

            DeselectFields();

            if (fieldsToNull.Count == 0)
            {
                foreach (View view in sourceProject.Views)
                {
                    fieldsToNull.Add(view.Name, new List<string>());
                }
            }

            FillForms();
        }

        public Dictionary<string, List<string>> FieldsToNull 
        {
            get
            {
                return this.fieldsToNull;
            }
        }

        private void FillForms()
        {

            string selectedForm = formName;

            if (sourceProject.Views.Contains(selectedForm))
            {
                cmbFormSelector.Items.Clear();

                View parentView = sourceProject.Views[selectedForm];
                cmbFormSelector.Items.Add(parentView.Name);

                foreach (View view in sourceProject.Views)
                {
                    if (Epi.ImportExport.ImportExportHelper.IsFormDescendant(view, parentView))
                    {
                        if (!cmbFormSelector.Items.Contains(view.Name))
                        {
                            cmbFormSelector.Items.Add(view.Name);
                        }
                    }
                }

                cmbFormSelector.SelectedIndex = 0;
            }

            if (cmbFormSelector.Items.Count == 1)
            {
                cmbFormSelector.Enabled = false;
            }
        }

        private void cmbFormSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillFields();
        }

        private void lbxFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbFormSelector.SelectedItem != null)
            {
                if (sourceProject.Views.Contains(cmbFormSelector.SelectedItem.ToString()))
                {
                    View currentView = sourceProject.Views[cmbFormSelector.SelectedItem.ToString()];
                    List<string> nullList = new List<string>();
                    foreach (string s in lbxFields.SelectedItems)
                    {
                        nullList.Add(s);
                    }
                    fieldsToNull[currentView.Name] = nullList;
                }
                else
                {
                    MsgBox.ShowError("Not a valid form.");
                    this.Close();
                }
            }
            else
            {
                ClearFields();
            }
        }

        private void ClearFields()
        {
            lbxFields.DataSource = null;
            lbxFields.Items.Clear();
        }

        private void DeselectFields()
        {
            lbxFields.SelectedItems.Clear();
        }

        private void FillFields()
        {
            if (cmbFormSelector.SelectedIndex >= 0)
            {
                ClearFields();

                Project selectedProject = sourceProject;
                View selectedView = selectedProject.Views[cmbFormSelector.SelectedItem.ToString()];

                List<string> fieldList = new List<string>();

                foreach (Field field in selectedView.Fields)
                {
                    if (field is IDataField &&
                        !(
                        field is RecStatusField ||
                        field is UniqueKeyField ||
                        field is GlobalRecordIdField ||
                        field is ForeignKeyField ||
                        field is CommandButtonField ||
                        field is GroupField ||
                        field is GridField
                        ))
                    {
                        fieldList.Add(field.Name);                        
                    }
                }

                fieldList.Sort();

                foreach (string s in fieldList) // TODO: Use item source instead
                {
                    lbxFields.Items.Add(s);
                }

                List<string> selectedFields = new List<string>();

                if (lbxFields.Items.Count > 0 && cmbFormSelector.SelectedIndex > -1)
                {
                    foreach (KeyValuePair<string, List<string>> kvp in fieldsToNull)
                    {
                        if (kvp.Key.Equals(cmbFormSelector.SelectedItem.ToString()))
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
                    lbxFields.SelectedItems.Add(s);
                }
            }
        }
    }
}
