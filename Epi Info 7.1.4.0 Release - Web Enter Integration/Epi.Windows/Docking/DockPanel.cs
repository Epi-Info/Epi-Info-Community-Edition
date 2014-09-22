using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;
using System.Reflection;
using System.Security.Permissions;
using System.Drawing.Drawing2D;

namespace Epi.Windows.Docking
{
	/// <summary>
	/// This class is derived from the standard framework class <see cref="System.Windows.Forms.Panel"/>.
	/// It is used as final container of the window's controls and is transferred between <see cref="DockContainer"/> objects and the window.
	/// </summary>
	public partial class DockPanel : System.Windows.Forms.Panel
	{
		#region Construction and dispose
		/// <summary>
		/// Initializes a new instance of the <seealso cref="Panel"/> class.
		/// </summary>
		/// <param name="container">The host container.</param>
		public DockPanel(System.ComponentModel.IContainer container)
		{
			container.Add(this);
			InitializeComponent();

			Init();
		}

		/// <summary>
		/// Initializes a new instance of the <seealso cref="Panel"/> class.
		/// </summary>
		public DockPanel()
		{
			InitializeComponent();
			Init();
		}

		private void Init()
		{
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
		}
		#endregion

		#region Variables
		private RectangleF tabRect = Rectangle.Empty;
		
		private Size minFormSize;
		private Size maxFormSize;

		private DockWindow form;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the rectangle of the tab panel.
		/// </summary>
		[Browsable(false)]
		public RectangleF TabRect
		{
			get { return tabRect; }
			set { tabRect = value; }
		}

		/// <summary>
		/// Gets or sets the host window.
		/// </summary>
		[Browsable(false)]
		public DockWindow Form
		{
			get { return form; }
			set { form = value; }
		}

		/// <summary>
		/// Gets or sets the minimum size of the form.
		/// </summary>
		[Browsable(false)]
		public Size MinFormSize
		{
			get { return minFormSize; }
			set { minFormSize = value; }
		}

		/// <summary>
		/// Gets or sets the maximum size of the form.
		/// </summary>
		[Browsable(false)]
		public Size MaxFormSize
		{
			get { return maxFormSize; }
			set { maxFormSize = value; }
		}
		#endregion

		#region Overrides
		/// <summary>
		/// Overrides the base class function OnMouseDown.
		/// Used to set the focus to the parent control.
		/// </summary>
		/// <param name="e">A <see cref="MouseEventArgs"/> that contains the mouse event data.</param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (Parent != null)
				Parent.Focus();

			base.OnMouseDown (e);
		}
		
		/// <summary>
		/// Overrides the base class function IsInputKey.
		/// </summary>
		/// <param name="keyData">The key that is to be evaluated.</param>
		/// <returns>Always set to true.</returns>
		protected override bool IsInputKey(Keys keyData)
		{
			return true;
		}

		/// <summary>
		/// Overrides the base class function OnPaint.
		/// Fires the PostPaint event after drawing the contents.
		/// </summary>
		/// <param name="e">A <see cref="PaintEventArgs"/> that contains the paint data.</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint (e);

			if (PostPaint != null)
				PostPaint(this, e);
		}
		
		/// <summary>
		/// Overrides the base class function ToString.
		/// </summary>
		/// <returns>The caption string of the attached form.</returns>
		public override string ToString()
		{
			return form.Text;
		}
		#endregion

		#region Own events
		/// <summary>
		/// Occurs after the base drawing is finished.
		/// </summary>
		public event PaintEventHandler PostPaint;

		/// <summary>
		/// Occurs when the panel gets activated.
		/// </summary>
		public event EventHandler Activated;

		/// <summary>
		/// Occurs when the panel gets deactivated.
		/// </summary>
		public event EventHandler Deactivate;

		/// <summary>
		/// Sets the focus to the panel.
		/// </summary>
		/// <param name="activate">True, if the panel is activated.</param>
		public void SetFocus(bool activate)
		{
			if (activate && (Activated != null))
				Activated(this, new EventArgs());
			else if (!activate && (Deactivate != null))
				Deactivate(this, new EventArgs());
		}
		#endregion

		#region XML r/w
		/// <summary>
		/// Writes the panel data to the window save list.
		/// </summary>
		/// <param name="writer">The <see cref="XmlTextWriter"/> object that writes to the target stream.</param>
		internal void WriteXml(XmlTextWriter writer)
		{
			writer.WriteStartElement("panel");
			writer.WriteAttributeString("dock", this.Dock.ToString());
			writer.WriteAttributeString("width", this.Width.ToString());
			writer.WriteAttributeString("height", this.Height.ToString());
			writer.WriteAttributeString("type", form.GetType().AssemblyQualifiedName);
			form.WriteXml(writer);
			writer.WriteEndElement();
		}

		/// <summary>
		/// Reads the panel data from the window save list.
		/// </summary>
		/// <param name="reader">The <see cref="XmlReader"/> object that reads from the source stream.</param>
		internal void ReadXml(XmlReader reader)
		{
			try
			{
				string s;

				switch (reader.GetAttribute("dock"))
				{
					case "Fill":
						this.Dock = DockStyle.Fill;
						break;
					case "Top":
						this.Dock = DockStyle.Top;
						break;
					case "Bottom":
						this.Dock = DockStyle.Bottom;
						break;
					case "Left":
						this.Dock = DockStyle.Left;
						break;
					case "Right":
						this.Dock = DockStyle.Right;
						break;
					default:
						return;
				}

				s = reader.GetAttribute("width");
				if (s != null)
					this.Width = int.Parse(s);

				s = reader.GetAttribute("height");
				if (s != null)
					this.Height = int.Parse(s);

				s = reader.GetAttribute("type");
				if (s == null)
					return;
				
				Type type = Type.GetType(s, true);

				if (type == null)
					return;

				ConstructorInfo info = type.GetConstructor(Type.EmptyTypes);

				if (info == null)
					return;

				DockWindow wnd = info.Invoke(new object[0]) as DockWindow;
				wnd.ControlContainer = this;
				this.form = wnd;
				wnd.CreateContainer();
				wnd.ReadXml(reader);
			}
			catch (Exception ex)
			{
				Console.WriteLine("DockPanel.ReadXml: " + ex.Message);
			}
		}
		#endregion

		/// <summary>
		/// Selects this panel as the active panel in the <see cref="DockContainer"/> parent.
		/// </summary>
		public void SelectTab()
		{
			if (this.Parent is DockContainer)
			{
				(this.Parent as DockContainer).SelectTab(this);
				(this.Parent as DockContainer).Select();
			}
		}
	}
}
