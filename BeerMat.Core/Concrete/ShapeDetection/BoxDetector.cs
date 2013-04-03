using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using BeerMat.Core.Abstract;
using BeerMat.Core.Model;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace BeerMat.Core.Concrete.ShapeDetection
{
    internal class BoxDetector : IBoxDetector
    {
        #region Fields

        private static List<DetectedBox> trackedBoxes = new List<DetectedBox>();


        #endregion

        #region Constructors and Destructors

        public BoxDetector()
        {
          
        }

        #endregion

        #region Public Methods and Operators

        public IEnumerable<DetectedBox> DetectShapes(Image<Gray, byte> sourceFrame)
        {
            var transformationCalculator = new BoxTransformationCalculator();

            var boxList = GetRectangularContours(sourceFrame);

            // avoid possible multiple enumeration
            var detectedBoxs = boxList as DetectedBox[] ?? boxList.ToArray();

            foreach (var box in detectedBoxs)
            {
                transformationCalculator.DetermineTransformationMatrix(box);
            }

          //  return boxList;

            ExpireTrackedBoxes();

            MergeWithTrackedBoxes(detectedBoxs);

            return trackedBoxes;
        }

        private void ExpireTrackedBoxes()
        {
            long ticksNow = DateTime.UtcNow.Ticks;
            lock (trackedBoxes)
            {
                trackedBoxes =
                    trackedBoxes.Where(x => new TimeSpan(ticksNow - x.LastDetectedAt).TotalMilliseconds < 100).ToList();
            }
        }

        private void MergeWithTrackedBoxes(IEnumerable<DetectedBox> boxList)
        {

            foreach (var detectedBox in boxList)
            {

                var match = trackedBoxes.FirstOrDefault(x => detectedBox.Equals(x, 50));
                if (match == null)
                {
                    trackedBoxes.Add(detectedBox);
                }
                else
                {
                    match.NumberOfDetections++;
                    int weightFactor = match.NumberOfDetections % 10;
                    match.CornerPoints[0] = GetCenterPoint(match.CornerPoints[0], detectedBox.CornerPoints[0], weightFactor);
                    match.CornerPoints[1] = GetCenterPoint(match.CornerPoints[1], detectedBox.CornerPoints[1], weightFactor);
                    match.CornerPoints[2] = GetCenterPoint(match.CornerPoints[2], detectedBox.CornerPoints[2], weightFactor);
                    match.CornerPoints[3] = GetCenterPoint(match.CornerPoints[3], detectedBox.CornerPoints[3], weightFactor);
                    match.LastDetectedAt = DateTime.UtcNow.Ticks;
                }
            }
        }

        private Point GetCenterPoint(Point a, Point b, int weightFactorForA)
        {
            var c = new Point();

            c.X = (a.X * weightFactorForA + b.X) / (weightFactorForA + 1);
            c.Y = (a.Y * weightFactorForA + b.Y) / (weightFactorForA + 1);

            return c;
        }

        /*
        public IEnumerable<DetectedBox> FindPattern(Image<Bgr, byte> i)
        {           

        Emgu.CV.Image<Emgu.CV.Structure.Gray, byte> _binary_marker, _warped, _tmp;
        int _binary_threshold;
        float _max_error_normed;
        System.Drawing.PointF[] _warp_dest;
         //   MarkerPattern my_pattern = this.Pattern as MarkerPattern;
            

            float best_error = _max_error_normed;

            // Find contour points in black/white image
            Emgu.CV.Contour<Point> contour_points;
            Emgu.CV.Image<Gray, byte> my_img = i.Convert<Gray, Byte>();

            using (var binary = new Emgu.CV.Image<Gray, byte>(my_img.Size))
            {
                Emgu.CV.CvInvoke.cvThreshold(
                  my_img, binary,
                  _binary_threshold, 255,
                  Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY | THRESH.CV_THRESH_OTSU);
                binary._Not(); // Contour is searched on white points, marker envelope is black.
                contour_points = binary.FindContours();
            }

            // Loop over all contours

            var _contour_storage = new Emgu.CV.MemStorage();
            while (contour_points != null)
            {
                // Approximate contour points by poly-lines.
                // For our marker-envelope should generate a poly-line consisting of four vertices.
                Emgu.CV.Contour<System.Drawing.Point> c = contour_points.ApproxPoly(contour_points.Perimeter * 0.05, _contour_storage);
                if (c.Total != 4 || c.Perimeter < 200)
                {
                    contour_points = contour_points.HNext;
                    continue;
                }

                // Warp content of poly-line as if looking at it from the top
                var warp_source = new System.Drawing.PointF[]
                    {
                        new System.Drawing.PointF(c[0].X, c[0].Y), new System.Drawing.PointF(c[1].X, c[1].Y),
                        new System.Drawing.PointF(c[2].X, c[2].Y), new System.Drawing.PointF(c[3].X, c[3].Y)
                    };

                var warp_matrix = new Emgu.CV.Matrix<float>(3, 3);
                Emgu.CV.CvInvoke.cvGetPerspectiveTransform(warp_source, _warp_dest, warp_matrix);
                Emgu.CV.CvInvoke.cvWarpPerspective(
                  my_img, _warped, warp_matrix,
                  (int)Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC + (int)Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS,
                  new Emgu.CV.Structure.MCvScalar(0)
                );

                //CvInvoke.cvThreshold(
                //  _warped, _warped,
                //  _binary_threshold, 255,
                //  Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY | Emgu.CV.CvEnum.THRESH.CV_THRESH_OTSU);

                // Perform a template matching with the stored pattern in order to
                // determine if content of the envelope matches the stored pattern and
                // determine the orientation of the pattern in the image.
                // Orientation is encoded
                // 0: 0°, 1: 90°, 2: 180°, 3: 270°
                float error;
                int orientation;
                TemplateMatch(out error, out orientation);

                if (error < best_error)
                {
                    best_error = error;
                    int id_0 = orientation;
                    int id_1 = (orientation + 1) % 4;
                    int id_2 = (orientation + 2) % 4;
                    int id_3 = (orientation + 3) % 4;

                    // ids above are still counterclockwise ordered, we need to permute them
                    // 0   3    0   1
                    // +---+    +---+
                    // |   | -> |   |
                    // +---+    +---+
                    // 1   2    2   3

                    //dr.Success = true;
                    //dr.ViewCorrespondences.ImagePoints.Clear();
                    //dr.ViewCorrespondences.ImagePoints.Add(c[id_0]);
                    //dr.ViewCorrespondences.ImagePoints.Add(c[id_3]);
                    //dr.ViewCorrespondences.ImagePoints.Add(c[id_1]);
                    //dr.ViewCorrespondences.ImagePoints.Add(c[id_2]);
                }

                //contour_points = contour_points.HNext;
            } // End of contours

            // Complete with model correspondences.           
            
        }

      

        #endregion

        #region Methods

        private void TemplateMatch(out float error, out int orientation)
        {
            // 0 degrees
            orientation = 0;
            error = (float)_warped.MatchTemplate(_binary_marker, Emgu.CV.CvEnum.TM_TYPE.CV_TM_SQDIFF_NORMED)[0, 0].Intensity;

            // 90 degrees
            Emgu.CV.CvInvoke.cvTranspose(_warped, _tmp);
            Emgu.CV.CvInvoke.cvFlip(_tmp, IntPtr.Zero, 1); // y-axis 
            float err = (float)_tmp.MatchTemplate(_binary_marker, Emgu.CV.CvEnum.TM_TYPE.CV_TM_SQDIFF_NORMED)[0, 0].Intensity;
            if (err < error)
            {
                error = err;
                orientation = 1;
            }

            // 180 degrees
            Emgu.CV.CvInvoke.cvFlip(_warped, _tmp, -1);
            err = (float)_tmp.MatchTemplate(_binary_marker, Emgu.CV.CvEnum.TM_TYPE.CV_TM_SQDIFF_NORMED)[0, 0].Intensity;
            if (err < error)
            {
                error = err;
                orientation = 2;
            }

            // 270 degrees
            Emgu.CV.CvInvoke.cvTranspose(_warped, _tmp);
            Emgu.CV.CvInvoke.cvFlip(_tmp, IntPtr.Zero, 0); // x-axis 
            err = (float)_tmp.MatchTemplate(_binary_marker, Emgu.CV.CvEnum.TM_TYPE.CV_TM_SQDIFF_NORMED)[0, 0].Intensity;
            if (err < error)
            {
                error = err;
                orientation = 3;
            }
        }
         */
       

        /// <summary>
        /// Gt a list of detected rectangles in the image
        /// </summary>      
        /// <returns>the list of rectangles</returns>
        private static IEnumerable<DetectedBox> GetRectangularContours(Image<Gray, byte> sourceFrame)
        {
            var boxList = new List<DetectedBox>();
            var rectangularContours = new List<Contour<Point>>();

            using (var storage = new MemStorage()) //allocate storage for contour approximation
            {
                var detectedContours = sourceFrame.FindContours(
                    CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_EXTERNAL);

                for (Contour<Point> contours = detectedContours; contours != null; contours = contours.HNext)
                {
                    Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.02, storage);

                    // has a significant size and 4 or more points
                    if (currentContour.Convex && currentContour.Total == 4 && currentContour.Area > 2000)
                    {
                        var box = new DetectedBox
                        {
                            CornerPoints = currentContour.ToArray(),
                            LastDetectedAt = DateTime.UtcNow.Ticks
                        };
                        boxList.Add(box);
                      
                    }
                }
            }
            return boxList;
        }

        #endregion      
    }
}