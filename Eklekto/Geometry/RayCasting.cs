/*
©2012 Alex Kazaev
This product is licensed under Ms-PL http://www.opensource.org/licenses/MS-PL
*/

using System.Collections.Generic;
using AForge;

namespace Eklekto.Geometry
{
    /// <summary>
    /// Class. Solves point-in-polygon problem.
    /// </summary>
    public static class RayCasting
    {
        /// <summary>
        /// Determines if the given point is inside the polygon
        /// </summary>
        /// <param name="polygon">the vertices of polygon</param>
        /// <param name="testPoint">the given point</param>
        /// <returns>true if the point is inside the polygon; otherwise, false</returns>
        public static bool IsPointInside(this IList<IntPoint> polygon, IntPoint testPoint)
        {
            bool result = false;
            int j = polygon.Count - 1;
            for (int i = 0; i < polygon.Count; i++)
            {
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i; 
            }
            return result;
        }
    }
}
