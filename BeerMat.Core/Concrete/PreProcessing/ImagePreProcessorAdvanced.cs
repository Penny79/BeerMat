using System;

using BeerMat.Core.Abstract;
using BeerMat.Core.Model;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace BeerMat.Core.Concrete.PreProcessing
{
    internal class ImagePreProcessorAdvanced : IImagePreProcessor
    {
        #region Fields

        private readonly double cannyThreshold;

        #endregion

        #region Constructors and Destructors

        internal ImagePreProcessorAdvanced(double cannyThreshold, double cannyThresholdLinking)
        {
            this.cannyThreshold = cannyThreshold;
        }

        #endregion
      

        #region Public Methods and Operators

        public Image<Gray, byte> Run(Image<Bgr, byte> frame)
        {
            var grayFrame = frame.Convert<Gray, Byte>();

            //smoothing                        
            Image<Gray, byte> smoothedGrayFrame = grayFrame.PyrDown();
            smoothedGrayFrame = smoothedGrayFrame.PyrUp();
            grayFrame = smoothedGrayFrame;

            //canny            
            Image<Gray, byte> cannyFrame = smoothedGrayFrame.Canny(this.cannyThreshold, this.cannyThreshold);

            //binarize
            const int AdaptiveThresholdBlockSize = 16;
            const double AdaptiveThresholdParameter = 4;

            CvInvoke.cvAdaptiveThreshold(
                grayFrame,
                grayFrame,
                255,
                ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_MEAN_C,
                THRESH.CV_THRESH_BINARY,
                AdaptiveThresholdBlockSize + AdaptiveThresholdBlockSize % 2 + 1,
                AdaptiveThresholdParameter);

            grayFrame._Not();
            grayFrame._Or(cannyFrame);

            return grayFrame;
        }

        #endregion
    }
}