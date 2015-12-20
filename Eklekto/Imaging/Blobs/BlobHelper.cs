using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Eklekto.Imaging.Contours;

namespace Eklekto.Imaging.Blobs
{
    public static class BlobHelper
    {
        /// <returns>Extract Blobs from image</returns>
        public static AForge.Imaging.Blob[] ExtractBlobs(this Bitmap image)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Blob extractor can be applied to binary 8bpp images only");

            AForge.Imaging.BlobCounter bc = new AForge.Imaging.BlobCounter(image);

            return bc.GetObjects(image, true);
        }
        
        public static List<BlobEntity> FindBlobs(this Bitmap image)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Countour extractor can be applied to binary 8bpp images only");

            BlobCounter bc = new BlobCounter(image);
            Blob[] blobs = bc.GetBlobs(image, true);

            return blobs.Select(blob => new BlobEntity(blob, new Contour(bc.GetBlobsContourPoints<SqareTracer>(blob)))).ToList();
        }
    }
}
