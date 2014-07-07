#region Namespaces
using System;
using System.Collections.Generic;
using System.Text;

#endregion //Namespaces

namespace Epi.Windows.Controls
{
    /// <summary>
    /// A combo box that holds a key, value, and text
    /// </summary>
    public class ComboBoxItem
    {
        #region Private Data Members
        private string key;
        private object value;
        private string text;
        #endregion //Private Data Members

        #region Public Properties
        
        /// <summary>
        /// The key of the combo box item
        /// </summary>
        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        /// <summary>
        /// The combo box's selected item value
        /// </summary>
        public object Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        /// <summary>
        /// The combo box selected item's text
        /// </summary>
        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        #endregion  //Public Properties

        #region Constructors
        /// <summary>
        /// The combo box item
        /// </summary>
        /// <param name="key">The combo box's key</param>
        /// <param name="text">The text of the combo box</param>
        /// <param name="value">The value selected in the combo box</param>
        public ComboBoxItem(string key, string text, object value)
        {
            this.key = key;
            this.value = value;
            this.text = text;
        }
        #endregion  //Constructors

        #region //Public Methods
        /// <summary>
        /// Returns the text of the combo box item
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return text.ToString();
        }
        #endregion  //Public Methods
    } 
}
