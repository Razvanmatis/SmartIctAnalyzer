using Interfaces.Gui;
using ProMik.BSDL.Interfaces.Entities;
using SVFHelper.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SVFHelper.Implementations
{
    public class SVFHelper : ISVFHelper
    {
        private const string Frequency = "10E6";
        private readonly ILogger logger;

        public SVFHelper(ILogger logger)
        {
            this.logger = logger;
        }

        public static byte[] StringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", string.Empty);
        }

        public ISvfData GetSvfContent(string basePath, string jtagName, string pinLabel, ProMik.BSDL.Interfaces.Entities.IBSDLOutput bsdlOutput, bool requestValue, bool expectedValue, byte[] defaultVector)
        {
            if (defaultVector == null || defaultVector.Length == 0)
            {
                logger.LogMessage("Please define first the default vector of BSDL devide!", LogCategory.ERROR);
                return new SvfData(jtagName, pinLabel, basePath, string.Empty, string.Empty, string.Empty, string.Empty, requestValue, expectedValue, string.Empty);
            }

            string prefix = GetPrefix(bsdlOutput);
            try
            {
                List<string> mappedPins = GetMappedPins(new List<string>() { pinLabel }, bsdlOutput.PinMap);
                if (mappedPins.Count == 0)
                {
                    return new SvfData(jtagName, pinLabel, basePath, string.Empty, string.Empty, prefix.ToString(), string.Empty, requestValue, expectedValue, string.Empty);
                }
                int position = 0;
                string requestString = GenerateNewVector(mappedPins[0], requestValue, bsdlOutput.BoundaryCells, ref position, defaultVector);
                string checkString = GenerateNewVector(mappedPins[0], expectedValue, bsdlOutput.BoundaryCells, ref position, defaultVector);
                if (string.IsNullOrEmpty(requestString) || string.IsNullOrEmpty(checkString))
                {
                    return new SvfData(jtagName, pinLabel, basePath, string.Empty, string.Empty, prefix.ToString(), string.Empty, requestValue, expectedValue, string.Empty);
                }
                string suffix = GetSuffix(bsdlOutput);
                string mask = GetMask(position, defaultVector);
                return new SvfData(jtagName, pinLabel, basePath, requestString, checkString, prefix, suffix, requestValue, expectedValue, mask);
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return new SvfData(jtagName, pinLabel, basePath, string.Empty, string.Empty, string.Empty, string.Empty, requestValue, expectedValue, string.Empty);
            }
        }

        public ISvfData GetSvfContent(string basePath, string jtagName, List<string> pinLabels, ProMik.BSDL.Interfaces.Entities.IBSDLOutput bsdlOutput, bool requestValue, bool expectedValue, byte[] defaultVector, List<string> notFoundPins = null, List<string> foundPins = null)
        {
            if (defaultVector == null || defaultVector.Length == 0)
            {
                logger.LogMessage("Please define first the default vector of BSDL devide!", LogCategory.ERROR);
                return new SvfData(jtagName, pinLabels.Count.ToString(), basePath, string.Empty, string.Empty, string.Empty, string.Empty, requestValue, expectedValue, string.Empty);
            }

            string prefix = GetPrefix(bsdlOutput);
            try
            {
                List<string> mappedPins = GetMappedPins(pinLabels, bsdlOutput.PinMap);
                if (mappedPins.Count == 0)
                {
                    return new SvfData(jtagName, pinLabels.Count.ToString(), basePath, string.Empty, string.Empty, prefix, string.Empty, requestValue, expectedValue, string.Empty);
                }
                List<int> positions = new List<int>();
                string request = GenerateNewVector(mappedPins, requestValue, bsdlOutput.BoundaryCells, defaultVector, positions);
                string check = GenerateNewVector(mappedPins, expectedValue, bsdlOutput.BoundaryCells, defaultVector);
                if (string.IsNullOrEmpty(request) || string.IsNullOrEmpty(check))
                {
                    return new SvfData(jtagName, pinLabels.Count.ToString(), basePath, string.Empty, string.Empty, prefix, string.Empty, requestValue, expectedValue, string.Empty);
                }
                string suffix = GetSuffix(bsdlOutput);
                string mask = GetMask(0, defaultVector, positions);
                return new SvfData(jtagName, pinLabels.Count.ToString(), basePath, request, check, prefix, suffix, requestValue, expectedValue, mask);
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return new SvfData(jtagName, pinLabels.Count.ToString(), basePath, string.Empty, string.Empty, string.Empty, string.Empty, requestValue, expectedValue, string.Empty);
            }
        }

        private static string GetSuffix(IBSDLOutput bsdlOutput)
        {
            return "SDR " + bsdlOutput.BoundaryCells.Count() + " TDI (";
        }

        private static string GetPrefix(ProMik.BSDL.Interfaces.Entities.IBSDLOutput bsdlOutput)
        {
            StringBuilder result = new StringBuilder();

            // Get the instruction length from bsdl output InstructionLength
            var instructionLength = bsdlOutput.InstructionLength;

            // Take the frequency and write it to the svf vector as the first thing e.g. "FREQUENCY 10E6 HZ;" for 10 MHz and please consider the exponential representation
            result.Append("FREQUENCY ").Append(Frequency).AppendLine(" HZ;");

            // Write "STATE IDLE;"
            result.AppendLine("STATE IDLE;");

            // Write "ENDIR IDLE;"
            result.AppendLine("ENDIR IDLE;");

            // "ENDDR IDLE;"
            result.AppendLine("ENDDR IDLE;");

            var opcode = bsdlOutput.InstructionOpcodes["EXTEST"];

            if (string.IsNullOrEmpty(opcode))
            {
                opcode = bsdlOutput.InstructionOpcodes["PRELOAD"];
            }

            var opcodeInt = Convert.ToInt32(opcode, 2);
            opcode = opcodeInt.ToString("X2");

            result.Append("SIR ").Append(instructionLength).Append(" TDI ").Append('(').Append(opcode).AppendLine(");");
            return result.ToString();
        }

        private static List<string> GetMappedPins(List<string> pinLabels, IDictionary<string, string> pinMap)
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

        private string GetMask(int position, byte[] defaultVector, List<int> positions = null)
        {
            if (defaultVector == null || defaultVector.Length == 0)
            {
                logger.LogMessage("Please define first the default vector of BSDL devide!", LogCategory.ERROR);
                return string.Empty;
            }

            string mask = string.Empty;
            for (int idx = 0; idx < defaultVector.Length * 2; idx++)
            {
                mask += "0";
            }

            byte[] data = StringToByteArray(mask);
            var vectorArray = new BitArray(data);
            if (positions == null)
            {
                vectorArray.Set(position, true);
            }
            else
            {
                foreach (var pos in positions)
                {
                    vectorArray.Set(pos, true);
                }
            }

            byte[] resultBytes = new byte[((vectorArray.Length - 1) / 8) + 1];
            vectorArray.CopyTo(resultBytes, 0);

            Array.Reverse(resultBytes, 0, resultBytes.Length);
            return ByteArrayToString(resultBytes);
        }

        /// <summary>
        /// GenerateNewVector
        /// </summary>
        /// <param name="pinLabel"></param>
        /// <param name="value"></param>
        /// <param name="boundaryCells"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Ignore.</exception>
        private string GenerateNewVector(string pinLabel, bool value, Dictionary<int, IBoundaryCell> boundaryCells, ref int position, byte[] defaultVector)
        {
            var index = 0;
            foreach (var cell in boundaryCells)
            {
                if (cell.Value.Port.Equals(pinLabel))
                {
                    index = cell.Key;
                    position = index;
                    break;
                }
            }

            if (index == 0)
            {
                return string.Empty;
            }

            var vectorArray = new BitArray(defaultVector);
            try
            {
                vectorArray.Set(index, value);
            }
            catch (ArgumentOutOfRangeException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
            }

            byte[] resultBytes = new byte[((vectorArray.Length - 1) / 8) + 1];
            vectorArray.CopyTo(resultBytes, 0);

           // Array.Copy(resultBytes, currentVector, resultBytes.Length); // CLARIFY IF NEEDED AT ALL!!!
            Array.Reverse(resultBytes, 0, resultBytes.Length);
            return ByteArrayToString(resultBytes);
        }

        /// <summary>
        /// GenerateNewVector
        /// </summary>
        /// <param name="pinLabels"></param>
        /// <param name="value"></param>
        /// <param name="boundaryCells"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Ignore.</exception>
        private string GenerateNewVector(List<string> pinLabels, bool value, Dictionary<int, IBoundaryCell> boundaryCells, byte[] defaultVector, List<int> positions = null)
        {
            var vectorArray = new BitArray(defaultVector);
            var index = 0;
            foreach (var pin in pinLabels)
            {
                foreach (var cell in boundaryCells)
                {
                    if (cell.Value.Port.Equals(pin))
                    {
                        index = cell.Key;
                        try
                        {
                            vectorArray.Set(index, value);
                            positions?.Add(index);
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            logger.LogMessage(e.Message, LogCategory.ERROR);
                        }

                        break;
                    }
                }
            }

            if (index == 0)
            {
                return string.Empty;
            }

            byte[] resultBytes = new byte[((vectorArray.Length - 1) / 8) + 1];
            vectorArray.CopyTo(resultBytes, 0);

          //  Array.Copy(resultBytes, currentVector, resultBytes.Length);
            Array.Reverse(resultBytes, 0, resultBytes.Length);
            return ByteArrayToString(resultBytes);
        }
    } 
}