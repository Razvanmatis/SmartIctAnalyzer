using System;
using System.Collections.Generic;
using System.Text;
using Uniconverter.Core.DataModel.OutLineObjects;

namespace Uniconverter.Core.DataModel
{
	public class Component
	{
	
		#region Public

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

		public string Catogory
		{
			get
			{
				if (CPHigh >= 15)
				{
					return "Cut_out_KTE";
				}
				if (CPHigh >= 14)
				{
					return "Pocket_KTE_t9";
				}
				if (CPHigh >= 13)
				{
					return "Pocket_KTE_t8";
				}
				if (CPHigh > 12)
				{
					return "Pocket_KTE_t7";
				}
				if (CPHigh > 11)
				{
					return "Pocket_KTE_t6";
				}
				if (CPHigh > 10)
				{
					return "Pocket_KTE_t5";
				}
				if (CPHigh > 9)
				{
					return "Pocket_KTE_t4";
				}
				if (CPHigh > 8)
				{
					return "Pocket_KTE_t3";
				}
				if (CPHigh > 7)
				{
					return "Pocket_KTE_t2";
				}
				if (CPHigh > 6)
				{
					return "Pocket_KTE_t1";
				}
				if (CPHigh > 5)
				{
					return "Cut_out_ADP";
				}
				if (CPHigh > 4)
				{
					return "Pocket_ADP_t3";
				}
				if (CPHigh > 3)
				{
					return "Pocket_ADP_t2";
				}
				if (CPHigh > 2.5)
				{
					return "Pocket_ADP_t1";
				}
				return "smaller than 2.5";
			}
		}

		#endregion
	}
}
