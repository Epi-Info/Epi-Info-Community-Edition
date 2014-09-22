#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

#endregion

namespace Epi.Windows.Controls
{
	public class TemplateFootprint : System.Windows.Forms.UserControl, IDragable
	{
		#region Private Members

        private int x;
        private int y;
        private bool hasMoved = false;
        private bool isMouseDown = false;

		private System.ComponentModel.Container components = null;
		private Rectangle[] SmallRect = new Rectangle[8];
        private Rectangle FenceRect = new Rectangle();
		private Size Square = new Size(6,6);
		private Graphics g;
		#endregion

		#region Private Enums
		#endregion

        #region Event Handlers

        /// <summary>
        /// Handles the mouse-down event of the label
        /// </summary>
        /// <param name="sender">.NET supplied object</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void TemplateFootprint_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            isMouseDown = true;
            x = e.X;
            y = e.Y;
        }

        /// <summary>
        /// Handles the mouse-move event over the label
        /// </summary>
        /// <param name="sender">.NET supplied object</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void TemplateFootprint_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (isMouseDown)
            {
                DataObject data = new DataObject("DragControl", this);
                this.DoDragDrop(data, DragDropEffects.Move);
                isMouseDown = false;
                this.hasMoved = true;
                this.Invalidate();
                this.Refresh();
            }

            this.Invalidate();
            this.Refresh();
        }

        /// <summary>
        /// Handles the mouse-leave event of the label
        /// </summary>
        /// <param name="sender">.NET supplied object</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void TemplateFootprint_MouseLeave(object sender, System.EventArgs e)
        {
            isMouseDown = false;
        }

        /// <summary>
        /// Handles the drag-over event of the label
        /// </summary>
        /// <param name="sender">.NET supplied object</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void TemplateFootprint_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        #endregion

		#region Component Designer generated code

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.SuspendLayout();
            this.BackColor = System.Drawing.SystemColors.HotTrack;
            this.Name = "TemplateFootprint";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ControlTracker_Paint);

            this.DragOver += new System.Windows.Forms.DragEventHandler(this.TemplateFootprint_DragOver);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TemplateFootprint_MouseMove);
            this.MouseLeave += new System.EventHandler(this.TemplateFootprint_MouseLeave);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TemplateFootprint_MouseDown);

            this.ResumeLayout(false);
		}
		#endregion

		#region Constructors

		public TemplateFootprint(Rectangle rectangle)
		{
            InitializeComponent();
            Create(rectangle);
		}

        public TemplateFootprint()
		{			
			InitializeComponent();
		}

		#endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the horizontal distance of the mouse from the edge of the label
        /// </summary>
        public int XOffset
        {
            get { return this.x; }
            set { this.x = value; }
        }

        /// <summary>
        /// Gets or sets the vertical distance of the mouse from the edge of the control
        /// </summary>
        public int YOffset
        {
            get { return this.y; }
            set { this.y = value; }
        }

        /// <summary>
        /// Gets or sets whether or not the dynamic control has moved
        /// </summary>
        public bool HasMoved
        {
            get { return hasMoved; }
            set { hasMoved = value; }
        }
		#endregion

		#region Public Methods

        /// <summary>
        /// Draws Squares
        /// </summary>
        public void Draw()
		{
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Setup the tracker
		/// </summary>
        private void Create(Rectangle rectangle)
		{
            this.Bounds = rectangle;

            //create transparent area around the control
            this.Region = new Region(BuildFrame());
        
            //create graphics
            g = this.CreateGraphics();
        }

        public void SizeTo(Rectangle rectangle)
        {
            this.Bounds = rectangle;

            //create transparent area around the control
            this.Region = new Region(BuildFrame());

            //create graphics
            g = this.CreateGraphics();
        }


		/// <summary>
		/// Transparents the control area in the tracker
		/// </summary>
		/// <returns>Graphics Path made for transparenting</returns>
		private GraphicsPath BuildFrame()
		{
			//make the tracker to "contain" the control like this:
			//
			//+++++++++++++++++++++++
			//+						+
			//+						+
			//+	 	 CONTROL		+
			//+						+
			//+						+
			//+++++++++++++++++++++++
			//

            GraphicsPath path = new GraphicsPath();

            int margin = 1;
            Rectangle rec = new Rectangle(0, 0, Size.Width, Size.Height);
            path.AddRectangle(rec);

            int voidlWidth = this.Size.Width - (margin * 2);
            int voidHeight = this.Size.Height - (margin * 2);
            rec = new Rectangle(margin, margin, voidlWidth, voidHeight);
            path.AddRectangle(rec);

            return path;
		}

		#endregion

		#region Event Handlers
		private void ControlTracker_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Draw();
		}
		#endregion
	}
}
