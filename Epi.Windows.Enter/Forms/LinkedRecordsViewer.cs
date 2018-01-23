using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Data;

namespace Epi.Windows.Enter
{
    public partial class LinkedRecordsViewer : Epi.Windows.Docking.DockWindow
    {

        #region Private Variables

        private View currentView;
        private EnterMainForm enterMainForm;
        private IDbDriver db;
        private string fields;
        private Epi.EnterCheckCodeEngine.RunTimeView runTimeView;
        private SNAViewer sna;

        #endregion

        #region Constructors

        public LinkedRecordsViewer()
        {
            InitializeComponent();
        }

        public LinkedRecordsViewer(EnterMainForm mainForm)
        {
            InitializeComponent();
            this.enterMainForm = mainForm;
            mainForm.OpenViewEvent += new Epi.Windows.Enter.PresentationLogic.GuiMediator.OpenViewEventHandler(mainForm_OpenViewEvent);
            mainForm.CloseViewEvent += new Epi.Windows.Enter.PresentationLogic.GuiMediator.CloseViewEventHandler(mainForm_CloseViewEvent);
            lvLinkedFrom.DoubleClick += new EventHandler(lvLinkedFrom_DoubleClick);
            lvLinkedTo.DoubleClick += new EventHandler(lvLinkedTo_DoubleClick);
            lvLinkedFrom.SelectedIndexChanged += new EventHandler(lvLinkedFrom_SelectedIndexChanged);
            lvLinkedTo.SelectedIndexChanged += new EventHandler(lvLinkedTo_SelectedIndexChanged);
            lvLinkedTo.ItemMouseHover += new ListViewItemMouseHoverEventHandler(lvLinkedTo_ItemMouseHover);
            lvLinkedFrom.ItemMouseHover += new ListViewItemMouseHoverEventHandler(lvLinkedFrom_ItemMouseHover);
            lvLinkedTo.MouseLeave += new EventHandler(lvLinkedTo_MouseLeave);
            lvLinkedFrom.MouseLeave += new EventHandler(lvLinkedFrom_MouseLeave);
            //lvLinkedTo.LostFocus += LvLinkedTo_LostFocus;
            //lvLinkedFrom.LostFocus += LvLinkedFrom_LostFocus;
        }

        //private void LvLinkedFrom_LostFocus(object sender, EventArgs e)
        //{
        //    //lvLinkedFrom.SelectedItems.Clear();
        //}

        //private void LvLinkedTo_LostFocus(object sender, EventArgs e)
        //{
        //    //lvLinkedTo.SelectedItems.Clear();
        //}

        void lvLinkedFrom_MouseLeave(object sender, EventArgs e)
        {
            this.toolTip1.Hide(enterMainForm);
        }
        
        void lvLinkedTo_MouseLeave(object sender, EventArgs e)
        {
            this.toolTip1.Hide(enterMainForm);
        }

        void lvLinkedFrom_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            try
            {
                Point relativePosition = enterMainForm.PointToClient(MousePosition);
                Query query = db.CreateQuery(@"Select * " + enterMainForm.View.Project.GetViewById(int.Parse(e.Item.SubItems[3].Text)).FromViewSQL + " where t.GlobalRecordId = @GlobalRecordId");
                query.Parameters.Add(new QueryParameter("@GlobalRecordId", DbType.StringFixedLength, e.Item.SubItems[1].Text));
                DataTable data = db.Select(query);
                StringBuilder sb = new StringBuilder();
                int counter = 0;
                foreach (DataRow row in data.Rows)
                {
                    foreach (DataColumn col in data.Columns)
                    {
                        if (!col.ColumnName.ToLowerInvariant().Contains("uniquekey") && !col.ColumnName.ToLowerInvariant().Contains("recstatus") && !col.ColumnName.ToLowerInvariant().Contains("globalrecordid"))
                        {
                            counter++;
                            sb.Append(col.ColumnName + " = " + row[col].ToString() + Environment.NewLine);
                            if (counter >= 5)
                                break;
                        }
                    }
                }
                this.toolTip1.ToolTipTitle = "Record preview:";
                this.toolTip1.Show(sb.ToString().Substring(0, sb.Length - 2), enterMainForm, relativePosition.X + 5, relativePosition.Y - (15 + (counter * 13)));
            }
            catch (Exception ex)
            {
                //
            }
        }

        void lvLinkedTo_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            try
            {
                Point relativePosition = enterMainForm.PointToClient(MousePosition);
                Query query = db.CreateQuery(@"Select * " + enterMainForm.View.Project.GetViewById(int.Parse(e.Item.SubItems[4].Text)).FromViewSQL + " where t.GlobalRecordId = @GlobalRecordId");
                query.Parameters.Add(new QueryParameter("@GlobalRecordId", DbType.StringFixedLength, e.Item.SubItems[2].Text));
                DataTable data = db.Select(query);
                StringBuilder sb = new StringBuilder();
                int counter = 0;
                foreach (DataRow row in data.Rows)
                {
                    foreach (DataColumn col in data.Columns)
                    {
                        if (!col.ColumnName.ToLowerInvariant().Contains("uniquekey") && !col.ColumnName.ToLowerInvariant().Contains("recstatus") && !col.ColumnName.ToLowerInvariant().Contains("globalrecordid"))
                        {
                            counter++;
                            sb.Append(col.ColumnName + " = " + row[col].ToString() + Environment.NewLine);
                            if (counter >= 5)
                                break;
                        }
                    }
                }
                this.toolTip1.ToolTipTitle = SharedStrings.LINKED_RECS_PREVIEW;
                this.toolTip1.Show(sb.ToString().Substring(0, sb.Length - 2), enterMainForm, relativePosition.X + 5, relativePosition.Y - (15 + (counter * 13)));
            }
            catch (Exception ex)
            {
                //
            }
        }

        #endregion

        #region Event Handlers

        private void lvLinkedTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnUnlinkTo.Enabled = false;
            if (lvLinkedTo.SelectedItems != null)
            {
                if (lvLinkedTo.SelectedItems.Count > 0)
                {
                    btnUnlinkTo.Enabled = true;
                }
            }
        }

        private void lvLinkedFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnUnlinkFrom.Enabled = false;
            if (lvLinkedFrom.SelectedItems != null)
            {
                if (lvLinkedFrom.SelectedItems.Count > 0)
                {
                    btnUnlinkFrom.Enabled = true;
                }
            }
        }

        private void lvLinkedTo_DoubleClick(object sender, EventArgs e)
        {
            if (lvLinkedTo.SelectedItems != null)
            {
                if (lvLinkedTo.SelectedItems.Count > 0)
                {
                    if (lvLinkedTo.SelectedItems[0].Group.Name.Equals(enterMainForm.View.Id.ToString()))
                    {
                        enterMainForm.LoadRecord(int.Parse(lvLinkedTo.SelectedItems[0].Text));
                    }
                    else
                    {
                        View view = enterMainForm.View.Project.GetViewById(int.Parse(lvLinkedTo.SelectedItems[0].Group.Name));
                        int recordId = int.Parse(lvLinkedTo.SelectedItems[0].Text);
                        enterMainForm.InvokeOpenView(view);
                        enterMainForm.LoadRecord(recordId);
                    }
                }
            }
        }

        private void lvLinkedFrom_DoubleClick(object sender, EventArgs e)
        {
            if (lvLinkedFrom.SelectedItems != null)
            {
                if (lvLinkedFrom.SelectedItems.Count > 0)
                {
                    if (lvLinkedFrom.SelectedItems[0].Group.Name.Equals(enterMainForm.View.Id.ToString()))
                    {
                        enterMainForm.LoadRecord(int.Parse(lvLinkedFrom.SelectedItems[0].Text));
                    }
                    else
                    {
                        View view = enterMainForm.View.Project.GetViewById(int.Parse(lvLinkedFrom.SelectedItems[0].Group.Name));
                        int recordId = int.Parse(lvLinkedFrom.SelectedItems[0].Text);
                        enterMainForm.InvokeOpenView(view);
                        enterMainForm.LoadRecord(recordId);
                    }
                }
            }
        }

        private void mainForm_CloseViewEvent(object sender, EventArgs e)
        {
            lvLinkedFrom.Clear();
            lvLinkedTo.Clear();
            ToggleEnable(false);
        }

        private void mainForm_OpenViewEvent(object sender, Epi.Windows.Enter.PresentationLogic.OpenViewEventArgs e)
        {
            if (e.View != null)
            {
                if (!e.View.IsRelatedView)
                {
                    this.currentView = e.View;
                    this.fields = null;
                    db = DBReadExecute.GetDataDriver(e.View.Project.FilePath);
                    CreateFromGrid();
                    CreateToGrid();
                    ToggleEnable(true);
                }
                else
                {
                    lvLinkedFrom.Clear();
                    lvLinkedTo.Clear();
                    ToggleEnable(false);
                }
            }
        }

        private void btnViewSNA_Click(object sender, EventArgs e)
        {
            if (lvLinkedTo.Items.Count > 0 || lvLinkedFrom.Items.Count > 0)
            {
                sna = new SNAViewer(enterMainForm.View, db);
                sna.RecordSelected += new EpiDashboard.SphereSelectedHandler(sna_RecordSelected);
                sna.Show();
            }
            else
            {
                MessageBox.Show(SharedStrings.LINKED_RECS_NONE_LINKED, SharedStrings.LINKED_RECS_EMPTY, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void sna_RecordSelected(int viewId, int recordId)
        {
            if (viewId == enterMainForm.View.Id)
            {
                enterMainForm.LoadRecord(recordId);
            }
            else
            {
                View view = enterMainForm.View.Project.GetViewById(viewId);
                enterMainForm.InvokeOpenView(view);
                enterMainForm.LoadRecord(recordId);
            }
        }

        private void btnNewLinkFrom_Click(object sender, EventArgs e)
        {
            if (enterMainForm.View.CurrentRecordId > 0)
            {
                View selectedView;
                Epi.Windows.Enter.Dialogs.FindRecords findrecords;
                if (enterMainForm.View.Project.Views.Count > 1)
                {
                    Epi.Enter.Dialogs.WithinProjectViewSelectionDialog viewSelection = new Epi.Enter.Dialogs.WithinProjectViewSelectionDialog(enterMainForm, enterMainForm.View.Project);
                    if (viewSelection.ShowDialog() == DialogResult.OK)
                    {
                        selectedView = currentView.Project.GetViewById(viewSelection.ViewId);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    selectedView = currentView;
                }

                findrecords = new Epi.Windows.Enter.Dialogs.FindRecords(selectedView, enterMainForm);
                DialogResult result = findrecords.ShowDialog();
                this.BringToFront();

                if (result == DialogResult.OK)
                {
                    Query query = db.CreateQuery("Insert into metaLinks (FromRecordGuid, ToRecordGuid, FromViewId, ToViewId) values (@FromRecordGuid, @ToRecordGuid, @FromViewId, @ToViewId)");
                    query.Parameters.Add(new QueryParameter("@FromRecordGuid", DbType.StringFixedLength, findrecords.GlobalRecordId.ToString("D").ToLowerInvariant()));
                    query.Parameters.Add(new QueryParameter("@ToRecordGuid", DbType.StringFixedLength, enterMainForm.View.CurrentGlobalRecordId.ToLowerInvariant()));
                    query.Parameters.Add(new QueryParameter("@FromViewId", DbType.Int32, selectedView.Id));
                    query.Parameters.Add(new QueryParameter("@ToViewId", DbType.Int32, enterMainForm.View.Id));
                    db.ExecuteNonQuery(query);
                    FillFromNodes();
                }
            }
            else
            {
                MessageBox.Show(SharedStrings.LINKED_RECS_FIRST_SAVE, SharedStrings.LINKED_RECS_CANT_CREATE, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnNewLinkTo_Click(object sender, EventArgs e)
        {
            if (enterMainForm.View.CurrentRecordId > 0)
            {
                View selectedView;
                Epi.Windows.Enter.Dialogs.FindRecords findrecords;
                if (enterMainForm.View.Project.Views.Count > 1)
                {
                    Epi.Enter.Dialogs.WithinProjectViewSelectionDialog viewSelection = new Epi.Enter.Dialogs.WithinProjectViewSelectionDialog(enterMainForm, enterMainForm.View.Project);
                    if (viewSelection.ShowDialog() == DialogResult.OK)
                    {
                        selectedView = currentView.Project.GetViewById(viewSelection.ViewId);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    selectedView = currentView;
                }

                findrecords = new Epi.Windows.Enter.Dialogs.FindRecords(selectedView, enterMainForm);
                DialogResult result = findrecords.ShowDialog();
                this.BringToFront();

                if (result == DialogResult.OK)
                {
                    Query query = db.CreateQuery("Insert into metaLinks (FromRecordGuid, ToRecordGuid, FromViewId, ToViewId) values (@FromRecordGuid, @ToRecordGuid, @FromViewId, @ToViewId)");
                    query.Parameters.Add(new QueryParameter("@FromRecordGuid", DbType.StringFixedLength, enterMainForm.View.CurrentGlobalRecordId.ToLowerInvariant()));
                    query.Parameters.Add(new QueryParameter("@ToRecordGuid", DbType.StringFixedLength, findrecords.GlobalRecordId.ToString("D").ToLowerInvariant()));
                    query.Parameters.Add(new QueryParameter("@FromViewId", DbType.Int32, enterMainForm.View.Id));
                    query.Parameters.Add(new QueryParameter("@ToViewId", DbType.Int32, selectedView.Id));
                    db.ExecuteNonQuery(query);
                    FillToNodes();
                }
            }
            else
            {
                MessageBox.Show(SharedStrings.LINKED_RECS_FIRST_SAVE, SharedStrings.LINKED_RECS_CANT_CREATE, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnUnlinkTo_Click(object sender, EventArgs e)
        {
            if (lvLinkedTo.SelectedItems != null)
            {
                string recordNumber = ((ListViewItem)lvLinkedTo.SelectedItems[0]).Text;
                DialogResult result = MsgBox.ShowQuestion(SharedStrings.UNLINK_PRE + " " + recordNumber + Environment.NewLine + SharedStrings.UNLINK_POST, MessageBoxButtons.OKCancel);

                if (lvLinkedTo.SelectedItems.Count > 0 && result == DialogResult.OK)
                {
                    Query query = db.CreateQuery("Delete from metaLinks Where FromRecordGuid = @FromRecordGuid and ToRecordGuid = @ToRecordGuid");
                    query.Parameters.Add(new QueryParameter("@FromRecordGuid", DbType.StringFixedLength, enterMainForm.View.CurrentGlobalRecordId));
                    query.Parameters.Add(new QueryParameter("@ToRecordGuid", DbType.StringFixedLength, lvLinkedTo.SelectedItems[0].SubItems[2].Text));
                    db.ExecuteNonQuery(query);
                    FillToNodes();
                }
            }
        }

        private void btnUnlinkFrom_Click(object sender, EventArgs e)
        {
            if (lvLinkedFrom.SelectedItems != null)
            {
                string recordNumber = ((ListViewItem)lvLinkedFrom.SelectedItems[0]).Text;
                DialogResult result = MsgBox.ShowQuestion(SharedStrings.UNLINK_PRE + " " + recordNumber + Environment.NewLine + SharedStrings.UNLINK_POST, MessageBoxButtons.OKCancel);

                if (lvLinkedFrom.SelectedItems.Count > 0 && result == DialogResult.OK)
                {
                    Query query = db.CreateQuery("Delete from metaLinks Where ToRecordGuid = @ToRecordGuid and FromRecordGuid = @FromRecordGuid");
                    query.Parameters.Add(new QueryParameter("@ToRecordGuid", DbType.StringFixedLength, enterMainForm.View.CurrentGlobalRecordId));
                    query.Parameters.Add(new QueryParameter("@FromRecordGuid", DbType.StringFixedLength, lvLinkedFrom.SelectedItems[0].SubItems[1].Text));
                    db.ExecuteNonQuery(query);
                    FillFromNodes();
                }
            }
        }

        #endregion

        #region Private Methods

        private void CreateToGrid()
        {
            lvLinkedTo.Clear();
            lvLinkedTo.Columns.Add("UniqueKey", 150);
            lvLinkedTo.Columns.Add("FromRecordGuid", 150);
            lvLinkedTo.Columns[0].Width = 0;
        }

        private void CreateFromGrid()
        {
            lvLinkedFrom.Clear();
            lvLinkedFrom.Columns.Add("UniqueKey", 150);
            lvLinkedFrom.Columns.Add("ToRecordGuid", 150);
            lvLinkedFrom.Columns[0].Width = 0;
        }


        private void ToggleEnable(bool isEnabled)
        {
            btnViewSNA.Enabled = isEnabled;
            btnNewLinkFrom.Enabled = isEnabled;
            btnNewLinkTo.Enabled = isEnabled;
        }

        private void FillToNodes()
        {
            BackgroundWorker toNodeFiller = new BackgroundWorker();
            toNodeFiller.DoWork += new DoWorkEventHandler(ToNodeFiller_DoWork);
            toNodeFiller.RunWorkerAsync();
        }

        void ToNodeFiller_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (lvLinkedTo.InvokeRequired)
                {
                    lvLinkedTo.Invoke(new MethodInvoker(delegate
                    {
                        lvLinkedTo.Items.Clear();
                        lvLinkedTo.Groups.Clear();
                    }));
                }
                else
                {
                    lvLinkedTo.Items.Clear();
                    lvLinkedTo.Groups.Clear();
                }
                if (btnUnlinkTo.InvokeRequired)
                {
                    btnUnlinkTo.Invoke(new MethodInvoker(delegate
                    {
                        btnUnlinkTo.Enabled = false;
                    }));
                }
                else
                {
                    btnUnlinkTo.Enabled = false;
                }

                string uniqueKeys = "";
                string parens = "";
                string joins = "";
                foreach (View view in enterMainForm.View.Project.Views)
                {
                    // IF statement added by E. Knudsen 4/5/2011 to prevent errors in Enter if another view (form) in the
                    // same project didn't have a data table.
                    //if (!string.IsNullOrEmpty(view.TableName) && db.TableExists(view.TableName))
                    //{
                    if (!view.IsRelatedView)
                    {
                        uniqueKeys += "t" + view.Id + ".UniqueKey as Key" + view.Id + ", ";
                        parens += "(";
                        joins += "left outer join " + view.TableName + " t" + view.Id + " on m.ToRecordGuid = t" + view.Id + ".GlobalRecordId) ";
                    }
                    //}
                }
                uniqueKeys = uniqueKeys.Substring(0, uniqueKeys.Length - 2) + " ";

                Query query = db.CreateQuery(@"Select FromRecordGuid, ToRecordGuid, FromViewId, ToViewId, " + uniqueKeys + " from " + parens + "metaLinks m " + joins + " where m.FromRecordGuid = @GlobalRecordId");
                QueryParameter parameter = new QueryParameter("@GlobalRecordId", DbType.StringFixedLength, enterMainForm.View.CurrentGlobalRecordId);
                parameter.Size = enterMainForm.View.CurrentGlobalRecordId.Length;
                query.Parameters.Add(parameter);
                DataTable data = db.Select(query);
                
                if (data.Rows.Count > 0)
                {
                    foreach (View view in enterMainForm.View.Project.Views)
                    {
                        if (lvLinkedTo.InvokeRequired)
                        {
                            lvLinkedTo.Invoke(new MethodInvoker(delegate
                            {
                                lvLinkedTo.Groups.Add(view.Id.ToString(), view.Name);
                            }));
                        }
                        else
                        {
                            lvLinkedTo.Groups.Add(view.Id.ToString(), view.Name);
                        }
                    }
                }

                List<string> names = new List<string>();

                foreach (DataRow row in data.Rows)
                {
                    ListViewItem item = new ListViewItem(row["Key" + row["ToViewId"].ToString()].ToString());
                    for (int x = 0; x < data.Columns.Count; x++)
                    {
                        item.SubItems.Add(row[x].ToString());
                    }
                    item.ImageIndex = 0;
                    item.Group = lvLinkedTo.Groups[row["ToViewId"].ToString()];

                    if (names.Contains(item.Text) == false)
                    {
                        names.Add(item.Text);
                        
                        if (lvLinkedTo.InvokeRequired)
                        {
                            lvLinkedTo.Invoke(new MethodInvoker(delegate
                            {
                                lvLinkedTo.Items.Add(item);
                            }));
                        }
                        else
                        {
                            lvLinkedTo.Items.Add(item);
                        }
                    }
                }
            }
            catch { }
        }

        private void FillFromNodes()
        {
            BackgroundWorker fromNodeFiller = new BackgroundWorker();
            fromNodeFiller.DoWork += new DoWorkEventHandler(FromNodeFiller_DoWork);
            fromNodeFiller.RunWorkerAsync();
        }

        void FromNodeFiller_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (lvLinkedFrom.InvokeRequired)
                {
                    lvLinkedFrom.Invoke(new MethodInvoker(delegate
                    {
                        lvLinkedFrom.Items.Clear();
                        lvLinkedFrom.Groups.Clear();
                    }));
                }
                else
                {
                    lvLinkedFrom.Items.Clear();
                    lvLinkedFrom.Groups.Clear();
                }

                if (btnUnlinkFrom.InvokeRequired)
                {
                    btnUnlinkFrom.Invoke(new MethodInvoker(delegate
                        {
                            btnUnlinkFrom.Enabled = false;
                        }));
                }
                else
                {
                    btnUnlinkFrom.Enabled = false;
                }

                string uniqueKeys = "";
                string parens = "";
                string joins = "";
                foreach (View view in enterMainForm.View.Project.Views)
                {
                    // IF statement added by E. Knudsen 4/5/2011 to prevent errors in Enter if another view (form) in the
                    // same project didn't have a data table.
                    //if (!string.IsNullOrEmpty(view.TableName) && db.TableExists(view.TableName)) 
                    //{
                    if (!view.IsRelatedView)
                    {
                        uniqueKeys += "t" + view.Id + ".UniqueKey as Key" + view.Id + ", ";
                        parens += "(";
                        joins += "left outer join " + view.TableName + " t" + view.Id + " on m.FromRecordGuid = t" + view.Id + ".GlobalRecordId) ";
                    }
                    //}
                }
                uniqueKeys = uniqueKeys.Substring(0, uniqueKeys.Length - 2) + " ";

                Query query = db.CreateQuery(@"Select FromRecordGuid, ToRecordGuid, FromViewId, ToViewId, " + uniqueKeys + " from " + parens + "metaLinks m " + joins + " where m.ToRecordGuid = @GlobalRecordId");
                QueryParameter parameter = new QueryParameter("@GlobalRecordId", DbType.StringFixedLength, enterMainForm.View.CurrentGlobalRecordId);
                parameter.Size = enterMainForm.View.CurrentGlobalRecordId.Length;
                query.Parameters.Add(parameter);

                DataTable data = db.Select(query);
                if (data.Rows.Count > 0)
                {
                    foreach (View view in enterMainForm.View.Project.Views)
                    {
                        if (lvLinkedFrom.InvokeRequired)
                        {
                            lvLinkedFrom.Invoke(new MethodInvoker(delegate
                            {
                                lvLinkedFrom.Groups.Add(view.Id.ToString(), view.Name);
                            }));
                        }
                        else
                        {
                            lvLinkedFrom.Groups.Add(view.Id.ToString(), view.Name);
                        }
                    }
                }

                List<string> names = new List<string>();

                foreach (DataRow row in data.Rows)
                {
                    ListViewItem item = new ListViewItem(row["Key" + row["FromViewId"].ToString()].ToString());
                    for (int x = 0; x < data.Columns.Count; x++)
                    {
                        item.SubItems.Add(row[x].ToString());
                    }
                    item.ImageIndex = 0;
                    item.Group = lvLinkedFrom.Groups[row["FromViewId"].ToString()];

                    if(names.Contains(item.Text) == false)
                    {
                        names.Add(item.Text);

                        if (lvLinkedFrom.InvokeRequired)
                        {
                            lvLinkedFrom.Invoke(new MethodInvoker(delegate
                            {
                                lvLinkedFrom.Items.Add(item);
                            }));
                        }
                        else
                        {
                            lvLinkedFrom.Items.Add(item);
                        }
                    }
                }
            }
            catch { }
        }

        #endregion

        #region Public Methods

        public void Render(Epi.EnterCheckCodeEngine.RunTimeView runTimeView)
        {
            try
            {
                if (!runTimeView.View.IsRelatedView)
                {
                    this.runTimeView = runTimeView;
                    FillFromNodes();
                    FillToNodes();
                    ToggleEnable(true);
                }
                else
                {
                    lvLinkedFrom.Clear();
                    lvLinkedTo.Clear();
                    ToggleEnable(false);
                }
            }
            catch { }
        }

        #endregion
        
    }
}