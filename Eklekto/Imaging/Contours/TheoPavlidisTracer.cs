using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AForge;
using AForge.Imaging;

namespace Eklekto.Imaging.Contours
{
    public class TheoPavlidisTracer : IContourTracer
    {
        public Blob Blob { get; set; }
        public int[] ObjectLabels { get; set; }
        public Size ImageSize { get; set; }

        /// <summary>
        /// Copyright Scott Alexander. 
        /// http://scottalexander.co/theo-palvids-algorithm/
        /// </summary>
        public List<IntPoint> SelectContour(IntPoint startPoint)
        {
            int startPointFound = 0;
            int direction = 0;
            List<IntPoint> border = new List<IntPoint>();
            HashSet<IntPoint> failPoints = new HashSet<IntPoint>();

            // repeat the process until and border has been found
            while (!border.Any())
            {
                IntPoint searchPoint = startPoint;

                // run the key algorithm
                while (startPointFound < 2)
                {
                    // Get all the points around our current point in one go.
                    List<int> localPoints = SquareSearchTheoHash(searchPoint);
                    List<int> nextPoints = new List<int>();

                    // Drections: 0 = straight up 1 = left, 2 = down, 3 = right
                    for (int turns = 0; turns < 4; turns++)
                    {
                        switch (direction)
                        {
                            case 0:
                                nextPoints.Add(localPoints.ElementAt(0));
                                nextPoints.Add(localPoints.ElementAt(1));
                                nextPoints.Add(localPoints.ElementAt(2)); break;
                            case 1:
                                nextPoints.Add(localPoints.ElementAt(2));
                                nextPoints.Add(localPoints.ElementAt(5));
                                nextPoints.Add(localPoints.ElementAt(8)); break;
                            case 2:
                                nextPoints.Add(localPoints.ElementAt(8));
                                nextPoints.Add(localPoints.ElementAt(7));
                                nextPoints.Add(localPoints.ElementAt(6)); break;
                            case 3:
                                nextPoints.Add(localPoints.ElementAt(6));
                                nextPoints.Add(localPoints.ElementAt(3));
                                nextPoints.Add(localPoints.ElementAt(0)); break;
                        }

                        // Determins if a turn is required if no points are found
                        if ((nextPoints.ElementAt(0) == 0) && (nextPoints.ElementAt(1) == 0) && (nextPoints.ElementAt(2) == 0))
                        {
                            if (direction == 3)
                                direction = 0;
                            else
                                direction += 1;

                            nextPoints.Clear();
                        }
                        else
                            break;
                    }

                    // If we leave the algorithm search with no points weve found a fail point, reutn to the start and chose a different starting position
                    if (nextPoints.Count == 0)
                    {
                        failPoints.Add(searchPoint);
                        border.Clear();
                        break;
                    }

                    if ((nextPoints.ElementAt(0) == 0) && (nextPoints.ElementAt(1) == 0) && (nextPoints.ElementAt(2) == 0))
                    {
                        failPoints.Add(searchPoint);
                        border.Clear();
                        break;
                    }

                    // Get the cords of the points around the current point
                    HashSet<IntPoint> coOrdPoints = GetSquareCordsHash(searchPoint);

                    // Check the three rules and add to the border when required
                    if (nextPoints.ElementAt(0) == 1)
                    {
                        switch (direction)
                        {
                            case 0: border.Add(coOrdPoints.ElementAt(0));
                                searchPoint = coOrdPoints.ElementAt(0); break;
                            case 1: border.Add(coOrdPoints.ElementAt(2));
                                searchPoint = coOrdPoints.ElementAt(2); break;
                            case 2: border.Add(coOrdPoints.ElementAt(8));
                                searchPoint = coOrdPoints.ElementAt(8); break;
                            case 3: border.Add(coOrdPoints.ElementAt(6));
                                searchPoint = coOrdPoints.ElementAt(6); break;
                        }

                        if (direction == 0)
                            direction = 3;
                        else
                            direction -= 1;
                    }
                    else
                    {
                        if (nextPoints.ElementAt(1) == 1)
                        {
                            switch (direction)
                            {
                                case 0: border.Add(coOrdPoints.ElementAt(1));
                                    searchPoint = coOrdPoints.ElementAt(1); break;
                                case 1: border.Add(coOrdPoints.ElementAt(5));
                                    searchPoint = coOrdPoints.ElementAt(5); break;
                                case 2: border.Add(coOrdPoints.ElementAt(7));
                                    searchPoint = coOrdPoints.ElementAt(7); break;
                                case 3: border.Add(coOrdPoints.ElementAt(3));
                                    searchPoint = coOrdPoints.ElementAt(3); break;
                            }
                        }
                        else
                        {
                            if (nextPoints.ElementAt(2) == 1)
                            {
                                switch (direction)
                                {
                                    case 0:
                                        border.Add(coOrdPoints.ElementAt(2));
                                        searchPoint = coOrdPoints.ElementAt(2); break;
                                    case 1:
                                        border.Add(coOrdPoints.ElementAt(8));
                                        searchPoint = coOrdPoints.ElementAt(8); break;
                                    case 2:
                                        border.Add(coOrdPoints.ElementAt(6));
                                        searchPoint = coOrdPoints.ElementAt(6); break;
                                    case 3:
                                        border.Add(coOrdPoints.ElementAt(0));
                                        searchPoint = coOrdPoints.ElementAt(0); break;
                                }
                            }
                        }
                    }

                    // check to see if the start point has been reached, if so add to the counter that records how many times its been reached.
                    if (searchPoint == startPoint)
                    {
                        Console.WriteLine("searchPoint Point is" + searchPoint.X + ' ' + searchPoint.Y);
                        startPointFound += 1;
                    }
                }
            }

            return border;
        }

        public HashSet<IntPoint> GetSquareCordsHash(IntPoint point)
        {
            HashSet<IntPoint> pixelList = new HashSet<IntPoint>();
            int moveX = point.X;
            int moveY = point.Y;

            for (int ltr = moveX - 1; ltr <= moveX + 1; ltr++)
            {
                for (int ttb = moveY - 1; ttb <= moveY + 1; ttb++)
                {
                    pixelList.Add(new IntPoint(ltr, ttb));
                }
            }
            return pixelList;
        }

        public List<int> SquareSearchTheoHash(IntPoint point)
        {
            List<int> pixelList = new List<int>();

            int moveX = point.X;
            int moveY = point.Y;

            for (int ltr = moveX - 1; ltr <= moveX + 1; ltr++)
            {
                for (int ttb = moveY - 1; ttb <= moveY + 1; ttb++)
                {
                    if ((ttb < 0) || (ltr < 0) ||
                        (ttb >= ImageSize.Height) || (ltr >= ImageSize.Width) || 
                        (ObjectLabels[ttb * ImageSize.Width + ltr] != Blob.ID))
                    {
                        pixelList.Add(0);
                    }
                    else
                    {
                        pixelList.Add(1);
                    }
                }
            }
            return pixelList.ToList();
        }   
    }
}
