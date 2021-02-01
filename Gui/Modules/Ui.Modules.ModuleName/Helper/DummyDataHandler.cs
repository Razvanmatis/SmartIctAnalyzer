using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Interfaces.PcbInvestigator;

namespace Ui.Modules.ModuleName.Helper
{
    public static class DummyDataHandler
    {
        public static void LogResults(IList<INetComponent> gndNets, IList<INetComponent> powerNets, IList<INetComponent> jtagNets, IList<IPCBComponent> pullDownObjects, IList<IPCBComponent> pullUpObjects, IList<IPCBComponent> icObjects)
        {
            Debug.WriteLine("GND nets:");
            foreach (var net in gndNets)
            {
                Debug.WriteLine(net.NetName);
            }

            Debug.WriteLine(string.Empty);
            Debug.WriteLine("Power nets:");
            foreach (var net in powerNets)
            {
                Debug.WriteLine(net.NetName);
            }

            Debug.WriteLine(string.Empty);
            Debug.WriteLine("JTAG nets:");
            foreach (var net in jtagNets)
            {
                Debug.WriteLine(net.NetName);
            }

            Debug.WriteLine(string.Empty);
            Debug.WriteLine("Pull down resistors:");
            foreach (var comp in pullDownObjects)
            {
                Debug.WriteLine(comp.FunctionalAttributes.Ref);
            }

            Debug.WriteLine(string.Empty);
            Debug.WriteLine("Pull up resistors:");
            foreach (var comp in pullUpObjects)
            {
                Debug.WriteLine(comp.FunctionalAttributes.Ref);
            }

            Debug.WriteLine(string.Empty);
            Debug.WriteLine("ICs:");
            foreach (var comp in icObjects)
            {
                Debug.WriteLine(comp.FunctionalAttributes.Ref);
            }
        }
    }
}
