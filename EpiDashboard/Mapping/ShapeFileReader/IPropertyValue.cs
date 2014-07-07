using System;

namespace EpiDashboard.Mapping.ShapeFileReader
{
    public interface IPropertyValue<T>
    {
        // Properties
        string Name
        {
            get;
            set;
        }
        T Value
        {
            get;
            set;
        }
    }
}