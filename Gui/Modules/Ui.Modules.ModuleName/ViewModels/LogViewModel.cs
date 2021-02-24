using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Interfaces.Gui;
using log4net;
using Prism.Commands;
using Ui.Core.Mvvm;
using Ui.Modules.ModuleName.Helper;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ObservableCollection<LogMessage> messages = new ObservableCollection<LogMessage>();
        private LogMessage selectedMessage;

        public LogViewModel(ILogger logger)
        {
            logger.RegisterMethodCallback(AddLoggingEntry);
            CopyMessage = new DelegateCommand(CopyMessageToClipboard);
            Log.Info("Logging initialized...");
            AddLoggingEntry("The location of the logfile: " + Directory.GetCurrentDirectory() + "\\logs" + "\\" + DateTime.Now.Year + " - " + DateTime.Now.Month + " - " + DateTime.Now.Day + "\\SmartIctTool.log", LogCategory.INFO);
        }

        public ObservableCollection<LogMessage> Messages
        {
            get
            {
                return messages;
            }
        }

        public LogMessage SelectedMessage
        {
            get
            {
                return selectedMessage;
            }

            set
            {
                SetProperty(ref selectedMessage, value);
            }
        }

        public ICommand CopyMessage { get; set; }

        public List<LogMessage> SelectedItems { get; set; }

        private void AddLoggingEntry(string message, LogCategory category)
        {
            Messages.Insert(0, new LogMessage(message, category));
            if (category == LogCategory.INFO)
            {
                Log.Info(message);
            }
            else if (category == LogCategory.WARNING)
            {
                Log.Warn(message);
            }
            else
            {
                Log.Error(message);
            }
        }

        private void CopyMessageToClipboard()
        {
            string text = string.Empty;
            if (SelectedItems != null && SelectedItems.Count > 1)
            {
                foreach (LogMessage message in SelectedItems)
                {
                    text += message.DateTime + " " + message.Message + "\r\n";
                }
            }
            else if (selectedMessage != null)
            {
                text = selectedMessage.DateTime + " " + selectedMessage.Message;
            }

            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    Clipboard.SetDataObject(text);
                }
                catch (ArgumentNullException e)
                {
                    AddLoggingEntry(e.Message, LogCategory.ERROR);
                }
                catch (ExternalException e)
                {
                    AddLoggingEntry(e.Message, LogCategory.ERROR);
                }
            }
        }
    }
}
