using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ui.Modules.ModuleName.ViewModels;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IComponentViewModel
    {
        Task ToggleCompleteVisibility(Type typeToHide);

        Task ToggleShowConnections(ViewModelPCBComponentBase comp, bool shouldShow);

        Task ShowLayerObjects(IList<string> layersToShow);
    }
}
