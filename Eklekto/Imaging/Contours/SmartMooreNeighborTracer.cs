using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using AForge;
using AForge.Imaging;

namespace Eklekto.Imaging.Contours
{
    /// <summary>
    /// http://www.imageprocessingplace.com/downloads_V3/root_downloads/tutorials/contour_tracing_Abeer_George_Ghuneim/square.html
    /// </summary>
    public class SmartMooreNeighborTracer : IContourTracer
    {
        private static readonly Dictionary<IntPoint, IntPoint> ClockwiseOffset = new Dictionary<IntPoint, IntPoint>
        {
             {new IntPoint(1,0), new IntPoint(1,-1) },    // right        => down-right
             {new IntPoint(1,-1), new IntPoint(0,-1)},    // down-right   => down
             {new IntPoint(0,-1), new IntPoint(-1,-1)},   // down         => down-left
             {new IntPoint(-1,-1), new IntPoint(-1,0)},   // down-left    => left
             {new IntPoint(-1,0), new IntPoint(-1,1)},    // left         => top-left
             {new IntPoint(-1,1), new IntPoint(0,1)},     // top-left     => top
             {new IntPoint(0,1), new IntPoint(1,1)},      // top          => top-right
             {new IntPoint(1,1), new IntPoint(1,0)}       // top-right    => right
        };

        public Blob Blob { get; set; }
        public int[] ObjectLabels { get; set; }
        public Size ImageSize { get; set; }

        /// <summary>
        ///  Moore-Neighborhood with remember contour point
        /// retrieved from http://en.wikipedia.org/wiki/Moore_neighborhood
        /// </summary>
        /// <returns></returns>
        public List<IntPoint> SelectContour(IntPoint startPoint)
        {
            List<IntPoint> contour = new List<IntPoint>();
            int xBound = ImageSize.Width;
            int yBound = ImageSize.Height;

            IntPoint prev = startPoint + new IntPoint(-1, 0);                         // The point we entered curr from
            IntPoint boundary = startPoint;// current know black pixel we're finding neighbours of
            IntPoint curr = Clockwise(boundary, prev);          // The point currently being inspected

            contour.Add(startPoint);

            BitArray visitedPoints = new BitArray(xBound*yBound, false) {[startPoint.Y*xBound + startPoint.X] = true};

            int notfoundCountdown = 8;
            while ((curr != startPoint) && (notfoundCountdown > 0))
            {
                if (curr.Y >= 0 && curr.X >= 0 &&
                    curr.Y < yBound && curr.X < xBound &&
                    !visitedPoints[curr.Y * xBound + curr.X] &&
                    ObjectLabels[curr.Y * xBound + curr.X] == Blob.ID)
                {
                    notfoundCountdown = 8;
                    contour.Add(curr);
                    prev = boundary;
                    boundary = curr;
                    visitedPoints[curr.Y * xBound + curr.X] = true;
                    curr = Clockwise(boundary, prev);
                }
                else
                {
                    notfoundCountdown--;
                    prev = curr;
                    curr = Clockwise(boundary, prev);
                }
            } 
            return contour;
        }

        private static IntPoint Clockwise(IntPoint target, IntPoint prev)
        {
            return ClockwiseOffset[prev - target] + target;
        }
    }
}
