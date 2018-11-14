using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace EpiDashboard.Mapping
{
    public class MapUIHelper
    {
        public static long CountTimeStopsByTimeInterval(DashboardHelper dashboardHelper, string timeVar)
        {

            List<string> columnNames = new List<string>();
            if (dashboardHelper.IsUsingEpiProject)
                columnNames.Add("UniqueKey");

            if (!string.IsNullOrEmpty(timeVar))
            {
                if (!columnNames.Exists(s => s.Equals(timeVar)))
                    columnNames.Add(timeVar);
            }

            DataTable data = dashboardHelper.GenerateTable(columnNames);

            var minTime = DateTime.MaxValue;
            var maxTime = DateTime.MinValue;
            var minX = double.MaxValue;
            var maxX = double.MinValue;
            var minY = double.MaxValue;
            var maxY = double.MinValue;

            if (data != null)
            {
                foreach (DataRow row in data.Rows)
                {
                    if (row[timeVar] != DBNull.Value)
                    {
                        DateTime time = (DateTime)row[timeVar];
                        minTime = minTime < time ? minTime : time;
                        maxTime = maxTime > time ? maxTime : time;


                    }
                }

            }

            IEnumerable<DateTime> intervals = TimeSlider.CreateTimeStopsByTimeInterval(
                new TimeExtent(minTime, maxTime), new TimeSpan(1, 0, 0, 0));

            return intervals.ToList().Count;

        }
    }
}



