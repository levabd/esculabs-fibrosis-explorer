using System;
using System.Drawing;

namespace Eklekto.Geometry
{
    /// <summary>
    /// x = Ay + B
    /// </summary>
    public class ReflectionedLine
    {
        private readonly double _a;
        private readonly double _b;

        public double A => _a;

        public double B => _b;

        public double Angle => Math.Atan(_a) * 180 / Math.PI;

        public ReflectionedLine(double a, double b)
        {
            _a = a;
            _b = b;
        }

        public ReflectionedLine(Point p1, Point p2)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            // Divisioning by zero
            if (p1.Y == p2.Y)
                _a = 0;
            else
            {
                _a = (p1.X - p2.X) / (double)(p1.Y - p2.Y);
                _b = p1.X - p1.Y * _a;
            }
        }

        public int GetX(double y)
        {
            return (int) Math.Round(_a * y + _b);
        }

        public int GetY(double x)
        {
            return (int) Math.Round((x - _b) / _a);
        }
    }
}
