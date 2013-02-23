using System;
using System.Collections.Generic;
using BeerMat.Core.Filter;
using BeerMat.Core.Model;
using Emgu.CV;
using Emgu.CV.Structure;

namespace BeerMat.Core
{
    /// <summary>
    /// Orchestrates the needed steps for detection the beer mat
    /// Uses sperate filter classes for the actual implementations
    ///</summary>
    public class ShapeDetection
    {

        /// <summary>
        /// You can use this event to subscribe to the results of each single processing step. (e.g. for display purposes)
        /// </summary>
        public event EventHandler<StepCompletedEventArgs> OnStepCompleted;
 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        /// <param name="cannyThreshold"></param>
        /// <param name="cannyThresholdLinking"></param>
        public void Process(ImageData imageData, double cannyThreshold, double cannyThresholdLinking)
        {
            var processingPipeline = new List<IImageFilter>
                {
                    //new GrayFilter(),
                    //new TresholdFilter(),
                    //new EdgeDetector(cannyThreshold, cannyThresholdLinking),
                    new PreProcessingTest1(cannyThreshold, cannyThresholdLinking),
                    new BoxDetector()
                };

            if (imageData.CurrentImage == null)
            {
                imageData.CurrentImage = imageData.OriginalImage.Convert<Gray, Byte>();
            }

            foreach (var imageFilter in processingPipeline)
            {
                imageData = imageFilter.Run(imageData);
                FireEvent(imageFilter.FilterType, imageData);
            }              
        }

        //private void FireEvent(FilterType processingStep, ImageData imageData)
        //{            
        //    FireEvent(processingStep, imageData);
        //}

        private void FireEvent(FilterType processingStep, ImageData imageData)
        {
            if (OnStepCompleted != null)
            {
                var args = new StepCompletedEventArgs()
                {
                    ImageData = imageData,
                    ProcessingStep = processingStep
                };

                OnStepCompleted(this, args);
            }
        } 
    }
}
    
