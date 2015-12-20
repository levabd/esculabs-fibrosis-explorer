using System.Collections.Generic;
using System.Drawing;
using AForge;
using AForge.Imaging;

namespace Eklekto.Imaging.Contours
{
    public class RadialSweepTacer : IContourTracer
    {
        public Blob Blob { get; set; }
        public int[] ObjectLabels { get; set; }
        public Size ImageSize { get; set; }

        public List<IntPoint> SelectContour(IntPoint startPoint)
        {
            throw new System.InvalidOperationException("Method not implemented. You can try. http://www.imageprocessingplace.com/downloads_V3/root_downloads/tutorials/contour_tracing_Abeer_George_Ghuneim/ray.html");
        }
    }
}
