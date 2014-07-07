using System;
using ESRI.ArcGIS.Client;
using EpiDashboard.Mapping.ShapeFileReader;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Collections;

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
            Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle,
            Color = new SolidColorBrush( Colors.Red )
        };

        public static SimpleLineSymbol DEFAULT_LINE_SYMBOL = new SimpleLineSymbol()
        {
            Color = new SolidColorBrush( Colors.Red ),
            Style = SimpleLineSymbol.LineStyle.Solid,
            Width = 2
        };

        public static SimpleFillSymbol DEFAULT_FILL_SYMBOL = new SimpleFillSymbol()
        {
            Fill = new SolidColorBrush(Color.FromArgb(192, 255, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = 1
        };

        public static Symbol GetDefaultSymbol( this ESRI.ArcGIS.Client.Geometry.Geometry geometry )
        {
            if( geometry == null )
                return null;

            Type t = geometry.GetType();
            if( t == typeof( MapPoint ) )
                return DEFAULT_MARKER_SYMBOL;
            else if( t == typeof( MultiPoint ) )
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
            if( record == null )
                return null;

            Graphic graphic = new Graphic();
            graphic.MouseEnter += new System.Windows.Input.MouseEventHandler(graphic_MouseEnter);
            graphic.MouseLeave += new System.Windows.Input.MouseEventHandler(graphic_MouseLeave);

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
            SolidColorBrush currentBrush = ((SolidColorBrush)((SimpleFillSymbol)((Graphic)sender).Symbol).Fill).Clone();
            Color currentColor = currentBrush.Color;
            currentColor.A -= 15;
            currentBrush.Color = currentColor;
            ((SimpleFillSymbol)((Graphic)sender).Symbol).Fill = currentBrush;
        }

        static void graphic_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SolidColorBrush currentBrush = ((SolidColorBrush)((SimpleFillSymbol)((Graphic)sender).Symbol).Fill).Clone();
            Color currentColor = currentBrush.Color;
            currentColor.A += 15;
            currentBrush.Color = currentColor;
            ((SimpleFillSymbol)((Graphic)sender).Symbol).Fill = currentBrush;
        }

        private static ESRI.ArcGIS.Client.Geometry.Geometry GetPolyline( ShapeFileRecord record )
        {
            Polyline line = new Polyline();
            for( int i = 0; i < record.NumberOfParts; i++ )
            {
                // Determine the starting index and the end index
                // into the points array that defines the figure.
                int start = record.Parts[ i ];
                int end;
                if( record.NumberOfParts > 1 && i != ( record.NumberOfParts - 1 ) )
                    end = record.Parts[ i + 1 ];
                else
                    end = record.NumberOfPoints;

                ESRI.ArcGIS.Client.Geometry.PointCollection points = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                // Add line segments to the polyline
                for( int j = start; j < end; j++ )
                {
                    System.Windows.Point point = record.Points[ j ];
                    points.Add( new MapPoint( point.X, point.Y ) );
                }

                line.Paths.Add( points );
            }

            return line;
        }

        private static ESRI.ArcGIS.Client.Geometry.Geometry GetPolygon( ShapeFileRecord record )
        {
            Random rnd = new Random();
            Polygon polygon = new Polygon();
            SpatialReference geoReference = new SpatialReference(4326);
            try
            {
                for (int i = 0; i < record.NumberOfParts; i++)
                {
                    // Determine the starting index and the end index
                    // into the points array that defines the figure.
                    int start = record.Parts[i];
                    int end;
                    if (record.NumberOfParts > 1 && i != (record.NumberOfParts - 1))
                        end = record.Parts[i + 1];
                    else
                        end = record.NumberOfPoints;

                    ESRI.ArcGIS.Client.Geometry.PointCollection points = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                    // Add line segments to the polyline
                    bool isWebMercator = false;
                    if (record.Points.Count > 0)
                    {
                        if (record.Points[0].Y < -90 || record.Points[0].Y > 90)
                        {
                            isWebMercator = true;
                        }
                        else
                        {
                            polygon.SpatialReference = geoReference;
                        }
                    }
                    for (int j = start; j < end; j++)
                    {
                        if (record.NumberOfPoints < 5000 || rnd.Next(0, 5) == 1)
                        {
                            System.Windows.Point point = record.Points[j];
                            if (isWebMercator)
                            {
                                points.Add(new MapPoint(point.X, point.Y));
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

                    polygon.Rings.Add(points);
                }
            }
            catch (Exception ex)
            {
                //
            }
            return polygon;
        }

        public static void ClearPoints() 
        {
            leftMostPoint = 0;
            rightMostPoint = 0;
            topMostPoint = 0;
            bottomMostPoint = 0;
        }

        public static ESRI.ArcGIS.Client.Geometry.Envelope GetExtent(this ShapeFile shapeFile)
        {
            Envelope env = new Envelope(leftMostPoint, topMostPoint, rightMostPoint, bottomMostPoint);
            if (topMostPoint >= -90 && topMostPoint <= 90)
            {
                env.SpatialReference = new SpatialReference(4326);
            }
            return env;
        }

        private static ESRI.ArcGIS.Client.Geometry.Geometry GetPoint( ShapeFileRecord record )
        {
            MapPoint point = new MapPoint();
            point.X = record.Points[ 0 ].X;
            point.Y = record.Points[ 0 ].Y;
            return point;
        }

        private static ESRI.ArcGIS.Client.Geometry.Geometry GetMultiPoint( ShapeFileRecord record )
        {
            MultiPoint points = new MultiPoint();
            for( int i = 0; i < record.Points.Count; i++ )
            {
                System.Windows.Point point = record.Points[ i ];
                points.Points.Add( new MapPoint( point.X, point.Y ) );
            }

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