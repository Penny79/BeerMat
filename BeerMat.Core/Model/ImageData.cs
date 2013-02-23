using Emgu.CV;
using Emgu.CV.Structure;

namespace BeerMat.Core.Model
{
    public class ImageData
    {
        #region Public Properties

        public Image<Gray, byte> CurrentImage { get; set; }

        public Image<Gray, byte> GrayImage { get; set; }

        public Image<Bgr, byte> OriginalImage { get; set; }

        #endregion
    }
}