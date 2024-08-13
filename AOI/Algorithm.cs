using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = OpenCvSharp.Point;

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

        public Mat Binarization(Mat src, double threshold)
        {
            Mat grayImg = new Mat();
            Cv2.CvtColor(src, grayImg, ColorConversionCodes.BGR2GRAY);
            Mat binaryImg = new Mat();
            Cv2.Threshold(grayImg, binaryImg, threshold, 255, ThresholdTypes.Binary);
            return binaryImg;
        }

        public Mat BoundingBox(Mat src, double threshold)
        {
            Mat binaryImg = Binarization(src, threshold);
            Cv2.FindContours(binaryImg, out Point[][] contours, out HierarchyIndex[] hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            Mat dst = src.Clone();
            foreach (var contour in contours)
            {
                Rect boundingRect = Cv2.BoundingRect(contour);
                Cv2.Rectangle(dst, boundingRect, Scalar.Red, 2);
            }
            return dst;
        }

        public double CalculateGrayAverage(Mat src)
        {
            Mat grayImage = new Mat();
            Cv2.CvtColor(src, grayImage, ColorConversionCodes.BGR2GRAY);
            Scalar meanValue = Cv2.Mean(grayImage);
            return meanValue.Val0;
        }

    }
}
