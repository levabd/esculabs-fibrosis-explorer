using System;
using System.Drawing;

namespace Eklekto.Imaging.Filters.Kuwahara
{ 
    class KuwaharaRegion
    {
        private readonly int _minX;
        private readonly int _maxX;
        private readonly int _minY;
        private readonly int _maxY;
        private readonly int _numberOfElements;

        private double _estimation = -1;

        private readonly SimpleGrayImage _image;
        
        public KuwaharaRegion(Point centerPoint, Point regionPoint, SimpleGrayImage image)
        {
            _minX = Math.Min(centerPoint.X, regionPoint.X);
            _maxX = Math.Max(centerPoint.X, regionPoint.X);
            _minY = Math.Min(centerPoint.Y, regionPoint.Y);
            _maxY = Math.Max(centerPoint.Y, regionPoint.Y);
            _image = image;
            _numberOfElements = (_maxX - _minX + 1) * (_maxY - _minY + 1);
        }
        
        public double Estimation()
        {
            if (_estimation > -1)
                return _estimation;

            long sum = 0;
            for (int j = _minY; j <= _maxY; j++)
                for (int i = _minX; i <= _maxX; i++)
                    sum += _image.Data[j, i];
            _estimation = (double) sum/_numberOfElements;
            return _estimation;
        }

        public double Variance()
        {
            double var = 0;
            double estimation = Estimation();
            for (int j = _minY; j <= _maxY; j++)
                for (int i = _minX; i <= _maxX; i++)
                    var += Math.Pow(_image.Data[j, i] - estimation, 2);
            var = var / _numberOfElements;
            return var;
        }
    }

}
