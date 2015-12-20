using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Eklekto.Imaging.Blobs;

namespace Eklekto.Imaging.Contours
{
    public static class ContoursHelper
    {
        public static List<Contour> FindContours(this Bitmap image)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Countour extractor can be applied to binary 8bpp images only");

            BlobCounter bc = new BlobCounter(image);
            Blob[] blobs = bc.GetBlobs(image, true);

            return blobs.Select(blob => new Contour(bc.GetBlobsContourPoints<SqareTracer>(blob))).ToList();
        }
    }
}
