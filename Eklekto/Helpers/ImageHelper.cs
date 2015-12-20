using System;
using System.Collections.Generic;
using System.Drawing;
using Image = System.Drawing.Image;
using Emgu.CV;
using System.Drawing.Imaging;
using System.Linq;
using Accord.Imaging.Filters;
using AForge.Imaging.Filters;
using FibroscanProcessor.ImageProcessing.Blobs;
using FibroscanProcessor.ImageProcessing.Contours;

namespace FibroscanProcessor.Helpers
{
    public static class ImageHelper
    {
        /// <summary>
        /// Load image from file without filetype exception
        /// </summary>
        /// <param name="filename">Name of file</param>
        /// <returns>Loaded image or null otherwise</returns>
        public static Image SafeLoadFromFile(string filename)
        {
            try
            {
                return Image.FromFile(filename);
            }
            catch (OutOfMemoryException)
            {
                //The file does not have a valid image format.
                //-or- GDI+ does not support the pixel format of the file

                return null;
            }
        }


        #region Converters
        /// <summary>
        /// Conveert any Image to OpenCV Mat type
        /// </summary>
        /// <typeparam name="TColor">Color sheme of resulted Mat image</typeparam>
        public static Mat ToMat<TColor>(this Image image) where TColor : struct, IColor
        {
            Image<TColor, Byte> img = new Image<TColor, Byte>(new Bitmap(image));

            return img.Mat;
        }

        public static Bitmap ToGrayscale(this Image original)
        {
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                //create the grayscale ColorMatrix
                ColorMatrix colorMatrix = new ColorMatrix(new []
                {
                    new [] {.3f, .3f, .3f, 0, 0},
                    new [] {.59f, .59f, .59f, 0, 0},
                    new [] {.11f, .11f, .11f, 0, 0},
                    new [] {0, 0, 0, 1f, 0},
                    new [] {0, 0, 0, 0, 1f}
                });

                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);

                g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                   0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);    
            }
            return newBitmap;
        }
        #endregion

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

        /// <returns>Cropped image</returns>
        public static Bitmap Crop(this Image image, Rectangle rectangle)
        {
            Crop cropper = new Crop(rectangle);
            return cropper.Apply(new Bitmap(image));
        }

        /// <returns>Extract Blobs from image</returns>
        public static AForge.Imaging.Blob[] ExtractBlobs(this Bitmap image)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Blob extractor can be applied to binary 8bpp images only");

            AForge.Imaging.BlobCounter bc = new AForge.Imaging.BlobCounter(image);

            return bc.GetObjects(image, true);
        }

        /// <returns>Invert image</returns>
        public static Bitmap Invert(this Bitmap image)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Operation can be applied to binary 8bpp images only");

            Invert filter = new Invert();
            // apply the filter
            return filter.Apply(image);
        }

        #region Aforge Global Binarizations
        /// <summary>
        /// Binarize image with threshold filter
        /// </summary>
        /// <param name="image"></param>
        /// <param name="threshold">threshold for binarization</param>
        public static Bitmap Threshold(this Bitmap image, byte threshold)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Filter can be applied to binary 8bpp images only");

            Threshold thresholdFilter = new Threshold(threshold);
            return thresholdFilter.Apply(image);
        }

        /// <summary>
        /// Binarize image with SIS threshold filter
        /// </summary>
        /// <param name="image"></param>
        public static Image SisThreshold(this Bitmap image)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Filter can be applied to binary 8bpp images only");

            SISThreshold thresholdFilter = new SISThreshold();
            return thresholdFilter.Apply(image);
        }

        /// <summary>
        /// Binarize image with Otsu threshold filter
        /// </summary>
        /// <param name="image"></param>
        public static Image OtsuThreshold(this Bitmap image)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Filter can be applied to binary 8bpp images only");

            OtsuThreshold thresholdFilter = new OtsuThreshold();
            return thresholdFilter.Apply(image);
        }

        /// <summary>
        /// Binarize image with iterative threshold filter
        /// </summary>
        /// <param name="image"></param>
        /// <param name="threshold">threshold for binarization</param>
        public static Image IterativeThreshold(this Bitmap image, byte threshold)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Filter can be applied to binary 8bpp images only");

            IterativeThreshold thresholdFilter = new IterativeThreshold(2, threshold);
            return thresholdFilter.Apply(image);
        }
        #endregion

        #region Aforge Local Binarizations
        /// <summary>
        /// Binarize image with Niblack filter
        /// </summary>
        /// <param name="image"></param>
        /// <param name="k"></param>
        /// <param name="radius">radius of local area of pixel </param>
        /// <returns></returns>
        public static Bitmap NiblackBinarization(this Bitmap image, double k, int radius)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Filter can be applied to binary 8bpp images only");
            var threshold = new NiblackThreshold() { K = k, Radius = radius };
            return threshold.Apply(image);
        }

        /// <summary>
        /// Binarize image with Sauvola filter
        /// </summary>
        /// <param name="image"></param>
        /// <param name="k"></param>
        /// <param name="radius">radius of local area of pixel </param>
        /// <returns></returns>
        public static Bitmap SauvolaBinarization(this Bitmap image, double k, int radius)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Filter can be applied to binary 8bpp images only");
            var threshold = new SauvolaThreshold() { K = k, Radius = radius };
            return threshold.Apply(image);
        }

        /// <summary>
        /// Binarize image with WolfJoulion filter
        /// </summary>
        /// <param name="image"></param>
        /// <param name="k"></param>
        /// <param name="radius">radius of local area of pixel </param>
        /// <returns></returns>
        public static Bitmap WolfJoulionBinarization(this Bitmap image, double k, int radius)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Filter can be applied to binary 8bpp images only");
            var threshold = new WolfJolionThreshold { K = k, Radius = radius };
            return threshold.Apply(image);
        }
        #endregion

        /// <summary>
        /// 
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

        #region Help functions for contours
        public static List<Contour> FindContours(this Bitmap image)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Countour extractor can be applied to binary 8bpp images only");

            BlobCounter bc = new BlobCounter(image);
            Blob[] blobs = bc.GetBlobs(image, true);

            return blobs.Select(blob => new Contour(bc.GetBlobsContourPoints<SqareTracer>(blob))).ToList();
        }

        public static List<BlobEntity> FindBlobs(this Bitmap image)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Countour extractor can be applied to binary 8bpp images only");

            BlobCounter bc = new BlobCounter(image);
            Blob[] blobs = bc.GetBlobs(image, true);

            return blobs.Select(blob => new BlobEntity(blob, new Contour(bc.GetBlobsContourPoints<SqareTracer>(blob)))).ToList();
        }
        #endregion

        #region OtherAforgeFilters
        /// <summary>
        /// Fast Implementation of Kuwahara filter. Work only with grayscale See https://en.wikipedia.org/wiki/Kuwahara_filter
        /// </summary>
        /// <param name="image"></param>
        /// <param name="kernelSize">Kuwahara kernel size</param>
        public static Bitmap GrayscaleKuwahara(this Bitmap image, int kernelSize)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Filter can be applied to binary 8bpp images only");

            Kuwahara kuwahara = new Kuwahara { Size = kernelSize };
            return kuwahara.Apply(image);
        }
        #endregion

        #region hand-made filters
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
        #endregion
    }
}
