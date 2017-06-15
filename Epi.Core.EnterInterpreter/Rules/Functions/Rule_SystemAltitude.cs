using System;
using System.Device.Location;
using System.Collections.Generic;
using com.calitha.goldparser;
using System.Net;
using System.Collections.Specialized;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Reduction to get the altitude.
    /// </summary>
    public partial class Rule_SystemAltitude : EnterRule
    {
        public Rule_SystemAltitude(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //SYSALTITUDE
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the altitude.</returns>
        public override object Execute()
        {
            GeoCoordinate geoCoordinate = null;

            if (Context.EnterCheckCodeInterface.LastPosition != null)
            {
                geoCoordinate = Context.EnterCheckCodeInterface.LastPosition.Location;
            }

            if (geoCoordinate != null && geoCoordinate.IsUnknown != true)
            {
                return geoCoordinate.Altitude.ToString();
            }

            return null;
        }
    }
}
