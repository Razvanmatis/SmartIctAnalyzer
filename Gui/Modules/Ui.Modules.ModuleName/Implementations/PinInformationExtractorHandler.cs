using System;
using System.Globalization;
using Interfaces.Gui;
using PinInformationExtractor.Interfaces;
using TestCoverage.Interfaces;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Implementations
{
    public class PinInformationExtractorHandler : IPinInformationExtractorHandler
    {
        private readonly IDialogSelector dialogSelector;
        private readonly IPinInformationExtractor pinInformationExtractor;
        private readonly ILogger logger;
        private readonly ITestCoverageDataModel testCoverageDataModel;

        public PinInformationExtractorHandler(
            IDialogSelector dialogSelector,
            IPinInformationExtractor pinInformationExtractor,
            ILogger logger,
            ITestCoverageDataModel testCoverageDataModel)
        {
            this.dialogSelector = dialogSelector;
            this.logger = logger;
            this.testCoverageDataModel = testCoverageDataModel;
            this.pinInformationExtractor = pinInformationExtractor;
        }

        public void GetPinInformationJtagsIntoFile(string jtagPinInformation = PinInformationExtractor.Implementations.PinInformationExtractor.DEFJTAGPINIDENTIFIER)
        {
            string fileName = string.Empty;
            if (!dialogSelector.OpenGenericDialog(DialogType.SAVEFILE, TextRessources.PinExportTitle, "No valid target to save selected!", out fileName))
            {
                return;
            }

            if (!fileName.ToLower(CultureInfo.CurrentCulture)[(fileName.Length - 4)..].Contains(".", StringComparison.Ordinal))
            {
                fileName += ".txt";
            }

            var result = pinInformationExtractor.CreatePinInformationFile(fileName, testCoverageDataModel.Ics, testCoverageDataModel.PullUps, testCoverageDataModel.PullDowns, jtagPinInformation);
            if (result != null && result.Count > 0)
            {
                logger.LogMessage("PIN information exported into file " + fileName, LogCategory.INFO);
            }
            else
            {
                logger.LogMessage("No PIN information exported into file performed for: " + fileName, LogCategory.WARNING);
            }
        }
    }
}
