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

namespace EpiDashboard.Mapping.ShapeFileReader
{
    public class DBFField
    {
        public string FieldName
        {
            get;
            set;
        }

        public byte FieldType
        {
            get;
            set;
        }

        public uint FieldDataAddress
        {
            get;
            set;
        }

        public byte FieldLengthInBytes
        {
            get;
            set;
        }

        public byte NumberOfDecimalPlaces
        {
            get;
            set;
        }

        public byte FieldFlags
        {
            get;
            set;
        }

        public uint NextAutoIncrementValue
        {
            get;
            set;
        }

        public byte AutoIncrementStepValue
        {
            get;
            set;
        }

        public byte[] ReservedBytes
        {
            get;
            set;
        }
    }
}
