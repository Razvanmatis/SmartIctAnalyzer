using System;
using System.Collections.Generic;
using System.Text;

namespace Uniconverter.Core.DataModel
{
	public class TestPoint
	{
		#region Private Values


		private List<TestPointDistance> _mil50;
		private List<TestPointDistance> _mil75;
		private List<TestPointDistance> _mil100;
		private List<TestPointDistance> _smaller50mil;
		#endregion



		#region Public

		public double X_Value { get; set; }

		public double Y_Value { get; set; }

		public string TPname { get; set; }

		public string PointPos { get; set; }

		public string NetList { get; set; }

		public bool ProgrammingPoint { get; set; }


		public string TPDescription { get; set; }


		public void addTestPoint(TestPointDistance tp, string catogory = "100")
		{
			switch (catogory)
			{
				case "100":
					if (_mil100 == null)
					{
						_mil100 = new List<TestPointDistance>();
					}
					else
					{
						_mil100.Add(tp);
					}
					break;
				case "75":
					if (_mil75 == null)
					{
						_mil75 = new List<TestPointDistance>();
					}
					else
					{
						_mil75.Add(tp);
					}
					break;

				case "50":
					if (_mil50 == null)
					{
						_mil50 = new List<TestPointDistance>();
					}
					else
					{
						_mil50.Add(tp);
					}
					break;
				case "smaller50mil":
					if(_smaller50mil == null)
					{
						_smaller50mil = new List<TestPointDistance>();
					}
					else
					{
						_smaller50mil.Add(tp);
					}

					break;
			}
		}
		#endregion

	}

	public class TestPointDistance
	{
		private string _pointtodistance;
		private double _distance;

		public TestPointDistance(string ptName, double distance)
		{
			_pointtodistance = ptName;
			_distance = distance;
		}

		public string PointName
		{
			get { return _pointtodistance; }
			set { _pointtodistance = value; }
		}

		public double Distance
		{
			get { return _distance; }
			set { _distance = value; }
		}

	}
}
