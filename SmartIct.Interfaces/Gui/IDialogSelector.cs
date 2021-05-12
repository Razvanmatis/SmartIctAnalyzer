namespace Interfaces.Gui
{
    public enum DialogType
    {
        /// <summary>
        /// OPENFILE
        /// </summary>
        OPENFILE,

        /// <summary>
        /// SAVEFILE
        /// </summary>
        SAVEFILE,

        /// <summary>
        /// OPENFOLDER
        /// </summary>
        OPENFOLDER,

        /// <summary>
        /// Open multiple files
        /// </summary>
        OPENMULTIPLEFILES,
    }

    public interface IDialogSelector
    {
        bool OpenGenericDialog(DialogType dialogType, string title, string errorMessage, out string selectedTarget, string filter = "");

        bool OpenGenericDialogForMultipleFileSelection(string title, string errorMessage, out string[] selectedTargets, string filter = "");
    }
}
