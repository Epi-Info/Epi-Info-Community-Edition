using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Input;

namespace EpiDashboard.Gadgets.StatCalc
{
    public class StatCalcGadgetBase : GadgetBase
    {
        #region Private Members
        private bool hostedInOwnWindow = false;
        #endregion // Private Members

        #region Public Properties
        public bool IsHostedInOwnWindow
        {
            get
            {
                return this.hostedInOwnWindow;
            }
            set
            {
                this.hostedInOwnWindow = value;
                if (IsHostedInOwnWindow)
                {
                    object el = this.FindName("closeButton");
                    if (el != null)
                    {
                        Controls.CloseButton closeButton = el as Controls.CloseButton;
                        closeButton.Visibility = System.Windows.Visibility.Collapsed;
                    }

                    el = this.FindName("borderAll");
                    if (el != null)
                    {
                        Border borderAll = el as Border;
                        borderAll.BorderBrush = System.Windows.Media.Brushes.Transparent;
                    }
                }
                else
                {
                    object el = this.FindName("closeButton");
                    if (el != null)
                    {
                        Controls.CloseButton closeButton = el as Controls.CloseButton;
                        closeButton.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
        }
        #endregion // Protected Properties

        #region Event Handlers
        protected override void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
            }
        }

        protected virtual void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {            
        }        

        protected void CloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.CloseGadget();
        }
        #endregion // Event Handlers

        #region IGadget Members

        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            base.SetGadgetToFinishedState();
        }

        public override void RefreshResults()
        {
        }

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public override void UpdateVariableNames()
        {
        }

        /// <summary>
        /// Generates Xml representation of this gadget
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the frequency gadget from an Xml element
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        public override void CreateFromXml(XmlElement element)
        {
            return;
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false, bool ForWeb = false)
        {
            return string.Empty;
        }

        private string customOutputHeading;
        private string customOutputDescription;
        private string customOutputCaption;

        /// <summary>
        /// Gets/sets the gadget's custom output heading
        /// </summary>
        public override string CustomOutputHeading
        {
            get
            {
                return this.customOutputHeading;
            }
            set
            {
                this.customOutputHeading = value;
            }
        }

        /// <summary>
        /// Gets/sets the gadget's custom output description
        /// </summary>
        public override string CustomOutputDescription
        {
            get
            {
                return this.customOutputDescription;
            }
            set
            {
                this.customOutputDescription = value;
            }
        }

        public override string CustomOutputCaption
        {
            get
            {
                return this.customOutputCaption;
            }
            set
            {
                this.customOutputCaption = value;
            }
        }

        #endregion
    }
}
