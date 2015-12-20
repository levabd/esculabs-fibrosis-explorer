using System;

namespace Eklekto.Imaging.Lgbt
{
    class LgbtPixelProcessing
    {
        private readonly SimpleGrayImage _image;
        readonly int _x;
        readonly int _y;
        private readonly double _k;
        private readonly int _localRadius;
        private readonly int _globalRadius;
        private readonly int _globalThreshold;

        public LgbtPixelProcessing (int y, int x, SimpleGrayImage image, double k, int localRadius, int globalRadius, byte globalThreshold)
        {
            _x = x;
            _y = y;
            _k = k;

            _image = image;
            //_image = new SimpleGrayImage(image.Mat);

            _localRadius = localRadius;
            _globalRadius = globalRadius;
            _globalThreshold = globalThreshold;
        }

        public byte CalculatedPixelValue()
        {
            LgbtRegion globalRegion = new LgbtRegion(new System.Drawing.Point(_x, _y), _globalRadius, _image);

            if (globalRegion.DarkestValue >= _globalThreshold)
                return 255;
            
            LgbtRegion localRegion = new LgbtRegion(new System.Drawing.Point(_x, _y), _localRadius, _image);

            //Niblack
            double localThreshold = localRegion.Expectation() + _k * Math.Sqrt(localRegion.Variance());
            if (_image.Data[_y, _x] > localThreshold)
                return 255;

            return 0;
        }
    }
}

