//
// Copied from Mapbox (https://github.com/mapbox/polylabel) and translated to C#
// from JavaScript to work with ESRI.ArcGIS.Client.Geometry.Polygon
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using System.Windows.Media;

namespace EpiDashboard.Mapping
{
    public class Cell
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Half { get; set; }
        public double Distance { get; set; }
        public double Max { get; set; }

        public Cell(double x, double y, double half, ObservableCollection<ESRI.ArcGIS.Client.Geometry.PointCollection> polygon)
        {
            X = x; 
            Y = y;
            Half = half;
            Distance = Adjunct.PointToPolygonDistance(x, y, polygon);
            Max = Distance + Half * Math.Sqrt(2);
        }
    }

    public class CellMaxComparer : IComparer<Cell>
    {
        public int Compare(Cell x, Cell y)
        {
            return x.Max.CompareTo(y.Max);
        }
    }

    public static class Adjunct
    {
        public static double PointToPolygonDistance(double x, double y, ObservableCollection<ESRI.ArcGIS.Client.Geometry.PointCollection> polygon)
        {
            bool inside = false;
            double minDistSq = double.MaxValue;

            for (int k = 0; k < polygon.Count; k++)
            {
                ESRI.ArcGIS.Client.Geometry.PointCollection ring = polygon[k];

                int count = ring.Count;
                int j = count - 1;

                for (int i = 0; i < count; j = i++)
                {
                    MapPoint a = ring[i];
                    MapPoint b = ring[j];

                    if ((a.Y > y != b.Y > y) && (x < (b.X - a.X) * (y - a.Y) / (b.Y - a.Y) + a.X))
                    {
                        inside = !inside;
                    }

                    minDistSq = Math.Min(minDistSq, GetSegDistSq(x, y, a, b));
                }
            }
            return (inside ? 1 : -1) * Math.Sqrt(minDistSq);
        }

        public static double GetSegDistSq(double px, double py, MapPoint a, MapPoint b)
        {
            var x = a.X;
            var y = a.Y;
            var dx = b.X - x;
            var dy = b.Y - y;

            if (dx != 0 || dy != 0)
            {
                var t = ((px - x) * dx + (py - y) * dy) / (dx * dx + dy * dy);

                if (t > 1)
                {
                    x = b.X;
                    y = b.Y;

                }
                else if (t > 0)
                {
                    x += dx * t;
                    y += dy * t;
                }
            }

            dx = px - x;
            dy = py - y;

            return dx * dx + dy * dy;
        }
    }

    static class PolyLabel
    {
        public static Tuple<double, double> PoleOfInaccessibility(ObservableCollection<ESRI.ArcGIS.Client.Geometry.PointCollection> polygon, double precision = 1.0, bool debug = false, GraphicsLayer graphicsLayer = null)
        {
            precision = 0.05;

            double minY = 0;
            double maxY = 0;
            double minX = 0;
            double maxX = 0;

            ESRI.ArcGIS.Client.Geometry.PointCollection points = polygon[polygon.Count - 1];

            int pointCount = 0;

            foreach(var ring in polygon)
            {
                if(ring.Count > pointCount)
                {
                    pointCount = ring.Count;
                    points = ring;
                }
            }

            for (int i = 0; i < points.Count; i++)
            {
                MapPoint p = points[i];
                if (i == 0 || p.X < minX) minX = p.X;
                if (i == 0 || p.Y < minY) minY = p.Y;
                if (i == 0 || p.X > maxX) maxX = p.X;
                if (i == 0 || p.Y > maxY) maxY = p.Y;
            }

            double width = maxX - minX;
            double height = maxY - minY;
            double cellSize = Math.Min(width, height);
            double half = cellSize / 2;
            
            List<Cell> cellList = new List<Cell>();
            CellMaxComparer comp = new CellMaxComparer();
            
            if (cellSize == 0)
            {
                return Tuple.Create<double, double>(minX, minY);
            }

            bool isFirst = true;

            for (double x = minX; x < maxX; x += cellSize)
            {
                for (double y = minY; y < maxY; y += cellSize)
                {
                    Cell cell = new Cell(x + half, y + half, half, polygon);
                    cellList.Add(cell);
                    if (isFirst)
                    {
                        AddDebugGraphic(cell, graphicsLayer);
                        isFirst = false;
                    }
                }
            }
            
            cellList.Sort(comp);

            //Console.WriteLine(string.Format("first sort"));
            //cellList.ForEach(delegate (Cell queueCell)
            //{
            //    Console.WriteLine(string.Format("{0}, {1}, {2}, {3}, {4}", queueCell.X, queueCell.Y, queueCell.Half, queueCell.Distance, queueCell.Max));
            //});

            Cell bestCell = GetCentroidCell(polygon);

            Cell bboxCell = new Cell(minX + width / 2, minY + height / 2, 0, polygon);
            if (bboxCell.Distance > bestCell.Distance)
            {
                bestCell = bboxCell;
            }

            int numProbes = cellList.Count;

            while (cellList.Count != 0)
            {
                //Console.WriteLine(string.Format("cellQueue.Count = {0}", cellList.Count));

                //cellList.ForEach(delegate (Cell queueCell)
                //{
                 //   Console.WriteLine(string.Format("{0}, {1}, {2}, {3}, {4}", queueCell.X, queueCell.Y, queueCell.Half, queueCell.Distance, queueCell.Max));
                //});

                int i = cellList.Count - 1;

                Cell cell = cellList[i];

                cellList.RemoveAt(i);

                if (cell.Distance > bestCell.Distance)
                {
                    bestCell = cell;
                    //Console.WriteLine(string.Format("found best {0} after {1} probes", Math.Round(1e4 * cell.Distance) / 1e4, numProbes));
                }

                if (cell.Max - bestCell.Distance <= precision) continue;

                //Console.WriteLine(string.Format("add four more"));

                half = cell.Half / 2;

                Cell debugCell = new Cell(cell.X - half, cell.Y - half, half, polygon);
                cellList.Add(debugCell);
                AddDebugGraphic(debugCell, graphicsLayer);

                debugCell = new Cell(cell.X + half, cell.Y - half, half, polygon);
                cellList.Add(debugCell);
                AddDebugGraphic(debugCell, graphicsLayer);

                debugCell = new Cell(cell.X - half, cell.Y + half, half, polygon);
                cellList.Add(debugCell);
                AddDebugGraphic(debugCell, graphicsLayer);

                debugCell = new Cell(cell.X + half, cell.Y + half, half, polygon);
                cellList.Add(debugCell);
                AddDebugGraphic(debugCell, graphicsLayer);

                cellList.Sort(comp);
                numProbes += 4;
            }

            //Console.WriteLine("num probes: " + numProbes);
            //Console.WriteLine("best distance: " + bestCell.Distance);

            return Tuple.Create<double, double>(bestCell.X, bestCell.Y);
        }

        public static void AddDebugGraphic(Cell c, GraphicsLayer graphicsLayer)
        {
            return; // COMMENT OUT TO SHOW CELLS

            if (graphicsLayer == null) return;

            Graphic graphic = new Graphic();
            ObservableCollection<ESRI.ArcGIS.Client.Geometry.PointCollection> rings = new ObservableCollection<ESRI.ArcGIS.Client.Geometry.PointCollection>();

            ESRI.ArcGIS.Client.Geometry.PointCollection pointList = new ESRI.ArcGIS.Client.Geometry.PointCollection();
            SpatialReference geoReference = new SpatialReference(4326);

            pointList.Add(new MapPoint(c.X - c.Half, c.Y - c.Half, geoReference));
            pointList.Add(new MapPoint(c.X - c.Half , c.Y + c.Half, geoReference));
            pointList.Add(new MapPoint(c.X + c.Half, c.Y + c.Half, geoReference));
            pointList.Add(new MapPoint(c.X + c.Half, c.Y - c.Half, geoReference));
            pointList.Add(new MapPoint(c.X - c.Half, c.Y - c.Half, geoReference));

            rings.Add(new ESRI.ArcGIS.Client.Geometry.PointCollection(pointList));

            Color color = Colors.Black;
            Brush fill = new SolidColorBrush(Colors.Transparent); 

            ESRI.ArcGIS.Client.Symbols.SimpleFillSymbol symbol = new ESRI.ArcGIS.Client.Symbols.SimpleFillSymbol();

            symbol.Fill = fill;
            symbol.BorderBrush = new SolidColorBrush(Colors.Black);
            symbol.BorderThickness = 1;

            graphic.Symbol = symbol;
 
            Polygon polygon = new Polygon();
            
            polygon.SpatialReference = geoReference;
            polygon.Rings.Add(pointList);

            graphic.Geometry = polygon;

            graphicsLayer.Graphics.Add(graphic);
        }


        public static Cell GetCentroidCell(ObservableCollection<ESRI.ArcGIS.Client.Geometry.PointCollection> polygon)
        {
            double area = 0;
            double x = 0;
            double y = 0;
            ESRI.ArcGIS.Client.Geometry.PointCollection points = polygon[polygon.Count - 1];

            int pointCount = 0;

            foreach (var ring in polygon)
            {
                if (ring.Count > pointCount)
                {
                    pointCount = ring.Count;
                    points = ring;
                }
            }

            int count = points.Count;
            int j = count - 1;

            for (int i = 0; i < count; j = i++)
            {
                MapPoint a = points[i];
                MapPoint b = points[j];
                double f = a.X * b.Y - b.X * a.Y;
                x += (a.X + b.X) * f;
                y += (a.Y + b.Y) * f;
                area += f * 3;
            }

            if (area == 0)
            {
                return new Cell(points[0].X, points[0].Y, 0, polygon);
            }

            return new Cell(x / area, y / area, 0, polygon);
        }
    }
}
