// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.Serialization;

namespace Datask.Common.Utilities
{
    [Serializable]
    public class DataskException : Exception
    {
        public DataskException()
        {
        }

        public DataskException(string? message)
            : base(message)
        {
        }

        public DataskException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected DataskException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
