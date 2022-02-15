using System;
using System.Threading.Tasks;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IProjectFileHandler
    {
        Task OpenProjectFile(
            string path,
            Action resetAllAction,
            Action resetTestCoverageAction,
            Action<bool, bool, IProjectHandler> setItemsAction);

        void CreateProjectFileBase(Action resetAllAction, Action<bool, bool, IProjectHandler> actionItemsSelect);

        void CloseProjectAction(Action resetAllAction, Action<bool, bool, IProjectHandler> actionItemsSelect);

        Task<bool> SaveProjectFile(IProjectHandler projectHandlerToUse, Action<bool, bool, IProjectHandler> actionItemsSelect);
    }
}
