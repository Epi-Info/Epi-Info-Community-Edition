using System;
using EpiDashboard.Mapping.ShapeFileReader;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Collections;

using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;

namespace EpiDashboard.Mapping.ShapeFileReader
{
    public static class ExtensionMethods
    {

        public static double leftMostPoint;
        public static double topMostPoint;
        public static double rightMostPoint;
        public static double bottomMostPoint;

        public static SimpleMarkerSymbol DEFAULT_MARKER_SYMBOL = new SimpleMarkerSymbol()
        {
            Style = SimpleMarkerStyle.Circle,
            Color = Colors.Red
        };

        public static SimpleLineSymbol DEFAULT_LINE_SYMBOL = new SimpleLineSymbol()
        {
            Color = Colors.Red,
            Style = SimpleLineStyle.Solid,
            Width = 2
        };

        public static SimpleFillSymbol DEFAULT_FILL_SYMBOL = new SimpleFillSymbol()
        {
            Color = Color.FromArgb(192, 255, 0, 0),
            Style = SimpleFillStyle.Solid,
            Outline = new SimpleLineSymbol()
            {
                Color = Colors.Gray,
                Style = SimpleLineStyle.Solid,
                Width = 1
            }
        };

        public static Symbol GetDefaultSymbol( Esri.ArcGISRuntime.Geometry.Geometry geometry )
        {
            if( geometry == null )
                return null;

            Type t = geometry.GetType();
            if( t == typeof( MapPoint ) )
                return DEFAULT_MARKER_SYMBOL;
            else if( t == typeof( Multipoint ) )
                return DEFAULT_MARKER_SYMBOL;
            else if( t == typeof( Polyline ) )
                return DEFAULT_LINE_SYMBOL;
            else if( t == typeof( Polygon ) )
                return DEFAULT_FILL_SYMBOL;
            else if( t == typeof( Envelope ) )
                return DEFAULT_FILL_SYMBOL;

            return null;
        }

        public static bool Contains<T>( this IEnumerable<T> collection, Func<T, bool> evaluator )
        {
            foreach( T local in collection )
            {
                if( evaluator( local ) )
                    return true;
            }
            return false;
        }

        public static void ForEach<T>( this IEnumerable<T> collection, Action<T> action )
        {
            foreach( T local in collection )
            {
                action( local );
            }
        }

        public static int Count<T>( this IEnumerable<T> collection )
        {
            int count = 0;
            foreach( T local in collection )
            {
                count++;
            }
            return count;
        }

        public static void ForEach<T>( this IEnumerable collection, Action<T> action ) where T : class
        {
            foreach( var entry in collection )
            {
                if( entry is T )
                {
                    action( entry as T );
                }
            }
        }

        public static ArgumentPropertyValue<T> RequireArgument<T>( this T item, string argName )
        {
            return new ArgumentPropertyValue<T>( argName, item );
        }

        public static ArgumentPropertyValue<T> NotNull<T>( this ArgumentPropertyValue<T> item ) where T : class
        {
            if( item.Value == null )
            {
                throw new ArgumentNullException( item.Name );
            }
            return item;
        }

        public static ArgumentPropertyValue<string> NotNullOrEmpty( this ArgumentPropertyValue<string> item )
        {
            if( string.IsNullOrEmpty( item.Value ) )
            {
                throw new ArgumentNullException( item.Name );
            }
            return item;
        }

        public static ArgumentPropertyValue<string> ShorterThan( this ArgumentPropertyValue<string> item, int limit )
        {
            if( item.Value.Length >= limit )
            {
                throw new ArgumentException( string.Format( "Parameter {0} must be shorter than {1} chars", item.Name, limit ) );
            }
            return item;
        }

        public static ArgumentPropertyValue<string> StartsWith( this ArgumentPropertyValue<string> item, string pattern )
        {
            if( !item.Value.StartsWith( pattern ) )
            {
                throw new ArgumentException( string.Format( "Parameter {0} must start with {1}", item.Name, pattern ) );
            }
            return item;
        }

        public static Graphic ToGraphic( this ShapeFileRecord record )
        {
            if( record == null || record.Attributes == null)
                return null;

            Graphic graphic = new Graphic();
            ////////graphic.MouseEnter += new System.Windows.Input.MouseEventHandler(graphic_MouseEnter);
            ////////graphic.MouseLeave += new System.Windows.Input.MouseEventHandler(graphic_MouseLeave);

            //add all the attributes to the graphic
            foreach( var item in record.Attributes )
            {
                graphic.Attributes.Add( item.Key, item.Value );
            }

            //add the geometry to the graphic
            switch( record.ShapeType )
            {
                case ( int ) ShapeType.NullShape:
                    break;
                case ( int ) ShapeType.Multipoint:
                    graphic.Geometry = GetMultiPoint( record );
                    graphic.Symbol = DEFAULT_MARKER_SYMBOL;
                    break;
                case ( int ) ShapeType.Point:
                    graphic.Geometry = GetPoint( record );
                    graphic.Symbol = DEFAULT_MARKER_SYMBOL;
                    break;
                case ( int ) ShapeType.Polygon:
                    graphic.Geometry = GetPolygon( record );
                    graphic.Symbol = DEFAULT_FILL_SYMBOL;
                    break;
                case ( int ) ShapeType.PolyLine:
                    graphic.Geometry = GetPolyline( record );
                    graphic.Symbol = DEFAULT_LINE_SYMBOL;
                    break;
                default:
                    break;
            }

            return graphic;
        }

        static void graphic_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Symbol symbol = ((Graphic)sender).Symbol;
            SimpleFillSymbol simpleFillSymbol = ((SimpleFillSymbol)symbol);
            Color currentColor = simpleFillSymbol.Color;
            SolidColorBrush currentBrush = new SolidColorBrush(currentColor);
            currentColor.A -= 15;
            currentBrush.Color = currentColor;
            ((SimpleFillSymbol)((Graphic)sender).Symbol).Color = ((SolidColorBrush)currentBrush).Color;
        }

        static void graphic_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Symbol symbol = ((Graphic)sender).Symbol;
            SimpleFillSymbol simpleFillSymbol = ((SimpleFillSymbol)symbol);
            Color currentColor = simpleFillSymbol.Color;
            SolidColorBrush currentBrush = new SolidColorBrush(currentColor);
            currentColor.A += 15;
            currentBrush.Color = currentColor;
            ((SimpleFillSymbol)((Graphic)sender).Symbol).Color = ((SolidColorBrush)currentBrush).Color;
        }

        private static Esri.ArcGISRuntime.Geometry.Geometry GetPolyline( ShapeFileRecord record )
        {
            Polyline line;
            Esri.ArcGISRuntime.Geometry.PointCollection points = new Esri.ArcGISRuntime.Geometry.PointCollection();

            for ( int i = 0; i < record.NumberOfParts; i++ )
            {
                int start = record.Parts[ i ];
                int end;

                if (record.NumberOfParts > 1 && i != (record.NumberOfParts - 1))
                {
                    end = record.Parts[i + 1];
                }
                else
                {
                    end = record.NumberOfPoints;
                }

                for( int j = start; j < end; j++ )
                {
                    System.Windows.Point point = record.Points[ j ];
                    points.Add( new MapPoint( point.X, point.Y ) );
                }
            }

            line = new Polyline(points);
            return line;
        }

        private static Esri.ArcGISRuntime.Geometry.Geometry GetPolygon( ShapeFileRecord record )
        {
            Random rnd = new Random();
            Polygon polygon;
            Esri.ArcGISRuntime.Geometry.PointCollection points = new Esri.ArcGISRuntime.Geometry.PointCollection(new SpatialReference(4326));
            SpatialReference geoReference = new SpatialReference(4326);
            try
            {

                bool isWebMercator = false;
                if (record.Points.Count > 0)
                {
                    if (record.Points[0].Y < -90 || record.Points[0].Y > 90)
                    {
                        isWebMercator = true;
                        points = new Esri.ArcGISRuntime.Geometry.PointCollection(new SpatialReference(102100));
                    }
                }

                for (int i = 0; i < record.NumberOfParts; i++)
                {
                    // Determine the starting index and the end index
                    // into the points array that defines the figure.
                    int start = record.Parts[i];
                    int end;
                    if (record.NumberOfParts > 1 && i != (record.NumberOfParts - 1))
                    {
                        end = record.Parts[i + 1];
                    }
                    else
                    { 
                        end = record.NumberOfPoints;
                    }

                    for (int j = start; j < end; j++)
                    {
                        if (record.NumberOfPoints < 5000 || rnd.Next(0, 5) == 1)
                        {
                            System.Windows.Point point = record.Points[j];
                            if (isWebMercator)
                            {
                                points.Add(new MapPoint(point.X, point.Y, new SpatialReference(102100)));
                            }
                            else
                            {
                                points.Add(new MapPoint(point.X, point.Y, geoReference));
                            }
                            if (leftMostPoint == 0)
                            {
                                leftMostPoint = point.X;
                                rightMostPoint = point.X;
                                topMostPoint = point.Y;
                                bottomMostPoint = point.Y;
                            }
                            else
                            {
                                if (point.X < leftMostPoint) leftMostPoint = point.X;
                                if (point.X > rightMostPoint) rightMostPoint = point.X;
                                if (point.Y < topMostPoint) topMostPoint = point.Y;
                                if (point.Y > bottomMostPoint) bottomMostPoint = point.Y;
                            }
                        }
                    }
                }
            }
            catch { }

            polygon = new Polygon(points, geoReference);

            return polygon;
        }

        public static void ClearPoints() 
        {
            leftMostPoint = 0;
            rightMostPoint = 0;
            topMostPoint = 0;
            bottomMostPoint = 0;
        }

        public static Esri.ArcGISRuntime.Geometry.Envelope GetExtent(this ShapeFile shapeFile)
        {
            Envelope envelope = new Envelope(leftMostPoint, topMostPoint, rightMostPoint, bottomMostPoint);
            if (topMostPoint >= -90 && topMostPoint <= 90)
            {
                envelope = new Envelope(leftMostPoint, topMostPoint, rightMostPoint, bottomMostPoint, new SpatialReference(4326));
            }
            return envelope;
        }

        private static Esri.ArcGISRuntime.Geometry.Geometry GetPoint( ShapeFileRecord record )
        {
            MapPoint point = new MapPoint(record.Points[0].X, record.Points[0].Y);
            return point;
        }

        private static Esri.ArcGISRuntime.Geometry.Geometry GetMultiPoint(ShapeFileRecord record)
        {
            Multipoint points;
            Esri.ArcGISRuntime.Geometry.PointCollection pointCollection = new Esri.ArcGISRuntime.Geometry.PointCollection();
            for ( int i = 0; i < record.Points.Count; i++ )
            {
                System.Windows.Point point = record.Points[ i ];
                pointCollection.Add( new MapPoint( point.X, point.Y ) );
            }

            points = new Multipoint(pointCollection);
            return points;
        }

        public static void Flash( this Graphic graphic )
        {
            Flash( graphic, 200, 1 );
        }

        public static void Flash( this Graphic graphic, double milliseconds, int repeat )
        {
            int count = 1;
            repeat = repeat * 2;
            Symbol tempSymbol = graphic.Symbol;
            Storyboard storyboard = new Storyboard();
            storyboard.Duration = TimeSpan.FromMilliseconds( milliseconds );
            graphic.Symbol = null;
            storyboard.Completed += ( sender, e ) =>
            {
                if( count % 2 == 1 )
                    graphic.Symbol = tempSymbol;
                else
                    graphic.Symbol = null;

                if( count <= repeat )
                    storyboard.Begin();

                count++;
            };
            storyboard.Begin();
        }

    }
}