using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AForge;
using Eklekto.Geometry;

namespace Eklekto.Approximators
{
    public class Ransac
    {
        public ReflectionedLine Line;

        private List<IntPoint> _sourcePoints;
        private int _sourcePointsCount;
        private readonly double _sampleShare;
        private readonly double _outlierShare;
        private readonly int _iterations;

        public Ransac(double sampleShare, double outlierShare, int iterations)
        {
            _sampleShare = sampleShare;
            _outlierShare = outlierShare;
            _iterations = iterations;
        }

        public ReflectionedLine Approximate(List<IntPoint> sourcePoints, int sourcePointCount, out double rSquares,
            out double relativeEstimation)
        {
            _sourcePoints = sourcePoints;
            _sourcePointsCount = sourcePointCount;
            LinearLeastSquares approximationOfRansac = new LinearLeastSquares(GetRansacPoints());
            rSquares = approximationOfRansac.RSquares;
            relativeEstimation = approximationOfRansac.RelativeEstimation;
            return approximationOfRansac.Line;
        }

        private List<IntPoint> GetSamplePoints(Random rnd, int sampleNum)
        {
            HashSet<IntPoint> candidates = new HashSet<IntPoint>();
            //Random rnd = new Random();
            while (candidates.Count < sampleNum)
                candidates.Add(_sourcePoints[rnd.Next(0, _sourcePointsCount - 1)]);
            List<IntPoint> result = new List<IntPoint>();
            result.AddRange(candidates);
            return result;
            
            /* //faster
            HashSet<int> indexes = new HashSet<int>();
            List<IntPoint> choices = new List<IntPoint>();
            while (indexes.Count < sampleNum)
                indexes.Add(rnd.Next(0, _sourcePoints.Count - 1));
            List<IntPoint> result = new List<IntPoint>();
            //result.AddRange(_sourcePoints[indexes]);
            indexes.
            */    
        }

        private List<IntPoint> GetInliers(ReflectionedLine line, int inlierNum)
        {
            List<double> estimations = new List<double>();
            _sourcePoints.ForEach(
                sourcePoint => estimations.Add(Math.Pow(sourcePoint.X - line.GetX(sourcePoint.Y), 2)));

            List<double> orderedEstimations = estimations.OrderBy(x => x).ToList();
            double estimationThreshold = orderedEstimations[inlierNum];
            List<IntPoint> inliers = new List<IntPoint>();
            for (int i = 0; i < _sourcePointsCount; i++)
                if (estimations[i] < estimationThreshold)
                    inliers.Add(_sourcePoints[i]);
            return inliers;
        }

        private List<IntPoint> GetRansacPoints()
        {
            List<IntPoint> ransacPoints = new List<IntPoint>();
            double bestRSquare = -1 * Double.MaxValue;
            
            var syncTask = new Task(() =>
            {
                System.Threading.Tasks.Parallel.For(0, _iterations, i =>
                {
                    Random rnd = new Random();
                    List<IntPoint> samplePoints = GetSamplePoints(rnd, (int)(_sampleShare * _sourcePointsCount));

                    LinearLeastSquares ols = new LinearLeastSquares(samplePoints);
                    ReflectionedLine approxLine = ols.Line;
                    List<IntPoint> inliers = GetInliers(approxLine, (int) ((1 - _outlierShare)* _sourcePointsCount));

                    //calculate rSquare of inliers with sampleApproxLine
                    int xMean = 0;
                    inliers.ForEach(point => xMean += point.X);
                    xMean = xMean/inliers.Count;

                    double rSquareNumerator = 0;
                    double rSquareDenominator = 0;
                    inliers.ForEach(point =>
                    {
                        rSquareNumerator += Math.Pow(point.X - approxLine.GetX(point.Y), 2);
                        rSquareDenominator += Math.Pow(point.X - xMean, 2);
                    });
                    double rSquare = 1 - rSquareNumerator/rSquareDenominator;
                    if (rSquare > bestRSquare)
                    {
                        bestRSquare = rSquare;
                        ransacPoints = inliers;
                    }
                });
            });
            syncTask.RunSynchronously();
            
            return ransacPoints;
        }

    }
}
