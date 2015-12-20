using System;
using System.Collections.Generic;
using AForge;
using Eklekto.Geometry;

namespace Eklekto.Approximators
{
    public class LinearLeastSquares
    {
        public ReflectionedLine Line;

        private readonly List<IntPoint> _sourcePoints; 
        private long _sumY;
        private long _sumX;
        private long _sumXy;
        private long _sumYy;

        private readonly int _n;

        public LinearLeastSquares(List<IntPoint> sourcePoints)
        {
            _sourcePoints = sourcePoints;
            _n = _sourcePoints.Count;
            sourcePoints.ForEach(point =>
            {
                _sumX += point.X;
                _sumY += point.Y;
                _sumXy += point.X * point.Y;
                _sumYy += (long) Math.Pow(point.Y, 2);
            });
            double a = (_n * _sumXy - _sumX * _sumY) / (_n * _sumYy - Math.Pow(_sumY, 2));
            double b = (_sumX - a * _sumY) / _n;
            Line = new ReflectionedLine(a, b);
        }


        public double RelativeEstimation
        {
            get
            {
                //_sourcePoints.ForEach(point=> estimation += Math.Abs(point.X - Line.GetX(point.Y)) / point.X);
                double numerator = 0;
                double denominator = 0;
                for (int i = 0; i < _sourcePoints.Count; i++)
                {
                    numerator += Math.Abs(_sourcePoints[i].X - Line.GetX(_sourcePoints[i].Y));
                    denominator += _sourcePoints[i].X;
                }
                return (1- numerator / denominator) * 100;
            }
        }

        public double RSquares
        {
            get
            {
                if (_n == 0)
                    return -1 * Double.MaxValue;
                // ReSharper disable once PossibleLossOfFraction
                double xMean = _sumX/_n;
                double numerator = 0;
                double denominator = 0;
                _sourcePoints.ForEach(point =>
                {
                    numerator += Math.Pow(point.X - Line.GetX(point.Y), 2);
                    denominator += Math.Pow(point.X - xMean, 2);
                });
                return Math.Max(0, 1 - numerator / denominator) *100;
            }
        }
    }
}
