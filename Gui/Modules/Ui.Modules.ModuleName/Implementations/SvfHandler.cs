using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Newtonsoft.Json;
using ProMik.BSDL.Interfaces;
using ProMik.Core.Interfaces.Bsdl;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.Interfaces.TestCoverage;
using ProMik.SmartIct.JtagPinInformationExtractor.Interfaces;
using ProMik.SmartIct.Services.ReportCreator.Interfaces;
using ProMik.SmartIct.Svf.SvfFileCreation.Interfaces;
using ProMik.SmartIct.Svf.SvfInterfaces.Container;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using ProMik.Svf.Contracts;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.ViewModels;

namespace Ui.Modules.ModuleName.Implementations
{
    public class SvfHandler : ISvfHandler
    {
        private const bool CalculationOfDefaultVectorByBsdlFileIsAllowed = true;
        private readonly IDialogSelector dialogSelector;
        private readonly ILogger logger;
        private readonly ISettingsStorageManager settingsStorageManager;
        private readonly IBSDLProcessor bsdlProcessor;
        private readonly ISvfPlayerHandler svfPlayer;
        private readonly IJtagPinInformationCreator pinInformationExtractor;
        private readonly ITestCoverageDataModel testCoverageDataModel;
        private readonly IPdfReportHandler pdfReportHandler;
        private readonly ICsvReportCreator csvReportHandler;
        private readonly IManifestHandler manifestHandler;
        private readonly IPackageAnalyzer packageAnalyzer;
        private readonly ISvfDataCreator svfDataCreator;
        private IResultModel resultModel;

        public SvfHandler(
            IManifestHandler manifestHandler,
            IDialogSelector dialogSelector,
            IBSDLProcessor packageService,
            ILogger logger,
            ISettingsStorageManager settingsStorageManager,
            ISvfPlayerHandler svfPlayer,
            IJtagPinInformationCreator pinInformationExtractor,
            ITestCoverageDataModel testCoverageDataModel,
            IPdfReportHandler pdfReportHandler,
            IPackageAnalyzer packageAnalyzer,
            IResultModel resultModel,
            ICsvReportCreator csvReportHandler,
            ISvfDataCreator svfDataCreator)
        {
            this.resultModel = resultModel;
            this.svfDataCreator = svfDataCreator;
            this.packageAnalyzer = packageAnalyzer;
            this.manifestHandler = manifestHandler;
            this.csvReportHandler = csvReportHandler;
            this.pdfReportHandler = pdfReportHandler;
            this.dialogSelector = dialogSelector;
            this.testCoverageDataModel = testCoverageDataModel;
            this.pinInformationExtractor = pinInformationExtractor;
            this.bsdlProcessor = packageService;
            this.logger = logger;
            this.svfPlayer = svfPlayer;
            this.settingsStorageManager = settingsStorageManager;
        }

        public Dictionary<string, List<ISvfData>> SavedSvfData { get; } = new Dictionary<string, List<ISvfData>>();

        public void PlaySvfFileHandler(ISvfExporter projectHandlerToUse, bool justFiles)
        {
            uint irLength;
            uint scanChainLength;
            IBSDLOutput package = null;
            Stream bsdlStream = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                bsdlStream = projectHandlerToUse.GetBsdlContenAsStream(out _);
            });

            if (bsdlStream == null)
            {
                logger.LogMessage("No valid BSDL file selected!", LogCategory.WARNING);
                return;
            }
            else
            {
                package = bsdlProcessor.ParseBSDLFile(bsdlStream)?.Result;
                bsdlStream.Close();
                if (package == null || package.BoundaryCells == null || package.PinCount == 0)
                {
                    logger.LogMessage("Received invalid BSDL package out of BSDL file!", LogCategory.ERROR);
                    return;
                }

                irLength = package.InstructionLength;
                scanChainLength = package.GetBoundaryScanChainLength();
                if (scanChainLength == 0)
                {
                    logger.LogMessage("No valid scan chain length of BSDL package found!", LogCategory.ERROR);
                    return;
                }
            }

            WrappedProgrammerSettings pgmSettings = GetInitPgmSettings(settingsStorageManager.GetStorageContent(), 1);
            pgmSettings = projectHandlerToUse.GetPgmSettings(
                string.Empty, pgmSettings, settingsStorageManager.GetStorageContent().AskForSvfSettings);
            string[] selectedSvfs = null;
            if (justFiles)
            {
                if (!dialogSelector.OpenGenericDialogForMultipleFileSelection(
                    TextRessources.SelectVsfFile, "No valid SVF file selected", out selectedSvfs))
                {
                    return;
                }
            }
            else
            {
                if (!dialogSelector.OpenGenericDialog(
                    DialogType.OPENFOLDER,
                    TextRessources.SelectSvfFolderForPlay,
                    "No valid SVF file selected",
                    out string selectedFolder))
                {
                    return;
                }

                selectedSvfs = GetFilesOfDir(selectedFolder);
            }

            if (!dialogSelector.OpenGenericDialog(
                DialogType.OPENFOLDER, TextRessources.SvfLogPath, "No valid SVF log output path selected", out string logPath))
            {
                return;
            }

            var settingsContent = settingsStorageManager.GetStorageContent();
            uint idCodeRegister = package.GetIdCodeRegister(out int amountToSkip);
            uint valueForMaskToSkip = package.GetValueMaskForSkippingIdCodeRegister(amountToSkip);
            if (svfPlayer.PlaySvfFile(
                package,
                scanChainLength,
                irLength,
                selectedSvfs,
                logPath,
                pgmSettings.Ip,
                pgmSettings.Port,
                pgmSettings.SupplyVoltage,
                pgmSettings.IoVoltage,
                idCodeRegister,
                pgmSettings.Frequency,
                pgmSettings.CableCompensation,
                valueForMaskToSkip,
                pgmSettings.Target,
                pgmSettings.Slot,
                package.GetIdCode(),
                package.GetPreload()))
            {
                logger.LogMessage("Successfully played the SVF file(s): " + selectedSvfs.Length, LogCategory.INFO);
            }
        }

        public void SaveCreatedSvfFilesIntoProject(string jtagSelection)
        {
            foreach (var (jtag, svfList) in SavedSvfData)
            {
                if (!jtagSelection.Equals(SelectionViewModel.ConsiderAllProjectfile) && !jtagSelection.Equals(jtag))
                {
                    continue;
                }

                manifestHandler.ExportSvfFiles(svfList);
            }
        }

        public void HandleSvfFileGeneration(
            ISvfExporter projectHandlerToUse,
            string jtagPinInformation = ProMik.SmartIct.JtagPinInformationExtractor.Implementations.JJtagPinInformationCreator.DEFJTAGPINIDENTIFIER)
        {
            string fileName = projectHandlerToUse.GetBasePath();
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            List<JtagConnectionInfo> result = pinInformationExtractor.GetAllPinInformation(
                testCoverageDataModel.Jtags,
                testCoverageDataModel.PullUps,
                testCoverageDataModel.PullDowns,
                testCoverageDataModel.Others,
                settingsStorageManager.GetStorageContent().GndNetIdentifier.Split(";").ToList(),
                settingsStorageManager.GetStorageContent().PowerNetIdentifier.Split(";").ToList(),
                resultModel.Result.Components.ToList(),
                settingsStorageManager.GetStorageContent().JTAGPinIdentifier.Split(";").ToList());
            if (result != null)
            {
                HandleSvfFileCreation(result, fileName, projectHandlerToUse);
            }
        }

        private static WrappedProgrammerSettings GetInitPgmSettings(ISettingsStorageModel settingsStorageModel, uint pgmId)
        {
            return new WrappedProgrammerSettings()
            {
                CableCompensation = settingsStorageModel.CableCompensation,
                Frequency = settingsStorageModel.Frequency,
                IoVoltage = settingsStorageModel.IoVoltageMv,
                Ip = settingsStorageModel.PgmIp,
                Port = settingsStorageModel.PgmPort,
                SupplyVoltage = settingsStorageModel.SupplyVoltageMv,
                Target = (int)settingsStorageModel.Target,
                Slot = (int)settingsStorageModel.Slot,
                Id = pgmId,
            };
        }

        private static int GetAmountOfBoundaryScanPins(IBSDLOutput package)
        {
            HashSet<string> cells = new HashSet<string>();
            foreach (var pin in package.BoundaryCells)
            {
                cells.Add(pin.Value.Port);
            }

            return cells.Count;
        }

        private static string GetListAsText(List<string> list)
        {
            return string.Join(", ", list);
        }

        

        private static ProgrammerSettings GetPgmSettingsFromWrappedOnes(WrappedProgrammerSettings pgmSettings)
        {
            return new ProgrammerSettings()
            {
                CableCompensation = pgmSettings.CableCompensation,
                Frequency = pgmSettings.Frequency,
                Id = pgmSettings.Id,
                IoVoltage = pgmSettings.IoVoltage,
                Port = pgmSettings.Port,
                SupplyVoltage = pgmSettings.SupplyVoltage,
                Target = pgmSettings.Target,
            };
        }

        private string[] GetFilesOfDir(string path)
        {
            List<string> svfs = new List<string>();
            svfs.AddRange(Directory.GetFiles(path));
            if (Directory.GetDirectories(path).Length > 0)
            {
                foreach (var folder in Directory.GetDirectories(path))
                {
                    svfs.AddRange(GetFilesOfDir(folder));
                }
            }

            return svfs.ToArray();
        }

        private void HandleSvfFileCreation(List<JtagConnectionInfo> result, string basePath, ISvfExporter projectHandlerToUse)
        {
            SavedSvfData.Clear();
            manifestHandler.BsdlContent.Clear();
            result.ForEach(x =>
            {
                x.ComponentRealName = x.ComponentName;
                x.ComponentName = dialogSelector.OpenInputDialog("Define alias name for JTAG device", x.ComponentName);
            });
            string selectedJtags = string.Empty;
            Thread thread = new Thread(new ThreadStart(
                () => selectedJtags = projectHandlerToUse.SelectJtagsToBeExported(result.Select(x => x.ComponentName).ToList())));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            uint pgmId = 0;
            foreach (var jtag in result)
            {
                if (!projectHandlerToUse.IsJtagIncludedForExport(jtag.ComponentName, selectedJtags))
                {
                    continue;
                }

                if (!projectHandlerToUse.CheckJtagDestinationLocation(
                    basePath, jtag.ComponentName, selectedJtags, result.Select(x => x.ComponentName).ToList()))
                {
                    logger.LogMessage("Error at preparing JTAG output location for: " + jtag.ComponentName, LogCategory.ERROR);
                    continue;
                }

                Stream bsdlStream = projectHandlerToUse.GetBsdlContenAsStream(out string fileName, jtag.ComponentName);
                if (bsdlStream == null)
                {
                    logger.LogMessage("Couldn't find BSDL file for device: " + jtag.ComponentName, LogCategory.ERROR);
                    continue;
                }

                var bsdlContent = bsdlProcessor.ParseBSDLFile(bsdlStream)?.Result;
                bsdlStream.Close();
                if (bsdlContent == null || bsdlContent.BoundaryCells == null || bsdlContent.BoundaryCells.Count == 0)
                {
                    logger.LogMessage("Received invalid BSDL package out of BSDL file!", LogCategory.ERROR);
                    continue;
                }

                if (!svfDataCreator.CheckPinConsistency(jtag, bsdlContent))
                {
                    if (dialogSelector.OpenMessageBox(
                        "PIN configuration does not match the BSDL file! Continue anyway?",
                        PlainDialogMode.OK_CANCEL) == PlainDialogResponse.ABORT)
                    {
                        logger.LogMessage(
                            "Skipping generation of SVF files for JTAG device: "
                            + jtag.ComponentName
                            + " cause invalid BSDL file was selected for it! Pinamount and content are not equal!",
                            LogCategory.ERROR);
                        continue;
                    }
                }

                var settingsContent = settingsStorageManager.GetStorageContent();
                pgmId++;
                WrappedProgrammerSettings pgmSettings = GetInitPgmSettings(settingsContent, pgmId);
                pgmSettings = projectHandlerToUse.GetPgmSettings(
                    jtag.ComponentName, pgmSettings, settingsContent.AskForSvfSettings);
                uint boundaryScanLength = bsdlContent.GetBoundaryScanChainLength();
                if (boundaryScanLength == 0)
                {
                    logger.LogMessage("No valid scan chain length of BSDL package found!", LogCategory.ERROR);
                    return;
                }

                uint idCodeRegister = bsdlContent.GetIdCodeRegister(out int amountToSkip);
                if (projectHandlerToUse is GeneralProjectHandler)
                {
                    manifestHandler.AddBsdlContentToDict(fileName, jtag.ComponentName, out _, out _);
                }

                manifestHandler.UpdateBsdlParameter(
                    jtag.ComponentName,
                    boundaryScanLength,
                    bsdlContent.InstructionLength,
                    idCodeRegister,
                    bsdlContent.GetIdCode(),
                    bsdlContent.GetPreload());

                manifestHandler.UpdatePgmSettings(jtag.ComponentName, GetPgmSettingsFromWrappedOnes(pgmSettings));
                dialogSelector.OpenMessageBox(
                    "Please make sure to have a programmer connected to the according JTAG device: "
                    + jtag.ComponentName
                    + " for being able to read out the default vector",
                    PlainDialogMode.OK);

                uint skipValue = bsdlContent.GetValueMaskForSkippingIdCodeRegister(amountToSkip);
                byte[] defaultVector = svfPlayer.GetDefaultVector(
                    boundaryScanLength,
                    bsdlContent.InstructionLength,
                    pgmSettings.Ip,
                    pgmSettings.Port,
                    pgmSettings.SupplyVoltage,
                    pgmSettings.IoVoltage,
                    out uint idCode,
                    idCodeRegister,
                    pgmSettings.Frequency,
                    pgmSettings.CableCompensation,
                    skipValue,
                    pgmSettings.Target,
                    pgmSettings.Slot,
                    bsdlContent.GetIdCode(),
                    bsdlContent.GetPreload());
                Debug.WriteLine(BitConverter.ToString(defaultVector).Replace("-", string.Empty));
                if (defaultVector == null || defaultVector.Length == 0)
                {
                    if (!CalculationOfDefaultVectorByBsdlFileIsAllowed)
                    {
                        logger.LogMessage(
                            "Were not able to read out the default vector for the device: "
                            + jtag.ComponentName + ". Further the calculation of it is not allowed!",
                            LogCategory.ERROR);
                        continue;
                    }

                    byte[] defVectorCreated = bsdlContent.GetDefaultVector();
                    logger.LogMessage(
                        "Created default vector from BSDL file: 0x"
                        + BitConverter.ToString(defVectorCreated),
                        LogCategory.INFO);
                    if (defVectorCreated != null && defVectorCreated.Length > 0)
                    {
                        logger.LogMessage(
                            "Were not able to read out the default vector for target: "
                            + jtag.ComponentName
                            + " and therefore using the created defaultVector from BSDL file",
                            LogCategory.WARNING);
                        defaultVector = defVectorCreated;
                        Debug.WriteLine(BitConverter.ToString(defaultVector).Replace("-", string.Empty));
                    }
                    else
                    {
                        logger.LogMessage(
                            "Were not able to get/create any valid default vector for target: "
                            + jtag.ComponentName,
                            LogCategory.ERROR);
                        continue;
                    }
                }

                svfDataCreator.PerformChecks(idCode, bsdlContent, idCodeRegister, amountToSkip, boundaryScanLength, defaultVector);
                // parse the BSDL content
                var resultPins = packageAnalyzer.GeneratePackage(bsdlContent);
                IBSDLPackage pinPackage = null;
                if (!resultPins.Success)
                {
                    logger.LogMessage(
                        "Neighbour detection couldn't be performed due to error: " + resultPins.ErrorMessage,
                        LogCategory.ERROR);
                }
                else
                {
                    pinPackage = resultPins.Result;
                }

                int amountBoundaryScanPins = GetAmountOfBoundaryScanPins(bsdlContent);
                logger.LogMessage(
                    "Generating SVF files based on maximum boundary scan compatible pin amount from BSDL file: "
                    + amountBoundaryScanPins,
                    LogCategory.INFO);

                var svfCreationResult = svfDataCreator.GetAllSvfDataFiles(jtag, pinPackage, bsdlContent, defaultVector, basePath);
                if (svfCreationResult.SvfFiles.Any())
                {
                    LogResults(svfCreationResult.SvfFiles);
                    svfDataCreator.UpdateTestTypeInternal(svfCreationResult.AllPinInformation, svfCreationResult.SvfFiles);

                    // export all svf files using the project handler
                    projectHandlerToUse.ExportSvfFiles(svfCreationResult.SvfFiles);
                    SavedSvfData.Add(jtag.ComponentName, svfCreationResult.SvfFiles);
                    svfCreationResult.AllPinInformation.RemoveAll(svf => !svfCreationResult.SvfFiles.Select(x => x.PinName).Contains(svf.PinTestObject.PinNumber));

                    // handle the report contents
                    pdfReportHandler.UpdateReportContent(
                        jtag.ComponentName,
                        svfCreationResult.AllPinInformation,
                        jtag.ComponentRealName,
                        new List<PinTestTypeContainer>());
                    csvReportHandler.UpdateReportContent(
                        jtag.ComponentName,
                        svfCreationResult.AllPinInformation,
                        jtag.ComponentRealName,
                        svfDataCreator.GetAllUntestedPins(jtag.Pins, svfCreationResult.AllPinInformation));
                }
                else
                {
                    logger.LogMessage(
                        "No valid SVF files generated for export for device: "
                        + jtag.ComponentName,
                        LogCategory.WARNING);
                }
            }
        }

        private void LogResults(List<ISvfData> svfDataResult)
        {
            Dictionary<Type, ISvfWriter> typesOfResults = new Dictionary<Type, ISvfWriter>();
            foreach (var svf in svfDataResult)
            {
                if (!typesOfResults.ContainsKey(svf.SvfWriter.GetType()))
                {
                    typesOfResults.Add(svf.SvfWriter.GetType(), svf.SvfWriter);
                }
            }

            foreach (var type in typesOfResults)
            {
                List<ISvfData> filteredDataByType = svfDataResult.Where(svf => svf.SvfWriter.GetType() == type.Key).ToList();
                if (type.Value.MultiPinType != MultiPinType.SameType)
                {
                    HashSet<string> pins = new HashSet<string>();
                    foreach (var svf in filteredDataByType)
                    {
                        if (!pins.Contains(svf.PinName))
                        {
                            pins.Add(svf.PinName);
                        }
                    }

                    List<string> pinsSorted = pins.ToList();
                    pinsSorted.Sort();
                    logger.LogMessage(
                        "Successfully determined "
                        + pins.Count
                        + " pins of the test case: "
                        + type.Value.Description
                        + " (" + GetListAsText(pinsSorted) + ")",
                        LogCategory.INFO);
                }
                else
                {
                    logger.LogMessage(
                        "Successfully determined one test of the test case: "
                        + type.Value.Description,
                        LogCategory.INFO);
                }
            }

            HashSet<string> totalPins = new HashSet<string>();
            foreach (var svf in svfDataResult)
            {
                totalPins.Add(svf.PinName);
            }

            logger.LogMessage(
                "Successfully determined SVF files for totally amount of different pins: "
                + totalPins.Count,
                LogCategory.INFO);
        }
    }
}
