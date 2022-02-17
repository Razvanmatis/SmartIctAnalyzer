using System.Diagnostics;
using ProMik.SmartIct.Interfaces.Gui;

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
