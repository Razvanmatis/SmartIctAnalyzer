using System;
using System.Collections.Generic;
using System.Text;

namespace PCBInvestAPI.DataModel
{
	public struct FileInformations
	{
		private List<string> _stepslist;

		public List<string> Steplist
		{
			get { return _stepslist; }
			set { _stepslist = value; }
		}

		private List<LayerInformations> _listOfLayers;

		public List<LayerInformations> ListofLayers
		{
			get { return _listOfLayers; }
			set { _listOfLayers = value; }
		}

	}

	public struct LayerInformations
	{
		private string _stepName;

		public string StepName
		{
			get { return _stepName; }
			set { _stepName = value; }
		}


		private string _layerName;

		public string LayerName
		{
			get { return _layerName; }
			set { _layerName = value; }
		}


		private int _anzahlObjecte;

		public int ObjectCount
		{
			get { return _anzahlObjecte; }
			set { _anzahlObjecte = value; }
		}

	}

}
