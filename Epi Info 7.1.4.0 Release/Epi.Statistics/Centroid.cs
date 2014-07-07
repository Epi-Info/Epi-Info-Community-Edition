using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.Statistics
{
    public static class Centroid
    {
        public static Boolean testCentroid()
        {
            List<List<double>> testpoints = new List<List<double>>();
            List<double> point;
            Random random = new Random();
            for (int i = 0; i < 10; i++)
            {
                point = new List<double>();
                point.Add(Math.Round(100 * random.NextDouble(), 1));
                point.Add(Math.Round(100 * random.NextDouble(), 1));
                testpoints.Add(point);
            }
            double meanX = 0.0;
            double meanY = 0.0;
            for (int i = 0; i < testpoints.Count; i++)
            {
                meanX += testpoints[i][0];
                meanY += testpoints[i][1];
            }
            meanX /= testpoints.Count;
            meanY /= testpoints.Count;
            List<double> centroid = MinSumSquaredDistance(testpoints);
            return true;
        }
        public static List<double> MinSumSquaredDistance(List<List<double>> points)
        {
            List<double> centroid = new List<double>();
            double minX = points[0][0];
            double maxX = points[0][0];
            double minY = points[0][1];
            double maxY = points[0][1];

            for (int i = 0; i < points.Count; i++)
            {
                minX = Math.Round(Math.Min(minX, points[i][0]), 1);
                maxX = Math.Round(Math.Max(maxX, points[i][0]), 1);
                minY = Math.Round(Math.Min(minY, points[i][1]), 1);
                maxY = Math.Round(Math.Max(maxY, points[i][1]), 1);
            }

            double x0 = minX;
            double y0 = minY;
            double x = minX;
            double y = minY;
            double squaredDistance = 0.0;
            double tempSquaredDistance = 0.0;

            while (x0 <= maxX)
            {
                while (y0 <= maxY)
                {
                    tempSquaredDistance = 0.0;
                    for (int i = 0; i < points.Count; i++)
                    {
                        tempSquaredDistance += SquaredDistance(x0, y0, points[i][0], points[i][1]);
                    }
                    if (x0 == minX && y0 == minY)
                        squaredDistance = tempSquaredDistance;
                    if (tempSquaredDistance < squaredDistance)
                    {
                        squaredDistance = tempSquaredDistance;
                        x = x0;
                        y = y0;
                    }
                    y0 += 0.1;
                }
                y0 = minY;
                x0 += 0.1;
            }

            centroid.Add(x);
            centroid.Add(y);
            return centroid;
        }

        public static double SquaredDistance(double x0, double y0, double x1, double y1)
        {
            return Math.Pow(x0 - x1, 2.0) + Math.Pow(y0 - y1, 2.0);
        }
    }
}
