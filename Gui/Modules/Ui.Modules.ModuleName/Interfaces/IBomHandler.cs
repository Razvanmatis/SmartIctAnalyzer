using System;
using System.Collections.Generic;
using System.Text;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IBomHandler
    {
        bool CheckAndPerformBomParsing();

        void ResetBomView();

        void OpenBomView(bool autoMode = false);
    }
}
