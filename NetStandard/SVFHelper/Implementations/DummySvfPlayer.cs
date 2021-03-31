using SVFHelper.Interfaces;
using System;

namespace SVFHelper.Implementations
{
    public class DummySvfPlayer : ISvfPlayer
    {
        private const string DefaultVector = "0001FFFFF3FFFFFFFDBFFE79E4F27FFFFF7FFFFFE9249FF8B6DB6FB6DB2DB6DBEDB64924924930D2492492492DB6DBEDB6CB6DB6FB6DFFFFFFFFFFFFFFFFFFFFBFFFFFFFFFFFFFFFFC00000FFFFFFFFFFFFFFFFFFFFFFFFFFFDFFFFFDBEFFFFFFFFFFFFFFFFFFFFFFFFFFFFF7FFFFF6DBE400007FFFFFFFFFFF7FF7FFFFFFFFFFFFFFFE0000BF6";
        private static readonly byte[] currentVector = GetCurrentVector();

        public byte[] GetDefaultVector(uint scanChainLength, uint instructionRegisterLength, string pgmIp, uint pgmPort, uint supplyVoltageMv, uint ioVoltageMv)
        {
            return currentVector;
        }

        private static byte[] GetCurrentVector()
        {
            byte[] currentVectorNew = SVFHelper.StringToByteArray(DefaultVector);
            Array.Reverse(currentVectorNew, 0, currentVectorNew.Length);
            return currentVectorNew;
        }

        public bool PlaySvfFile(uint scanChainLength, uint instructionRegisterLength, string svfFilePath, string logFilePath, string pgmIp, uint pgmPort, uint supplyVoltageMv, uint ioVoltageMv)
        {
            return true;
        }
    }
}
