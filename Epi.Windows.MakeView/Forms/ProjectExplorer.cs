#region Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Epi.Data;
using Epi.Fields;
using Epi.Windows;
using Epi.Windows.Controls;
using Epi.Windows.Docking;
using Epi.Windows.MakeView.Dialogs;
using Epi.Data.Services;
using EpiInfo.Plugin;
using System.Net.Mail;
#endregion  //Namespaces

namespace Epi.Windows.MakeView.Forms
{
    #region Public Delegates

    public delegate void ProjectExplorerPageSelectedEventHandler(Page page);
    public delegate void ProjectExplorerNewOpenFieldEventHandler(MetaFieldType type);
    public delegate void ProjectExplorerNewProjectFromTemplateEventHandler(string templatePathFragment);
    public delegate void ProjectExplorerCreateFieldLayoutControlEventHandler(Size size);
    public delegate void ProjectExplorerApplyDefaultFont(Font controlFont, Font promptFont, int viewId, int pageId);

    #endregion

    /// <summary>
    /// MakeView's project explorer 
    /// </summary>
    public partial class ProjectExplorer : Epi.Windows.Docking.DockWindow
    {
        #region Fields
        private TreeNode rightClickedNode;
        private TreeNode leftClickedNode;
        private TreeNode fieldsNode;
        private PageNode draggedPageNode;
        private TemplateNode draggedTemplateNode;
        private TreeNode dropOnNode;
        private MakeViewMainForm mainForm;
        public bool OpenForViewingOnly = false;
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectExplorer));
        private string _nameOfCopiedTemplate;

        public Page currentPage;
        #endregion 

        #region Public Events
        public event ProjectExplorerPageSelectedEventHandler PageSelected;
        public event ProjectExplorerNewOpenFieldEventHandler CreateNewOpenField;
        public event ProjectExplorerNewProjectFromTemplateEventHandler CreateNewProjectFromTemplate;
        public event ProjectExplorerCreateFieldLayoutControlEventHandler CreateFieldLayoutControl;
        public event ProjectExplorerApplyDefaultFont ApplyDefaultFont;
        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="frm">The main form</param>
        public ProjectExplorer(MakeViewMainForm frm) : base(frm)
        {
            InitializeComponent();

            AddOpenFields();
            UpdateTemplates(expandAllNodes: true);

            this.mainForm = frm;
            projectTree.Enabled = true;
        }

        #endregion  //Constructors

        #region Public Methods

        /// <summary>
        /// Gets a value indicating whether a project is currently loaded
        /// </summary>
        public bool IsProjectLoaded
        {
            get
            {
                bool isProjectLoaded = false;
                if (projectTree.Nodes.Count > 0 && projectTree.Nodes[0] is ProjectNode)
                {
                    isProjectLoaded = true;
                }
                return (isProjectLoaded);
            }
        }

        /// <summary>
        /// Gets a value indicating whether a page node is selected in the projectTree
        /// </summary>
        public bool IsPageNodeSelected
        {
            get
            {
                bool isPageNodeSelected = false;
                if (projectTree.SelectedNode is PageNode)
                {
                    isPageNodeSelected = true;
                }
                return (isPageNodeSelected);
            }
        }

        /// <summary>
        /// Resets the project explorer to its initial state
        /// </summary>
        public void Reset()
        {
            if (IsProjectLoaded)
            {
                projectTree.Nodes.Clear();
            }

            UpdateTemplates(expandAllNodes: true);

            if (this.mainForm.EpiInterpreter != null)
            {
                this.mainForm.EpiInterpreter.Context.RemoveVariablesInScope(VariableScope.DataSource | VariableScope.Standard | VariableScope.DataSourceRedefined);
            }
        }

        /// <summary>
        /// Loads a project into the explorer
        /// </summary>
        /// <param name="project">The project to load</param>
        public void LoadProject(Project project)
        {
            Epi.Windows.Controls.ProjectNode projectNode = new Epi.Windows.Controls.ProjectNode(project);
            
            projectTree.Nodes.Insert(0, projectNode);
            
            if (projectNode.Nodes.Count > 0)
            {
                if (projectNode.Nodes[0].Nodes.Count > 0)
                {
                    projectTree.SelectedNode = projectNode.Nodes[0].Nodes[0];
                }
                else
                {
                    projectTree.SelectedNode = projectNode.Nodes[0];
                }
            }

            AddOpenFields();
            UpdateTemplates();

            projectTree.Enabled = true;
        }

        /// <summary>
        /// Saves the view's data fields as local variables in the memory region
        /// </summary>
        public void SaveFieldVariables()
        {
            if (projectTree.SelectedNode is PageNode)
            {
                //Remove field variables for previously loaded view.
                this.mainForm.EpiInterpreter.Context.RemoveVariablesInScope(VariableScope.DataSource | VariableScope.Standard | VariableScope.DataSourceRedefined);

                //Load field variables for the current view.
                View currentView = ((ViewNode)projectTree.SelectedNode.Parent).View;
                foreach (IDataField dataField in currentView.Fields.DataFields)
                {
                    this.mainForm.EpiInterpreter.Context.DefineVariable(dataField);
                }
            }
        }

        /// <summary>
        /// Adds a view to the current project
        /// </summary>
        /// <param name="view">The view to add</param>
        public void AddView(View view)
        {
            bool NodeFound = false;
            ViewNode VN = null;
            // check that the View does NOT already exist in the project tree
            for (int i = 0; i < projectTree.Nodes[0].Nodes.Count; i++)
            {
                VN = ((ViewNode)projectTree.Nodes[0].Nodes[i]);

                if (VN.View.Id == view.Id)
                {
                    NodeFound = true;
                    break;
                }
            }

            if (!NodeFound)
            {
                Epi.Windows.Controls.ViewNode viewNode = ((ProjectNode)projectTree.Nodes[0]).InsertViewNode(view);
                //projectTree.Nodes[0].Collapse();
                if (viewNode.Nodes.Count > 0)
                {
                    projectTree.SelectedNode = viewNode.Nodes[0];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        public void SetView(View view)
        {
            bool NodeFound = false;
            ViewNode VN = null;
            int viewNodeIndex = -1;

            for (int i = 0; i < projectTree.Nodes[0].Nodes.Count; i++)
            {
                VN = ((ViewNode)projectTree.Nodes[0].Nodes[i]);

                if (VN.View.Id == view.Id)
                {
                    NodeFound = true;
                    viewNodeIndex = i;
                    break;
                }
            }

            if (NodeFound)
            {
                Epi.Windows.Controls.ViewNode viewNode = ((ViewNode)projectTree.Nodes[0].Nodes[viewNodeIndex]);
                viewNode.View = view;
                string viewName = view.Name;

                foreach (TreeNode node in viewNode.Nodes)
                {
                    if (node is PageNode)
                    {
                        if (((PageNode)node).Page.view.Name.Equals(viewName))
                        {
                            PageNode pageNode = node as PageNode;
                            pageNode.Page.view = view;
                        }
                    }
                }                
            }
        }

        public bool ViewNameAlreadyExists(string viewNameCandidate )
        {
            bool NodeFound = false;
            ViewNode VN = null;

            for (int i = 0; i < projectTree.Nodes[0].Nodes.Count; i++)
            {
                VN = ((ViewNode)projectTree.Nodes[0].Nodes[i]);

                if (VN.View.Name == viewNameCandidate)
                {
                    NodeFound = true;
                    break;
                }
            }

            return NodeFound;
        }

        /// <summary>
        /// Method to add a view to the tree without changing the selection
        /// </summary>
        /// <param name="view"></param>
        public void AddViewWithOutChangingSelection(View view)
        {
            bool NodeFound = false;
            ViewNode VN = null;
            // check that the View does NOT already exist in the project tree
            for (int i = 0; i < projectTree.Nodes[0].Nodes.Count; i++)
            {
                VN = ((ViewNode)projectTree.Nodes[0].Nodes[i]);

                if (VN.View.Id == view.Id)
                {
                    NodeFound = true;
                    break;
                }
            }

            if (!NodeFound)
            {
                Epi.Windows.Controls.ViewNode viewNode = ((ProjectNode)projectTree.Nodes[0]).InsertViewNode(view);
            }
        }

        /// <summary>
        /// Deletes the currently selected page
        /// </summary>
        public void DeleteCurrentPage()
        {
            string msg = SharedStrings.CONFIRM_PAGE_DELETE;            
            DeletePage((PageNode)projectTree.SelectedNode, msg);
        }

        /// <summary>
        /// Renames the currently selected page
        /// </summary>
        public void RenameCurrentPage()
        {
            RenamePage((PageNode)projectTree.SelectedNode);
        }

        /// <summary>
        /// Adds a view to the project
        /// </summary>
        public void AddView()
        {
            View view = PromptUserToCreateView();
            if (view != null)
            {
                Page page = view.CreatePage("New Page", 0);
                AddView(view);
            }
        }

        /// <summary>
        /// Adds the first page to a View. 
        /// </summary>
        /// <param name="treeNode">The node the page is to be added</param>        
        public void AddFirstPage(TreeNode viewTreeNode)
        {
            string pageName = "Page " + (((ViewNode)viewTreeNode).View.Pages.Count + 1).ToString();
            CreateAndInsertNewPage(viewTreeNode, pageName);
        }

        /// <summary>
        /// Add a page to a view's tree node
        /// </summary>
        /// <param name="treeNode">The parent node of the page node to be added</param>        
        public void AddPage(TreeNode viewTreeNode)
        {
            RenamePageDialog dialog = new RenamePageDialog(this.mainForm, ((ViewNode)viewTreeNode).View);
            View currentView = ((ViewNode)viewTreeNode).View;
            string pageName = "Page " + (currentView.Pages.Count + 1).ToString();
            dialog.PageName = pageName;

            DialogResult result = dialog.ShowDialog();
            pageName = dialog.PageName;

            if (result == DialogResult.OK)
            {
                CreateAndInsertNewPage(viewTreeNode, pageName);
            }
        }
      
        public void InsertPage(TreeNode pageTreeNode)
        {
            if (pageTreeNode is PageNode)
            { 
                PageNode pageNode = (PageNode)pageTreeNode;
                ViewNode viewTreeNode = (ViewNode)pageNode.Parent;
                RenamePageDialog dialog = new RenamePageDialog(this.mainForm, ((ViewNode)viewTreeNode).View);
                dialog.PageName = "InsertPage";
                DialogResult result = dialog.ShowDialog();
                string pageName = dialog.PageName;

                if (result == DialogResult.OK)
                {
                    Page page = ((ViewNode)viewTreeNode).View.CreatePage(pageName, pageNode.Index);
                    PageNode node = ((ViewNode)viewTreeNode).InsertPageNode(page, pageNode.Index);
                    projectTree.SelectedNode = node;

                    foreach (TreeNode eachNode in pageTreeNode.Parent.Nodes)
                    {
                        if (eachNode is PageNode)
                        {
                            ((PageNode)eachNode).Page.Position = eachNode.Index;
                            ((PageNode)eachNode).Page.SaveToDb();
                        }
                    }
                }
            }
        }

        private void CreateAndInsertNewPage(TreeNode viewTreeNode, string pageName)
        {
            Page page = ((ViewNode)viewTreeNode).View.CreatePage(pageName, ((ViewNode)viewTreeNode).View.Pages.Count);
            InsertPage(viewTreeNode, page);
        }

        private void InsertPage(TreeNode viewTreeNode, Page page)
        {
            PageNode pageNode;
            
            if (dropOnNode != null)
            {
                pageNode = ((ViewNode)viewTreeNode).InsertPageNode(page, dropOnNode.Index);
            }
            else
            {
                pageNode = ((ViewNode)viewTreeNode).InsertPageNode(page, ((ViewNode)viewTreeNode).View.Pages.Count);
            }

            projectTree.SelectedNode = pageNode;

            pageNode.Text = page.Name;
            pageNode.Page.Name = page.Name;
            pageNode.Page.SaveToDb();
            ViewNode viewNode = (ViewNode)pageNode.Parent;

            Page foundPage = viewNode.View.Pages.Find(delegate(Page p) { return p.Id == pageNode.Page.Id; });

            if (foundPage != null)
            {
                foundPage.Name = page.Name;
            }
            else
            {
                viewNode.View.Pages.Add(page);
            }

            PageSelected(SelectedPage);
        }
      
        /// <summary>
        /// Add a page to a view's tree node
        /// </summary>
        /// <param name="page">new page</param>
        /// <param name="view">target view</param>
        public void AddPageNode(Page page, View view, bool insertAsLastPage = false)
        {
            ViewNode viewNode = null;

            if (insertAsLastPage == true)
            {
                dropOnNode = null;
            }

            for (int i = 0; i < projectTree.Nodes[0].Nodes.Count; i++)
            {
                viewNode = ((ViewNode)projectTree.Nodes[0].Nodes[i]);

                if (viewNode.View.Id == view.Id)
                {
                    projectTree.SelectedNode = viewNode;
                    break;
                }
            }
            
            ViewNode viewNodeSelected = (ViewNode)projectTree.SelectedNode;
            InsertPage(viewNodeSelected, page);
        }

        /// <summary>
        /// Inserts a page
        /// </summary>        
        public void InsertPage()
        {
            if (projectTree.SelectedNode is PageNode)
            {
                if (((ViewNode)(projectTree.SelectedNode.Parent)).View.Pages.Count < 150) // TODO: Hard coded (magic) number not allowed. Not sure who wrote this code.
                {
                       RenamePageDialog dialog = new RenamePageDialog(this.mainForm, ((ViewNode)projectTree.SelectedNode.Parent).View);
                       dialog.PageName = "InsertPage"; ;
                       DialogResult result = dialog.ShowDialog();
                       string pageName = dialog.PageName;
                       if (result == DialogResult.OK)
                       {
                           Page page = ((ViewNode)projectTree.SelectedNode.Parent).View.CreatePage(pageName, projectTree.SelectedNode.Index);
                           // Page page = ((ViewNode)projectTree.SelectedNode.Parent).View.CreatePage(SharedStrings.INSERTED_PAGE, projectTree.SelectedNode.Index);
                           PageNode node = ((ViewNode)projectTree.SelectedNode.Parent).InsertPageNode(page, projectTree.SelectedNode.Index);
                           projectTree.SelectedNode = node;

                           foreach (TreeNode eachNode in node.Parent.Nodes)
                           {
                               if (eachNode is PageNode)
                               {
                                   ((PageNode)eachNode).Page.Position = eachNode.Index;
                                   ((PageNode)eachNode).Page.SaveToDb();
                               }
                           }
                       }
                }
                else
                {
                    MessageBox.Show(SharedStrings.MAXIMUM_NUMBER_PAGES, SharedStrings.MAX_PAGES_REACHED, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (projectTree.SelectedNode is ViewNode)
            {
                if (((ViewNode)projectTree.SelectedNode).View.Pages.Count < 150) // TODO: Hard coded (magic) number not allowed. Not sure who wrote this code.
                {
                    RenamePageDialog dialog = new RenamePageDialog(this.mainForm, ((ViewNode)projectTree.SelectedNode).View) ;
                    dialog.PageName = "InsertPage"; ;
                    DialogResult result = dialog.ShowDialog();
                    string pageName = dialog.PageName;
                    if (result == DialogResult.OK)
                    {
                      //Page page = ((ViewNode)projectTree.SelectedNode).View.CreatePage(SharedStrings.INSERTED_PAGE, 0);
                        Page page = ((ViewNode)projectTree.SelectedNode).View.CreatePage(pageName, 0);
                        PageNode node = ((ViewNode)projectTree.SelectedNode).InsertPageNode(page, 0);
                        projectTree.SelectedNode = node;

                        foreach (TreeNode eachNode in node.Parent.Nodes)
                        {
                            if (eachNode is PageNode)
                            {
                                ((PageNode)eachNode).Page.Position = eachNode.Index;
                                ((PageNode)eachNode).Page.SaveToDb();
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show(SharedStrings.MAXIMUM_NUMBER_PAGES, SharedStrings.MAX_PAGES_REACHED, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MsgBox.ShowError(SharedStrings.SPECIFY_VIEW_TO_ADD_PAGE);
            }
        }

        /// <summary>
        /// Adds a page to the currently selected view
        /// </summary>
        public void AddPageToCurrentView()
        {
            TreeNode selectedNode = (projectTree.SelectedNode.Parent);

            if (projectTree.SelectedNode is ViewNode)
            {
                selectedNode = projectTree.SelectedNode;
            }
            else if (!(projectTree.SelectedNode is PageNode))
            {
                MsgBox.ShowError(SharedStrings.SPECIFY_VIEW_TO_ADD_PAGE);
                return;
            }

            if (projectTree.Nodes[0].Nodes[0].Nodes.Count > 0)
            {
                if (((ViewNode)selectedNode).View.Pages.Count < 150)
                {
                    AddPage(selectedNode);
                }
                else
                {
                    MessageBox.Show(SharedStrings.MAXIMUM_NUMBER_PAGES);
                }
            }
            else
            {
                AddFirstPage(projectTree.Nodes[0].Nodes[0]);
            }
        }

        /// <summary>
        /// Selects a view based on view name
        /// </summary>
        /// <param name="viewName">The name of the view</param>
        public void SelectView(string viewName)
        {
            foreach (TreeNode node in projectTree.Nodes[0].Nodes)
            {
                if (node is ViewNode)
                {
                    if (((ViewNode)node).View.Name.Equals(viewName))
                    {
                        if (node.Nodes.Count > 0)
                        {
                            projectTree.SelectedNode = node.Nodes[0];
                        }
                    }
                }
            }
        }

        public void SelectPage(Page page)
        {
            string viewName = page.view.Name;

            foreach (TreeNode viewNode in projectTree.Nodes[0].Nodes)
            {
                if (viewNode is ViewNode)
                {
                    if (((ViewNode)viewNode).View.Name.Equals(viewName))
                    {
                        if (viewNode.Nodes.Count > 0)
                        {
                            foreach (TreeNode pageNode in viewNode.Nodes)
                            {
                                if (((PageNode)pageNode).Page.Id == page.Id)
                                {
                                    projectTree.SelectedNode = pageNode;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion  //Public Methods

        #region Event Handlers

        /// <summary>
        /// Handles the drag event of the project Tree 
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">Item drag event parameters</param>
        private void projectTree_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Item is TemplateNode)
            {
                DataObject data = new DataObject("DraggedTemplateNode", e.Item);
                DoDragDrop(data, DragDropEffects.Move);
            }
            else if (e.Item is FieldNode)
            {
                DataObject data = new DataObject("DraggedTreeNode", e.Item);
                DoDragDrop(data, DragDropEffects.Move);
            }
            else if (e.Item is PageNode)
            {
                DataObject data = new DataObject("DraggedPageNode", DragDropEffects.Move);
                DoDragDrop(data, DragDropEffects.Move);
            }
        }

        void projectTree_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (sender is TreeView)
            {
                Point pt = new Point(e.X, e.Y);
                pt = ((TreeView)sender).PointToClient(pt);
                dropOnNode = ((TreeView)sender).GetNodeAt(pt);

                if (e.Data.GetData("DraggedPageNode") != null)
                {
                    if (dropOnNode != null && dropOnNode.Parent != null && dropOnNode is PageNode)
                    {
                        if (dropOnNode != draggedPageNode)
                        { 
                            draggedPageNode.Remove();
                            int mouseOverIndex = dropOnNode.Index;
                            
                            if (dropOnNode.Parent != null)
                            {
                                dropOnNode.Parent.Nodes.Insert(mouseOverIndex, draggedPageNode);

                                foreach (TreeNode pageNode in dropOnNode.Parent.Nodes)
                                {
                                    if (pageNode is PageNode)
                                    {
                                        ((PageNode)pageNode).Page.Position = pageNode.Index;
                                        ((PageNode)pageNode).Page.SaveToDb();
                                    }
                                }
                            }
                        }
                    }
                }
                if (e.Data.GetData("DraggedTemplateNode") != null)
                {
                    if (dropOnNode != null && dropOnNode is PageNode)
                    {
                        int dropOnNodePosition = ((PageNode)dropOnNode).Page.Position;
                        SelectView(((PageNode)dropOnNode).Page.view.Name);
                        Template template = new Template(this.mainForm.mediator);
                        template.CreateFromTemplate((TemplateNode)draggedTemplateNode, dropOnNodePosition);

                        int mouseOverIndex = dropOnNodePosition;

                        if (dropOnNode != null && dropOnNode.Parent != null)
                        {
                            foreach (TreeNode pageNode in dropOnNode.Parent.Nodes)
                            {
                                if (pageNode is PageNode)
                                {
                                    ((PageNode)pageNode).Page.Position = pageNode.Index;
                                    ((PageNode)pageNode).Page.SaveToDb();
                                }
                            }
                        }

                        Page selectedPage = this.mainForm.mediator.ProjectExplorer.SelectedPage;
                        this.mainForm.mediator.projectExplorer_PageSelected(selectedPage);
                    }
                    else if (dropOnNode != null && dropOnNode is ViewNode)
                    {
                        if (draggedTemplateNode.FullPath.Contains(@"Templates\Pages\"))
                        {
                            int dropOnNodePosition = ((ViewNode)dropOnNode).Nodes.Count;
                            SelectView(((ViewNode)dropOnNode).Text);
                            Template template = new Template(this.mainForm.mediator);
                            template.CreateFromTemplate((TemplateNode)draggedTemplateNode, dropOnNodePosition);
                            int mouseOverIndex = dropOnNodePosition;
                            if (dropOnNode != null && dropOnNode.Parent != null)
                            {
                                foreach (TreeNode pageNode in dropOnNode.Parent.Nodes)
                                {
                                    if (pageNode is PageNode)
                                    {
                                        ((PageNode)pageNode).Page.Position = pageNode.Index;
                                        ((PageNode)pageNode).Page.SaveToDb();
                                    }
                                }
                            }
                            Page selectedPage = this.mainForm.mediator.ProjectExplorer.SelectedPage;
                            this.mainForm.mediator.projectExplorer_PageSelected(selectedPage);
                        }
                        else
                        new Template(this.mainForm.mediator).CreateFromTemplate((TemplateNode)draggedTemplateNode, new Point(0, 0));
                    }
                }
            }

            dropOnNode = null;
        }

        void projectTree_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (sender is TreeView)
            {
                Point point = new Point(e.X, e.Y);
                TreeNode node = ((TreeView)sender).GetNodeAt(point);

                if (node != null && node is PageNode)
                {
                    draggedPageNode = (PageNode)node;
                }
                if (node != null && node is TemplateNode)
                {
                    draggedTemplateNode = (TemplateNode)node;
                }
            }
        }

        void projectTree_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (sender is TreeView)
            {
                if (e.Data.GetData("DraggedPageNode") != null)
                {
                    Point pt = new Point(e.X, e.Y);
                    pt = ((TreeView)sender).PointToClient(pt);
                    TreeNode hoverOverNode = ((TreeView)sender).GetNodeAt(pt);

                    if (hoverOverNode != null && hoverOverNode is PageNode)
                    {
                        if (((PageNode)hoverOverNode).Parent == draggedPageNode.Parent)
                        {
                            e.Effect = System.Windows.Forms.DragDropEffects.Move;
                        }
                        else
                        {
                            e.Effect = DragDropEffects.None;
                        }
                    }
                    else
                    {
                        e.Effect = DragDropEffects.None;
                    }
                }
                else if (e.Data.GetData("DraggedTemplateNode") != null)
                {
                    Point pt = new Point(e.X, e.Y);
                    pt = ((TreeView)sender).PointToClient(pt);
                    TreeNode hoverOverNode = ((TreeView)sender).GetNodeAt(pt);

                    if (hoverOverNode != null && draggedTemplateNode != null)
                    {
                        if (hoverOverNode is PageNode && (draggedTemplateNode.FullPath.Contains(@"\Forms\") || draggedTemplateNode.FullPath.Contains(@"\Pages\")))
                        {
                            e.Effect = System.Windows.Forms.DragDropEffects.Move;
                        }
                        else if (hoverOverNode is ViewNode && draggedTemplateNode.FullPath.Contains(@"\Forms\"))
                        {
                            e.Effect = System.Windows.Forms.DragDropEffects.Move;
                        }
                        else if (draggedTemplateNode.FullPath.Contains(@"Templates\Pages\"))
                        {
                            e.Effect = System.Windows.Forms.DragDropEffects.Move;
                        }
                        else
                        {
                            e.Effect = DragDropEffects.None;
                        }
                    }
                    else
                    {
                        e.Effect = DragDropEffects.None;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the After Select event of the project tree
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">Tree view event parameters</param>
        private void projectTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            MakeViewMainForm makeViewMainForm = (MakeViewMainForm)this.mainForm;
            makeViewMainForm.SetRenameDeletePageMenuItems();
            if (e.Node is PageNode)
            {
                currentPage = ((PageNode)e.Node).Page;
                foreach (Page page in mainForm.CurrentPage.view.Pages)
                {
                    if (page.Id == currentPage.Id)
                    {
                        currentPage = page;
                    }
                }
                
                if (PageSelected != null)
                {
                    PageSelected(currentPage);

                    try
                    {
                        makeViewMainForm.SetDataTableMenuItems();
                        makeViewMainForm.SetPublishMenuItems(currentPage.view);                                      
                        //if (!currentPage.view.Project.CollectedData.TableExists(currentPage.view.TableName))
                        //{
                        //    makeViewMainForm.SetDataTableMenuItems();
                        //    Configuration config = Configuration.GetNewInstance();

                        //    if (config.Settings.Republish_IsRepbulishable == true)
                        //    {
                        //        makeViewMainForm.SetPublishMenuItems(currentPage.view);
                        //    }
                        //}
                        //else
                        //{
                        //    makeViewMainForm.SetDataTableMenuItems();
                        //    makeViewMainForm.SetPublishMenuItems(currentPage.view);
                        //}
                    }
                    catch { }
                }
            }
            else if(e.Node is ViewNode)
            {
                if (this.mainForm != null)
                {
                    ViewNode viewNode = (ViewNode)e.Node;
                    if (viewNode != null)
                    {
                        if (!viewNode.View.Project.CollectedData.TableExists(viewNode.View.TableName))
                        {
                            makeViewMainForm.SetDataTableMenuItems();
                        }
                        else
                        {
                            makeViewMainForm.SetDataTableMenuItems();
                        }

                        makeViewMainForm.SetPublishMenuItems(viewNode.View);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Before Select event of the project tree
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">Tree View event parameters</param>
        private void projectTree_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            //if (e.Node is PageNode)
            //{
            //    currentPage = ((PageNode)e.Node).Page;
            //}
        }

        /// <summary>
        /// Handles the After Expand event of the project tree's nodes
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">Tree View Event parameters</param>
        private void projectTree_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (e.Node is ViewNode)
            {
                projectTree.SelectedNode = projectTree.Nodes[0].Nodes[e.Node.Index].Nodes[0];
            }
        }

        /// <summary>
        /// Handles the double click event of the project tree's nodes
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">Tree Node event parameters</param>
        private void projectTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node is ViewNode)
            {
                projectTree.SelectedNode = projectTree.Nodes[0].Nodes[e.Node.Index].Nodes[0];
                e.Node.ExpandAll();
            }
            else if (e.Node is TemplateNode)
            {
                TemplateNode templateNode = (TemplateNode)e.Node;
                Enums.TemplateLevel templateLevel = Template.GetTemplateLevel(templateNode);

                if (templateLevel == Enums.TemplateLevel.Field)
                {

                }
                else if (templateLevel == Enums.TemplateLevel.Page)
                {
                
                }
                else if (templateLevel == Enums.TemplateLevel.Form)
                {
                    string path = templateNode.FullPath;
                    CreateNewProjectFromTemplate(path);
                }
                else if (templateLevel == Enums.TemplateLevel.Project)
                {
                    string path = templateNode.FullPath;
                    CreateNewProjectFromTemplate(path);
                }
            }
            else if (e.Node is OpenFieldNode)
            {
                CreateNewOpenField(((MetaFieldType)((OpenFieldNode)e.Node).Value));
            }
        }

        /// <summary>
        /// Handles the Click event of the project tree's nodes
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">Tree Node event parameters</param>
        private void projectTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right )
            {
                rightClickedNode = e.Node;

                if (e.Node is PageNode)
                {
                    BuildPageContextMenu(e.Node).Show(projectTree, e.X, e.Y);
                }
                else if (e.Node is ViewNode)
                {
                    BuildViewContextMenu(e.Node).Show(projectTree, e.X, e.Y);
                }
                else if (e.Node is ProjectNode)
                {
                    BuildProjectContextMenu(e.Node).Show(projectTree, e.X, e.Y);
                }
                else if (e.Node is LinkedRootNode)
                {
                    BuildLinkedModeContextMenu().Show(projectTree, e.X, e.Y);
                }
                else if (e.Node is TemplateNode)
                {
                    BuildTemplateContextMenu(e.Node).Show(projectTree, e.X, e.Y);
                }
                else
                {
                    return;
                }
                projectTree.SelectedNode = e.Node;
            }
        }

        void ProjectExplorer_MouseLeave(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void mnuProjectUseDefaultFonts_Click(object sender, EventArgs e)
        {
            DialogResult result = MsgBox.ShowQuestion("The font of all controls including all labels will be changed to the default prompt. The changes cannot be undone.\r\n\r\nContinue with changes?");

            if (result == DialogResult.Yes)
            {
                System.Drawing.Font promptFont;
                System.Drawing.Font controlFont;

                mainForm.mediator.GetDefaultFonts(out promptFont, out controlFont, Configuration.GetNewInstance().Settings);

                currentPage.GetMetadata().UpdateFonts(controlFont, promptFont);
                mainForm.mediator.LoadPage(currentPage);
            }
        }
        
        void mnuViewUseDefaultFonts_Click(object sender, EventArgs e)
        {
            DialogResult result = MsgBox.ShowQuestion("The font of all controls including all labels will be changed to the default prompt. The changes cannot be undone.\r\n\r\nContinue with changes?");

            if (result == DialogResult.Yes)
            {
                System.Drawing.Font promptFont;
                System.Drawing.Font controlFont;

                mainForm.mediator.GetDefaultFonts(out promptFont, out controlFont, Configuration.GetNewInstance().Settings);

                int viewId = currentPage.view.Id;
                currentPage.GetMetadata().UpdateFonts(controlFont, promptFont, viewId: viewId);
                mainForm.mediator.LoadPage(currentPage);
            }
        }

        void mnuPageUseDefaultFonts_Click(object sender, EventArgs e)
        {
            DialogResult result = MsgBox.ShowQuestion("The font of all controls including all labels will be changed to the default prompt. The changes cannot be undone.\r\n\r\nContinue with changes?");
            
            if (result == DialogResult.Yes)
            {
                System.Drawing.Font promptFont;
                System.Drawing.Font controlFont;

                mainForm.mediator.GetDefaultFonts(out promptFont, out controlFont, Configuration.GetNewInstance().Settings);

                int pageId = currentPage.Id;
                int viewId = currentPage.view.Id;
                currentPage.GetMetadata().UpdateFonts(controlFont, promptFont,viewId: viewId, pageId: pageId);
                mainForm.mediator.LoadPage(currentPage);
            }
        }

        /// <summary>
        /// Handles the Click event of the New link Connection menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuNewLinkedConnection_Click(object sender, EventArgs e)
        {
            MsgBox.ShowInformation("This functionality will be available in a later build."); 
        }

        /// <summary>
        /// Handles the Click event of the Check Code for a view 
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuViewCheckCode_Click(object sender, EventArgs e)
        {
            Epi.Windows.MakeView.Forms.CheckCode checkCode = new Epi.Windows.MakeView.Forms.CheckCode(((ViewNode)rightClickedNode).View, (MakeViewMainForm)this.mainForm);
            checkCode.ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of the 'Save as Template' for a view 
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuSaveAsTemplate_Click(object sender, EventArgs e)
        {
            if (((System.Windows.Forms.ToolStripItem)(sender)).Text.Contains("Quick"))
            {
                Template template = new Template(this.mainForm.mediator);
                template.CreateProjectTemplate
                    (
                        this.projectTree.TopNode.Text,    
                        string.Format(@"Quick saved: {0}", DateTime.Now.ToString("dddd, MMMM dd, yyyy h:mm:ss tt")),
                        string.Format("{0} {1}", this.projectTree.TopNode.Text, DateTime.Now.ToString("yyyyMMddhmmsstt"))
                    );
                this.mainForm.mediator.ProjectExplorer.UpdateTemplates();
            }
            else if (((System.Windows.Forms.ToolStripItem)(sender)).Tag is ProjectNode)
            {
                mainForm.TemplateNode = "Projects";
                Dialogs.AddProjectTemplateDialog dialog = new Epi.Windows.MakeView.Dialogs.AddProjectTemplateDialog(mainForm);
                dialog.ShowDialog();

                if (dialog.DialogResult == DialogResult.OK)
                {
                    Template template = new Template(this.mainForm.mediator);
                    template.CreateProjectTemplate(dialog.TemplateName.ToString(), dialog.TemplateDescription.ToString());
                    this.mainForm.mediator.ProjectExplorer.UpdateTemplates();
                }
            }
            else if (((System.Windows.Forms.ToolStripItem)(sender)).Tag is ViewNode)
            {
                mainForm.TemplateNode = "Forms";
                Dialogs.AddTemplateDialog dialog = new Epi.Windows.MakeView.Dialogs.AddTemplateDialog(mainForm);
                dialog.ShowDialog();

                if (dialog.DialogResult == DialogResult.OK)
                {
                    Template template = new Template(this.mainForm.mediator);
                    template.CreateViewTemplate(dialog.TemplateName, ((Epi.Windows.Controls.ViewNode)(rightClickedNode)).View); 
                    this.mainForm.mediator.ProjectExplorer.UpdateTemplates();
                }
            }
            else if (((System.Windows.Forms.ToolStripItem)(sender)).Tag is PageNode)
            {
                mainForm.TemplateNode = "Pages";
                Dialogs.AddTemplateDialog dialog = new Epi.Windows.MakeView.Dialogs.AddTemplateDialog(mainForm);
                dialog.ShowDialog();

                if (dialog.DialogResult == DialogResult.OK)
                {
                    Template template = new Template(this.mainForm.mediator);
                    template.CreatePageTemplate(dialog.TemplateName.ToString());
                    this.mainForm.mediator.ProjectExplorer.UpdateTemplates();
                }
            }
        }

        void mnuShareViaEmail_Click(object sender, EventArgs e)
        {
            Template template = new Template(this.mainForm.mediator);
            string templatesFolderPath = "";
            string templateNameWithSubfolders = "";

            if (((System.Windows.Forms.ToolStripItem)(sender)).Tag is ProjectNode)
            {
                ProjectNode projectNode = (ProjectNode)((System.Windows.Forms.ToolStripItem)(sender)).Tag;
                templateNameWithSubfolders = projectNode.Text;
                mainForm.TemplateNode = "Projects";
                templatesFolderPath = Template.GetTemplatePath("Projects");
                template.CreateProjectTemplate
                (
                    templateNameWithSubfolders,
                    string.Format(@"Shared Via Email {0}", DateTime.Now.ToString("dddd MMMM dd yyyy hmmsstt"))
                );
            }
            else if (((System.Windows.Forms.ToolStripItem)(sender)).Tag is ViewNode)
            {
                ViewNode viewNode = (ViewNode)((System.Windows.Forms.ToolStripItem)(sender)).Tag;
                templateNameWithSubfolders = viewNode.Text;
                mainForm.TemplateNode = "Forms";
                templatesFolderPath = Template.GetTemplatePath("Forms");
                template.CreateViewTemplate
                    (
                        templateNameWithSubfolders,
                        ((Epi.Windows.Controls.ViewNode)(rightClickedNode)).View
                    );
            }
            else if (((System.Windows.Forms.ToolStripItem)(sender)).Tag is PageNode)
            {
                PageNode pageNode = (PageNode)((System.Windows.Forms.ToolStripItem)(sender)).Tag;
                templateNameWithSubfolders = pageNode.Text;
                mainForm.TemplateNode = "Pages";
                templatesFolderPath = Template.GetTemplatePath("Pages");
                template.CreatePageTemplate
                    (
                        templateNameWithSubfolders
                    );
            }

            string xmlFullPath = System.IO.Path.Combine(templatesFolderPath, templateNameWithSubfolders) + ".xml";

            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("ita3@cdc.gov");
            mailMessage.Subject = "Epi Info 7 Shared Template";
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = "<span style='font-size: 10pt; color: black; font-family: Segoe UI,sans-serif;'>Attached please find an Epi Info 7 template.</span>";

            mailMessage.Attachments.Add(new Attachment(xmlFullPath));

            string asmPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            var filename = System.IO.Path.Combine(asmPath, "epiInfoSharedViaEmail.eml") ;

            mailMessage.Save(filename);

            System.Diagnostics.Process.Start(filename);
        }

        /// <summary>
        /// Handles the Click event of the Check Code for a page
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuPageCheckCode_Click(object sender, EventArgs e)
        {
            Epi.Windows.MakeView.Forms.CheckCode checkCode = new Epi.Windows.MakeView.Forms.CheckCode(((PageNode)rightClickedNode).Page, (MakeViewMainForm)this.mainForm,CurrentView);
            checkCode.ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of the add view menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuAddView_Click(object sender, EventArgs e)
        {
            AddView();
        }

        /// <summary>
        /// Handles the Click event of the Add page menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuAddPage_Click(object sender, EventArgs e)
        {
            if (((ViewNode)(projectTree.SelectedNode)).View.Pages.Count < 150)
            {
                AddPage(rightClickedNode);
            }
            else
            {
                MessageBox.Show(SharedStrings.MAXIMUM_NUMBER_PAGES, SharedStrings.MAX_PAGES_REACHED, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the Click event of the Delete View menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuDeleteView_Click(object sender, EventArgs e)
        {
            string msg = SharedStrings.CONFIRM_VIEW_DELETE;
            DeleteView((ViewNode)rightClickedNode, msg);
        }

        /// <summary>
        /// Handles the Click event of the Delete Page menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuDeletePage_Click(object sender, EventArgs e)
        {
            string msg = SharedStrings.CONFIRM_PAGE_DELETE;
            DeletePage((PageNode)rightClickedNode, msg);
        }

        /// <summary>
        /// Handles the Click event of the Insert Page menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuInsertPage_Click(object sender, EventArgs e)
        {
            if (((ViewNode)(projectTree.SelectedNode.Parent)).View.Pages.Count < 150)
            {
                InsertPage(rightClickedNode);
            }
            else
            {
                MessageBox.Show(SharedStrings.MAXIMUM_NUMBER_PAGES, SharedStrings.MAX_PAGES_REACHED, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the Click event of the Rename Page menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event paramters</param>
        void mnuRenamePage_Click(object sender, EventArgs e)
        {
            RenamePage((PageNode)rightClickedNode);
        }

        #endregion  //Event Handlers

        #region Private Methods

        /// <summary>
        /// Adds open fields to the explorer
        /// </summary>
        private void AddOpenFields()
        {
            if (this.IsProjectLoaded == false || OpenForViewingOnly) return;
            
            this.SuspendLayout();
            this.fieldsNode = new TreeNode();

            fieldsNode.Name = "fieldsNode";
            //resources.ApplyResources(this.fieldsNode, "fieldsNode");
            fieldsNode.Text = SharedStrings.PE_FIELDS_NODE;
            fieldsNode.ImageIndex = 6;
            fieldsNode.SelectedImageIndex = 6;
            projectTree.Nodes.Add(fieldsNode);
            
            OpenFieldNode labelNode = new OpenFieldNode();
            labelNode.Text = SharedStrings.PE_LABEL_NODE;
            labelNode.ImageIndex = 8;
            labelNode.SelectedImageIndex = 8;
            labelNode.Value = MetaFieldType.LabelTitle;
            fieldsNode.Nodes.Add(labelNode);

            OpenFieldNode singleLineNode = new OpenFieldNode();
            singleLineNode.Text = SharedStrings.PE_SINGLE_LINE_NODE;
            singleLineNode.ImageIndex = 7;
            singleLineNode.SelectedImageIndex = 7;
            singleLineNode.Value = MetaFieldType.Text;
            fieldsNode.Nodes.Add(singleLineNode);

            OpenFieldNode uppercaseNode = new OpenFieldNode();
            uppercaseNode.Text = SharedStrings.PE_UPPER_CASE_NODE;
            uppercaseNode.ImageIndex = 9;
            uppercaseNode.SelectedImageIndex = 9;
            uppercaseNode.Value = MetaFieldType.TextUppercase;
            fieldsNode.Nodes.Add(uppercaseNode);

            OpenFieldNode multilineNode = new OpenFieldNode();
            multilineNode.Text = SharedStrings.PE_MULTI_LINE_NODE;
            multilineNode.ImageIndex = 7;
            multilineNode.SelectedImageIndex = 7;
            multilineNode.Value = MetaFieldType.Multiline;
            fieldsNode.Nodes.Add(multilineNode);

            //OpenFieldNode guidNode = new OpenFieldNode();
            //guidNode.Text = SharedStrings.PE_GUID_NODE;                
            //guidNode.ImageIndex = 7;
            //guidNode.SelectedImageIndex = 7;
            //guidNode.Value = MetaFieldType.GUID;
            //fieldsNode.Nodes.Add(guidNode);

            OpenFieldNode numberNode = new OpenFieldNode();
            numberNode.Text = SharedStrings.PE_NUMBER_NODE;
            numberNode.ImageIndex = 10;
            numberNode.SelectedImageIndex = 10;
            numberNode.Value = MetaFieldType.Number;
            fieldsNode.Nodes.Add(numberNode);

            OpenFieldNode phoneNode = new OpenFieldNode();
            phoneNode.Text = SharedStrings.PE_PHONE_NODE;
            phoneNode.ImageIndex = 10;
            phoneNode.SelectedImageIndex = 10;
            phoneNode.Value = MetaFieldType.PhoneNumber;
            fieldsNode.Nodes.Add(phoneNode);

            OpenFieldNode dateNode = new OpenFieldNode();
            dateNode.Text = SharedStrings.PE_DATE_NODE;
            dateNode.ImageIndex = 11;
            dateNode.SelectedImageIndex = 11;
            dateNode.Value = MetaFieldType.Date;
            fieldsNode.Nodes.Add(dateNode);

            OpenFieldNode timeNode = new OpenFieldNode();
            timeNode.Text = SharedStrings.PE_TIME_NODE;
            timeNode.ImageIndex = 11;
            timeNode.SelectedImageIndex = 11;
            timeNode.Value = MetaFieldType.Time;
            fieldsNode.Nodes.Add(timeNode);

            OpenFieldNode dateTimeNode = new OpenFieldNode();
            dateTimeNode.Text = SharedStrings.PE_DATETIME_NODE;
            dateTimeNode.ImageIndex = 11;
            dateTimeNode.SelectedImageIndex = 11;
            dateTimeNode.Value = MetaFieldType.DateTime;
            fieldsNode.Nodes.Add(dateTimeNode);

            OpenFieldNode checkboxNode = new OpenFieldNode();
            checkboxNode.Text = SharedStrings.PE_CHECKBOX_NODE;
            checkboxNode.ImageIndex = 12;
            checkboxNode.SelectedImageIndex = 12;
            checkboxNode.Value = MetaFieldType.Checkbox;
            fieldsNode.Nodes.Add(checkboxNode);

            OpenFieldNode yesNoNode = new OpenFieldNode();
            yesNoNode.Text = SharedStrings.PE_YESNO_NODE;
            yesNoNode.ImageIndex = 5;
            yesNoNode.SelectedImageIndex = 5;
            yesNoNode.Value = MetaFieldType.YesNo;
            fieldsNode.Nodes.Add(yesNoNode);

            OpenFieldNode optionNode = new OpenFieldNode();
            optionNode.Text = SharedStrings.PE_OPTION_NODE;
            optionNode.ImageIndex = 13;
            optionNode.SelectedImageIndex = 13;
            optionNode.Value = MetaFieldType.Option;
            fieldsNode.Nodes.Add(optionNode);

            OpenFieldNode commandButtonNode = new OpenFieldNode();
            commandButtonNode.Text = SharedStrings.PE_COMMAND_BUTTON_NODE;
            commandButtonNode.ImageIndex = 14;
            commandButtonNode.SelectedImageIndex = 14;
            commandButtonNode.Value = MetaFieldType.CommandButton;
            fieldsNode.Nodes.Add(commandButtonNode);

            OpenFieldNode imageNode = new OpenFieldNode();
            imageNode.Text = SharedStrings.PE_IMAGE_NODE;
            imageNode.ImageIndex = 15;
            imageNode.SelectedImageIndex = 15;
            imageNode.Value = MetaFieldType.Image;
            fieldsNode.Nodes.Add(imageNode);

            OpenFieldNode mirrorNode = new OpenFieldNode();
            mirrorNode.Text = SharedStrings.PE_MIRROR_NODE;
            mirrorNode.ImageIndex = 16;
            mirrorNode.SelectedImageIndex = 16;
            mirrorNode.Value = MetaFieldType.Mirror;
            fieldsNode.Nodes.Add(mirrorNode);

            // dpbrown - enable after 7.0.8.0
            OpenFieldNode gridNode = new OpenFieldNode();
            gridNode.Text = SharedStrings.PE_GRID_NODE;
            gridNode.ImageIndex = 17;
            gridNode.SelectedImageIndex = 17;
            gridNode.Value = MetaFieldType.Grid;
            fieldsNode.Nodes.Add(gridNode);

            OpenFieldNode legalValuesNode = new OpenFieldNode();
            legalValuesNode.Text = SharedStrings.PE_LEGAL_VALUE_NODE;
            legalValuesNode.ImageIndex = 5;
            legalValuesNode.SelectedImageIndex = 5;
            legalValuesNode.Value = MetaFieldType.LegalValues;
            fieldsNode.Nodes.Add(legalValuesNode);

            OpenFieldNode commentLegalNode = new OpenFieldNode();
            commentLegalNode.Text = SharedStrings.PE_COMMENT_LEGAL_NODE;
            commentLegalNode.ImageIndex = 5;
            commentLegalNode.SelectedImageIndex = 5;
            commentLegalNode.Value = MetaFieldType.CommentLegal;
            fieldsNode.Nodes.Add(commentLegalNode);

            OpenFieldNode codesNode = new OpenFieldNode();
            codesNode.Text = SharedStrings.PE_CODES_NODE;
            codesNode.ImageIndex = 5;
            codesNode.SelectedImageIndex = 5;
            codesNode.Value = MetaFieldType.Codes;
            fieldsNode.Nodes.Add(codesNode);

            OpenFieldNode relateNode = new OpenFieldNode();
            relateNode.Text = SharedStrings.PE_RELATE_NODE;
            relateNode.ImageIndex = 14;
            relateNode.SelectedImageIndex = 14;
            relateNode.Value = MetaFieldType.Relate;
            fieldsNode.Nodes.Add(relateNode);

            FieldGroupNode groupNode = new FieldGroupNode();
            groupNode.Text = SharedStrings.PE_GROUP_NODE;
            groupNode.ImageIndex = 18;
            groupNode.SelectedImageIndex = 18;
            groupNode.Value = MetaFieldType.Group;
            fieldsNode.Nodes.Add(groupNode);
            
            this.ResumeLayout();
            fieldsNode.ExpandAll();
        }

        /// <summary>
        /// Updates the template nodes in the project explorer
        /// </summary>
        public void UpdateTemplates(bool expandAllNodes = false)
        {
            if (projectTree.Nodes.ContainsKey("Templates"))
            {
                projectTree.Nodes["Templates"].Remove();
            }

            if (OpenForViewingOnly)
            {
                return;
            }
            
            TreeNode templateRootNode = new TreeNode();
            templateRootNode.Text = SharedStrings.PE_TEMPLATES;
            templateRootNode.Name = "Templates";
            templateRootNode.ImageIndex = 0;
            templateRootNode.SelectedImageIndex = 0;
            projectTree.Nodes.Add(templateRootNode);

            Configuration config = Configuration.GetNewInstance();
            string path = config.Directories.Templates;

            if (System.IO.Directory.Exists(path) == false)
            {
                System.IO.Directory.CreateDirectory(path);
            } 
            
            AddTemplateNodes(templateRootNode, path);

            if (expandAllNodes == true)
            {
                templateRootNode.ExpandAll();
            }
            else
            {
                templateRootNode.Expand();
            }
        }

        /// <summary>
        /// Recursive method that adds template nodes to the project explorer
        /// </summary>
        private void AddTemplateNodes(TreeNode parentNode, string path)
        {
            Configuration config = Configuration.GetNewInstance();
            string execPath = config.Directories.Templates;

            string[] templateNames = System.IO.Directory.GetFiles(path, "*.xml");
            string[] directoryNames = System.IO.Directory.GetDirectories(path);

            for (int i = 0; i < directoryNames.Length; i++)
            {
                string name = (string)directoryNames[i];
                name = name.Substring(name.LastIndexOf('\\'));
                name = name.TrimStart(new char[] { '\\' });

                if ((IsProjectLoaded && name != "SourceTables") || 
                    (IsProjectLoaded == false && name == "Projects") ||
                    (IsProjectLoaded == false && name == "Forms"))
                {
                    
                    TemplateNode templateNode = new TemplateNode();
                    templateNode.Text = Path.GetFileNameWithoutExtension(directoryNames[i]);
                    templateNode.ImageIndex = 0;
                    templateNode.Tag = directoryNames[i];
                    templateNode.SelectedImageIndex = 0;
                    templateNode.Value = MetaFieldType.Group;

                    parentNode.Nodes.Add(templateNode);
                    AddTemplateNodes(templateNode, System.IO.Path.Combine(path, directoryNames[i]));
                }
            }

            for (int i = 0; i < templateNames.Length; i++)
            {
                TemplateNode templateNode = new TemplateNode();
                templateNode.Text = System.IO.Path.GetFileNameWithoutExtension(templateNames[i]);
                templateNode.Tag = templateNames[i].Replace(execPath, "");
                templateNode.ImageIndex = 6;
                templateNode.SelectedImageIndex = 6;
                templateNode.Value = MetaFieldType.Group;

                bool hasProjectName = true;

                if (IsProjectLoaded == false && parentNode.Text == "Forms")
                {
                    using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(templateNames[i]))
                    {
                        try
                        {
                            while (reader.ReadToFollowing("Template"))
                            {
                                string nameCandidate = reader.GetAttribute("Name");

                                if (string.IsNullOrEmpty(nameCandidate))
                                {
                                    hasProjectName = false;
                                }
                            }
                        }
                        catch
                        {
                            Logger.Log("Project Explorer - The template file [ " + reader.BaseURI + " ] cannot be read.");
                        }
                    }
                }

                if (hasProjectName)
                {
                    parentNode.Nodes.Add(templateNode);
                }
            }
        }

        /// <summary>
        /// Deletes a page
        /// </summary>
        /// <param name="delNode">The page node</param>
        /// <param name="msg">Message to user to confirm deletion of page</param>
        private void DeletePage(PageNode delNode, string msg)
        {
            if (delNode.Parent.Nodes.Count != 1)
            {
                DialogResult result = MsgBox.Show(msg + " \n\n" + string.Format(SharedStrings.PAGE_NAME, delNode.Text), SharedStrings.MAKE_VIEW, MessageBoxButtons.YesNo);     //Epi.Windows.MsgBox.ShowQuestion(msg);
                if (result == DialogResult.Yes)
                {
                    mainForm.mediator.OnPageDeleteUpdateFieldQueue(delNode.Page);
                    
                    ViewNode viewNode = (ViewNode)delNode.Parent;
                    int currentNodeIndex = delNode.Index;
                    viewNode.View.DeletePage(delNode.Page);
                    projectTree.Nodes.Remove(delNode);

                    if (currentNodeIndex == 0)
                    {
                        projectTree.SelectedNode = viewNode.Nodes[0];
                    }
                    else
                    {
                        projectTree.SelectedNode = viewNode.Nodes[currentNodeIndex - 1];
                    }
                }
            }
            else
            {
                MsgBox.ShowInformation(SharedStrings.WARNING_CANNOT_DELETE_PRIMARY_PAGE);
            }
        }

        /// <summary>
        /// Deletes a view
        /// </summary>
        /// <param name="delNode">The view node</param>
        /// <param name="msg">Message to user to confirm deletion of view</param>
        private void DeleteView(ViewNode delNode, string msg)
        {
            ProjectNode prjNode = (ProjectNode)delNode.Parent;
            int currentNodeIndex = delNode.Index;
            PresentationLogic.GuiMediator mediator = ((MakeViewMainForm)Forms.MakeViewMainForm.ActiveForm).GetMediator();

            if (prjNode.Nodes.Count != 1)
            {
                DialogResult result = MessageBox.Show(msg + ": \n\n" + delNode.Text, SharedStrings.MAKE_VIEW, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    foreach (Page page in delNode.View.Pages)
                    {
                        foreach (Field field in page.Fields)
                        {
                            if (field is RelatedViewField)
                            {
                                MessageBox.Show(SharedStrings.WARNING_DELETE_PARENT_VIEW);
                                return;
                            }
                        }
                    }

                    foreach (View view in prjNode.Project.Views)
                    {
                        if (view.Id == delNode.View.Id)
                        {
                            delNode.View.IsRelatedView = view.IsRelatedView;
                        }
                    }

                    if (delNode.View.IsRelatedView)
                    {
                        DialogResult confirmDelete = MessageBox.Show(SharedStrings.WARNING_DELETE_RELATED_VIEW, SharedStrings.MAKE_VIEW, MessageBoxButtons.YesNo);
                        if (confirmDelete == DialogResult.Yes)
                        {
                            IMetadataProvider metadata = delNode.View.GetMetadata();
                            View parentView = delNode.View.ParentView = metadata.GetParentView(delNode.View.Id);

                            if (!Util.IsEmpty(parentView))
                            {
                                List<Field> relatedFields = new List<Field>();

                                foreach (RelatedViewField field in parentView.Fields.RelatedFields)
                                {
                                    if (field.ChildView != null)
                                    {
                                        if (field.ChildView.Id == delNode.View.Id)
                                        {
                                            relatedFields.Add(field);
                                        }
                                    }
                                }

                                foreach (Field field in relatedFields)
                                {
                                    mediator.Canvas.RemoveControls(field.Name);
                                    field.Delete();
                                }

                                parentView.MustRefreshFieldCollection = true;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }

                    foreach (Page page in delNode.View.Pages)
                    {
                        delNode.View.DeletePage(page);
                    }
                    
                    prjNode.Project.Metadata.DeleteView(delNode.View.Name);
                    prjNode.Project.Views.Remove(delNode.View.Name);
                    projectTree.Nodes.Remove(delNode);

                    Reset();
                    LoadProject(mediator.Project);
                }
            }
            else
            {
                MsgBox.ShowInformation(SharedStrings.WARNING_CANNOT_DELETE_PRIMARY_VIEW);
            }
        }

        /// <summary>
        /// Adds linked mode fields to the explorer
        /// </summary>
        private void AddPhinVocabulary()
        {
            try
            {
                PHINVSProvider dataManager = PHINVSProvider.Instance;
                LinkedRootNode linkedNode = new LinkedRootNode();
                linkedNode.Text = SharedStrings.VOCABULARY_FIELDS; //"Vocabulary Fields";
                linkedNode.ImageIndex = 3;
                linkedNode.SelectedImageIndex = 3;
                linkedNode.Tag = "Vocabulary Fields";
                projectTree.Nodes.Add(linkedNode);

                DataTable domains = dataManager.GetDomains();
                foreach (DataRow row in domains.Rows)
                {
                    TreeValueNode domainNode = new TreeValueNode();
                    domainNode.Text = row["Name"].ToString();
                    domainNode.Value = row["Code"].ToString();
                    domainNode.ImageIndex = 4;
                    domainNode.SelectedImageIndex = 4;
                    linkedNode.Nodes.Add(domainNode);
                    DataTable valueSets = dataManager.GetValueSets(row["Code"].ToString());
                    foreach (DataRow valueSetRow in valueSets.Rows)
                    {
                        LinkedFieldNode valueSetNode = new LinkedFieldNode();
                        valueSetNode.Text = valueSetRow["Name"].ToString();
                        valueSetNode.Value = valueSetRow["Code"].ToString();
                        valueSetNode.ImageIndex = 5;
                        valueSetNode.SelectedImageIndex = 5;
                        domainNode.Nodes.Add(valueSetNode);
                    }
                }
            }
            //catch (Exception)
            //{
            //    // Ignore it for now. If PHIN VS database is not available, Linked mode will be ignored.
            //}
            finally { }
        }

        public void SetIsLinkedFieldVisibile(bool pValue)
        {
            bool HasLinkedNode = false;
            int NodeIndex = -1;
            for (int i = 0; i < projectTree.Nodes.Count; i++)
            {
                if (projectTree.Nodes[i].Tag == "Vocabulary Fields")
                {
                    NodeIndex = i;
                    HasLinkedNode = true;
                    break;
                }
            }

            if (pValue)
            {

                if (!HasLinkedNode)
                {
                    AddPhinVocabulary();
                }
            }
            else
            {
                if (HasLinkedNode)
                {
                    TreeNode node = projectTree.Nodes[NodeIndex];
                    projectTree.Nodes.Remove(node);
                }
            }
        }

        /// <summary>
        /// Builds linked mode context menu
        /// </summary>
        /// <returns>Context menu</returns>
        private ContextMenuStrip BuildLinkedModeContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.ImageList = imageList1;
            ToolStripMenuItem mnuNewLinkedConnection = new ToolStripMenuItem("New &Connection");
            mnuNewLinkedConnection.ImageIndex = 23;
            mnuNewLinkedConnection.Click += new EventHandler(mnuNewLinkedConnection_Click);
            contextMenu.Items.Add(mnuNewLinkedConnection);
            return contextMenu;
        }

        /// <summary>
        /// Builds project context menu
        /// </summary>
        /// <param name="node">Tree Node to add context menu to</param>
        /// <returns>Context menu for a project</returns>
        private ContextMenuStrip BuildProjectContextMenu(TreeNode node)
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.ImageList = imageList1;
            ToolStripMenuItem mnuAddView = new ToolStripMenuItem(SharedStrings.ADD_FORM);
            mnuAddView.ImageIndex = 19;
            mnuAddView.Click += new EventHandler(mnuAddView_Click);
            contextMenu.Items.Add(mnuAddView);

            ///---
            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem mnuProjectUseDefaultFonts = new ToolStripMenuItem(SharedStrings.APPLY_DEFAULT_FONTS_PROJECT);
            mnuProjectUseDefaultFonts.ImageIndex = 22;
            mnuProjectUseDefaultFonts.Tag = node;
            mnuProjectUseDefaultFonts.Click += new EventHandler(mnuProjectUseDefaultFonts_Click);
            contextMenu.Items.Add(mnuProjectUseDefaultFonts);

            ///---
            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem mnuProjectSaveAsTemplate = new ToolStripMenuItem(SharedStrings.SAVE_PROJECT_AS_TEMPLATE);
            mnuProjectSaveAsTemplate.ImageIndex = 24;
            mnuProjectSaveAsTemplate.Tag = node;
            mnuProjectSaveAsTemplate.Click += new EventHandler(mnuSaveAsTemplate_Click);
            contextMenu.Items.Add(mnuProjectSaveAsTemplate);
            
            
            ToolStripMenuItem mnuQuickSaveProjectAsTemplate = new ToolStripMenuItem(SharedStrings.QUICK_SAVE_PROJECT_AS_TEMPLATE);
            mnuQuickSaveProjectAsTemplate.ImageIndex = 24;
            mnuQuickSaveProjectAsTemplate.Tag = node;
            mnuQuickSaveProjectAsTemplate.Click += new EventHandler(mnuSaveAsTemplate_Click);
            contextMenu.Items.Add(mnuQuickSaveProjectAsTemplate);

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem mnuShareViaEmail = new ToolStripMenuItem(SharedStrings.SHARE_PROJECT_VIA_EMAIL);
            mnuShareViaEmail.ImageIndex = 25;
            mnuShareViaEmail.Tag = node;
            mnuShareViaEmail.Click += new EventHandler(mnuShareViaEmail_Click);
            contextMenu.Items.Add(mnuShareViaEmail);

            return contextMenu;
        }

        /// <summary>
        /// Builds view context menu
        /// </summary>
        /// <param name="node">Context menu for a view</param>
        /// <returns>Context menu for a view</returns>
        private ContextMenuStrip BuildViewContextMenu(TreeNode node)
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.ImageList = imageList1;
            ToolStripMenuItem mnuAddPage = new ToolStripMenuItem(SharedStrings.ADD_PAGE);
            mnuAddPage.ImageIndex = 20;
            mnuAddPage.Click += new EventHandler(mnuAddPage_Click);
            contextMenu.Items.Add(mnuAddPage);

            ToolStripMenuItem mnuDeleteView = new ToolStripMenuItem(SharedStrings.DELETE_FORM);
            mnuDeleteView.ImageIndex = 21;
            mnuDeleteView.Click += new EventHandler(mnuDeleteView_Click);
            mnuDeleteView.Enabled = true;
            contextMenu.Items.Add(mnuDeleteView);
            
            ToolStripMenuItem mnuViewCheckCode = new ToolStripMenuItem(SharedStrings.VIEW_CHECK_CODE);
            mnuViewCheckCode.ImageIndex = 22;
            mnuViewCheckCode.Click += new EventHandler(mnuViewCheckCode_Click);
            contextMenu.Items.Add(mnuViewCheckCode);

            ///---
            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem mnuViewUseDefaultFonts = new ToolStripMenuItem(SharedStrings.APPLY_DEFAULT_FONTS_FORM);
            mnuViewUseDefaultFonts.ImageIndex = 22;
            mnuViewUseDefaultFonts.Tag = node;
            mnuViewUseDefaultFonts.Click += new EventHandler(mnuViewUseDefaultFonts_Click);
            contextMenu.Items.Add(mnuViewUseDefaultFonts);

            ///---
            contextMenu.Items.Add(new ToolStripSeparator());
            
            ToolStripMenuItem mnuViewSaveAsTemplate = new ToolStripMenuItem(SharedStrings.SAVE_FORM_AS_TEMPLATE);
            mnuViewSaveAsTemplate.ImageIndex = 24;
            mnuViewSaveAsTemplate.Tag = node;
            mnuViewSaveAsTemplate.Click += new EventHandler(mnuSaveAsTemplate_Click);
            contextMenu.Items.Add(mnuViewSaveAsTemplate);

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem mnuShareViaEmail = new ToolStripMenuItem(SharedStrings.SHARE_FORM_VIA_EMAIL);
            mnuShareViaEmail.ImageIndex = 25;
            mnuShareViaEmail.Tag = node;
            mnuShareViaEmail.Click += new EventHandler(mnuShareViaEmail_Click);
            contextMenu.Items.Add(mnuShareViaEmail);

            return contextMenu;
        }

        /// <summary>
        /// Builds page context menu
        /// </summary>
        /// <param name="node">Tree node to add context menu to</param>
        /// <returns>Context menu for a page</returns>
        private ContextMenuStrip BuildPageContextMenu(TreeNode node)
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.ImageList = imageList1;
            
            ToolStripMenuItem mnuDeletePage = new ToolStripMenuItem(SharedStrings.DELETE_PAGE); // TODO: Hard coded string
            mnuDeletePage.ImageIndex = 21;
            mnuDeletePage.Click += new EventHandler(mnuDeletePage_Click);
            contextMenu.Items.Add(mnuDeletePage);
            
            ToolStripMenuItem mnuInsertPage = new ToolStripMenuItem(SharedStrings.INSERT_PAGE);// TODO: Hard coded string
            mnuInsertPage.ImageIndex = 20;
            mnuInsertPage.Click += new EventHandler(mnuInsertPage_Click);
            contextMenu.Items.Add(mnuInsertPage);
            
            ToolStripMenuItem mnuRenamePage = new ToolStripMenuItem(SharedStrings.RENAME_PAGE);// TODO: Hard coded string
            mnuRenamePage.Click += new EventHandler(mnuRenamePage_Click);
            contextMenu.Items.Add(mnuRenamePage);
            
            ToolStripMenuItem mnuPageCheckCode = new ToolStripMenuItem(SharedStrings.PAGE_CHECK_CODE);// TODO: Hard coded string
            mnuPageCheckCode.ImageIndex = 22;
            mnuPageCheckCode.Click += new EventHandler(mnuPageCheckCode_Click);
            contextMenu.Items.Add(mnuPageCheckCode);

            ///---
            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem mnuPageUseDefaultFonts = new ToolStripMenuItem(SharedStrings.APPLY_DEFAULT_FONTS_PAGE);
            mnuPageUseDefaultFonts.ImageIndex = 22;
            mnuPageUseDefaultFonts.Tag = node;
            mnuPageUseDefaultFonts.Click += new EventHandler(mnuPageUseDefaultFonts_Click);
            contextMenu.Items.Add(mnuPageUseDefaultFonts);

            ///---
            contextMenu.Items.Add(new ToolStripSeparator());
            
            ToolStripMenuItem mnuPageSaveAsTemplate = new ToolStripMenuItem(SharedStrings.SAVE_PAGE_AS_TEMPLATE);
            mnuPageSaveAsTemplate.ImageIndex = 24;
            mnuPageSaveAsTemplate.Tag = node;
            mnuPageSaveAsTemplate.Click += new EventHandler(mnuSaveAsTemplate_Click);
            contextMenu.Items.Add(mnuPageSaveAsTemplate);

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem mnuShareViaEmail = new ToolStripMenuItem(SharedStrings.SHARE_PAGE_VIA_EMAIL);
            mnuShareViaEmail.ImageIndex = 25;
            mnuShareViaEmail.Tag = node;
            mnuShareViaEmail.Click += new EventHandler(mnuShareViaEmail_Click);
            contextMenu.Items.Add(mnuShareViaEmail);

            return contextMenu;
        }

        private ContextMenuStrip BuildTemplateContextMenu(TreeNode node)
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.ImageList = imageList1;

            bool isTemplateParentFolder = node.Parent.Tag != null &&
                (((string)node.Parent.Tag).EndsWith(SharedStrings.PROJECTS)
                || ((string)node.Parent.Tag).EndsWith(SharedStrings.FORMS)
                || ((string)node.Parent.Tag).EndsWith(SharedStrings.PAGES)
                || ((string)node.Parent.Tag).EndsWith(SharedStrings.FIELDS));

            if (node.Parent.Tag is string && isTemplateParentFolder)
            {
                if (((string)node.Parent.Tag).EndsWith(SharedStrings.PROJECTS))
                { 
                    ToolStripMenuItem mnuOpen = new ToolStripMenuItem(SharedStrings.OPEN);
                    mnuOpen.Tag = node;
                    mnuOpen.Click += new EventHandler(mnuOpen_Click);
                    contextMenu.Items.Add(mnuOpen);
                    contextMenu.Items.Add(new ToolStripSeparator());
                }

                ToolStripMenuItem mnuRename = new ToolStripMenuItem(SharedStrings.RENAME);
                mnuRename.Tag = node;
                mnuRename.Click += new EventHandler(mnuRename_Click);
                contextMenu.Items.Add(mnuRename);

                ToolStripMenuItem mnuDelete = new ToolStripMenuItem(SharedStrings.DELETE);
                mnuDelete.Tag = node;
                mnuDelete.Click += new EventHandler(mnuDelete_Click);
                contextMenu.Items.Add(mnuDelete);

                ToolStripMenuItem mnuCopy = new ToolStripMenuItem(SharedStrings.COPY);
                mnuCopy.Tag = node;
                mnuCopy.Click += new EventHandler(mnuCopy_Click);
                contextMenu.Items.Add(mnuCopy);

                ToolStripMenuItem mnuPaste = new ToolStripMenuItem(SharedStrings.PASTE);
                mnuPaste.Tag = node;
                mnuPaste.Click += new EventHandler(mnuPaste_Click);
                mnuPaste.Enabled = string.IsNullOrEmpty(_nameOfCopiedTemplate) ? false : true;
                contextMenu.Items.Add(mnuPaste);

                //ToolStripMenuItem mnuProperties = new ToolStripMenuItem(SharedStrings.PROPERTIES);
                //mnuProperties.Tag = node;
                //mnuProperties.Click += new EventHandler(mnuProperties_Click);
                //contextMenu.Items.Add(mnuProperties);

                contextMenu.Items.Add(new ToolStripSeparator());
            }

            ToolStripMenuItem mnuOpenFolder = new ToolStripMenuItem(SharedStrings.OPEN_CONTAINING_FOLDER);
            mnuOpenFolder.Tag = node;
            mnuOpenFolder.Click += new EventHandler(mnuOpenFolder_Click);
            contextMenu.Items.Add(mnuOpenFolder);

            return contextMenu;
        }

        string GetTemplateFullPath(object sender)
        {
            TemplateNode node = ((TemplateNode)((ToolStripMenuItem)sender).Tag);
            string pathFromTag = node.Tag.ToString();
            Configuration config = Configuration.GetNewInstance();
            string templateDirectoryPath = config.Directories.Templates;
            string fullPath = Path.Combine(templateDirectoryPath, pathFromTag);
            return fullPath;
        }

        void mnuOpen_Click(object sender, EventArgs e)
        {
            TemplateNode node = ((TemplateNode)((ToolStripMenuItem)sender).Tag);
            string path = node.FullPath; 
            CreateNewProjectFromTemplate(path); 
        }

        void mnuRename_Click(object sender, EventArgs e)
        {
            string fullPath = GetTemplateFullPath(sender);
            string copiedFolderName = string.Empty;
            string copiedFileName = string.Empty;
            string copiedExtension = string.Empty;

            string fileNameCandidate = string.Empty;
            string fullPathCanditate = string.Empty;

            if (File.Exists(fullPath))
            {
                copiedFolderName = Path.GetDirectoryName(fullPath);
                copiedFileName = Path.GetFileNameWithoutExtension(fullPath);
                copiedExtension = Path.GetExtension(fullPath);
            }

            Dialogs.RenameTemplate dialog = new Epi.Windows.MakeView.Dialogs.RenameTemplate(mainForm, copiedFolderName, copiedExtension);
            dialog.ShowDialog();

            if (dialog.DialogResult == DialogResult.OK)
            {
                fullPathCanditate = Path.Combine(copiedFolderName, dialog.TemplateName) + copiedExtension; 

                File.Copy(fullPath, fullPathCanditate);
                File.Delete(fullPath);
                UpdateTemplates();
            }
        }

        void mnuDelete_Click(object sender, EventArgs e)
        {
            string fullPath = GetTemplateFullPath(sender);
            DialogResult continueWithDelete = MessageBox.Show(SharedStrings.DELETE_TEMPLATE, SharedStrings.DELETE_TEMPLATE, MessageBoxButtons.YesNoCancel);
            if (continueWithDelete == DialogResult.Yes)
            { 
                File.Delete(fullPath);
                UpdateTemplates();
            }
        }
        
        void mnuCopy_Click(object sender, EventArgs e)
        {
            _nameOfCopiedTemplate = GetTemplateFullPath(sender);
        }
        
        void mnuPaste_Click(object sender, EventArgs e)
        {
            string copyText = " - Copy";
            string copyTextInteration = " - Copy({0})";

            string copiedFolderName = string.Empty;
            string copiedFileName = string.Empty;
            string copiedExtension = string.Empty;

            string fileNameCandidate = string.Empty;
            string fullPathCanditate = string.Empty;

            if (File.Exists(_nameOfCopiedTemplate))
            {
                copiedFolderName = Path.GetDirectoryName(_nameOfCopiedTemplate);
                copiedFileName = Path.GetFileNameWithoutExtension(_nameOfCopiedTemplate);
                copiedExtension = Path.GetExtension(_nameOfCopiedTemplate);
            }

            fullPathCanditate = Path.Combine(copiedFolderName, copiedFileName);
            fullPathCanditate = fullPathCanditate + copyText + copiedExtension;

            int index = 2;

            while (File.Exists(fullPathCanditate))
            {
                fullPathCanditate = Path.Combine(copiedFolderName, copiedFileName);
                fullPathCanditate = fullPathCanditate + string.Format(copyTextInteration, index) + copiedExtension;
                index++;
            }

            if (File.Exists(_nameOfCopiedTemplate))
            {
                File.Copy(_nameOfCopiedTemplate, fullPathCanditate);
            }
        }

        void mnuOpenFolder_Click(object sender, EventArgs e)
        {
            TemplateNode node = ((TemplateNode)((ToolStripMenuItem)sender).Tag);

            if (node.Tag is string)
            {
                string templateFilePath = Template.GetTemplatePath(node);
                if (File.Exists(templateFilePath))
                {
                    string containingFolder = Path.GetDirectoryName(templateFilePath);
                    System.Diagnostics.Process.Start(containingFolder);
                }
                else if (Directory.Exists(templateFilePath))
                {
                    System.Diagnostics.Process.Start(templateFilePath);
                }
            }
        }

        void mnuProperties_Click(object sender, EventArgs e)
        {
            TemplateNode node = ((TemplateNode)((ToolStripMenuItem)sender).Tag);
            string templatesFolderPath = Template.GetTemplatePath(node);
            System.Diagnostics.Process.Start(templatesFolderPath); 
        }

        /// <summary>
        /// Creates a view
        /// </summary>
        /// <returns>View that was created</returns>
        private View PromptUserToCreateView()
        {
            View newView = null;
            Project project = ((ProjectNode)projectTree.Nodes[0]).Project;
            CreateViewDialog viewDialog = new CreateViewDialog(MainForm, project);
            viewDialog.ShowDialog();
            if (viewDialog.DialogResult == DialogResult.OK)
            {
                string newViewName = viewDialog.ViewName;
                newView = project.CreateView(newViewName);
                viewDialog.Close();
            }
            return newView;
        }

        /// <summary>
        /// Renames a page in a view
        /// </summary>
        /// <param name="node">Page node of the page to be renamed</param>
        private void RenamePage(PageNode node)
        {
            RenamePageDialog dialog = new RenamePageDialog(this.MainForm, node);
            dialog.PageName = node.Text;
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string strOldPageName = node.Text;
                node.Text = dialog.PageName;
                node.Page.Name = dialog.PageName;
                node.Page.SaveToDb();
                //--EI-52
                UpdatePageNameinCheckCode(strOldPageName, dialog.PageName);
                //--
                ViewNode viewNode = (ViewNode)node.Parent;
                Page foundPage = viewNode.View.Pages.Find(delegate(Page p) { return p.Id == node.Page.Id; });
                foundPage.Name = dialog.PageName;
                PageSelected(SelectedPage);
            }
        }
        // EI-52 Updates checkcode with changed pagename
        private void UpdatePageNameinCheckCode(string strOldPage, string strNewPage)
        {
            string strCheckcode = CurrentView.CheckCode;
            string strReplacedCheckcode = strCheckcode.Replace(strOldPage, strNewPage);
            CurrentView.CheckCode = strReplacedCheckcode;
            CurrentView.SaveToDb();
        }
        #endregion  //Private Methods

        #region Public Properties

        /// <summary>
        /// Gets the currently selected page
        /// </summary>
        public Page SelectedPage
        {
            get
            {
                if (projectTree.SelectedNode != null && projectTree.SelectedNode is PageNode)
                {
                    return ((PageNode)projectTree.SelectedNode).Page;
                }
                else
                {
                    if (projectTree.SelectedNode is ViewNode)
                    {
                        return ((PageNode)(projectTree.Nodes[0].Nodes[projectTree.SelectedNode.Index].Nodes[0])).Page;
                    }
                    else
                    {
                        return currentPage;
                    }
                }
            }
        }

        public IEnterInterpreter EpiInterpreter
        {
            get { return this.mainForm.EpiInterpreter; }
        }


        public View CurrentView
        {
            get {

                if (projectTree.SelectedNode is ViewNode)
                {
                    return ((ViewNode)projectTree.SelectedNode).View;
                }
                else if (projectTree.SelectedNode is PageNode)
                {
                    return ((ViewNode)projectTree.SelectedNode.Parent).View;
                }
                else if(projectTree.SelectedNode is ProjectNode)
                {
                    return this.mainForm.CurrentView;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion  //Public Properties

        #region Public Methods
        #endregion  //Public Methods
    }
}
