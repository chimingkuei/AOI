using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOI
{
    class Algorithm
    {
        public double CalcSlope(Point p1, Point p2)
        {
            double slope = -1;
            if (p1.X == p2.X)
            {
                Console.WriteLine("無法計算!");
            }
            else
            {
                slope = (p2.Y - p1.Y) / (p2.X - p1.X);
            }
            return slope;
        }

        // Equation:mx-y+(b1-ma1)=0
        public Tuple<double, double, double> calcLinearEquation(Point p1, Point p2)
        {
            double m = CalcSlope(p1, p2);
            double c = p1.Y - m * p1.X;
            return Tuple.Create(m, -1.0, c);
        }

        public OpenCvSharp.Mat Binarization(OpenCvSharp.Mat src, double threshold)
        {
            OpenCvSharp.Mat grayImg = new OpenCvSharp.Mat();
            OpenCvSharp.Cv2.CvtColor(src, grayImg, OpenCvSharp.ColorConversionCodes.BGR2GRAY);
            OpenCvSharp.Mat binaryImg = new OpenCvSharp.Mat();
            OpenCvSharp.Cv2.Threshold(grayImg, binaryImg, threshold, 255, OpenCvSharp.ThresholdTypes.Binary);
            return binaryImg;
        }

        public OpenCvSharp.Mat BoundingBox(OpenCvSharp.Mat src, double threshold)
        {
            OpenCvSharp.Mat binaryImg = Binarization(src, threshold);
            OpenCvSharp.Cv2.FindContours(binaryImg, out OpenCvSharp.Point[][] contours, out OpenCvSharp.HierarchyIndex[] hierarchy, OpenCvSharp.RetrievalModes.Tree, OpenCvSharp.ContourApproximationModes.ApproxSimple);
            OpenCvSharp.Mat dst = src.Clone();
            foreach (var contour in contours)
            {
                OpenCvSharp.Rect boundingRect = OpenCvSharp.Cv2.BoundingRect(contour);
                OpenCvSharp.Cv2.Rectangle(dst, boundingRect, OpenCvSharp.Scalar.Red, 2);
            }
            return dst;
        }

    }
}
