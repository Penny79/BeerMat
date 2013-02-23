using System;

using Emgu.CV;
using Emgu.CV.Structure;

namespace BeerMat.Core.Abstract
{
    internal interface ICircleDetector
    {
        #region Public Methods and Operators

        CircleF[] DetectShapes(Image<Gray, Byte> sourceFrame);

        #endregion
    }
}