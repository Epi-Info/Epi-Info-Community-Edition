using System;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Epi;
using Epi.Data;
using Epi.Fields;
using EpiDashboard.Rules;

namespace EpiDashboard.Gadgets.StatCalc
{
    /// <summary>
    /// Interaction logic for MatchedPairCaseControlGadget.xaml
    /// </summary>
    public partial class MatchedPairCaseControlGadget : StatCalcGadgetBase, IStatCalcControl
    {
        #region Private Variables
        #endregion

        #region Delegates
        #endregion

        #region Constructors

        public MatchedPairCaseControlGadget()
        {
            InitializeComponent();
            Construct();
        }

        protected override void Construct()
        {
            this.IsProcessing = false;
            base.Construct();
        }
        
        #endregion

        #region Public Properties
        public int PreferredUIHeight 
        {
            get
            {
                return 550;
            }
        }
        public int PreferredUIWidth
        {
            get
            {
                return 530;
            }
        }        
        #endregion // Public Properties

        #region Public Methods
        #endregion

        #region Event Handlers
        #endregion

        #region Private Methods
        /// <summary>
        /// Closes the gadget
        /// </summary>
        protected override void CloseGadget()
        {
            base.CloseGadget();
            GadgetOptions = null;
        }

        #endregion

        #region Private Properties
        #endregion // Private Properties

        /// <summary>
        /// Returns the gadget's description as a string.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "Matched Pair Case Control Statistical Calculator";
        }
    }
}
