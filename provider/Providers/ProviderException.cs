using System.Runtime.Serialization;

namespace Datask.Providers
{
    [Serializable]
    public sealed class ProviderException : Exception
    {
        public ProviderException()
        {
        }

        public ProviderException(string message)
            : base(message)
        {
        }

        public ProviderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        private ProviderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
