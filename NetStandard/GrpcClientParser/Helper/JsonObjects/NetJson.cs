using System;
using System.Collections.Generic;
using System.Text;
using GrpcClientParser.Interfaces;

namespace GrpcClientParser.Helper.JsonObjects
{
    public class NetJson
    {
        public NetJson(IList<int> pins, IList<int> components, string netName)
        {
            Pins = pins;
            Components = components;
            NetName = netName;
        }

        public IList<int> Pins { get; set; }

        public IList<int> Components { get; set; }

        public string NetName { get; set; }
    }
}
