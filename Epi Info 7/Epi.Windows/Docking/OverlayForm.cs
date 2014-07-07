using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Epi.Windows.Docking
{
	internal partial class OverlayForm : Form
	{
		#region Constructor and intialization
		public OverlayForm()
		{
			Init();

			targetHost = null;
		}

		public OverlayForm(DockContainer target)
		{
			Init();

			targetHost = target;
		}

		private void Init()
		{
			InitializeComponent();
			Application.OpenForms[0].AddOwnedForm(this);

			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.DoubleBuffer, false); System.Reflection.Assembly thisExe;

			if (bmpDockAll == null)
			{
				thisExe = System.Reflection.Assembly.GetExecutingAssembly();
				System.IO.Stream file;

				file = thisExe.GetManifestResourceStream("Epi.Windows.Docking.img.DockAll.png");
				bmpDockAll = new Bitmap(file);

				file = thisExe.GetManifestResourceStream("Epi.Windows.Docking.img.DockCenter.bmp");
				bmpDockCenter = new Bitmap(file);

				file = thisExe.GetManifestResourceStream("Epi.Windows.Docking.img.DockLeft.bmp");
				bmpDockLeft = new Bitmap(file);

				file = thisExe.GetManifestResourceStream("Epi.Windows.Docking.img.DockTop.bmp");
				bmpDockTop = new Bitmap(file);

				file = thisExe.GetManifestResourceStream("Epi.Windows.Docking.img.DockRight.bmp");
				bmpDockRight = new Bitmap(file);

				file = thisExe.GetManifestResourceStream("Epi.Windows.Docking.img.DockBottom.bmp");
				bmpDockBottom = new Bitmap(file);
			}
		}
		#endregion

		#region Variables & Properties
		private static Bitmap bmpDockAll = null;
		private static Bitmap bmpDockCenter = null;
		private static Bitmap bmpDockLeft = null;
		private static Bitmap bmpDockTop = null;
		private static Bitmap bmpDockRight = null;
		private static Bitmap bmpDockBottom = null;

		private DockContainer targetHost = null;

		public DockContainer TargetHost
		{
			get { return targetHost; }
			set { targetHost = value; }
		}
		#endregion

		#region HitTest
		public DockStyle HitTest(int x, int y)
		{
			return HitTest(new Point(x, y));
		}

		public DockStyle HitTest(Point pt)
		{
			// Layout:
			//
			//       |-29-|
			//      32    |
			//       |  T |
			//   |-32-    -32-|
			//  29 L    C   R 29
			//   |----    ----|
			//       |  B |
			//      32    |
			//       |-29-|
			//
			// C is the center of the client rectangle.

			int sizeS = 29;								// short side
			int sizeL = 32;								// long side
			int xBase = this.Width / 2 - sizeS / 2;		// left coordinate of center rectangle
			int yBase = this.Height / 2 - sizeS / 2;	// top coordinate of center rectangle

			Rectangle C = RectangleToScreen(new Rectangle(xBase, yBase, sizeS, sizeS));
			Rectangle T = RectangleToScreen(new Rectangle(xBase, yBase - sizeL, sizeS, sizeL));
			Rectangle L = RectangleToScreen(new Rectangle(xBase - sizeL, yBase, sizeL, sizeS));
			Rectangle B = RectangleToScreen(new Rectangle(xBase, yBase + sizeS, sizeS, sizeL));
			Rectangle R = RectangleToScreen(new Rectangle(xBase + sizeS, yBase, sizeL, sizeS));

			if (C.Contains(pt))
			{
				// Center rectangle hit.
				return DockStyle.Fill;
			}
			else if (!targetHost.AllowSplit)
			{
				// Must be center but no hit.
				return DockStyle.None;
			}
			else if (T.Contains(pt))
			{
				// Top rectangle hit.
				return DockStyle.Top;
			}
			else if (L.Contains(pt))
			{
				// Left rectangle hit.
				return DockStyle.Left;
			}
			else if (B.Contains(pt))
			{
				// Bottom rectangle hit.
				return DockStyle.Bottom;
			}
			else if (R.Contains(pt))
			{
				// Right rectangle hit.
				return DockStyle.Right;
			}

			return DockStyle.None;
		}
		#endregion

		#region Paint
		protected override void OnPaintBackground(PaintEventArgs e)
		{
			e.Graphics.Clear(this.TransparencyKey);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			Pen p;

			this.BringToFront();

			if (this.Opacity < 1)
				this.Opacity = 1;

			if (targetHost != null) 
			{
				Bitmap bmp = bmpDockAll;
				int x, y;

				if (!targetHost.AllowSplit)
					bmp = bmpDockCenter;

				// central bitmap
				x = this.Width / 2 - bmp.Width / 2;
				y = this.Height / 2 - bmp.Height / 2;

				g.DrawImageUnscaled(bmp, new Point(x, y));
			}
			else
			{
				Rectangle rc = this.ClientRectangle;

				if ((DockManager.Style == DockVisualStyle.VS2003) | DockManager.FastMoveDraw)
				{
					HatchBrush brush = new HatchBrush(HatchStyle.Percent50, Color.FromArgb(5, 5, 5), this.TransparencyKey);
					p = new Pen(brush, 6);
				}
				else
				{
					this.Opacity = 0.4;
					g.Clear(Color.FromArgb(10, 36, 106));
					p = new Pen(SystemColors.Control, 2);

					rc.Inflate(-1, -1);
				}

				g.DrawRectangle(p, rc);
			}
		}
		#endregion
	}
}