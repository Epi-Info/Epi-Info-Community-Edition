using System;
using System.Device.Location;
using System.Collections.Generic;
using com.calitha.goldparser;
using System.Net;
using System.Collections.Specialized;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Reduction to get the latitude.
    /// </summary>
    public partial class Rule_SystemLatitude : EnterRule
    {
        public Rule_SystemLatitude(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //SYSTEMLATITUDE
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the latitude.</returns>
        public override object Execute()
        {
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();
            watcher.TryStart(false, TimeSpan.FromMilliseconds(5000));
            var coord = watcher.Position.Location;

            if (coord.IsUnknown != true)
            {
                return coord.Latitude.ToString();
            }
            else
            {
                try
                {
                    string url = "https://www.googleapis.com/geolocation/v1/geolocate?key=AIzaSyCkwERu0dtXN0CARM8DQNE_12-JiDsF2OM";
                    string rs = "";

                    using (var client = new WebClient())
                    {
                        var values = new NameValueCollection();
                        var response = client.UploadValues(url, values);
                        rs = System.Text.Encoding.Default.GetString(response);
                    }

                    int latIndex = rs.IndexOf("lat");
                    string lat = rs.Substring(latIndex + 5, 18).Trim();

                    double checkLat;
                    if(double.TryParse(lat, out checkLat))
                    {
                        return lat;
                    }
                }
                catch { }

                return null;
            }
        }
    }
    public class Address
    {
        public string name;
        public Status Status;
        public IList<Placemark> Placemark;
    }

    public class Status
    {
        public int code;
        public string request;
    }

    public class Placemark
    {
        public string id;
        public string address;
        public AddressDetails AddressDetails;
        public ExtendedData ExtendedData;
        public Point Point;
    }

    public class AddressDetails
    {
        public int Accuracy;
        public Country Country;
    }

    public class ExtendedData
    {
        public LatLonBox LatLonBox;
    }

    public class Point
    {
        public IList<double> coordinates;
    }

    public class Country
    {
        public AdministrativeArea AdministrativeArea;
        public string CountryName;
        public string CountryNameCode;
    }

    public class LatLonBox
    {
        public double north;
        public double south;
        public double east;
        public double west;
    }

    public class AdministrativeArea
    {
        public string AdministrativeAreaName;
        public Locality Locality;
    }

    public class Locality
    {
        public string LocalityName;
        public PostalCode PostalCode;
        public Thoroughfare Thoroughfare;
    }

    public class PostalCode
    {
        public string PostalCodeNumber;
    }

    public class Thoroughfare
    {
        public string ThoroughfareName;
    }
}
