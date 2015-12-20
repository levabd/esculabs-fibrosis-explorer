using System;
using System.Collections.Generic;
using System.Linq;
using AForge;
using Eklekto.Approximators;
using Eklekto.Geometry;
using Eklekto.Imaging;

namespace FibroscanProcessor.Ultrasound
{
    public class UltrasoundModA
    {
        public SimpleGrayImage Image;
        private int _topLine = 0;
        private int _bottomLine;
        private double _rSquare = -1;
        private double _relativeEstimation = -1;
        private ReflectionedLine _approxLine = null;

        public UltrasoundModA(SimpleGrayImage image, int topIndention, int bottomIndention)
        {
            Image = image;
            _topLine = topIndention;
            _bottomLine = image.Rows - bottomIndention;
        }

        public List<IntPoint> GetGraphicPoints()
        {
            List<IntPoint> graphicPoints = new List<IntPoint>();

            for (int j = _topLine; j < _bottomLine; j++)
            {
                if (graphicPoints.Count == 0)
                {
                    for (int i = 1; i < Image.Cols - 1; i++)
                        if (Image.Data[j, i] < 50)
                            graphicPoints.Add(new IntPoint(i, j));
                    continue;
                }
                int lastX = graphicPoints.Last().X;
                int maxDistanceIndex = lastX;
                int maxDistanceValue = 0;
                for (int i = 1; i < Image.Cols - 1; i++)
                {
                    if ((Image.Data[j, i] < 50) && (Math.Abs(lastX - i) > maxDistanceValue))
                    {
                        maxDistanceIndex = i;
                        maxDistanceValue = Math.Abs(lastX - i);
                    }
                }
                graphicPoints.Add(new IntPoint(maxDistanceIndex,j));
            }
            return graphicPoints;
        }

        private void Approximation()
        {
            LinearLeastSquares approx = new LinearLeastSquares(GetGraphicPoints());
            _rSquare = approx.RSquares;
            _approxLine = approx.Line;
            _relativeEstimation = approx.RelativeEstimation;
        }

        public double RSquare
        {
            get
            {
                if (_rSquare < 0)
                    Approximation();
                return _rSquare;
            }
        }

        public double RelativeEstimation
        {
            get
            {
                if (_relativeEstimation<0)
                    Approximation();
                return _relativeEstimation;
            }
        }

        public ReflectionedLine ApproxLine
        {
            get
            {
                if (_approxLine == null)
                    Approximation();
                return _approxLine;
            }
        }
    }
}
