using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using Interfaces.PcbInvestigator;
using ProMik.Services.Interfaces;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class ViewModelTestpoint : ViewModelPCBComponentBase
    {
        private IGeneralSettingsData settingsVm;

        public ViewModelTestpoint(IPCBComponent component, IEventService eventService, IGeneralSettingsData settingsVm)
            : base(component, eventService, settingsVm)
        {
            this.settingsVm = settingsVm;
            InitSettings();
        }

        public override void InitSettings()
        {
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = settingsVm.TestpointColor;
            BackgroundColor = brush;
            if (!settingsVm.TestpointNamesChecked)
            {
                Name = string.Empty;
            }
            else
            {
                Name = BaseComponent.FunctionalAttributes.Ref;
            }
        }
    }
}
