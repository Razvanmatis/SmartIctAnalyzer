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
    }

    public interface IDialogSelector
    {
        bool OpenGenericDialog(DialogType dialogType, string title, string errorMessage, out string selectedTarget, string filter = "");
    }
}
