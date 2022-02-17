namespace ProMik.SmartIct.Interfaces.Gui
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

    /// <summary>
    /// PlainDialogMode
    /// </summary>
    public enum PlainDialogMode
    {
        /// <summary>
        /// OK
        /// </summary>
        OK,

        /// <summary>
        /// OK and CANCEL
        /// </summary>
        OK_CANCEL,
    }

    /// <summary>
    /// PlainDialogResponse.
    /// </summary>
    public enum PlainDialogResponse
    {
        /// <summary>
        /// Ok
        /// </summary>
        OK,

        /// <summary>
        /// abort
        /// </summary>
        ABORT,
    }

    public interface IDialogSelector
    {
        bool OpenGenericDialog(
            DialogType dialogType, string title, string errorMessage, out string selectedTarget, string filter = "");

        bool OpenGenericDialogForMultipleFileSelection(
            string title, string errorMessage, out string[] selectedTargets, string filter = "");

        PlainDialogResponse OpenMessageBox(string title, PlainDialogMode mode);

        string OpenInputDialog(string title, string presetValue = "", System.Drawing.Size? size = null);
    }
}
