using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AForge.Imaging.Filters;
using Eklekto.Imaging;
using AForge;

namespace FibroscanProcessor.Ultrasound
{
    public class UltrasoundModM
    {
        public SimpleGrayImage Image;

        private int _topLine;
        private int _bottomLine;
        private readonly double[] _deviations;
        private readonly double[] _expectations;
        private readonly int[] _iqr;
        private readonly double _deviationThreshold;
        private readonly int _deviationStreakSize;
        private readonly List<int> _deviationStreakLines;

        public UltrasoundModM(double deviationThreshold, int deviationStreakSize, SimpleGrayImage image, int topIndention, int bottomIndention)
        {
            Image = image;
            _topLine = topIndention;
            _bottomLine = image.Rows - bottomIndention;
            _deviationStreakLines = new List<int>();
            _deviations = new double[image.Rows];
            _expectations = new double[image.Rows];
            _deviationStreakSize = deviationStreakSize;
            _deviationThreshold = deviationThreshold;
            _iqr = new int[Image.Rows];

            SetExpectations();
            SetDeviations();
            SetIqr();
        }

        public List<int> DeviationStreakLines
        {
            get
            {
                if (_deviationStreakLines.Count == 0)
                    CalculateDeviationStreakLines();

                return _deviationStreakLines;
            }
        }

        private void SetExpectations()
        {
            for (int j = _topLine; j < _bottomLine; j++)
            {
                int sum = 0;
                for (int i = 0; i < Image.Cols; i++)
                    sum += Image.Data[j, i];
                _expectations[j] = (double)sum / Image.Cols;
            }
        }

        private void SetDeviations()
        {
            for (int j = _topLine; j < _bottomLine; j++)
            {
                double dev = 0;
                double expect = _expectations[j];
                for (int i = 0; i < Image.Cols; i++)
                    dev += Math.Pow(Image.Data[j, i] - expect, 2);
                _deviations[j] = Math.Sqrt(dev / Image.Cols);
            }
        }

        private void SetIqr()
        {

            for (int j = _topLine; j < _bottomLine; j++)
            {
                List<int>line = new List<int>();
                for (int i = 0; i < Image.Cols; i++)
                    line.Add(Image.Data[j, i]);
                List<int> orderedLine = line.OrderBy(x => x).ToList();
                _iqr[j] = orderedLine[Image.Cols*3/4] - orderedLine[Image.Cols/4];
            }
        }

        public void CalculateDeviationStreakLines()
        {
            int streakCounter = 0;
            for (int j = _topLine; j < _bottomLine; j++)
            {
                if (_deviations[j] >= _deviationThreshold)
                    streakCounter++;
                else if (streakCounter >= _deviationStreakSize)
                {
                    for (int i = j - streakCounter; i < j; i++)
                        _deviationStreakLines.Add(i);
                    streakCounter = 0;
                }
                else
                    streakCounter = 0;
            }
        }

        public List<int> getBadIqrLines(int iqrThreshold)
        {
            List<int> badIqr = new List<int>();
            for (int j = _topLine; j < _bottomLine; j++)
                if (_iqr[j] >= iqrThreshold)
                    badIqr.Add(j);
            return badIqr;
        }

        public List<int> getBrightLines(int brightTreshold, int whiteLimitPixel)
        {
            List<int> whiteLine = new List<int>();
            for (int j = _topLine; j < _bottomLine; j++)
            {
                int whitePixelCount = 0;
                for (int i = 0; i < Image.Cols; i++)
                    if (Image.Data[j, i] >= brightTreshold)
                        whitePixelCount++;
                if (whitePixelCount > whiteLimitPixel)
                    whiteLine.Add(j);
            }
            return whiteLine;
        }

        public List<IntPoint> GetLastColumnPoints()
        {
            List<IntPoint> lastColumn = new List<IntPoint>();
            int lastColumnIndex = Image.Cols - 1;
            for (int j = 0; j < Image.Rows; j++)
                lastColumn.Add(new IntPoint(j, Image.Data[j,lastColumnIndex]));
            return lastColumn;
        }
    }
}
