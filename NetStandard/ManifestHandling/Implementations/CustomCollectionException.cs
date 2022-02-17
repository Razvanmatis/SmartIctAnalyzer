using System;

namespace ProMik.SmartIct.Services.ManifestHandler.Implementations
{
    public class CustomCollectionException : Exception
    {
        public CustomCollectionException()
        {
        }

        public CustomCollectionException(string message)
            : base(message)
        {
        }

        public CustomCollectionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
