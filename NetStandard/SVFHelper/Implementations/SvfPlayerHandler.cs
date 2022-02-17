using Newtonsoft.Json;
using ProMik.Core.Interfaces.Bsdl;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Svf.SvfFileCreation.Helper;
using ProMik.SmartIct.Svf.SvfFileCreation.Interfaces;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations
{
    public class SvfPlayerHandler : ISvfPlayerHandler
    {
        private const int CounterMax = 10;
        private const int CounterGood = 3;
        private readonly ILogger logger;
        private readonly IDialogSelector dialogSelector;

        public SvfPlayerHandler(ILogger logger, IDialogSelector dialogSelector)
        {
            this.logger = logger;
            this.dialogSelector = dialogSelector;
        }

        public byte[] GetDefaultVector(
            uint scanChainLength,
            uint instructionRegisterLength,
            string pgmIp,
            uint pgmPort,
            uint supplyVoltageMv,
            uint ioVoltageMv,
            out uint idCodeReadOut,
            uint idCodeRegister,
            uint frequency,
            uint cableCompensation,
            uint idMask,
            int channel,
            int slot,
            uint idCodeInstr,
            uint preloadInstr)
        {
            // scanChainLength = 1077;
            // instructionRegisterLength = 6;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            byte[] emptyVector = new byte[0];
            byte[] realData = emptyVector;
            idCodeReadOut = 0;
            try
            {
                IntPtr target = IntPtr.Zero;
                if (RunInitSequence(pgmIp, pgmPort, supplyVoltageMv, ioVoltageMv, out target, channel, slot) != ResultEnum.OK)
                {
                    Thread.Sleep(200);
                    if (RunInitSequence(pgmIp, pgmPort, supplyVoltageMv, ioVoltageMv, out target, channel, slot) != ResultEnum.OK)
                    {
                        return emptyVector;
                    }
                }

                string errorMessage = string.Empty;
                byte[] data = new byte[0];
                byte[] backData = new byte[0];
                uint code = 0;
                int counter = 0;
                int counterGood = 0;
                while (counter++ < CounterMax)
                {
                    var result = SvfPlayer.InitializeSvfPlayer(
                    target,
                    instructionRegisterLength,
                    scanChainLength,
                    ref backData,
                    ref code,
                    idCodeRegister,
                    frequency,
                    cableCompensation,
                    idMask,
                    idCodeInstr,
                    preloadInstr);
                    if (result != ResultEnum.OK)
                    {
                        errorMessage = SvfPlayer.ParseResult((int)result);
                        logger.LogMessage(
                            "Error at InitializeSvfPlayer: " + errorMessage + " with id code: " + code
                            + " Using empty default vector...", LogCategory.ERROR);
                        realData = emptyVector;
                        break;
                    }
                    else if (data.Length == 0 || !StructuralComparisons.StructuralEqualityComparer.Equals(backData, data))
                    {
                        data = backData;
                        counterGood = 0;
                        Thread.Sleep(100);
                    }
                    else if (++counterGood >= CounterGood)
                    {
                        realData = data;
                        break;
                    }
                }

                if (counter >= CounterMax)
                {
                    realData = emptyVector;
                    logger.LogMessage("Error at reading out default vector! Not able to read out " + CounterGood
                        + " times the same one!", LogCategory.ERROR);
                }

                idCodeReadOut = code;
                EndSvfSequence(target);
            }
            catch (SvfException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
            }

            watch.Stop();
            logger.LogMessage("SVF getDefaultVector time: "
                + watch.ElapsedMilliseconds + " ms: 0x" + BitConverter.ToString(realData), LogCategory.INFO);
            return realData;
        }

        /// <summary>
        /// EndSvfSequence
        /// </summary>
        /// <param name="target"></param>
        /// <exception cref="SVFHelper.SvfException">Ignore.</exception>
        /// <exception cref="SvfException">Ignore.</exception>
        private void EndSvfSequence(IntPtr target)
        {
            try
            {
                string errorMessage = string.Empty;
                var result = SvfPlayer.SetVsupSwitch(target, false, 100);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SvfPlayer.ParseResult((int)result);
                    logger.LogMessage("Error at SetVsupSwitch: " + errorMessage, LogCategory.WARNING);
                }

                result = SvfPlayer.SetVmodSwitch(target, false);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SvfPlayer.ParseResult((int)result);
                    logger.LogMessage("Error at SetVmodSwitch: " + errorMessage, LogCategory.WARNING);
                }

                result = SvfPlayer.SetProgSwitches(target, 0);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SvfPlayer.ParseResult((int)result);
                    logger.LogMessage("Error at SetProgSwitches: " + errorMessage, LogCategory.WARNING);
                }
            }
            catch (Exception e)
            {
                throw new SvfException(e.Message);
            }
        }

        /// <summary>
        /// RunInitSequence
        /// </summary>
        /// <param name="pgmIp"></param>
        /// <param name="pgmPort"></param>
        /// <param name="supplyVoltageMv"></param>
        /// <param name="ioVoltageMv"></param>
        /// <returns></returns>
        /// <exception cref="SVFHelper.SvfException">Ignore.</exception>
        /// <exception cref="SvfException">Ignore.</exception>
        private ResultEnum RunInitSequence(
            string pgmIp, uint pgmPort, uint supplyVoltageMv, uint ioVoltageMv, out IntPtr target, int channel, int slot)
        {
            try
            {
                Ping p = new Ping();
                PingReply reply = p.Send(pgmIp);
                int counter = 0;
                while (reply.Status != IPStatus.Success)
                {
                    if (dialogSelector != null && dialogSelector.OpenMessageBox(
                        "Please make sure to connect a programmer to your computer!",
                        PlainDialogMode.OK_CANCEL) == PlainDialogResponse.ABORT)
                    {
                        logger.LogMessage("Playing of SVF file aborted by user", LogCategory.WARNING);
                        target = IntPtr.Zero;
                        return ResultEnum.NETWORK_ERROR;
                    }
                    else
                    {
                        logger.LogMessage("Please make sure to connect a programmer to your computer!", LogCategory.ERROR);
                        if (++counter > 10)
                        {
                            target = IntPtr.Zero;
                            return ResultEnum.NETWORK_ERROR;
                        }
                    }

                    reply = p.Send(pgmIp);
                    Thread.Sleep(100);
                }

                string errorMessage = string.Empty;
                var progHandle = SvfPlayer.ConnectProgrammer(pgmIp, pgmPort);
                SvfPlayer.SetLoggingFunc(progHandle, (x) => LogResult(x));
                target = SvfPlayer.CreateTarget(progHandle, channel, slot);
                var result = SvfPlayer.InitProgrammer(progHandle, supplyVoltageMv, ioVoltageMv);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SvfPlayer.ParseResult((int)result);
                    logger.LogMessage(
                        "Error at InitProgrammer: " + errorMessage + " Using empty default vector...",
                        LogCategory.ERROR);
                    return result;
                }

                result = SvfPlayer.Start(progHandle, (uint)channel, (uint)slot);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SvfPlayer.ParseResult((int)result);
                    logger.LogMessage("Error at Start: " + errorMessage + " Using empty default vector...", LogCategory.ERROR);
                    return result;
                }

                result = SvfPlayer.SetVsupSwitch(target, true, 0);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SvfPlayer.ParseResult((int)result);
                    logger.LogMessage(
                        "Error at SetVsupSwitch: " + errorMessage + " Using empty default vector...", LogCategory.ERROR);
                    return result;
                }

                result = SvfPlayer.SetVmodSwitch(target, true);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SvfPlayer.ParseResult((int)result);
                    logger.LogMessage(
                        "Error at SetVmodSwitch: " + errorMessage + " Using empty default vector...", LogCategory.ERROR);
                    return result;
                }

                result = SvfPlayer.SetProgSwitches(target, 0x3ff);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SvfPlayer.ParseResult((int)result);
                    logger.LogMessage(
                        "Error at SetProgSwitches: " + errorMessage + " Using empty default vector...", LogCategory.ERROR);
                    return result;
                }

                uint voltage = 0;
                result = SvfPlayer.ReadVoltage(target, 0, ref voltage);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SvfPlayer.ParseResult((int)result);
                    logger.LogMessage(
                        "Error at ReadVoltage: " + errorMessage + " Using empty default vector...", LogCategory.ERROR);
                    return result;
                }

                result = SvfPlayer.ReadVoltage(target, 1, ref voltage);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SvfPlayer.ParseResult((int)result);
                    logger.LogMessage(
                        "Error at ReadVoltage: " + errorMessage + " Using empty default vector...", LogCategory.ERROR);
                    return result;
                }

                return ResultEnum.OK;
            }
            catch (Exception e)
            {
                throw new SvfException(e.Message);
            }
        }

        public bool PlaySvfFile(
            IBSDLOutput package,
            uint scanChainLength,
            uint instructionRegisterLength,
            string[] svfFilePath,
            string logFilePath,
            string pgmIp,
            uint pgmPort,
            uint supplyVoltageMv,
            uint ioVoltageMv,
            uint idCodeRegister,
            uint frequency,
            uint cableCompensation,
            uint idMask,
            int channel,
            int slot,
            uint idCodeInstr,
            uint preloadInstr)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            bool state;
            try
            {
                string errorMessage = string.Empty;
                IntPtr target = IntPtr.Zero;
                if (RunInitSequence(pgmIp, pgmPort, supplyVoltageMv, ioVoltageMv, out target, channel, slot) != ResultEnum.OK)
                {
                    Thread.Sleep(200);
                    if (RunInitSequence(pgmIp, pgmPort, supplyVoltageMv, ioVoltageMv, out target, channel, slot) != ResultEnum.OK)
                    {
                        return false;
                    }
                }

                byte[] data = new byte[0];
                byte[] backData = new byte[0];
                uint idCodeRead = 0;
                int counter = 0;
                int goodCounter = 0;
                while (++counter < CounterMax)
                {
                    var result = SvfPlayer.InitializeSvfPlayer(
                    target,
                    instructionRegisterLength,
                    scanChainLength,
                    ref backData,
                    ref idCodeRead,
                    idCodeRegister,
                    frequency,
                    cableCompensation,
                    idMask,
                    idCodeInstr, preloadInstr);
                    if (result != ResultEnum.OK)
                    {
                        errorMessage = SvfPlayer.ParseResult((int)result);
                        logger.LogMessage("Error at InitializeSvfPlayer: " + errorMessage + " with id code: " + idCodeRead
                            + " Using empty default vector...", LogCategory.ERROR);
                        EndSvfSequence(target);
                        return false;
                    }

                    if (data.Length == 0 || !StructuralComparisons.StructuralEqualityComparer.Equals(backData, data))
                    {
                        data = backData;
                        goodCounter = 0;
                        Thread.Sleep(100);
                    }
                    else if (++goodCounter >= CounterGood)
                    {
                        break;
                    }
                }

                if (counter >= CounterMax)
                {
                    logger.LogMessage("Error at reading out default vector! Not able to read out " + CounterGood
                        + " times the same one!", LogCategory.ERROR);
                    return false;
                }

                byte[] rev = new byte[data.Length];
                Array.Copy(data, rev, data.Length);
                Array.Reverse(rev);
                Debug.WriteLine(BitConverter.ToString(rev).Replace("-", string.Empty));
                uint idCodeCalc = package.GetIdCodeRegister(out int amountToSkip);
                if (!package.CheckIdCodeRegister(idCodeRead, idCodeCalc, amountToSkip))
                {
                    logger.LogMessage("ID code from device: " + idCodeRead + " is not matching that one from BSDL file: "
                        + idCodeCalc, LogCategory.ERROR);
                    EndSvfSequence(target);
                    return false;
                }

                logger.LogMessage("SVF read default vector time: " + watch.ElapsedMilliseconds + " ms", LogCategory.INFO);
                if (!logFilePath.EndsWith("\\"))
                {
                    logFilePath += "\\";
                }

                state = true;
                List<string> errors = new List<string>();
                foreach (var file in svfFilePath)
                {
                    Stopwatch watchPlay = new Stopwatch();
                    watchPlay.Start();
                    string logPath = logFilePath + file[(file.LastIndexOf("\\") + 1)..] + "_log";
                    var result = SvfPlayer.ExecuteSvfSequence(target, file, logPath);
                    if (result != (int)ResultEnum.OK)
                    {
                        errorMessage = SvfPlayer.ParseResult((int)result);
                        logger.LogMessage("Error at ExecuteSvfSequence: " + errorMessage, LogCategory.ERROR);
                        state = false;
                    }
                    else
                    {
                        SaveReversedContentIntoNewLogFile(logPath);
                        string error = CheckTdoMissmatches(file, logPath);
                        if (!string.IsNullOrEmpty(error))
                        {
                            errors.Add(file[(file.LastIndexOf("\\") + 1)..] + ": " + error);
                        }
                    }

                    logger.LogMessage("SVF PlaySvfFile time for file " + file + ":" + watchPlay.ElapsedMilliseconds
                        + " ms", LogCategory.INFO);
                }

                if (errors.Count > 0)
                {
                    WriteGeneralResult(logFilePath, errors);
                }

                EndSvfSequence(target);
                watch.Stop();
                logger.LogMessage("SVF PlaySvfFiles with read defaultVector time: " + watch.ElapsedMilliseconds
                    + " ms", LogCategory.INFO);
                return state;
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return false;
            }
        }

        private void WriteGeneralResult(string logFilePath, List<string> errors)
        {
            try
            {
                if (File.Exists(logFilePath + "\\generalMissmatchesOverview.txt"))
                {
                    File.Delete(logFilePath + "\\generalMissmatchesOverview.txt");
                }

                File.AppendAllLines(logFilePath + "\\generalMissmatchesOverview.txt", errors);
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
            }
        }

        private string CheckTdoMissmatches(string file, string logPath)
        {
            bool failed = false;
            string result = "No missmatch found. Everything OK";
            string[] logLines = File.ReadAllLines(logPath + "_reversed");
            if (logLines.Length >= 4)
            {
                string[] fileLines = File.ReadAllLines(file);
                if (fileLines.Length >= 10)
                {
                    string tdoReadString = logLines[3].Replace("TDO DR SHIFT: ", string.Empty).Replace(");", string.Empty);
                    byte[] tdoRead = ISvfWriter.StringToByteArray(tdoReadString);
                    string tdoExpString = fileLines[9].Replace("TDO (", string.Empty).Replace(")", string.Empty);
                    string maskString = fileLines[10].Replace("MASK (", string.Empty).Replace(");", string.Empty);
                    while (tdoExpString.Length < tdoReadString.Length)
                    {
                        tdoExpString = "0" + tdoExpString;
                        maskString = "0" + maskString;
                    }

                    byte[] tdoExp = ISvfWriter.StringToByteArray(tdoExpString);
                    byte[] mask = ISvfWriter.StringToByteArray(maskString);
                    Array.Reverse(tdoRead);
                    Array.Reverse(tdoExp);
                    Array.Reverse(mask);
                    BitArray arrMask = new BitArray(mask);
                    BitArray tdoExpArray = new BitArray(tdoExp);
                    BitArray tdoReadArray = new BitArray(tdoRead);
                    List<int> missmatches = new List<int>();
                    for (int idx = 0; idx < arrMask.Length; idx++)
                    {
                        if (arrMask.Get(idx) && tdoExpArray.Get(idx) != tdoReadArray.Get(idx))
                        {
                            missmatches.Add(idx);
                        }
                    }

                    if (missmatches.Count > 0)
                    {
                        MetaDataSvfFile resultObject = null;
                        try
                        {
                            resultObject = JsonConvert.DeserializeObject<MetaDataSvfFile>(fileLines[0][2..]);
                        }
                        catch (Exception e)
                        {
                            logger.LogMessage(e.Message, LogCategory.ERROR);
                        }

                        failed = true;
                        if (missmatches.Count == 1)
                        {
                            result = "Missmatch found at position " + missmatches[0] + ": "
                                + GetPinName(resultObject, missmatches[0]) + "!";
                        }
                        else
                        {
                            result = "Missmatches found at positions: ";
                            foreach (var idx in missmatches)
                            {
                                result += idx + ": " + GetPinName(resultObject, idx) + ", ";
                            }

                            result = result[0..^2];
                        }
                    }

                    File.AppendAllText(logPath + "_reversed", "\n" + result);
                }
            }

            if (failed)
            {
                return result;
            }
            else
            {
                return string.Empty;
            }
        }

        private string GetPinName(MetaDataSvfFile resultObject, int position)
        {
            if (resultObject == null)
            {
                return string.Empty;
            }

            return resultObject.PinInformation.FirstOrDefault(pin => pin.MaskPosition == position)?.PinName ?? string.Empty;
        }

        private void SaveReversedContentIntoNewLogFile(string logFilePath)
        {
            string[] lines = File.ReadAllLines(logFilePath);
            if (lines.Length >= 2)
            {
                for (int idx = 1; idx <= lines.Length; idx += 2)
                {
                    string content = lines[idx].Replace("TDO DR SHIFT: ", string.Empty).Replace(";", string.Empty);
                    byte[] bytes = ISvfWriter.StringToByteArray(content);
                    Array.Reverse(bytes, 0, bytes.Length);
                    string swapped = BitConverter.ToString(bytes).Replace("-", string.Empty);
                    lines[idx] = "TDO DR SHIFT: " + swapped;
                }

                File.WriteAllLines(logFilePath + "_reversed", lines);
            }
        }

        private void LogResult(string x)
        {
            if (x.ToLower().Contains("info"))
            {
                logger.LogMessage(x, LogCategory.INFO);
            }
            else
            {
                logger.LogMessage(x, LogCategory.ERROR);
            }
        }
    }
}
