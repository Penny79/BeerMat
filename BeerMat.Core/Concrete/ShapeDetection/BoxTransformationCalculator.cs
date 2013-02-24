using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;

namespace BeerMat.Core.Concrete.ShapeDetection
{
    /// <summary>
    /// Calculates the transformation and rotation of a rectangular beer mat with respect to the camera
    /// </summary>
    class BoxTransformationCalculator
    {
        /// <summary>
        /// The clockwise coodinates of the marker in the camera coordinate system
        /// </summary>
        private readonly Point[] destinationPoints = new Point[] { new Point(100, 100), new Point(100, 400), new Point(400, 400), new Point(400, 100) };

        public void DetermineTransformationMatrix(Point[] srcPoints)
        {
            var homographyMatrix = new HomographyMatrix();
            CvInvoke.cvFindHomography(PointsToMat(srcPoints), PointsToMat(destinationPoints), homographyMatrix.Ptr, Emgu.CV.CvEnum.HOMOGRAPHY_METHOD.DEFAULT, 2.0, IntPtr.Zero);
           
        }

        private IntPtr PointsToMat(Point[] p)
        {
            var src = new Matrix<double>(4, 2);
            for (int i = 0; i < 4; i++)
            {
                src.Data[i, 0] = p[i].X;
                src.Data[i, 1] = p[i].Y;
            }
            return src.Ptr;
        }
    }
}
