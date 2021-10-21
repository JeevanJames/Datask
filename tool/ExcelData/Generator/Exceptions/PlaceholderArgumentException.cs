using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Datask.Tool.ExcelData.Generator.Exceptions
{
    [Serializable]
    public sealed class PlaceholderArgumentException : ArgumentException
    {
        public PlaceholderArgumentException()
        {
        }

        public PlaceholderArgumentException([Localizable(false)] string message)
            : base(message)
        {
        }

        public PlaceholderArgumentException([Localizable(false)] string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PlaceholderArgumentException([Localizable(false)] string message, string paramName)
            : base(message, paramName)
        {
        }

        public PlaceholderArgumentException([Localizable(false)] string message, string paramName,
            Exception innerException)
            : base(message, paramName, innerException)
        {
        }

        private PlaceholderArgumentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
