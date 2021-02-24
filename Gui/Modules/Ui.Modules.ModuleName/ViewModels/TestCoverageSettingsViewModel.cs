using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Interfaces.Gui;
using Prism.Commands;
using ProMik.Services.Interfaces;
using Ui.Core.Mvvm;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class TestCoverageSettingsViewModel : ViewModelBase
    {
        private IGeneralSettingsData settingData;
        private IEventService eventService;
        private string gndIdentifier;
        private string gndBlacklist;
        private string powerIdentifier;
        private string powerBlacklist;
        private string jtagIdentifier;
        private string jtagBlacklist;
        private ILogger logger;

        public TestCoverageSettingsViewModel(IGeneralSettingsData settingData, IEventService eventService, ILogger logger)
        {
            this.settingData = settingData;
            this.logger = logger;
            this.eventService = eventService;
            InitValues();
            SaveCommand = new DelegateCommand(SaveCommandHandler);
            KeyCommand = new DelegateCommand(HandleEnterKey);
        }

        public ICommand KeyCommand { get; private set; }

        public string GndIdentifier
        {
            get
            {
                return gndIdentifier;
            }

            set
            {
                SetProperty(ref gndIdentifier, value);
                settingData.GndNetIdentifier = value;
            }
        }

        public string GndBlacklist
        {
            get
            {
                return gndBlacklist;
            }

            set
            {
                SetProperty(ref gndBlacklist, value);
                settingData.GndNetBlacklist = value;
            }
        }

        public string PowerIdentifier
        {
            get
            {
                return powerIdentifier;
            }

            set
            {
                SetProperty(ref powerIdentifier, value);
                settingData.PowerNetIdentifier = value;
            }
        }

        public string PowerBlacklist
        {
            get
            {
                return powerBlacklist;
            }

            set
            {
                SetProperty(ref powerBlacklist, value);
                settingData.PowerNetBlacklist = value;
            }
        }

        public string JTAGIdentifier
        {
            get
            {
                return jtagIdentifier;
            }

            set
            {
                SetProperty(ref jtagIdentifier, value);
                settingData.JTAGNetIdentifier = value;
            }
        }

        public string JTAGBlacklist
        {
            get
            {
                return jtagBlacklist;
            }

            set
            {
                SetProperty(ref jtagBlacklist, value);
                settingData.JTAGNetBlacklist = value;
            }
        }

        public ICommand SaveCommand
        {
            get;
            private set;
        }

        private void SaveCommandHandler()
        {
            settingData.SaveValues();
            eventService.Publish<CloseTestCoverageSettingsEvent>(new CloseTestCoverageSettingsEvent(false));
            logger.LogMessage("Test coverage settings saved", LogCategory.INFO);
        }

        private void HandleEnterKey()
        {
            SaveCommand.Execute(null);
        }

        private void InitValues()
        {
            GndIdentifier = settingData.GndNetIdentifier;
            GndBlacklist = settingData.GndNetBlacklist;
            PowerIdentifier = settingData.PowerNetIdentifier;
            PowerBlacklist = settingData.PowerNetBlacklist;
            JTAGIdentifier = settingData.JTAGNetIdentifier;
            JTAGBlacklist = settingData.JTAGNetBlacklist;
        }
    }
}
