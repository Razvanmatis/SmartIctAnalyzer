using System.Collections.Generic;
using System.Windows;
using Interfaces.Gui;
using PinInformationExtractor.Helper;
using PinInformationExtractor.Interfaces;
using SVFHelper.Implementations;
using SVFHelper.Interfaces;
using TestCoverage.Interfaces;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Implementations
{
    public class SvfHandler : ISvfHandler
    {
        private readonly IDialogSelector dialogSelector;
        private readonly ILogger logger;
        private readonly ISettingsStorageManager settingsStorageManager;
        private readonly IPackageService packageService;
        private readonly ISvfPlayer svfPlayer;
        private readonly ISVFHelper svfHelper;
        private readonly IPinInformationExtractor pinInformationExtractor;
        private readonly ITestCoverageDataModel testCoverageDataModel;

        public SvfHandler(
            IDialogSelector dialogSelector,
            IPackageService packageService,
            ILogger logger,
            ISettingsStorageManager settingsStorageManager,
            ISvfPlayer svfPlayer,
            ISVFHelper svfHelper,
            IPinInformationExtractor pinInformationExtractor,
            ITestCoverageDataModel testCoverageDataModel)
        {
            this.dialogSelector = dialogSelector;
            this.testCoverageDataModel = testCoverageDataModel;
            this.pinInformationExtractor = pinInformationExtractor;
            this.packageService = packageService;
            this.logger = logger;
            this.svfHelper = svfHelper;
            this.svfPlayer = svfPlayer;
            this.settingsStorageManager = settingsStorageManager;
        }

        public void PlaySvfFileHandler()
        {
            if (!dialogSelector.OpenGenericDialog(DialogType.OPENFILE, TextRessources.SelectBsdlFile, "No valid BSDL file selected!", out string bsdlFile))
            {
                return;
            }

            var package = packageService.GetPackage(bsdlFile);
            if (package == null || package.BoundaryCells == null || package.BoundaryCells.Count == 0)
            {
                logger.LogMessage("Received invalid BSDL package out of BSDL file!", LogCategory.ERROR);
                return;
            }

            if (!dialogSelector.OpenGenericDialog(DialogType.OPENFILE, TextRessources.SelectVsfFile, "No valid SVF file selected", out string selectedSvf))
            {
                return;
            }

            if (!dialogSelector.OpenGenericDialog(DialogType.SAVEFILE, TextRessources.SvfLogPath, "No valid SVF log output path selected", out string logPath))
            {
                return;
            }

            var settingsContent = settingsStorageManager.GetStorageContent();
            if (svfPlayer.PlaySvfFile((uint)package.PinMap.Count, package.InstructionLength, selectedSvf, logPath, settingsContent.PgmIp, settingsContent.PgmPort, settingsContent.SupplyVoltageMv, settingsContent.IoVoltageMv))
            {
                logger.LogMessage("Successfully played the SVF file: " + selectedSvf, LogCategory.INFO);
            }
        }

        public void HandleSvfFileGeneration(IProjectHandler projectHandlerToUse)
        {
            if (!dialogSelector.OpenGenericDialog(DialogType.OPENFOLDER, TextRessources.SelectSvfFolder, "No valid path selected", out string fileName))
            {
                return;
            }

            List<JtagConnectionInfo> result = pinInformationExtractor.GetAllPinInformation(testCoverageDataModel.Ics, testCoverageDataModel.PullUps, testCoverageDataModel.PullDowns);
            if (result != null)
            {
                HandleSvfFileCreation(result, fileName, projectHandlerToUse);
            }
        }

        private void HandleSvfFileCreation(List<JtagConnectionInfo> result, string basePath, IProjectHandler projectHandlerToUse)
        {
            foreach (var jtag in result)
            {
                if (!dialogSelector.OpenGenericDialog(DialogType.OPENFILE, "Select BSDL file for JTAG device: " + jtag.ComponentName, "No valid BSDL file selected!", out string bsdlFile))
                {
                    continue;
                }

                var package = packageService.GetPackage(bsdlFile);
                if (package == null || package.BoundaryCells == null || package.BoundaryCells.Count == 0)
                {
                    logger.LogMessage("Received invalid BSDL package out of BSDL file!", LogCategory.ERROR);
                    continue;
                }

                MessageBox.Show("Please make sure to have a programmer connected to the according JTAG device: " + jtag.ComponentName + " for being able to read out the default vector");
                var settingsContent = settingsStorageManager.GetStorageContent();
                byte[] defaultVector = svfPlayer.GetDefaultVector((uint)package.PinMap.Count, package.InstructionLength, settingsContent.PgmIp, settingsContent.PgmPort, settingsContent.SupplyVoltageMv, settingsContent.IoVoltageMv);
                if (defaultVector == null || defaultVector.Length == 0)
                {
                    logger.LogMessage("Were not able to get a valid default vector for target: " + jtag.ComponentName, LogCategory.ERROR);
                    continue;
                }

                int realLength = package.PinMap.Count / 8;
                if (package.PinMap.Count % 8 > 0)
                {
                    realLength++;
                }

                if (defaultVector.Length != realLength)
                {
                    logger.LogMessage("Created a default vector with invalid length! Length needs to be " + realLength + ", but it was " + defaultVector.Length, LogCategory.WARNING);
                }

                ISvfWriter writer = new PullDownSvfWriter(svfHelper);
                List<ISvfData> svfDataResult = new List<ISvfData>();
                svfDataResult.AddRange(writer.GetSVFFiles(basePath, jtag.ComponentName, pinInformationExtractor.GetAllGroundPins(jtag.Pins), package, defaultVector));
                writer = new PullUpSvfWriter(svfHelper);
                svfDataResult.AddRange(writer.GetSVFFiles(basePath, jtag.ComponentName, pinInformationExtractor.GetAllPowerPins(jtag.Pins), package, defaultVector));
                projectHandlerToUse.ExportSvfFiles(svfDataResult);
            }
        }
    }
}
