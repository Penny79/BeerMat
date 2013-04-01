using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;

namespace BeerMat.Core.Concrete
{
    public static class PointExtensions
    {
              
        public static IntPtr ToMatix(this Point[] p)
        {
            var src = new Matrix<double>(4, 2);
            for (int i = 0; i < 4; i++)
            {
                src.Data[i, 0] = p[i].X;
                src.Data[i, 1] = p[i].Y;
            }
            return src.Ptr;
        }

        public static double DistanceTo(this Point a, Point b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

    }
}
