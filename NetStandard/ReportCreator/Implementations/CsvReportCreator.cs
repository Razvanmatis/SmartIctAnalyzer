using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.Interfaces.PcbInvestigator.Enums;
using ProMik.SmartIct.Services.ReportCreator.Container;
using ProMik.SmartIct.Services.ReportCreator.Interfaces;
using ReportCreator;

namespace ProMik.SmartIct.Services.ReportCreator.Implementations
{
    public class CsvReportCreator : ICsvReportCreator
    {
        private const string NotTested = "NOT_TESTED";
        private const string JtagCom = "JTAG_COM";
        private const string Infrastructure = "Infrastructure";
        private const string Interconnection = "Interconnection";
        private const string INVALID = "INVALID";
        private const string Input = "Input";
        private const string Output = "Output";
        private const string High = "High";
        private const string Low = "Low";
        private readonly ILogger logger;
        private readonly IDialogSelector dialogSelector;
        private readonly Dictionary<string, List<PinTestTypeContainer>> pinContainerNotTested =
            new Dictionary<string, List<PinTestTypeContainer>>();

        public CsvReportCreator(ILogger logger, IDialogSelector dialogSelector)
        {
            this.logger = logger;
            this.dialogSelector = dialogSelector;
        }

        public Dictionary<string, List<PinTestTypeContainer>> ReportContent { get; } = new Dictionary<string, List<PinTestTypeContainer>>();

        public void UpdateReportContent(string jtagDevice, List<PinTestTypeContainer> pinContainer, string jtagDeviceRealname, List<PinTestTypeContainer> pinContainerNotTested)
        {
            if (!string.IsNullOrEmpty(jtagDevice))
            {
                ReportContent.Remove(jtagDevice);
                if (pinContainer != null && pinContainer.Count > 0)
                {
                    ReportContent.Add(jtagDevice, pinContainer);
                    this.pinContainerNotTested.Add(jtagDevice, pinContainerNotTested);
                }
            }
            else
            {
                ReportContent.Clear();
                this.pinContainerNotTested.Clear();
            }
        }

        public void CreateReportFile(string destinationFileName = "")
        {
            if (ReportContent == null || ReportContent.Count == 0)
            {
                logger.LogMessage("No content to extract for CSV. Please generate SVF files first...", LogCategory.WARNING);
                return;
            }

            string fileName = destinationFileName;
            if (string.IsNullOrEmpty(fileName) && !dialogSelector.OpenGenericDialog(
                DialogType.SAVEFILE,
                TextRessources.CsvSaveFile,
                "No valid target to save selected!",
                out fileName,
                TextRessources.CsvFilter))
            {
                return;
            }

            if (!fileName.ToLower(CultureInfo.CurrentCulture).EndsWith(".csv", StringComparison.InvariantCulture))
            {
                fileName += ".csv";
            }

            logger.LogMessage("Starting the export of a CSV file...", LogCategory.INFO);
            List<CsvReportElement> reportElements = new List<CsvReportElement>();
            foreach (var (jtag, pins) in ReportContent)
            {
                foreach (var pin in pins)
                {
                    foreach (var connected in pin.PinConnectionType.ConnectedComponents)
                    {
                        foreach (var testType in pin.PinConnectionType.BoundaryScanTypes)
                        {
                            if (testType == BoundaryScanTestTypeInternal.PullupPulldown)
                            {
                                reportElements.Add(new CsvReportElement(
                                    jtag,
                                    Enum.GetName(typeof(BoundaryScanTestTypeInternal), testType),
                                    connected.FunctionalAttributes.Ref,
                                    pin.PinTestObject.PinNumber,
                                    Enum.GetName(typeof(PcbComponentType), connected.ComponentType),
                                    string.Join(", ", pin.PinTestObject.Nets),
                                    GetControlState(BoundaryScanTestTypeInternal.Pullup),
                                    GetExpectedState(BoundaryScanTestTypeInternal.Pullup),
                                    GetTest(pin.PinConnectionType.PinConnectionType)));
                                reportElements.Add(new CsvReportElement(
                                    jtag,
                                    Enum.GetName(typeof(BoundaryScanTestTypeInternal), testType),
                                    connected.FunctionalAttributes.Ref,
                                    pin.PinTestObject.PinNumber,
                                    Enum.GetName(typeof(PcbComponentType), connected.ComponentType),
                                    string.Join(", ", pin.PinTestObject.Nets),
                                    GetControlState(BoundaryScanTestTypeInternal.Pulldown),
                                    GetExpectedState(BoundaryScanTestTypeInternal.Pulldown),
                                    GetTest(pin.PinConnectionType.PinConnectionType)));
                            }
                            else if (testType != BoundaryScanTestTypeInternal.Neighbouring)
                            {
                                reportElements.Add(new CsvReportElement(
                                    jtag,
                                    Enum.GetName(typeof(BoundaryScanTestTypeInternal), testType),
                                    connected.FunctionalAttributes.Ref,
                                    pin.PinTestObject.PinNumber,
                                    Enum.GetName(typeof(PcbComponentType), connected.ComponentType),
                                    string.Join(", ", pin.PinTestObject.Nets),
                                    GetControlState(testType),
                                    GetExpectedState(testType),
                                    GetTest(pin.PinConnectionType.PinConnectionType)));
                            }
                        }
                    }
                }

                foreach (var pin in pinContainerNotTested[jtag])
                {
                    if (pin.PinConnectionType.ConnectedComponents.Count > 0)
                    {
                        foreach (var connected in pin.PinConnectionType.ConnectedComponents)
                        {
                            reportElements.Add(new CsvReportElement(
                                jtag,
                                GetTestType(pin.PinConnectionType.BoundaryScanTypes, pin.PinConnectionType.PinConnectionType),
                                connected.FunctionalAttributes.Ref,
                                pin.PinTestObject.PinNumber,
                                Enum.GetName(typeof(PcbComponentType), connected.ComponentType),
                                string.Join(", ", pin.PinTestObject.Nets),
                                string.Empty,
                                string.Empty,
                                pin.PinConnectionType.PinConnectionType == PinConnectionType.JTAG ? Infrastructure : string.Empty));
                        }
                    }
                    else
                    {
                        reportElements.Add(new CsvReportElement(
                            jtag,
                            GetTestType(pin.PinConnectionType.BoundaryScanTypes, pin.PinConnectionType.PinConnectionType),
                            string.Empty,
                            pin.PinTestObject.PinNumber,
                            string.Empty,
                            string.Join(", ", pin.PinTestObject.Nets),
                            string.Empty,
                            string.Empty,
                            pin.PinConnectionType.PinConnectionType == PinConnectionType.JTAG ? Infrastructure : string.Empty));
                    }
                }
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Encoding = Encoding.UTF8 };
            try
            {
                using (var writer = new StreamWriter(fileName))
                {
                    using (var csv = new CsvWriter(writer, config))
                    {
                        csv.WriteRecords(reportElements);
                        logger.LogMessage("Successfully created CSV file at: " + fileName, LogCategory.INFO);
                    }
                }
            }
            catch (Exception e) when (
                e is UnauthorizedAccessException
                || e is ArgumentException
                || e is ArgumentNullException
                || e is DirectoryNotFoundException
                || e is PathTooLongException
                || e is IOException)
            {
                logger.LogMessage("Creation of CSV file failed: " + e.Message, LogCategory.ERROR);
            }
        }

        private static string GetTestType(List<BoundaryScanTestTypeInternal> boundaryScanTypes, PinConnectionType pinConnectionType)
        {
            if (pinConnectionType == PinConnectionType.JTAG)
            {
                return JtagCom;
            }
            else if (boundaryScanTypes.Count == 0)
            {
                return NotTested;
            }
            else
            {
                return Enum.GetName(typeof(BoundaryScanTestTypeInternal), boundaryScanTypes[0]);
            }
        }

        private static string GetTest(PinConnectionType pinConnectionType)
        {
            switch (pinConnectionType)
            {
                case PinConnectionType.JTAG: return Infrastructure;
                default: return Interconnection;
            }
        }

        private static string GetExpectedState(BoundaryScanTestTypeInternal testType)
        {
            switch (testType)
            {
                case BoundaryScanTestTypeInternal.Pullup: return High + ", " + Low;
                case BoundaryScanTestTypeInternal.Pulldown: return Low + ", " + High;
                case BoundaryScanTestTypeInternal.DirectGnd: return Low + ", " + Low;
                case BoundaryScanTestTypeInternal.DirectPower: return High + ", " + High;
                case BoundaryScanTestTypeInternal.UnknownControl:
                case BoundaryScanTestTypeInternal.UnknownInput: return High + ", " + Low;
                default: return INVALID;
            }
        }

        private static string GetControlState(BoundaryScanTestTypeInternal testType)
        {
            switch (testType)
            {
                case BoundaryScanTestTypeInternal.Pullup:
                case BoundaryScanTestTypeInternal.Pulldown:
                case BoundaryScanTestTypeInternal.DirectGnd:
                case BoundaryScanTestTypeInternal.DirectPower: return Input + ", " + Output;
                case BoundaryScanTestTypeInternal.UnknownControl: return Output + ", " + Output;
                case BoundaryScanTestTypeInternal.UnknownInput: return Input + ", " + Input;
                default: return INVALID;
            }
        }
    }
}
