using System;
using System.Collections.Generic;
using System.Text;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface ISvfHandler
    {
        void HandleSvfFileGeneration(IProjectHandler projectHandlerToUse);

        void PlaySvfFileHandler();
    }
}
