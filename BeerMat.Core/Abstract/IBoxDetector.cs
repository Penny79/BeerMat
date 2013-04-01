using System;
using System.Collections.Generic;
using System.Drawing;

using BeerMat.Core.Model;

using Emgu.CV;
using Emgu.CV.Structure;

namespace BeerMat.Core.Abstract
{
    public interface IBoxDetector
    {
        #region Public Methods and Operators

        IEnumerable<DetectedBox> DetectShapes(Image<Gray, byte> sourceFrame);

        #endregion
    }
}