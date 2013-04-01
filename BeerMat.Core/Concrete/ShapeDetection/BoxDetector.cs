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
            //CircleF[] circles = grayImage.HoughCircles(
            //    new Gray(120),
            //    new Gray(120),
            //    70.0, //Resolution of the accumulator used to detect centers of the circles
            //    100.0, //min distance 
            //    100, //min radius
            //    0 //max radius
            //    )[0];

            var boxList = new List<DetectedBox>();

            using (var storage = new MemStorage()) //allocate storage for contour approximation
            {
                var detectedContours = sourceFrame.FindContours(
                    CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_EXTERNAL);

                var interesstingContours = FilterContours(detectedContours);

                //Parallel.ForEach(
                //    interesstingContours,
                //    (contour) =>
                //        {
                foreach (var contour in interesstingContours)
                {
                    Contour<Point> currentContour = contour.ApproxPoly(contour.Perimeter * 0.02, storage);

                    var cornerPoints = currentContour.ToArray();

                    if (cornerPoints.Length == 4 && currentContour.Convex)
                    {
                        var box = new DetectedBox
                            {
                                CornerPoints = cornerPoints,
                                LastDetectedAt = DateTime.UtcNow.Ticks
                            };
                        
                        transformationCalculator.DetermineTransformationMatrix(box);

                        boxList.Add(box);
                    }
                    //});
                }
            }

           ExpireTrackedBoxes();

            MergeWithTrackedBoxes(boxList);
            
            return trackedBoxes;
        }

        private void ExpireTrackedBoxes()
        {
            long ticksNow = DateTime.UtcNow.Ticks;
           lock (trackedBoxes)
           {
               trackedBoxes = trackedBoxes.Where(x => new TimeSpan(ticksNow - x.LastDetectedAt).TotalMilliseconds < 100).ToList();
           }
        }

        private void MergeWithTrackedBoxes(List<DetectedBox> boxList)
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
                    match.CornerPoints[0] = detectedBox.CornerPoints[0];
                    match.CornerPoints[1] = detectedBox.CornerPoints[1];
                    match.CornerPoints[2] = detectedBox.CornerPoints[2];
                    match.CornerPoints[3] = detectedBox.CornerPoints[3];
                    match.LastDetectedAt = DateTime.UtcNow.Ticks;
                }
            }

           // boxList.AddRange(trackedBoxes);
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

        private static float CalculateDistance(Point convexHullPoint, PointF point)
        {
            var dX = point.X - convexHullPoint.X;
            var dY = point.Y - convexHullPoint.Y;
            return (dX * dX) + (dY * dY);
        }

        /// <summary>
        /// Filters contours that can not be rectangular and that are small
        /// </summary>
        /// <param name="detectedContours">list of contours</param>
        /// <returns>filtered contours</returns>
        private static IEnumerable<Contour<Point>> FilterContours(Contour<Point> detectedContours)
        {
            var interesstingContours = new List<Contour<Point>>();
            for (Contour<Point> contours = detectedContours; contours != null; contours = contours.HNext)
            {
                // has a significant size and 4 or more points
                if (contours.Area > 1500 && contours.Total >= 4)
                {
                    interesstingContours.Add(contours);
                }
            }
            return interesstingContours;
        }

        /// <summary>
        /// Calclates the MinArea RectAngle and the Convex Hull for the given Contour
        /// Determines the Corner points of the rectangle by finding 4 points with minimal distance between the
        /// convex hull and the min area rectangle.
        /// </summary>
        /// <param name="currentContour">the contour to work on</param>
        /// <returns>the corner points of a rectangle with perspective</returns>
        private static Point[] GetCornerPointsOfPerspectiveRectangle(Contour<Point> currentContour)
        {
            // Get vertices of Min Area Rectangle
            var minAreaRect = currentContour.GetMinAreaRect();
            var minAreavertices = minAreaRect.GetVertices();

            // init dictionary
            var closestPoints = minAreavertices.Distinct().ToDictionary(
                minAreavertice => minAreavertice,
                minAreavertice =>
                new DistanceInformation { Point = new Point(-1000000, -10000000), Distance = float.MaxValue });

            var convexHull = currentContour.GetConvexHull(ORIENTATION.CV_CLOCKWISE);

            foreach (var convexHullPoint in convexHull)
            {
                foreach (var point in minAreavertices)
                {
                    float distance = CalculateDistance(convexHullPoint, point);

                    if (distance < closestPoints[point].Distance)
                    {
                        closestPoints[point].Point = convexHullPoint;
                        closestPoints[point].Distance = distance;
                    }
                }
            }

            return SortCounterClockWise(closestPoints.Select(x => x.Value.Point).ToArray());
        }

        private static Point[] SortCounterClockWise(Point[] cornerPoints)
        {
            Array.Sort(cornerPoints, new PointPolarAngleComparer());


            return cornerPoints;
        }


        /// <summary>
        /// Tries to group close points to areas and counts the number of areas retrieved
        /// </summary>
        /// <param name="currentContour">the contour that contains the points</param>
        /// <returns>the number of areas</returns>
        private static Point[] GetAreasOfPointConcentration(Contour<Point> currentContour)
        {
            //try to group to 4 areas of close points
            var pointsToProcess = currentContour.ToList();

            var pointAreas = new List<Point>();

            while (pointsToProcess.Count > 0)
            {
                // add to first point in the list as the point for a new vertex area
                var currentPoint = pointsToProcess[0];

                var areaCenterPoint = pointAreas.FirstOrDefault(x => currentPoint != x && x.DistanceTo(currentPoint) < 30);
                
                if (!areaCenterPoint.IsEmpty)
                {
                    // incremetally improve the center of the area
                    areaCenterPoint.X = (areaCenterPoint.X + currentPoint.X) / 2;
                    areaCenterPoint.Y = (areaCenterPoint.Y + currentPoint.Y) / 2;
                }
                else
                {
                    pointAreas.Add(currentPoint);
                }


             
                pointsToProcess.Remove(currentPoint);
            }

           // pointAreas
            return SortCounterClockWise(pointAreas.ToArray());
            
        }

        #endregion

        private class DistanceInformation
        {
            #region Public Properties

            public float Distance { get; set; }

            public Point Point { get; set; }

            #endregion
        }
    }
}