#region Namespaces

using System;
using System.Collections.Generic;
using System.Text;
using Epi.Windows.Dialogs;

#endregion  //Namespaces

namespace Epi.Windows.MakeView.Dialogs.CheckCodeCommandDialogs
{
    /// <summary>
    /// Base class for all check code design dialogs
    /// </summary>
    public partial class CheckCodeDesignDialog : DialogBase
    {
        #region Private Attributes
        private string output;
        public EpiInfo.Plugin.IEnterInterpreter EpiInterpreter;
        #endregion Private Attributes

        #region Constructors
        /// <summary>
        /// Default Constructor - Design time only
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public CheckCodeDesignDialog()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="frm">The main form</param>
        public CheckCodeDesignDialog(MainForm frm)
            : base(frm)
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The view
        /// </summary>
		public virtual View View
        {
            set {/* Do Nothing */}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <summary>
        /// Gets or sets the output of the dialog
        /// </summary>
        public string Output
        {
            get
            {
                return this.output;
            }
            set
            {
                this.output = value;
            }
        }
        #endregion Properties

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CheckCodeDesignDialog));
            this.SuspendLayout();
            // 
            // baseImageList
            // 
            this.baseImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("baseImageList.ImageStream")));
            this.baseImageList.Images.SetKeyName(0, "");
            this.baseImageList.Images.SetKeyName(1, "");
            this.baseImageList.Images.SetKeyName(2, "");
            this.baseImageList.Images.SetKeyName(3, "");
            this.baseImageList.Images.SetKeyName(4, "");
            this.baseImageList.Images.SetKeyName(5, "");
            this.baseImageList.Images.SetKeyName(6, "");
            this.baseImageList.Images.SetKeyName(7, "");
            this.baseImageList.Images.SetKeyName(8, "");
            this.baseImageList.Images.SetKeyName(9, "");
            this.baseImageList.Images.SetKeyName(10, "");
            this.baseImageList.Images.SetKeyName(11, "");
            this.baseImageList.Images.SetKeyName(12, "");
            this.baseImageList.Images.SetKeyName(13, "");
            this.baseImageList.Images.SetKeyName(14, "");
            this.baseImageList.Images.SetKeyName(15, "");
            this.baseImageList.Images.SetKeyName(16, "");
            this.baseImageList.Images.SetKeyName(17, "");
            this.baseImageList.Images.SetKeyName(18, "");
            this.baseImageList.Images.SetKeyName(19, "");
            this.baseImageList.Images.SetKeyName(20, "");
            this.baseImageList.Images.SetKeyName(21, "");
            this.baseImageList.Images.SetKeyName(22, "");
            this.baseImageList.Images.SetKeyName(23, "");
            this.baseImageList.Images.SetKeyName(24, "");
            this.baseImageList.Images.SetKeyName(25, "");
            this.baseImageList.Images.SetKeyName(26, "");
            this.baseImageList.Images.SetKeyName(27, "");
            this.baseImageList.Images.SetKeyName(28, "");
            this.baseImageList.Images.SetKeyName(29, "");
            this.baseImageList.Images.SetKeyName(30, "");
            this.baseImageList.Images.SetKeyName(31, "");
            this.baseImageList.Images.SetKeyName(32, "");
            this.baseImageList.Images.SetKeyName(33, "");
            this.baseImageList.Images.SetKeyName(34, "");
            this.baseImageList.Images.SetKeyName(35, "");
            this.baseImageList.Images.SetKeyName(36, "");
            this.baseImageList.Images.SetKeyName(37, "");
            this.baseImageList.Images.SetKeyName(38, "");
            this.baseImageList.Images.SetKeyName(39, "");
            this.baseImageList.Images.SetKeyName(40, "");
            this.baseImageList.Images.SetKeyName(41, "");
            this.baseImageList.Images.SetKeyName(42, "");
            this.baseImageList.Images.SetKeyName(43, "");
            this.baseImageList.Images.SetKeyName(44, "");
            this.baseImageList.Images.SetKeyName(45, "");
            this.baseImageList.Images.SetKeyName(46, "");
            this.baseImageList.Images.SetKeyName(47, "");
            this.baseImageList.Images.SetKeyName(48, "");
            this.baseImageList.Images.SetKeyName(49, "");
            this.baseImageList.Images.SetKeyName(50, "");
            this.baseImageList.Images.SetKeyName(51, "");
            this.baseImageList.Images.SetKeyName(52, "");
            this.baseImageList.Images.SetKeyName(53, "");
            this.baseImageList.Images.SetKeyName(54, "");
            this.baseImageList.Images.SetKeyName(55, "");
            this.baseImageList.Images.SetKeyName(56, "");
            this.baseImageList.Images.SetKeyName(57, "");
            this.baseImageList.Images.SetKeyName(58, "");
            this.baseImageList.Images.SetKeyName(59, "");
            this.baseImageList.Images.SetKeyName(60, "");
            this.baseImageList.Images.SetKeyName(61, "");
            this.baseImageList.Images.SetKeyName(62, "");
            this.baseImageList.Images.SetKeyName(63, "");
            this.baseImageList.Images.SetKeyName(64, "");
            this.baseImageList.Images.SetKeyName(65, "");
            this.baseImageList.Images.SetKeyName(66, "");
            this.baseImageList.Images.SetKeyName(67, "");
            this.baseImageList.Images.SetKeyName(68, "");
            this.baseImageList.Images.SetKeyName(69, "");
            this.baseImageList.Images.SetKeyName(70, "");
            this.baseImageList.Images.SetKeyName(71, "");
            this.baseImageList.Images.SetKeyName(72, "");
            this.baseImageList.Images.SetKeyName(73, "");
            this.baseImageList.Images.SetKeyName(74, "");
            this.baseImageList.Images.SetKeyName(75, "");
            this.baseImageList.Images.SetKeyName(76, "");
            this.baseImageList.Images.SetKeyName(77, "");
            this.baseImageList.Images.SetKeyName(78, "");
            this.baseImageList.Images.SetKeyName(79, "");
            // 
            // CheckCodeDesignDialog
            // 
            this.ClientSize = new System.Drawing.Size(520, 262);
            this.Name = "CheckCodeDesignDialog";
            this.ShowIcon = false;
            this.ResumeLayout(false);

        }
    }
}
