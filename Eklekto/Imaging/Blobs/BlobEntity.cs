using Eklekto.Imaging.Contours;

namespace Eklekto.Imaging.Blobs
{
    public class BlobEntity
    {
        public Blob Blob { get; }

        public Contour Contour { get; }

        public BlobEntity(Blob blob, Contour contour)
        {
            Blob = blob;
            Contour = contour;
        }
    }
}
