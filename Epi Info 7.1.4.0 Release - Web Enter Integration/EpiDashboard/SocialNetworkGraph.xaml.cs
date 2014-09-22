using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EpiDashboard
{

    public delegate void SphereSelectedHandler(int viewId, int recordId);

    /// <summary>
    /// Interaction logic for SocialNetworkGraph.xaml
    /// </summary>
    public partial class SocialNetworkGraph : UserControl
    {

        public event SphereSelectedHandler SphereSelected;
        private Epi.View view;
        private Epi.Data.IDbDriver db;
        private int counter;

        private struct Position
        {
            public double Top;
            public double Left;
        }

        public SocialNetworkGraph(Epi.View view, Epi.Data.IDbDriver db)
        {
            InitializeComponent();
            this.db = db;
            this.view = view;
            this.Loaded += new RoutedEventHandler(SocialNetworkGraph_Loaded);
            grdMain.SizeChanged += new SizeChangedEventHandler(grdMain_SizeChanged);
        }

        public void SaveAsImage()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG Image (.png)|*.png";

            if (dlg.ShowDialog().Value)
            {
                BitmapSource img = (BitmapSource)ToImageSource(grdMain);

                FileStream stream = new FileStream(dlg.SafeFileName, FileMode.Create);
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                encoder.Save(stream);
                stream.Close();
                MessageBox.Show("Social network image has been saved.", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private ImageSource ToImageSource(FrameworkElement obj)
        {
            // Save current canvas transform
            System.Windows.Media.Transform transform = obj.LayoutTransform;

            // fix margin offset as well
            Thickness margin = obj.Margin;
            obj.Margin = new Thickness(0, 0,
                 margin.Right - margin.Left, margin.Bottom - margin.Top);

            // Get the size of canvas
            Size size = new Size(obj.ActualWidth, obj.ActualHeight);

            // force control to Update
            obj.Measure(size);
            obj.Arrange(new Rect(size));

            RenderTargetBitmap bmp = new RenderTargetBitmap(
                (int)obj.ActualWidth, (int)obj.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            bmp.Render(obj);

            // return values as they were before
            obj.LayoutTransform = transform;
            obj.Margin = margin;
            return bmp;
        }        

        public void Print()
        {
            PrintDialog dialog = new PrintDialog();
            if (dialog.ShowDialog() == true)
            { 
                dialog.PrintVisual(grdMain, "Social Network Analysis"); 
            }
        }

        void grdMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas.SetTop(MainCanvas, (grdMain.ActualHeight / 2) - (MainCanvas.ActualHeight / 2));
            Canvas.SetLeft(MainCanvas, (grdMain.ActualWidth / 2) - (MainCanvas.ActualWidth / 2));
        }

        private void SocialNetworkGraph_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            Canvas.SetTop(MainCanvas, (grdMain.ActualHeight / 2) - (MainCanvas.ActualHeight / 2));
            Canvas.SetLeft(MainCanvas, (grdMain.ActualWidth / 2) - (MainCanvas.ActualWidth / 2));
            MainCanvas.Cursor = Cursors.ScrollAll;

            Hashtable nodes = new Hashtable();
            List<string> connectors = new List<string>();
            counter = 1;
            Ellipse center = AddEllipseToCanvas(Colors.Red, 25, 25, "Current Record: " + view.CurrentRecordId, new SphereInfo(view.CurrentGlobalRecordId, view.Id, view.CurrentRecordId));
            Canvas.SetTop(center, (MainCanvas.ActualHeight / 2) - 25);
            Canvas.SetLeft(center, (MainCanvas.ActualWidth / 2) - 25);
            nodes.Add(view.CurrentGlobalRecordId, center);
            AddSpheres(false, view.CurrentGlobalRecordId, center, nodes, connectors, 175, 17.1);
            AddConnections(nodes, connectors);
        }

        private void AddConnections(Hashtable nodes, List<string> connectors)
        {
            foreach (string connector in connectors)
            {
                Ellipse from = (Ellipse)nodes[connector.Split(',')[0]];
                Ellipse to = (Ellipse)nodes[connector.Split(',')[1]];
                Line line = new Line();
                line.X1 = Canvas.GetLeft(from) + (from.Height / 2);
                line.Y1 = Canvas.GetTop(from) + (from.Height / 2);
                line.X2 = Canvas.GetLeft(to) + (to.Height / 2);
                line.Y2 = Canvas.GetTop(to) + (to.Height / 2);
                line.Stroke = Brushes.DarkGray;
                line.StrokeThickness = 1;
                line.StrokeEndLineCap = PenLineCap.Triangle;
                Point endPoint = GetVectorEndPoint(line, (to.Height / 2) + 9);
                line.X2 = endPoint.X;
                line.Y2 = endPoint.Y;
                Point arrowStartPoint = GetVectorEndPoint(line, 2);
                Line arrowHead = new Line();
                arrowHead.X1 = arrowStartPoint.X;
                arrowHead.Y1 = arrowStartPoint.Y;
                arrowHead.X2 = line.X2;
                arrowHead.Y2 = line.Y2;
                arrowHead.Stroke = Brushes.DarkGray;
                arrowHead.StrokeThickness = 20;
                arrowHead.StrokeEndLineCap = PenLineCap.Triangle;

                //line.StrokeDashArray = new DoubleCollection { 3 };
                //line.StrokeDashCap = PenLineCap.Triangle;

                //DoubleAnimation animaDouble1 = new DoubleAnimation(10, 50, new Duration(new TimeSpan(0, 0, 5)));
                //animaDouble1.AutoReverse = false;
                //animaDouble1.RepeatBehavior = RepeatBehavior.Forever;
                //line.BeginAnimation(Line.StrokeDashOffsetProperty, animaDouble1);

                MainCanvas.Children.Add(line);
                MainCanvas.Children.Add(arrowHead);
            }
        }

        private Point GetVectorEndPoint(Line line, double radius)
        {
            double adjacent = line.Y2 - line.Y1;
            double opposite = line.X1 - line.X2;
            double angle = Math.Atan(opposite / adjacent);
            Point point;
            if (line.Y2 >= line.Y1)
            {
                point = new Point(line.X2 + (radius * Math.Sin(angle)), line.Y2 - (radius * Math.Cos(angle)));
            }
            else
            {
                point = new Point(line.X2 - (radius * Math.Sin(angle)), line.Y2 + (radius * Math.Cos(angle)));
            }
            return point;
        }

        private DataTable GetToData(string fromRecordGuid)
        {
            string uniqueKeys = "";
            string parens = "";
            string joins = "";
            foreach (Epi.View epiView in view.Project.Views)
            {
                uniqueKeys += "t" + epiView.Id + ".UniqueKey as Key" + epiView.Id + ", ";
                parens += "(";
                joins += "left outer join " + epiView.TableName + " t" + epiView.Id + " on m.ToRecordGuid = t" + epiView.Id + ".GlobalRecordId) ";
            }
            uniqueKeys = uniqueKeys.Substring(0, uniqueKeys.Length - 2) + " ";

            Epi.Data.Query query = db.CreateQuery(@"Select FromRecordGuid, ToRecordGuid, FromViewId, ToViewId, " + uniqueKeys + " from " + parens + "metaLinks m " + joins + " where m.FromRecordGuid = @GlobalRecordId");
            query.Parameters.Add(new Epi.Data.QueryParameter("@GlobalRecordId", DbType.StringFixedLength, fromRecordGuid));
            return db.Select(query);
        }

        private DataTable GetFromData(string toRecordGuid)
        {
            string uniqueKeys = "";
            string parens = "";
            string joins = "";
            foreach (Epi.View epiView in view.Project.Views)
            {
                uniqueKeys += "t" + epiView.Id + ".UniqueKey as Key" + epiView.Id + ", ";
                parens += "(";
                joins += "left outer join " + epiView.TableName + " t" + epiView.Id + " on m.FromRecordGuid = t" + epiView.Id + ".GlobalRecordId) ";
            }
            uniqueKeys = uniqueKeys.Substring(0, uniqueKeys.Length - 2) + " ";

            Epi.Data.Query query = db.CreateQuery(@"Select FromRecordGuid, ToRecordGuid, FromViewId, ToViewId, " + uniqueKeys + " from " + parens + "metaLinks m " + joins + " where m.ToRecordGuid = @GlobalRecordId");
            query.Parameters.Add(new Epi.Data.QueryParameter("@GlobalRecordId", DbType.StringFixedLength, toRecordGuid));
            return db.Select(query);
        }

        private void AddSpheres(bool rotate, string globalRecordId, Ellipse center, Hashtable nodes, List<string> connectors, double connectorLength, double radius)
        {
            List<Ellipse> localCircles = new List<Ellipse>();
            DataTable toLinks = GetToData(globalRecordId);
            DataTable fromLinks = GetFromData(globalRecordId);

            foreach (DataRow link in toLinks.Rows)
            {
                Ellipse circle;

                if (nodes.ContainsKey(link["ToRecordGuid"]))
                {
                    circle = (Ellipse)nodes[link["ToRecordGuid"]];
                }
                else
                {
                    counter++;
                    circle = AddEllipseToCanvas(Colors.White, radius, radius, view.Project.GetViewById((int)link["ToViewId"]).Name + ": " + link["Key" + link["ToViewId"].ToString()], new SphereInfo(link["ToRecordGuid"].ToString(), (int)link["ToViewId"], (int)link["Key" + link["ToViewId"].ToString()]));
                    nodes.Add(link["ToRecordGuid"], circle);
                    localCircles.Add(circle);
                }
                if (!connectors.Contains(link["ToRecordGuid"] + "," + globalRecordId) && !connectors.Contains(globalRecordId + "," + link["ToRecordGuid"]))
                {
                    connectors.Add(circle.Tag == null ? circle.ToString() : globalRecordId + "," + circle.Tag.ToString());
                }
            }

            foreach (DataRow link in fromLinks.Rows)
            {
                Ellipse circle;
                if (nodes.ContainsKey(link["FromRecordGuid"]))
                {
                    circle = (Ellipse)nodes[link["FromRecordGuid"]];
                }
                else
                {
                    counter++;
                    circle = AddEllipseToCanvas(Colors.White, radius, radius, view.Project.GetViewById((int)link["FromViewId"]).Name + ": " + link["Key" + link["FromViewId"].ToString()], new SphereInfo(link["FromRecordGuid"].ToString(), (int)link["FromViewId"], (int)link["Key" + link["FromViewId"].ToString()]));
                    nodes.Add(link["FromRecordGuid"], circle);
                    localCircles.Add(circle);
                }
                if (!connectors.Contains(link["FromRecordGuid"] + "," + globalRecordId) && !connectors.Contains(globalRecordId + "," + link["FromRecordGuid"]))
                {

                    connectors.Add(circle.Tag == null ? circle.ToString() : circle.Tag.ToString() + "," + globalRecordId);
                }
            }

            Ellipse prevCircle = null;
            int localCircleCounter = 0;
            foreach (Ellipse circle in localCircles)
            {
                Position position = GetChildLocation(rotate, localCircleCounter, Canvas.GetTop(center) + (center.Height / 2), Canvas.GetLeft(center) + (center.Height / 2), connectorLength, localCircles.Count);
                Canvas.SetTop(circle, position.Top - (circle.Height / 2));
                Canvas.SetLeft(circle, position.Left - (circle.Height / 2));

                AddSpheres(!rotate, circle.Tag.ToString(), circle, nodes, connectors, connectorLength / 1.5, radius / 1.5);
                prevCircle = circle;
                localCircleCounter++;
            }
        }

        private Position GetChildLocation(bool rotate, int childNumber, double parentTop, double parentLeft, double radius, int totalChildren)
        {
            double angle = (2.0 * System.Math.PI * childNumber / totalChildren);
            if (rotate)
            {
                angle += System.Math.PI / 4;
            }
            Position position;
            position.Top = parentTop + radius * System.Math.Sin(angle);
            position.Left = parentLeft - radius * System.Math.Cos(angle);
            return position;
        }

        #region Rendering

        private Ellipse AddEllipseToCanvas(Color color, double radiusX, double radiusY, string toolTip, SphereInfo tag)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.StrokeThickness = 0.5;
            ellipse.Stroke = Brushes.Black;
            ellipse.ToolTip = toolTip;
            ellipse.Tag = tag;
            if (color == Colors.Red)
                ellipse.Fill = GetRedBrush();
            else if (color == Colors.Blue)
                ellipse.Fill = GetBlueBrush();
            else
                ellipse.Fill = GetDefaultBrush();
            ellipse.Width = radiusX * 2;
            ellipse.Height = radiusY * 2;
            ellipse.Cursor = Cursors.Hand;
            ellipse.MouseDown += new MouseButtonEventHandler(ellipse_MouseDown);
            Canvas.SetZIndex(ellipse, 99);

            AddVisualToCanvas(ellipse);

            return ellipse;
        }

        private void AddVisualToCanvas(FrameworkElement visual)
        {

            MainCanvas.Children.Add(visual);

            visual.IsHitTestVisible = true;
        }

        private void ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int recordId = ((SphereInfo)((System.Windows.Shapes.Ellipse)e.Source).Tag).RecordId;
            int viewId = ((SphereInfo)((System.Windows.Shapes.Ellipse)e.Source).Tag).ViewId;

            if (SphereSelected != null)
                SphereSelected(viewId, recordId);
        }

        private Brush GetRedBrush()
        {
            RadialGradientBrush gradient = new RadialGradientBrush();
            gradient.Center = new Point(0.54326, 0.45465);
            gradient.GradientOrigin = new Point(0.4326, 0.45465);
            gradient.RadiusX = 0.602049;
            gradient.RadiusY = 0.602049;

            GradientStop color1 = new GradientStop();
            color1.Color = Color.FromArgb(0xFF, 0xFF, 0xC1, 0x54);
            color1.Offset = 0;
            gradient.GradientStops.Add(color1);

            GradientStop color2 = new GradientStop();
            color2.Color = Color.FromArgb(0xFF, 0xE4, 0x1E, 0x18);
            color2.Offset = 0.718518495559692;
            gradient.GradientStops.Add(color2);

            GradientStop color3 = new GradientStop();
            color3.Color = Color.FromArgb(0xFF, 0xA5, 0, 0);
            color3.Offset = 1;
            gradient.GradientStops.Add(color3);

            return gradient;
        }

        private Brush GetBlueBrush()
        {
            RadialGradientBrush gradient = new RadialGradientBrush();
            gradient.Center = new Point(0.54326, 0.45465);
            gradient.GradientOrigin = new Point(0.4326, 0.45465);
            gradient.RadiusX = 0.602049;
            gradient.RadiusY = 0.602049;

            GradientStop color1 = new GradientStop();
            color1.Color = Color.FromRgb(0x57, 0xFF, 0xE6);
            color1.Offset = 0;
            gradient.GradientStops.Add(color1);

            GradientStop color2 = new GradientStop();
            color2.Color = Color.FromRgb(0, 0x8E, 0xE7);
            color2.Offset = 0.718518495559692;
            gradient.GradientStops.Add(color2);

            GradientStop color3 = new GradientStop();
            color3.Color = Color.FromRgb(0x2C, 0, 0x72);
            color3.Offset = 1;
            gradient.GradientStops.Add(color3);

            return gradient;
        }

        private Brush GetDefaultBrush()
        {
            RadialGradientBrush gradient = new RadialGradientBrush();
            gradient.Center = new Point(0.54326, 0.45465);
            gradient.GradientOrigin = new Point(0.4326, 0.45465);
            gradient.RadiusX = 0.602049;
            gradient.RadiusY = 0.602049;

            GradientStop color1 = new GradientStop();
            color1.Color = Color.FromRgb(0xFF, 0xFF, 0xFF);
            color1.Offset = 0;
            gradient.GradientStops.Add(color1);

            GradientStop color2 = new GradientStop();
            color2.Color = Color.FromRgb(0xCC, 0xCC, 0xCC);
            color2.Offset = 0.718518495559692;
            gradient.GradientStops.Add(color2);

            GradientStop color3 = new GradientStop();
            color3.Color = Color.FromRgb(0, 0, 0x72);
            color3.Offset = 1;
            gradient.GradientStops.Add(color3);

            return gradient;
        }

        #endregion

    }

    public class SphereInfo
    {
        public string GlobalRecordId { get; set; }
        public int ViewId { get; set; }
        public int RecordId { get; set; }

        public SphereInfo(string globalRecordId, int viewId, int recordId)
        {
            this.GlobalRecordId = globalRecordId;
            this.ViewId = viewId;
            this.RecordId = recordId;
        }

        public override string ToString()
        {
            return this.GlobalRecordId;
        }

    }
}
