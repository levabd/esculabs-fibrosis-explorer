using System.Collections.Generic;
using System.Drawing;
using AForge;
using AForge.Imaging;

namespace Eklekto.Imaging.Contours
{
    public interface IContourTracer
    {
        /// <summary>
        /// Current BLOB
        /// </summary>
        Blob Blob { get; set; }

        /// <summary>
        /// Labels of all object
        /// </summary>
        int[] ObjectLabels { get; set; }
        
        /// <summary>
        /// For ObjectLabels
        /// </summary>
        Size ImageSize { get; set; }

        List<IntPoint> SelectContour(IntPoint startPoint);
    }
}
