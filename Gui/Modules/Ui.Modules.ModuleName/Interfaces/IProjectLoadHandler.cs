using System;
using System.Threading.Tasks;
using Ui.Modules.ModuleName.Events;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IProjectLoadHandler
    {
        Task HandleImportJsonProject(IDataSourceProvider projectHandler, bool useSavingInProject, Action resetAction);

        Task HandleExportJsonProject(IProjectHandler projectHandlerToUse);

        Task PerformLoadAction(Action resetTestCoverageAction, string selectedPath);

        Task OpenOdbFolder(IDataSourceProvider projectHandler, Action resetTestCoverageAction);

        void HandleComponentImportFinishedEvent(ComponentsImportFinishedEvent obj, bool autoLoad, IProjectHandler projectHandlerToUse);
    }
}
