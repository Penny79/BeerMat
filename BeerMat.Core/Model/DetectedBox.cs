using System.Drawing;
using BeerMat.Core.Concrete;
using Emgu.CV;

namespace BeerMat.Core.Model
{
    public class DetectedBox
    {
        /// <summary>
        /// Contains the corner points for the detected box in the image plane
        /// </summary>
        public Point[] CornerPoints { get; set; }

        /// <summary>
        /// The transformation matrix of the Box plane with respect to the camera COS
        /// </summary>
        public Matrix<float> AffineTransformation { get; set; }

        /// <summary>
        /// The Ticks when this box was detected the last time
        /// </summary>
        public long LastDetectedAt { get; set; }

        public bool Equals(DetectedBox otherBox, double maxDeviation)
        {
            for (int i = 0; i < 4; i++)
            {
                if (CornerPoints[i].DistanceTo(otherBox.CornerPoints[i]) > maxDeviation) return false;
            }

            return true;
        }

    }
}
