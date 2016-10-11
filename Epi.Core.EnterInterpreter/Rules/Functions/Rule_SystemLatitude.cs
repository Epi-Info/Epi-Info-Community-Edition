using System;
using System.Device.Location;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

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
                return null;
            }
        }
    }
}
