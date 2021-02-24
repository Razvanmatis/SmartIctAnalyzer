using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Media;
using Interfaces.Gui;

namespace Ui.Modules.ModuleName.Helper
{
    public class LogMessage
    {
        private readonly string dateTime;
        private readonly string message;
        private readonly LogCategory category;
        private SolidColorBrush brush;

        public LogMessage(string message, LogCategory category)
        {
            this.dateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.CurrentInfo);
            this.message = message;
            this.category = category;
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
