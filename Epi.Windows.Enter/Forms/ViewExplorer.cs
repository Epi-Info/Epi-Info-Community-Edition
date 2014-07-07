#region Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Fields;
using Epi.Windows;
using Epi.Windows.Controls;
using Epi.Windows.Enter.PresentationLogic;
using Epi.EnterCheckCodeEngine;

#endregion  //Namespaces

namespace Epi.Windows.Enter
{
    #region Public Delegates

    /// <summary>
    /// Delegate for page selection
    /// </summary>
    /// <param name="page">The page that was selected</param>
    public delegate void ViewExplorerPageSelectedEventHandler(Page page);

    /// <summary>
    /// Delegate for first record selection
    /// </summary>
    public delegate void ViewExplorerGoToFirstRecordSelectedEventHandler();

    /// <summary>
    /// Delegate for previous record selection
    /// </summary>
    public delegate void ViewExplorerGoToPreviousRecordSelectedEventHandler();
    
    /// <summary>
    /// Delegate for next record selection
    /// </summary>
    public delegate void ViewExplorerGoToNextRecordSelectedEventHandler();
    
    /// <summary>
    /// Delegate for last record selection
    /// </summary>
    public delegate void ViewExplorerGoToLastRecordSelectedEventHandler();

    /// <summary>
    /// Delegate to load a specific record
    /// </summary>
    /// <param name="recordId">The record Id of the record to be loaded</param>
    public delegate void ViewExplorerLoadRecordSpecifiedEventHandler(string recordId);

    /// <summary>
    /// Delegate to run check code
    /// </summary>
    /// <param name="checkCode">Enter module check code.</param>
    public delegate void ViewExplorerRunCheckCodeEventHandler(string checkCode);

    #endregion  //Public Delegates

    /// <summary>
    /// Enter's view explorer
    /// </summary>
    public partial class ViewExplorer : Epi.Windows.Docking.DockWindow
    {

        public event GuiMediator.OpenPageEventHandler OpenPageEvent;
        public event GuiMediator.ClosePageEventHandler ClosePageEvent;
        public event GuiMediator.GotoRecordEventHandler GotoRecordEvent;

        #region Private Members
        private Epi.Page currentPage = null;
        private RunTimeView view = null;
        #endregion



        #region Public Events

        /// <summary>
        /// Declaration of Load Record Specified Event Handler
        /// </summary>
        public event ViewExplorerLoadRecordSpecifiedEventHandler GoToSpecifiedRecord;

        #endregion      

        #region Constructors

        /// <summary>
        /// Constructor for View Explorer
        /// </summary>
        public ViewExplorer(MainForm frm) : base(frm)
        {
            InitializeComponent();            
        }
        #endregion  //Constructors

        #region Private Methods  
        #endregion  Private Methods

        #region Public Methods

        /// <summary>
        /// Resets the view explorer to its original state
        /// </summary>
        public void Reset()
        {
            if (viewTree.Nodes.Count > 0)
            {
                viewTree.Nodes.Clear();
            }
        }

        /// <summary>
        /// Attach the view
        /// </summary>
        /// <param name="view">The view to be loaded</param>
        public void LoadView(RunTimeView view)
        {
            #region Input Validation
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            #endregion Input validation          

            Epi.Windows.Controls.ViewNode viewNode = new Epi.Windows.Controls.ViewNode(view.View);
            viewNode.ImageIndex = 76;
            viewNode.SelectedImageIndex = 76;
            viewTree.Nodes.Add(viewNode);

            foreach (PageNode page in viewTree.Nodes[0].Nodes)
            {
                page.ImageIndex = 17;
                page.SelectedImageIndex = 17;
            }

            viewTree.ExpandAll();
            this.view = view;
        }

        /// <summary>
        /// Attach the view
        /// </summary>
        /// <param name="pView">The view to be loaded</param>
        /// <param name="pPage">The Page to be loaded</param>
        public void LoadView(RunTimeView pView, Page pPage)
        {
            #region Input Validation
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            #endregion Input validation

            Epi.Windows.Controls.ViewNode viewNode = new Epi.Windows.Controls.ViewNode(pView.View);
            viewTree.Nodes.Clear();
            viewNode.ImageIndex = 76;
            viewNode.SelectedImageIndex = 76;
            viewTree.Nodes.Add(viewNode);

            foreach (PageNode page in viewTree.Nodes[0].Nodes)
            {
                page.ImageIndex = 17;
                page.SelectedImageIndex = 17;
            }

            viewTree.ExpandAll();
            this.view = pView;
            
            
            for (int i = 0; i < viewTree.Nodes[0].Nodes.Count; i++)
            {
                PageNode page = (PageNode) viewTree.Nodes[0].Nodes[i];
                if (page.Text == pPage.Name)
                {
                    viewTree.SelectedNode = viewTree.Nodes[0].Nodes[i];
                    break;
                }
            }/**/
        }

        /// <summary>
        /// Selects a view based on view name
        /// </summary>
        /// <param name="viewName">The name of the view</param>
        public void SelectView(string viewName)
        {
            foreach (TreeNode node in viewTree.Nodes[0].Nodes)
            {
                if (node is ViewNode)
                {
                    if (((ViewNode)node).View.Name.Equals(viewName))
                    {
                        if (node.Nodes.Count > 0)
                        {
                            viewTree.CollapseAll();
                            viewTree.SelectedNode = node.Nodes[0];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Obtains the rec status
        /// </summary>
        /// <param name="recStatus">The record's status</param>
        /// <returns>The boolean form of the record status</returns>
        public bool GetRecStatus(int recStatus)
        {
            #region Input Validation
            if (recStatus < 0)
            {
                throw new ArgumentOutOfRangeException("Record Status");
            }
            #endregion  //Input Validation

            if (recStatus == 0)
            {
                return (false);
            }
            else
            {
                return (true);
            }
        }

        /// <summary>
        /// Attach the next page of the view
        /// </summary>
        public void GoToNextPage()
        {            
            if (viewTree.SelectedNode != viewTree.Nodes[0].LastNode)
            {
                viewTree.SelectedNode = viewTree.Nodes[0].Nodes[viewTree.SelectedNode.Index + 1];
            }            
        }

        /// <summary>
        /// Navigate to the first page of the view
        /// </summary>
        public void GoToFirstPage()
        {
            if (viewTree.Nodes.Count > 0)
            {
                if (viewTree.SelectedNode != viewTree.Nodes[0].Nodes[0])
                {
                    viewTree.SelectedNode = viewTree.Nodes[0].Nodes[0];
                }
            }
        }

        /// <summary>
        /// Navigate to the previous page of the view
        /// </summary>
        public void GoToPreviousPage()
        {
            if (viewTree.SelectedNode != viewTree.Nodes[0].Nodes[0])
            {
                viewTree.SelectedNode = viewTree.Nodes[0].Nodes[viewTree.SelectedNode.Index - 1];
            }
        }

        /// <summary>
        /// Navigate to a specific page of the view
        /// </summary>
        public void GoToSpecificPage(string pagePosition)
        {
            foreach (PageNode pageNode in viewTree.Nodes[0].Nodes)
            {
                if (pageNode.Page.Position == int.Parse(pagePosition))
                {
                    viewTree.SelectedNode = pageNode;
                }
            }
        }

        public void Render()
        {
            if (this.currentPage != null)
            {
                PageNode SelectedNode = null;
                if (this.viewTree.SelectedNode is PageNode)
                {
                    SelectedNode = (PageNode)this.viewTree.SelectedNode;
                }

                foreach (PageNode TestNode in this.viewTree.Nodes[0].Nodes)
                {
                    if (TestNode.Page == this.currentPage)
                    {
                        if (TestNode != SelectedNode)
                        {
                            this.viewTree.SelectedNode = TestNode;
                        }
                    }
                }
            }
        }

        #endregion  //Public Methods

        #region Private Events

        /// <summary>
        /// Handles the After Selection event of the tree view
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">Tree View supplied event parameters</param>
        private void viewTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node is PageNode)
            {
                if (this.currentPage != ((PageNode)e.Node).Page)
                {
                    if (OpenPageEvent != null)
                    {
                        Page selectedPage = ((PageNode)e.Node).Page;
                        
                        if (this.currentPage.view.Name == ((PageNode)e.Node).Page.view.Name)
                        {
                            int pageId = ((PageNode)e.Node).Page.Id;

                            foreach (Page page in currentPage.view.Pages)
                            {
                                if (page.Id == pageId)
                                {
                                    selectedPage = page;
                                    break;
                                }
                            }
                        }

                        OpenPageEvent(this, new PageSelectedEventArgs(selectedPage));
                    }
                }
            }
        }
      
        #endregion  //Private Events

        #region Public Properties

        /// <summary>
        /// Gets the currently selected page
        /// </summary>
        public Page SelectedPage
        {
            get
            {
                if (viewTree.SelectedNode != null && viewTree.SelectedNode is PageNode)
                {
                    return ((PageNode)viewTree.SelectedNode).Page;
                }
                else
                {
                    return null;
                }
            }
        }

        public RunTimeView View
        {
            get { return this.view; }
            set { this.view = value; } 
        }

        public Epi.Page CurrentPage
        {
            set { this.currentPage = value; }
        }

        #endregion  //Public Properties       

    }

    }
