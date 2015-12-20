using System;
using System.Collections.Generic;
using System.Linq;
using AForge;

namespace Eklekto.Imaging.Contours
{
    public class Contour
    {
        private readonly List<IntPoint> _points;

        /// <summary>
        /// Contour points one by one
        /// </summary>
        public List<IntPoint> Points => _points;

        /// <summary>
        /// Return accuracy calculated contour length (calculate hypotenuse of triangles if needed)
        /// </summary>
        public double Perimeter
        {
            get
            {
                return _points.Take(_points.Count - 1).Select((p, i) => _points[i + 1].DistanceTo(_points[i])).Sum();
            }
        }

        /// <summary>
        /// Return fast calculated contour length
        /// </summary>
        public int Length => _points.Count;

        /// <param name="points">Contour points</param>
        public Contour(List<IntPoint> points)
        {
            _points = points;
        }

        /// <param name="points">Contour points</param>
        public Contour(IntPoint[] points)
        {
            _points = points.ToList();
        }

        /// <summary>
        /// Fill points betwen edges if we hawe edges, not contour
        /// </summary>
        /// <param name="closed">True if contour is closed. For example left border will have false closed value</param>
        public void FillMissedPoints(bool closed = true)
        {
            List<IntPoint> oldPoints = _points;
            List<IntPoint> pointsToInsertion = new List<IntPoint>();
            for (int pointNumber = closed ? 0 : 1; pointNumber < oldPoints.Count; pointNumber++) // from second point if contour not closed
            {
                int firstPointIndex = pointNumber;
                int secondPointIndex = (pointNumber == 0) ? oldPoints.Count - 1 : pointNumber - 1;
                int xDistance = oldPoints[firstPointIndex].X - oldPoints[secondPointIndex].X;
                int yDistance = oldPoints[firstPointIndex].Y - oldPoints[secondPointIndex].Y;
                int xSign = Math.Sign(xDistance);
                int ySign = Math.Sign(yDistance);

                if (Math.Abs(yDistance) > 1) //hole in Y nnumeration
                {
                    pointsToInsertion.Clear();
                    for (int i = 1; i < Math.Abs(yDistance); i++) // from second point
                        pointsToInsertion.Add(new IntPoint(
                            oldPoints[firstPointIndex].X, 
                            oldPoints[secondPointIndex].Y + i * ySign));

                    _points.InsertRange(pointNumber, pointsToInsertion);
                }
                else if (Math.Abs(xDistance) > 1) //hole in Y nnumeration
                {
                    pointsToInsertion.Clear();
                    for (int i = 1; i < Math.Abs(xDistance); i++) // from second point
                        pointsToInsertion.Add(new IntPoint(
                            oldPoints[secondPointIndex].X + i * xSign, 
                            oldPoints[firstPointIndex].Y));

                    _points.InsertRange(pointNumber, pointsToInsertion);
                }
            }
        }
    }
}
