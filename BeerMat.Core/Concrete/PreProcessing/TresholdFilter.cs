using System;

using BeerMat.Core.Abstract;
using BeerMat.Core.Model;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace BeerMat.Core.Concrete.PreProcessing
{
    internal class TresholdFilter : IImagePreProcessor
    {     

        #region Public Methods and Operators

        public Image<Gray, byte> Run(Image<Bgr, Byte> frame)
        {
            var grayFrame = frame.Convert<Gray, Byte>();

            //smoothed
            grayFrame = grayFrame.PyrDown();
            grayFrame = grayFrame.PyrUp();

            grayFrame = grayFrame.SmoothBlur(6, 6);

            //binarize
            const int adaptiveThresholdBlockSize = 75;
            const double adaptiveThresholdParameter = 5;
            CvInvoke.cvAdaptiveThreshold(
                grayFrame,
                grayFrame,
                255,
                ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_MEAN_C,
                THRESH.CV_THRESH_BINARY,
                adaptiveThresholdBlockSize + adaptiveThresholdBlockSize % 2 + 1,
                adaptiveThresholdParameter);

            grayFrame = grayFrame.Erode(3);
            grayFrame = grayFrame.Dilate(3);

            return grayFrame;
        }

        #endregion
    }
}