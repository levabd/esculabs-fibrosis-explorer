using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using Accord.Imaging;
using AForge;
using Eklekto.Geometry;
using Eklekto.Imaging.Lgbt;

namespace Eklekto.Imaging
{
    public class SimpleGrayImage
    {
        public const byte WhiteBrightness = 255;
        public const byte BlackBrightness = 0;
        public const byte GrayBrightness = 128;

        public byte[,] Data;

        #region Properties
        public int Cols => Data.GetLength(1);

        public int Rows => Data.GetLength(0);
        #endregion

        #region Constructors
        public SimpleGrayImage()
        {
            Data = new byte[0, 0];
        }

        public SimpleGrayImage(byte[,] data)
        {
            Data = new byte[data.GetLength(0), data.GetLength(1)];
            Array.Copy(data, Data, data.Length);
        }

        public SimpleGrayImage(Bitmap image)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new BadImageFormatException("only grayscale bitmap accepted");
            int width = image.Width;
            int height = image.Height;

            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = image.LockBits(rect, ImageLockMode.ReadWrite, image.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            int bytes = Math.Abs(bmpData.Stride) * height;
            byte[] oneDimensionalData = new byte[bytes];

            Marshal.Copy(ptr, oneDimensionalData, 0, bytes);

            image.UnlockBits(bmpData);
            Data = new byte[height, width];
            int count = 0;

            for (int column = 0; column < height; column++)
            {
                for (int row = 0; row < bmpData.Stride; row++)
                {
                    if (row < width - 1) //Bitmap allow bytes array width only dividible by 4
                        Data[column, row] = oneDimensionalData[count++];
                    else                 //So here workaround it
                        count++;
                }
            }
        }
        #endregion

        #region Help Functions
#pragma warning disable 618
        public Bitmap Bitmap => Data.ToBitmap();
#pragma warning restore 618

        public Bitmap NativeBitmap()
        {
            var b = new Bitmap(Cols, Rows, PixelFormat.Format8bppIndexed);
            ColorPalette ncp = b.Palette;
            for (int i = 0; i < 256; i++)
                ncp.Entries[i] = Color.FromArgb(255, i, i, i);
            b.Palette = ncp;
            BitmapData bmpData = b.LockBits(new Rectangle(0, 0, Cols, Rows), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            IntPtr ptr = bmpData.Scan0;
            int stride = bmpData.Stride;

            //Fill bitmap bytes
            int bytes = stride * Rows;
            byte[] rgbValues = new byte[bytes];
            int count = 0;
            for (int column = 0; column < bmpData.Height; column++)
            {
                for (int row = 0; row < stride; row++)
                {
                    if (row > Cols - 1)            //Bitmap allow bytes array width only dividible by 4
                        rgbValues[count++] = 0;    //So here workaround it
                    else
                        rgbValues[count++] = Data[column, row];
                }
            }

            Marshal.Copy(rgbValues, 0, ptr, rgbValues.Length);
            b.UnlockBits(bmpData);
            return b;
        }

        public void FillBlob(Blobs.Blob blob, byte color = BlackBrightness)
        {
            for (int y = 0; y < blob.Rectangle.Height; y++)
            {
                for (int x = 0; x < blob.Rectangle.Width; x++)
                {
                    if (blob.Object[y * blob.Rectangle.Width + x])
                        Data[y + blob.Rectangle.Y, +blob.Rectangle.X + x] = color;
                }
            }
        }

        /// <summary>
        /// Faster method to fill polygon. We dont canculate polygon rectangle
        /// </summary>
        /// <param name="points">Polygon</param>
        /// <param name="rectangle">Polygon rectangle</param>
        /// <param name="color">Color to fill</param>
        public void FillPolygon(List<IntPoint> points, Rectangle rectangle, byte color = BlackBrightness)
        {
            Parallel.For(rectangle.Y, rectangle.Height + rectangle.Y, y =>
            {
                for (int x = rectangle.X; x < rectangle.Width + rectangle.X; x++)
                {
                    if (points.IsPointInside(new IntPoint(x, y)))
                        Data[y, x] = color;
                }
            });
        }

        /// <summary>
        /// Fill polygon using Ray casting algorithm. http://rosettacode.org/wiki/Ray-casting_algorithm
        /// </summary>
        /// <param name="points">Polygon</param>
        /// <param name="color">Color to fill</param>
        public void FillPolygon(List<IntPoint> points, byte color = BlackBrightness)
        {
            int minx = points.Min(coordinate => coordinate.X);
            int miny = points.Min(coordinate => coordinate.Y);
            int maxx = points.Max(coordinate => coordinate.X);
            int maxy = points.Max(coordinate => coordinate.Y);
            FillPolygon(points, new Rectangle(minx, miny, maxx - minx + 1, maxy - miny + 1), color);
        }


        //Simple Drawing on Bitmap
        //PointsMarker marker = new PointsMarker(contour, Color.DarkGray);
        //return marker.Apply(Bitmap);
        public void DrawPolygon(List<IntPoint> points, byte brightness = GrayBrightness, int weight = 3)
        {
            points.ForEach(point =>
            {
                for (int pointWeight = 0; pointWeight < weight; pointWeight++)
                {
                    int currentWeightIndex = pointWeight - weight/2;
                    if ((point.X + currentWeightIndex > 0) && (point.X + currentWeightIndex < Cols))
                        Data[point.Y, point.X + currentWeightIndex] = brightness;
                }
            });
        }



        // ReSharper disable once RedundantAssignment
        // We create new Image every time
        public void CopyTo(ref SimpleGrayImage newSimpleGrayImage)
        {
            byte[,] data = new byte[Data.GetLength(0), Data.GetLength(1)];
            Array.Copy(Data, data, Data.Length);
            newSimpleGrayImage = new SimpleGrayImage(data);
        }

        #endregion

        #region Filters

        /// <summary>
        /// Fast threshold binarization. Doest require bitmap convertion
        /// </summary>
        /// <param name="threshold"></param>
        public void ApplyBinarization(byte threshold)
        {
            for (int y = 0; y < Rows; y++)
                for (int x = 0; x < Cols; x++)
                {
                    byte br = Data[y, x];
                    if (br > threshold)
                        Data[y, x] = WhiteBrightness;
                    else
                        Data[y, x] = BlackBrightness;
                }
        }

        public void ApplyLgbtBinarization(double k, int localRadius, int globalRadius, byte globalThreshold)
        {
            LgbtBinarization lgbtBinarization = new LgbtBinarization(k, localRadius, globalRadius, globalThreshold);
            Data = lgbtBinarization.Apply(this).Data;
        }

        #endregion

        #region line drawing
        /// <summary>
        /// Draw line with width 2
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="brightness">brightness of line</param>
        public void DrawGrayLine(IntPoint startPoint, IntPoint endPoint, byte brightness)
        {
            int dx = startPoint.X - endPoint.X;
            int dy = startPoint.Y - endPoint.Y;

            if ((dx == 0) && (dy == 0))
                return;
            if (Math.Abs(dx) > Math.Abs(dy))
            //y=ax+b
            {
                int minX = Math.Min(startPoint.X, endPoint.X);
                minX = Math.Max(minX, 0);
                int maxX = Math.Max(startPoint.X, endPoint.X);
                maxX = Math.Min(maxX, Cols - 1);
                double a = (double)dy / dx;
                double b = startPoint.Y - a * startPoint.X;
                for (int x = minX; x <= maxX; x++)
                {
                    int y = (int)(a * x + b);
                    if ((y - 1 < Rows)&&(y > 0))
                        Data[y - 1, x] = brightness;
                    if (y < Rows)
                        Data[y, x] = brightness;
                    if (y + 1 < Rows)
                        Data[y + 1, x] = brightness;
                }
            }
            else
            //x=ay+b
            {
                int minY = Math.Min(startPoint.Y, endPoint.Y);
                int maxY = Math.Max(startPoint.Y, endPoint.Y);
                maxY = Math.Min(maxY, Rows-1);
                minY = Math.Max(minY, 0);
                double a = (double)dx / dy;
                double b = startPoint.X - a * startPoint.Y;
                for (int y = minY; y <= maxY; y++)
                {
                    int x = (int)(a * y + b);
                    //new
                    if ((x -1 < Cols) && (x - 1 >= 0))
                        Data[y, x - 1] = brightness;
                    //end new
                    if ((x < Cols) && (x >= 0))
                        Data[y, x] = brightness;
                    if ((x + 1 < Cols) && (x + 1 >= 0)) 
                        Data[y, x + 1] = brightness;
                }
            }

        }

        /// <summary>
        /// Draw line with width 2
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="brightness">brightness of line</param>
        public void DrawHorisontalGrayLine(int startX, int endX, int y, byte brightness)
        {
            for (int x = startX; x <= endX; x++)
                Data[y, x] = brightness;
        }

        public void DrawVerticalGrayLine(int startY, int endY, int x, byte brightness)
        {
            for (int y = startY; y <= endY; y++)
                Data[y, x] = brightness;
        }
        #endregion
    }
}
