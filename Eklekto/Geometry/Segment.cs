using System;
using System.Drawing;

namespace Eklekto.Geometry
{
    public class Segment
    {
        private Point _begin;
        private Point _end;

        public ReflectionedLine Equation;

        public Segment()
        {
            _begin = new Point(0, 0);
            _end = new Point(0, 0);
            Equation = new ReflectionedLine(_begin, _end);
        }

        public Segment(Point p1, Point p2)
        {
            _begin = p1;
            _end = p2;
            Equation = new ReflectionedLine(_begin, _end);
        }

        public Point Top
        {
            get
            {
                if (_begin.Y < _end.Y)
                    return _begin;
                return _end;
            }
        }
        public Point Bottom
        {
            get
                {
                if (_begin.Y > _end.Y)
                    return _begin;
                return _end;
            }
        }

        public int GetX(double y)
        {
            if ((y > Math.Max(_begin.Y, _end.Y)) || (y < Math.Min(_begin.Y, _end.Y)))
                throw new ArgumentOutOfRangeException(nameof(y));
            return (int)Math.Round(Equation.A * y + Equation.B);
        }

        public int GetY(double x)
        {
            if ((x > Math.Max(_begin.X, _end.X)) || (x < Math.Min(_begin.X, _end.X)))
                throw new ArgumentOutOfRangeException(nameof(x));
            return (int)Math.Round((x - Equation.B) / Equation.A);
        }

    }
}
