using System.Drawing;
using System.Collections;

namespace Eklekto.Imaging.Blobs
{
    public class Blob: AForge.Imaging.Blob
    {
        private BitArray _object;

        public BitArray Object
        {
            get
            {
                if ((_object == null) || (_object.Length < 1))
                    // ReSharper disable once NotResolvedInText
                    throw new System.ArgumentNullException("The object has not been saved");

                return _object;
            }
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Blob"/> class.
        /// </summary>
        /// 
        /// <param name="id">Blob's ID in the original image.</param>
        /// <param name="rect">Blob's rectangle in the original image.</param>
        /// 
        /// <remarks><para>This constructor leaves <see cref="AForge.Imaging.Image"/> property not initialized. The blob's
#pragma warning disable 1574
        /// image may be extracted later using <see cref="AForge.Imaging.BlobCounterBase.ExtractBlobsImage( Bitmap, Blob, bool )"/>
        /// or <see cref="AForge.Imaging.BlobCounterBase.ExtractBlobsImage( AForge.Imaging.UnmanagedImage, Blob, bool )"/> method.</para></remarks>
#pragma warning restore 1574
        /// 
        public Blob(int id, Rectangle rect) : base(id, rect) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Blob"/> class.
        /// </summary>
        /// 
        /// <param name="source">Source blob to copy.</param>
        /// 
        /// <remarks><para>This copy constructor leaves <see cref="AForge.Imaging.Image"/> property not initialized. The blob's
#pragma warning disable 1584, 1711, 1572, 1581, 1580
        /// image may be extracted later using <see cref="BlobCounterBase.ExtractBlobsImage( Bitmap, Blob, bool )"/>
        /// or <see cref="BlobCounterBase.ExtractBlobsImage( UnmanagedImage, Blob, bool )"/> method.</para></remarks>
#pragma warning restore 1584, 1711, 1572, 1581, 1580
        /// 
        public Blob(AForge.Imaging.Blob source) : base(source) { }

        #endregion

        public void SetupObject(Rectangle rect, int[] objectLabels, int objectLabel, int imageWidth)
        {
            _object = new BitArray(rect.Width * rect.Height, false);

            for (int y = rect.Y; y < rect.Height + rect.Y; y++)
            {
                for (int x = rect.X; x < rect.Width + rect.X; x++)
                {
                    if (objectLabels[y * imageWidth + x] == objectLabel)
                        _object[(y - rect.Y) * rect.Width + x - rect.X] = true;
                }
            }
        }
    }
}