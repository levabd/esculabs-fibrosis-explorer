using System;
using System.Drawing;
using System.Drawing.Imaging;
using AForge.Imaging.Filters;

namespace Eklekto.Imaging
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

        /// <returns>Cropped image</returns>
        public static Bitmap Crop(this Image image, Rectangle rectangle)
        {
            Crop cropper = new Crop(rectangle);
            return cropper.Apply(new Bitmap(image));
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
    }
}
