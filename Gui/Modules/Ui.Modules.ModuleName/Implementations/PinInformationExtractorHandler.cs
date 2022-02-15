using System;
using System.Globalization;
using System.Linq;
using PinInformationExtractor.Interfaces;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.TestCoverage;
using ProMik.SmartIct.JtagPinInformationExtractor.Interfaces;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Implementations
{
    public class PinInformationExtractorHandler : IPinInformationExtractorHandler
    {
        private readonly IDialogSelector dialogSelector;
        private readonly IJtagPinInformationCreator pinInformationExtractor;
        private readonly ILogger logger;
        private readonly ITestCoverageDataModel testCoverageDataModel;
        private readonly ISettingsData settingsData;
        private readonly IResultModel resultModel;

        public PinInformationExtractorHandler(
            IDialogSelector dialogSelector,
            IJtagPinInformationCreator pinInformationExtractor,
            ILogger logger,
            ITestCoverageDataModel testCoverageDataModel,
            ISettingsData settingsData,
            IResultModel resultModel)
        {
            this.settingsData = settingsData;
            this.dialogSelector = dialogSelector;
            this.resultModel = resultModel;
            this.logger = logger;
            this.testCoverageDataModel = testCoverageDataModel;
            this.pinInformationExtractor = pinInformationExtractor;
        }

        public void GetPinInformationJtagsIntoFile()
        {
            string fileName = string.Empty;
            if (!dialogSelector.OpenGenericDialog(
                DialogType.SAVEFILE,
                TextRessources.PinExportTitle,
                "No valid target to save selected!",
                out fileName))
            {
                return;
            }

            if (!fileName.ToLower(CultureInfo.CurrentCulture)[(fileName.Length - 4)..].Contains(".", StringComparison.Ordinal))
            {
                fileName += ".txt";
            }

            var result = pinInformationExtractor.CreatePinInformationFile(
                fileName,
                testCoverageDataModel.Jtags,
                testCoverageDataModel.PullUps,
                testCoverageDataModel.PullDowns,
                testCoverageDataModel.Others,
                settingsData.GndNetIdentifier.Split(";").ToList(),
                settingsData.PowerNetIdentifier.Split(";").ToList(),
                resultModel.Result.Components.ToList(),
                settingsData.JTAGPinIdentifier.Split(";").ToList());
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
