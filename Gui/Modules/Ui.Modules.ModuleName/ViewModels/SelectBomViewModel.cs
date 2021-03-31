using System.Windows.Input;
using Interfaces.Gui;
using Prism.Commands;
using ProMik.Core.Interfaces.Events;
using Ui.Core.Mvvm;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class SelectBomViewModel : ViewModelBase
    {
        private readonly IEventService eventService;
        private readonly IBomDataModel bomDataModel;
        private readonly IDialogSelector dialogSelector;
        private string columnRef;
        private string columnValue;
        private string bomFile;
        private string separator;

        public SelectBomViewModel(IEventService eventService, IBomDataModel bomDataModel, IDialogSelector dialogSelector)
        {
            this.bomDataModel = bomDataModel;
            this.dialogSelector = dialogSelector;
            this.eventService = eventService;
            PathCommand = new DelegateCommand(OpenSelectFileDialog);
            FinishCommand = new DelegateCommand(FinishEventCalling);
            Separator = ";";
            ColumnRef = "0";
            ColumnValue = "2";
        }

        public string ColumnRef
        {
            get
            {
                return columnRef;
            }

            set
            {
                SetProperty(ref columnRef, value);
                bomDataModel.ColumnRef = value;
            }
        }

        public bool AutoMode { get; set; }

        public string ColumnValue
        {
            get
            {
                return columnValue;
            }

            set
            {
                SetProperty(ref columnValue, value);
                bomDataModel.ColumnValue = value;
            }
        }

        public string BomFile
        {
            get
            {
                return bomFile;
            }

            set
            {
                SetProperty(ref bomFile, value);
                bomDataModel.BomFile = value;
            }
        }

        public ICommand PathCommand { get; }

        public ICommand FinishCommand { get; }

        public string Separator
        {
            get
            {
                return separator;
            }

            set
            {
                SetProperty(ref separator, value);
                bomDataModel.Separator = value;
            }
        }

        private void OpenSelectFileDialog()
        {
            string fileName = string.Empty;
            if (!dialogSelector.OpenGenericDialog(DialogType.OPENFILE, TextRessources.SelectBomFile, "No valid BOM file selected!", out fileName))
            {
                return;
            }

            BomFile = fileName;
        }

        private void FinishEventCalling()
        {
            eventService.Publish<SelectBomFinishEvent>(new SelectBomFinishEvent(false, AutoMode));
        }
    }
}
