#region Namespaces

using System;
using System.Windows.Forms;
using System.Drawing;

#endregion

namespace Epi.Windows
{
	/// <summary>
	/// Class that manages font standardization for all forms.
	/// </summary>
	public class FontStandardization
	{

		#region Private Members

		private const float FORM_FONT_SIZE = 10f;
		private const string FORM_FONT_NAME = "Arial";
		private const float LABEL_FONT_SIZE = 9;
		private const string LABEL_FONT_NAME = "Times New Roman";
		private const float BUTTON_FONT_SIZE = 10f;
		private const string BUTTON_FONT_NAME = "Arial";
		private const float TOOLBARITEM_FONT_SIZE = 11f;
		private const string TOOLBARITEM_FONT_NAME = "Arial";
//		private const float GROUPBOX_FONT_SIZE = 10f;
//		private const string GROUPBOX_FONT_NAME = "Arial";
		private const float COMBOBOX_FONT_SIZE = 9f;
		private const string COMBOBOX_FONT_NAME = "Tahoma";
		private const float LISTBOX_FONT_SIZE = 10f;
		private const string LISTBOX_FONT_NAME = "Arial";
		private const float CHECKBOX_FONT_SIZE = 11f;
		private const string CHECKBOX_FONT_NAME = "Times New Roman";
		private const float RADIOBUTTON_FONT_SIZE = 10f;
		private const string RADIOBUTTON_FONT_NAME = "Arial";
		private const float TEXTBOX_FONT_SIZE = 10f;
		private const string TEXTBOX_FONT_NAME = "Tahoma";

		#endregion //Private Members
		
		#region Constructors

		/// <summary>
		/// Constructor for Font Standardization dialog
		/// </summary>
		public FontStandardization()
		{
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Sets the font property for all the controls in the form
		/// </summary>
		/// <param name="controls">A control collection</param>
		private void StandardizeControls(System.Windows.Forms.Control.ControlCollection controls)
		{
			if (controls.Count == 0)
			{
				throw new ArgumentException("controls");
			}
			foreach (Control control in controls)
			{				
                //if (control is TD.SandBar.MenuBar)
                //{
                //    StandardizeToolbarItems(((TD.SandBar.MenuBar)control).Items);
                //}
                //else
                //{
					StandardizeControls(control.Controls);
                //}
				if (control is TabControl)
				{	
					StandardizeControls(control.Controls);
          		}
				if (control is TabPage)
				{
					StandardizeControls(control.Controls);
				}
				if (control is TextBox)
				{
					control.Font = new Font(TEXTBOX_FONT_NAME,TEXTBOX_FONT_SIZE);
				}
				if (control is CheckBox)
				{
					control.Font = new Font(CHECKBOX_FONT_NAME,CHECKBOX_FONT_SIZE);
				}
				if (control is Label)
				{
					control.Font  = new Font(LABEL_FONT_NAME,LABEL_FONT_SIZE);
				}
				if (control is ComboBox)
				{
					control.Font = new Font(COMBOBOX_FONT_NAME,COMBOBOX_FONT_SIZE);
				}
				if (control is RadioButton)
				{
					control.Font = new Font(RADIOBUTTON_FONT_NAME,RADIOBUTTON_FONT_SIZE);
				}
				if (control is ListBox)
				{
					control.Font = new Font(LISTBOX_FONT_NAME,LISTBOX_FONT_SIZE);
				}
				if (control is GroupBox)
				{					
					StandardizeControls(control.Controls);
				}
				if (control is Button)
				{
					control.Font = new Font(BUTTON_FONT_NAME,BUTTON_FONT_SIZE);
				}
				
			}
		}

        ///// <summary>
        ///// Sets the font property for Sandbar toolbar items
        ///// </summary>
        ///// <param name="toolbarItems">A tool bar item collection</param>
        //private void StandardizeToolbarItems(TD.SandBar.ToolbarItemBaseCollection toolbarItems)
        //{
        //    if (toolbarItems.Count == 0)
        //    {
        //        throw new ArgumentException("toolbarItems");
        //    }
        //    foreach (TD.SandBar.ToolbarItemBase toolbarItem in toolbarItems)
        //    {
						
        //        if (toolbarItem is TD.SandBar.MenuBarItem)
        //        {
        //            StandardizeMenuItems(((TD.SandBar.MenuBarItem)toolbarItem).Items);
        //        }
        //        toolbarItem.Font = new Font(TOOLBARITEM_FONT_NAME,TOOLBARITEM_FONT_SIZE,FontStyle.Bold);
        //    }
        //}

        ///// <summary>
        ///// Sets the font property for Sandbar menuitems
        ///// </summary>
        ///// <param name="menuItems">A menu item collection</param>
        //private void StandardizeMenuItems(TD.SandBar.MenuItemBase.MenuItemCollection menuItems)
        //{
        //    if (menuItems.Count == 0)
        //    {
        //        throw new ArgumentException("menuItems");
        //    }
        //    foreach (TD.SandBar.MenuItemBase menuItem in menuItems)
        //    {
        //        menuItem.Font = new Font(TOOLBARITEM_FONT_NAME,TOOLBARITEM_FONT_SIZE);
        //        StandardizeMenuItems(menuItem.Items);
        //    }
        //}

		#endregion

		#region Public Methods

		/// <summary>
		/// Standardizes form control fonts
		/// </summary>
		/// <param name="form">The form for which to standardize fonts</param>
		public void StandardizeFormFont(Form form)
		{
			if (form == null)
			{
				throw new ArgumentNullException("form");
			}
			StandardizeControls(form.Controls);
			
		//	form.Font = new Font(FORM_FONT_NAME,FORM_FONT_SIZE);
		}

		#endregion

	}
}