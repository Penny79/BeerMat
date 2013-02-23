using Emgu.CV;
using Emgu.CV.Structure;

namespace BeerMat.Core
{
    public class ImageData
    {
        public Image<Bgr, byte> OriginalImage
        {
            get;
            set;
        }

        public Image<Gray, byte> GrayImage
        {
            get; set;
        }


        public Image<Gray, byte> CurrentImage
        {
            get;
            set;
        }   
    }
}
