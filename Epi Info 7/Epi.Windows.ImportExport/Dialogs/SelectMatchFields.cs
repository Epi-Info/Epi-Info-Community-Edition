using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Epi.Data;
using Epi.Fields;

namespace Epi.Windows.ImportExport.Dialogs
{
    public partial class SelectMatchFields : Form
    {
        public List<Field> KeyFields { get; private set; }
        private View Form { get; set; }
        private Project Project { get; set; }

        public SelectMatchFields()
        {
            InitializeComponent();
        }

        public SelectMatchFields(Project sourceProject, View form, List<Field> selectedFields = null)
        {
            InitializeComponent();

            this.Project = sourceProject;
            this.Form = form;
            this.KeyFields = new List<Field>();

            foreach (Field field in Form.Fields)
            {
                if (field is RenderableField && field is IDataField)
                {
                    lbxFields.Items.Add(field.Name);
                }
            }

            if (selectedFields != null)
            {
                foreach (Field field in selectedFields)
                {
                    lbxFields.SelectedItems.Add(field.Name);
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            KeyFields = new List<Field>();

            if (lbxFields.SelectedItems.Count == 0)
            {
                return;
            }

            try
            {
                #region Check #1 - Make sure key is unique on parent form
                IDbDriver db = Project.CollectedData.GetDatabase();

                Query selectQuery = db.CreateQuery("SELECT Count(*) FROM [" + Form.TableName + "]");
                int recordCount = (int)db.ExecuteScalar(selectQuery);

                WordBuilder wb = new WordBuilder(",");

                foreach (Field field in Form.Fields)
                {
                    if (field is RenderableField && lbxFields.SelectedItems.Contains(field.Name))
                    {
                        wb.Add(field.Name);
                    }
                }

                selectQuery = db.CreateQuery("SELECT DISTINCT " + wb.ToString() + " " + Form.FromViewSQL);
                int distinctCount = db.Select(selectQuery).Rows.Count; // probably better way to do this, but unsure if can be made generic... this query is most generic across DB types?

                if (distinctCount == recordCount)
                {
                    foreach (Field field in Form.Fields)
                    {
                        if (field is RenderableField && lbxFields.SelectedItems.Contains(field.Name))
                        {
                            KeyFields.Add(field);
                        }
                    }
                }
                else
                {
                    if (lbxFields.SelectedItems.Count == 1)
                    {
                        Epi.Windows.MsgBox.ShowError(String.Format("The selected match key ({0}) is not unique.", lbxFields.SelectedItem.ToString()));
                    }
                    else if (lbxFields.SelectedItems.Count > 1)
                    {
                        WordBuilder keyFields = new WordBuilder(",");
                        foreach (string s in lbxFields.SelectedItems)
                        {
                            keyFields.Add(s);
                        }
                        Epi.Windows.MsgBox.ShowError(String.Format("The selected match key ({0}) is not unique.", keyFields.ToString()));
                    }
                    
                    this.DialogResult = System.Windows.Forms.DialogResult.None;
                    return;
                }
                #endregion // Check #1 - Make sure key is unique on parent form

                // Currently, disable match keys if related forms exist. TODO: Change this later?
                foreach (View otherForm in Project.Views)
                {
                    if (otherForm != Form && Epi.ImportExport.ImportExportHelper.IsFormDescendant(otherForm, Form))
                    {
                        Epi.Windows.MsgBox.ShowError("Custom match keys cannot be used to package a form that contains child forms.");
                        this.DialogResult = System.Windows.Forms.DialogResult.None;
                        return;
                    }
                }
                //#region Check #2 - Make sure key exists in other forms in the hierarchy and that it's the same field type
                //foreach (View otherForm in Project.Views)
                //{
                //    if (otherForm != Form && Epi.ImportExport.ImportExportHelper.IsFormDescendant(otherForm, Form))
                //    {
                //        foreach (Field field in KeyFields)
                //        {
                //            if (!otherForm.Fields.Contains(field.Name))
                //            {
                //                Epi.Windows.MsgBox.ShowError(
                //                    String.Format(
                //                    "The selected field {0} does not exist in the child form {1}. The keys selected in this dialog must exist across all child forms.",
                //                    field.Name, otherForm.Name));
                //                this.DialogResult = System.Windows.Forms.DialogResult.None;
                //                return;
                //            }
                //            else
                //            {
                //                Field otherField = otherForm.Fields[field.Name];
                //                if (otherField.FieldType != field.FieldType)
                //                {
                //                    Epi.Windows.MsgBox.ShowError(
                //                    String.Format(
                //                    "The selected field {0} is implemented as a different field type on child form {1}. The keys selected in this dialog must exist across all child forms and those fields must be of the same field type.",
                //                    field.Name, otherForm.Name));
                //                    this.DialogResult = System.Windows.Forms.DialogResult.None;
                //                    return;
                //                }
                //            }
                //        }
                //    }
                //}
                //#endregion // Check #2 - Make sure key exists in other forms in the hierarchy and that it's the same field type
            }
            catch (Exception ex)
            {
                Epi.Windows.MsgBox.ShowException(ex);
                this.DialogResult = System.Windows.Forms.DialogResult.None;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lbxFields.SelectedItems.Clear();
        }
    }
}
