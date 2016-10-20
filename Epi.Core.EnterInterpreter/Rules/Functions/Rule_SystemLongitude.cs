using System;
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
            GeoCoordinate geoCoordinate = null;

            if (Context.EnterCheckCodeInterface.LastPosition != null)
            {
                geoCoordinate = Context.EnterCheckCodeInterface.LastPosition.Location;
            }

            if (geoCoordinate != null && geoCoordinate.IsUnknown != true)
            {
                return geoCoordinate.Longitude.ToString();
            }

            return null;
        }
    }
}
