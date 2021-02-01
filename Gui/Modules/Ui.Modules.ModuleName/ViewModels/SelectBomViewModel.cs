using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using Prism.Commands;
using ProMik.Services.Interfaces;
using Ui.Core.Mvvm;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class SelectBomViewModel : ViewModelBase
    {
        private string columnRef;
        private string columnValue;
        private string bomFile;
        private string separator;
        private IEventService eventService;
        private IBomDataModel bomDataModel;

        public SelectBomViewModel(IEventService eventService, IBomDataModel bomDataModel)
        {
            this.bomDataModel = bomDataModel;
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
            VistaOpenFileDialog fileDialog = new VistaOpenFileDialog();
            fileDialog.Title = TextRessources.SelectBomFile;
            fileDialog.ShowDialog();
            if (fileDialog.FileName != null && fileDialog.FileName.Length > 0)
            {
                BomFile = fileDialog.FileName;
            }
        }

        private void FinishEventCalling()
        {
            eventService.Publish<SelectBomFinishEvent>(new SelectBomFinishEvent(false));
        }
    }
}
