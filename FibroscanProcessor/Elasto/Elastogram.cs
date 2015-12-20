using System;
using System.Drawing;
using System.Collections.Generic;
using AForge;
using Eklekto.Geometry;
using Eklekto.Imaging;
using Eklekto.Imaging.Blobs;
using Point = System.Drawing.Point;
using System.Threading.Tasks;
using AForge.Imaging.Filters;

namespace FibroscanProcessor.Elasto
{
    public class Elastogram
    {
        private const int FibroLineColor = 230;

        private ElastoBlob _targetObject;

        private Segment _fibroLine;

        public SimpleGrayImage Image;

        public ElastoBlob TargetObject => _targetObject;

        public Segment Fibroline => _fibroLine;

        public Elastogram(SimpleGrayImage image)
        {
            Image = image;
        }

        public void PaintOverFibroline()
        {
            if (_fibroLine == null)
                throw new InvalidOperationException("Get fibroline first.");

            for (int y = _fibroLine.Top.Y; y <= _fibroLine.Bottom.Y; y++)
                for (int x = 1; x < Image.Cols-3; x++)
                {
                    if (Image.Data[y, x] > FibroLineColor)
                    {
                        Image.Data[y, x] = Image.Data[y, x - 1];
                        if (Image.Data[y, x + 1] > FibroLineColor)
                            Image.Data[y, x + 1] = Image.Data[y, x + 3];
                        if (Image.Data[y, x + 2] > FibroLineColor)
                            Image.Data[y, x + 2] = Image.Data[y, x + 3];
                    }
                }
        }

        public void GetFibroLine()
        {
            Point startPoint = new Point();
            Point endPoint = new Point();
            bool fibroLineTopPointFounded = false;

            for (int y = 0; y < Image.Rows; y++)
                for (int x = 0; x < Image.Cols; x++)
                {
                    int br = Image.Data[y, x];
                    if (br > FibroLineColor)
                    {
                        if (!fibroLineTopPointFounded)
                        {
                            startPoint = new Point(x, y);
                            fibroLineTopPointFounded = true;
                        }
                        else
                        {
                            endPoint = new Point(x, y);
                        }
                    }
                }
            _fibroLine = new Segment(startPoint, endPoint);
        }

        public void RemoveEdgeObjects(int leftDist1, int leftCentralDist1, int leftDist2, int leftCentralDist2,
            int rightDist, int rightCentralDist)
        {
            Bitmap result = Image.Bitmap.Invert();
            List<BlobEntity> objects = result.FindBlobs();
            objects.ForEach(currentObject =>
            {
                bool isLeftObject1 = false;
                bool isCentralLeftObject1 = false;
                bool isLeftObject2 = false;
                bool isCentralLeftObject2 = false;
                bool isRightObject = false;
                bool isCentralRightObject = false;
                currentObject.Contour.Points.ForEach(currentPoint =>
                {
                    int currentX = currentPoint.X;

                    if (currentX < leftDist1)
                        isLeftObject1 = true;
                    if (currentX > leftCentralDist1)
                        isCentralLeftObject1 = true;

                    if (currentX < leftDist2)
                        isLeftObject2 = true;
                    if (currentX > leftCentralDist2)
                        isCentralLeftObject2 = true;

                    if (Image.Cols - currentX < rightDist)
                        isRightObject = true;
                    if (Image.Cols - currentX > rightCentralDist)
                        isCentralRightObject = true;
                });
                bool isDrawing = ((!isLeftObject1 || isCentralLeftObject1) && (!isLeftObject2 || isCentralLeftObject2) &&
                                  (!isRightObject || isCentralRightObject));
                Image.FillBlob(currentObject.Blob,
                    isDrawing ? SimpleGrayImage.BlackBrightness : SimpleGrayImage.WhiteBrightness);
                
            });
        }

        public void CropObjects(int step, int cropDistance)
        {
            Bitmap result = Image.Bitmap.Invert();
            List<BlobEntity> objects = result.FindBlobs();

            foreach (BlobEntity blob in objects)
            {
                int objectSize = blob.Contour.Points.Count;
                if (objectSize < 2 * step)
                    return;

                //fix one point
                var syncTask = new Task(() =>
                {
                    System.Threading.Tasks.Parallel.For(0, objectSize, i =>
                    {
                        IntPoint startPoint = blob.Contour.Points[i];
                            //for each check one other point
                            for (int j = i; j < objectSize; j++)
                        {
                            int circleDist = Math.Min(j - i, objectSize - (j - i));
                            if (circleDist >= step)
                            {
                                IntPoint endPoint = blob.Contour.Points[j];
                                int distance =
                                    (int)
                                        Math.Sqrt(Math.Pow((startPoint.X - endPoint.X), 2) +
                                                    Math.Pow((startPoint.Y - endPoint.Y), 2));
                                if ((distance < cropDistance)) //optimisation equal
                                    Image.DrawGrayLine(startPoint, endPoint, 255);
                            }
                        }
                    });
                });

                syncTask.RunSynchronously();
            }
        }

        public void ChooseContour(double areaProportion, int areaMinLimit, double heightPropotion)
        {
            Bitmap result = Image.Bitmap.Invert();
            List<BlobEntity> objects = result.FindBlobs();

            int maxArea = -1;
            int maxHeight = -1;
            // ReSharper disable PossibleLossOfFraction
            AForge.Point imageCenter = new AForge.Point(result.Width/2, result.Height/2);
            int targetObjectIndex = -1; //

            for(int i =0; i<objects.Count;i++)
            {
                int currentArea = objects[i].Blob.Area;
                if (currentArea > maxArea)
                {
                    maxArea = currentArea;
                    targetObjectIndex = i;
                }
                int currentHeight = objects[i].Blob.Rectangle.Height;
                if (currentHeight > maxHeight)
                    maxHeight = currentHeight;
            };

            int limitArea = Math.Min((int) (maxArea*areaProportion), areaMinLimit);
            int limitHeight = (int) (maxHeight*heightPropotion);
            double minDistanceToCenter = result.Height + result.Width; //Triangle inequality:)

            for (int i = 0; i < objects.Count; i++)
            {
                if ((objects[i].Blob.Area >= limitArea) && (objects[i].Blob.Rectangle.Height >= limitHeight))
                {
                    AForge.Point objectCenter = objects[i].Blob.CenterOfGravity;
                    double distToCenter =
                        Math.Sqrt(Math.Pow(objectCenter.X - imageCenter.X, 2) +
                                  Math.Pow(objectCenter.X - imageCenter.X, 2));
                    if (distToCenter < minDistanceToCenter)
                    {
                        minDistanceToCenter = distToCenter;
                        targetObjectIndex = i;
                    }
                }
            }
            _targetObject = new ElastoBlob(objects[targetObjectIndex]);

            for (int i = 0; i < objects.Count; i++)
                if (i != targetObjectIndex)
                    Image.FillBlob(objects[i].Blob, SimpleGrayImage.WhiteBrightness);
        }
    }
}
