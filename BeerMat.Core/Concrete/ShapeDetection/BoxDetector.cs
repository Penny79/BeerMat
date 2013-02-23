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

        #region Public Methods and Operators

        public IEnumerable<Point[]> DetectShapes(Image<Gray, byte> sourceFrame)
        {
            //CircleF[] circles = grayImage.HoughCircles(
            //    new Gray(120),
            //    new Gray(120),
            //    70.0, //Resolution of the accumulator used to detect centers of the circles
            //    100.0, //min distance 
            //    100, //min radius
            //    0 //max radius
            //    )[0];

            var boxList = new List<Point[]>();

            using (var storage = new MemStorage()) //allocate storage for contour approximation
            {
                var detectedContours = sourceFrame.FindContours(
                    CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_EXTERNAL);

                ////  Parallel.For(
                //  Parallel.ForEach<Contour<Point>>(detectedContours.ToList(), (contour) =>
                //      {

                //      });

                for (Contour<Point> contours = detectedContours; contours != null; contours = contours.HNext)
                {
                    Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.05, storage);

                    if (contours.Area > 1000)
                    {
                        if (this.IsRectangle(currentContour))
                        {
                            var cornerPoints = GetCornerPointsOfPerspectiveRectangle(currentContour);
                            boxList.Add(cornerPoints);
                        }
                        else
                        {
                        }
                    }
                }
            }

            return boxList;
        }

        #endregion

        #region Methods

        private static float CalculateDistance(Point convexHullPoint, PointF point)
        {
            var dX = point.X - convexHullPoint.X;
            var dY = point.Y - convexHullPoint.Y;
            return (dX * dX) + (dY * dY);
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
            var closestPoints = minAreavertices.ToDictionary(
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

            return closestPoints.Select(x => x.Value.Point).ToArray();
        }

        /// <summary>
        /// Check wether the contour has 4 vertices and, determine if all the angles in the contour are within the range of [80, 100] degree
        /// </summary>
        /// <param name="currentContour">the contour to check on</param>
        /// <returns>true if it is a rectangle</returns>
        private bool IsRectangle(Contour<Point> currentContour)
        {
            if (currentContour.Total != 4)
            {
                return false;
            }

            bool isRectangle = true;
            Point[] pts = currentContour.ToArray();
            LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

            for (int i = 0; i < edges.Length; i++)
            {
                double angle = Math.Abs(edges[(i + 1) % edges.Length].GetExteriorAngleDegree(edges[i]));
                if (angle < 60 || angle > 120)
                {
                    isRectangle = false;
                    break;
                }
            }
            return isRectangle;
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