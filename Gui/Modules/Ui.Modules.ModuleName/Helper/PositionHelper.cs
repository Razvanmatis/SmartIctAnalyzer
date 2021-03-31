using System.Drawing;
using System.Windows.Media;

namespace Ui.Modules.ModuleName.Helper
{
    public class PositionHelper
    {
        private Point upperLeft;
        private Point upperRight;
        private Point bottomLeft;
        private Point bottomRight;
        private Point center;
        private Point midTop;
        private Point midLeft;
        private Point midRight;
        private Point midBottom;

        public PositionHelper(Rectangle rect)
        {
            upperLeft = new Point(rect.X, rect.Y);
            upperRight = new Point(rect.X + rect.Width, rect.Y);
            bottomLeft = new Point(rect.X, rect.Y + rect.Height);
            bottomRight = new Point(rect.X + rect.Width, rect.Y + rect.Height);
            center = new Point(rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2));
            midLeft = new Point(rect.X, rect.Y + (rect.Height / 2));
            midRight = new Point(rect.X + rect.Width, rect.Y + (rect.Height / 2));
            midTop = new Point(rect.X + (rect.Width / 2), rect.Y);
            midBottom = new Point(rect.X + (rect.Width / 2), rect.Y + rect.Height);
        }

        public Point UpperLeft
        {
            get { return upperLeft; }
        }

        public Point UpperRight
        {
            get { return upperRight; }
        }

        public Point BottomLeft
        {
            get { return bottomLeft; }
        }

        public Point BottomRight
        {
            get { return bottomRight; }
        }

        public Point Center
        {
            get { return center; }
        }

        public Point MidTop
        {
            get { return midTop; }
        }

        public Point MidLeft
        {
            get { return midLeft; }
        }

        public Point MidRight
        {
            get { return midRight; }
        }

        public Point MidBottom
        {
            get { return midBottom; }
        }

        public bool XIsEqual(Point p)
        {
            return center.X == p.X;
        }

        public bool YIsEqual(Point p)
        {
            return center.Y == p.Y;
        }

        public bool MyXMidIsGreaterThan(Point p)
        {
            return center.X > p.X;
        }

        public bool MyYMidIsGreaterThan(Point p)
        {
            return center.Y > p.Y;
        }

        public bool IsHorizontalOverlap(PositionHelper p)
        {
            return (p.Center.X > UpperLeft.X && p.Center.X < UpperRight.X) || (MidTop.X > p.MidLeft.X && MidTop.X < p.MidRight.X) || (p.MidTop.X > MidLeft.X && p.MidTop.X < MidRight.X);
        }

        public bool IsVerticalOverlap(PositionHelper p)
        {
            return (p.Center.Y > BottomLeft.Y && p.Center.Y < UpperLeft.Y) || (MidLeft.Y < p.MidBottom.Y && MidLeft.Y > p.MidTop.Y) || (p.MidLeft.Y < MidBottom.Y && p.MidLeft.Y > MidTop.Y);
        }

        public PointCollection GetPointsForObject(PositionHelper p)
        {
            PointCollection collection = new PointCollection();
            if (XIsEqual(p.Center))
            {
                if (!MyYMidIsGreaterThan(p.Center))
                {
                    collection.Add(new System.Windows.Point(midBottom.X, midBottom.Y));
                    collection.Add(new System.Windows.Point(p.MidTop.X, p.MidTop.Y));
                }
                else
                {
                    collection.Add(new System.Windows.Point(midTop.X, midTop.Y));
                    collection.Add(new System.Windows.Point(p.MidBottom.X, p.MidBottom.Y));
                }
            }
            else if (YIsEqual(p.Center))
            {
                if (MyXMidIsGreaterThan(p.Center))
                {
                    collection.Add(new System.Windows.Point(midLeft.X, midLeft.Y));
                    collection.Add(new System.Windows.Point(p.MidRight.X, p.MidRight.Y));
                }
                else
                {
                    collection.Add(new System.Windows.Point(midRight.X, midRight.Y));
                    collection.Add(new System.Windows.Point(p.MidLeft.X, p.MidLeft.Y));
                }
            }
            else
            {
                bool horizontalOverlap = IsHorizontalOverlap(p);
                bool verticalOverlap = IsVerticalOverlap(p);
                bool isXGreaterThan = MyXMidIsGreaterThan(p.Center);
                bool isYGreaterThan = MyYMidIsGreaterThan(p.Center);
                if (!horizontalOverlap && !verticalOverlap)
                {
                    if (isXGreaterThan && isYGreaterThan)
                    {
                        collection.Add(new System.Windows.Point(midLeft.X, midLeft.Y));
                        collection.Add(new System.Windows.Point(p.MidBottom.X, midLeft.Y));
                        collection.Add(new System.Windows.Point(p.MidBottom.X, p.MidBottom.Y));
                    }
                    else if (!isXGreaterThan && isYGreaterThan)
                    {
                        collection.Add(new System.Windows.Point(midRight.X, midRight.Y));
                        collection.Add(new System.Windows.Point(p.MidBottom.X, midRight.Y));
                        collection.Add(new System.Windows.Point(p.MidBottom.X, p.MidBottom.Y));
                    }
                    else if (isXGreaterThan && !isYGreaterThan)
                    {
                        collection.Add(new System.Windows.Point(midLeft.X, midLeft.Y));
                        collection.Add(new System.Windows.Point(p.MidTop.X, midLeft.Y));
                        collection.Add(new System.Windows.Point(p.MidTop.X, p.MidTop.Y));
                    }
                    else
                    {
                        collection.Add(new System.Windows.Point(midBottom.X, midBottom.Y));
                        collection.Add(new System.Windows.Point(midBottom.X, p.MidLeft.Y));
                        collection.Add(new System.Windows.Point(p.MidLeft.X, p.MidLeft.Y));
                    }
                }
                else if (horizontalOverlap)
                {
                    if (isYGreaterThan)
                    {
                        collection.Add(new System.Windows.Point(midTop.X, midTop.Y));
                        double yMid = p.MidBottom.Y + ((midTop.Y - p.MidBottom.Y) / 2);
                        collection.Add(new System.Windows.Point(midTop.X, yMid));
                        collection.Add(new System.Windows.Point(p.MidBottom.X, yMid));
                        collection.Add(new System.Windows.Point(p.MidBottom.X, p.MidBottom.Y));
                    }
                    else
                    {
                        collection.Add(new System.Windows.Point(midBottom.X, midBottom.Y));
                        double yMid = midBottom.Y + ((p.MidTop.Y - midBottom.Y) / 2);
                        collection.Add(new System.Windows.Point(midBottom.X, yMid));
                        collection.Add(new System.Windows.Point(p.MidTop.X, yMid));
                        collection.Add(new System.Windows.Point(p.MidTop.X, p.MidTop.Y));
                    }
                }
                else
                {
                    if (isXGreaterThan)
                    {
                        collection.Add(new System.Windows.Point(midLeft.X, midLeft.Y));
                        double xMid = p.MidRight.X + ((midLeft.X - p.MidRight.X) / 2);
                        collection.Add(new System.Windows.Point(xMid, midLeft.Y));
                        collection.Add(new System.Windows.Point(xMid, p.MidRight.Y));
                        collection.Add(new System.Windows.Point(p.MidRight.X, p.MidRight.Y));
                    }
                    else
                    {
                        collection.Add(new System.Windows.Point(midRight.X, midRight.Y));
                        double xMid = midRight.X + ((p.MidLeft.X - midRight.X) / 2);
                        collection.Add(new System.Windows.Point(xMid, midRight.Y));
                        collection.Add(new System.Windows.Point(xMid, p.MidLeft.Y));
                        collection.Add(new System.Windows.Point(p.MidLeft.X, p.MidLeft.Y));
                    }
                }
            }

            return collection;
        }
    }
}
