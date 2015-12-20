using System.Threading.Tasks;

namespace Eklekto.Imaging.Filters.Kuwahara
{
    class KuwaharaFilter
    {
        private readonly int _kernel;

        public KuwaharaFilter(int kernelSize = 2)
        {
            _kernel = kernelSize;
        }

        public SimpleGrayImage Apply(SimpleGrayImage image)
        {
            SimpleGrayImage source = image;
            var syncTask = new Task(() =>
            {
                Parallel.For(0, image.Rows, columnCounter =>
                {
                    for (int rowCounter = 0; rowCounter < image.Cols; rowCounter++)
                    {
                        KuwaharaPixelProcessing pixelProcessor = new KuwaharaPixelProcessing(rowCounter, columnCounter, source, _kernel);
                        image.Data[columnCounter, rowCounter] = pixelProcessor.CalculatedPixelValue();
                    }
                });
            });

            syncTask.RunSynchronously();

            return image;
        }

        
    }

}
