using System;
using System.Drawing;
using System.Drawing.Imaging;
using AForge.Imaging.Filters;

namespace Eklekto.Imaging.Morfology
{
    public static class MorfologyHelper
    {
        /// <returns>Morphology opened image</returns>
        public static Bitmap MorphologyOpening(this Bitmap image, int kernel)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Operation can be applied to binary 8bpp images only");//we use morphology only for binarized images

            Erosion morphologyErosion = new Erosion();
            Bitmap temp = morphologyErosion.Apply(image);
            for (int i = 0; i < kernel - 1; i++)
                temp = morphologyErosion.Apply(temp);
            Dilatation morphologyDilatation = new Dilatation();
            for (int i = 0; i < kernel; i++)
                temp = morphologyDilatation.Apply(temp);
            return temp;
        }
    }
}
