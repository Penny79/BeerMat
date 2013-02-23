using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;

namespace BeerMat.Core.Model
{
    /// <summary>
    /// A uniform result container
    /// </summary>
    public class StepCompletedEventArgs
    {
        public FilterType ProcessingStep { get; set; }
        
        public ImageData ImageData { get; set; }        
    }
}
