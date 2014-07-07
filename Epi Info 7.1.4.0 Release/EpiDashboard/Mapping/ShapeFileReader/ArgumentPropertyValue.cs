using System;
using System.ComponentModel;

namespace EpiDashboard.Mapping.ShapeFileReader
{
    public class ArgumentPropertyValue<T> : IPropertyValue<T>, INotifyPropertyChanged
    {
        string _name;
        T _value;

        public ArgumentPropertyValue(string name, T val):this()
        {
            _name = name;
            _value = val;
        }

        public ArgumentPropertyValue()
        {

        }

        #region IPropertyValue<T> Members

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if( _name != value )
                {
                    _name = value;
                    RaisePropertyChanged( "Name" );
                }
            }
        }

        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                if( !_value.Equals( value ) )
                {
                    _value = value;
                    RaisePropertyChanged( "Value" );
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged( string propertyName )
        {
            if( PropertyChanged != null )
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }

        #endregion
    }
}