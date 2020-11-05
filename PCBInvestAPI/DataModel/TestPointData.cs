using System;
using System.Collections.Generic;
using System.Text;

namespace PCBInvestAPI.DataModel
{
	public class TestPoint
	{
		#region Private Values




		#endregion

		public TestPoint(string tpName, string tpDes, double x, double y, string pos, Conversions einheit = Conversions.mm, string netListName ="")
		{
			double ConversionValue = 1.00000;
			switch (einheit)
			{
				case Conversions.mil:
					ConversionValue = 1.00000;
					break;
				case Conversions.mm:
					ConversionValue = 0.0254;
					break;
				default:
					ConversionValue = 1.00000;
					break;
			}

			X_Value = Math.Round(x*ConversionValue,3);
			Y_Value = Math.Round(y*ConversionValue,3);
			TPname = tpName;
			TPDescription = tpDes;
			PointPos = pos;
			ProgrammingPoint = false;
			NetList = netListName;
		}

		#region Public

		public double X_Value { get; set; }

		public double Y_Value { get; set; }

		public string TPname  { get; set; }

		public string PointPos { get; set; }

		public string NetList { get; set; }

		public bool ProgrammingPoint { get; set; }


		public string TPDescription { get; set; }


		#endregion

	}

}
