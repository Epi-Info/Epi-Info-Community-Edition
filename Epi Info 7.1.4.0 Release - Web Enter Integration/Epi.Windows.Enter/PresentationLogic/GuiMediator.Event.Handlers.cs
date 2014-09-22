using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Epi.Fields;
using Epi.EnterCheckCodeEngine;
using Epi.Windows.Controls;

namespace Epi.Windows.Enter.PresentationLogic
{
    public partial class GuiMediator
    {
        public delegate void SaveRecordEventHander(object sender, EventArgs e);

        public delegate void OpenViewEventHandler(object sender, OpenViewEventArgs e);
        public delegate void CloseViewEventHandler(object sender, EventArgs e);

        public delegate void OpenPageEventHandler(object sender, PageSelectedEventArgs e);
        public delegate void ClosePageEventHandler(object sender, EventArgs e);

        public delegate void GotoRecordEventHandler(object sender, GoToRecordEventArgs e);
        public delegate void CloseRecordEventHandler(object sender, EventArgs e);

        public delegate void OpenFieldEventHandler(object sender, GoToFieldEventArgs e);
        public delegate void CloseFieldEventHandler(object sender, CloseFieldEventArg e);
        public delegate void ClickFieldEventHandler(object sender, ClickFieldEventArg e);
        public delegate void FieldChangeEventHandler(object sender, Field field);
        public delegate void DirtyFieldEventHandler(object sender, EventArgs e);

        private bool IsClosingRelatedView = false;


        public void Subscribe(EnterMainForm enterMainForm, ViewExplorer viewExplorer, Canvas canvas, LinkedRecordsViewer linkedRecordsViewer)
        {
            this.mainForm = enterMainForm;
            this.viewExplorer = viewExplorer;
            this.canvas = canvas;
            this.linkedRecordsViewer = linkedRecordsViewer;

            viewExplorer.OpenPageEvent += new OpenPageEventHandler(this.OpenPageHandler);
            viewExplorer.ClosePageEvent += new ClosePageEventHandler(this.ClosePageHandler);
            viewExplorer.GotoRecordEvent += new GotoRecordEventHandler(this.GoToRecordHandler);

            enterMainForm.GotoRecordEvent += new GotoRecordEventHandler(this.GoToRecordHandler);
            enterMainForm.OpenViewEvent += new OpenViewEventHandler(this.OpenViewHandler);
            enterMainForm.CloseViewEvent += new CloseViewEventHandler(this.CloseViewHandler);
            enterMainForm.SaveRecordEvent += new SaveRecordEventHander(this.SaveRecordHandler);
            enterMainForm.MarkAsDeletedRecordEvent += new EventHandler(this.MarkAsDeletedRecordHandler);
            enterMainForm.UnMarkDeletedRecordEvent += new EventHandler(this.UnMarkDeletedRecordHandler);
            enterMainForm.CloseFormEvent += new EventHandler(this.CloseFormEventHandler);

            canvas.GotoFieldEvent += new OpenFieldEventHandler(this.OpenFieldHandler);
            canvas.CloseFieldEvent += new CloseFieldEventHandler(this.CloseFieldHandler);
            canvas.ClickFieldEvent += new ClickFieldEventHandler(this.ClickFieldHandler);
            canvas.DirtyFieldEvent += new DirtyFieldEventHandler(this.DirtyFieldHandler);
            canvas.DataGridRowAddedEvent += new DataGridViewRowEventHandler(this.DataGridRowAddedHandler);

            this.DisplayFormat += new EventHandler(mediator_DisplayFormat);
        }

        public void UnSubscribe()
        {
            viewExplorer.OpenPageEvent -= new OpenPageEventHandler(this.OpenPageHandler);
            viewExplorer.ClosePageEvent -= new ClosePageEventHandler(this.ClosePageHandler);
            viewExplorer.GotoRecordEvent -= new GotoRecordEventHandler(this.GoToRecordHandler);

            this.mainForm.GotoRecordEvent -= new GotoRecordEventHandler(this.GoToRecordHandler);
            this.mainForm.OpenViewEvent -= new OpenViewEventHandler(this.OpenViewHandler);
            this.mainForm.CloseViewEvent -= new CloseViewEventHandler(this.CloseViewHandler);
            this.mainForm.SaveRecordEvent -= new SaveRecordEventHander(this.SaveRecordHandler);
            this.mainForm.MarkAsDeletedRecordEvent -= new EventHandler(this.MarkAsDeletedRecordHandler);
            this.mainForm.UnMarkDeletedRecordEvent -= new EventHandler(this.UnMarkDeletedRecordHandler);
            this.mainForm.CloseFormEvent -= new EventHandler(this.CloseFormEventHandler);

            canvas.GotoFieldEvent -= new OpenFieldEventHandler(this.OpenFieldHandler);
            canvas.CloseFieldEvent -= new CloseFieldEventHandler(this.CloseFieldHandler);
            canvas.ClickFieldEvent -= new ClickFieldEventHandler(this.ClickFieldHandler);
            canvas.DataGridRowAddedEvent -= new DataGridViewRowEventHandler(this.DataGridRowAddedHandler);

            this.DisplayFormat -= new EventHandler(mediator_DisplayFormat);

            this.mainForm = null;
            this.viewExplorer = null;
            this.canvas = null;
            this.linkedRecordsViewer = null;
        }

        public void OpenViewHandler(object sender, Epi.Windows.Enter.PresentationLogic.OpenViewEventArgs e)
        {
            if (e.View != null)
            {
                this.Reset();
                this.canvas.ResetControlFactoryFields();

                if (this.EnterCheckCodeEngine.Project == null || this.EnterCheckCodeEngine.Project.FilePath != e.View.Project.FilePath)
                {
                    this.EnterCheckCodeEngine.Reset();
                    this.EnterCheckCodeEngine.Project = e.View.Project;
                }

                this.EnterCheckCodeEngine.OpenViewHandler(this, new Epi.EnterCheckCodeEngine.OpenViewEventArgs(this, e.View.Id));
                this.view = this.EnterCheckCodeEngine.CurrentView.View;
                this.view.ReturnToParent = e.View.ReturnToParent;

                this.mainForm.OpenView(this.view);
                this.viewExplorer.LoadView(this.EnterCheckCodeEngine.CurrentView);
                this.canvas.CurrentView = this.view;

                if(string.IsNullOrEmpty(e.RecordNumber.Trim()))
                {
                    this.EnterCheckCodeEngine.CheckCodeHandler(this, new RunCheckCodeEventArgs(EventActionEnum.OpenRecord, "<<"));
                }
                else if (e.RecordNumber.Equals("*"))
                {
                    this.EnterCheckCodeEngine.CheckCodeHandler(this, new RunCheckCodeEventArgs(EventActionEnum.NewRecord, "+"));
                }
                else
                {
                    this.EnterCheckCodeEngine.CheckCodeHandler(this, new RunCheckCodeEventArgs(EventActionEnum.OpenRecord, e.RecordNumber));
                }
                this.CurrentRecordId = this.EnterCheckCodeEngine.CurrentView.CurrentRecordNumber;  

                this.OpenPageHandler(sender, new PageSelectedEventArgs(this.view.Pages[0]));
                
                IsDirty = false;
            }
        }

        public void CloseViewHandler(object returnHome, EventArgs e)
        {
            this.canvas.UnsubscribeControlEventHandlers();
            this.SetFieldData();
            if (SaveRecord() == false)
                return;
            RunTimeView RTV = this.EnterCheckCodeEngine.CurrentView;
            this.IsClosingRelatedView = RTV.View.IsRelatedView;
            EpiInfo.Plugin.IScope scope = RTV.EpiInterpreter.Context.Scope.GetEnclosingScope();
            this.EnterCheckCodeEngine.CheckCodeHandler(this, new RunCheckCodeEventArgs(EventActionEnum.CloseView, ""));

            if (returnHome is Boolean && (bool)returnHome)
            {
                while (this.EnterCheckCodeEngine.CurrentView.View.IsRelatedView)
                {
                    this.EnterCheckCodeEngine.CheckCodeHandler(this, new RunCheckCodeEventArgs(EventActionEnum.CloseView, ""));
                }
            }

            if (this.EnterCheckCodeEngine.CurrentView != null)
            {
                this.view = this.EnterCheckCodeEngine.CurrentView.View;
                this.Reset();
                this.canvas.CurrentView = this.view;
                this.mainForm.OpenView(this.view);
                this.viewExplorer.LoadView(this.EnterCheckCodeEngine.CurrentView, this.EnterCheckCodeEngine.CurrentView.CurrentPage);

                // *** populate view data with any changes from child - begin
                if(scope != null && this.view.Name.Equals(scope.Name, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (Epi.Fields.Field field in this.view.Fields)
                    {
                        if (field is EpiInfo.Plugin.IVariable)
                        {
                            EpiInfo.Plugin.IVariable iVariable = scope.Resolve(field.Name, this.view.Name);

                            if (iVariable != null && (field is Epi.Fields.ImageField == false))
                            {
                                if (field is Epi.Fields.CheckBoxField || field is Epi.Fields.YesNoField)
                                {
                                    if (((Epi.Fields.IDataField)field).CurrentRecordValueString == "true" || ((Epi.Fields.IDataField)field).CurrentRecordValueString == "1")
                                    {
                                        //v.Expression = "true";
                                        ((Epi.Fields.IDataField)field).CurrentRecordValueString = "1";
                                    }
                                    else if (((Epi.Fields.IDataField)field).CurrentRecordValueString == "false" || ((Epi.Fields.IDataField)field).CurrentRecordValueString == "0")
                                    {
                                        //v.Expression = "false";
                                        ((Epi.Fields.IDataField)field).CurrentRecordValueString = "0";
                                    }
                                    else
                                    {
                                        ((Epi.Fields.IDataField)field).CurrentRecordValueString = null;
                                    }
                                }
                                else
                                {
                                    string value = iVariable.Expression;

                                    if (value != ((EpiInfo.Plugin.IVariable)field).Expression)
                                    {
                                        ((Epi.Fields.IDataField)field).CurrentRecordValueString = value;
                                    }
                                }
                            }
                        }
                    }
                }
                // *** populate view data with any changes from child - end

                this.Render();
                this.IsClosingRelatedView = false;
            }
            else
            {
                this.view = null;
                this.currentPage = null;
                this.mainForm.View = this.view;
                this.viewExplorer.View = this.EnterCheckCodeEngine.CurrentView;
                this.Reset();
                this.Render();
                this.canvas.CurrentView = this.view;
            }
        }

        public void GoToRecordHandler(object sender, GoToRecordEventArgs e)
        {
            if (this.view != null)
            {
                if (this.IsDirty)
                {
                    if (this.SaveRecord() == false)
                    {
                        return;
                    }
                }

                switch (e.RecordString)
                {
                    case "+": // add new record
                        if (this.View.IsRelatedView && this.View.ReturnToParent)
                        {
                            this.mainForm.CloseView();
                            return;
                        }

                        if (AllowOneRecordOnly) { return; }
                        
                        this.EnterCheckCodeEngine.CheckCodeHandler(this, new RunCheckCodeEventArgs(EventActionEnum.NewRecord, e.RecordString));
                        this.canvas.UnsubscribeControlEventHandlers();
                        this.CurrentRecordId = this.EnterCheckCodeEngine.CurrentView.CurrentRecordNumber;

                        this.canvas.SetNewRecordValues();
                        this.mainForm.UpdateAppSpecificInfo(SharedStrings.NEW_RECORD);
                        this.mainForm.AddNewRecordSettings();
                        this.viewExplorer.GoToFirstPage();
                        this.OpenPageHandler(sender, new PageSelectedEventArgs(this.currentPage));                        
                        this.Render();
                        break;

                    default:                        
                        this.mainForm.UpdateAppSpecificInfo(String.Empty);
                        this.CurrentRecordId = this.EnterCheckCodeEngine.CurrentView.CurrentRecordNumber;
                        
                        RunCheckCodeEventArgs args = new RunCheckCodeEventArgs(EventActionEnum.OpenRecord, e.RecordString);
                        this.EnterCheckCodeEngine.CheckCodeHandler(this, args);

                        this.EnterCheckCodeEngine.SaveRecord();

                        PageSelectedEventArgs pageSelectArgs = new PageSelectedEventArgs(this.currentPage);
                        this.OpenPageHandler(sender, pageSelectArgs);
                        
                        IsDirty = false; 
                        break;
                }

                isNewRecord = EnterCheckCodeEngine.CurrentView.CurrentRecordNumber == 0 ? true : false;
            }
            else
            {
                MsgBox.Show(SharedStrings.SPECIFY_VIEW, SharedStrings.ENTER);
            }
        }

        public void CloseRecordHandler(object sender, EventArgs e)
        {
            this.EnterCheckCodeEngine.CheckCodeHandler(this, new RunCheckCodeEventArgs(EventActionEnum.CloseRecord, ""));
        }

        public void OpenPageHandler(object sender, PageSelectedEventArgs e)
        {   
            canvas.UnsubscribeControlEventHandlers();
            
            if (currentPage != null)
            {
                if (e.Page != currentPage)
                {
                    SetFieldData();
                }
            }
            ControlFactory.Instance.IsPopup = false;  
            LoadPanel(e.Page);
            
            currentPage = e.Page;
            viewExplorer.CurrentPage = currentPage;
            mainForm.ChangeBackgroundData(currentPage);

            canvas.Text = currentPage.Name.ToString();

            canvas.ShowPanel(_fieldPanel);
            SetZeeOrderOfGroups(_fieldPanel);

            SetCodeFieldTargetLabelText();

            canvas.CurrentView = EnterCheckCodeEngine.CurrentView.View;
            mainForm.RunTimeView = EnterCheckCodeEngine.CurrentView;
            
            mainForm.Render();

            viewExplorer.Render();
            linkedRecordsViewer.Render(EnterCheckCodeEngine.CurrentView);

            bool enabled = true;

            if (!string.IsNullOrEmpty(view.RecStatusField.CurrentRecordValueString))
            {   
                enabled = viewExplorer.GetRecStatus(int.Parse(view.RecStatusField.CurrentRecordValueString));
                EnableDisableCurrentRecord(view, enabled);
            }

            this.canvas.Render(enabled);
            if (!IsClosingRelatedView)
            {
                EnterCheckCodeEngine.CheckCodeHandler(this, new RunCheckCodeEventArgs(EventActionEnum.OpenPage, e.Page.Name));
            }
            canvas.SubscribeControlEventHandlers();
            if (!canvas.IsGotoPageField)
            {
                if (canvas.GotoPageControl == null && canvas.GotoPageField == null)
                    SetFocusToFirstControl(currentPage, _fieldPanel);
                else
                    canvas.BeginInvoke((MethodInvoker)delegate { canvas.SetFocusToControl(canvas.GotoPageControl, canvas.GotoPageField); });
            }            
        }

        void SetCodeFieldTargetLabelText()
        {
            foreach (RenderableField field in currentPage.Fields)
            {
                if (field is DDLFieldOfCodes)
                {
                    string selectedText = ((DDLFieldOfCodes)field).CurrentRecordValueString;

                    if (string.IsNullOrEmpty(selectedText) == false)
                    {
                        foreach (KeyValuePair<string, int> kvp in ((DDLFieldOfCodes)field).PairAssociated)
                        {
                            foreach (RenderableField candidate in currentPage.Fields)
                            {
                                if (kvp.Value == candidate.Id && candidate is LabelField)
                                {
                                    string predicate = ((DDLFieldOfCodes)field).FieldName;
                                    String filterExpression = string.Empty;
                                    DataTable codeTable = ((TableBasedDropDownField)field).CodeTable;
                                    filterExpression = string.Format("[{0}] = '{1}'", predicate, selectedText.Replace("'", "''"));
                                    DataRow[] dataRows = codeTable.Select(filterExpression);
                                    DataTable filteredTable = dataRows.CopyToDataTable();
                                    
                                    String valueString = filteredTable.Rows[0][kvp.Key].ToString();
                                    candidate.Control.Text = valueString;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ClosePageHandler(object sender, EventArgs e)
        {            
            this.EnterCheckCodeEngine.CheckCodeHandler(sender, new RunCheckCodeEventArgs(EventActionEnum.ClosePage, ""));
        }

        public void OpenFieldHandler(object sender, GoToFieldEventArgs e)
        {
            StringBuilder display = new StringBuilder();

            ControlFactory factory = ControlFactory.Instance;
            Field field = factory.GetAssociatedField((Control)sender);

            if (!string.IsNullOrEmpty(field.Name))
            { 
                display.Append(string.Format("[ {0}:{1} ] ", SharedStrings.NAME , field.Name));
            }

            if ((Control)sender is MaskedTextBox)
            {
                if (DisplayFormat != null)
                {
                    string pattern = ((IPatternable)field).Pattern;
                    if (!string.IsNullOrEmpty(field.Name))
                    {
                        display.Append(string.Format("[ {0}:{1} ] ", SharedStrings.MASK, pattern));
                    }
                }
            }

            if (field is DateField && (string.IsNullOrEmpty(field.Name) == false))
            {
                System.Globalization.DateTimeFormatInfo formatInfo = System.Globalization.DateTimeFormatInfo.CurrentInfo;

                display.Append(string.Format("[ {0}:{1} {2} ] ",
                    SharedStrings.TYPE, 
                    field.FieldType,
                    formatInfo.ShortDatePattern.ToUpper()));

                if (((DateField)field).LowerDate.Ticks > 0)
                {
                    display.Append(string.Format("[ Lower: {0} ] ", ((DateField)field).LowerDate.ToShortDateString()));
                }

                if (((DateField)field).UpperDate.Ticks > 0)
                {
                    display.Append(string.Format("[ Upper: {0} ] ", ((DateField)field).UpperDate.ToShortDateString()));
                }
            }
            else if (string.IsNullOrEmpty(field.Name) == false)
            {
                display.Append(string.Format("[ {0}:{1} ] ", SharedStrings.TYPE, field.FieldType));
            }
            
            DisplayFormat(display.ToString(), new EventArgs());

            this.EnterCheckCodeEngine.CheckCodeHandler(sender, new RunCheckCodeEventArgs(EventActionEnum.OpenField, e.Field.Name));
        }

        public void DirtyFieldHandler(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        public void ClickFieldHandler(object sender, ClickFieldEventArg e)
        {
            Control currentControl = (Control)sender;
            if (e.Field is RelatedViewField)
            {
                this.EnterCheckCodeEngine.CheckCodeHandler(sender, new RunCheckCodeEventArgs(EventActionEnum.ClickField, ""));
                
                if (CheckRequiredFields(currentControl))
                {
                    RelatedViewField rvField = (RelatedViewField)e.Field;
                    Epi.View childView = rvField.GetProject().Metadata.GetChildView(rvField);

                    if (childView == null)
                    {
                        MsgBox.Show(SharedStrings.WARNING_CHILD_VIEW_NOT_SET, SharedStrings.ENTER);
                    }
                    else
                    {
                        childView.ReturnToParent = rvField.ShouldReturnToParent;
                        if (childView != null)
                        {
                            this.SetFieldData();
                            this.EnterCheckCodeEngine.SaveRecord();
                            childView.ForeignKeyField.CurrentRecordValueString = rvField.GetView().CurrentGlobalRecordId;
                            this.OpenViewHandler(this, new OpenViewEventArgs(childView));

                        }
                    }
                }
            }
            else if (e.Field is CommandButtonField)
            {
                ControlFactory factory = ControlFactory.Instance;
                Epi.Page ThisPage = this.EnterCheckCodeEngine.CurrentView.CurrentPage;
                Epi.Fields.Field ThisField = this.EnterCheckCodeEngine.CurrentView.CurrentField;

                this.EnterCheckCodeEngine.CheckCodeHandler(sender, new RunCheckCodeEventArgs(EventActionEnum.ClickField, ""));                
            }
            else if (e.Field is ImageField)
            {
                SelectImage(e.Field as ImageField, sender as PictureBox);
            }
            else if (e.Field is CheckBoxField)
            {
                ControlFactory factory = ControlFactory.Instance;
                Epi.Page ThisPage = this.EnterCheckCodeEngine.CurrentView.CurrentPage;
                Epi.Fields.Field ThisField = this.EnterCheckCodeEngine.CurrentView.CurrentField;
                this.EnterCheckCodeEngine.CheckCodeHandler(sender, new RunCheckCodeEventArgs(EventActionEnum.ClickField, ""));
            }
            else if (e.Field is OptionField)
            {
                ControlFactory factory = ControlFactory.Instance;
                Epi.Page ThisPage = this.EnterCheckCodeEngine.CurrentView.CurrentPage;
                Epi.Fields.Field ThisField = this.EnterCheckCodeEngine.CurrentView.CurrentField;
                this.EnterCheckCodeEngine.CheckCodeHandler(sender, new RunCheckCodeEventArgs(EventActionEnum.ClickField, ""));
            }
        }

        public void CloseFieldHandler(object sender, CloseFieldEventArg closeFieldEventArg)
        {
            Control currentControl = (Control)sender;
            
            if (closeFieldEventArg.Field is Epi.Fields.GridField)
            {

            }
            else if (closeFieldEventArg.Field is ImageField)
            {
                SelectImage(closeFieldEventArg.Field as ImageField, currentControl as PictureBox);
            }
            else
            {
                if (closeFieldEventArg.Field is CheckBoxField)
                {
                    SetCheckBoxData(closeFieldEventArg.Field, currentControl);
                }
                else
                {
                    ControlFactory factory = ControlFactory.Instance;
                    List<Control> fieldControl = factory.GetAssociatedControls(closeFieldEventArg.Field);
                    
                    if (fieldControl.Count > 0)
                    {
                        int index = 0;

                        if (fieldControl.Count > 1)
                        {
                            index++;
                        }
                        if (IsValidData(fieldControl[index]) == false)
                        {
                            return;
                        }
                        
                        SetFieldData(fieldControl[index]);
                    }
                }

                Epi.Page ThisPage = this.EnterCheckCodeEngine.CurrentView.CurrentPage;
                Epi.Fields.Field ThisField = this.EnterCheckCodeEngine.CurrentView.CurrentField;

                this.EnterCheckCodeEngine.CheckCodeHandler(sender, new RunCheckCodeEventArgs(EventActionEnum.CloseField, ""));
                View TestView = this.EnterCheckCodeEngine.CurrentView.View;
                
                if (
                    ThisPage == this.EnterCheckCodeEngine.CurrentView.CurrentPage
                    && ThisField == this.EnterCheckCodeEngine.CurrentView.CurrentField
                    )
                {
                    if (this.IsLastControlOnPage(currentControl) && closeFieldEventArg.Tab == true)
                    {
                        if (this.IsLastControlOnView(currentControl) && isNewRecord == false)
                        {
                            Page firstPage = this.EnterCheckCodeEngine.CurrentView.View.Pages[0];
                            this.OpenPageHandler(sender, new PageSelectedEventArgs(firstPage));
                        }
                        else if (this.IsLastControlOnView(currentControl) && isNewRecord == true)
                        {
                            if (!AllowOneRecordOnly)
                            {
                                this.GoToRecordHandler(sender, new GoToRecordEventArgs("+"));
                            }
                        }
                        else if (canvas.EnableTabToNextControl)
                        {
                            for (int i = 0; i < TestView.Pages.Count; i++)
                            {
                                Page TestPage = this.EnterCheckCodeEngine.CurrentView.View.Pages[i];
                                if (TestPage.Equals(this.currentPage))
                                {
                                    if (i + 1 < TestView.Pages.Count)
                                    {
                                        this.OpenPageHandler(sender, new PageSelectedEventArgs(TestView.Pages[i + 1]));
                                    }

                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (this.canvas.EnableTabToNextControl)
                        {
                            canvas.GoToNextControl(this.currentPage, this.view, currentControl);
                        }
                    }
                }
            }
        }

        public void FieldChangeHandler(object sender, Field field)
        {
            Control currentControl = (Control)sender;

            if (field is Epi.Fields.TableBasedDropDownField)
            {
                ControlFactory factory = ControlFactory.Instance;
                List<Control> fieldControl = factory.GetAssociatedControls(field);

                if (fieldControl.Count > 0)
                {
                    int index = 0;

                    if (fieldControl.Count > 1)
                    {
                        index++;
                    }
                    if (IsValidData(fieldControl[index]) == false)
                    {
                        return;
                    }

                    SetFieldData(fieldControl[index]);
                }

                this.EnterCheckCodeEngine.CheckCodeHandler(sender, new RunCheckCodeEventArgs(EventActionEnum.ClickField, ""));
            }
        }

        public void SaveRecordHandler(object sender, EventArgs e)
        {
            if (this.view != null)
            {
                this.SetFieldData();
                this.SaveRecord();
            }
        }

        public void MarkAsDeletedRecordHandler(object sender, EventArgs e)
        {
            this.SetFieldData();

            if (view.IsEmptyNewRecord() == false)
            {
                this.EnterCheckCodeEngine.CurrentView.View.RecStatusField.CurrentRecordValue = 0;
                this.EnterCheckCodeEngine.CurrentView.SaveRecord();
                //begin -- perform cascading delete of any related view records.
                if (this.EnterCheckCodeEngine.CurrentView.View.UniqueKeyField.CurrentRecordValueObject != null && !string.IsNullOrEmpty(this.EnterCheckCodeEngine.CurrentView.View.UniqueKeyField.CurrentRecordValueObject.ToString()))
                {
                    int RecordId = 0;
                    if (int.TryParse(this.EnterCheckCodeEngine.CurrentView.View.UniqueKeyField.CurrentRecordValueObject.ToString(), out RecordId))
                    {
                        this.MarkRelatedRecoredsAsDeleted(RecordId);
                    }
                }
                //end -- perform cascading delete of any related view records.
                this.Render();
                string guid = this.EnterCheckCodeEngine.CurrentView.View.GlobalRecordIdField.GlobalRecordId;
                UpdateRecStatus(this.EnterCheckCodeEngine.CurrentView.View, true, guid);
            }
        }

        public void UnMarkDeletedRecordHandler(object sender, EventArgs e)
        {
            this.EnterCheckCodeEngine.CurrentView.View.RecStatusField.CurrentRecordValue = 1;
            this.EnterCheckCodeEngine.CurrentView.SaveRecord();
            //begin -- perform cascading delete of any related view records.
            if (this.EnterCheckCodeEngine.CurrentView.View.UniqueKeyField.CurrentRecordValueObject != null && !string.IsNullOrEmpty(this.EnterCheckCodeEngine.CurrentView.View.UniqueKeyField.CurrentRecordValueObject.ToString()))
            {
                int RecordId = 0;
                if (int.TryParse(this.EnterCheckCodeEngine.CurrentView.View.UniqueKeyField.CurrentRecordValueObject.ToString(), out RecordId))
                {
                    this.UnMarkRelatedRecoredsAsDeleted(RecordId);
                }
            }
            //end -- perform cascading delete of any related view records.
            this.Render();
            string guid = this.EnterCheckCodeEngine.CurrentView.View.GlobalRecordIdField.GlobalRecordId;
            UpdateRecStatus(this.EnterCheckCodeEngine.CurrentView.View, false, guid);

        }

        private void UpdateRecStatus(View view, bool isDelete, string globalRecordId)
        {
            Epi.Data.Services.CollectedDataProvider collectedData = this.EnterCheckCodeEngine.Project.CollectedData;

            Epi.Data.Query updateQuery = collectedData.CreateQuery("update " + view.TableName + " set [RecStatus] = @RecStatus where [GlobalRecordId] = @GlobalRecordId");
            updateQuery.Parameters.Add(new Epi.Data.QueryParameter("@RecStatus", DbType.Int32, isDelete ? 0 : 1));
            updateQuery.Parameters.Add(new Epi.Data.QueryParameter("@GlobalRecordId", DbType.String, globalRecordId));
            collectedData.ExecuteNonQuery(updateQuery);
            
            // < GRID TABLES >

            foreach (GridField gridField in view.Fields.GridFields)
            {
                UpdateRecStatus(gridField, isDelete, globalRecordId);
            }

            // < CHILD VIEW >

            List<Epi.Fields.RelatedViewField> RelatedViewList = FindRelatedFields(view);
            
            foreach (Epi.Fields.RelatedViewField field in RelatedViewList)
            {
                View relatedView = this.EnterCheckCodeEngine.Project.GetViewById(field.RelatedViewID);

                if (collectedData.TableExists(relatedView.Name))
                {
                    Epi.Data.Query childRecordQuery = collectedData.CreateQuery("Select GlobalRecordId From " + relatedView.TableName + " Where [FKEY] = @FKEY");
                    childRecordQuery.Parameters.Add(new Epi.Data.QueryParameter("@FKEY", DbType.String, globalRecordId));
                    IDataReader dataReader = collectedData.ExecuteReader(childRecordQuery);

                    while(dataReader.Read())
                    {
                        string readerGlobalRecordId = dataReader["GlobalRecordId"].ToString();
                        UpdateRecStatus(relatedView, isDelete, readerGlobalRecordId);
                    }
                }
            }
        }

        private void UpdateRecStatus(GridField gridField, bool isDelete, string globalRecordId)
        {
            Epi.Data.Services.CollectedDataProvider collectedData = this.EnterCheckCodeEngine.Project.CollectedData;

            Epi.Data.Query updateQuery = collectedData.CreateQuery("update " + gridField.TableName + " set [RecStatus] = @RecStatus where [FKEY] = @FKEY");
            updateQuery.Parameters.Add(new Epi.Data.QueryParameter("@RecStatus", DbType.Int32, isDelete ? 0 : 1));
            updateQuery.Parameters.Add(new Epi.Data.QueryParameter("@FKEY", DbType.String, globalRecordId));
            collectedData.ExecuteNonQuery(updateQuery);
        }

        private List<Epi.Fields.RelatedViewField> FindRelatedFields(View pView)
        {
            List<Epi.Fields.RelatedViewField> result = new List<RelatedViewField>();

            foreach (Epi.Fields.Field field in pView.Fields)
            {
                if (field is Epi.Fields.RelatedViewField)
                {
                    result.Add((Epi.Fields.RelatedViewField) field);
                }
            }

            return result;
        }

        public void DataGridRowAddedHandler(object sender, DataGridViewRowEventArgs e)
        {
            ControlFactory factory = ControlFactory.Instance;
            Field field = factory.GetAssociatedField((Control)sender);
            if ((Control)sender is DataGridView)
            {
                DataGridView dataGridView = sender as DataGridView;

                if (dataGridView.DataSource is DataTable)
                {
                    DataTable dataTable = dataGridView.DataSource as DataTable;
                    if (dataTable.Rows.Count > 1)
                    {
                        view.Project.CollectedData.SaveGridRecord(view, int.Parse(this.view.UniqueKeyField.CurrentRecordValueString), (GridField)field, (DataTable)dataTable);
                    }
                }
                /**/
            }

        }

        private void Render()
        {
            if (this.view != null)
            {
                bool enable = true;

                if (!string.IsNullOrEmpty(this.view.RecStatusField.CurrentRecordValueString))
                {
                    enable = viewExplorer.GetRecStatus(int.Parse(this.view.RecStatusField.CurrentRecordValueString));
                }

                this.canvas.CurrentView = this.EnterCheckCodeEngine.CurrentView.View;
                this.mainForm.RunTimeView = this.EnterCheckCodeEngine.CurrentView;
                canvas.Render(enable);
                mainForm.Render();
                viewExplorer.Render();
                linkedRecordsViewer.Render(this.EnterCheckCodeEngine.CurrentView);

                if (!string.IsNullOrEmpty(this.view.RecStatusField.CurrentRecordValueString))
                {
                    EnableDisableCurrentRecord(this.view, enable);
                }
            }
            else
            {
                canvas.Reset();
                mainForm.Reset();
                mainForm.isViewOpened = false;
                mainForm.Menu_Render();
                viewExplorer.Reset();
            }
        }

        private void WriteGC(object o)
        {
            Console.WriteLine("******* Mediator  - Garbage Collector Status ******");
            Console.WriteLine("Estimated bytes on heap: {0}", GC.GetTotalMemory(false));
            Console.WriteLine("This OS has {0} object generations.\n", GC.MaxGeneration + 1);
            Console.WriteLine("Generation of parameter object is: {0}", GC.GetGeneration(o));
        }
    }
}
