using System;

using Emgu.CV;
using Emgu.CV.Structure;

namespace BeerMat.Core.Abstract
{
    /// <summary>
    /// Defines a common interface for the preprocessing step
    /// </summary>
    public interface IImagePreProcessor
    {
        #region Public Methods and Operators

        Image<Gray, Byte> Run(Image<Bgr, Byte> frame);

        #endregion
    }
}