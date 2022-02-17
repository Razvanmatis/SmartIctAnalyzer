using ProMik.SmartIct.Interfaces.Gui;
using System;

namespace ProMik.SmartIct.Console.Contracts.Implementations
{
    public class ConsoleLogger : ILogger
    {
        private Action<string, LogCategory> action;
        private bool actionCallJustOnError = true;

        public ConsoleLogger(bool actionCallJustOnError = true)
        {
            this.actionCallJustOnError = actionCallJustOnError;
        }

        public void LogMessage(string message, LogCategory category)
        {
            if (!actionCallJustOnError || category == LogCategory.ERROR)
            {
                action?.Invoke(message, category);
            }
        }

        public void SetLoggerCallbackForError(Action<string, LogCategory> action)
        {
            this.action = action;
        }
    }
}
