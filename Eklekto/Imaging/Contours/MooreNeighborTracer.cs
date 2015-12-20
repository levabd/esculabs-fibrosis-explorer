using System.Collections.Generic;
using System.Drawing;
using AForge;
using AForge.Imaging;

namespace Eklekto.Imaging.Contours
{
    /// <summary>
    /// http://www.imageprocessingplace.com/downloads_V3/root_downloads/tutorials/contour_tracing_Abeer_George_Ghuneim/square.html
    /// </summary>
    public class MooreNeighborTracer : IContourTracer
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
        /// returns all the points that make up the outline of a two dimensional black and white image as represented by a bool[,]
        ///
        /// Pseudo code for Moore-Neighborhood
        /// retrieved from http://en.wikipedia.org/wiki/Moore_neighborhood
        /// Begin
        ///     Set B to be empty.
        ///     From bottom to top and left to right scan the cells of T until a black pixel, s, of P is found.
        ///     Insert s in B.
        ///     Set the current boundary point p to s i.e. p=s
        ///     b = the pixel from which s was entered during the image scan.
        ///     Set c to be the next clockwise pixel (from b) in M(p).
        ///     While c not equal to s do
        ///     If c is black
        ///         insert c in B
        ///         b = p
        ///         p = c
        ///         (backtrack: move the current pixel c to the pixel from which p was entered)
        ///         c = next clockwise pixel (from b) in M(p).
        ///     else
        ///         (advance the current pixel c to the next clockwise pixel in M(p) and update backtrack)
        ///         b = c
        ///         c = next clockwise pixel (from b) in M(p).
        ///     end While
        /// End
        /// </summary>
        /// <returns></returns>
        public List<IntPoint> SelectContour(IntPoint startPoint)
        {
            List<IntPoint> contour = new List<IntPoint>();
            int xBound = ImageSize.Width;
            int yBound = ImageSize.Height;

            IntPoint prev = startPoint;                         // The point we entered curr from
            IntPoint boundary = startPoint + new IntPoint(1, 0);// current know black pixel we're finding neighbours of
            IntPoint curr = Clockwise(boundary, prev);          // The point currently being inspected

            contour.Add(startPoint);

            // Jacob's stopping criterion:
            // stop only when we enter the original pixel in the same way we entered it
            while (curr != startPoint + new IntPoint(1, 0) || prev != startPoint)
            {
                if (curr.Y >= 0 && curr.X >= 0 &&
                    curr.Y < yBound && curr.X < xBound &&
                    ObjectLabels[curr.Y * xBound + curr.X] == Blob.ID)
                {
                    contour.Add(curr);
                    prev = boundary;
                    boundary = curr;
                    curr = Clockwise(boundary, prev);
                }
                else
                {
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
