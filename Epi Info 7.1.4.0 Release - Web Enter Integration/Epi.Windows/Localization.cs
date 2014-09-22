//using System;
//using System.Drawing;
//using System.Collections;
//using System.ComponentModel;
//using System.Windows.Forms;
//using System.Data;
//using Epi;
//using Epi.Data;

//using Epi.Windows.Controls;

//namespace Epi.Windows
//{
//    /// <summary>
//    /// Class that manages localization of the user interface
//    /// </summary>
//    public sealed class Localization : Epi.Localization
//    {
//        #region Private Methods

//        /// <summary>
//        /// Localize a tree view control
//        /// </summary>
//        /// <param name="control">The control to translate</param>
//        /// <param name="reverse">Whether or not to revert to the base language</param>
//        private static void Localize(TreeView treeView)
//        {
//            #region Input Validation
//            if (treeView == null)
//            {
//                throw new ArgumentNullException("treeView");
//            }
//            #endregion Input Validation
            
//            foreach (TreeNode treeNode in treeView.Nodes)
//            {
//                Localize(treeNode);
//            }
//        }

//        private static DataTable supportedLanguages = null;

//        /// <summary>
//        /// Gets a DataTable containing the list of installed languages.
//        /// </summary>
//        public static DataTable SupportedLanguages
//        {
//            get
//            {
//                if (supportedLanguages == null)
//                {
//                    IDbDriver db = DatabaseFactory.GetConfiguredDatabase(DatabaseFactory.KnownDatabaseNames.Translation);
//                    supportedLanguages = GetSupportedLanguages(db);
//                }
//                return supportedLanguages;
//            }
//        }



//        /// <summary>
//        /// Localize each node in a tree view
//        /// </summary>
//        /// <param name="treeNode">The tree node to translate</param>
//        /// <param name="reverse">Whether or not to revert to the base language</param>
//        private static void Localize(TreeNode treeNode)
//        {
//            #region Input Validation
//            if (treeNode == null)
//            {
//                throw new ArgumentNullException("treeNode");
//            }
//            #endregion Input Validation

//            treeNode.Text = Translate(treeNode.Text);
//            foreach (TreeNode tn in treeNode.Nodes)
//            {
//                Localize(tn);
//            }
//        }

//        /// <summary>
//        /// Translates the text of all the controls in a control collection
//        /// </summary>
//        /// <param name="controls">A control collection</param>
//        /// <param name="reverse">Whether or not to revert to the base language</param>
//        private static void Localize(Control.ControlCollection controls)
//        {
//            #region Input validation
//            if (controls == null)
//            {
//                throw new ArgumentNullException("controls");
//            }
//            #endregion  Input validation block

//            foreach (Control control in controls)
//            {
//                Localize(control);                
//            }
//        }

//        //private static void Localize(MenuBar menuBar)
//        //{
//        //    Localize(menuBar.Items);
//        //}

//        //private static void Localize(ContainerBar containerBar)
//        //{
//        //    Localize(containerBar.Items);
//        //}

//        //private static void Localize(ToolStripContainer toolStripContainer)
//        //{
//        //    if (toolStripContainer.TopToolStripPanel != null)
//        //    {
//        //        Localize(toolStripContainer.TopToolStripPanel);
//        //    }
//        //    if (toolStripContainer.BottomToolStripPanel != null)
//        //    {
//        //        Localize(toolStripContainer.BottomToolStripPanel);
//        //    }
//        //    if (toolStripContainer.LeftToolStripPanel != null)
//        //    {
//        //        Localize(toolStripContainer.LeftToolStripPanel);
//        //    }
//        //    if (toolStripContainer.RightToolStripPanel != null)
//        //    {
//        //        Localize(toolStripContainer.RightToolStripPanel);
//        //    }
//        //}

//        private static void Localize(ToolStripPanel toolStripPanel)
//        {
//            foreach (Control control in toolStripPanel.Controls)
//            {
//                Localize(control);
//            }
//        }

//        private static void Localize(ToolStrip toolStrip)
//        {
//            Localize(toolStrip.Items);
//        }

//        private static void Localize(ToolStripItemCollection toolStripItemCollection)
//        {
//            foreach (ToolStripItem toolStripItem in toolStripItemCollection)
//            {
//                Localize(toolStripItem);
//            }
//        }

//        private static void Localize(ToolStripItem toolStripItem)
//        {
//            toolStripItem.Text = Translate(toolStripItem.Text);
//            toolStripItem.ToolTipText = Translate(toolStripItem.ToolTipText);
//        }

//        private static void Localize(Control control)
//        {
//            control.Text = Translate(control.Text);

//            // ToolStringContainer
//            if (control is ToolStripContainer)
//            {
//                //ToolStripContainer toolStripContainer = (ToolStripContainer)control;
//                //Localize(toolStripContainer);
//            }

//            // ToolStrip
//            else if (control is ToolStrip)
//            {
//                ToolStrip toolStrip = (ToolStrip)control;
//                Localize(toolStrip);
//            }

//            // TreeView
//            else if (control is TreeView)
//            {
//                TreeView treeView = (TreeView)control;
//                Localize(treeView);
//            }

//            //// Menubar
//            //else if (control is MenuBar)
//            //{
//            //    MenuBar menuBar = (MenuBar)control;
//            //    Localize(menuBar);
//            //}

//            //// ContainerBar
//            //else if (control is ContainerBar)
//            //{
//            //    ContainerBar containerBar = (ContainerBar)control;
//            //    Localize(containerBar);
//            //}

//            // Localize it's child controls
//            Localize(control.Controls);
//        }

//        ///// <summary>
//        ///// Translates the text of all the "SandBar" toolbar items in a toolbar item collection
//        ///// </summary>
//        ///// <param name="toolbarItems">A "SandBar" toolbar item collection</param>
//        ///// <param name="reverse">Whether or not to revert to the base language</param>
//        //private static void Localize(ToolbarItemBaseCollection toolbarItems)
//        //{
//        //    #region Input Validation
//        //    if (toolbarItems == null)
//        //    {
//        //        throw new ArgumentNullException("toolbarItems");
//        //    }
//        //    #endregion Input Validation

//        //    foreach (ToolbarItemBase toolbarItem in toolbarItems)
//        //    {
//        //        toolbarItem.Text = Translate(toolbarItem.Text);
//        //        if (toolbarItem is MenuBarItem)
//        //        {
//        //            Localize(((MenuBarItem)toolbarItem).Items);
//        //        }
//        //    }
//        //}

//        ///// <summary>
//        ///// Translates the text of all the "SandBar" menubar items in a menubar item collection
//        ///// </summary>
//        ///// <param name="menuItems">A "SandBar" menubar item collection</param>
//        ///// <param name="reverse">Whether or not to revert to the base language</param>
//        //private static void Localize(MenuItemBase.MenuItemCollection menuItems)
//        //{
//        //    #region Input Validation
//        //    if (menuItems == null)
//        //    {
//        //        throw new ArgumentNullException("menuItems");
//        //    }
//        //    #endregion Input Validation

//        //    foreach (MenuItemBase menuItem in menuItems)
//        //    {
//        //        menuItem.Text = Translate(menuItem.Text);
//        //        Localize(menuItem.Items);
//        //    }
//        //}
        
//        /// <summary>
//        /// Translates the text of all items in a ComboxBox items collection
//        /// </summary>
//        /// <param name="control">A control collection</param>
//        /// <param name="reverse">true to reverse the translation; otherwise, false</param>
//        private static void Localize(ComboBox cb)
//        {
//            #region Input Validation
//            if (cb == null)
//            {
//                throw new ArgumentNullException("cb");
//            }
//            #endregion Input Validation
            
//            int index;
//            for (index = 0; index < cb.Items.Count; index++)
//            {
//                cb.Items[index] = Translate(cb.Items[index].ToString());
//            }
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="lb"></param>
//        private static void Localize(ListBox lb)
//        {
//            #region Input Validation
//            if (lb == null)
//            {
//                throw new ArgumentNullException("lb");
//            }
//            #endregion Input Validation

//            int index;
//            for (index = 0; index < lb.Items.Count; index++)
//            {
//                lb.Items[index] = Translate(lb.Items[index].ToString());
//            }
//        }

//        #endregion private methods

//        #region Public Methods

//        /// <summary>
//        /// Performs translation for the controls on a form
//        /// </summary>
//        /// <param name="form">The form to localize</param>
//        public static void Localize(Form form)
//        {
//            #region Input Validation
//            if (form == null)
//            {
//                throw new ArgumentNullException("form");
//            }
//            #endregion Input Validation

//            form.Text = Translate(form.Text);
//            Localize(form.Controls);
//        }

//        #endregion Public Methods
//    }
//}
