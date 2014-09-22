using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

namespace Epi.Windows.Docking
{
	/// <summary>
	/// Summary description for FlatButton.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(FlatButton))]
	public partial class FlatButton : System.Windows.Forms.Button
	{
        #region Constructors
        /// <summary>
		/// Initializes a new instance of the <see cref="FlatButton"/> class.
		/// </summary>
		/// <param name="container">The host container.</param>
		public FlatButton(System.ComponentModel.IContainer container)
		{
			container.Add(this);
			InitializeComponent();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FlatButton"/> class.
		/// </summary>
		public FlatButton()
		{
			InitializeComponent();
        }
        #endregion Constructors

        #region Variables
        private bool pressed = false;

		private Color lightColor = Color.White;
		private Color shadowColor = Color.Black;
		private Color selectColor = Color.Transparent;
		#endregion
		
		#region Properties
		/// <summary>
		/// Gets or sets the light color of the border, when highlighted or pressed.
		/// </summary>
		public Color LightColor
		{
			get { return lightColor; }
			set { lightColor = value; }
		}

		/// <summary>
		/// Gets or sets the shadow color of the border, when highlighted or pressed.
		/// </summary>
		public Color ShadowColor
		{
			get { return shadowColor; }
			set { shadowColor = value; }
		}

		/// <summary>
		/// Gets or sets the shadow color of the border, when highlighted or pressed.
		/// </summary>
		public Color SelectColor
		{
			get { return selectColor; }
			set { selectColor = value; }
		}

		/// <summary>
		/// Gets the pressed state of the button.
		/// </summary>
		public bool Pressed
		{
			get { return pressed; }
		}
		#endregion

		#region Paint
		/// <summary>
		/// Use this event to add custom painting to the control.
		/// The user may draw over the standard layout.
		/// </summary>
		public event PaintEventHandler PostPaint;

		/// <summary>
		/// Performs the drawing actions for the basic layout including highlighting.
		/// </summary>
		/// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			
			g.Clear(this.BackColor);

			Point pt = MousePosition;
			Rectangle rc = RectangleToScreen(this.ClientRectangle);

			if (rc.Contains(pt) && this.Enabled)
			{
				if (selectColor != Color.Transparent)
					g.FillRectangle(new SolidBrush(selectColor), this.ClientRectangle);

				Pen pen1, pen2;

				if (MouseButtons == MouseButtons.Left)
				{
					pen1 = new Pen(shadowColor);
					pen2 = new Pen(lightColor);

					pressed = true;
				}
				else
				{
					pen1 = new Pen(lightColor);
					pen2 = new Pen(shadowColor);

					pressed = false;
				}

				g.DrawLine(pen1, 0, 0, this.Width - 1, 0);
				g.DrawLine(pen1, 0, 1, 0, this.Height - 2);
				g.DrawLine(pen2, 0, this.Height - 1, this.Width - 1, this.Height - 1);
				g.DrawLine(pen2, this.Width - 1, 1, this.Width - 1, this.Height - 2);
			}

			if (PostPaint != null)
				PostPaint(this, e);
		}
		
		/// <summary>
		/// Occurs when the mouse pointer enters the control.
		/// Controls highlighting.
		/// </summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains no event data.</param>
		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			Invalidate();
		}

		/// <summary>
		/// Occurs when the mouse pointer leaves the control.
		/// Controls highlighting.
		/// </summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains no event data.</param>
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			pressed = false;
			Invalidate();
		}

		/// <summary>
		/// Occurs when the mouse pointer is over the control and a mouse button is pressed.
		/// Controls the "Pressed" state.
		/// </summary>
		/// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			Invalidate();
		}

		/// <summary>
		/// Occurs when the mouse pointer is over the control and a mouse button is released.
		/// Controls the "Pressed" state.
		/// </summary>
		/// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			Invalidate();
		}
		#endregion
	}
}
