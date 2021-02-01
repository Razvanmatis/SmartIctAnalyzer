using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using GrpcClientParser.Interfaces;
using Prism.Commands;
using ProMik.Services.Interfaces;
using Ui.Core.Mvvm;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class GrpcServerSettingsViewModel : ViewModelBase
    {
        private IGeneralSettingsData settingData;
        private string ip;
        private IGrpcClientParserHandler grpcParser;
        private SolidColorBrush brush;
        private IEventService eventService;

        public GrpcServerSettingsViewModel(IEventService eventService, IGeneralSettingsData settingData, IGrpcClientParserHandler grpcParser)
        {
            this.eventService = eventService;
            this.settingData = settingData;
            this.grpcParser = grpcParser;
            InitValues();
            CheckColor();
            SaveCommand = new DelegateCommand(SaveTheIp);
            CloseCommand = new DelegateCommand(CloseTheWindow);
        }

        public string IP
        {
            get
            {
                return ip;
            }

            set
            {
                ip = value;
                settingData.IPAddress = value;
            }
        }

        public SolidColorBrush Color
        {
            get
            {
                return brush;
            }

            set
            {
                SetProperty(ref brush, value);
            }
        }

        public ICommand SaveCommand { get; private set; }

        public ICommand CloseCommand { get; private set; }

        private async void SaveTheIp()
        {
            settingData.SaveValues();
            await grpcParser.ChangeIpAdressOfClient(IP).ConfigureAwait(true);
            CheckColor();
        }

        private async void CheckColor()
        {
            bool result = await grpcParser.IsGrpcServerAvailable().ConfigureAwait(true);
            if (result)
            {
                Color = Brushes.Green;
            }
            else
            {
                Color = Brushes.Red;
            }
        }

        private void CloseTheWindow()
        {
            eventService.Publish<CloseGrpcSettingsEvent>(new CloseGrpcSettingsEvent());
        }

        private void InitValues()
        {
            settingData.InitContent();
            IP = settingData.IPAddress;
        }
    }
}
