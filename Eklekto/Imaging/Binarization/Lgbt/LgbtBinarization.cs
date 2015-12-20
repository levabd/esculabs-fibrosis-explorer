using System.Threading.Tasks;

namespace Eklekto.Imaging.Lgbt
{
    public class LgbtBinarization
    {
        private readonly double _k;
        private readonly int _localRadius;
        private readonly int _globalRadius;
        private readonly byte _globalThreshold;

        public LgbtBinarization(double k, int localRadius, int globalRadius, byte globalThreshold)
        {
            _k = k;
            _localRadius = localRadius;
            _globalRadius = globalRadius;
            _globalThreshold = globalThreshold;
        }

        public SimpleGrayImage Apply(SimpleGrayImage image)
        {
            //SimpleGrayImage source =  image;
            SimpleGrayImage source = new SimpleGrayImage(image.Data);
            /*Parallel.For(0, image.Rows, columnCounter =>
            {
                for (int rowCounter = 0; rowCounter < image.Cols; rowCounter++)
                {
                    //LgbtPixelProcessing pixelProcessor = new LgbtPixelProcessing(rowCounter, columnCounter, image, k, localRadius, globalRadius, globalThreshold);
                    //image.Data[columnCounter, rowCounter] = pixelProcessor.CalculatedPixelValue();
                    LgbtPixelProcessing pixelProcessor = new LgbtPixelProcessing(rowCounter, columnCounter, source, k, localRadius, globalRadius, globalThreshold);
                    image.Data[columnCounter, rowCounter] = pixelProcessor.CalculatedPixelValue();
                }
            }
            );
            */

            var syncTask = new Task(() =>
            {
                Parallel.For(0, image.Rows, j =>
                {
                    for (int i = 0; i < image.Cols; i++)
                    {
                        //LgbtPixelProcessing pixelProcessor = new LgbtPixelProcessing(rowCounter, columnCounter, image, k, localRadius, globalRadius, globalThreshold);
                        //image.Data[columnCounter, rowCounter] = pixelProcessor.CalculatedPixelValue();
                        LgbtPixelProcessing pixelProcessor = new LgbtPixelProcessing(j, i, source, _k, _localRadius, _globalRadius, _globalThreshold);
                        image.Data[j, i] = pixelProcessor.CalculatedPixelValue();
                    }
                });
            });
            syncTask.RunSynchronously();

            return image;
            
        }
    }
}
