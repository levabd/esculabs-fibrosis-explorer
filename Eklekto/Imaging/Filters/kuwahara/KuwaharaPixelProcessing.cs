using System.Drawing;

namespace Eklekto.Imaging.Filters.Kuwahara
{
    class KuwaharaPixelProcessing
    {
        private readonly SimpleGrayImage _image;
        readonly int _x;
        readonly int _y;
        private readonly int _kernel;

        public KuwaharaPixelProcessing(int x, int y, SimpleGrayImage image, int kernelSize = 2)
        {
            _x = x;
            _y = y;
            _image = image;
            _kernel = kernelSize;
        }

        public byte CalculatedPixelValue()
        {
            KuwaharaRegion[] region = new KuwaharaRegion[4];
            region[0] = GetTopLeftRegion();
            region[1] = GetTopRightRegion();
            region[2] = GetBottomLeftRegion();
            region[3] = GetBottomRightRegion();

            //Find Region with max variation
            int minVariationIndex = 0;
            double minVariation = region[0].Variance();
            for (int i = 1; i < 4; i++)
            {
                double variation = region[i].Variance();
                if (variation < minVariation)
                {
                    minVariationIndex = i;
                    minVariation = variation;
                }
            }

            return (byte)region[minVariationIndex].Estimation();
        }

        private KuwaharaRegion GetTopLeftRegion()
        {
            int regionX = (_x >= _kernel - 1) ? _x - (_kernel - 1) : 0;
            int regionY = (_y >= _kernel - 1) ? _y - (_kernel - 1) : 0;
            return new KuwaharaRegion(new Point(_x, _y), new Point(regionX, regionY), _image);
        }

        private KuwaharaRegion GetTopRightRegion()
        {
            int regionX = (_x + _kernel - 1 < _image.Cols) ? _x + (_kernel - 1) : _image.Cols - 1;
            int regionY = (_y >= _kernel - 1) ? _y - (_kernel - 1) : 0;
            return new KuwaharaRegion(new Point(_x, _y), new Point(regionX, regionY), _image);
        }

        private KuwaharaRegion GetBottomLeftRegion()
        {
            int regionX = (_x >= _kernel - 1) ? _x - (_kernel - 1) : 0;
            int regionY = (_y + (_kernel - 1) < _image.Rows) ? _y + (_kernel - 1) : _image.Rows - 1;
            return new KuwaharaRegion(new Point(_x, _y), new Point(regionX, regionY), _image);
        }

        private KuwaharaRegion GetBottomRightRegion()
        {
            int regionX = (_x + _kernel - 1 < _image.Cols) ? _x + (_kernel - 1) : _image.Cols - 1;
            int regionY = (_y + (_kernel - 1) < _image.Rows) ? _y + (_kernel - 1) : _image.Rows - 1;
            return new KuwaharaRegion(new Point(_x, _y), new Point(regionX, regionY), _image);
        }
    }

}
