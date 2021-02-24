using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.Gui;

namespace Ui.Modules.ModuleName.Helper
{
    public class Logger : ILogger
    {
        private Action<string, LogCategory> action;

        public void LogMessage(string message, LogCategory category)
        {
            if (action != null)
            {
                action(message, category);
            }
        }

        public void RegisterMethodCallback(Action<string, LogCategory> action)
        {
            this.action = action;
        }
    }
}
