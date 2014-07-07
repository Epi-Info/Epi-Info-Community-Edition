using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace EpiDashboard.Mapping.ShapeFileReader
{
    /// <summary>
    /// The ShapeFileRecord class represents the contents of
    /// a shape record, which is of variable length.
    /// </summary>
    public class ShapeFileRecord
    {
        #region Private fields
        // Record Header.
        private int recordNumber;
        private int contentLength;

        // Shape type.
        private int shapeType;

        // Bounding box for shape.
        private double xMin;
        private double yMin;
        private double xMax;
        private double yMax;

        // Part indices and points array.
        private Collection<int> parts = new Collection<int>();
        private Collection<Point> points = new Collection<Point>();

        // Shape attributes from a row in the dBASE file.
        private IDictionary<string, object> attributes;
        #endregion Private fields

        #region Constructor
        /// <summary>
        /// Constructor for the ShapeFileRecord class.
        /// </summary>
        public ShapeFileRecord()
        {
        }
        #endregion Constructor

        #region Properties
        /// <summary>
        /// Indicates the record number (or index) which starts at 1.
        /// </summary>
        public int RecordNumber
        {
            get
            {
                return this.recordNumber;
            }
            set
            {
                this.recordNumber = value;
            }
        }

        /// <summary>
        /// Specifies the length of this shape record in 16-bit words.
        /// </summary>
        public int ContentLength
        {
            get
            {
                return this.contentLength;
            }
            set
            {
                this.contentLength = value;
            }
        }

        /// <summary>
        /// Specifies the shape type for this record.
        /// </summary>
        public int ShapeType
        {
            get
            {
                return this.shapeType;
            }
            set
            {
                this.shapeType = value;
            }
        }

        /// <summary>
        /// Indicates the minimum x-position of the bounding
        /// box for the shape (expressed in degrees longitude).
        /// </summary>
        public double XMin
        {
            get
            {
                return this.xMin;
            }
            set
            {
                this.xMin = value;
            }
        }

        /// <summary>
        /// Indicates the minimum y-position of the bounding
        /// box for the shape (expressed in degrees latitude).
        /// </summary>
        public double YMin
        {
            get
            {
                return this.yMin;
            }
            set
            {
                this.yMin = value;
            }
        }

        /// <summary>
        /// Indicates the maximum x-position of the bounding
        /// box for the shape (expressed in degrees longitude).
        /// </summary>
        public double XMax
        {
            get
            {
                return this.xMax;
            }
            set
            {
                this.xMax = value;
            }
        }

        /// <summary>
        /// Indicates the maximum y-position of the bounding
        /// box for the shape (expressed in degrees latitude).
        /// </summary>
        public double YMax
        {
            get
            {
                return this.yMax;
            }
            set
            {
                this.yMax = value;
            }
        }

        /// <summary>
        /// Indicates the number of parts for this shape.
        /// A part is a connected set of points, analogous to
        /// a PathFigure in WPF.
        /// </summary>
        public int NumberOfParts
        {
            get
            {
                return this.parts.Count;
            }
        }

        /// <summary>
        /// Specifies the total number of points defining
        /// this shape record.
        /// </summary>
        public int NumberOfPoints
        {
            get
            {
                return this.points.Count;
            }
        }

        /// <summary>      
        /// A collection of indices for the points array.
        /// Each index identifies the starting point of the
        /// corresponding part (or PathFigure using WPF
        /// terminology).
        /// </summary>
        public Collection<int> Parts
        {
            get
            {
                return this.parts;
            }
        }

        /// <summary>
        /// A collection of all of the points defining the
        /// shape record.
        /// </summary>
        public Collection<Point> Points
        {
            get
            {
                return this.points;
            }
        }

        /// <summary>
        /// Access the (dBASE) attribute values associated
        /// with this shape record.
        /// </summary>
        public IDictionary<string, object> Attributes
        {
            get
            {
                return this.attributes;
            }
            set
            {
                this.attributes = value;
            }
        }
        #endregion Properties

        #region Public methods
        /// <summary>
        /// Output some of the fields of the shapefile record.
        /// </summary>
        /// <returns>A string representation of the record.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat( "ShapeFileRecord: RecordNumber={0}, ContentLength={1}, ShapeType={2}",
                this.recordNumber, this.contentLength, this.shapeType );

            return sb.ToString();
        }
        #endregion Public methods
    }
}
