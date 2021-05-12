using Interfaces.Gui;
using Ookii.Dialogs.Wpf;

namespace Ui.Modules.ModuleName.Implementations
{
    public class DialogSelector : IDialogSelector
    {
        private ILogger logger;

        public DialogSelector(ILogger logger)
        {
            this.logger = logger;
        }



        public bool OpenGenericDialog(DialogType dialogType, string title, string errorMessage, out string selectedTarget, string filter = "")
        {
            string content = string.Empty;
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

        public bool OpenGenericDialogForMultipleFileSelection(string title, string errorMessage, out string[] selectedTargets, string filter = "")
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
    }
}
