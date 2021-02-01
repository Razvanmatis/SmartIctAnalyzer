using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Prism.Commands;
using ProMik.Services.Interfaces;
using Ui.Core.Mvvm;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class GeneralSettingsViewModel : ViewModelBase
    {
        private Color resistorColor;
        private Color capacitorColor;
        private Color inductionColor;
        private Color testpointColor;
        private Color icColor;
        private int fontSizeNames;
        private string resistorIdentifier;
        private string capacitorIdentifier;
        private string inductionIdentifier;
        private string testpointIdentifier;
        private string icIdentifier;
        private IEventService eventService;
        private Color compColor;
        private bool resistorNamesChecked;
        private bool capacitorNamesChecked;
        private bool inductionNamesChecked;
        private bool testpointNamesChecked;
        private bool icNamesChecked;
        private bool compNamesChecked;
        private bool connectorNamesChecked;
        private Color connectorColor;
        private string connectorIdentifier;
        private bool netNamesChecked;
        private IGeneralSettingsData settingsData;

        public GeneralSettingsViewModel(IGeneralSettingsData settingsData, IEventService eventService)
        {
            this.settingsData = settingsData;
            this.eventService = eventService;
            InitValues();
            SaveCommand = new DelegateCommand(SaveValues);
            KeyCommand = new DelegateCommand(HandleEnterKey);
        }

        public ICommand KeyCommand { get; private set; }

        public string ResistorIdentifier
        {
            get
            {
                return resistorIdentifier;
            }

            set
            {
                SetProperty(ref resistorIdentifier, value);
                settingsData.ResistorChars = GetListContent(value);
            }
        }

        public Color ResistorColor
        {
            get
            {
                return resistorColor;
            }

            set
            {
                SetProperty(ref resistorColor, value);
                settingsData.ResistorColor = value;
            }
        }

        public string CapacitorIdentifier
        {
            get
            {
                return capacitorIdentifier;
            }

            set
            {
                SetProperty(ref capacitorIdentifier, value);
                settingsData.CapacitorChars = GetListContent(value);
            }
        }

        public Color CapacitorColor
        {
            get
            {
                return capacitorColor;
            }

            set
            {
                SetProperty(ref capacitorColor, value);
                settingsData.CapacitorColor = value;
            }
        }

        public string InductionIdentifier
        {
            get
            {
                return inductionIdentifier;
            }

            set
            {
                SetProperty(ref inductionIdentifier, value);
                settingsData.InductionChars = GetListContent(value);
            }
        }

        public Color InductionColor
        {
            get
            {
                return inductionColor;
            }

            set
            {
                SetProperty(ref inductionColor, value);
                settingsData.InductionColor = value;
            }
        }

        public string TestpointIdentifier
        {
            get
            {
                return testpointIdentifier;
            }

            set
            {
                SetProperty(ref testpointIdentifier, value);
                settingsData.TestpointChars = GetListContent(value);
            }
        }

        public string ConnectorIdentifier
        {
            get
            {
                return connectorIdentifier;
            }

            set
            {
                SetProperty(ref connectorIdentifier, value);
                settingsData.ConnectorChars = GetListContent(value);
            }
        }

        public Color CompColor
        {
            get
            {
                return compColor;
            }

            set
            {
                SetProperty(ref compColor, value);
                settingsData.CompColor = value;
            }
        }

        public Color TestpointColor
        {
            get
            {
                return testpointColor;
            }

            set
            {
                SetProperty(ref testpointColor, value);
                settingsData.TestpointColor = value;
            }
        }

        public Color ConnectorColor
        {
            get
            {
                return connectorColor;
            }

            set
            {
                SetProperty(ref connectorColor, value);
                settingsData.ConnectorColor = value;
            }
        }

        public string ICIdentifier
        {
            get
            {
                return icIdentifier;
            }

            set
            {
                SetProperty(ref icIdentifier, value);
                settingsData.ICChars = GetListContent(value);
            }
        }

        public bool ResistorNamesChecked
        {
            get
            {
                return resistorNamesChecked;
            }

            set
            {
                SetProperty(ref resistorNamesChecked, value);
                settingsData.ResistorNamesChecked = value;
            }
        }

        public bool CapacitorNamesChecked
        {
            get
            {
                return capacitorNamesChecked;
            }

            set
            {
                SetProperty(ref capacitorNamesChecked, value);
                settingsData.CapacitorNamesChecked = value;
            }
        }

        public bool InductionNamesChecked
        {
            get
            {
                return inductionNamesChecked;
            }

            set
            {
                SetProperty(ref inductionNamesChecked, value);
                settingsData.InductionNamesChecked = value;
            }
        }

        public bool TestpointNamesChecked
        {
            get
            {
                return testpointNamesChecked;
            }

            set
            {
                SetProperty(ref testpointNamesChecked, value);
                settingsData.TestpointNamesChecked = value;
            }
        }

        public bool ICNamesChecked
        {
            get
            {
                return icNamesChecked;
            }

            set
            {
                SetProperty(ref icNamesChecked, value);
                settingsData.ICNamesChecked = value;
            }
        }

        public bool CompNamesChecked
        {
            get
            {
                return compNamesChecked;
            }

            set
            {
                SetProperty(ref compNamesChecked, value);
                settingsData.CompNamesChecked = value;
            }
        }

        public bool ConnectorNamesChecked
        {
            get
            {
                return connectorNamesChecked;
            }

            set
            {
                SetProperty(ref connectorNamesChecked, value);
                settingsData.ConnectorNamesChecked = value;
            }
        }

        public Color ICColor
        {
            get
            {
                return icColor;
            }

            set
            {
                SetProperty(ref icColor, value);
                settingsData.ICColor = value;
            }
        }

        public int FontSizeNames
        {
            get
            {
                return fontSizeNames;
            }

            set
            {
                SetProperty(ref fontSizeNames, value);
                settingsData.FontSizeNames = value;
            }
        }

        public bool NetNamesChecked
        {
            get
            {
                return netNamesChecked;
            }

            set
            {
                SetProperty(ref netNamesChecked, value);
                settingsData.NetNamesChecked = value;
            }
        }

        public ICommand SaveCommand { get; private set; }

        private static IList<string> GetListContent(string value)
        {
            IList<string> list = new List<string>();
            foreach (var text in value.Split(";"))
            {
                list.Add(text);
            }

            return list;
        }

        private static string GetTextFromList(IList<string> values)
        {
            string text = string.Empty;
            foreach (string val in values)
            {
                text += val + ";";
            }

            return text.Substring(0, text.Length - 1);
        }

        private void HandleEnterKey()
        {
            SaveCommand.Execute(null);
        }

        private void InitValues()
        {
            settingsData.InitContent();
            resistorColor = settingsData.ResistorColor;
            capacitorColor = settingsData.CapacitorColor;
            inductionColor = settingsData.InductionColor;
            testpointColor = settingsData.TestpointColor;
            icColor = settingsData.ICColor;
            compColor = settingsData.CompColor;
            connectorColor = settingsData.ConnectorColor;
            resistorIdentifier = GetTextFromList(settingsData.ResistorChars);
            capacitorIdentifier = GetTextFromList(settingsData.CapacitorChars);
            inductionIdentifier = GetTextFromList(settingsData.InductionChars);
            testpointIdentifier = GetTextFromList(settingsData.TestpointChars);
            icIdentifier = GetTextFromList(settingsData.ICChars);
            connectorIdentifier = GetTextFromList(settingsData.ConnectorChars);
            fontSizeNames = settingsData.FontSizeNames;
            resistorNamesChecked = settingsData.ResistorNamesChecked;
            capacitorNamesChecked = settingsData.CapacitorNamesChecked;
            inductionNamesChecked = settingsData.InductionNamesChecked;
            testpointNamesChecked = settingsData.TestpointNamesChecked;
            connectorNamesChecked = settingsData.ConnectorNamesChecked;
            icNamesChecked = settingsData.ICNamesChecked;
            compNamesChecked = settingsData.CompNamesChecked;
            netNamesChecked = settingsData.NetNamesChecked;
        }

        private void SaveValues()
        {
            settingsData.SaveValues();
            eventService.Publish<CloseGeneralSettingsEvent>(new CloseGeneralSettingsEvent(false));
        }
    }
}
