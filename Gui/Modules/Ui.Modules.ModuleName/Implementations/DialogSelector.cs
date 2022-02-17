using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Ookii.Dialogs.Wpf;
using ProMik.SmartIct.Interfaces.Gui;
using Ui.Modules.ModuleName.ViewModels;
using Ui.Modules.ModuleName.Views;
using MessageBox = System.Windows.MessageBox;

namespace Ui.Modules.ModuleName.Implementations
{
    public class DialogSelector : IDialogSelector
    {
        private readonly ILogger logger;

        public DialogSelector(ILogger logger)
        {
            this.logger = logger;
        }

        public bool OpenGenericDialog(
            DialogType dialogType,
            string title,
            string errorMessage,
            out string selectedTarget,
            string filter = "")
        {
            string content;
            if (dialogType == DialogType.OPENFILE)
            {
                VistaOpenFileDialog openDialog = new VistaOpenFileDialog() { Title = title, Filter = filter };
                openDialog.ShowDialog();
                content = openDialog.FileName;
            }
            else if (dialogType == DialogType.SAVEFILE)
            {
                VistaSaveFileDialog saveDialog = new VistaSaveFileDialog
                {
                    Title = title,
                    Filter = filter,
                };
                saveDialog.ShowDialog();
                content = saveDialog.FileName;
            }
            else if (dialogType == DialogType.OPENFOLDER)
            {
                VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog() { Description = title };
                folderDialog.ShowDialog();
                content = folderDialog.SelectedPath;
            }
            else
            {
                logger.LogMessage("Invalid dialog type received!", LogCategory.ERROR);
                selectedTarget = string.Empty;
                return false;
            }

            if (string.IsNullOrEmpty(content))
            {
                logger.LogMessage(errorMessage, LogCategory.WARNING);
                selectedTarget = string.Empty;
                return false;
            }

            selectedTarget = content;
            return true;
        }

        public bool OpenGenericDialogForMultipleFileSelection(
            string title,
            string errorMessage,
            out string[] selectedTargets,
            string filter = "")
        {
            VistaOpenFileDialog openDialog = new VistaOpenFileDialog() { Title = title, Filter = filter, Multiselect = true };
            openDialog.ShowDialog();
            selectedTargets = openDialog.FileNames;
            if (selectedTargets == null || selectedTargets.Length == 0)
            {
                logger.LogMessage(errorMessage, LogCategory.WARNING);
                selectedTargets = System.Array.Empty<string>();
                return false;
            }
            else
            {
                return true;
            }
        }

        public string OpenInputDialog(string title, string presetValue = "", System.Drawing.Size? size = null)
        {
            InputBoxView boxView = null;
            string result = string.Empty;
            Thread thread = new Thread(() =>
            {
                boxView = new InputBoxView();
                (boxView.DataContext as InputBoxViewModel).Height = size.HasValue ? size.Value.Height : 200;
                (boxView.DataContext as InputBoxViewModel).Width = size.HasValue ? size.Value.Width : 500;
                (boxView.DataContext as InputBoxViewModel).Title = title;
                (boxView.DataContext as InputBoxViewModel).Info = title;
                (boxView.DataContext as InputBoxViewModel).Text = presetValue;
                boxView.Topmost = true;
                boxView.Top = (Screen.PrimaryScreen.Bounds.Height / 2) - (boxView.Height / 2);
                boxView.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (boxView.Width / 2);
                boxView.ShowDialog();
                result = (boxView.DataContext as InputBoxViewModel).Text;
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return result;
        }

        public PlainDialogResponse OpenMessageBox(string title, PlainDialogMode mode)
        {
            if (mode == PlainDialogMode.OK)
            {
                MessageBox.Show(title);
                return PlainDialogResponse.OK;
            }
            else
            {
                MessageBoxResult box = MessageBox.Show(title, title, MessageBoxButton.OKCancel);
                if (box == MessageBoxResult.Cancel)
                {
                    return PlainDialogResponse.ABORT;
                }
                else
                {
                    return PlainDialogResponse.OK;
                }
            }
        }
    }
}
