using System.Diagnostics;
using Interfaces.Gui;

namespace TestDotNetCore
{
    public class DebugLogger : ILogger
    {
        public void LogMessage(string message, LogCategory category)
        {
            Debug.WriteLine(message + " with the category: " + category.ToString());
        }
    }
}
