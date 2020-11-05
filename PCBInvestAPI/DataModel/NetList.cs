using System;
using System.Collections.Generic;
using System.Text;

namespace PCBInvestAPI
{
    public class NetList
    {
		private string _netListName;

		public string NetListName
		{
			get { return _netListName; }
			set { _netListName = value; }
		}

		private List<string> _netTeilnehmer;

		public List<string> Teilnehmer
		{
			get { return _netTeilnehmer; }
			set { _netTeilnehmer = value; }
		}


	}
}
