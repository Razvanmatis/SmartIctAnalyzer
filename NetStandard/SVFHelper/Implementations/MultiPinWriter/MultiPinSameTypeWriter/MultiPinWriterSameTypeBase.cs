using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.Svf.SvfFileCreation.Interfaces;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System.Collections.Generic;
using System.Linq;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinSameTypeWriter
{
    public abstract class MultiPinWriterSameTypeBase : MultiPinWriterBase, IMultiSvfFilesHandler, ISvfCreationContent
    {
        public override BoundaryScanTestTypeInternal BoundaryScanTestTypeInternal => BoundaryScanTestTypeInternal.Neighbouring;

        public abstract List<(bool isInput, bool expectedValue)> SvfCreationContent { get; }

        public override MultiPinType MultiPinType { get; } = MultiPinType.SameType;

        public List<ISvfData> GetNewSvfFilesOfMultiSvfs(List<ISvfData> svfData)
        {
            List<ISvfData> results = new List<ISvfData>();
            SvfCreationContent.ForEach(entry => AddNewObjectOfSvfOfSameTypeToList(
                results, BasedTestType, svfData, entry.isInput, entry.expectedValue));
            return results;
        }

        /// <summary>
        /// add the new object of svf of same type of test cases into list
        /// </summary>
        /// <param name="newResults">all new svfs</param>
        /// <param name="typeOfTest">the test type</param>
        /// <param name="svfResults">all sfvs</param>
        /// <param name="tdiInput">should the test be a input?</param>
        /// <param name="expectedValue">the expected value to be checked of svf</param>
        protected virtual void AddNewObjectOfSvfOfSameTypeToList(
            List<ISvfData> newResults,
            BoundaryScanTestType typeOfTest,
            List<ISvfData> svfResults,
            bool tdiInput,
            bool expectedValue)
        {
            var newObject = CreateSvfForSameType(typeOfTest, svfResults, tdiInput, expectedValue);
            if (newObject != null)
            {
                newResults.Add(newObject);
            }
        }

        /// <summary>
        /// create svf file for same type
        /// </summary>
        /// <param name="typeOfTest">test type</param>
        /// <param name="svfResults">all svfs</param>
        /// <param name="tdiInput">should the svf be a input?</param>
        /// <param name="expectedValue">the expected value to be checked</param>
        /// <returns></returns>
        protected virtual ISvfData CreateSvfForSameType(
            BoundaryScanTestType typeOfTest,
            List<ISvfData> svfResults,
            bool tdiInput,
            bool expectedValue)
        {
            // get the list of interest
            List<ISvfData> toConsider = svfResults.Where(svf =>
                svf.TestType == typeOfTest
                && svf.TdiControlInput == tdiInput
                && svf.ExpectedValue == expectedValue).ToList();
            if (toConsider == null || toConsider.Count == 0)
            {
                return null;
            }

            // clonde base object by the first entry
            ISvfData baseObject = (ISvfData)toConsider[0].Clone();

            // set the test type
            baseObject.TestType = BoundaryScanTestType.Neighbouring;

            // set the neighbour type
            baseObject.MultiPinType = MultiPinType.SameType;

            // set the based test type
            baseObject.BasedTestTypeForCloning = typeOfTest;

            baseObject.SvfWriter = this;

            // add all neighbours considering the first element to be skipped as it was used as cloning reference
            baseObject.Neighbours.AddRange(toConsider.Skip(1));
            if (baseObject.Neighbours.Count > 0)
            {
                // apply all OR combined statements to the svf
                ApplyCombinedStrings(baseObject);
                return baseObject;
            }

            return null;
        }
    }
}
