using System;
using System.Drawing;

using BeerMat.Core.Model;

using Emgu.CV;

namespace BeerMat.Core.Concrete.ShapeDetection
{
    /// <summary>
    /// Calculates the transformation and rotation of a rectangular beer mat with respect to the camera
    /// </summary>
    internal class BoxTransformationCalculator
    {
        #region Fields

        /// <summary>
        /// The clockwise coodinates of the marker in the camera coordinate system
        /// </summary>
        private readonly Point[] destinationPoints = new Point[]
            { new Point(100, 100), new Point(100, 400), new Point(400, 400), new Point(400, 100) };

        #endregion

        #region Public Methods and Operators

        public void DetermineTransformationMatrix(DetectedBox box)
        {
            var homographyMatrix = new HomographyMatrix();
            var warpMatrix = new Matrix<float>(3, 3);

            Emgu.CV.CvInvoke.cvGetPerspectiveTransform(
                box.CornerPoints.ToMatix(), destinationPoints.ToMatix(), warpMatrix);

            //CvInvoke.cvFindHomography(
            //    this.PointsToMatix(box.CornerPoints),
            //    this.PointsToMatix(this.destinationPoints),
            //    homographyMatrix.Ptr,
            //    Emgu.CV.CvEnum.HOMOGRAPHY_METHOD.DEFAULT,
            //    2.0,
            //    IntPtr.Zero);

            box.AffineTransformation = warpMatrix;
        }

        #endregion

        #region Methods

        private IntPtr PointsToMatix(Point[] p)
        {
            var src = new Matrix<double>(4, 2);
            for (int i = 0; i < 4; i++)
            {
                src.Data[i, 0] = p[i].X;
                src.Data[i, 1] = p[i].Y;
            }
            return src.Ptr;
        }

        #endregion
    }
}