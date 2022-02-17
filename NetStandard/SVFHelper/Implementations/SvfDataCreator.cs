using Newtonsoft.Json;
using ProMik.Core.Interfaces.Bsdl;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Svf.SvfFileCreation.Implementations.SinglePinWriter;
using ProMik.Svf.Contracts;
using ProMik.SmartIct.Svf.SvfFileCreation.Helper;
using ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinNeighbourWriter;
using ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinSameNetWriter;
using ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinSameTypeWriter;
using System;
using System.Collections.Generic;
using System.Linq;
using ProMik.SmartIct.Svf.SvfFileCreation.Interfaces;
using ProMik.SmartIct.Svf.SvfInterfaces.Container;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations
{
    public class SvfDataCreator : ISvfDataCreator
    {
        private readonly ILogger logger;
        public SvfDataCreator(ILogger logger)
        {
            this.logger = logger;
        }

        public List<PinTestTypeContainer> GetAllUntestedPins(List<PinConnectionInfo> pins, List<PinTestTypeContainer> allTestedPins)
        {
            List<PinTestTypeContainer> list = new List<PinTestTypeContainer>();
            pins.Where(pin => pin.PinConnections.Any(con => con.PinConnectionType == PinConnectionType.JTAG)).ToList().ForEach(pin =>
            {
                List<BoundaryScanTestTypeInternal> boundaryScanTestTypes = new List<BoundaryScanTestTypeInternal>();
                if (allTestedPins.Any(x => x.PinTestObject.PinNumber.Equals(pin.PinNumber)))
                {
                    boundaryScanTestTypes = allTestedPins.FirstOrDefault(x =>
                        x.PinTestObject.PinNumber.Equals(pin.PinNumber)).PinConnectionType.BoundaryScanTypes;
                }

                list.Add(new PinTestTypeContainer()
                {
                    PinTestObject = new PinTestObject(pin.PinNumber, pin.NetNames),
                    PinConnectionType = new PinConnectionTypeContainer()
                    {
                        BoundaryScanTypes = boundaryScanTestTypes,
                        ConnectedComponents = new List<IPCBComponent>(GetConnectedComponents(pin)),
                        PinConnectionType = PinConnectionType.JTAG,
                    },
                });
            });

            pins.Where(pin => !allTestedPins.Any(tested => tested.PinTestObject.PinNumber.Equals(pin.PinNumber)) && !list.Any(x =>
                x.PinTestObject.PinNumber.Equals(pin.PinNumber))).ToList().ForEach(pin => list.Add(new PinTestTypeContainer()
                {
                    PinTestObject = new PinTestObject(pin.PinNumber, pin.NetNames),
                    PinConnectionType = new PinConnectionTypeContainer()
                    {
                        BoundaryScanTypes = new List<BoundaryScanTestTypeInternal>(),
                        ConnectedComponents = new List<IPCBComponent>(GetConnectedComponents(pin)),
                        PinConnectionType = pin.PinConnections[0].PinConnectionType,
                    },
                }));

            return list;
        }

        public void UpdateTestTypeInternal(List<PinTestTypeContainer> allPins, List<ISvfData> svfResult)
        {
            HashSet<BoundaryScanTestTypeInternal> testTypes = new HashSet<BoundaryScanTestTypeInternal>();
            foreach (var svf in svfResult)
            {
                testTypes.Add(svf.SvfWriter.BoundaryScanTestTypeInternal);
            }

            foreach (var type in testTypes)
            {
                HashSet<string> pins = new HashSet<string>();
                List<ISvfData> dataFilteredByTestType = svfResult
                    .Where(x => x.SvfWriter.BoundaryScanTestTypeInternal == type).ToList();
                foreach (var svf in dataFilteredByTestType)
                {
                    pins.Add(svf.PinName);
                }

                foreach (var pinToFind in pins)
                {
                    foreach (var pin in allPins)
                    {
                        if (pin.PinTestObject.PinNumber.Equals(pinToFind))
                        {
                            if (!pin.PinConnectionType.BoundaryScanTypes.Contains(type))
                            {
                                pin.PinConnectionType.BoundaryScanTypes.Add(type);
                            }
                        }
                    }
                }
            }
        }

        public SvfCreationResult GetAllSvfDataFiles(
            JtagConnectionInfo jtag,
            IBSDLPackage pinPackage,
            IBSDLOutput bsdlContent,
            byte[] defaultVector,
            string basePath = "")
        {
            List<ISvfData> svfDataResult = new List<ISvfData>();
            List<PinTestTypeContainer> allPins = new List<PinTestTypeContainer>();
            var mainInformation = (basePath, jtag.ComponentName, bsdlContent, defaultVector);

            // get all pull downs svf files
            GenerateSvfFilesForSpecificType(
                new SinglePinPullDownWriter(logger),
                () => jtag.GetAllPullDownPins(),
                allPins,
                svfDataResult,
                mainInformation);

            // get all direct gnd svf files
            GenerateSvfFilesForSpecificType(
                new SinglePinDirectGndWriter(logger),
                () => jtag.GetAllDirectGndPins(),
                allPins,
                svfDataResult,
                mainInformation);

            // get all pull up svf files
            GenerateSvfFilesForSpecificType(
                new SinglePinPullUpWriter(logger),
                () => jtag.GetAllPullUpPins(),
                allPins,
                svfDataResult,
                mainInformation);

            // get all direct power svf files
            GenerateSvfFilesForSpecificType(
                new SinglePinDirectPowerWriter(logger),
                () => jtag.GetAllDirectPowerPins(),
                allPins,
                svfDataResult,
                mainInformation);

            // get all unknown control svf files
            GenerateSvfFilesForSpecificType(
                new SinglePinUnknownComponentsControlWriter(logger),
                () => jtag.GetAllOtherPins(),
                allPins,
                svfDataResult,
                mainInformation);

            // get all unknown input svf files
            GenerateSvfFilesForSpecificType(
                new SinglePinUnknownComponentsInputWriter(logger),
                () => jtag.GetAllOtherPins(),
                allPins,
                svfDataResult,
                mainInformation);
            if (svfDataResult.Any())
            {
                svfDataResult.AddRange(new SinglePinPullUpPullDownWriter(logger).GetNewSvfFilesOfMultiSvfs(svfDataResult));

                // determine all svf for same type
                svfDataResult.AddRange(new MultiPinSameTypeDirectGndWriter().GetNewSvfFilesOfMultiSvfs(svfDataResult));
                svfDataResult.AddRange(new MultiPinSameTypeDirectPowerWriter().GetNewSvfFilesOfMultiSvfs(svfDataResult));
                svfDataResult.AddRange(new MultiPinSameTypePullDownWriter().GetNewSvfFilesOfMultiSvfs(svfDataResult));
                svfDataResult.AddRange(new MultiPinSameTypePullUpWriter().GetNewSvfFilesOfMultiSvfs(svfDataResult));
                svfDataResult.AddRange(new MultiPinSameTypePullUpPullDownWriter().GetNewSvfFilesOfMultiSvfs(svfDataResult));
                svfDataResult.AddRange(
                    new MultiPinSameTypeUnknownComponentsControlWriter().GetNewSvfFilesOfMultiSvfs(svfDataResult));
                svfDataResult.AddRange(
                    new MultiPinSameTypeUnknownComponentsInputWriter().GetNewSvfFilesOfMultiSvfs(svfDataResult));

                // determine all svf for same net
                svfDataResult.AddRange(new MultiPinSameNetDirectGndWriter(allPins).GetNewSvfFilesOfMultiSvfs(svfDataResult));
                svfDataResult.AddRange(
                    new MultiPinSameNetDirectPowerWriter(allPins).GetNewSvfFilesOfMultiSvfs(svfDataResult));
                svfDataResult.AddRange(new MultiPinSameNetPullDownWriter(allPins).GetNewSvfFilesOfMultiSvfs(svfDataResult));
                svfDataResult.AddRange(new MultiPinSameNetPullUpWriter(allPins).GetNewSvfFilesOfMultiSvfs(svfDataResult));
                svfDataResult.AddRange(
                    new MultiPinSameNetPullUpPullDownWriter(allPins).GetNewSvfFilesOfMultiSvfs(svfDataResult));
                svfDataResult.AddRange(
                    new MultiPinSameNetUnknownComponentsControlWriter(allPins).GetNewSvfFilesOfMultiSvfs(svfDataResult));
                svfDataResult.AddRange(
                    new MultiPinSameNetUnknownComponentsInputWriter(allPins).GetNewSvfFilesOfMultiSvfs(svfDataResult));
                if (pinPackage != null)
                {
                    // determine all svf which are containing all physical neighbours of a pin in dependency on the BSDL content
                    svfDataResult.AddRange(
                        new MultiPinNeighbourDirectGndWriter(pinPackage, bsdlContent)
                        .GetNewSvfFilesOfMultiSvfs(svfDataResult));
                    svfDataResult.AddRange(
                        new MultiPinNeighbourDirectPowerWriter(pinPackage, bsdlContent)
                        .GetNewSvfFilesOfMultiSvfs(svfDataResult));
                    svfDataResult.AddRange(
                        new MultiPinNeighbourPullDownWriter(pinPackage, bsdlContent)
                        .GetNewSvfFilesOfMultiSvfs(svfDataResult));
                    svfDataResult.AddRange(
                        new MultiPinNeighbourPullUpWriter(pinPackage, bsdlContent).GetNewSvfFilesOfMultiSvfs(svfDataResult));
                    svfDataResult.AddRange(
                        new MultiPinNeighbourPullUpPullDownWriter(pinPackage, bsdlContent)
                        .GetNewSvfFilesOfMultiSvfs(svfDataResult));
                    svfDataResult.AddRange(
                        new MultiPinNeighbourUnknownComponentsControlWriter(pinPackage, bsdlContent)
                        .GetNewSvfFilesOfMultiSvfs(svfDataResult));
                }
            }

            // generate the JSON header for each svf
            AddJsonHeaderIntoSvfFiles(svfDataResult);
            return new SvfCreationResult(svfDataResult, allPins);
        }

        public bool CheckPinConsistency(JtagConnectionInfo jtag, IBSDLOutput package)
        {
            if (jtag.TotalAmountOfPins != package.PinCount)
            {
                logger.LogMessage(
                    "The amount of project pins: "
                    + jtag.TotalAmountOfPins
                    + " does not equal the value from BSDL file: "
                    + package.PinCount,
                    LogCategory.ERROR);
                return false;
            }

            string missingPins = string.Empty;
            missingPins = string.Join(", ", jtag.Pins.Where(pin =>
                !package.PinMap.Keys.Contains(pin.PinNumber)
                && !package.PinMap.Values.Contains(pin.PinNumber)).Select(pin => pin.PinNumber));
            if (missingPins.Length > 0)
            {
                logger.LogMessage("The following pins were not found within the BSDL file: " + missingPins, LogCategory.ERROR);
            }

            return string.IsNullOrEmpty(missingPins);
        }

        public void PerformChecks(
           uint idCode,
           IBSDLOutput bsdlResult,
           uint jtagIdCode,
           int amountToSkip,
           uint scanChainLength,
           byte[] defaultVector)
        {
            if (idCode == 0 || !bsdlResult.CheckIdCodeRegister(idCode, jtagIdCode, amountToSkip))
            {
                logger.LogMessage(
                    "ID code from device: "
                    + idCode
                    + " is not matching that one from BSDL file: "
                    + jtagIdCode,
                    LogCategory.WARNING);
            }

            List<int> missmatches = bsdlResult.GetCalcDefVectorMissmatchesForPinPositions(defaultVector);
            if (missmatches == null)
            {
                logger.LogMessage(
                    "Missmatches between the calculated default vector and the read out could not be detected!",
                    LogCategory.ERROR);
            }
            else if (missmatches.Count > 0)
            {
                string positions = string.Join(", ", missmatches);
                logger.LogMessage(
                    "Missmatches between the calculated default vector and the read out detected at pin positions: "
                    + positions,
                    LogCategory.WARNING);
            }

            uint realLength = scanChainLength / 8;
            if (scanChainLength % 8 > 0)
            {
                realLength++;
            }

            if (defaultVector.Length != realLength)
            {
                logger.LogMessage(
                    "Created a default vector with invalid length! Length needs to be "
                    + realLength + ", but it was " + defaultVector.Length,
                    LogCategory.WARNING);
            }
        }

        private static IEnumerable<IPCBComponent> GetConnectedComponents(PinConnectionInfo pin)
        {
            List<IPCBComponent> list = new List<IPCBComponent>();
            if (pin.ConnectedJtagDevice != null)
            {
                list.AddRange(pin.ConnectedJtagDevice.Select(x => x.JTAG).ToList());
            }

            foreach (var con in pin.PinConnections)
            {
                if (con.ConnectedComponents != null)
                {
                    list.AddRange(con.ConnectedComponents);
                }
            }

            return list;
        }

        private static void AddJsonHeaderIntoSvfFiles(
            List<ISvfData> svfDataResult)
        {
            MetaDataSvfFile metaFile = new MetaDataSvfFile();
            foreach (var svf in svfDataResult)
            {
                metaFile.PinInformation = svf.GetPinInformation();
                metaFile.TestType = svf.TestType;
                svf.Content.Insert(0, "//" + JsonConvert.SerializeObject(metaFile, Formatting.None));
            }
        }

        private static List<ISvfData> GenerateSvfFilesForSpecificType(
            SinglePinWriterBase writer,
            Func<List<PinTestTypeContainer>> pinsToUse,
            List<PinTestTypeContainer> allPins,
            List<ISvfData> svfDataResult,
            (string BasePath, string JtagComponentName, IBSDLOutput BsdlContent, byte[] DefaultVector) mainInformation)
        {
            List<PinTestTypeContainer> pinsForUsage = pinsToUse();
            allPins.AddRange(AddNewPins(pinsForUsage, allPins));
            List<ISvfData> results = writer.GetSVFFiles(
                mainInformation.BasePath,
                mainInformation.JtagComponentName,
                pinsForUsage,
                mainInformation.BsdlContent,
                mainInformation.DefaultVector);
            results.RemoveAll(svf => svf.Content.Count == 0
            || string.IsNullOrEmpty(svf.TdiString)
            || string.IsNullOrEmpty(svf.TdoString));
            svfDataResult.AddRange(results);
            return results;
        }

        private static List<PinTestTypeContainer> AddNewPins(List<PinTestTypeContainer> pinsToUse, List<PinTestTypeContainer> allPins)
        {
            return new List<PinTestTypeContainer>(pinsToUse.Where(x => !allPins.Any(y => x.Equals(y))));
        }
    }
}
