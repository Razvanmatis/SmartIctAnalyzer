using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Interfaces.Gui;

namespace TestDotNetCore
{
    public class DebugLogger : ILogger
    {
        public void LogMessage(string message, LogCategory category)
        {
            Debug.WriteLine(message + " with the category: " + category.ToString());
        }

        public void RegisterMethodCallback(Action<string, LogCategory> action)
        {
        }
    }
}
