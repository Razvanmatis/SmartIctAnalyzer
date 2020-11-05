using System;
using System.Collections.Generic;
using System.Text;
using PCBInvestAPI.DataModel.OutLineObjects;

namespace PCBInvestAPI.DataModel
{
	public class Component
	{

		public Component(string CPname, string CPdes, double x, double y, double cphigh, string position,Conversions einheit = Conversions.mm, string Netlist = "")
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

			X_Value = Math.Round(x * ConversionValue, 3);
			Y_Value = Math.Round(y * ConversionValue, 3);

			CPName = CPname;
			CPDescription = CPdes;
			CPHigh = Math.Round(cphigh * ConversionValue, 3); ;
			CPPosition = position;
			NetList = NetList;
		}


		#region Public
		public string CPName { get; set; }

		public double X_Value { get; set; }

		public double Y_Value { get; set; }


		public string CPPosition { get; set; }

		public string CPDescription { get; set; }


		public double CPHigh { get; set; }

		public string NetList { get; set; }


		public List<BaseOutlineObject> ComponentenOutline { get; set; }

		#endregion
	}
}
