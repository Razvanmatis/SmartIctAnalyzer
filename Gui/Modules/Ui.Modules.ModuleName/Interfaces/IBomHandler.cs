using System;
using System.Collections.Generic;
using System.Text;
using Ui.Modules.ModuleName.Events;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IBomHandler
    {
        bool CheckAndPerformBomParsing();

        void ResetBomView();

        void OpenBomView(bool autoMode = false);

        void PerformAfterBomAction(SelectBomFinishEvent eventData);
    }
}
