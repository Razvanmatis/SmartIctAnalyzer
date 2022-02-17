using ProMik.Core.Interfaces.Events;
using System.Collections.Generic;
using System.Diagnostics;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName
{
    public class TestEventListener : ITestEventListener
    {
        private IEventService eventService;
        private ISettingsData settingsVm;
        private IComponentHandler componentVm;

        public TestEventListener(IEventService eventService, ISettingsData settingsVm, IComponentHandler componentVm)
        {
            this.eventService = eventService;
            this.settingsVm = settingsVm;
            this.componentVm = componentVm;
            RegisterEvents();
        }

        public void RegisterEvents()
        {
            eventService.Subscribe<AddPcbObjectsEvent>(HandleObjectsInsertedEvent);
        }

        public void HandleObjectsInsertedEvent(AddPcbObjectsEvent obj)
        {
            //List<string> listOfNets = new List<string>();
            //foreach (var net in obj.Nets)
            //{
            //    if (!listOfNets.Contains(net.NetName))
            //    {
            //        listOfNets.Add(net.NetName);
            //    }
            //}

            //foreach (var net in listOfNets)
            //{
            //    Debug.WriteLine(net);
            //}
        }
    }
}
