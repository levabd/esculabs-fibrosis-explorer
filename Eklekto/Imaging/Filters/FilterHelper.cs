using System;
using System.Drawing;
using System.Drawing.Imaging;
using Accord.Imaging.Filters;

namespace Eklekto.Imaging.Filters
{
    public static class FilterHelper
    {
        /// <summary>
        /// Aforge median filtration
        /// </summary>
        /// <param name="image"></param>
        /// <param name="core">Median core size</param>
        public static Image Median(this Image image, int core)
        {
            var bitmapImage = new Bitmap(image);
            Median medianFilter = new Median(core);
            medianFilter.ApplyInPlace(bitmapImage);
            return bitmapImage;
        }

        /// <summary>
        /// Show edges on image
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Image CannyEdges(this Bitmap image)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Blob extractor can be applied to binary 8bpp images only");

            CannyEdgeDetector cannyEdge = new CannyEdgeDetector();
            return cannyEdge.Apply(image);
        }


        /// <summary>
        /// Blur image with Gauss
        /// </summary>
        /// <param name="image"></param>
        /// <param name="sigma">Gaussia sigma</param>
        /// <param name="kernelSize">Gaussia kermel</param>
        /// <returns></returns>
        public static Bitmap Gauss(this Image image, double sigma, int kernelSize)
        {
            GaussianBlur blur = new GaussianBlur(sigma, kernelSize);
            return blur.Apply(new Bitmap(image));
        }

        /// <summary>
        /// Fast Implementation of Kuwahara filter. Work only with grayscale See https://en.wikipedia.org/wiki/Kuwahara_filter
        /// </summary>
        /// <param name="image"></param>
        /// <param name="kernelSize">Kuwahara kernel size</param>
        public static Bitmap GrayscaleKuwahara(this Bitmap image, int kernelSize)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Filter can be applied to binary 8bpp images only");

            Accord.Imaging.Filters.Kuwahara kuwahara = new Accord.Imaging.Filters.Kuwahara { Size = kernelSize };
            return kuwahara.Apply(image);
        }
    }
}
