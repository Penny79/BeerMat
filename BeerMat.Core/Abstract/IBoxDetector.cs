using System;
using System.Collections.Generic;
using System.Drawing;

using Emgu.CV;
using Emgu.CV.Structure;

namespace BeerMat.Core.Abstract
{
    public interface IBoxDetector
    {
        #region Public Methods and Operators

        IEnumerable<Point[]> DetectShapes(Image<Gray, Byte> sourceFrame);

        #endregion
    }
}