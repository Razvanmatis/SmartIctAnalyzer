using ProMik.SvfPlayer_sharp;
using System.Linq;
using System.Collections.Generic;
using ProMik.Svf.Contracts;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.Svf.SvfFileCreation.Interfaces;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.SinglePinWriter
{
    public class SinglePinPullUpPullDownWriter : SinglePinWriterBase, IMultiSvfFilesHandler
    {
        public SinglePinPullUpPullDownWriter(ILogger logger) : base(logger)
        {
        }

        public override string Description => "PULL UPS and PULL DOWNS";

        public override string DestinationPath => "pullUpsAndDowns";

        public override BoundaryScanTestTypeInternal BoundaryScanTestTypeInternal => BoundaryScanTestTypeInternal.PullupPulldown;

        public override BoundaryScanTestType BasedTestType => BoundaryScanTestType.DirectGnd;

        public override List<(bool isInput, bool expectedValue)> SvfCreationContent =>
            new List<(bool isInput, bool expectedValue)>()
                {
                    (true, true),
                    (false, false),
                    (true, false),
                    (false, true)
                };

        public List<ISvfData> GetNewSvfFilesOfMultiSvfs(List<ISvfData> svfData)
        {
            List<ISvfData> list = svfData.Where(svf =>
                svfData.Exists(a =>
                    a.PinName.Equals(svf.PinName) && a.TestType == BoundaryScanTestType.Pulldown)
                && svfData.Exists(b => b.PinName.Equals(svf.PinName) && b.TestType == BoundaryScanTestType.Pullup))
                .ToList();
            svfData.RemoveAll(svf => list.Contains(svf));
            foreach (var svf in list)
            {
                svf.TestType = BoundaryScanTestType.PullupPulldown;
                svf.SvfWriter = this;
            }

            return list;
        }
    }
}
