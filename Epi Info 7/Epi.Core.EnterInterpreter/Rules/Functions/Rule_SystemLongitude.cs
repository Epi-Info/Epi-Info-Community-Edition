﻿using System;
using System.Device.Location;
using System.Collections.Generic;
using com.calitha.goldparser;
using System.Net;
using System.Collections.Specialized;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Reduction to get the longitude.
    /// </summary>
    public partial class Rule_SystemLongitude : EnterRule
    {
        public Rule_SystemLongitude(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //SYSLONGITUDE
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the longitude.</returns>
        public override object Execute()
        {
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();
            watcher.TryStart(false, TimeSpan.FromMilliseconds(5000));
            var coord = watcher.Position.Location;

            if (coord.IsUnknown != true)
            {
                return coord.Longitude.ToString();
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

                    int lngIndex = rs.IndexOf("lng");
                    string lng = rs.Substring(lngIndex + 5).Trim();
                    int endIndex = lng.IndexOf("\n");
                    lng = lng.Substring(0,endIndex);

                    double checkLng;
                    if (double.TryParse(lng, out checkLng))
                    {
                        return lng;
                    }
                }
                catch { }

                return null;
            }
        }
    }
}