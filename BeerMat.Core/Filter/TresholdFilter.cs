using System;
using BeerMat.Core.Model;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace BeerMat.Core.Filter
{
    internal class TresholdFilter : IImageFilter
    {

        public ImageData Run(ImageData imageData)            
        {
            var tresholdedImage = imageData.CurrentImage.Convert<Gray, Byte>();

            //smoothed
            tresholdedImage = tresholdedImage.PyrDown();
            tresholdedImage = tresholdedImage.PyrUp();

            tresholdedImage = tresholdedImage.SmoothBlur(6, 6);

            //binarize
            const int adaptiveThresholdBlockSize = 75;
            const double adaptiveThresholdParameter = 5;
            CvInvoke.cvAdaptiveThreshold(tresholdedImage, tresholdedImage, 255, 
                                        ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_MEAN_C, 
                                        THRESH.CV_THRESH_BINARY, 
                                        adaptiveThresholdBlockSize + adaptiveThresholdBlockSize % 2 + 1, 
                                        adaptiveThresholdParameter);

            tresholdedImage = tresholdedImage.Erode(3);
            tresholdedImage = tresholdedImage.Dilate(3);            
            

            //Entfernen störender Details
            //CvInvoke.cvThreshold(tresholdedImage, tresholdedImage, 150, 255, THRESH.CV_THRESH_BINARY_INV);

        
            //tresholdedImage = tresholdedImage.SmoothBilatral(10, 255, 220);

            imageData.CurrentImage = tresholdedImage;
            return imageData;
        }

        public FilterType FilterType
        {
            get { return FilterType.Treshold; }
            private set { }
        }
    }
}
