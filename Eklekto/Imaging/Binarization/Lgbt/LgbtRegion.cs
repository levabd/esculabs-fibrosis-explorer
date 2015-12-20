using System;
using System.Drawing;

namespace Eklekto.Imaging.Lgbt
{
    class LgbtRegion
    {
        private readonly int _minX;
        private readonly int _maxX;
        private readonly int _minY;
        private readonly int _maxY;
        private readonly int _numberOfElements;

        private double _estimation = -1;

        private readonly SimpleGrayImage _image;

        public LgbtRegion(Point centerPoint, int radius, SimpleGrayImage image)
        {
            _image = image;
            //_image = new SimpleGrayImage(image.Mat);

            _minX = Math.Max(centerPoint.X - radius, 0);
            _maxX = Math.Min(centerPoint.X + radius, _image.Cols-1);
            _minY = Math.Max(centerPoint.Y - radius, 0);
            _maxY = Math.Min(centerPoint.Y + radius, _image.Rows-1);
            _numberOfElements = (_maxX - _minX + 1) * (_maxY - _minY + 1);
        }

        public byte DarkestValue//may be optimized
        {
            get {
                byte tempDarkest = 255;
                for (int j = _minY; j <= _maxY; j++)
                    for (int i = _minX; i <= _maxX; i++)
                        if (_image.Data[j, i] < tempDarkest)
                            tempDarkest = _image.Data[j,i];
                return tempDarkest;
            }
        }

        public double Expectation()
        {
            if (_estimation > -1)
                return _estimation;

            long sum = 0;
            for (int j = _minY; j <= _maxY; j++)
                for (int i = _minX; i <= _maxX; i++)
                    sum += _image.Data[j, i];
            _estimation = (double)sum / _numberOfElements;
            return _estimation;
        }

        public double Variance()
        {
            double var = 0;
            double estimation = Expectation();
            for (int j = _minY; j <= _maxY; j++)
                for (int i = _minX; i <= _maxX; i++)
                    var += Math.Pow(_image.Data[j, i] - estimation, 2);
            var = var / _numberOfElements;
            return var;
        }
    }
}
