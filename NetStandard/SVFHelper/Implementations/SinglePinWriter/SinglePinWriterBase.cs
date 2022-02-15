using ProMik.BSDL.Interfaces.Enums;
using ProMik.Core.Interfaces.Bsdl;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.Svf.SvfFileCreation.Interfaces;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.SinglePinWriter
{
    public abstract class SinglePinWriterBase : ISvfWriter, ISvfCreationContent
    {
        private const string Frequency = "10E6";
        private readonly ILogger logger;

        public SinglePinWriterBase(ILogger logger)
        {
            this.logger = logger;
        }


        public abstract List<(bool isInput, bool expectedValue)> SvfCreationContent { get; }

        public override MultiPinType MultiPinType { get; } = MultiPinType.None;

        protected virtual string GenerateTdiVector(
            bool tdiControlInput,
            Dictionary<int, IBoundaryCell> positionsFound,
            byte[] defaultVector,
            bool expectedValue = false)
        {
            return string.Empty;
        }

        public virtual List<ISvfData> GetSVFFiles(
            string basePath, string jtagName, List<PinTestTypeContainer> pinLabel, IBSDLOutput bsdlOutput, byte[] defaultVector)
        {
            List<ISvfData> data = new List<ISvfData>();
            foreach (var pin in pinLabel)
            {
                foreach (var entry in SvfCreationContent)
                {
                    var result = GetSvfContentOfOnePin(
                        basePath, jtagName, pin, bsdlOutput, entry.isInput, entry.expectedValue, defaultVector);
                    result.TestType = BasedTestType;
                    data.Add(result);
                }
            }

            return data;
        }

        protected virtual string GenerateTdoVector(
            bool expectedValue, Dictionary<int, IBoundaryCell> positions, byte[] defaultVector)
        {
            if (positions.Count == 0)
            {
                return string.Empty;
            }

            BitArray array = new BitArray(defaultVector);
            List<int> positionsToDelete = new List<int>();
            foreach (var entry in positions)
            {
                if (positions.Count >= 3)
                {
                    if (entry.Value.Func == BoundaryFunction.Input || entry.Value.Func == BoundaryFunction.Bidir
                        || entry.Value.Func == BoundaryFunction.Observe_only)
                    {
                        try
                        {
                            array.Set(entry.Key, expectedValue);
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            logger.LogMessage(e.Message, LogCategory.ERROR);
                        }
                    }
                    else
                    {
                        positionsToDelete.Add(entry.Key);
                    }
                }
                else if (positions.Count == 2)
                {
                    if (entry.Value.Func == BoundaryFunction.Output3
                        || entry.Value.Func == BoundaryFunction.Output2
                        || entry.Value.Func == BoundaryFunction.Bidir)
                    {
                        try
                        {
                            array.Set(entry.Key, expectedValue);
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            logger.LogMessage(e.Message, LogCategory.ERROR);
                        }
                    }
                    else
                    {
                        positionsToDelete.Add(entry.Key);
                    }
                }
                else
                {
                    if (entry.Value.Func == BoundaryFunction.Input || entry.Value.Func == BoundaryFunction.Observe_only)
                    {
                        try
                        {
                            array.Set(entry.Key, expectedValue);
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            logger.LogMessage(e.Message, LogCategory.ERROR);
                        }
                    }
                    else
                    {
                        positionsToDelete.Add(entry.Key);
                    }
                }
            }

            positionsToDelete.ForEach(entry => positions.Remove(entry));
            byte[] result = new byte[defaultVector.Length];
            array.CopyTo(result, 0);
            Array.Reverse(result, 0, result.Length);
            return ByteArrayToString(result);
        }

        protected virtual ISvfData GetSvfContentOfOnePin(
            string basePath,
            string jtagName,
            PinTestTypeContainer pinLabel,
            IBSDLOutput bsdlOutput,
            bool tdiContolInput,
            bool expectedValue,
            byte[] defaultVector)
        {
            if (defaultVector == null || defaultVector.Length == 0)
            {
                logger.LogMessage("Please define first the default vector of BSDL devide!", LogCategory.ERROR);
                return new SvfData(jtagName, pinLabel.PinTestObject.PinNumber, basePath, true, this);
            }

            try
            {
                List<string> mappedPins = GetMappedPins(
                    new List<string>() { pinLabel.PinTestObject.PinNumber }, bsdlOutput.PinMap);
                if (mappedPins.Count == 0)
                {
                    return new SvfData(jtagName, pinLabel.PinTestObject.PinNumber, basePath, true, this);
                }

                Dictionary<int, IBoundaryCell> positions = GetAffectedPositions(mappedPins[0], bsdlOutput.BoundaryCells);
                if (positions.Count == 0)
                {
                    return new SvfData(jtagName, pinLabel.PinTestObject.PinNumber, basePath, false, this);
                }

                string tdiString = GenerateTdiVector(tdiContolInput, positions, defaultVector, expectedValue);
                if (string.IsNullOrEmpty(tdiString))
                {
                    return new SvfData(jtagName, pinLabel.PinTestObject.PinNumber, basePath, false, this);
                }

                string tdoString = GenerateTdoVector(expectedValue, positions, defaultVector);
                if (string.IsNullOrEmpty(tdoString) || positions.Count == 0)
                {
                    return new SvfData(jtagName, pinLabel.PinTestObject.PinNumber, basePath, false, this);
                }

                string prefix = GetPrefix(bsdlOutput);
                string sirPrefixPreload = GetSirPrefix(bsdlOutput, true);
                string sirPrefixExTest = GetSirPrefix(bsdlOutput, false);
                string suffix = GetSuffix(bsdlOutput);
                string mask = GetMask(positions.Keys.ToList(), defaultVector);
                if (pinLabel.PinConnectionType.ConnectedComponents == null
                    || pinLabel.PinConnectionType.ConnectedComponents.Count == 0)
                {
                    logger.LogMessage(
                        "No connected components found for PIN: "
                        + pinLabel.PinTestObject.PinNumber
                        + " of JTAG device: " + jtagName,
                        LogCategory.ERROR);
                }

                string contentLine1 = prefix + sirPrefixPreload + suffix + tdiString + ");";
                string contentLine2 = sirPrefixExTest + suffix + tdiString + ")";
                string contentLine3 = "TDO (" + tdoString + ")";
                string contentLine4 = "MASK (" + mask + ");";
                List<string> contentComplete = new List<string>
                {
                    contentLine1,
                    contentLine2,
                    contentLine3,
                    contentLine4
                };
                string port = "No port found!";
                if (bsdlOutput.PinMap.ContainsKey(pinLabel.PinTestObject.PinNumber))
                {
                    port = bsdlOutput.PinMap[pinLabel.PinTestObject.PinNumber];
                }
                return new SvfData(
                    jtagName,
                    pinLabel.PinTestObject.PinNumber,
                    basePath,
                    contentComplete,
                    tdiContolInput,
                    expectedValue,
                    false,
                    positions.Keys.ToList(),
                    pinLabel.PinConnectionType.ConnectedComponents.Select(x => x.FunctionalAttributes.Ref).ToList(),
                    port,
                    BoundaryScanTestType.Undefined,
                    tdiString,
                    tdoString,
                    mask,
                    GetSafeValues(positions),
                    this);
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return new SvfData(jtagName, pinLabel.PinTestObject.PinNumber, basePath, true, this);
            }
        }

        private Dictionary<int, string> GetSafeValues(Dictionary<int, IBoundaryCell> positions)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            foreach (var (pos, cell) in positions)
            {
                dict.Add(pos, cell.Safe.ToString());
            }

            return dict;
        }

        protected virtual Dictionary<int, IBoundaryCell> GetAffectedPositions(
            string pinLabel, Dictionary<int, IBoundaryCell> boundaryCells)
        {
            Dictionary<int, IBoundaryCell> positionsFound = new Dictionary<int, IBoundaryCell>();
            foreach (var cell in boundaryCells)
            {
                if (cell.Value.Port.Equals(pinLabel))
                {
                    positionsFound.Add(cell.Key, cell.Value);
                    if (cell.Value.Ccell > -1 && boundaryCells.ContainsKey(cell.Value.Ccell)
                        && (boundaryCells[cell.Value.Ccell].Func == BoundaryFunction.Controlr
                        || boundaryCells[cell.Value.Ccell].Func == BoundaryFunction.Control))
                    {
                        positionsFound.Add(cell.Value.Ccell, boundaryCells[cell.Value.Ccell]);
                    }
                }
                else if (positionsFound.Count >= 3)
                {
                    break;
                }
            }

            return positionsFound;
        }

        protected virtual string GetSuffix(IBSDLOutput bsdlOutput)
        {
            return "SDR " + bsdlOutput.BoundaryCells.Count() + " TDI (";
        }

        protected virtual string GetPrefix(IBSDLOutput bsdlOutput)
        {
            StringBuilder result = new StringBuilder();

            // Take the frequency and write it to the svf vector as the first thing e.g. "FREQUENCY 10E6 HZ;"
            // for 10 MHz and please consider the exponential representation
            result.Append("// FREQUENCY ").Append(Frequency).AppendLine(" HZ;");

            // Write "STATE IDLE;"
            result.AppendLine("STATE IDLE;");

            // Write "ENDIR IDLE;"
            result.AppendLine("ENDIR IDLE;");

            // "ENDDR IDLE;"
            result.AppendLine("ENDDR IDLE;");
            return result.ToString();
        }

        protected virtual string GetSirPrefix(IBSDLOutput bsdlOutput, bool useOnlyPreload)
        {
            StringBuilder result = new StringBuilder();
            // Get the instruction length from bsdl output InstructionLength
            var instructionLength = bsdlOutput.InstructionLength;
            var opcode = bsdlOutput.InstructionOpcodes["PRELOAD"];

            if (string.IsNullOrEmpty(opcode) || !useOnlyPreload)
            {
                opcode = bsdlOutput.InstructionOpcodes["EXTEST"];
            }

            var opcodeInt = Convert.ToInt32(opcode, 2);
            opcode = opcodeInt.ToString("X2");

            result.Append("SIR ").Append(instructionLength).Append(" TDI ").Append('(').Append(opcode).AppendLine(");");
            return result.ToString();
        }

        protected virtual List<string> GetMappedPins(List<string> pinLabels, IDictionary<string, string> pinMap)
        {
            List<string> mappedPins = new List<string>();
            foreach (var pin in pinLabels)
            {
                if (pinMap.ContainsKey(pin))
                {
                    mappedPins.Add(pinMap[pin]);
                }
            }

            return mappedPins;
        }

        protected virtual string GetMask(List<int> positions, byte[] defaultVector)
        {
            if (defaultVector == null || defaultVector.Length == 0)
            {
                logger.LogMessage("Please define first the default vector of BSDL devide!", LogCategory.ERROR);
                return string.Empty;
            }

            string mask = string.Empty.PadRight(defaultVector.Length * 2, '0');
            byte[] data = StringToByteArray(mask);
            var vectorArray = new BitArray(data);
            positions.ForEach(pos => vectorArray.Set(pos, true));
            byte[] resultBytes = new byte[(vectorArray.Length - 1) / 8 + 1];
            vectorArray.CopyTo(resultBytes, 0);
            Array.Reverse(resultBytes, 0, resultBytes.Length);
            return ByteArrayToString(resultBytes);
        }
    }
}
