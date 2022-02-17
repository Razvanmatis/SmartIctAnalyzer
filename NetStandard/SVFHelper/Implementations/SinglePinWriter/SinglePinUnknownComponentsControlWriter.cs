using ProMik.Core.Interfaces.Bsdl;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.Helper;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.SinglePinWriter
{
    public class SinglePinUnknownComponentsControlWriter : SinglePinWriterBase
    {
        protected readonly ILogger logger;

        public SinglePinUnknownComponentsControlWriter(ILogger logger) : base(logger)
        {
            this.logger = logger;
        }

        public override string Description => "UNKNOWN CONTROLS";

        public override BoundaryScanTestTypeInternal BoundaryScanTestTypeInternal => BoundaryScanTestTypeInternal.UnknownControl;

        public override string DestinationPath => "unknownsControl";

        public override BoundaryScanTestType BasedTestType => BoundaryScanTestType.UnknownControl;

        public override List<(bool isInput, bool expectedValue)> SvfCreationContent =>
            new List<(bool isInput, bool expectedValue)>()
                {
                    (false, true),
                    (false, false)
                };

        protected override string GenerateTdiVector(
            bool tdiControlInput,
            Dictionary<int, IBoundaryCell> positionsFound,
            byte[] defaultVector,
            bool expectedValue = false)
        {
            var vectorArray = new BitArray(defaultVector);
            try
            {
                foreach (var index in positionsFound)
                {
                    if (positionsFound.Count == 1 && !tdiControlInput)
                    {
                        return string.Empty;
                    }

                    if (index.Value.Func == BoundaryFunction.Controlr || index.Value.Func == BoundaryFunction.Control)
                    {
                        bool contolInput = !(index.Value.Safe == 0);
                        if (!tdiControlInput)
                        {
                            contolInput = index.Value.Safe == 0;
                        }

                        vectorArray.Set(index.Key, contolInput);
                    }
                    else if (!tdiControlInput && index.Value.Func == BoundaryFunction.Output2
                        || index.Value.Func == BoundaryFunction.Output3
                        || index.Value.Func == BoundaryFunction.Bidir)
                    {
                        vectorArray.Set(index.Key, expectedValue);
                    }
                    else
                    {
                        vectorArray.Set(index.Key, false);
                    }
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
            }

            byte[] resultBytes = new byte[defaultVector.Length];
            vectorArray.CopyTo(resultBytes, 0);
            Array.Reverse(resultBytes, 0, resultBytes.Length);
            return ByteArrayToString(resultBytes);
        }
    }
}
