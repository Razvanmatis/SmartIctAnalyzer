using ProMik.Core.Interfaces.Bsdl;
using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.Svf.SvfFileCreation.Interfaces;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System.Collections.Generic;
using System.Linq;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinNeighbourWriter
{
    public abstract class MultiPinWriterNeighbourBase : MultiPinWriterBase, IMultiSvfFilesHandler
    {
        protected IBSDLPackage pinPackage;
        protected IBSDLOutput bsdlOutput;

        public MultiPinWriterNeighbourBase(IBSDLPackage pinPackage, IBSDLOutput bsdlOutput)
        {
            this.pinPackage = pinPackage;
            this.bsdlOutput = bsdlOutput;
        }

        public override BoundaryScanTestTypeInternal BoundaryScanTestTypeInternal => BoundaryScanTestTypeInternal.Neighbouring;

        public override MultiPinType MultiPinType { get; } = MultiPinType.Neighbours;

        public List<ISvfData> GetNewSvfFilesOfMultiSvfs(List<ISvfData> svfData)
        {
            return DetermineNeighbours(svfData);
        }

        /// <summary>
        /// determine all svf files for pins which have physically located neighbours
        /// </summary>
        /// <param name="svfDataResult">all svf files</param>
        protected virtual List<ISvfData> DetermineNeighbours(
            List<ISvfData> svfDataResult)
        {
            List<ISvfData> listFilteredForOutputs = svfDataResult.Where(
                x => x.TestType != BoundaryScanTestType.Neighbouring
                && x.TestType != BoundaryScanTestType.Undefined
                && x.TestType != BoundaryScanTestType.UnknownInput
                && !x.TdiControlInput).ToList();
            List<ISvfData> listFilteredForInputs = svfDataResult.Where(
                x => x.TestType != BoundaryScanTestType.Neighbouring
                && x.TestType != BoundaryScanTestType.Undefined
                && x.TestType != BoundaryScanTestType.UnknownControl
                && x.TdiControlInput).ToList();
            List<ISvfData> listToConsiderForInputs = new List<ISvfData>();

            // get all svfs of all inputs to consider for later usage
            foreach (var svf in listFilteredForInputs)
            {
                if (svf.TestType == BoundaryScanTestType.DirectGnd && !svf.ExpectedValue ||
                    svf.TestType == BoundaryScanTestType.DirectPower && svf.ExpectedValue ||
                    svf.TestType == BoundaryScanTestType.Pulldown && !svf.ExpectedValue ||
                    svf.TestType == BoundaryScanTestType.Pullup && svf.ExpectedValue ||
                    svf.TestType == BoundaryScanTestType.UnknownInput && svf.SafeValuesAreMatchingExpectedValue())
                {
                    listToConsiderForInputs.Add(svf);
                }
            }

            return GetNewNeighbours(
                listFilteredForOutputs,
                pinPackage,
                bsdlOutput,
                listToConsiderForInputs);
        }

        /// <summary>
        /// get new neighbours for a category
        /// </summary>
        /// <param name="listFilteredForOutputs">all output svfs</param>
        /// <param name="pinPackage">the bsdl pin package</param>
        /// <param name="bsdlOutput">the bsdl output</param>
        /// <param name="listToConsiderForInputs">all input svfs</param>
        protected virtual List<ISvfData> GetNewNeighbours(
            List<ISvfData> listFilteredForOutputs,
            IBSDLPackage pinPackage,
            IBSDLOutput bsdlOutput,
            List<ISvfData> listToConsiderForInputs)
        {
            // determine all svfs to be really considered for creation
            List<ISvfData> listToConsiderForCreation = listFilteredForOutputs
                .Where(svf => svf.TestType == BasedTestType).ToList();
            List<ISvfData> newData = new List<ISvfData>();

            // create for all determined svfs a cloned one and add to list
            foreach (var svf in listToConsiderForCreation)
            {
                ISvfData newResult = GetAllNeighbours(svf, pinPackage, bsdlOutput, listToConsiderForInputs);
                if (newResult != null)
                {
                    newData.Add(newResult);
                }
            }

            // add check patch content
            return newData;
        }

        /// <summary>
        /// get all svf of pins with neighbours
        /// </summary>
        /// <param name="svfData">the specific svf to be considered for neighbour detection</param>
        /// <param name="bsdlPackage">the bsdl package</param>
        /// <param name="bsdlOutput">the bsdl output</param>
        /// <param name="svfDataInputs">all svf inputs</param>
        /// <returns></returns>
        protected virtual ISvfData GetAllNeighbours(
            ISvfData svfData, IBSDLPackage bsdlPackage, IBSDLOutput bsdlOutput, List<ISvfData> svfDataInputs)
        {
            ISvfData result = null;

            // find the pin to be considered of the svf file
            IPin pin = bsdlPackage.Pins.FirstOrDefault(x => x.Name.Equals(svfData.PinPort));
            if (pin != null)
            {
                // find all neighbours of a pin
                List<string> neighboursPorts = pin.Neighbors.Select(x => x.Name).ToList();
                if (neighboursPorts.Count > 0)
                {
                    List<ISvfData> svfNeighbours = new List<ISvfData>();
                    foreach (var port in neighboursPorts)
                    {
                        if (bsdlOutput.PortMap.ContainsKey(port))
                        {
                            foreach (var pinFound in bsdlOutput.PortMap[port])
                            {
                                foreach (var svf in svfDataInputs)
                                {
                                    if (svf.PinName.Equals(pinFound))
                                    {
                                        svfNeighbours.Add(svf);
                                    }
                                }
                            }
                        }
                    }

                    if (svfNeighbours.Count > 0)
                    {
                        // clone the svf file
                        result = (ISvfData)svfData.Clone();

                        // set the based test type to the used value
                        result.BasedTestTypeForCloning = result.TestType;

                        // change the general test type
                        result.TestType = BoundaryScanTestType.Neighbouring;

                        // set the neighbour type
                        result.MultiPinType = MultiPinType.Neighbours;

                        result.SvfWriter = this;

                        // remove the already used pin from neighbours list
                        svfNeighbours.RemoveAll(x => x.PinName.Equals(result.PinName));
                        result.Neighbours.AddRange(svfNeighbours);
                        ApplyCombinedStrings(result);
                    }
                }
            }

            return result;
        }
    }
}
