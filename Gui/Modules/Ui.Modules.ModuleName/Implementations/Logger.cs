using ProMik.Core.Interfaces.Events;
using ProMik.Core.Interfaces.Events.UIEvents;
using ProMik.SmartIct.Interfaces.Gui;

namespace Ui.Modules.ModuleName.Implementations
{
    public class Logger : ILogger
    {
        private readonly IEventService eventService;

        public Logger(IEventService eventService)
        {
            this.eventService = eventService;
        }

        public void LogMessage(string message, LogCategory category)
        {
            eventService.Publish(new ProMikLogEvent(message) { Level = GetLogLevel(category) });
        }

        private static LogLevelEnum GetLogLevel(LogCategory category)
        {
            if (category == LogCategory.INFO)
            {
                return LogLevelEnum.INFO;
            }
            else if (category == LogCategory.WARNING)
            {
                return LogLevelEnum.WARN;
            }
            else
            {
                return LogLevelEnum.ERROR;
            }
        }
    }
}
