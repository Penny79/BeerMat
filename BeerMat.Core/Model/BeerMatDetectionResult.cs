using System;

using Emgu.CV;
using Emgu.CV.Structure;

namespace BeerMat.Core.Model
{
    public class BeerMatDetectionResult
    {
        #region Public Properties

        public Image<Bgr, Byte> ImageWithShapes { get; set; }

        public Image<Bgr, Byte> OriginalImage { get; set; }

        public Image<Gray, Byte> ThreshholdedImage { get; set; }

        public TimeSpan TimeTaken { get; set; }

        #endregion
    }
}