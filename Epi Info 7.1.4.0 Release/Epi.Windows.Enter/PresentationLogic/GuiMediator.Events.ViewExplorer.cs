using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Epi.Fields;

namespace Epi.Windows.Enter.PresentationLogic
{
    public partial class GuiMediator
    {

        /// <summary>
        /// Check required fields
        /// </summary>        
        /// <returns>bool</returns>
        public bool CheckCurrentPageRequiredFields()
        {
            ControlFactory factory = ControlFactory.Instance;

            //check required fields just for this page - before going to next page

            foreach (IField pageField in this.currentPage.GetView().GetFieldsOnPage(this.currentPage))
            {
                try
                {
                    if (pageField is InputFieldWithSeparatePrompt)
                    {
                        if (((InputFieldWithSeparatePrompt)pageField).IsRequired)
                        {
                            if (String.IsNullOrEmpty(((InputFieldWithSeparatePrompt)pageField).CurrentRecordValueString))
                            {
                                //MessageBox.Show(((InputFieldWithSeparatePrompt)pageField).Name + ": This field is required.");
                                MsgBox.ShowInformation(string.Format(SharedStrings.FIELD_IS_REQUIRED, ((InputFieldWithSeparatePrompt)pageField).Name));
                                factory.GetAssociatedControls((InputFieldWithSeparatePrompt)pageField)[1].Focus();

                                return false;
                            }
                        }
                    }
                }
                catch (InvalidCastException ice)
                {
                    //if cast fails, skip viewField
                    //Logger.Log(ice.Message.ToString());
                }
                catch
                {
                    throw new ApplicationException("Unable to check required fields.");
                }
            }
            return true;
        }

        /// <summary>
        /// Check required fields
        /// </summary>        
        /// <returns>bool</returns>
        public bool CheckViewRequiredFields()
        {
            ControlFactory factory = ControlFactory.Instance;

            foreach (Page page in this.view.Pages)
            {
                if (page == this.currentPage)
                {
                    foreach (IField pageField in this.currentPage.GetView().Fields)
                    {
                        try
                        {
                            if (pageField is InputFieldWithSeparatePrompt)
                            {
                                if (((InputFieldWithSeparatePrompt)pageField).IsRequired)
                                {
                                    if (String.IsNullOrEmpty(((InputFieldWithSeparatePrompt)pageField).CurrentRecordValueString))
                                    {
                                        //MessageBox.Show(((InputFieldWithSeparatePrompt)pageField).Name + ": This field is required.");
                                        //--Ei-159
                                        // MsgBox.ShowWarning(string.Format(SharedStrings.FIELD_IS_REQUIRED_LONG, ((InputFieldWithSeparatePrompt)pageField).Prompt, (((InputFieldWithSeparatePrompt)pageField).Page.Position + 1).ToString()));
                                         MsgBox.ShowWarning(string.Format(SharedStrings.FIELD_IS_REQUIRED_LONG, ((InputFieldWithSeparatePrompt)pageField).Prompt, (((InputFieldWithSeparatePrompt)pageField).Page.Name).ToString()));
                                        //
                                        List<Control> fieldList = factory.GetAssociatedControls((InputFieldWithSeparatePrompt)pageField);
                                        if (fieldList != null && fieldList.Count > 1)
                                        {
                                            fieldList[1].Focus();
                                        }

                                        return false;
                                    }
                                }
                            }
                        }
                        catch (InvalidCastException ice)
                        {
                            //if cast fails, skip viewField
                            //Logger.Log(ice.Message.ToString());
                        }
                        catch
                        {
                            //throw new ApplicationException("Unable to check required fields.");
                        }
                    }
                }
                else
                {
                    foreach (IField pageField in page.Fields)
                    {
                        try
                        {
                            if (pageField is InputFieldWithSeparatePrompt)
                            {
                                if (((InputFieldWithSeparatePrompt)pageField).IsRequired)
                                {
                                    if (String.IsNullOrEmpty(((InputFieldWithSeparatePrompt)pageField).CurrentRecordValueString))
                                    {
                                        //MessageBox.Show(((InputFieldWithSeparatePrompt)pageField).Name + ": This field is required.");
                                        //Ei-159
                                        //MsgBox.ShowWarning(string.Format(SharedStrings.FIELD_IS_REQUIRED_LONG, ((InputFieldWithSeparatePrompt)pageField).Prompt, (((InputFieldWithSeparatePrompt)pageField).Page.Position + 1).ToString()));
                                        MsgBox.ShowWarning(string.Format(SharedStrings.FIELD_IS_REQUIRED_LONG, ((InputFieldWithSeparatePrompt)pageField).Prompt, (((InputFieldWithSeparatePrompt)pageField).Page.Name).ToString()));
                                        //
                                        return false;
                                    }
                                }
                            }
                        }
                        catch (InvalidCastException ice)
                        {
                            //if cast fails, skip viewField
                            //Logger.Log(ice.Message.ToString());
                        }
                        catch
                        {
                            //throw new ApplicationException("Unable to check required fields.");
                        }
                    }
                }
            }
            return true;
        }

    }
}
