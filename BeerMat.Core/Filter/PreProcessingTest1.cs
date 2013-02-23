using System;
using BeerMat.Core.Model;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace BeerMat.Core.Filter
{
    internal class PreProcessingTest1 : IImageFilter
    {
        private readonly double cannyThreshold;
        private double cannyThresholdLinking;


        internal PreProcessingTest1(double cannyThreshold, double cannyThresholdLinking)
        {
            this.cannyThreshold = cannyThreshold;
            this.cannyThresholdLinking = cannyThresholdLinking;
        }

        public ImageData Run(ImageData imageData)            
        {
            var grayFrame = imageData.CurrentImage.Convert<Gray, Byte>();
            

            //smoothing                        
            Image<Gray, byte> smoothedGrayFrame = grayFrame.PyrDown();
            smoothedGrayFrame = smoothedGrayFrame.PyrUp();
            grayFrame = smoothedGrayFrame;
            
            //canny            
            Image<Gray, byte> cannyFrame = smoothedGrayFrame.Canny(cannyThreshold,cannyThreshold);
                       
            //binarize
            const int adaptiveThresholdBlockSize = 16;
            const double adaptiveThresholdParameter = 5;

            CvInvoke.cvAdaptiveThreshold(grayFrame, grayFrame, 255,
                                       ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_MEAN_C,
                                       THRESH.CV_THRESH_BINARY,
                                       adaptiveThresholdBlockSize + adaptiveThresholdBlockSize % 2 + 1,
                                       adaptiveThresholdParameter);
            
            grayFrame._Not();
            grayFrame._Or(cannyFrame);

            imageData.CurrentImage = grayFrame;
            imageData.GrayImage = grayFrame;
            return imageData;
        }

        public FilterType FilterType
        {
            get { return FilterType.PreProcessingTest1; }
            private set { }
        }
    }
}
