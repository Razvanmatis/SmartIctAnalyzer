using System;
using System.Threading.Tasks;
using Ui.Modules.ModuleName.Events;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IDropHandler
    {
        Task HandleDropEventMethod(HandleDropEvent obj, Action resetAllAction, Action resetTestCoverageAction, Action<bool, bool, IProjectHandler> setItemsAction);
    }
}
