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
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace EpiDashboard.Mapping.ShapeFileReader
{
    /// <summary>
    /// The ShapeFile class represents the contents of a single
    /// ESRI shapefile. This is the class which contains functionality
    /// for reading shapefiles and their corresponding dBASE attribute
    /// files.
    /// </summary>
    /// <remarks>
    /// You can call the Read() method to import both shapes and attributes
    /// at once. Or, you can open the file stream yourself and read the file
    /// header or individual records one at a time. The advantage of this is
    /// that it allows you to implement your own progress reporting functionality,
    /// for example.
    /// </remarks>
    public class ShapeFile
    {
        #region Constants
        private const int expectedFileCode = 9994;
        #endregion Constants

        #region Private static fields
        private static byte[] intBytes = new byte[ 4 ];
        private static byte[] doubleBytes = new byte[ 8 ];
        #endregion Private static fields

        #region Private fields
        // File Header.
        private ShapeFileHeader fileHeader = new ShapeFileHeader();

        // Collection of Shapefile Records.
        private Collection<ShapeFileRecord> records = new Collection<ShapeFileRecord>();
        #endregion Private fields

        #region Constructor
        /// <summary>
        /// Constructor for the ShapeFile class.
        /// </summary>
        public ShapeFile()
        {
        }
        #endregion Constructor

        #region Properties
        /// <summary>
        /// Access the file header of this shapefile.
        /// </summary>
        public ShapeFileHeader FileHeader
        {
            get
            {
                return this.fileHeader;
            }
        }

        /// <summary>
        /// Access the collection of records for this shapefile.
        /// </summary>
        public Collection<ShapeFileRecord> Records
        {
            get
            {
                return this.records;
            }
        }
        #endregion Properties

        #region Public methods

        public void Read( FileInfo shapeFile, FileInfo dbfFile )
        {
            FileStream sfs = shapeFile.OpenRead();
            ReadShapes( sfs );
            FileStream dfs = dbfFile.OpenRead();
            ReadAttributes( dfs );
        }

        /// <summary>
        /// Read shapes (geometry) from the given shapefile.
        /// </summary>
        /// <param name="fileName">Tthe shapefile.</param>
        public void ReadShapes( FileInfo shapeFile )
        {
            FileStream sfs = shapeFile.OpenRead();
            ReadShapes( sfs );
        }

        /// <summary>
        /// Read shapes (geometry) from the given stream.
        /// </summary>
        /// <param name="stream">Input stream for a shapefile.</param>
        public void ReadShapes( Stream stream )
        {
            // Read the File Header.
            this.ReadShapeFileHeader( stream );

            // Read the shape records.
            this.records.Clear();
            while( true )
            {
                try
                {
                    this.ReadShapeFileRecord( stream );
                }
                catch( IOException )
                {
                    // Stop reading when EOF exception occurs.
                    break;
                }
            }
        }

        /// <summary>
        /// Read the file header of the shapefile.
        /// </summary>
        /// <param name="stream">Input stream.</param>
        public void ReadShapeFileHeader( Stream stream )
        {
            // File Code.
            this.fileHeader.FileCode = ShapeFile.ReadInt32_BE( stream );
            if( this.fileHeader.FileCode != ShapeFile.expectedFileCode )
            {
                string msg = String.Format( System.Globalization.CultureInfo.InvariantCulture, "Invalid FileCode encountered. Expecting {0}.", ShapeFile.expectedFileCode );
                throw new NotSupportedException( msg );
            }

            // 5 unused values.
            ShapeFile.ReadInt32_BE( stream );
            ShapeFile.ReadInt32_BE( stream );
            ShapeFile.ReadInt32_BE( stream );
            ShapeFile.ReadInt32_BE( stream );
            ShapeFile.ReadInt32_BE( stream );

            // File Length.
            this.fileHeader.FileLength = ShapeFile.ReadInt32_BE( stream );

            // Version.
            this.fileHeader.Version = ShapeFile.ReadInt32_LE( stream );

            // Shape Type.
            this.fileHeader.ShapeType = ShapeFile.ReadInt32_LE( stream );

            // Bounding Box.
            this.fileHeader.XMin = ShapeFile.ReadDouble64_LE( stream );
            this.fileHeader.YMin = ShapeFile.ReadDouble64_LE( stream );
            this.fileHeader.XMax = ShapeFile.ReadDouble64_LE( stream );
            this.fileHeader.YMax = ShapeFile.ReadDouble64_LE( stream );

            // Adjust the bounding box in case it is too small.
            if( Math.Abs( this.fileHeader.XMax - this.fileHeader.XMin ) < 1 )
            {
                this.fileHeader.XMin -= 5;
                this.fileHeader.XMax += 5;
            }
            if( Math.Abs( this.fileHeader.YMax - this.fileHeader.YMin ) < 1 )
            {
                this.fileHeader.YMin -= 5;
                this.fileHeader.YMax += 5;
            }

            // Skip the rest of the file header.
            stream.Seek( 100, SeekOrigin.Begin );
        }

        /// <summary>
        /// Read a shapefile record.
        /// </summary>
        /// <param name="stream">Input stream.</param>
        public ShapeFileRecord ReadShapeFileRecord( Stream stream )
        {
            ShapeFileRecord record = new ShapeFileRecord();

            // Record Header.
            record.RecordNumber = ShapeFile.ReadInt32_BE( stream );
            record.ContentLength = ShapeFile.ReadInt32_BE( stream );

            // Shape Type.
            record.ShapeType = ShapeFile.ReadInt32_LE( stream );

            // Read the shape geometry, depending on its type.
            switch( record.ShapeType )
            {
                case ( int ) ShapeType.NullShape:
                    // Do nothing.
                    break;
                case ( int ) ShapeType.Point:
                    ShapeFile.ReadPoint( stream, record );
                    break;
                case ( int ) ShapeType.PolyLine:
                    // PolyLine has exact same structure as Polygon in shapefile.
                    ShapeFile.ReadPolygon( stream, record );
                    break;
                case ( int ) ShapeType.Polygon:
                    ShapeFile.ReadPolygon( stream, record );
                    break;
                case ( int ) ShapeType.Multipoint:
                    ShapeFile.ReadMultipoint( stream, record );
                    break;
                default:
                    {
                        string msg = String.Format( System.Globalization.CultureInfo.InvariantCulture, "ShapeType {0} is not supported.", ( int ) record.ShapeType );
                        throw new NotSupportedException( msg );
                    }
            }

            // Add the record to our internal list.
            this.records.Add( record );

            return record;
        }

        public void ReadAttributes( Stream stream )
        {
            BinaryReader read = new BinaryReader( stream );

            //Read the DBF header info
            //The header is 32 bytes in length

            // Is it a type of file that I can handle?
            //File type:
            //    0x02   FoxBASE
            //    0x03   FoxBASE+/Dbase III plus, no memo
            //    0x30   Visual FoxPro
            //    0x31   Visual FoxPro, autoincrement enabled
            //    0x32   Visual FoxPro with field type Varchar or Varbinary
            //    0x43   dBASE IV SQL table files, no memo
            //    0x63   dBASE IV SQL system files, no memo
            //    0x83   FoxBASE+/dBASE III PLUS, with memo
            //    0x8B   dBASE IV with memo
            //    0xCB   dBASE IV SQL table files, with memo
            //    0xF5   FoxPro 2.x (or earlier) with memo
            //    0xE5   HiPer-Six format with SMT memo file
            //    0xFB   FoxBASE
            byte fileType = read.ReadByte();
            if( new byte[] { 0x02, 0x03, 0x30, 0x43, 0x63, 0x83, 0x8b,
                             0xcb, 0xf5, 0xfb }.Contains( fileType ) )
            {
                // Skip date.
                read.BaseStream.Seek( 3, SeekOrigin.Current );

                // Read useful datas...
                uint RecordCount = read.ReadUInt32();
                ushort FirstRecord = read.ReadUInt16();//or length of header
                ushort RecordLength = read.ReadUInt16();//length of each record
                ushort reserved = read.ReadUInt16();
                byte incompleteTransaction = read.ReadByte();
                byte encryptionFlag = read.ReadByte();
                uint freeRecordThread = read.ReadUInt32();
                byte[] reservedForMultiUserDB = read.ReadBytes( 8 );
                byte tableFlags = read.ReadByte();//0x01   file has a structural .cdx, 0x02   file has a Memo field, 0x04   file is a database (.dbc)
                byte languageDriver = read.ReadByte();
                ushort reserved2 = read.ReadUInt16();

                if( RecordCount != records.Count )
                    throw new InvalidOperationException( "The number of records in the DBF files does not match the number of records in the SHP file." );

                //Read all the dbf fields info
                List<DBFField> DBFFields = new List<DBFField>();
                while( read.PeekChar() != 0x0D )
                {
                    //The Field info is 32 bytes in length
                    DBFField field = new DBFField();
                    field.FieldName = Encoding.UTF8.GetString( read.ReadBytes( 11 ), 0, 11 ).Replace( "\0", "" ).ToLower();
                    field.FieldType = read.ReadByte();
                    //Field type: 
                    //    C   –   Character
                    //    Y   –   Currency
                    //    N   –   Numeric
                    //    F   –   Float
                    //    D   –   Date
                    //    T   –   DateTime
                    //    B   –   Double
                    //    I   –   Integer
                    //    L   –   Logical
                    //    M   – Memo
                    //    G   – General
                    //    C   –   Character (binary)
                    //    M   –   Memo (binary)
                    //    P   –   Picture

                    field.FieldDataAddress = read.ReadUInt32();//Displacement of field in record
                    field.FieldLengthInBytes = read.ReadByte();
                    field.NumberOfDecimalPlaces = read.ReadByte();
                    field.FieldFlags = read.ReadByte();
                    //0x01   System Column (not visible to user), 
                    //0x02   Column can store null values, 
                    //0x04   Binary column (for CHAR and MEMO only), 
                    //0x06   (0x02+0x04) When a field is NULL and binary (Integer, Currency, and Character/Memo fields), 
                    //0x0C   Column is autoincrementing

                    field.NextAutoIncrementValue = read.ReadUInt32();
                    field.AutoIncrementStepValue = read.ReadByte();
                    field.ReservedBytes = read.ReadBytes( 8 );

                    DBFFields.Add( field );
                }
                byte headerTerminator = read.ReadByte();//Read the header record terminator

                //Read all the records now
                read.BaseStream.Seek( FirstRecord, SeekOrigin.Begin );
                int recordIndex = 0;
                while(read.PeekChar() != 0x1A)
                {
                    byte[] recordContent = read.ReadBytes( RecordLength );
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    int currentIndex = 1;
                    foreach( DBFField field in DBFFields )
                    {
                        string temp = Encoding.UTF8.GetString( recordContent, currentIndex, field.FieldLengthInBytes );
                        currentIndex = currentIndex + field.FieldLengthInBytes;
                        row.Add( field.FieldName, temp );
                    }

                    this.records[ recordIndex ].Attributes = row;
                    recordIndex++;
                }
            }
            else
            {
                return;
            }
        }

        public List<Dictionary<string, object>> ReadDBFFile( Stream stream )
        {
            BinaryReader read = new BinaryReader( stream );

            //Read the DBF header info
            //The header is 32 bytes in length

            // Is it a type of file that I can handle?
            //File type:
            //    0x02   FoxBASE
            //    0x03   FoxBASE+/Dbase III plus, no memo
            //    0x30   Visual FoxPro
            //    0x31   Visual FoxPro, autoincrement enabled
            //    0x32   Visual FoxPro with field type Varchar or Varbinary
            //    0x43   dBASE IV SQL table files, no memo
            //    0x63   dBASE IV SQL system files, no memo
            //    0x83   FoxBASE+/dBASE III PLUS, with memo
            //    0x8B   dBASE IV with memo
            //    0xCB   dBASE IV SQL table files, with memo
            //    0xF5   FoxPro 2.x (or earlier) with memo
            //    0xE5   HiPer-Six format with SMT memo file
            //    0xFB   FoxBASE
            byte fileType = read.ReadByte();
            if( new byte[] { 0x02, 0x03, 0x30, 0x43, 0x63, 0x83, 0x8b,
                             0xcb, 0xf5, 0xfb }.Contains( fileType ) )
            {
                // Skip date.
                read.BaseStream.Seek( 3, SeekOrigin.Current );

                // Read useful datas...
                uint RecordCount = read.ReadUInt32();
                ushort FirstRecord = read.ReadUInt16();//or length of header
                ushort RecordLength = read.ReadUInt16();//length of each record
                ushort reserved = read.ReadUInt16();
                byte incompleteTransaction = read.ReadByte();
                byte encryptionFlag = read.ReadByte();
                uint freeRecordThread = read.ReadUInt32();
                byte[] reservedForMultiUserDB = read.ReadBytes( 8 );
                byte tableFlags = read.ReadByte();//0x01   file has a structural .cdx, 0x02   file has a Memo field, 0x04   file is a database (.dbc)
                byte languageDriver = read.ReadByte();
                ushort reserved2 = read.ReadUInt16();

                //Read all the dbf fields info
                List<DBFField> DBFFields = new List<DBFField>();
                while( read.PeekChar() != 0x0D )
                {
                    //The Field info is 32 bytes in length
                    DBFField field = new DBFField();
                    field.FieldName = Encoding.UTF8.GetString( read.ReadBytes( 11 ), 0, 11 ).Replace( "\0", "" ).ToLower();
                    field.FieldType = read.ReadByte();
                    //Field type: 
                    //    C   –   Character
                    //    Y   –   Currency
                    //    N   –   Numeric
                    //    F   –   Float
                    //    D   –   Date
                    //    T   –   DateTime
                    //    B   –   Double
                    //    I   –   Integer
                    //    L   –   Logical
                    //    M   – Memo
                    //    G   – General
                    //    C   –   Character (binary)
                    //    M   –   Memo (binary)
                    //    P   –   Picture

                    field.FieldDataAddress = read.ReadUInt32();//Displacement of field in record
                    field.FieldLengthInBytes = read.ReadByte();
                    field.NumberOfDecimalPlaces = read.ReadByte();
                    field.FieldFlags = read.ReadByte();
                    //0x01   System Column (not visible to user), 
                    //0x02   Column can store null values, 
                    //0x04   Binary column (for CHAR and MEMO only), 
                    //0x06   (0x02+0x04) When a field is NULL and binary (Integer, Currency, and Character/Memo fields), 
                    //0x0C   Column is autoincrementing

                    field.NextAutoIncrementValue = read.ReadUInt32();
                    field.AutoIncrementStepValue = read.ReadByte();
                    field.ReservedBytes = read.ReadBytes( 8 );

                    DBFFields.Add( field );
                }
                byte headerTerminator = read.ReadByte();//Read the header record terminator

                //Read all the records now
                read.BaseStream.Seek( FirstRecord, SeekOrigin.Begin );
                List<Dictionary<string, object>> table = new List<Dictionary<string, object>>();
                while( read.PeekChar() != 0x1A )
                {
                    byte[] recordContent = read.ReadBytes( RecordLength );
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    int currentIndex = 1;
                    foreach( DBFField field in DBFFields )
                    {
                        string temp = Encoding.UTF8.GetString( recordContent, currentIndex, field.FieldLengthInBytes );
                        temp = temp.Trim();
                        currentIndex = currentIndex + field.FieldLengthInBytes;
                        row.Add( field.FieldName, temp );
                    }

                    table.Add( row );
                }

                return table;
            }
            else
            {
                throw new NotSupportedException( "The DBF file type is not supported." );
            }
        }

        /// <summary>
        /// Output the File Header in the form of a string.
        /// </summary>
        /// <returns>A string representation of the file header.</returns>
        public override string ToString()
        {
            return "ShapeFile: " + this.fileHeader.ToString();
        }
        #endregion Public methods

        #region Private methods
        /// <summary>
        /// Read a 4-byte integer using little endian (Intel)
        /// byte ordering.
        /// </summary>
        /// <param name="stream">Input stream to read.</param>
        /// <returns>The integer value.</returns>
        private static int ReadInt32_LE( Stream stream )
        {
            for( int i = 0; i < 4; i++ )
            {
                int b = stream.ReadByte();
                if( b == -1 )
                    throw new EndOfStreamException();
                intBytes[ i ] = ( byte ) b;
            }

            return BitConverter.ToInt32( intBytes, 0 );
        }

        /// <summary>
        /// Read a 4-byte integer using big endian
        /// byte ordering.
        /// </summary>
        /// <param name="stream">Input stream to read.</param>
        /// <returns>The integer value.</returns>
        private static int ReadInt32_BE( Stream stream )
        {
            for( int i = 3; i >= 0; i-- )
            {
                int b = stream.ReadByte();
                if( b == -1 )
                    throw new EndOfStreamException();
                intBytes[ i ] = ( byte ) b;
            }

            return BitConverter.ToInt32( intBytes, 0 );
        }

        /// <summary>
        /// Read an 8-byte double using little endian (Intel)
        /// byte ordering.
        /// </summary>
        /// <param name="stream">Input stream to read.</param>
        /// <returns>The double value.</returns>
        private static double ReadDouble64_LE( Stream stream )
        {
            for( int i = 0; i < 8; i++ )
            {
                int b = stream.ReadByte();
                if( b == -1 )
                    throw new EndOfStreamException();
                doubleBytes[ i ] = ( byte ) b;
            }

            return BitConverter.ToDouble( doubleBytes, 0 );
        }

        /// <summary>
        /// Read a shapefile Point record.
        /// </summary>
        /// <param name="stream">Input stream.</param>
        /// <param name="record">Shapefile record to be updated.</param>
        private static void ReadPoint( Stream stream, ShapeFileRecord record )
        {
            // Points - add a single point.
            Point p = new System.Windows.Point();
            p.X = ShapeFile.ReadDouble64_LE( stream );
            p.Y = ShapeFile.ReadDouble64_LE( stream );
            record.Points.Add( p );

            // Bounding Box.
            record.XMin = p.X;
            record.YMin = p.Y;
            record.XMax = record.XMin;
            record.YMax = record.YMin;
        }

        /// <summary>
        /// Read a shapefile MultiPoint record.
        /// </summary>
        /// <param name="stream">Input stream.</param>
        /// <param name="record">Shapefile record to be updated.</param>
        private static void ReadMultipoint( Stream stream, ShapeFileRecord record )
        {
            // Bounding Box.
            record.XMin = ShapeFile.ReadDouble64_LE( stream );
            record.YMin = ShapeFile.ReadDouble64_LE( stream );
            record.XMax = ShapeFile.ReadDouble64_LE( stream );
            record.YMax = ShapeFile.ReadDouble64_LE( stream );

            // Num Points.
            int numPoints = ShapeFile.ReadInt32_LE( stream );

            // Points.           
            for( int i = 0; i < numPoints; i++ )
            {
                Point p = new Point();
                p.X = ShapeFile.ReadDouble64_LE( stream );
                p.Y = ShapeFile.ReadDouble64_LE( stream );
                record.Points.Add( p );
            }
        }

        /// <summary>
        /// Read a shapefile Polygon record.
        /// </summary>
        /// <param name="stream">Input stream.</param>
        /// <param name="record">Shapefile record to be updated.</param>
        private static void ReadPolygon( Stream stream, ShapeFileRecord record )
        {
            // Bounding Box.
            record.XMin = ShapeFile.ReadDouble64_LE( stream );
            record.YMin = ShapeFile.ReadDouble64_LE( stream );
            record.XMax = ShapeFile.ReadDouble64_LE( stream );
            record.YMax = ShapeFile.ReadDouble64_LE( stream );

            // Num Parts and Points.
            int numParts = ShapeFile.ReadInt32_LE( stream );
            int numPoints = ShapeFile.ReadInt32_LE( stream );

            // Parts.           
            for( int i = 0; i < numParts; i++ )
            {
                record.Parts.Add( ShapeFile.ReadInt32_LE( stream ) );
            }

            // Points.           
            for (int i = 0; i < numPoints; i++)
            {
                Point p = new Point();
                p.X = ShapeFile.ReadDouble64_LE(stream);
                p.Y = ShapeFile.ReadDouble64_LE(stream);
                record.Points.Add(p);
            }
        }

        ///// <summary>
        ///// Merge data rows from the given table with
        ///// the shapefile records.
        ///// </summary>
        ///// <param name="table">Attributes table.</param>
        //private void MergeAttributes( DataTable table )
        //{
        //    // For each data row, assign it to a shapefile record.
        //    int index = 0;
        //    foreach( DataRow row in table.Rows )
        //    {
        //        if( index >= this.records.Count )
        //            break;
        //        this.records[ index ].Attributes = row;
        //        ++index;
        //    }
        //}
        #endregion Private methods
    }
}
