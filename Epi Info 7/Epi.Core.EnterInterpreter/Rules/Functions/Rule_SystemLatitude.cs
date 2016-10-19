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
                    // https://developers.google.com/maps/documentation/geolocation/intro

                    string url = "https://www.googleapis.com/geolocation/v1/geolocate?key=AIzaSyCkwERu0dtXN0CARM8DQNE_12-JiDsF2OM";
                    string rs = "";

                    using (var client = new WebClient())
                    {
                        try
                        {
                            rs = client.UploadString(url, "{ \"considerIp\": \"false\" }");
                        }
                        catch (System.Net.WebException)
                        {
                            return null;
                        }
                    }

                    int latIndex = rs.IndexOf("lat");
                    string lat = rs.Substring(latIndex + 5).Trim();
                    int endIndex = lat.IndexOf(",");
                    lat = lat.Substring(0, endIndex);

                    double checkLat;
                    if (double.TryParse(lat, out checkLat))
                    {
                        return lat;
                    }
                }
                catch { }

                return null;
            }
        }
    }
}