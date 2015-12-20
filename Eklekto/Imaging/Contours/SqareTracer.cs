using System;
using System.Collections.Generic;
using System.Drawing;
using AForge;
using AForge.Imaging;

namespace Eklekto.Imaging.Contours
{
    /// <summary>
    /// http://www.imageprocessingplace.com/downloads_V3/root_downloads/tutorials/contour_tracing_Abeer_George_Ghuneim/square.html
    /// </summary>
    public class SqareTracer : IContourTracer
    {
        private struct Move
        {
            public const int Left = 0;
            public const int Down = 1;
            public const int Right = 2;
            public const int Up = 3;
        };

        private readonly IntPoint[] _direction = 
        {
            new IntPoint(-1, 0),
            new IntPoint(0, 1),
            new IntPoint(1, 0),
            new IntPoint(0, -1)
        };

        public Blob Blob { get; set; }
        public int[] ObjectLabels { get; set; }
        public Size ImageSize { get; set; }

        public int StartVisitingCount = 1;

        public List<IntPoint> SelectContour(IntPoint startPoint)
        {
            IntPoint currentPoint = startPoint;
            List<IntPoint> contour = new List<IntPoint>();
            int startVisitingСountdown = StartVisitingCount;

            //clockwise
            currentPoint += _direction[Move.Right];
            int move = Move.Right; // if we mowe left we`ll have trermination in first step

            while (startVisitingСountdown > 0)
            {
                if (currentPoint == startPoint)
                    startVisitingСountdown--;

                if ((currentPoint.Y >= 0) && (currentPoint.Y < ImageSize.Height) &&
                    (currentPoint.X >= 0) && (currentPoint.X < ImageSize.Width) &&
                    (ObjectLabels[currentPoint.Y * ImageSize.Width + currentPoint.X] == Blob.ID))
                {
                    contour.Add(currentPoint);
                    move = TurnLeft(move);
                    currentPoint += _direction[move];
                }
                else
                {
                    move = TurnRight(move);
                    currentPoint += _direction[move];
                }
            }
            return contour;
        }

        private static int TurnRight(int move)
        {
            switch (move)
            {
                case Move.Left:
                    move = Move.Up;
                    break;
                case Move.Down:
                    move = Move.Left;
                    break;
                case Move.Right:
                    move = Move.Down;
                    break;
                case Move.Up:
                    move = Move.Right;
                    break;
            }
            return move;
        }

        private static int TurnLeft(int move)
        {
            switch (move)
            {
                case Move.Left:
                    move = Move.Down;
                    break;
                case Move.Down:
                    move = Move.Right;
                    break;
                case Move.Right:
                    move = Move.Up;
                    break;
                case Move.Up:
                    move = Move.Left;
                    break;
            }
            return move;
        }
    }
}
