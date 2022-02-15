using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.Svf.SvfFileCreation.Interfaces;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System.Collections.Generic;
using System.Linq;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinSameNetWriter
{
    public abstract class MultiPinSameNetWriterBase : MultiPinWriterBase, IMultiSvfFilesHandler, ISvfCreationContent
    {
        protected List<PinTestTypeContainer> pinsToUse;

        public MultiPinSameNetWriterBase(List<PinTestTypeContainer> pinsToUse)
        {
            this.pinsToUse = pinsToUse;
        }

        public override BoundaryScanTestTypeInternal BoundaryScanTestTypeInternal => BoundaryScanTestTypeInternal.Neighbouring;

        public abstract List<(bool isInput, bool expectedValue)> SvfCreationContent { get; }

        public override MultiPinType MultiPinType { get; } = MultiPinType.SameNetConnections;

        public List<ISvfData> GetNewSvfFilesOfMultiSvfs(List<ISvfData> svfData)
        {
            Dictionary<string, List<string>> netsMultipleTimesFound = GetSameNetConnections(pinsToUse);
            return GetSvfFilesForSameNetConnections(netsMultipleTimesFound, svfData);
        }

        /// <summary>
        /// get svf files for same net connections
        /// </summary>
        /// <param name="netsMultipleTimesFound">the dict containing netname -> list of pinnames</param>
        /// <param name="svfResults">all svfs</param>
        /// <param name="testTypeToUse">test type to be used</param>
        /// <returns></returns>
        protected virtual List<ISvfData> GetSvfFilesForSameNetConnections(
            Dictionary<string, List<string>> netsMultipleTimesFound, List<ISvfData> svfResults)
        {
            List<ISvfData> listNewResult = new List<ISvfData>();
            SvfCreationContent.ForEach(entryOfCreation =>
                listNewResult.AddRange(GetCreatedSameNetSvfForInputAndControl(
                    BasedTestType, netsMultipleTimesFound, svfResults, entryOfCreation.isInput, entryOfCreation.expectedValue)));
            return listNewResult;
        }

        /// <summary>
        /// create new svf for same net and considering a input type and expected value
        /// </summary>
        /// <param name="testTypeToUse">test type</param>
        /// <param name="netsMultipleTimesFound">netname -> all pin names</param>
        /// <param name="svfResults">all svfs</param>
        /// <param name="tdiControlInput">should the svf be a input type?</param>
        /// <param name="expectedValue">the expected value to be checked</param>
        /// <returns></returns>
        protected virtual List<ISvfData> GetCreatedSameNetSvfForInputAndControl(
            BoundaryScanTestType testTypeToUse,
            Dictionary<string, List<string>> netsMultipleTimesFound,
            List<ISvfData> svfResults,
            bool tdiControlInput,
            bool expectedValue)
        {
            List<ISvfData> listNewResult = new List<ISvfData>();

            // go through all netname -> list of pins
            foreach (var (net, listOfPins) in netsMultipleTimesFound)
            {
                ISvfData valueToClone = null;

                // find a specific svf file matching all criteria
                for (int idx = 0; idx < listOfPins.Count; idx++)
                {
                    valueToClone = svfResults.FirstOrDefault(svf =>
                        svf.PinName.Equals(listOfPins[idx])
                        && svf.TdiControlInput == tdiControlInput
                        && svf.ExpectedValue == expectedValue
                        && svf.TestType == testTypeToUse);

                    // if found, break
                    if (valueToClone != null)
                    {
                        break;
                    }
                }

                // if not found, continue looping
                if (valueToClone == null)
                {
                    continue;
                }

                // create a clone of found svf file
                ISvfData svfCloned = (ISvfData)valueToClone.Clone();

                // set based testtype
                svfCloned.BasedTestTypeForCloning = testTypeToUse;

                // set test type
                svfCloned.TestType = BoundaryScanTestType.Neighbouring;

                // set neighbour type
                svfCloned.MultiPinType = MultiPinType.SameNetConnections;

                svfCloned.SvfWriter = this;

                // search for all remaining sfvs of same net but not exactly the already found one
                for (int idx = 0; idx < listOfPins.Count; idx++)
                {
                    ISvfData svfToAdd = svfResults.FirstOrDefault(svf =>
                        !svf.PinName.Equals(svfCloned.PinName)
                        && svf.PinName.Equals(listOfPins[idx])
                        && svf.TdiControlInput == tdiControlInput
                        && svf.ExpectedValue == expectedValue
                        && svf.TestType == testTypeToUse);
                    if (svfToAdd != null)
                    {
                        svfCloned.Neighbours.Add(svfToAdd);
                    }
                }

                if (svfCloned.Neighbours.Count > 0)
                {
                    // apply all OR combined statements to new generated svf file
                    ApplyCombinedStrings(svfCloned);
                    listNewResult.Add(svfCloned);
                }
            }

            return listNewResult;
        }

        /// <summary>
        /// get all same net connections (netname -> list of pin names)
        /// </summary>
        /// <param name="pinsToUse">all pins</param>
        /// <returns></returns>
        protected virtual Dictionary<string, List<string>> GetSameNetConnections(List<PinTestTypeContainer> pinsToUse)
        {
            Dictionary<string, List<string>> netsMultipleTimesFound = new Dictionary<string, List<string>>();

            // iterate first of pin list
            foreach (var pin in pinsToUse)
            {

                // iterate second of pin list
                foreach (var pinInner in pinsToUse)
                {

                    // if not the same element
                    if (pin != pinInner)
                    {
                        // iterate nets of first pin
                        foreach (var net in pin.PinTestObject.Nets)
                        {
                            // check whether second pin nets contains first net
                            if (pinInner.PinTestObject.Nets.Contains(net))
                            {
                                // if yes check whether the dict already contains the list
                                if (!netsMultipleTimesFound.ContainsKey(net))
                                {
                                    netsMultipleTimesFound.Add(net, new List<string>());
                                }

                                // check and add the first pin into list
                                if (!netsMultipleTimesFound[net].Contains(pin.PinTestObject.PinNumber))
                                {
                                    netsMultipleTimesFound[net].Add(pin.PinTestObject.PinNumber);
                                }

                                // check and add the second pin into list
                                if (!netsMultipleTimesFound[net].Contains(pinInner.PinTestObject.PinNumber))
                                {
                                    netsMultipleTimesFound[net].Add(pinInner.PinTestObject.PinNumber);
                                }
                            }
                        }
                    }
                }
            }

            return netsMultipleTimesFound;
        }

    }
}
