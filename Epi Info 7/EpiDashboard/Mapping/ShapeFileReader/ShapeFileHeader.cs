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

namespace EpiDashboard.Mapping.ShapeFileReader
{
    /// <summary>
    /// The ShapeFileHeader class represents the contents
    /// of the fixed length, 100-byte file header at the
    /// beginning of every shapefile.
    /// </summary>
    public class ShapeFileHeader
    {
        #region Private fields
        private int fileCode;
        private int fileLength;
        private int version;
        private int shapeType;

        // Bounding box.
        private double xMin;
        private double yMin;
        private double xMax;
        private double yMax;
        #endregion Private fields

        #region Constructor
        /// <summary>
        /// Constructor for the ShapeFileHeader class.
        /// </summary>
        public ShapeFileHeader()
        {
        }
        #endregion Constructor

        #region Properties
        /// <summary>
        /// Indicate the fixed-length of this header in bytes.
        /// </summary>
        public static int Length
        {
            get
            {
                return 100;
            }
        }

        /// <summary>
        /// Specifies the file code for an ESRI shapefile, which
        /// should be the value, 9994.
        /// </summary>
        public int FileCode
        {
            get
            {
                return this.fileCode;
            }
            set
            {
                this.fileCode = value;
            }
        }

        /// <summary>
        /// Specifies the length of the shapefile, expressed
        /// as the number of 16-bit words in the file.
        /// </summary>
        public int FileLength
        {
            get
            {
                return this.fileLength;
            }
            set
            {
                this.fileLength = value;
            }
        }

        /// <summary>
        /// Specifies the shapefile version number.
        /// </summary>
        public int Version
        {
            get
            {
                return this.version;
            }
            set
            {
                this.version = value;
            }
        }

        /// <summary>
        /// Specifies the shape type for the file. A shapefile
        /// contains only one type of shape.
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
        /// box for the shapefile (expressed in degrees longitude).
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
        /// box for the shapefile (expressed in degrees latitude).
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
        /// box for the shapefile (expressed in degrees longitude).
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
        /// box for the shapefile (expressed in degrees latitude).
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
        #endregion Properties

        #region Public methods
        /// <summary>
        /// Output some of the fields of the file header.
        /// </summary>
        /// <returns>A string representation of the file header.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat( "ShapeFileHeader: FileCode={0}, FileLength={1}, Version={2}, ShapeType={3}",
                this.fileCode, this.fileLength, this.version, this.shapeType );

            return sb.ToString();
        }
        #endregion Public methods
    }
}
