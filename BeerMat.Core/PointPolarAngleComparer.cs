using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerMat.Core
{
    internal class PointPolarAngleComparer :IComparer<Point>
    {
        public int Compare(Point a, Point b)
        {
            var angleOfA = Math.Atan2(a.X, a.X);
            var angleOfB = Math.Atan2(b.X, b.X);

            return angleOfA.CompareTo(angleOfB);
        }

    }
}
