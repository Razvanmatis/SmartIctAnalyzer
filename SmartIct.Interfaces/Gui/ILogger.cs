using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces.Gui
{
    public enum LogCategory
    {
        /// <summary>
        /// info.
        /// </summary>
        INFO = 0,

        /// <summary>
        /// warning.
        /// </summary>
        WARNING = 1,

        /// <summary>
        /// error.
        /// </summary>
        ERROR = 2,
    }

    public interface ILogger
    {
        void LogMessage(string message, LogCategory category);

        void RegisterMethodCallback(Action<string, LogCategory> action);
    }
}
