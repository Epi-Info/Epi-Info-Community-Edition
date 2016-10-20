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
            GeoCoordinate geoCoordinate = null;
            
            if(Context.EnterCheckCodeInterface.LastPosition != null)
            {
                geoCoordinate = Context.EnterCheckCodeInterface.LastPosition.Location;
            }

            if (geoCoordinate != null && geoCoordinate.IsUnknown != true)
            {
                return geoCoordinate.Latitude.ToString();
            }
            
            return null;
        }
    }
}