using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
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

using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;

using Epi;
using Epi.Data;
using EpiDashboard.Mapping.ShapeFileReader;
using System.Windows.Controls.DataVisualization.Charting;

namespace EpiDashboard.Mapping
{

    public class TextProvider : ILayerProvider
    {
        private MapView _mapView;
        private Guid layerId;
        MapPoint point;
        System.Drawing.Font font; 
        Brush fontColor;
        string text;


        public TextProvider(MapView mapView, MapPoint point)
        {
            _mapView = mapView;
            this.point = point;
            this.layerId = Guid.NewGuid();
        }

        public void Refresh()
        {
            GraphicsLayer textLayer = _mapView.Map.Layers[layerId.ToString()] as GraphicsLayer;
            if (textLayer != null)
            {
                textLayer.Graphics.Clear();
                RenderText(this.font, this.fontColor, this.text);
            }
        }

        public void MoveUp()
        {
            Layer layer = _mapView.Map.Layers[layerId.ToString()];
            int currentIndex = _mapView.Map.Layers.IndexOf(layer);
            if (currentIndex < _mapView.Map.Layers.Count - 1)
            {
                _mapView.Map.Layers.Remove(layer);
                _mapView.Map.Layers.Insert(currentIndex + 1, layer);
            }
        }

        public void MoveDown()
        {
            Layer layer = _mapView.Map.Layers[layerId.ToString()];
            int currentIndex = _mapView.Map.Layers.IndexOf(layer);
            if (currentIndex > 1)
            {
                _mapView.Map.Layers.Remove(layer);
                _mapView.Map.Layers.Insert(currentIndex - 1, layer);
            }
        }

        public void RenderText(System.Drawing.Font font, Brush fontColor, string text)
        {
            this.font = font;
            this.fontColor = fontColor;
            this.text = text;

            GraphicsLayer markerLayer = _mapView.Map.Layers[layerId.ToString()] as GraphicsLayer;
            if (markerLayer != null)
            {
                markerLayer.Graphics.Clear();
            }
            else
            {
                markerLayer = new GraphicsLayer();
                markerLayer.ID = layerId.ToString();
                _mapView.Map.Layers.Add(markerLayer);
            }

            markerLayer.Graphics.Add(new Graphic() { Geometry = point, Symbol = TextSymbol });
        }

        private Symbol TextSymbol
        {
            get
            {
                Color color = ((SolidColorBrush)fontColor).Color;

                TextSymbol textSymbol = new TextSymbol()
                {
                    Font = new SymbolFont()
                    {
                        FontStyle = SymbolFontStyle.Normal,
                        FontWeight = SymbolFontWeight.Normal,
                        FontFamily = font.FontFamily.Name,
                        FontSize = double.Parse(font.Size.ToString())
                    },

                    Color = color,
                    Text = text
                };
                return textSymbol;
            }
        }

        #region ILayerProvider Members

        public void CloseLayer()
        {
            GraphicsLayer graphicsLayer = _mapView.Map.Layers[layerId.ToString()] as GraphicsLayer;
            if (graphicsLayer != null)
            {
                _mapView.Map.Layers.Remove(graphicsLayer);
            }
        }

        #endregion
    }
}
