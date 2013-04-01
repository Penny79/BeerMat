using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

using BeerMat.Core.Abstract;
using BeerMat.Core.Concrete.PreProcessing;
using BeerMat.Core.Concrete.ShapeDetection;
using BeerMat.Core.Model;

using Emgu.CV;
using Emgu.CV.Structure;

namespace BeerMat.Core.Concrete
{
    /// <summary>
    /// Orchestrates the needed steps for detection the beer mat
    /// Uses sperate filter classes for the actual implementations
    ///</summary>
    public class BeerMatDetection
    {
        #region Fields

        private readonly IBoxDetector boxDetector;

        private readonly IImagePreProcessor preprocessor;    

        #endregion

        #region Constructors and Destructors

        public BeerMatDetection()
        {            
            this.preprocessor = new ImagePreProcessorAdvanced(120, 120);
            this.boxDetector = new BoxDetector();            
        }

        #endregion

        #region Public Methods and Operators

        public BeerMatDetectionResult Process(Image<Bgr, Byte> originalFrame)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();                        

            var result = new BeerMatDetectionResult() { OriginalImage = originalFrame };

            result.ThreshholdedImage = this.preprocessor.Run(originalFrame);

            IEnumerable<DetectedBox> detectedBoxes = this.boxDetector.DetectShapes(result.ThreshholdedImage);

            result.ImageWithShapes = this.DrawBoxes(originalFrame, detectedBoxes);
            
            stopWatch.Stop();            
            result.TimeTaken = stopWatch.Elapsed;
            return result;
        }

        #endregion

        #region Methods

        private Image<Bgr, Byte> DrawBoxes(Image<Bgr, Byte> frame, IEnumerable<DetectedBox> boxes)
        {
            var randomizer = new Random(255);
            Image<Bgr, Byte> boxImage = frame.Copy();

            foreach (var box in boxes)
            {
                boxImage.DrawPolyline(box.CornerPoints, true, new Bgr(randomizer.Next(255), randomizer.Next(255), randomizer.Next(255)), 5);
            }

            return boxImage;
        }

        #endregion
    }
}