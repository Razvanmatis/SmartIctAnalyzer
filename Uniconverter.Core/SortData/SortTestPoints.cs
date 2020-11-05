using System;
using System.Collections.Generic;
using System.Text;
using Uniconverter.Core.DataModel;
namespace Uniconverter.Core
{
    public class SortTestPoints
    {
        private double _75millimit = 2.2;
        private double _50millimit = 1.85;
        private double _smaller50 = 1.3;

        public SortTestPoints(ref List<TestPoint> list, ref TestPoint tp, double limit75mil = 2.2, double limit50mil = 1.85)
        {

            _75millimit = limit75mil;
            _50millimit = limit50mil;
            _smaller50 = 1.3;

            if (list.Count == 0)
            {
                list.Add(tp);
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    TestPoint p = list[i];
                    calculateDistance(ref p, ref tp);
                }
            }
        }

        void calculateDistance(ref TestPoint tp1, ref TestPoint tp2)
        {

            double result = Math.Round(GetDistance(tp1.X_Value, tp1.Y_Value, tp2.X_Value, tp2.Y_Value),3);


            TestPointDistance p1 = new TestPointDistance(tp1.TPname, result);
            TestPointDistance p2 = new TestPointDistance(tp2.TPname, result);

            if (_smaller50 >= result && result < _50millimit)
            {

                tp1.addTestPoint(p2, "50mil");
                tp2.addTestPoint(p1, "50mil");
            }
            else if(_75millimit > result && result >= _50millimit)
            {
                tp1.addTestPoint(p2, "75mil");
                tp2.addTestPoint(p1, "75mil");
            }
            else if(result >= _75millimit)
            {
                tp1.addTestPoint(p2);
                tp2.addTestPoint(p1);
            }
            else if(result < _smaller50)
            {
                tp1.addTestPoint(p2, "smaller50mil");
                tp2.addTestPoint(p1, "smaller50mil");
            }

        }

        private double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }
    }
}
