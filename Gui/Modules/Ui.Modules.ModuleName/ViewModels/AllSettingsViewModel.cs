using System;
using System.Threading.Tasks;
using ProMik.Core.Interfaces.Events;
using ProMik.Core.Interfaces.Settings;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.PCBComponentParser.Interfaces;
using Ui.Core.Mvvm;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class AllSettingsViewModel : ViewModelBase
    {
        private readonly IEventService eventService;
        private readonly ISettingsData settingData;
        private readonly IPCBComponentParser grpcParser;
        private readonly ILogger logger;
        private readonly ISettingsHandler settingsHandler;

        public AllSettingsViewModel(
            ISettingsService settingsService,
            IEventService eventService,
            ISettingsData settingData,
            IPCBComponentParser grpcParser,
            ILogger logger,
            ISettingsHandler settingsHandler)
        {
            SettingsService = settingsService;
            this.eventService = eventService;
            this.settingsHandler = settingsHandler;
            this.logger = logger;
            this.grpcParser = grpcParser;
            this.settingData = settingData;
            eventService.Subscribe<CloseAllSettingsEvent>(CloseAllSettingsView);
            AfterSaveAction = async () => await HandleAfterSave().ConfigureAwait(false);
        }

        public ISettingsService SettingsService { get; }

        public Action AfterSaveAction { get; }

        private async Task HandleAfterSave()
        {
            settingData.InitContent();
            eventService.Publish(new CloseAllSettingsEvent(false));
            await TryGrpcConnection().ConfigureAwait(false);
        }

        private void CloseAllSettingsView(CloseAllSettingsEvent obj)
        {
            settingsHandler.CloseAllSettingsView(obj);
        }

        private async Task TryGrpcConnection()
        {
            await grpcParser.ChangeIpAdressOfClient(settingData.IPAddress).ConfigureAwait(true);
            bool result = await grpcParser.IsGrpcServerAvailable().ConfigureAwait(true);
            if (result)
            {
                logger.LogMessage("Connection established", LogCategory.INFO);
            }
            else
            {
                logger.LogMessage("Connection not possible", LogCategory.ERROR);
            }
        }
    }
}
