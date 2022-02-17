using System.Globalization;
using System.Windows.Media;
using ProMik.SmartIct.Interfaces.Gui;

namespace Ui.Modules.ModuleName.Helper
{
    public class LogMessage
    {
        private readonly string dateTime;
        private readonly string message;
        private readonly SolidColorBrush brush;

        public LogMessage(string message, LogCategory category)
        {
            this.dateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.CurrentInfo);
            this.message = message;
            brush = new SolidColorBrush();
            if (category == LogCategory.INFO)
            {
                brush.Color = Color.FromRgb(0, 255, 0);
            }
            else if (category == LogCategory.WARNING)
            {
                brush.Color = Color.FromRgb(255, 165, 0);
            }
            else
            {
                brush.Color = Color.FromRgb(255, 0, 0);
            }
        }

        public string DateTime
        {
            get
            {
                return dateTime;
            }
        }

        public string Message
        {
            get
            {
                return message;
            }
        }

        public SolidColorBrush Background
        {
            get
            {
                return brush;
            }
        }
    }
}
