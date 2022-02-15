using ProMik.SmartIct.Interfaces.Helper;
using ProMik.Svf.Contracts;
using System;

namespace ProMik.SmartIct.Svf.SvfInterfaces.Interfaces
{
    public abstract class ISvfWriter
    {
        public abstract BoundaryScanTestType BasedTestType { get; }

        public abstract BoundaryScanTestTypeInternal BoundaryScanTestTypeInternal { get; }

        public abstract MultiPinType MultiPinType { get; }

        public abstract string DestinationPath { get; }

        public abstract string Description { get; }

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
    }
}
