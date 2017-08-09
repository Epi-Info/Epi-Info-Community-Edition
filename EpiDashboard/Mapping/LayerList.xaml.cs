using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EpiDashboard.Mapping
{
    /// <summary>
    /// Interaction logic for LayerList.xaml
    /// </summary>
    public partial class LayerList : UserControl
    {
        public event EventHandler LayerClosed;

        public bool HasDataLayer
        {
            get 
            {
                if (grdPlaceholder.Children.Count == 0) return false;

                foreach (UIElement element in grdPlaceholder.Children)
                {
                    if (element is Grid)
                    {
                        if (((Grid)element).Children[0] is ILayerProperties && (((Grid)element).Children[0] is IReferenceLayerProperties == false))
                        {
                            return true;
                        }
                    }
                }

                return false; 
            } 
        }
        
        public LayerList(Esri.ArcGISRuntime.Controls.Map myMap, Epi.View view, Epi.Data.IDbDriver db, DashboardHelper dashboardHelper)
        {
            InitializeComponent();

            #region Translation
            tblockMapLayers.Text = DashboardSharedStrings.GADGET_MAP_LAYERS;
            #endregion
        }

        public void AddListItem(ILayerProperties item, int index)
        {
            Grid container = new Grid();

            StackPanel stkNavButtons = new StackPanel();
            stkNavButtons.Orientation = Orientation.Horizontal;
            stkNavButtons.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

            Path pthUp = (Path)System.Windows.Markup.XamlReader.Parse("<Path xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'  Data=\"M85,30 L85,30 L87.5,25 L90,30 z\" RenderTransformOrigin=\"3.083,-1.417\"/>");
            pthUp.Height = 15;
            pthUp.Width = 15;
            pthUp.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            pthUp.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            pthUp.Stretch = Stretch.Fill;
            pthUp.Fill = new SolidColorBrush(Color.FromArgb(0x99, 0x5C, 0x90, 0xB2));
            pthUp.StrokeThickness = 0;
            pthUp.Tag = container;
            pthUp.Cursor = Cursors.Hand;
            pthUp.ToolTip = new Label() { Content = DashboardSharedStrings.GADGET_MAP_LAYER_FORWARD };
            pthUp.MouseDown += new MouseButtonEventHandler(pthUp_MouseDown);

            Path pthDown = (Path)System.Windows.Markup.XamlReader.Parse("<Path xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'  Data=\"M85,25 L85,25 L87.5,30 L90,25 z\" RenderTransformOrigin=\"3.083,-1.417\"/>");
            pthDown.Height = 15;
            pthDown.Width = 15;
            pthDown.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            pthDown.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            pthDown.Stretch = Stretch.Fill;
            pthDown.Fill = new SolidColorBrush(Color.FromArgb(0x99, 0x5C, 0x90, 0xB2));
            pthDown.StrokeThickness = 0;
            pthDown.Tag = container;
            pthDown.Cursor = Cursors.Hand;
            pthDown.ToolTip = new Label() { Content = DashboardSharedStrings.GADGET_MAP_LAYER_BACK };
            pthDown.MouseDown += new MouseButtonEventHandler(pthDown_MouseDown);

            Path pthClose = (Path)System.Windows.Markup.XamlReader.Parse("<Path xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'  Data=\"M85,25 L85,25 L90,30 M90,25 L90,25 L85,30\" RenderTransformOrigin=\"3.083,-1.417\"/>");
            pthClose.Height = 15;
            pthClose.Width = 15;
            pthClose.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            pthClose.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            pthClose.Stretch = Stretch.Fill;
            pthClose.Stroke = new SolidColorBrush(Color.FromArgb(0x99, 0x5C, 0x90, 0xB2));
            pthClose.StrokeThickness = 2;
            pthClose.Tag = container;
            pthClose.Cursor = Cursors.Hand;
            pthClose.ToolTip = new Label() { Content = DashboardSharedStrings.GADGET_MAP_LAYER_CLOSE };
            pthClose.Margin = new Thickness(3, 0, 0, 0);
            pthClose.MouseDown += new MouseButtonEventHandler(pthClose_MouseDown);

            stkNavButtons.Children.Add(pthUp);
            stkNavButtons.Children.Add(pthDown);
            stkNavButtons.Children.Add(pthClose);

            item.MakeReadOnly();
            container.Children.Add((UIElement)item);
            container.Children.Add(stkNavButtons);

            grdPlaceholder.Children.Insert(index, container);
            txtLayerCount.Text = grdPlaceholder.Children.Count.ToString();
        }

        void pthClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid item = (Grid)((Path)sender).Tag;
            ((ILayerProperties)item.Children[0]).CloseLayer();
            grdPlaceholder.Children.Remove(item);
            txtLayerCount.Text = grdPlaceholder.Children.Count.ToString();
            
            if (LayerClosed != null)
            {
                LayerClosed(this, new EventArgs());
            }
        }

        public void CloseAllLayers()
        {
            List<Grid> grids = new List<Grid>();
            foreach (UIElement child in grdPlaceholder.Children)
            {
                if (child is Grid)
                {
                    foreach (UIElement item in ((Grid)child).Children)
                    {
                        if (item is ILayerProperties)
                        {
                            ((ILayerProperties)item).CloseLayer();
                            grids.Add((Grid)child);
                        }
                    }
                }
            }
            
            foreach (Grid grid in grids)
            {
                grdPlaceholder.Children.Remove(grid);
            }
            
            txtLayerCount.Text = "0";
            
            if (LayerClosed != null)
            {
                LayerClosed(this, new EventArgs());
            }
            
            EpiDashboard.Mapping.ShapeFileReader.ExtensionMethods.ClearPoints();
        }

        public System.Xml.XmlElement SerializeLayers()
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            System.Xml.XmlElement root = doc.CreateElement("Layers");
            
            foreach (UIElement child in grdPlaceholder.Children)
            {
                if (child is Grid)
                {
                    foreach (UIElement item in ((Grid)child).Children)
                    {
                        if (item is ILayerProperties)
                        {
                            root.AppendChild(((ILayerProperties)item).Serialize(doc));
                        }
                    }
                }
            }
            
            return root;
        }

        void pthDown_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid item = (Grid)((Path)sender).Tag;
            if (item.Children[0] is ILayerProperties)
            {
                ILayerProperties layer = ((ILayerProperties)item.Children[0]);
                int currentIndex = grdPlaceholder.Children.IndexOf(item);
                if (currentIndex < grdPlaceholder.Children.Count - 1)
                {
                    item.Children.Remove((UIElement)layer);
                    grdPlaceholder.Children.Remove(item);
                    AddListItem(layer, currentIndex + 1);
                    layer.MoveDown();
                }
            }
        }

        void pthUp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid item = (Grid)((Path)sender).Tag;
            if (item.Children[0] is ILayerProperties)
            {
                ILayerProperties layer = ((ILayerProperties)item.Children[0]);
                int currentIndex = grdPlaceholder.Children.IndexOf(item);
                if (currentIndex > 0)
                {
                    item.Children.Remove((UIElement)layer);
                    grdPlaceholder.Children.Remove(item);
                    AddListItem(layer, currentIndex - 1);
                    layer.MoveUp();
                }
            }
        }

        public List<ILayerProperties> Layers
        {
            get
            {
                List<ILayerProperties> layers = new List<ILayerProperties>();
                foreach (UIElement element in grdPlaceholder.Children)
                {
                    if (element is Grid)
                    {
                        if (((Grid)element).Children[0] is ILayerProperties)
                        {
                            layers.Add((ILayerProperties)((Grid)element).Children[0]);
                        }
                    }
                }
                return layers;
            }
        }

        public List<ClusterLayerProperties> ClusterLayers
        {
            get
            {
                List<ClusterLayerProperties> layers = new List<ClusterLayerProperties>();
                foreach (UIElement element in grdPlaceholder.Children)
                {
                    if (element is Grid)
                    {
                        if (((Grid)element).Children[0] is ClusterLayerProperties)
                        {
                            layers.Add((ClusterLayerProperties)((Grid)element).Children[0]);
                        }
                    }
                }
                return layers;
            }
        }
    }
}
