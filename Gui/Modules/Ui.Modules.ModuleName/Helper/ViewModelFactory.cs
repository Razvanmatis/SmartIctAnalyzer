using System;
using System.Collections.Generic;
using System.Text;
using GrpcClientParser.Implementations;
using Interfaces.PcbInvestigator;
using ProMik.Services.Interfaces;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.ViewModels;

namespace Ui.Modules.ModuleName.Helper
{
    public static class ViewModelFactory
    {
        public static ViewModelPCBComponentBase GetViewModelObject(IPCBComponent component, IEventService eventService, IGeneralSettingsData settingsVm)
        {
            if (component is PCBResistor)
            {
                return new ViewModelResistor(component, eventService, settingsVm);
            }
            else if (component is PCBCapacitor)
            {
                return new ViewModelCapacitor(component, eventService, settingsVm);
            }
            else if (component is PCBInduction)
            {
                return new ViewModelInduction(component, eventService, settingsVm);
            }
            else if (component is PCBTestpoint)
            {
                return new ViewModelTestpoint(component, eventService, settingsVm);
            }
            else if (component is PCBIc)
            {
                return new ViewModelIc(component, eventService, settingsVm);
            }
            else if (component is PCBConnector)
            {
                return new ViewModelConnector(component, eventService, settingsVm);
            }
            else
            {
                return new ViewModelComponent(component, eventService, settingsVm);
            }
        }
    }
}
