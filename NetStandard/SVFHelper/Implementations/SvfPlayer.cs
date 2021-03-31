using Interfaces.Gui;
using ProMik.SfvPlayer_sharp;
using SVFHelper.Helper;
using SVFHelper.Interfaces;
using System;

namespace SVFHelper.Implementations
{
    public class SvfPlayer : ISvfPlayer
    {
        private readonly ILogger logger;

        public SvfPlayer(ILogger logger)
        {
            this.logger = logger;
        }

        public byte[] GetDefaultVector(uint scanChainLength, uint instructionRegisterLength, string pgmIp, uint pgmPort, uint supplyVoltageMv, uint ioVoltageMv)
        {
            byte[] defEmptyVector = GetDefEmptyVector(scanChainLength);
            byte[] realData = defEmptyVector;
            try
            {
                IntPtr target = IntPtr.Zero;
                if (RunInitSequence(pgmIp, pgmPort, supplyVoltageMv, ioVoltageMv, out target) != ResultEnum.OK) {
                    return defEmptyVector;
                }

                string errorMessage = string.Empty;
                byte[] data = new byte[0];
                uint idCode = 0;
                var result = SfvPlayer.InitializeSvfPlayer(target, instructionRegisterLength, scanChainLength, ref data, ref idCode);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SfvPlayer.SvfPlayer_ParseResult((int)result);
                    logger.LogMessage("Error at InitializeSvfPlayer: " + errorMessage + " with id code: " + idCode +" Using empty default vector...", LogCategory.ERROR);
                    return defEmptyVector;
                }

                realData = data;
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
                var result = SfvPlayer.SetVsupSwitch(target, false, 100);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SfvPlayer.SvfPlayer_ParseResult((int)result);
                    logger.LogMessage("Error at SetVsupSwitch: " + errorMessage, LogCategory.WARNING);
                }

                result = SfvPlayer.SetVmodSwitch(target, false);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SfvPlayer.SvfPlayer_ParseResult((int)result);
                    logger.LogMessage("Error at SetVmodSwitch: " + errorMessage, LogCategory.WARNING);
                }

                result = SfvPlayer.SetProgSwitches(target, 0);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SfvPlayer.SvfPlayer_ParseResult((int)result);
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
        private ResultEnum RunInitSequence(string pgmIp, uint pgmPort, uint supplyVoltageMv, uint ioVoltageMv, out IntPtr target)
        {
            try
            {
                string errorMessage = string.Empty;
                var progHandle = SfvPlayer.ConnectProgrammer(pgmIp, pgmPort);
                SfvPlayer.SetLoggingFunc(progHandle, (x) => LogResult(x));
                target = SfvPlayer.CreateTarget(progHandle, 0);
                var result = SfvPlayer.InitProgrammer(progHandle, supplyVoltageMv, ioVoltageMv);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SfvPlayer.SvfPlayer_ParseResult((int)result);
                    logger.LogMessage("Error at InitProgrammer: " + errorMessage + " Using empty default vector...", LogCategory.ERROR);
                    return result;
                }

                result = SfvPlayer.Start(progHandle);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SfvPlayer.SvfPlayer_ParseResult((int)result);
                    logger.LogMessage("Error at Start: " + errorMessage + " Using empty default vector...", LogCategory.ERROR);
                    return result;
                }

                result = SfvPlayer.SetVsupSwitch(target, true, 0);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SfvPlayer.SvfPlayer_ParseResult((int)result);
                    logger.LogMessage("Error at SetVsupSwitch: " + errorMessage + " Using empty default vector...", LogCategory.ERROR);
                    return result;
                }

                result = SfvPlayer.SetVmodSwitch(target, true);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SfvPlayer.SvfPlayer_ParseResult((int)result);
                    logger.LogMessage("Error at SetVmodSwitch: " + errorMessage + " Using empty default vector...", LogCategory.ERROR);
                    return result;
                }

                result = SfvPlayer.SetProgSwitches(target, 0x3ff);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SfvPlayer.SvfPlayer_ParseResult((int)result);
                    logger.LogMessage("Error at SetProgSwitches: " + errorMessage + " Using empty default vector...", LogCategory.ERROR);
                    return result;
                }

                uint voltage = 0;
                result = SfvPlayer.ReadVoltage(target, 0, ref voltage);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SfvPlayer.SvfPlayer_ParseResult((int)result);
                    logger.LogMessage("Error at ReadVoltage: " + errorMessage + " Using empty default vector...", LogCategory.ERROR);
                    return result;
                }

                result = SfvPlayer.ReadVoltage(target, 1, ref voltage);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SfvPlayer.SvfPlayer_ParseResult((int)result);
                    logger.LogMessage("Error at ReadVoltage: " + errorMessage + " Using empty default vector...", LogCategory.ERROR);
                    return result;
                }

                return ResultEnum.OK;
            }
            catch (Exception e)
            {
                throw new SvfException(e.Message);
            }
        }

        public bool PlaySvfFile(uint scanChainLength, uint instructionRegisterLength, string svfFilePath, string logFilePath, string pgmIp, uint pgmPort, uint supplyVoltageMv, uint ioVoltageMv)
        {
            bool state = false;
            try
            {
                string errorMessage = string.Empty;
                IntPtr target = IntPtr.Zero;
                if (RunInitSequence(pgmIp, pgmPort, supplyVoltageMv, ioVoltageMv, out target) != ResultEnum.OK)
                {
                    return false;
                }

                byte[] data = new byte[0];
                uint idCode = 0;
                var result = SfvPlayer.InitializeSvfPlayer(target, instructionRegisterLength, scanChainLength, ref data, ref idCode);
                if (result != ResultEnum.OK)
                {
                    errorMessage = SfvPlayer.SvfPlayer_ParseResult((int)result);
                    logger.LogMessage("Error at InitializeSvfPlayer: " + errorMessage + " with id code: " + idCode + " Using empty default vector...", LogCategory.ERROR);
                    return false;
                }

                result = SfvPlayer.SvfPlayer_ExecuteSvfSequence(target, svfFilePath, logFilePath);
                if (result != (int)ResultEnum.OK)
                {
                    errorMessage = SfvPlayer.SvfPlayer_ParseResult((int)result);
                    logger.LogMessage("Error at ExecuteSvfSequence: " + errorMessage, LogCategory.ERROR);
                }
                else
                {
                    state = true;
                }

                EndSvfSequence(target);
                return state;
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return false;
            }
        }

        private static byte[] GetDefEmptyVector(uint scanChainLength)
        {
            uint realLength = scanChainLength / 8;
            if (scanChainLength % 8 > 0)
            {
                realLength++;
            }

            return new byte[realLength];
        }

        private void LogResult(string x)
        {
            logger.LogMessage(x, LogCategory.ERROR);
        }
    }
}
